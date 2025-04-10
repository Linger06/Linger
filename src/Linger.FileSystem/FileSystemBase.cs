using System.Runtime.CompilerServices;

namespace Linger.FileSystem;

/// <summary>
/// 所有文件系统的抽象基类，统一实现公共功能
/// </summary>
public abstract class FileSystemBase(RetryOptions? retryOptions = null) : IFileSystemOperations
{
    protected readonly RetryHelper RetryHelper = new(retryOptions ?? new RetryOptions());

    /// <summary>
    /// 是否为远程文件系统
    /// </summary>
    public virtual bool IsRemoteFileSystem => false;

    #region 基础路径操作

    /// <summary>
    /// 异常处理
    /// </summary>
    protected virtual void HandleException(string operation, Exception ex, string? path = null, [CallerMemberName] string callerMethod = "")
    {
        string message = $"{operation} failed. {(path != null ? $"Path: {path}" : string.Empty)}, Method: {callerMethod}";
        throw new FileSystemException(operation, path, message, ex);
    }

    #endregion

    #region 公共属性

    #endregion

    #region IFileSystem 同步实现

    public abstract bool FileExists(string filePath);

    public abstract bool DirectoryExists(string directoryPath);

    public abstract void CreateDirectoryIfNotExists(string directoryPath);

    public abstract void DeleteFileIfExists(string filePath);

    #endregion

    #region IAsyncFileSystem 实现

    public abstract Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

    public abstract Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    public abstract Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    public abstract Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default);

    #endregion

    #region IFileSystemOperations 实现

    public abstract Task<FileOperationResult> UploadAsync(Stream inputStream, string filePath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationPath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DownloadToStreamAsync(string filePath, Stream outputStream, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DownloadFileAsync(string filePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    #endregion
}
