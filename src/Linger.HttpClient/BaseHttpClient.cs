using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Linger.Extensions.Core;
using Linger.HttpClient.Contracts;
using Linger.Helper;
using Linger.Exceptions;


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
        // 只有当启用重试时才创建RetryHelper
        if (!Options.EnableRetry)
        {
            return await requestFunc();
        }
        
        // 创建RetryOptions并配置
        var retryOptions = new RetryOptions
        {
            MaxRetries = Options.MaxRetryCount,
            BaseDelayMs = Options.RetryInterval,
            UseExponentialBackoff = true, // 使用指数退避策略
            JitterFactor = 0.2 // 添加20%的随机抖动
        };
        
        var retryHelper = new RetryHelper(retryOptions);
        
        // 定义哪些异常类型需要重试
        bool ShouldRetry(Exception ex)
        {
            return ex is HttpRequestException || ex is TaskCanceledException;
        }
        
        try
        {
            // 使用RetryHelper执行HTTP请求
            return await retryHelper.ExecuteAsync(
                requestFunc,
                "HTTP Request",
                ShouldRetry,
                cancellationToken);
        }
        catch (OutOfReTryCountException retryEx)
        {
            // 保持与原始实现一致，将原始异常抛出
            if (retryEx.InnerException != null)
            {
                throw retryEx.InnerException;
            }
            throw;
        }
    }
}