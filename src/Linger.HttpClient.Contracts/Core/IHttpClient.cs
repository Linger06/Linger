using Linger.HttpClient.Contracts.Models;

namespace Linger.HttpClient.Contracts.Core;

/// <summary>
/// HTTP客户端接口
/// </summary>
public interface IHttpClient
{
    /// <summary>
    /// 使用 GET 方法调用 API
    /// </summary>
    /// <typeparam name="T">返回数据类型</typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="queryParams">查询参数</param>
    /// <param name="timeout">超时时间，单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API 调用结果</returns>
    Task<ApiResult<T>> CallApi<T>(string url, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 调用 API 接口
    /// </summary>
    /// <typeparam name="T">返回数据类型</typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">HTTP 方法</param>
    /// <param name="requestBody">请求体</param>
    /// <param name="queryParams">查询参数</param>
    /// <param name="timeout">超时时间，单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API 调用结果</returns>
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, object? requestBody = null, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用表单数据发送请求
    /// </summary>
    /// <typeparam name="T">返回数据类型</typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">HTTP 方法</param>
    /// <param name="formData">表单数据</param>
    /// <param name="timeout">超时时间，单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API 调用结果</returns>
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? formData, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <typeparam name="T">返回数据类型</typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">HTTP 方法</param>
    /// <param name="formData">表单数据</param>
    /// <param name="fileData">文件数据</param>
    /// <param name="filename">文件名</param>
    /// <param name="timeout">超时时间，单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API 调用结果</returns>
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? formData, byte[] fileData, string filename, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用自定义 HttpContent 调用 API
    /// </summary>
    /// <typeparam name="T">返回数据类型</typeparam>
    /// <param name="url">调用地址</param>
    /// <param name="method">HTTP 方法</param>
    /// <param name="content">请求内容</param>
    /// <param name="queryParams">查询参数</param>
    /// <param name="timeout">超时时间，单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API 调用结果</returns>
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, HttpContent? content = null, object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置授权令牌
    /// </summary>
    /// <param name="token">Bearer 令牌</param>
    void SetToken(string token);

    /// <summary>
    /// 添加请求头
    /// </summary>
    /// <param name="name">请求头名称</param>
    /// <param name="value">请求头值</param>
    void AddHeader(string name, string value);

    /// <summary>
    /// HTTP 客户端选项
    /// </summary>
    HttpClientOptions Options { get; }

    #region 流式下载

    /// <summary>
    /// 流式下载文件，返回可读取的 Stream
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="timeout">超时时间，单位秒</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>包含 Stream 的 API 结果，调用方负责释放 Stream</returns>
    /// <remarks>
    /// 使用流式模式下载，内存占用仅为缓冲区大小，适合大文件下载。
    /// 注意：必须手动释放返回的 Stream 或使用 using 语句。
    /// </remarks>
    Task<ApiResult<Stream>> DownloadStreamAsync(string url, int? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 流式下载文件并保存到指定路径
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="destinationPath">保存路径</param>
    /// <param name="timeout">超时时间，单位秒</param>
    /// <param name="bufferSize">缓冲区大小，默认 8192 字节</param>
    /// <param name="progress">进度回调（已下载字节数，总字节数或 null）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>API 结果，包含下载是否成功</returns>
    Task<ApiResult> DownloadToFileAsync(
        string url,
        string destinationPath,
        int? timeout = null,
        int bufferSize = 8192,
        IProgress<(long downloaded, long? total)>? progress = null,
        CancellationToken cancellationToken = default);

    #endregion
}