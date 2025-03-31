using System.Text.Json;
using Linger.Extensions;
using Linger.Extensions.Core;
using Linger.HttpClient.Contracts.Metrics;
using Linger.HttpClient.Contracts.Models;
using Linger.HttpClient.Contracts.Helpers;
using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace Linger.HttpClient.Contracts.Core;

/// <summary>
/// HTTP客户端抽象基类
/// </summary>
public abstract class HttpClientBase : IHttpClient
{
    private readonly ConcurrentBag<IHttpClientInterceptor> _interceptors = new();

    /// <summary>
    /// HTTP客户端选项
    /// </summary>
    public HttpClientOptions Options { get; } = new HttpClientOptions();

    /// <summary>
    /// 设置授权令牌
    /// </summary>
    public abstract void SetToken(string token);

    /// <summary>
    /// 添加请求头
    /// </summary>
    public virtual void AddHeader(string name, string value)
    {
        Options.DefaultHeaders[name] = value;
    }

    /// <summary>
    /// 添加拦截器
    /// </summary>
    public virtual void AddInterceptor(IHttpClientInterceptor interceptor)
    {
        _interceptors.Add(interceptor);
    }

    /// <summary>
    ///     使用Get方法调用api
    /// </summary>
    /// <param name="url">调用地址</param>
    /// <param name="queryParams"></param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual async Task<ApiResult<T>> CallApi<T>(string url, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default) // where T : class
    {
        // 启动性能监控
        var requestId = HttpClientMetrics.Instance.StartRequest(url);

        try
        {
            var result = await CallApi<T>(url, HttpMethodEnum.Get, null, queryParams, timeout, cancellationToken).ConfigureAwait(false);

            // 记录请求完成
            HttpClientMetrics.Instance.EndRequest(url, requestId, result.IsSuccess);

            return result;
        }
        catch
        {
            // 记录请求失败
            HttpClientMetrics.Instance.EndRequest(url, requestId, false);
            throw;
        }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">调用方式</param>
    /// <param name="postData">Post Json Data,提交的object,会被转成HttpContent提交</param>
    /// <param name="queryParams">查询参数</param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, object? postData = null, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default) //where T : class
    {
        HttpContent? content = null;

        if (postData == null)
        {
            return await CallApi<T>(url, method, content, queryParams, timeout, cancellationToken);
        }

        if (postData is IDictionary<string, string> dictionary)
        {
            var content2 = new MultipartFormDataContent();
            //填充表单数据
            if (!(dictionary.IsNull() || dictionary.Count == 0))
            {
                foreach (var key in dictionary.Keys)
                {
                    content2.Add(new StringContent(dictionary[key]), key);
                }
            }

            content = content2;
        }
        else
        {
            var json = JsonSerializer.Serialize(postData, ExtensionMethodSetting.DefaultPostJsonOption);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await CallApi<T>(url, method, content, queryParams, timeout, cancellationToken);
    }

    /// <summary>
    /// 使用表单数据发送请求
    /// </summary>
    public virtual Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? postData, int? timeout = null, CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(postData ?? new Dictionary<string, string>());
        return CallApi<T>(url, method, content, null, timeout, cancellationToken);
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    public virtual Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? postData, byte[] fileData, string filename, int? timeout = null, CancellationToken cancellationToken = default)
    {
        // 使用统一的辅助方法创建MultipartFormDataContent
        var content = MultipartHelper.CreateMultipartContent(postData, fileData, filename);
        return CallApi<T>(url, method, content, null, timeout, cancellationToken);
    }

    public abstract Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, HttpContent? content = null, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default); //where T : class;

    protected async Task<HttpRequestMessage> ApplyInterceptorsToRequestAsync(HttpRequestMessage request)
    {
        foreach (var interceptor in _interceptors)
        {
            request = await interceptor.OnRequestAsync(request);
        }
        return request;
    }

    protected async Task<HttpResponseMessage> ApplyInterceptorsToResponseAsync(HttpResponseMessage response)
    {
        foreach (var interceptor in _interceptors)
        {
            response = await interceptor.OnResponseAsync(response);
        }
        return response;
    }

    // 改进1: 优化HandleResponseMessage方法的内存管理
    protected static async Task<ApiResult<T>> HandleResponseMessage<T>(HttpResponseMessage res)
    {
        try
        {
            var rv = new ApiResult<T> { StatusCode = res.StatusCode };

            if (res.IsSuccessStatusCode)
            {
                Type type = typeof(T);
                if (type == typeof(byte[]))
                {
                    var responseBytes = await res.Content.ReadAsByteArrayAsync();

                    if (responseBytes is not T bytes)
                    {
                        throw new NullReferenceException(nameof(bytes));
                    }

                    rv.Data = bytes;
                }
                else
                {
                    var responseTxt = await res.Content.ReadAsStringAsync();
                    if (type == typeof(string))
                    {
                        if (responseTxt is not T txt)
                        {
                            throw new NullReferenceException(nameof(txt));
                        }

                        rv.Data = txt;
                    }
                    else
                    {
                        if (responseTxt.IsNull())
                        {
                            rv.Data = default!;
                        }
                        else
                        {
                            T? response = responseTxt.Deserialize<T>(ExtensionMethodSetting.DefaultJsonSerializerOptions);
                            if (response.IsNull())
                            {
                                rv.Data = default!;
                            }
                            else
                            {
                                rv.Data = response;
                            }
                        }
                    }
                }
            }
            else
            {
                // 优化特定状态码的处理方式
                switch (res.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        rv.ErrorMsg = "需要身份验证";
                        break;
                    case HttpStatusCode.NotFound:
                        rv.ErrorMsg = "资源不存在";
                        break;
                    case (HttpStatusCode)429: // TooManyRequests
                        rv.ErrorMsg = "请求过于频繁，请稍后再试";
                        break;
                    default:
                        // 原有的错误提取逻辑
                        var responseTxt = await res.Content.ReadAsStringAsync();
                        try
                        {
                            rv.Errors = responseTxt.Deserialize<ErrorObj>(ExtensionMethodSetting.DefaultJsonSerializerOptions);
                        }
                        catch { rv.ErrorMsg = responseTxt; }
                        break;
                }
            }

            return rv;
        }
        finally
        {
            // 改进2: 确保响应资源被释放(避免当T是HttpResponseMessage时释放它)
            if (typeof(T) != typeof(HttpResponseMessage))
            {
                res.Dispose();
            }
        }
    }

    // 改进3: 添加请求超时监控
    protected CancellationTokenSource CreateTimeoutTokenSource(int? timeout, CancellationToken userToken)
    {
        CancellationTokenSource timeoutSource = new();

        if (timeout.HasValue)
        {
            timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(userToken);
            timeoutSource.CancelAfter(TimeSpan.FromSeconds(timeout.Value));
        }

        return timeoutSource;
    }

    // 改进4: 提供空安全的查询参数构建方法
    protected string BuildQueryString(object? queryParams)
    {
        if (queryParams == null)
            return string.Empty;

        // 将对象转换为键值对序列
        var properties = queryParams.GetType().GetProperties()
            .Where(p => p.GetValue(queryParams) != null)
            .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(queryParams)?.ToString() ?? string.Empty)}");

        return string.Join("&", properties);
    }
}