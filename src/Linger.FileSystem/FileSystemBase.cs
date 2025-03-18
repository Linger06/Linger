using Linger.FileSystem.Exceptions;
using Linger.Helper;

namespace Linger.FileSystem;

/// <summary>
/// 所有文件系统的抽象基类，统一实现公共功能
/// </summary>
public abstract class FileSystemBase : IFileSystemOperations
{
    protected readonly RetryHelper RetryHelper;

    protected FileSystemBase(RetryOptions? retryOptions = null)
    {
        RetryHelper = new RetryHelper(retryOptions ?? new RetryOptions());
    }

    /// <summary>
    /// 是否为远程文件系统
    /// </summary>
    public virtual bool IsRemoteFileSystem => false;

    #region 基础路径操作

    /// <summary>
    /// 标准化路径，确保使用正确的路径分隔符
    /// </summary>
    protected virtual string NormalizePath(string directoryPath, string fileName)
    {
        if (string.IsNullOrEmpty(directoryPath))
            return fileName;

        char separator = IsRemoteFileSystem ? '/' : Path.DirectorySeparatorChar;

        // 替换分隔符
        directoryPath = directoryPath.Replace('\\', separator).Replace('/', separator);

        // 确保路径以分隔符结尾
        if (!directoryPath.EndsWith(separator.ToString()))
            directoryPath += separator;

        return directoryPath + (fileName ?? string.Empty)
            .Replace('\\', separator)
            .Replace('/', separator);
    }

    /// <summary>
    /// 获取目录路径
    /// </summary>
    protected virtual string? GetDirectoryPath(string filePath)
    {
        return Path.GetDirectoryName(filePath)?.Replace('\\', IsRemoteFileSystem ? '/' : Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// 异常处理
    /// </summary>
    protected virtual void HandleException(string operation, Exception ex, string? path = null)
    {
        string message = $"{operation} failed. {(path != null ? $"Path: {path}" : string.Empty)}";
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

    public abstract Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationPath, string fileName, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationPath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DownloadToStreamAsync(string filePath, Stream outputStream, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DownloadFileAsync(string filePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    #endregion
}
