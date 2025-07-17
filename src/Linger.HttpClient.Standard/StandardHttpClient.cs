using System.Net.Http.Headers;
using Linger.Extensions.Core;
using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Models;
using Microsoft.Extensions.Logging;

#if NETFRAMEWORK
using System.Net;
using System.Net.Http;
#endif

namespace Linger.HttpClient.Standard;

/// <summary>
/// 基于标准 System.Net.Http.HttpClient 的 HTTP 客户端实现
/// </summary>
public class StandardHttpClient : HttpClientBase
{
    private readonly System.Net.Http.HttpClient _httpClient;
    private readonly ILogger<StandardHttpClient> _logger;

    /// <summary>
    /// 创建一个新的Standard HTTP客户端
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="logger">日志记录器，可选</param>
    public StandardHttpClient(string baseUrl, ILogger<StandardHttpClient>? logger = null)
    {
        _httpClient = new System.Net.Http.HttpClient { BaseAddress = new Uri(baseUrl) };
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<StandardHttpClient>.Instance;
        SetDefaultOptions();

        _logger.LogDebug("StandardHttpClient created with base URL: {BaseUrl}", baseUrl);
    }

    /// <summary>
    /// 创建一个新的Standard HTTP客户端
    /// </summary>
    /// <param name="httpClient">现有的HttpClient实例</param>
    /// <param name="logger">日志记录器，可选</param>
    public StandardHttpClient(System.Net.Http.HttpClient httpClient, ILogger<StandardHttpClient>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<StandardHttpClient>.Instance;
        SetDefaultOptions();

        _logger.LogDebug("StandardHttpClient created with existing HttpClient instance");
    }

    /// <summary>
    /// 创建一个新的Standard HTTP客户端并应用选项
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="options">客户端选项</param>
    /// <param name="logger">日志记录器，可选</param>
    public StandardHttpClient(string baseUrl, HttpClientOptions options, ILogger<StandardHttpClient>? logger = null)
    {
        _httpClient = new System.Net.Http.HttpClient { BaseAddress = new Uri(baseUrl) };
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<StandardHttpClient>.Instance;

        // 复制选项
        CopyOptions(options);
        SetDefaultOptions();

        _logger.LogDebug("StandardHttpClient created with base URL: {BaseUrl} and custom options", baseUrl);
    }

    /// <summary>
    /// 创建一个新的Standard HTTP客户端并应用选项
    /// </summary>
    /// <param name="httpClient">现有的HttpClient实例</param>
    /// <param name="options">客户端选项</param>
    /// <param name="logger">日志记录器，可选</param>
    public StandardHttpClient(System.Net.Http.HttpClient httpClient, HttpClientOptions options, ILogger<StandardHttpClient>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<StandardHttpClient>.Instance;

        // 复制选项
        CopyOptions(options);

        // 同时设置到HttpClient的默认头部
        foreach (var header in options.DefaultHeaders)
        {
            if (!httpClient.DefaultRequestHeaders.Contains(header.Key))
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        SetDefaultOptions();

        _logger.LogDebug("StandardHttpClient created with existing HttpClient instance and custom options");
    }

    /// <summary>
    /// 创建一个预配置的Standard HTTP客户端
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="configureOptions">配置选项的操作</param>
    /// <param name="logger">日志记录器，可选</param>
    /// <returns>配置好的HTTP客户端</returns>
    public static StandardHttpClient Create(string baseUrl, Action<HttpClientOptions>? configureOptions = null, ILogger<StandardHttpClient>? logger = null)
    {
        var options = new HttpClientOptions();
        configureOptions?.Invoke(options);

        var client = new StandardHttpClient(baseUrl, options, logger);
        return client;
    }

    private void CopyOptions(HttpClientOptions options)
    {
        // 复制选项到当前实例
        foreach (var header in options.DefaultHeaders)
        {
            Options.DefaultHeaders[header.Key] = header.Value;
        }

        Options.DefaultTimeout = options.DefaultTimeout;
    }

    private void SetDefaultOptions()
    {
        _httpClient.Timeout = TimeSpan.FromSeconds(Options.DefaultTimeout);
    }

    public override void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _logger.LogDebug("Authorization token set for HTTP client");
    }

    public override async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, HttpContent? content = null,
        object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default) //where T : class
    {
        ApiResult<T> rv = new();
        var requestId = Guid.NewGuid().ToString("N").Substring(0, 8); // 生成简短的请求ID用于跟踪

        try
        {
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogWarning("[{RequestId}] Empty URL provided to CallApi", requestId);
                return rv;
            }

            url = url.AppendQuery("culture=" + Thread.CurrentThread.CurrentUICulture.Name);

            _logger.LogInformation("[{RequestId}] Starting HTTP {Method} request to {Url}",
                requestId, method, url);

            if (timeout.HasValue)
            {
                _logger.LogDebug("[{RequestId}] Request timeout set to {Timeout} seconds",
                    requestId, timeout.Value);
            }

            // 使用超时令牌源替代直接修改 _httpClient.Timeout
            using var timeoutSource = HttpClientBase.CreateTimeoutTokenSource(timeout, cancellationToken);
            var combinedToken = timeoutSource.Token;

            HttpMethod httpMethod = method switch
            {
                HttpMethodEnum.Get => HttpMethod.Get,
                HttpMethodEnum.Post => HttpMethod.Post,
                HttpMethodEnum.Put => HttpMethod.Put,
                HttpMethodEnum.Delete => HttpMethod.Delete,
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
            };

#if NETFRAMEWORK
            if (httpMethod == HttpMethod.Get && content != null)
            {
                _logger.LogError("[{RequestId}] Cannot send content body with GET request", requestId);
                throw new ProtocolViolationException("Cannot send a content-body with this verb-type.");
            }
#endif

            var request = new HttpRequestMessage(httpMethod, url)
            {
                Content = content
            };

            // 添加默认请求头
            foreach (var header in Options.DefaultHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // 处理查询参数
            if (queryParams != null)
            {
                string queryString = HttpClientBase.BuildQueryString(queryParams);
                if (!string.IsNullOrEmpty(queryString))
                {
                    url = url.Contains('?') ? $"{url}&{queryString}" : $"{url}?{queryString}";
                    _logger.LogDebug("[{RequestId}] Query parameters added: {QueryString}",
                        requestId, queryString);
                }
            }

            // 记录请求详情
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                _logger.LogDebug("[{RequestId}] Request headers: {Headers}",
                    requestId, string.Join(", ", request.Headers.Select(h => $"{h.Key}={string.Join(";", h.Value)}")));

                if (content != null)
                {
                    var contentType = content.Headers?.ContentType?.MediaType ?? "unknown";
                    _logger.LogDebug("[{RequestId}] Request content type: {ContentType}",
                        requestId, contentType);
                }
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // 执行请求
            var res = await _httpClient.SendAsync(request, combinedToken).ConfigureAwait(false);

            stopwatch.Stop();

            _logger.LogInformation("[{RequestId}] HTTP {Method} request to {Url} completed in {ElapsedMs}ms with status {StatusCode}",
                requestId, method, url, stopwatch.ElapsedMilliseconds, (int)res.StatusCode);

            rv = await HandleResponseMessage<T>(res).ConfigureAwait(false);

            if (rv.IsSuccess)
            {
                _logger.LogDebug("[{RequestId}] Response processed successfully", requestId);
            }
            else
            {
                _logger.LogWarning("[{RequestId}] Response processing failed: {ErrorMessage}",
                    requestId, rv.ErrorMsg);
            }

            return rv;
        }
        catch (OperationCanceledException ex) when (timeout.HasValue && !cancellationToken.IsCancellationRequested)
        {
            // 处理超时异常（与用户取消区分开）
            var timeoutMessage = $"请求超时，超时设置: {timeout}秒";
            _logger.LogWarning(ex, "[{RequestId}] Request timed out after {Timeout} seconds",
                requestId, timeout);

            rv.ErrorMsg = timeoutMessage;
            return rv;
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(ex, "[{RequestId}] Request was cancelled by user", requestId);
            rv.ErrorMsg = "请求被取消";
            return rv;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{RequestId}] HTTP {Method} request to {Url} failed: {ErrorMessage}",
                requestId, method, url, ex.Message);

            rv.ErrorMsg = ex.ToString();
            return rv;
        }
    }
}
