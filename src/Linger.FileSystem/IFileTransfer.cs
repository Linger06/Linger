namespace Linger.FileSystem;

/// <summary>
/// 定义本地和远程文件系统共用的异步内容传输操作。
/// </summary>
public interface IFileTransfer
{
    /// <summary>异步将输入流内容写入目标文件。</summary>
    /// <param name="inputStream">要读取的输入流。</param>
    /// <param name="destinationFilePath">目标文件路径。</param>
    /// <param name="overwrite">是否覆盖已有文件。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>文件操作结果。</returns>
    Task<FileOperationResult> UploadAsync(
        Stream inputStream,
        string destinationFilePath,
        bool overwrite = false,
        CancellationToken cancellationToken = default);

    /// <summary>异步将本地文件内容写入目标文件。</summary>
    /// <param name="localFilePath">本地源文件路径。</param>
    /// <param name="destinationFilePath">目标文件路径。</param>
    /// <param name="overwrite">是否覆盖已有文件。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>文件操作结果。</returns>
    Task<FileOperationResult> UploadFileAsync(
        string localFilePath,
        string destinationFilePath,
        bool overwrite = false,
        CancellationToken cancellationToken = default);

    /// <summary>异步将文件内容复制到输出流。</summary>
    /// <param name="sourceFilePath">源文件路径。</param>
    /// <param name="outputStream">要写入的输出流。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>文件操作结果。</returns>
    Task<FileOperationResult> DownloadToStreamAsync(
        string sourceFilePath,
        Stream outputStream,
        CancellationToken cancellationToken = default);

    /// <summary>异步将文件内容复制到本地目标路径。</summary>
    /// <param name="sourceFilePath">源文件路径。</param>
    /// <param name="localDestinationPath">本地目标路径。</param>
    /// <param name="overwrite">是否覆盖已有文件。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>文件操作结果。</returns>
    Task<FileOperationResult> DownloadFileAsync(
        string sourceFilePath,
        string localDestinationPath,
        bool overwrite = false,
        CancellationToken cancellationToken = default);
}
