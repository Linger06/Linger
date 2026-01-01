using System.Runtime.CompilerServices;
using System.Text;

namespace Linger.FileSystem;

/// <summary>
/// 所有文件系统的抽象基类，统一实现公共功能
/// </summary>
/// <remarks>
/// <para>此基类提供了文件系统操作的通用实现，包括重试机制和日志记录。</para>
/// <para>派生类可通过构造函数传入 <see cref="ILogger"/> 以启用日志记录。</para>
/// </remarks>
public abstract class FileSystemBase : IFileSystemOperations
{
    /// <summary>
    /// 重试助手，用于在操作失败时自动重试
    /// </summary>
    protected readonly RetryHelper RetryHelper;

    /// <summary>
    /// 日志记录器（可选）
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// 初始化 <see cref="FileSystemBase"/> 的新实例。
    /// </summary>
    /// <param name="retryOptions">重试选项，为 <c>null</c> 时使用默认配置。</param>
    /// <param name="logger">日志记录器，为 <c>null</c> 时使用 <see cref="NullLogger"/>。</param>
    protected FileSystemBase(RetryOptions? retryOptions = null, ILogger? logger = null)
    {
        RetryHelper = new RetryHelper(retryOptions ?? new RetryOptions());
        Logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// 是否为远程文件系统
    /// </summary>
    public virtual bool IsRemoteFileSystem => false;

    #region 基础路径操作

    /// <summary>
    /// 异常处理并记录日志
    /// </summary>
    protected virtual void HandleException(string operation, Exception ex, string? path = null, [CallerMemberName] string callerMethod = "")
    {
        var message = $"{operation} failed. {(path is not null ? $"Path: {path}" : string.Empty)}, Method: {callerMethod}";
        Logger.LogError(ex, "{Message}", message);
        throw new FileSystemException(operation, path, message, ex);
    }

    /// <summary>
    /// 记录调试级别日志
    /// </summary>
    protected void LogDebug(string message, params object[] args)
    {
        Logger.LogDebug(message, args);
    }

    /// <summary>
    /// 记录信息级别日志
    /// </summary>
    protected void LogInformation(string message, params object[] args)
    {
        Logger.LogInformation(message, args);
    }

    /// <summary>
    /// 记录警告级别日志
    /// </summary>
    protected void LogWarning(string message, params object[] args)
    {
        Logger.LogWarning(message, args);
    }

    #endregion

    #region IAsyncFileSystem 实现

    public abstract Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

    public abstract Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    public abstract Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    public abstract Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default);

    #endregion

    #region IFileSystemOperations 实现

    public abstract Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default);

    public abstract Task<Stream> OpenWriteAsync(string filePath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<StreamReader> GetReaderAsync(string filePath, Encoding? encoding = null, CancellationToken cancellationToken = default);

    public abstract Task<StreamWriter> GetWriterAsync(string filePath, bool overwrite = false, Encoding? encoding = null, CancellationToken cancellationToken = default);

    public abstract Task<bool> IsDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    public abstract Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DownloadFileAsync(string remoteFilePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    #endregion
}
