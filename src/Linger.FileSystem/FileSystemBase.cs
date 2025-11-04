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
        var message = $"{operation} failed. {(path != null ? $"Path: {path}" : string.Empty)}, Method: {callerMethod}";
        throw new FileSystemException(operation, path, message, ex);
    }

    #endregion

    #region IFileSystem 同步实现

    /// <summary>
    /// 检查文件是否存在（同步方法 - 已过时）
    /// </summary>
    /// <remarks>
    /// ⚠️ 警告: 此同步方法在远程文件系统实现中可能导致死锁。
    /// 强烈建议使用 <see cref="FileExistsAsync"/> 异步版本。
    /// </remarks>
    [Obsolete("此同步方法可能导致死锁，请使用 FileExistsAsync 异步版本", false)]
    public abstract bool FileExists(string filePath);

    /// <summary>
    /// 检查目录是否存在（同步方法 - 已过时）
    /// </summary>
    /// <remarks>
    /// ⚠️ 警告: 此同步方法在远程文件系统实现中可能导致死锁。
    /// 强烈建议使用 <see cref="DirectoryExistsAsync"/> 异步版本。
    /// </remarks>
    [Obsolete("此同步方法可能导致死锁，请使用 DirectoryExistsAsync 异步版本", false)]
    public abstract bool DirectoryExists(string directoryPath);

    /// <summary>
    /// 创建目录（同步方法 - 已过时）
    /// </summary>
    /// <remarks>
    /// ⚠️ 警告: 此同步方法在远程文件系统实现中可能导致死锁。
    /// 强烈建议使用 <see cref="CreateDirectoryIfNotExistsAsync"/> 异步版本。
    /// </remarks>
    [Obsolete("此同步方法可能导致死锁，请使用 CreateDirectoryIfNotExistsAsync 异步版本", false)]
    public abstract void CreateDirectoryIfNotExists(string directoryPath);

    /// <summary>
    /// 删除文件（同步方法 - 已过时）
    /// </summary>
    /// <remarks>
    /// ⚠️ 警告: 此同步方法在远程文件系统实现中可能导致死锁。
    /// 强烈建议使用 <see cref="DeleteFileIfExistsAsync"/> 异步版本。
    /// </remarks>
    [Obsolete("此同步方法可能导致死锁，请使用 DeleteFileIfExistsAsync 异步版本", false)]
    public abstract void DeleteFileIfExists(string filePath);

    #endregion

    #region IAsyncFileSystem 实现

    public abstract Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

    public abstract Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    public abstract Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    public abstract Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default);

    #endregion

    #region IFileSystemOperations 实现

    public abstract Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DownloadFileAsync(string remoteFilePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    #endregion
}
