using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Linger.Extensions.Core;
using Linger.HttpClient.Contracts;

#if NETFRAMEWORK
using System.Net;
using System.Net.Http;
#endif

namespace Linger.HttpClient;

public class BaseHttpClient : BaseClient
{
    private readonly System.Net.Http.HttpClient _httpClient;

    public BaseHttpClient(string baseUrl)
    {
        _httpClient = new System.Net.Http.HttpClient { BaseAddress = new Uri(baseUrl) };
        SetDefaultOptions();
    }

    public BaseHttpClient(System.Net.Http.HttpClient httpClient)
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
            
            //设置超时
            var originalTimeout = _httpClient.Timeout;
            if (timeout.HasValue)
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(timeout.Value);
            }

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
            
            // 执行请求（带重试）
            var getResponseTask = ProcessRequestWithRetriesAsync(
                async () => await _httpClient.SendAsync(request, cancellationToken), 
                cancellationToken);
            
            var res = await getResponseTask;
            
            // 应用响应拦截器
            res = await ApplyInterceptorsToResponseAsync(res);
            
            // 恢复原始超时设置
            if (timeout.HasValue)
            {
                _httpClient.Timeout = originalTimeout;
            }

            rv = await HandleResponseMessage<T>(res);

            return rv;
        }
        catch (Exception ex)
        {
            rv.ErrorMsg = ex.ToString();
            return rv;
        }
    }

    private async Task<HttpResponseMessage> ProcessRequestWithRetriesAsync(
        Func<Task<HttpResponseMessage>> requestFunc, 
        CancellationToken cancellationToken)
    {
        int retryCount = 0;
        
        while (true)
        {
            try
            {
                return await requestFunc();
            }
            catch (Exception ex) when (
                (ex is HttpRequestException || ex is TaskCanceledException) && 
                Options.EnableRetry && 
                retryCount < Options.MaxRetryCount)
            {
                retryCount++;
                
                // 如果已经取消，直接抛出异常
                cancellationToken.ThrowIfCancellationRequested();
                
                // 延迟一段时间后重试
                await Task.Delay(Options.RetryInterval, cancellationToken);
            }
        }
    }
}