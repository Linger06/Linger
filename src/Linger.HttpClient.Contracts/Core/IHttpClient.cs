using Linger.HttpClient.Contracts.Models;

namespace Linger.HttpClient.Contracts.Core;

/// <summary>
/// 定义类型化 HTTP API 调用。
/// </summary>
public interface IHttpClient
{
    /// <summary>
    /// 发送 HTTP 请求并返回由调用方管理生命周期的原始响应。
    /// </summary>
    /// <param name="url">请求地址。</param>
    /// <param name="method">HTTP 方法。</param>
    /// <param name="requestBody">可选请求体；<see cref="HttpContent"/> 直接发送，字典按表单发送，其他对象按 JSON 发送。客户端负责释放传入的 <see cref="HttpContent"/>。</param>
    /// <param name="queryParams">可选查询参数。</param>
    /// <param name="headers">仅应用于本次请求的请求头。</param>
    /// <param name="completionOption">响应完成条件；流式读取应使用 <see cref="HttpCompletionOption.ResponseHeadersRead"/>。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>原始 HTTP 响应。调用方必须释放返回值。</returns>
    /// <example>
    /// <code>
    /// using var response = await client.SendAsync("events", HttpMethod.Get, cancellationToken: cancellationToken);
    /// response.EnsureSuccessStatusCode();
    /// using var stream = await response.Content.ReadAsStreamAsync();
    /// </code>
    /// </example>
    /// <remarks>
    /// 此方法不把非成功状态码或传输异常转换为 <see cref="ApiResult"/>；需要统一错误模型时应使用 <see cref="CallApi{T}"/>。
    /// </remarks>
    Task<HttpResponseMessage> SendAsync(
        string url,
        HttpMethod method,
        object? requestBody = null,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 调用 HTTP API 并将响应反序列化为指定类型。
    /// </summary>
    /// <typeparam name="T">响应数据类型。</typeparam>
    /// <param name="url">请求地址。</param>
    /// <param name="method">HTTP 方法。</param>
    /// <param name="requestBody">可选请求体；<see cref="HttpContent"/> 直接发送，字典按表单发送，其他对象按 JSON 发送。客户端负责释放传入的 <see cref="HttpContent"/>。</param>
    /// <param name="queryParams">可选查询参数。</param>
    /// <param name="headers">仅应用于本次请求的请求头。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>API 调用结果。</returns>
    Task<ApiResult<T>> CallApi<T>(
        string url,
        HttpMethod method,
        object? requestBody = null,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 以流式方式上传单个文件。
    /// </summary>
    /// <typeparam name="T">响应数据类型。</typeparam>
    /// <param name="url">请求地址。</param>
    /// <param name="method">HTTP 方法。</param>
    /// <param name="fileStream">文件流；请求完成后由客户端释放。</param>
    /// <param name="fileName">文件名。</param>
    /// <param name="formData">可选表单字段。</param>
    /// <param name="fileFieldName">文件字段名。</param>
    /// <param name="contentType">文件内容类型；未指定时根据扩展名推断。</param>
    /// <param name="headers">仅应用于本次请求的请求头。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>API 调用结果。</returns>
    Task<ApiResult<T>> UploadFileAsync<T>(
        string url,
        HttpMethod method,
        Stream fileStream,
        string fileName,
        IReadOnlyDictionary<string, string>? formData = null,
        string fileFieldName = "file",
        string? contentType = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 以流式方式下载文件，并在下载成功后替换目标文件。
    /// </summary>
    /// <param name="url">下载地址。</param>
    /// <param name="destinationPath">目标文件路径。</param>
    /// <param name="bufferSize">复制缓冲区大小。</param>
    /// <param name="progress">下载进度。</param>
    /// <param name="headers">仅应用于本次请求的请求头。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>API 调用结果。</returns>
    Task<ApiResult> DownloadToFileAsync(
        string url,
        string destinationPath,
        int bufferSize = 8192,
        IProgress<(long downloaded, long? total)>? progress = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);
}
