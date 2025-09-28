namespace Linger.FileSystem;

/// <summary>
/// 定义统一的文件系统操作接口，适用于本地和远程文件系统
/// </summary>
public interface IFileSystemOperations : IFileSystem, IAsyncFileSystem
{
    /// <summary>
    /// 上传流到文件系统
    /// </summary>
    /// <param name="inputStream">输入流</param>
    /// <param name="filePath">目标文件路径</param>
    /// <param name="overwrite">是否覆盖</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传结果</returns>
    Task<FileOperationResult> UploadAsync(Stream inputStream, string filePath, bool overwrite = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 上传本地文件到文件系统
    /// </summary>
    /// <param name="localFilePath">本地文件路径</param>
    /// <param name="destinationPath">目标路径</param>
    /// <param name="overwrite">是否覆盖</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传结果</returns>
    Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationPath, bool overwrite = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载文件到流
    /// </summary>
    /// <param name="remoteFilePath">文件路径</param>
    /// <param name="outputStream">输出流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>下载结果</returns>
    Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载文件到本地路径
    /// </summary>
    /// <param name="remoteFilePath">文件路径</param>
    /// <param name="localDestinationPath">本地目标路径</param>
    /// <param name="overwrite">是否覆盖</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>下载结果</returns>
    Task<FileOperationResult> DownloadFileAsync(string remoteFilePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>删除结果</returns>
    Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default);
}
