using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Linger.FileSystem.Helpers;
using Linger.FileSystem.Remote;
using Linger.Helper;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Linger.FileSystem.Exceptions;

namespace Linger.FileSystem.Sftp;

public abstract class SftpContext : IFileSystemOperations, IRemoteFileSystemContext
{
    private readonly RetryHelper _retryHelper;
    protected SftpClient SftpClient { get; set; } = default!;

    protected SftpContext(RetryOptions? retryOptions = null)
    {
        _retryHelper = new RetryHelper(retryOptions ?? new RetryOptions());
    }

    protected virtual void HandleException(string operation, Exception ex, string? path = null)
    {
        // 统一的异常处理，可以在派生类中重写实现具体的日志记录
        string message = $"{operation} failed. {(path != null ? $"Path: {path}. " : string.Empty)}";
        throw new FileSystemException(operation, path, message, ex);
    }

    public class SftpConnectionScope : IDisposable
    {
        private readonly SftpContext _context;
        private readonly bool _wasConnected;

        public SftpConnectionScope(SftpContext context)
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

    protected SftpConnectionScope CreateConnectionScope()
    {
        return new SftpConnectionScope(this);
    }

    public bool IsConnected() => SftpClient?.IsConnected ?? false;

    public void Connect()
    {
        if (SftpClient != null && !SftpClient.IsConnected)
            SftpClient.Connect();
    }

    public void Disconnect()
    {
        if (SftpClient?.IsConnected == true)
            SftpClient.Disconnect();
    }

    public void Dispose()
    {
        SftpClient?.Dispose();
        GC.SuppressFinalize(this);
    }

    public bool FileExists(string filePath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return SftpClient!.Exists(filePath) && SftpClient.GetAttributes(filePath).IsRegularFile;
        }
        catch (Exception ex)
        {
            HandleException("Check file exists", ex, filePath);
            return false;
        }
    }

    public bool DirectoryExists(string directoryPath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return SftpClient!.Exists(directoryPath) && SftpClient.GetAttributes(directoryPath).IsDirectory;
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
            {
                // SFTP可能需要递归创建目录
                string[] paths = directoryPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                string currentPath = string.Empty;

                foreach (var path in paths)
                {
                    currentPath += "/" + path;
                    if (!DirectoryExists(currentPath))
                        SftpClient!.CreateDirectory(currentPath);
                }
            }
        }
        catch (Exception ex)
        {
            HandleException("Create directory", ex, directoryPath);
        }
    }

    public void DeleteFileIfExists(string filePath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (FileExists(filePath))
                SftpClient!.DeleteFile(filePath);
        }
        catch (Exception ex)
        {
            HandleException("Delete file", ex, filePath);
        }
    }

    public async Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationPath, string fileName, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            var remoteFilePath = NormalizePath(destinationPath, fileName);
            
            // 确保目录存在
            var remoteDirectory = Path.GetDirectoryName(remoteFilePath)?.Replace("\\", "/");
            if (!string.IsNullOrEmpty(remoteDirectory))
                CreateDirectoryIfNotExists(remoteDirectory);
            
            // 检查文件是否存在
            if (FileExists(remoteFilePath) && !overwrite)
                return FileOperationResult.CreateFailure($"远程文件已存在: {remoteFilePath}");

            // 执行上传
            await _retryHelper.ExecuteAsync(
                async () => 
                {
                    await Task.Run(() =>
                    {
                        using var ms = new MemoryStream();
                        inputStream.Position = 0;
                        inputStream.CopyTo(ms);
                        ms.Position = 0;
                        
                        if (FileExists(remoteFilePath) && overwrite)
                            SftpClient!.DeleteFile(remoteFilePath);
                            
                        SftpClient!.UploadFile(ms, remoteFilePath);
                        return true;
                    }, cancellationToken);
                    return true;
                },
                "Upload file");

            // 获取文件大小
            var fileSize = SftpClient!.GetAttributes(remoteFilePath).Size;
            
            return FileOperationResult.CreateSuccess(remoteFilePath, null, fileSize);
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
            return FileOperationResult.CreateFailure($"本地文件不存在: {localFilePath}");

        try
        {
            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
            var fileName = Path.GetFileName(localFilePath);
            return await UploadAsync(fileStream, destinationPath, fileName, overwrite, cancellationToken);
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
                return FileOperationResult.CreateFailure($"文件不存在: {filePath}");

            await _retryHelper.ExecuteAsync(
                async () =>
                {
                    await Task.Run(() =>
                    {
                        SftpClient!.DownloadFile(filePath, outputStream);
                        return true;
                    }, cancellationToken);
                    return true;
                },
                "Download to stream");

            // 获取文件大小
            var fileSize = SftpClient!.GetAttributes(filePath).Size;
            
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
                return FileOperationResult.CreateFailure($"文件不存在: {filePath}");
                
            // 确保目标目录存在
            var destDir = Path.GetDirectoryName(localDestinationPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
                
            // 检查文件是否已存在
            if (File.Exists(localDestinationPath) && !overwrite)
                return FileOperationResult.CreateFailure($"目标文件已存在: {localDestinationPath}");

            await _retryHelper.ExecuteAsync(
                async () =>
                {
                    await Task.Run(() =>
                    {
                        if (File.Exists(localDestinationPath) && overwrite)
                            File.Delete(localDestinationPath);
                            
                        using var fileStream = File.Create(localDestinationPath);
                        SftpClient!.DownloadFile(filePath, fileStream);
                        return true;
                    }, cancellationToken);
                    return true;
                },
                "Download file");
                
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
                return FileOperationResult.CreateSuccess(filePath); // 文件不存在也视为成功

            await Task.Run(() => SftpClient!.DeleteFile(filePath), cancellationToken);
            return FileOperationResult.CreateSuccess(filePath);
        }
        catch (Exception ex)
        {
            HandleException("Delete file", ex, filePath);
            return FileOperationResult.CreateFailure($"删除文件失败: {ex.Message}", ex);
        }
    }
    
    // 辅助方法：标准化路径，将Windows路径转换为Unix路径
    protected string NormalizePath(string directoryPath, string fileName)
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
    
    // 列出目录中的文件
    public List<string> ListFiles(string directoryPath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!DirectoryExists(directoryPath))
                return new List<string>();
                
            var files = SftpClient!.ListDirectory(directoryPath)
                .Where(file => !file.IsDirectory && !file.Name.StartsWith("."))
                .Select(file => file.Name)
                .ToList();
                
            return files;
        }
        catch (Exception ex)
        {
            HandleException("List files", ex, directoryPath);
            return new List<string>();
        }
    }
    
    // 列出目录中的子目录
    public List<string> ListDirectories(string directoryPath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!DirectoryExists(directoryPath))
                return new List<string>();
                
            var directories = SftpClient!.ListDirectory(directoryPath)
                .Where(file => file.IsDirectory && !file.Name.StartsWith(".") && file.Name != "." && file.Name != "..")
                .Select(file => file.Name)
                .ToList();
                
            return directories;
        }
        catch (Exception ex)
        {
            HandleException("List directories", ex, directoryPath);
            return new List<string>();
        }
    }

    public void SetWorkingDirectory(string directoryPath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            SftpClient!.ChangeDirectory(directoryPath);
        }
        catch (Exception ex)
        {
            HandleException("Set working directory", ex, directoryPath);
        }
    }

    public void SetRootAsWorkingDirectory()
    {
        SetWorkingDirectory("/");
    }

    /// <summary>
    ///     获取文件最后修改时间
    /// </summary>
    /// <param name="remotePath">远程路径("/test/abc.txt")</param>
    /// <returns></returns>
    public DateTime GetLastModifiedTime(string remotePath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return SftpClient!.GetLastWriteTime(remotePath);
        }
        catch (Exception ex)
        {
            HandleException("Get last modified time", ex, remotePath);
            return DateTime.MinValue;
        }
    }

    public abstract string ServerDetails();

    // 添加异步接口实现
    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => FileExists(filePath), cancellationToken);
    }
    
    public Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => DirectoryExists(directoryPath), cancellationToken);
    }
    
    public Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => CreateDirectoryIfNotExists(directoryPath), cancellationToken);
    }
    
    public Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => DeleteFileIfExists(filePath), cancellationToken);
    }
}