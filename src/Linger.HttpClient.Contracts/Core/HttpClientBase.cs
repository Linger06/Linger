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
    /// <param name="requestBody">
    /// 请求体（body）。
    /// 当为 <see cref="HttpContent"/> 时将直接使用；
    /// 当为 <see cref="IDictionary{TKey, TValue}"/>（如 <see cref="IDictionary{String, String}"/>）时将作为表单（<see cref="FormUrlEncodedContent"/>）发送；
    /// 其他类型将被序列化为 JSON（使用 <see cref="ExtensionMethodSetting.DefaultRequestJsonOptions"/>）。
    /// 一般用于 POST/PUT/PATCH 等含有请求体的方法；对 GET 调用会被忽略。
    /// </param>
    /// <param name="queryParams">查询参数</param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API调用结果</returns>
    public virtual async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, object? requestBody = null, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default)
    {
        HttpContent? content = null;

        if (requestBody is not null)
        {
            content = CreateHttpContent(requestBody);
        }

        return await CallApi<T>(url, method, content, queryParams, timeout, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 根据数据类型创建HttpContent
    /// </summary>
    /// <param name="requestBody">请求体对象（payload），根据其类型转换为合适的 <see cref="HttpContent"/>。</param>
    /// <returns>HttpContent实例</returns>
    protected virtual HttpContent CreateHttpContent(object requestBody)
    {
        return requestBody switch
        {
            IDictionary<string, string> dictionary => new FormUrlEncodedContent(dictionary),
            HttpContent httpContent => httpContent,
            _ => new StringContent(
                JsonSerializer.Serialize(requestBody, ExtensionMethodSetting.DefaultRequestJsonOptions),
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
    /// <param name="formData">表单数据</param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API调用结果</returns>
    public virtual Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? formData, int? timeout = null, CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(formData ?? new Dictionary<string, string>());
        return CallApi<T>(url, method, content, null, timeout, cancellationToken);
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <typeparam name="T">返回数据类型</typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">HTTP方法</param>
    /// <param name="formData">表单数据</param>
    /// <param name="fileData">文件数据</param>
    /// <param name="filename">文件名</param>
    /// <param name="timeout">超时时间,单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API调用结果</returns>
    public virtual Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? formData, byte[] fileData, string filename, int? timeout = null, CancellationToken cancellationToken = default)
    {
        // 使用统一的辅助方法创建MultipartFormDataContent
        var content = MultipartHelper.CreateMultipartContent(formData, fileData, filename);
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
            if (targetType.IsValueType)
            {
                return default!;
            }

            if (targetType == typeof(object))
            {
                return (T)(object)new { };
            }

            // 安全地尝试创建默认实例
            try
            {
                return Activator.CreateInstance<T>();
            }
            catch
            {
                return default!;
            }
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

    protected virtual async Task<(string ErrorMsg, IEnumerable<Error> Errors)> GetErrorMessageAsync(HttpResponseMessage res)
    {
        try
        {
            var responseTxt = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            // 如果响应内容为空，返回通用错误消息
            if (string.IsNullOrWhiteSpace(responseTxt))
            {
                return (GetStatusCodeMessage(res.StatusCode) ?? $"HTTP {(int)res.StatusCode}: {res.ReasonPhrase}", []);
            }

            // 尝试解析 ProblemDetails 格式
            try
            {
                var problemDetails = responseTxt.Deserialize<ProblemDetailsWithErrors>(ExtensionMethodSetting.DefaultJsonSerializerOptions);
                if (problemDetails is not null && problemDetails.Errors.Count > 0)
                {
                    var errors = problemDetails.Errors.Select(kvp => new Error(kvp.Key, kvp.Value)).ToList();
                    // 将所有字段错误消息合并为全局错误消息；若 key 非空，包含在消息中："key: value"
                    var mergedMsg = string.Join("\n", problemDetails.Errors
                        .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                        .Select(kvp => string.IsNullOrWhiteSpace(kvp.Key) ? kvp.Value : $"{kvp.Key}: {kvp.Value}"));
                    var errorMsg = !string.IsNullOrWhiteSpace(mergedMsg)
                        ? mergedMsg
                        : (!string.IsNullOrWhiteSpace(problemDetails.Title) ? problemDetails.Title : "An unknown error occurred");
                    return (errorMsg, errors);
                }
            }
            catch (JsonException)
            {
                // ProblemDetails 解析失败，继续尝试其他格式
            }

            // 尝试解析直接的错误集合格式
            try
            {
                var errorList = responseTxt.Deserialize<IEnumerable<Error>>(ExtensionMethodSetting.DefaultJsonSerializerOptions);
                if (errorList is not null && errorList.Any())
                {
                    // 合并所有错误项的消息为全局错误消息；若 Code 非空，包含在消息中："Code: Message"
                    var mergedMsg = string.Join("\n", errorList
                        .Where(e => !string.IsNullOrWhiteSpace(e?.Message))
                        .Select(e => string.IsNullOrWhiteSpace(e!.Code) ? e!.Message : $"{e.Code}: {e.Message}"));
                    var errorMsg = !string.IsNullOrWhiteSpace(mergedMsg) ? mergedMsg : "An unknown error occurred";
                    return (errorMsg, errorList);
                }
            }
            catch (JsonException)
            {
                // Error 集合解析失败，继续使用其他方式
            }

            // 如果无法解析为结构化错误，尝试返回状态码对应的消息
            var statusMessage = GetStatusCodeMessage(res.StatusCode);
            if (statusMessage is not null)
            {
                return (statusMessage, []);
            }

            // 最后返回原始响应文本
            return (responseTxt, []);
        }
        catch (Exception ex)
        {
            // 读取响应内容失败
            return ($"HTTP {(int)res.StatusCode}: {res.ReasonPhrase} (Failed to read response: {ex.Message})", []);
        }
    }

    /// <summary>
    /// 获取状态码对应的友好错误消息
    /// </summary>
    /// <param name="statusCode">HTTP状态码</param>
    /// <returns>友好的错误消息</returns>
    private static string? GetStatusCodeMessage(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "Bad request", // 请求参数错误
        HttpStatusCode.Unauthorized => "Authentication required", // 身份验证失败
        HttpStatusCode.Forbidden => "Access forbidden", // 访问被拒绝
        HttpStatusCode.NotFound => "Resource not found", // 请求的资源不存在
        HttpStatusCode.MethodNotAllowed => "Method not allowed", // 请求方法不被允许
        HttpStatusCode.Conflict => "Request conflict", // 请求冲突
        (HttpStatusCode)422 => "Validation failed", // 数据验证失败 (Unprocessable Entity)
        (HttpStatusCode)429 => "Too many requests, please try again later", // 请求过于频繁，请稍后重试
        HttpStatusCode.InternalServerError => "Internal server error", // 服务器内部错误
        HttpStatusCode.BadGateway => "Bad gateway", // 网关错误
        HttpStatusCode.ServiceUnavailable => "Service unavailable", // 服务暂时不可用
        HttpStatusCode.GatewayTimeout => "Gateway timeout", // 网关超时
        _ => null
    };

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
