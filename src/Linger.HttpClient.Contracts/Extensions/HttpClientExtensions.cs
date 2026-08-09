using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Models;

namespace Linger.HttpClient.Contracts.Extensions;

/// <summary>
/// 提供常用 HTTP 方法的便捷调用。
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// 发送 GET 请求并返回分页结果。
    /// </summary>
    public static Task<ApiResult<ApiPagedResult<T>>> GetPagedAsync<T>(
        this IHttpClient client,
        string url,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        return client.CallApi<ApiPagedResult<T>>(url, HttpMethod.Get, queryParams: queryParams, headers: headers, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 发送 GET 请求。
    /// </summary>
    public static Task<ApiResult<T>> GetAsync<T>(
        this IHttpClient client,
        string url,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        return client.CallApi<T>(url, HttpMethod.Get, queryParams: queryParams, headers: headers, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 使用单次调用超时发送 GET 请求。
    /// </summary>
    /// <typeparam name="T">响应数据类型。</typeparam>
    /// <param name="client">HTTP 客户端。</param>
    /// <param name="url">请求地址。</param>
    /// <param name="queryParams">可选查询参数。</param>
    /// <param name="timeout">本次调用的超时时间；null 或无限表示不增加单次超时。</param>
    /// <param name="cancellationToken">调用方取消令牌。</param>
    /// <returns>API 调用结果。</returns>
    public static async Task<ApiResult<T>> GetWithTimeoutAsync<T>(
        this IHttpClient client,
        string url,
        object? queryParams = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(client);
#else
        if (client is null)
        {
            throw new ArgumentNullException(nameof(client));
        }
#endif

        if (timeout is null || timeout == System.Threading.Timeout.InfiniteTimeSpan)
        {
            return await client
                .GetAsync<T>(url, queryParams, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout.Value);

        try
        {
            return await client
                .GetAsync<T>(url, queryParams, cancellationToken: timeoutSource.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (
            !cancellationToken.IsCancellationRequested &&
            timeoutSource.IsCancellationRequested)
        {
            return new ApiResult<T> { ErrorMsg = "The HTTP request timed out." };
        }
    }

    /// <summary>
    /// 发送 POST 请求。
    /// </summary>
    public static Task<ApiResult<T>> PostAsync<T>(
        this IHttpClient client,
        string url,
        object requestBody,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        return client.CallApi<T>(url, HttpMethod.Post, requestBody, queryParams, headers, cancellationToken);
    }

    /// <summary>
    /// 发送 PUT 请求。
    /// </summary>
    public static Task<ApiResult<T>> PutAsync<T>(
        this IHttpClient client,
        string url,
        object requestBody,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        return client.CallApi<T>(url, HttpMethod.Put, requestBody, queryParams, headers, cancellationToken);
    }

    /// <summary>
    /// 发送 DELETE 请求。
    /// </summary>
    public static Task<ApiResult<T>> DeleteAsync<T>(
        this IHttpClient client,
        string url,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        return client.CallApi<T>(url, HttpMethod.Delete, queryParams: queryParams, headers: headers, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 发送无响应数据的 POST 请求。
    /// </summary>
    public static Task<ApiResult> PostAsync(
        this IHttpClient client,
        string url,
        object requestBody,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        return CallWithoutResultAsync(client, url, HttpMethod.Post, requestBody, queryParams, headers, cancellationToken);
    }

    /// <summary>
    /// 发送无响应数据的 PUT 请求。
    /// </summary>
    public static Task<ApiResult> PutAsync(
        this IHttpClient client,
        string url,
        object requestBody,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        return CallWithoutResultAsync(client, url, HttpMethod.Put, requestBody, queryParams, headers, cancellationToken);
    }

    /// <summary>
    /// 发送无响应数据的 DELETE 请求。
    /// </summary>
    public static Task<ApiResult> DeleteAsync(
        this IHttpClient client,
        string url,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        return CallWithoutResultAsync(client, url, HttpMethod.Delete, null, queryParams, headers, cancellationToken);
    }

    private static async Task<ApiResult> CallWithoutResultAsync(
        IHttpClient client,
        string url,
        HttpMethod method,
        object? requestBody,
        object? queryParams,
        IReadOnlyDictionary<string, string>? headers,
        CancellationToken cancellationToken)
    {
        return await client.CallApi<object>(url, method, requestBody, queryParams, headers, cancellationToken).ConfigureAwait(false);
    }
}
