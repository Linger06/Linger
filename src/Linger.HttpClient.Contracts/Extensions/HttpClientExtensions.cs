using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Models;

namespace Linger.HttpClient.Contracts.Extensions;

/// <summary>
/// HttpClient扩展方法
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// 发送GET请求并返回分页结果
    /// </summary>
    public static async Task<ApiResult<ApiPagedResult<T>>> GetPagedAsync<T>(
        this IHttpClient client,
        string url,
        object? queryParams = null,
        int? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return await client.CallApi<ApiPagedResult<T>>(url, queryParams, timeout, cancellationToken);
    }

    /// <summary>
    /// 发送GET请求
    /// </summary>
    public static async Task<ApiResult<T>> GetAsync<T>(
        this IHttpClient client,
        string url,
        object? queryParams = null,
        int? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return await client.CallApi<T>(url, queryParams, timeout, cancellationToken);
    }

    /// <summary>
    /// 使用查询参数发送GET请求并自动处理超时
    /// </summary>
    public static async Task<ApiResult<T>> GetWithTimeoutAsync<T>(
        this IHttpClient client,
        string url,
        object? queryParams = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        // 将TimeSpan转换为秒数
        int? timeoutSeconds = timeout.HasValue ? (int)timeout.Value.TotalSeconds : null;

        // 使用基础方法发送请求
        return await client.CallApi<T>(url, queryParams, timeoutSeconds, cancellationToken);
    }

    /// <summary>
    /// 发送POST请求
    /// </summary>
    public static async Task<ApiResult<T>> PostAsync<T>(
        this IHttpClient client,
        string url,
        object postData,
        object? queryParams = null,
        int? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return await client.CallApi<T>(url, HttpMethodEnum.Post, postData, queryParams, timeout, cancellationToken);
    }

    /// <summary>
    /// 发送PUT请求
    /// </summary>
    public static async Task<ApiResult<T>> PutAsync<T>(
        this IHttpClient client,
        string url,
        object postData,
        object? queryParams = null,
        int? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return await client.CallApi<T>(url, HttpMethodEnum.Put, postData, queryParams, timeout, cancellationToken);
    }

    /// <summary>
    /// 发送DELETE请求
    /// </summary>
    public static async Task<ApiResult<T>> DeleteAsync<T>(
        this IHttpClient client,
        string url,
        object? queryParams = null,
        int? timeout = null,
        CancellationToken cancellationToken = default)
    {
        return await client.CallApi<T>(url, HttpMethodEnum.Delete, null, queryParams, timeout, cancellationToken);
    }
}