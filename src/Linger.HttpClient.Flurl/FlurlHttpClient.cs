using Flurl;
using Flurl.Http;
using Linger.Extensions.Core;
using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Factories;
using Linger.HttpClient.Contracts.Models;
#if NETFRAMEWORK
using System.Net.Http;
#endif

namespace Linger.HttpClient.Flurl;

/// <summary>
/// 基于Flurl的HTTP客户端实现
/// </summary>
public class FlurlHttpClient : HttpClientBase
{
    private readonly IFlurlClient _flurlClient;

    /// <summary>
    /// 创建一个新的Flurl HTTP客户端
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    public FlurlHttpClient(string baseUrl)
    {
        _flurlClient = new FlurlClient(baseUrl);
        ConfigureFlurlClient();
    }

    /// <summary>
    /// 创建一个新的Flurl HTTP客户端
    /// </summary>
    /// <param name="httpClient">现有的HttpClient实例</param>
    public FlurlHttpClient(System.Net.Http.HttpClient httpClient)
    {
        _flurlClient = new FlurlClient(httpClient);
        ConfigureFlurlClient();
    }

    /// <summary>
    /// 创建一个新的Flurl HTTP客户端
    /// </summary>
    /// <param name="flurlClient">现有的FlurlClient实例</param>
    public FlurlHttpClient(IFlurlClient flurlClient)
    {
        _flurlClient = flurlClient ?? throw new ArgumentNullException(nameof(flurlClient));
        ConfigureFlurlClient();
    }

    /// <summary>
    /// 创建一个新的Flurl HTTP客户端并应用选项
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="options">客户端选项</param>
    public FlurlHttpClient(string baseUrl, HttpClientOptions options)
    {
        _flurlClient = new FlurlClient(baseUrl);
        
        // 复制选项
        CopyOptions(options);
        ConfigureFlurlClient();
    }

    /// <summary>
    /// 创建一个新的Flurl HTTP客户端并应用选项
    /// </summary>
    /// <param name="httpClient">现有的HttpClient实例</param>
    /// <param name="options">客户端选项</param>
    public FlurlHttpClient(System.Net.Http.HttpClient httpClient, HttpClientOptions options)
    {
        _flurlClient = new FlurlClient(httpClient);
        
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
        
        ConfigureFlurlClient();
    }

    /// <summary>
    /// 创建一个预配置的Flurl HTTP客户端
    /// </summary>
    /// <param name="baseUrl">基础URL</param>
    /// <param name="configureOptions">配置选项的操作</param>
    /// <returns>配置好的HTTP客户端</returns>
    public static FlurlHttpClient Create(string baseUrl, Action<HttpClientOptions>? configureOptions = null)
    {
        var options = new HttpClientOptions();
        configureOptions?.Invoke(options);

        var client = new FlurlHttpClient(baseUrl, options);

        // 添加基于配置的标准拦截器
        foreach (var interceptor in DefaultInterceptorFactory.CreateStandardInterceptors(client.Options))
        {
            client.AddInterceptor(interceptor);
        }

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
        Options.EnableRetry = options.EnableRetry;
        Options.MaxRetryCount = options.MaxRetryCount;
        Options.RetryInterval = options.RetryInterval;
    }

    private void ConfigureFlurlClient()
    {
        _flurlClient.Settings.Timeout = TimeSpan.FromSeconds(Options.DefaultTimeout);

        // 设置默认请求头
        foreach (var header in Options.DefaultHeaders)
        {
            _flurlClient.Headers.Add(header.Key, header.Value);
        }
    }

    public override void SetToken(string token)
    {
        // 修正：不使用忽略结果的语法，确保令牌正确应用
        if (string.IsNullOrEmpty(token))
        {
            _flurlClient.Headers.Remove("Authorization");
        }
        else
        {
            _flurlClient.WithOAuthBearerToken(token);
        }
    }

    /// <summary>
    /// 获取底层Flurl客户端用于高级操作
    /// </summary>
    /// <returns>Flurl客户端实例</returns>
    public IFlurlClient GetFlurlClient()
    {
        return _flurlClient;
    }

    /// <summary>
    ///     最终都会调用此Api
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">调用方式</param>
    /// <param name="content">HttpContent</param>
    /// <param name="queryParams">查询参数</param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, HttpContent? content = null,
        object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default) //where T : class
    {
        ApiResult<T> rv = new();

        if (string.IsNullOrEmpty(url))
        {
            return rv;
        }

        try
        {
            // 使用超时令牌源
            using var timeoutSource = CreateTimeoutTokenSource(timeout, cancellationToken);
            var combinedToken = timeoutSource.Token;

            // 处理基础URL和路径组合
            IFlurlRequest? flurlRequest = _flurlClient.Request();

            if (timeout.HasValue)
            {
                flurlRequest = flurlRequest.WithTimeout(timeout.Value);
            }

            var baseUrl = _flurlClient.BaseUrl;
            var baseUrlAddPath = Url.Combine(baseUrl, url);
            var df = new Url(baseUrlAddPath);
            QueryParamCollection? queryParamCollection = df.QueryParams;

            if (queryParamCollection != null)
            {
                flurlRequest = flurlRequest.SetQueryParams(queryParamCollection);
            }

            var query = df.Query;

            url = url.Replace($"?{query}", string.Empty);

            // 添加文化信息
            url = url.AppendQuery("culture=" + Thread.CurrentThread.CurrentUICulture.Name);

            flurlRequest = flurlRequest.AppendPathSegment(url).AllowAnyHttpStatus();

            // 使用BuildQueryString替代直接设置查询参数
            if (queryParams != null)
            {
                string queryString = BuildQueryString(queryParams);
                if (!string.IsNullOrEmpty(queryString))
                {
                    flurlRequest = flurlRequest.SetQueryParams(queryString);
                }
            }

            // 构建HttpRequestMessage用于应用拦截器
            var httpMethod = method switch
            {
                HttpMethodEnum.Get => HttpMethod.Get,
                HttpMethodEnum.Post => HttpMethod.Post,
                HttpMethodEnum.Put => HttpMethod.Put,
                HttpMethodEnum.Delete => HttpMethod.Delete,
                _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
            };

            var requestUri = new Uri(flurlRequest.Url);
            var request = new HttpRequestMessage(httpMethod, requestUri)
            {
                Content = content
            };

            // 应用请求拦截器
            request = await ApplyInterceptorsToRequestAsync(request);

            // 执行请求 - 使用修改后的URI (如果拦截器改变了URI)
            if (request.RequestUri != requestUri)
            {
                flurlRequest = new FlurlRequest(request.RequestUri);
            }

            // 从拦截器处理过的请求中复制头部到Flurl请求
            foreach (var header in request.Headers)
            {
                flurlRequest.WithHeader(header.Key, header.Value);
            }

            // 使用合并后的取消令牌，直接执行请求
            var flurlResponse = await ExecuteFlurlRequest(flurlRequest, method, request.Content ?? content, combinedToken);

            // 应用响应拦截器
            var res = flurlResponse.ResponseMessage;
            if (res != null)
            {
                res = await ApplyInterceptorsToResponseAsync(res);
            }

            rv = await HandleResponseMessage<T>(res);

            return rv;
        }
        catch (OperationCanceledException) when (timeout.HasValue && !cancellationToken.IsCancellationRequested)
        {
            rv.ErrorMsg = $"请求超时，超时设置: {timeout}秒";
            return rv;
        }
        catch (Exception ex)
        {
            rv.ErrorMsg = ex.ToString();
            return rv;
        }
    }

    private async Task<IFlurlResponse> ExecuteFlurlRequest(
        IFlurlRequest request,
        HttpMethodEnum method,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        return method switch
        {
            HttpMethodEnum.Get => await request.GetAsync(cancellationToken: cancellationToken),
            HttpMethodEnum.Post => await request.PostAsync(content, cancellationToken: cancellationToken),
            HttpMethodEnum.Put => await request.PutAsync(content, cancellationToken: cancellationToken),
            HttpMethodEnum.Delete => await request.DeleteAsync(cancellationToken: cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
        };
    }
}