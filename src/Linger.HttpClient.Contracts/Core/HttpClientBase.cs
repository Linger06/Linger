using System.Text.Json;
using Linger.Extensions;
using Linger.Extensions.Core;
using Linger.HttpClient.Contracts.Helpers;
using Linger.HttpClient.Contracts.Models;

namespace Linger.HttpClient.Contracts.Core;

/// <summary>
/// HTTP客户端抽象基类
/// </summary>
public abstract class HttpClientBase : IHttpClient
{
    /// <summary>
    /// HTTP客户端选项
    /// </summary>
    public HttpClientOptions Options { get; } = new();

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
    /// 使用Get方法调用api
    /// </summary>
    /// <param name="url">调用地址</param>
    /// <param name="queryParams">查询参数</param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <typeparam name="T">返回数据类型</typeparam>
    /// <returns>API调用结果</returns>
    public virtual Task<ApiResult<T>> CallApi<T>(string url, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default)
    {
        return CallApi<T>(url, HttpMethodEnum.Get, null, queryParams, timeout, cancellationToken);
    }

    /// <summary>
    /// 调用API接口
    /// </summary>
    /// <typeparam name="T">返回数据类型</typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">HTTP方法</param>
    /// <param name="postData">提交的数据，会被转换为HttpContent</param>
    /// <param name="queryParams">查询参数</param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API调用结果</returns>
    public virtual async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, object? postData = null, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default)
    {
        HttpContent? content = null;

        if (postData is not null)
        {
            content = CreateHttpContent(postData);
        }

        return await CallApi<T>(url, method, content, queryParams, timeout, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 根据数据类型创建HttpContent
    /// </summary>
    /// <param name="postData">要转换的数据</param>
    /// <returns>HttpContent实例</returns>
    protected virtual HttpContent CreateHttpContent(object postData)
    {
        return postData switch
        {
            IDictionary<string, string> dictionary => new FormUrlEncodedContent(dictionary),
            HttpContent httpContent => httpContent,
            _ => new StringContent(
                JsonSerializer.Serialize(postData, ExtensionMethodSetting.DefaultPostJsonOption),
                Encoding.UTF8,
                "application/json")
        };
    }

    /// <summary>
    /// 使用表单数据发送请求
    /// </summary>
    /// <typeparam name="T">返回数据类型</typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">HTTP方法</param>
    /// <param name="postData">表单数据</param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API调用结果</returns>
    public virtual Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? postData, int? timeout = null, CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(postData ?? new Dictionary<string, string>());
        return CallApi<T>(url, method, content, null, timeout, cancellationToken);
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <typeparam name="T">返回数据类型</typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">HTTP方法</param>
    /// <param name="postData">表单数据</param>
    /// <param name="fileData">文件数据</param>
    /// <param name="filename">文件名</param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API调用结果</returns>
    public virtual Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? postData, byte[] fileData, string filename, int? timeout = null, CancellationToken cancellationToken = default)
    {
        // 使用统一的辅助方法创建MultipartFormDataContent
        var content = MultipartHelper.CreateMultipartContent(postData, fileData, filename);
        return CallApi<T>(url, method, content, null, timeout, cancellationToken);
    }

    /// <summary>
    /// 核心API调用方法
    /// </summary>
    /// <typeparam name="T">返回数据类型</typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">HTTP方法</param>
    /// <param name="content">请求内容</param>
    /// <param name="queryParams">查询参数</param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API调用结果</returns>
    public abstract Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, HttpContent? content = null, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 处理HTTP响应消息并将其转换为ApiResult
    /// </summary>
    /// <typeparam name="T">期望的响应数据类型</typeparam>
    /// <param name="res">HTTP响应消息</param>
    /// <returns>包含响应数据的ApiResult</returns>
    protected virtual async Task<ApiResult<T>> HandleResponseMessage<T>(HttpResponseMessage res)
    {
        try
        {
            var result = new ApiResult<T> { StatusCode = res.StatusCode };

            if (res.IsSuccessStatusCode)
            {
                result.Data = await DeserializeResponseContent<T>(res).ConfigureAwait(false);
            }
            else
            {
                (result.ErrorMsg, result.Errors) = await GetErrorMessageAsync(res).ConfigureAwait(false);
            }

            return result;
        }
        finally
        {
            // 确保响应资源被释放(避免当T是HttpResponseMessage时释放它)
            if (typeof(T) != typeof(HttpResponseMessage))
            {
                res.Dispose();
            }
        }
    }

    /// <summary>
    /// 反序列化响应内容为指定类型
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="response">HTTP响应消息</param>
    /// <returns>反序列化后的数据</returns>
    protected virtual async Task<T> DeserializeResponseContent<T>(HttpResponseMessage response)
    {
        var targetType = typeof(T);

        // 处理字节数组类型
        if (targetType == typeof(byte[]))
        {
            var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            return (T)(object)bytes;
        }

        // 读取响应文本
        var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        // 处理字符串类型
        if (targetType == typeof(string))
        {
            return (T)(object)responseText;
        }

        // 处理空或空白响应
        if (string.IsNullOrWhiteSpace(responseText))
        {
            return default!;
        }

        // 处理空JSON对象
        if (responseText == "{}")
        {
            return targetType switch
            {
                { IsValueType: true } => default!,
                not null when targetType == typeof(object) => (T)(object)new { },
                _ => TryCreateDefaultInstance<T>()
            };
        }

        // 尝试JSON反序列化
        try
        {
            var result = responseText.Deserialize<T>(ExtensionMethodSetting.DefaultJsonSerializerOptions);
            return result ?? default!;
        }
        catch (JsonException)
        {
            // 反序列化失败时，对object类型返回原始字符串，其他类型返回默认值
            return targetType == typeof(object) ? (T)(object)responseText : default!;
        }
    }

    /// <summary>
    /// 安全地尝试创建类型的默认实例
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <returns>创建的实例或默认值</returns>
    private static T TryCreateDefaultInstance<T>()
    {
        try
        {
            return Activator.CreateInstance<T>();
        }
        catch
        {
            return default!;
        }
    }

    protected virtual async Task<(string ErrorMsg, IEnumerable<Error> Errors)> GetErrorMessageAsync(HttpResponseMessage res)
    {
        // 优化特定状态码的处理方式
        var statusMessage = res.StatusCode switch
        {
            HttpStatusCode.BadRequest => "Bad request",
            HttpStatusCode.Unauthorized => "Authentication required",
            HttpStatusCode.Forbidden => "Access forbidden",
            HttpStatusCode.NotFound => "Resource not found",
            HttpStatusCode.MethodNotAllowed => "Method not allowed",
            HttpStatusCode.Conflict => "Request conflict",
            (HttpStatusCode)422 => "Validation failed", // Unprocessable Entity
            (HttpStatusCode)429 => "Too many requests, please try again later",
            HttpStatusCode.InternalServerError => "Internal server error",
            HttpStatusCode.BadGateway => "Bad gateway",
            HttpStatusCode.ServiceUnavailable => "Service unavailable",
            HttpStatusCode.GatewayTimeout => "Gateway timeout",
            _ => null
        };

        // 如果是已知状态码，直接返回预定义消息
        if (statusMessage is not null)
        {
            return (statusMessage, []);
        }

        // 尝试从响应内容中提取详细错误信息
        try
        {
            var responseTxt = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            // 如果响应内容为空，返回通用错误消息
            if (string.IsNullOrWhiteSpace(responseTxt))
            {
                return ($"HTTP {(int)res.StatusCode}: {res.ReasonPhrase}", []);
            }

            // 尝试反序列化为错误集合
            try
            {
                var errors = responseTxt.Deserialize<IEnumerable<Error>>(ExtensionMethodSetting.DefaultJsonSerializerOptions);
                if (errors.IsNotNull() && errors.Any())
                {
                    var firstError = errors.FirstOrDefault();
                    var errorMsg = firstError?.Message ?? "An unknown error occurred";
                    return (errorMsg, errors);
                }
            }
            catch (JsonException)
            {
                // 反序列化失败，可能不是标准的错误格式
                // 继续使用原始响应文本
            }

            return (responseTxt, []);
        }
        catch (Exception ex)
        {
            // 读取响应内容失败
            return ($"HTTP {(int)res.StatusCode}: {res.ReasonPhrase} (Failed to read response: {ex.Message})", []);
        }
    }

    /// <summary>
    /// 创建超时取消令牌源
    /// </summary>
    /// <param name="timeout">超时时间(秒)</param>
    /// <param name="userToken">用户提供的取消令牌</param>
    /// <returns>取消令牌源</returns>
    protected static CancellationTokenSource CreateTimeoutTokenSource(int? timeout, CancellationToken userToken)
    {
        if (!timeout.HasValue)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(userToken);
        }

        var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(userToken);
        timeoutSource.CancelAfter(TimeSpan.FromSeconds(timeout.Value));
        return timeoutSource;
    }

    /// <summary>
    /// 构建查询参数字符串
    /// </summary>
    /// <param name="queryParams">查询参数对象</param>
    /// <returns>查询参数字符串</returns>
    protected static string BuildQueryString(object? queryParams)
    {
        if (queryParams is null)
        {
            return string.Empty;
        }

        // 将对象转换为键值对序列
        var properties = queryParams.GetType().GetProperties()
            .Where(p => p.GetValue(queryParams) is not null)
            .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(queryParams)?.ToString() ?? string.Empty)}");

        return string.Join("&", properties);
    }
}
