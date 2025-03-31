using System.Net.Http.Headers;
using Linger.Exceptions;
using Linger.Extensions.Core;
using Linger.Helper;
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

    public StandardHttpClient(string baseUrl)
    {
        _httpClient = new System.Net.Http.HttpClient { BaseAddress = new Uri(baseUrl) };
        SetDefaultOptions();
    }

    public StandardHttpClient(System.Net.Http.HttpClient httpClient)
    {
        _httpClient = httpClient;
        SetDefaultOptions();
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
            using var timeoutSource = CreateTimeoutTokenSource(timeout, cancellationToken);
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

            // 应用请求拦截器
            request = await ApplyInterceptorsToRequestAsync(request);

            // 处理查询参数
            if (queryParams != null)
            {
                string queryString = BuildQueryString(queryParams);
                if (!string.IsNullOrEmpty(queryString))
                {
                    url = url.Contains('?') ? $"{url}&{queryString}" : $"{url}?{queryString}";
                }
            }

            // 执行请求 - 移除重试逻辑
            var res = await _httpClient.SendAsync(request, combinedToken);

            // 应用响应拦截器
            res = await ApplyInterceptorsToResponseAsync(res);

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