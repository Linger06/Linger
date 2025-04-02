using Linger.HttpClient.Contracts.Models;

namespace Linger.HttpClient.Contracts.Core;

/// <summary>
/// HTTP客户端接口
/// </summary>
public interface IHttpClient
{
    Task<ApiResult<T>> CallApi<T>(string url, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default);
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, object? postData = null, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default);
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? postData, int? timeout = null, CancellationToken cancellationToken = default);
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? postData, byte[] fileData, string filename, int? timeout = null, CancellationToken cancellationToken = default);
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, HttpContent? content = null, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default);
    void SetToken(string token);

    // 新增的方法
    void AddHeader(string name, string value);
    HttpClientOptions Options { get; }
}