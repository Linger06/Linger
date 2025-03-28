using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Linger.HttpClient.Contracts;

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
