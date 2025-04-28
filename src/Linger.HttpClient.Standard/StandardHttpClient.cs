using System.Net.Http.Headers;
using Linger.Extensions.Core;
using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Models;

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

    /// <summary>
    /// 创建一个新的Standard HTTP客户端
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    public StandardHttpClient(string baseUrl)
    {
        _httpClient = new System.Net.Http.HttpClient { BaseAddress = new Uri(baseUrl) };
        SetDefaultOptions();
    }

    /// <summary>
    /// 创建一个新的Standard HTTP客户端
    /// </summary>
    /// <param name="httpClient">现有的HttpClient实例</param>
    public StandardHttpClient(System.Net.Http.HttpClient httpClient)
    {
        _httpClient = httpClient;
        SetDefaultOptions();
    }

    /// <summary>
    /// 创建一个新的Standard HTTP客户端并应用选项
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="options">客户端选项</param>
    public StandardHttpClient(string baseUrl, HttpClientOptions options)
    {
        _httpClient = new System.Net.Http.HttpClient { BaseAddress = new Uri(baseUrl) };

        // 复制选项
        CopyOptions(options);
        SetDefaultOptions();
    }

    /// <summary>
    /// 创建一个新的Standard HTTP客户端并应用选项
    /// </summary>
    /// <param name="httpClient">现有的HttpClient实例</param>
    /// <param name="options">客户端选项</param>
    public StandardHttpClient(System.Net.Http.HttpClient httpClient, HttpClientOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

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
    }

    /// <summary>
    /// 创建一个预配置的Standard HTTP客户端
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="configureOptions">配置选项的操作</param>
    /// <returns>配置好的HTTP客户端</returns>
    public static StandardHttpClient Create(string baseUrl, Action<HttpClientOptions>? configureOptions = null)
    {
        var options = new HttpClientOptions();
        configureOptions?.Invoke(options);

        var client = new StandardHttpClient(baseUrl, options);
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
    }

    public override async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, HttpContent? content = null,
        object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default) //where T : class
    {
        ApiResult<T> rv = new();

        try
        {
            if (string.IsNullOrEmpty(url))
            {
                return rv;
            }

            url = url.AppendQuery("culture=" + Thread.CurrentThread.CurrentUICulture.Name);

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
                }
            }

            // 执行请求
            var res = await _httpClient.SendAsync(request, combinedToken);

            rv = await HandleResponseMessage<T>(res);

            return rv;
        }
        catch (OperationCanceledException) when (timeout.HasValue && !cancellationToken.IsCancellationRequested)
        {
            // 处理超时异常（与用户取消区分开）
            rv.ErrorMsg = $"请求超时，超时设置: {timeout}秒";
            return rv;
        }
        catch (Exception ex)
        {
            rv.ErrorMsg = ex.ToString();
            return rv;
        }
    }
}