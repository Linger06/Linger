using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Linger.Extensions.Core;
using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Models;
using Linger.Json;
using Microsoft.Extensions.Logging;

#if NETFRAMEWORK
using System.Net.Http;
#endif

namespace Linger.HttpClient.Standard;

/// <summary>
/// 使用 <see cref="System.Net.Http.HttpClient"/> 执行类型化 HTTP API 调用。
/// </summary>
public class StandardHttpClient : IHttpClient, IDisposable
{
    private const int MaxErrorBodyCharacters = 25 * 1024;
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> s_queryProperties = new();
    private static readonly JsonSerializerOptions s_defaultRequestOptions = JsonDefaults.CreateRequestOptions();
    private static readonly JsonSerializerOptions s_defaultResponseOptions = JsonDefaults.CreateResponseOptions();
    private readonly System.Net.Http.HttpClient _httpClient;
    private readonly ILogger<StandardHttpClient> _logger;
    private readonly bool _ownsHttpClient;

    /// <summary>
    /// 使用指定的基础地址创建客户端。
    /// </summary>
    /// <param name="baseUrl">所有相对请求地址使用的绝对基础地址。</param>
    /// <param name="logger">可选日志记录器。</param>
    /// <example>
    /// <code>
    /// using var client = new StandardHttpClient("https://api.example.com/");
    /// </code>
    /// </example>
    /// <remarks>应在应用生命周期内复用此实例，避免为每个请求创建新的连接池。</remarks>
    public StandardHttpClient(
        string baseUrl,
        ILogger<StandardHttpClient>? logger = null)
        : this(CreateHttpClient(baseUrl), logger, ownsHttpClient: true)
    {
    }

    /// <summary>
    /// 使用指定的基础地址和客户端配置回调创建客户端。
    /// </summary>
    /// <param name="baseUrl">所有相对请求地址使用的绝对基础地址。</param>
    /// <param name="configureClient">底层客户端配置回调。</param>
    /// <param name="logger">可选日志记录器。</param>
    public StandardHttpClient(
        string baseUrl,
        Action<System.Net.Http.HttpClient> configureClient,
        ILogger<StandardHttpClient>? logger = null)
        : this(CreateHttpClient(baseUrl, configureClient: configureClient), logger, ownsHttpClient: true)
    {
    }

    /// <summary>
    /// 使用指定的基础地址和消息处理器创建客户端。
    /// </summary>
    /// <param name="baseUrl">所有相对请求地址使用的绝对基础地址。</param>
    /// <param name="handler">底层消息处理器；客户端负责释放它。</param>
    /// <param name="configureClient">可选底层客户端配置回调。</param>
    /// <param name="logger">可选日志记录器。</param>
    public StandardHttpClient(
        string baseUrl,
        System.Net.Http.HttpMessageHandler handler,
        Action<System.Net.Http.HttpClient>? configureClient = null,
        ILogger<StandardHttpClient>? logger = null)
        : this(CreateHttpClient(baseUrl, handler, configureClient), logger, ownsHttpClient: true)
    {
    }

    /// <summary>
    /// 使用由调用方管理生命周期的 <see cref="System.Net.Http.HttpClient"/> 创建客户端。
    /// </summary>
    /// <param name="httpClient">底层 HTTP 客户端。</param>
    /// <param name="logger">可选日志记录器。</param>
    public StandardHttpClient(
        System.Net.Http.HttpClient httpClient,
        ILogger<StandardHttpClient>? logger = null)
        : this(httpClient, logger, ownsHttpClient: false)
    {
    }

    private StandardHttpClient(
        System.Net.Http.HttpClient httpClient,
        ILogger<StandardHttpClient>? logger,
        bool ownsHttpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        _httpClient = httpClient;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<StandardHttpClient>.Instance;
        _ownsHttpClient = ownsHttpClient;
    }

    /// <summary>
    /// 释放由此实例创建的底层 <see cref="System.Net.Http.HttpClient"/>。
    /// </summary>
    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    /// <inheritdoc />
    public virtual async Task<HttpResponseMessage> SendAsync(
        string url,
        HttpMethod method,
        object? requestBody = null,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(url, method);
        var requestUrl = AppendQueryParameters(url, queryParams);
        using var requestCancellationSource = CreateRequestCancellationSource(cancellationToken);
        var requestCancellationToken = requestCancellationSource.Token;

        try
        {
            return await SendCoreAsync(
                requestUrl,
                method,
                requestBody,
                headers,
                completionOption,
                requestCancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug(ex, "Raw HTTP {Method} request to {Url} was cancelled", method, requestUrl);

            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Raw HTTP {Method} request to {Url} timed out", method, requestUrl);

            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Raw HTTP {Method} request to {Url} failed", method, requestUrl);

            throw;
        }
    }

    /// <inheritdoc />
    public virtual async Task<ApiResult<T>> CallApi<T>(
        string url,
        HttpMethod method,
        object? requestBody = null,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(url, method);
        var requestUrl = AppendQueryParameters(url, queryParams);
        var result = new ApiResult<T>();
        using var requestCancellationSource = CreateRequestCancellationSource(cancellationToken);
        var requestCancellationToken = requestCancellationSource.Token;

        try
        {
            using var response = await SendCoreAsync(
                requestUrl,
                method,
                requestBody,
                headers,
                HttpCompletionOption.ResponseHeadersRead,
                requestCancellationToken).ConfigureAwait(false);
            result.StatusCode = response.StatusCode;
            result = await HandleResponseAsync<T>(response, requestCancellationToken).ConfigureAwait(false);

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "HTTP {Method} request to {Url} timed out", method, requestUrl);
            result.ErrorMsg = "The HTTP request timed out.";

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP {Method} request to {Url} failed", method, requestUrl);
            result.ErrorMsg = ex.Message;

            return result;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "HTTP {Method} response from {Url} could not be read", method, requestUrl);
            result.ErrorMsg = ex.Message;

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "HTTP {Method} response from {Url} contains invalid JSON", method, requestUrl);
            result.ErrorMsg = ex.Message;

            return result;
        }
    }

    /// <inheritdoc />
    public virtual async Task<ApiResult<T>> UploadFileAsync<T>(
        string url,
        HttpMethod method,
        Stream fileStream,
        string fileName,
        IReadOnlyDictionary<string, string>? formData = null,
        string fileFieldName = "file",
        string? contentType = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileFieldName);

        using var multipartContent = CreateMultipartContent(
            fileStream,
            fileName,
            formData,
            fileFieldName,
            contentType);

        return await CallApi<T>(
            url,
            method,
            multipartContent,
            headers: headers,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<ApiResult> DownloadToFileAsync(
        string url,
        string destinationPath,
        int bufferSize = 8192,
        IProgress<(long downloaded, long? total)>? progress = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);
        using var requestCancellationSource = CreateRequestCancellationSource(cancellationToken);
        var requestCancellationToken = requestCancellationSource.Token;

        try
        {
            using var response = await SendCoreAsync(
                url,
                HttpMethod.Get,
                requestBody: null,
                headers,
                HttpCompletionOption.ResponseHeadersRead,
                requestCancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var (errorMessage, errors) = await GetErrorMessageAsync(
                    response,
                    requestCancellationToken).ConfigureAwait(false);

                return new ApiResult
                {
                    StatusCode = response.StatusCode,
                    ErrorMsg = errorMessage,
                    Errors = errors
                };
            }

            using var sourceStream = await ReadResponseStreamAsync(
                response.Content,
                requestCancellationToken).ConfigureAwait(false);

            return await WriteDownloadAsync(
                sourceStream,
                response.Content.Headers.ContentLength,
                response.StatusCode,
                destinationPath,
                bufferSize,
                progress,
                requestCancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Download from {Url} timed out", url);

            return new ApiResult { ErrorMsg = "The HTTP request timed out." };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Download from {Url} failed", url);

            return new ApiResult { ErrorMsg = ex.Message };
        }
    }

    /// <summary>
    /// 获取请求序列化选项。
    /// </summary>
    /// <returns>JSON 序列化选项。</returns>
    protected virtual JsonSerializerOptions GetRequestJsonOptions()
    {
        return s_defaultRequestOptions;
    }

    /// <summary>
    /// 获取响应反序列化选项。
    /// </summary>
    /// <returns>JSON 反序列化选项。</returns>
    protected virtual JsonSerializerOptions GetResponseJsonOptions()
    {
        return s_defaultResponseOptions;
    }

    private async Task<HttpResponseMessage> SendCoreAsync(
        string requestUrl,
        HttpMethod method,
        object? requestBody,
        IReadOnlyDictionary<string, string>? headers,
        HttpCompletionOption completionOption,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, requestUrl)
        {
            Content = requestBody is null ? null : CreateHttpContent(requestBody)
        };
        ApplyHeaders(request, headers);

        var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
        _logger.LogDebug(
            "[{RequestId}] Sending HTTP {Method} request to {Url}",
            requestId,
            method,
            request.RequestUri);

        var response = await _httpClient.SendAsync(
            request,
            completionOption,
            cancellationToken).ConfigureAwait(false);

        _logger.LogDebug(
            "[{RequestId}] HTTP {Method} request completed with status {StatusCode}",
            requestId,
            method,
            (int)response.StatusCode);

        return response;
    }

    /// <summary>
    /// 解析失败响应。
    /// </summary>
    /// <param name="response">HTTP 响应。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>错误消息和错误集合。</returns>
    protected virtual async Task<(string ErrorMessage, IEnumerable<Error> Errors)> GetErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var responseText = await ReadErrorBodyAsync(response.Content, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(responseText))
        {
            return (GetStatusCodeMessage(response.StatusCode)
                ?? $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}", []);
        }

        var problemDetails = TryParseProblemDetails(response, responseText);
        if (problemDetails is not null)
        {
            var errors = FlattenErrors(problemDetails.Errors);
            var errorMessage = !string.IsNullOrWhiteSpace(problemDetails.Detail)
                ? problemDetails.Detail!
                : errors.Count > 0
                    ? errors[0].Message
                    : !string.IsNullOrWhiteSpace(problemDetails.Title)
                        ? problemDetails.Title!
                        : "The operation could not be completed.";

            return (errorMessage, errors);
        }

        var legacyErrors = TryParseLegacyErrors(responseText);
        if (legacyErrors.Count > 0)
        {
            var firstError = legacyErrors[0];
            var errorMessage = !string.IsNullOrWhiteSpace(firstError.Message)
                ? firstError.Message
                : !string.IsNullOrWhiteSpace(firstError.Code)
                    ? $"Business Error: {firstError.Code}"
                    : "The operation could not be completed.";

            return (errorMessage, legacyErrors);
        }

        var statusMessage = GetStatusCodeMessage(response.StatusCode);
        if (statusMessage is not null)
        {
            return (statusMessage, [new Error(string.Empty, responseText)]);
        }

#if NET8_0_OR_GREATER
        var fallbackMessage = responseText.Length > 200
            ? string.Concat(responseText.AsSpan(0, 200), "...".AsSpan())
            : responseText;
#else
        var fallbackMessage = responseText.Length > 200
            ? responseText.Substring(0, 200) + "..."
            : responseText;
#endif

        return (fallbackMessage, []);
    }

    private static void ValidateRequest(string url, HttpMethod method)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentNullException.ThrowIfNull(method);
    }

    private static System.Net.Http.HttpClient CreateHttpClient(
        string baseUrl,
        System.Net.Http.HttpMessageHandler? handler = null,
        Action<System.Net.Http.HttpClient>? configureClient = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);

        var httpClient = handler is null
            ? new System.Net.Http.HttpClient()
            : new System.Net.Http.HttpClient(handler);

        try
        {
            httpClient.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
            configureClient?.Invoke(httpClient);

            return httpClient;
        }
        catch
        {
            httpClient.Dispose();

            throw;
        }
    }

    private CancellationTokenSource CreateRequestCancellationSource(CancellationToken cancellationToken)
    {
        var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (_httpClient.Timeout != Timeout.InfiniteTimeSpan)
        {
            source.CancelAfter(_httpClient.Timeout);
        }

        return source;
    }

    private HttpContent CreateHttpContent(object requestBody)
    {
        return requestBody switch
        {
            HttpContent content => content,
            IDictionary<string, string> dictionary => new FormUrlEncodedContent(dictionary),
            _ => new StringContent(
                JsonSerializer.Serialize(requestBody, GetRequestJsonOptions()),
                Encoding.UTF8,
                "application/json")
        };
    }

    private static MultipartFormDataContent CreateMultipartContent(
        Stream fileStream,
        string fileName,
        IReadOnlyDictionary<string, string>? formData,
        string fileFieldName,
        string? contentType)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            contentType ?? GetContentType(fileName));
        content.Add(fileContent, fileFieldName, fileName);

        if (formData is not null)
        {
            foreach (var field in formData)
            {
                content.Add(new StringContent(field.Value), field.Key);
            }
        }

        return content;
    }

    private static string GetContentType(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }

    private static void ApplyHeaders(
        HttpRequestMessage request,
        IReadOnlyDictionary<string, string>? headers)
    {
        if (headers is null)
        {
            return;
        }

        foreach (var header in headers)
        {
            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }

    private static string AppendQueryParameters(string url, object? queryParams)
    {
        var queryString = BuildQueryString(queryParams);

        return string.IsNullOrEmpty(queryString) ? url : url.AppendQuery(queryString);
    }

    private static string BuildQueryString(object? queryParams)
    {
        if (queryParams is null)
        {
            return string.Empty;
        }

        if (queryParams is IEnumerable<KeyValuePair<string, string>> dictionary)
        {
            return string.Join("&", dictionary.Select(static item =>
                $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value)}"));
        }

        var properties = s_queryProperties.GetOrAdd(
            queryParams.GetType(),
            static type => type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(static property => property.CanRead && property.GetIndexParameters().Length == 0)
                .ToArray());
        var queryParts = new List<string>();

        foreach (var property in properties)
        {
            var value = property.GetValue(queryParams);
            if (value is null)
            {
                continue;
            }

            var propertyName = Uri.EscapeDataString(property.Name);
            if (value is not string && value is IEnumerable values)
            {
                foreach (var item in values)
                {
                    if (item is not null)
                    {
                        queryParts.Add($"{propertyName}={Uri.EscapeDataString(ToInvariantString(item))}");
                    }
                }
            }
            else
            {
                queryParts.Add($"{propertyName}={Uri.EscapeDataString(ToInvariantString(value))}");
            }
        }

        return string.Join("&", queryParts);
    }

    private static string ToInvariantString(object value)
    {
        return value is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty
            : value.ToString() ?? string.Empty;
    }

    private async Task<ApiResult<T>> HandleResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var result = new ApiResult<T> { StatusCode = response.StatusCode };

        if (response.IsSuccessStatusCode)
        {
            result.Data = await DeserializeResponseContentAsync<T>(response.Content, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            (result.ErrorMsg, result.Errors) = await GetErrorMessageAsync(response, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }

    private async Task<T> DeserializeResponseContentAsync<T>(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var targetType = typeof(T);

        if (targetType == typeof(Stream) || targetType == typeof(HttpResponseMessage))
        {
            throw new NotSupportedException(
                $"{targetType.Name} responses are not supported by {nameof(CallApi)}. " +
                $"Use {nameof(DownloadToFileAsync)} or {nameof(SendAsync)} instead.");
        }

        if (targetType == typeof(string))
        {
            return (T)(object)await ReadResponseStringAsync(content, cancellationToken).ConfigureAwait(false);
        }

        if (targetType == typeof(byte[]))
        {
            return (T)(object)await ReadResponseByteArrayAsync(content, cancellationToken).ConfigureAwait(false);
        }

        using var responseStream = new JsonContentTrackingStream(
            await ReadResponseStreamAsync(content, cancellationToken).ConfigureAwait(false));

        try
        {
            return await JsonSerializer.DeserializeAsync<T>(
                responseStream,
                GetResponseJsonOptions(),
                cancellationToken).ConfigureAwait(false) ?? default!;
        }
        catch (JsonException) when (!responseStream.HasNonWhitespaceContent)
        {
            return default!;
        }
    }

    private sealed class JsonContentTrackingStream(Stream inner) : Stream
    {
        public bool HasNonWhitespaceContent { get; private set; }
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => inner.Position = value;
        }

        public override void Flush()
        {
            inner.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = inner.Read(buffer, offset, count);
            TrackContent(buffer, offset, bytesRead);

            return bytesRead;
        }

        public override async Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
#if NET8_0_OR_GREATER
            var bytesRead = await inner.ReadAsync(
                buffer.AsMemory(offset, count),
                cancellationToken).ConfigureAwait(false);
#else
            var bytesRead = await inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
#endif
            TrackContent(buffer, offset, bytesRead);

            return bytesRead;
        }

#if NET8_0_OR_GREATER
        public override int Read(Span<byte> buffer)
        {
            var bytesRead = inner.Read(buffer);
            TrackContent(buffer[..bytesRead]);

            return bytesRead;
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            var bytesRead = await inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            TrackContent(buffer.Span[..bytesRead]);

            return bytesRead;
        }
#endif

        public override long Seek(long offset, SeekOrigin origin)
        {
            return inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
            }

            base.Dispose(disposing);
        }

        private void TrackContent(byte[] buffer, int offset, int count)
        {
            if (HasNonWhitespaceContent)
            {
                return;
            }

            for (var index = offset; index < offset + count; index++)
            {
                if (!IsJsonWhitespace(buffer[index]))
                {
                    HasNonWhitespaceContent = true;

                    return;
                }
            }
        }

#if NET8_0_OR_GREATER
        private void TrackContent(ReadOnlySpan<byte> buffer)
        {
            if (HasNonWhitespaceContent)
            {
                return;
            }

            foreach (var value in buffer)
            {
                if (!IsJsonWhitespace(value))
                {
                    HasNonWhitespaceContent = true;

                    return;
                }
            }
        }
#endif

        private static bool IsJsonWhitespace(byte value)
        {
            return value is 0x20 or 0x09 or 0x0A or 0x0D;
        }
    }

    private ProblemDetailsWithErrors? TryParseProblemDetails(
        HttpResponseMessage response,
        string responseText)
    {
        try
        {
            var problemDetails = JsonSerializer.Deserialize<ProblemDetailsWithErrors>(
                responseText,
                GetResponseJsonOptions());
            if (problemDetails is null)
            {
                return null;
            }

            var isProblemContentType = string.Equals(
                response.Content.Headers.ContentType?.MediaType,
                "application/problem+json",
                StringComparison.OrdinalIgnoreCase);
            var hasProblemFields = problemDetails.Status.HasValue
                || !string.IsNullOrWhiteSpace(problemDetails.Type)
                || !string.IsNullOrWhiteSpace(problemDetails.Title)
                || !string.IsNullOrWhiteSpace(problemDetails.Detail)
                || problemDetails.Errors is { Count: > 0 };

            return isProblemContentType || hasProblemFields ? problemDetails : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private List<Error> TryParseLegacyErrors(string responseText)
    {
        try
        {
            return JsonSerializer.Deserialize<List<Error>>(responseText, GetResponseJsonOptions())
                ?.Where(static error => error is not null)
                .ToList() ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static List<Error> FlattenErrors(Dictionary<string, string[]>? errors)
    {
        var result = new List<Error>();
        if (errors is null)
        {
            return result;
        }

        foreach (var error in errors)
        {
            if (error.Value is null)
            {
                continue;
            }

            foreach (var message in error.Value)
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    result.Add(new Error(error.Key, message));
                }
            }
        }

        return result;
    }

    private static async Task<string> ReadErrorBodyAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        using var stream = await ReadResponseStreamAsync(content, cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream);
        var buffer = new char[MaxErrorBodyCharacters];
#if NET8_0_OR_GREATER
        var readCount = await reader.ReadBlockAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
#else
        var readCount = await reader.ReadBlockAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
#endif

        return new string(buffer, 0, readCount);
    }

    private static Task<Stream> ReadResponseStreamAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        return content.ReadAsStreamAsync(cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return content.ReadAsStreamAsync();
#endif
    }

    private static Task<byte[]> ReadResponseByteArrayAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        return content.ReadAsByteArrayAsync(cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return content.ReadAsByteArrayAsync();
#endif
    }

    private static Task<string> ReadResponseStringAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        return content.ReadAsStringAsync(cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return content.ReadAsStringAsync();
#endif
    }

    private static async Task<ApiResult> WriteDownloadAsync(
        Stream sourceStream,
        long? totalBytes,
        HttpStatusCode statusCode,
        string destinationPath,
        int bufferSize,
        IProgress<(long downloaded, long? total)>? progress,
        CancellationToken cancellationToken)
    {
        var fullDestinationPath = Path.GetFullPath(destinationPath);
        var destinationDirectory = Path.GetDirectoryName(fullDestinationPath) ?? Directory.GetCurrentDirectory();
        var tempPath = Path.Combine(
            destinationDirectory,
            $".{Path.GetFileName(fullDestinationPath)}.{Guid.NewGuid():N}.tmp");
        var committed = false;

        try
        {
            using (var fileStream = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                true))
            {
                var buffer = new byte[bufferSize];
                long downloadedBytes = 0;
                int bytesRead;

#if NET8_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                while ((bytesRead = await sourceStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
#else
                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
#endif
                    downloadedBytes += bytesRead;
                    progress?.Report((downloadedBytes, totalBytes));
                }

                await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

#if NETCOREAPP3_0_OR_GREATER
            File.Move(tempPath, fullDestinationPath, true);
#else
            if (File.Exists(fullDestinationPath))
            {
                File.Replace(tempPath, fullDestinationPath, null);
            }
            else
            {
                File.Move(tempPath, fullDestinationPath);
            }
#endif
            committed = true;

            return new ApiResult { StatusCode = statusCode };
        }
        finally
        {
            if (!committed && File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private static string? GetStatusCodeMessage(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "Bad request",
            HttpStatusCode.Unauthorized => "Authentication required",
            HttpStatusCode.Forbidden => "Access forbidden",
            HttpStatusCode.NotFound => "Resource not found",
            HttpStatusCode.MethodNotAllowed => "Method not allowed",
            HttpStatusCode.Conflict => "Request conflict",
            (HttpStatusCode)422 => "Validation failed",
            (HttpStatusCode)429 => "Too many requests, please try again later",
            HttpStatusCode.InternalServerError => "Internal server error",
            HttpStatusCode.BadGateway => "Bad gateway",
            HttpStatusCode.ServiceUnavailable => "Service unavailable",
            HttpStatusCode.GatewayTimeout => "Gateway timeout",
            _ => null
        };
    }

}
