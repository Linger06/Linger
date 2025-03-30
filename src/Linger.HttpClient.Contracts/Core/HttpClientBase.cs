using System.Net;
using System.Text;
using System.Text.Json;
using Linger.Extensions;
using Linger.Extensions.Core;
using Linger.HttpClient.Contracts.Models;
using Linger.HttpClient.Contracts.Metrics;
#if NETFRAMEWORK
using System.Net.Http;
#endif

namespace Linger.HttpClient.Contracts.Core;

/// <summary>
/// HTTP客户端基类，提供所有HTTP客户端实现的共享基础功能
/// </summary>
public abstract class HttpClientBase : IHttpClient
{
    protected readonly List<IHttpClientInterceptor> Interceptors = new();
    public HttpClientOptions Options { get; } = new HttpClientOptions();

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

    public virtual async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? postData, int? timeout = null, CancellationToken cancellationToken = default) //where T : class
    {
        HttpContent? content = null;
        //填充表单数据
        if (postData == null || postData.Count == 0)
        {
            return await CallApi<T>(url, method, content, null, timeout, cancellationToken);
        }

        var paras = postData.Select(data => new KeyValuePair<string, string>(data.Key, data.Value)).ToList();

        if (paras.Count != 0)
        {
            url = url.AppendQuery(paras);
        }

        content = new FormUrlEncodedContent(paras);

        return await CallApi<T>(url, method, content, null, timeout, cancellationToken);
    }

    /// <summary>
    ///     提交表单
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="method"></param>
    /// <param name="postData"></param>
    /// <param name="fileData"></param>
    /// <param name="filename"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? postData, byte[] fileData, string filename, int? timeout = null, CancellationToken cancellationToken = default) //where T : class
    {
        var content = new MultipartFormDataContent();
        //填充表单数据
        if (!(postData == null || postData.Count == 0))
        {
            foreach (var key in postData.Keys)
            {
                content.Add(new StringContent(postData[key]), key);
            }
        }

        content.Add(new ByteArrayContent(fileData), "File", filename);

        return await CallApi<T>(url, method, content, null, timeout, cancellationToken).ConfigureAwait(false);
    }

    public abstract Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, HttpContent? content = null, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default); //where T : class;
    public abstract void SetToken(string token);
    public void AddHeader(string name, string value)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        
        Options.DefaultHeaders[name] = value;
    }

    public void AddInterceptor(IHttpClientInterceptor interceptor)
    {
        if (interceptor == null)
        {
            throw new ArgumentNullException(nameof(interceptor));
        }
        
        Interceptors.Add(interceptor);
    }

    protected async Task<HttpRequestMessage> ApplyInterceptorsToRequestAsync(HttpRequestMessage request)
    {
        if (Interceptors.Count == 0)
        {
            return request;
        }

        var currentRequest = request;
        foreach (var interceptor in Interceptors)
        {
            currentRequest = await interceptor.OnRequestAsync(currentRequest);
        }
        
        return currentRequest;
    }

    protected async Task<HttpResponseMessage> ApplyInterceptorsToResponseAsync(HttpResponseMessage response)
    {
        if (Interceptors.Count == 0)
        {
            return response;
        }

        var currentResponse = response;
        foreach (var interceptor in Interceptors)
        {
            currentResponse = await interceptor.OnResponseAsync(currentResponse);
        }
        
        return currentResponse;
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