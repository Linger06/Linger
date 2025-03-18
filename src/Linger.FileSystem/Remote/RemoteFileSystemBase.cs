using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Linger.Helper;

namespace Linger.FileSystem.Remote;

/// <summary>
/// 远程文件系统的通用基类，提供共享功能和实现
/// 注意：此类是为了未来的扩展设计的，现有FtpContext和SftpContext暂不需要修改为继承此类
/// </summary>
public abstract class RemoteFileSystemBase : IRemoteFileSystemContext, IFileSystemOperations
{
    protected readonly RetryHelper RetryHelper;
    
    protected RemoteFileSystemBase(RetryOptions? retryOptions = null)
    {
        RetryHelper = new RetryHelper(retryOptions ?? new RetryOptions());
    }

    #region IRemoteFileSystemContext 实现
    public abstract bool IsConnected();
    public abstract void Connect();
    public abstract void Disconnect();
    public abstract void Dispose();
    public abstract string ServerDetails();
    #endregion

    #region IFileSystem 实现
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
    
    /// <summary>
    /// 标准化路径，确保使用正确的路径分隔符
    /// </summary>
    protected virtual string NormalizePath(string directoryPath, string fileName)
    {
        if (string.IsNullOrEmpty(directoryPath))
            return fileName;
            
        // 替换Windows路径分隔符为Unix路径分隔符
        directoryPath = directoryPath.Replace("\\", "/");
        
        // 确保路径以/结尾
        if (!directoryPath.EndsWith("/"))
            directoryPath += "/";
            
        return directoryPath + fileName;
    }
    
    /// <summary>
    /// 统一的异常处理方法
    /// </summary>
    protected abstract void HandleException(string operation, Exception ex, string? path = null);
}
