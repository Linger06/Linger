using Flurl;
using Flurl.Http;
using Linger.Exceptions;
using Linger.Helper;
using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Models;
#if NETFRAMEWORK
using System.Net.Http;
#endif

namespace Linger.HttpClient.Flurl;

public class FlurlHttpClient : HttpClientBase
{
    private readonly IFlurlClient _flurlClient;

    public FlurlHttpClient(string baseUrl)
    {
        _flurlClient = new FlurlClient(baseUrl);
        ConfigureFlurlClient();
    }

    public FlurlHttpClient(System.Net.Http.HttpClient httpClient)
    {
        _flurlClient = new FlurlClient(httpClient);
        ConfigureFlurlClient();
    }

    public FlurlHttpClient(IFlurlClient flurlClient)
    {
        _flurlClient = flurlClient ?? throw new ArgumentNullException(nameof(flurlClient));
        ConfigureFlurlClient();
    }

    private void ConfigureFlurlClient()
    {
        _flurlClient.Settings.Timeout = TimeSpan.FromSeconds(Options.DefaultTimeout);

        // 设置默认请求头
        foreach (var header in Options.DefaultHeaders)
        {
            _flurlClient.Headers.Add(header.Key, header.Value);
        }

        if (Options.EnableRetry)
        {
            _flurlClient.Settings.Redirects.MaxAutoRedirects = Options.MaxRetryCount;
        }
    }

    public override void SetToken(string token)
    {
        _ = _flurlClient.WithOAuthBearerToken(token);
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

            // 设置语言环境
            flurlRequest = flurlRequest.SetQueryParam("culture", Thread.CurrentThread.CurrentUICulture.Name);

            // 使用合并后的取消令牌
            //var flurlResponse = await ExecuteFlurlRequest(flurlRequest, method, content, combinedToken);

            // 执行请求（带重试）
            var getResponseTask = ProcessRequestWithRetriesAsync(
                async () => await ExecuteFlurlRequest(flurlRequest, method, content, combinedToken),
                combinedToken);

            var res = await getResponseTask;
            //HttpResponseMessage? res = flurlResponse.ResponseMessage;
            rv = await HandleResponseMessage<T>(res.ResponseMessage);

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

    private async Task<IFlurlResponse> ProcessRequestWithRetriesAsync(
        Func<Task<IFlurlResponse>> requestFunc,
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
            return ex is FlurlHttpException;
        }

        try
        {
            // 使用RetryHelper执行HTTP请求
            return await retryHelper.ExecuteAsync(
                requestFunc,
                "Flurl HTTP Request",
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