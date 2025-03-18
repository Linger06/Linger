using FluentFTP;
using FluentFTP.Exceptions;
using Linger.FileSystem.Remote;
using Linger.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Linger.FileSystem.Ftp;

public abstract class FtpContext(RetryOptions? retryOptions = null) : IRemoteFileSystemContext, IFileSystemOperations
{
    private readonly RetryHelper _retryHelper = new(retryOptions ?? new RetryOptions());
    protected IFtpClient FtpClient { get; set; } = default!;

    protected virtual void HandleException(string operation, Exception ex, string? path = null)
    {
        // 统一的异常处理,可以在派生类中重写实现具体的日志记录
        string message = $"{operation} failed. {(path != null ? $"Path: {path}" : string.Empty)}";
        throw new FtpException(message, ex);
    }

    public class FtpConnectionScope : IDisposable
    {
        private readonly FtpContext _context;
        private readonly bool _wasConnected;

        public FtpConnectionScope(FtpContext context)
        {
            _context = context;
            _wasConnected = _context.IsConnected();
            if (!_wasConnected)
                _context.Connect();
        }

        public void Dispose()
        {
            if (!_wasConnected)
                _context.Disconnect();
        }
    }

    protected FtpConnectionScope CreateConnectionScope()
    {
        return new FtpConnectionScope(this);
    }

    public bool IsConnected() => FtpClient.IsConnected;

    public void Connect()
    {
        if (!IsConnected())
            FtpClient.Connect();
    }

    public void Disconnect()
    {
        if (FtpClient.IsConnected)
            FtpClient.Disconnect();
    }

    public void Dispose()
    {
        if (!FtpClient.IsDisposed)
            FtpClient.Dispose();
    }

    public bool FileExists(string filePath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return FtpClient.FileExists(filePath);
        }
        catch (Exception ex)
        {
            HandleException("Check file exists", ex, filePath);
            return false;
        }
    }

    public void DeleteFileIfExists(string filePath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (FileExists(filePath))
                FtpClient.DeleteFile(filePath);
        }
        catch (Exception ex)
        {
            HandleException("Delete file", ex, filePath);
        }
    }

    public bool DirectoryExists(string directoryPath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return FtpClient.DirectoryExists(directoryPath);
        }
        catch (Exception ex)
        {
            HandleException("Check directory exists", ex, directoryPath);
            return false;
        }
    }

    public void CreateDirectoryIfNotExists(string directoryPath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!DirectoryExists(directoryPath))
                _ = FtpClient.CreateDirectory(directoryPath);
        }
        catch (Exception ex)
        {
            HandleException("Create directory", ex, directoryPath);
        }
    }

    public void SetWorkingDirectory(string directoryPath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!string.IsNullOrWhiteSpace(directoryPath) && DirectoryExists(directoryPath))
                FtpClient.SetWorkingDirectory(directoryPath);
        }
        catch (Exception ex)
        {
            HandleException("Set working directory", ex, directoryPath);
        }
    }

    public void SetRootAsWorkingDirectory()
    {
        SetWorkingDirectory(string.Empty);
    }

    public void UploadFile(string localFilePath, string remoteFilePath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            _retryHelper.ExecuteAsync(
                            () => Task.Run(() =>
                            {
                                if (!File.Exists(localFilePath))
                                    throw new FileNotFoundException("Local file not found", localFilePath);

                                _ = FtpClient.UploadFile(localFilePath, remoteFilePath);
                                return true;
                            }),
                            "Upload file").GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Local: {localFilePath}, Remote: {remoteFilePath}");
        }
    }

    public bool DownloadFile(string localDic, string remotePath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return _retryHelper.ExecuteAsync(
                () => Task.Run(() =>
                {
                    if (!Directory.Exists(localDic))
                        Directory.CreateDirectory(localDic);

                    string localPath = Path.Combine(localDic, Path.GetFileName(remotePath));
                    return FtpClient.DownloadFile(localPath, remotePath) == FtpStatus.Success;
                }),
                "Download file").GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            HandleException("Download file", ex, remotePath);
            return false;
        }
    }

    public DateTime GetLastModifiedTime(string remotePath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return FtpClient.GetModifiedTime(remotePath);
        }
        catch (Exception ex)
        {
            HandleException("Get last modified time", ex, remotePath);
            return DateTime.MinValue;
        }
    }

    public List<string> ListDirectory(string? remoteDic = null, FtpObjectType type = FtpObjectType.File)
    {
        using var scope = CreateConnectionScope();
        try
        {
            FtpListItem[] files = remoteDic == null ?
                FtpClient.GetListing() :
                FtpClient.GetListing(remoteDic);

            return files.Where(f => f.Type == type)
                       .Select(f => f.Name)
                       .ToList();
        }
        catch (Exception ex)
        {
            HandleException("List directory", ex, remoteDic);
            return [];
        }
    }

    public int UploadFiles(IEnumerable<string> localFiles, string remoteDic, FtpRemoteExists remoteExistsMode = FtpRemoteExists.Overwrite)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return _retryHelper.ExecuteAsync(
                () => Task.Run(() =>
                {
                    var listFiles = localFiles.Where(File.Exists)
                                            .Select(f => new FileInfo(f))
                                            .ToList();

                    if (listFiles.Count == 0)
                        return 0;

                    List<FtpResult>? results = FtpClient.UploadFiles(listFiles, remoteDic, remoteExistsMode);
                    return results.Count;
                }),
                "Upload files").GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            HandleException("Upload files", ex, remoteDic);
            return 0;
        }
    }

    /// <summary>
    ///     上传单文件(支持工作目录)
    /// </summary>
    /// <param name="localPath">本地文件完整路径 (例如: @"D:\abc.txt")</param>
    /// <param name="remoteFileName">远程文件名 (例如: "abc.txt"), 为null时使用本地文件名</param>
    /// <param name="remoteExistsMode">远程文件存在时的处理模式, 默认覆盖</param>
    /// <returns>上传是否成功</returns>
    /// <exception cref="FileNotFoundException">本地文件不存在时抛出</exception>
    /// <exception cref="FtpException">FTP操作异常时抛出</exception>
    public bool UploadFile2(
        string localPath,
        string? remoteFileName = null,
        FtpRemoteExists remoteExistsMode = FtpRemoteExists.Overwrite)
    {
        ArgumentNullException.ThrowIfNull(localPath);

        if (!File.Exists(localPath))
        {
            throw new FileNotFoundException("本地文件不存在", localPath);
        }

        try
        {
            using var scope = CreateConnectionScope();
            using var fileStream = File.OpenRead(localPath);

            remoteFileName ??= Path.GetFileName(localPath);

            FtpStatus result = FtpClient.UploadStream(
                fileStream,
                remoteFileName,
                remoteExistsMode,
                createRemoteDir: true);

            return result == FtpStatus.Success;
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, localPath);
            return false;
        }
    }


    public int DownloadFiles(string localDic, IEnumerable<string> remoteFiles)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return _retryHelper.ExecuteAsync(
                () => Task.Run(() =>
                {
                    if (!Directory.Exists(localDic))
                        Directory.CreateDirectory(localDic);

                    List<FtpResult> results = FtpClient.DownloadFiles(localDic, remoteFiles);
                    return results.Count;
                }),
                "Download files").GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            HandleException("Download files", ex, localDic);
            return 0;
        }
    }

    public async Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationPath, string fileName, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            var remoteFilePath = string.IsNullOrEmpty(destinationPath) ? fileName : Path.Combine(destinationPath, fileName).Replace("\\", "/");
            
            // 确保目录存在
            var remoteDirectory = Path.GetDirectoryName(remoteFilePath)?.Replace("\\", "/");
            if (!string.IsNullOrEmpty(remoteDirectory))
            {
                CreateDirectoryIfNotExists(remoteDirectory);
            }

            // 检查是否支持异步方法（根据FluentFTP版本而定）
            var ftpStatus = FtpStatus.Failed;
            
#if NET5_0_OR_GREATER
            // 执行上传 - 异步版本
            var result = await _retryHelper.ExecuteAsync(
                async () =>
                {
                    inputStream.Position = 0;
                    var status = await FtpClient.UploadStreamAsync(
                        inputStream, 
                        remoteFilePath, 
                        overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                        createRemoteDir: true,
                        token: cancellationToken);
                    
                    return status == FtpStatus.Success;
                },
                "Upload file");
#else
            // 执行上传 - 同步版本
            var result = await _retryHelper.ExecuteAsync(
                () => Task.Run(() => 
                {
                    inputStream.Position = 0;
                    ftpStatus = FtpClient.UploadStream(
                        inputStream,
                        remoteFilePath,
                        overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                        createRemoteDir: true);
                    
                    return ftpStatus == FtpStatus.Success;
                }, cancellationToken),
                "Upload file");
#endif

            if (!result)
                return FileOperationResult.CreateFailure($"上传文件失败: {remoteFilePath}");

            return FileOperationResult.CreateSuccess(remoteFilePath);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Destination: {destinationPath}/{fileName}");
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    public async Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationPath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(localFilePath))
        {
            return FileOperationResult.CreateFailure($"本地文件不存在: {localFilePath}");
        }

        using var scope = CreateConnectionScope();
        try
        {
            var fileName = Path.GetFileName(localFilePath);
            var remoteFilePath = string.IsNullOrEmpty(destinationPath) ? fileName : Path.Combine(destinationPath, fileName).Replace("\\", "/");
            
            // 确保目录存在
            var remoteDirectory = Path.GetDirectoryName(remoteFilePath)?.Replace("\\", "/");
            if (!string.IsNullOrEmpty(remoteDirectory))
            {
                CreateDirectoryIfNotExists(remoteDirectory);
            }

#if NET5_0_OR_GREATER
            // 执行上传 - 异步版本
            var result = await _retryHelper.ExecuteAsync(
                async () =>
                {
                    var status = await FtpClient.UploadFileAsync(
                        localFilePath, 
                        remoteFilePath, 
                        overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                        createRemoteDir: true,
                        token: cancellationToken);
                    
                    return status == FtpStatus.Success;
                },
                "Upload file");
#else
            // 执行上传 - 同步版本
            var result = await _retryHelper.ExecuteAsync(
                () => Task.Run(() => 
                {
                    var status = FtpClient.UploadFile(
                        localFilePath,
                        remoteFilePath,
                        overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                        createRemoteDir: true);
                    
                    return status == FtpStatus.Success;
                }, cancellationToken),
                "Upload file");
#endif

            if (!result)
                return FileOperationResult.CreateFailure($"上传文件失败: {remoteFilePath}");

            var fileInfo = new FileInfo(localFilePath);
            return FileOperationResult.CreateSuccess(remoteFilePath, null, fileInfo.Length);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Local: {localFilePath}, Destination: {destinationPath}");
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    public async Task<FileOperationResult> DownloadToStreamAsync(string filePath, Stream outputStream, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!FileExists(filePath))
            {
                return FileOperationResult.CreateFailure($"文件不存在: {filePath}");
            }

#if NET5_0_OR_GREATER
            // 异步版本
            var result = await _retryHelper.ExecuteAsync(
                async () =>
                {
                    var status = await FtpClient.DownloadStreamAsync(
                        outputStream,
                        filePath,
                        token: cancellationToken);
                    
                    return status == FtpStatus.Success;
                },
                "Download to stream");
#else
            // 同步版本
            var result = await _retryHelper.ExecuteAsync(
                () => Task.Run(() => 
                {
                    var status = FtpClient.DownloadStream(
                        outputStream, 
                        filePath);
                    
                    return status == true;
                }, cancellationToken),
                "Download to stream");
#endif

            if (!result)
                return FileOperationResult.CreateFailure($"下载文件到流失败: {filePath}");

            // 获取文件大小
            long fileSize = 0;
            try { fileSize = FtpClient.GetFileSize(filePath); } catch { /* 忽略获取大小失败 */ }

            return FileOperationResult.CreateSuccess(filePath, null, fileSize);
        }
        catch (Exception ex)
        {
            HandleException("Download to stream", ex, filePath);
            return FileOperationResult.CreateFailure($"下载文件到流失败: {ex.Message}", ex);
        }
    }

    public async Task<FileOperationResult> DownloadFileAsync(string filePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!FileExists(filePath))
            {
                return FileOperationResult.CreateFailure($"文件不存在: {filePath}");
            }

            // 确保目标目录存在
            var destDir = Path.GetDirectoryName(localDestinationPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // 检查文件是否已存在
            if (File.Exists(localDestinationPath) && !overwrite)
            {
                return FileOperationResult.CreateFailure($"目标文件已存在: {localDestinationPath}");
            }

#if NET5_0_OR_GREATER
            // 异步版本
            var result = await _retryHelper.ExecuteAsync(
                async () =>
                {
                    var status = await FtpClient.DownloadFileAsync(
                        localDestinationPath,
                        filePath,
                        overwrite ? FtpLocalExists.Overwrite : FtpLocalExists.Skip,
                        token: cancellationToken);
                    
                    return status == FtpStatus.Success;
                },
                "Download file");
#else
            // 同步版本
            var result = await _retryHelper.ExecuteAsync(
                () => Task.Run(() => 
                {
                    var status = FtpClient.DownloadFile(
                        localDestinationPath, 
                        filePath, 
                        overwrite ? FtpLocalExists.Overwrite : FtpLocalExists.Skip);
                    
                    return status == FtpStatus.Success;
                }, cancellationToken),
                "Download file");
#endif

            if (!result)
                return FileOperationResult.CreateFailure($"下载文件失败: {filePath}");

            var fileInfo = new FileInfo(localDestinationPath);
            return FileOperationResult.CreateSuccess(filePath, localDestinationPath, fileInfo.Length);
        }
        catch (Exception ex)
        {
            HandleException("Download file", ex, filePath);
            return FileOperationResult.CreateFailure($"下载文件失败: {ex.Message}", ex);
        }
    }

    public async Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!FileExists(filePath))
            {
                return FileOperationResult.CreateSuccess(filePath); // 文件不存在也视为成功
            }

            await Task.Run(() => FtpClient.DeleteFile(filePath), cancellationToken);
            return FileOperationResult.CreateSuccess(filePath);
        }
        catch (Exception ex)
        {
            HandleException("Delete file", ex, filePath);
            return FileOperationResult.CreateFailure($"删除文件失败: {ex.Message}", ex);
        }
    }

    public abstract string ServerDetails();
}
