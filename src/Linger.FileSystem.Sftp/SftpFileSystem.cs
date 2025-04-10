using Linger.Extensions.Core;
using Linger.FileSystem.Remote;
using Linger.Helper;
using Renci.SshNet;

namespace Linger.FileSystem.Sftp;

/// <summary>
/// SFTP文件系统实现
/// </summary>
public class SftpFileSystem : RemoteFileSystemBase
{
    /// <summary>
    /// SFTP客户端
    /// </summary>
    protected SftpClient Client { get; }
    private static readonly char[] s_separator = ['/', '\\'];

    // 确保正确初始化客户端
    public SftpFileSystem(RemoteSystemSetting setting, RetryOptions? retryOptions = null)
        : base(setting, retryOptions)
    {
        Client = CreateClient();
    }

    private SftpClient CreateClient()
    {
        // 创建连接信息，考虑证书认证选项
        ConnectionInfo connectionInfo;

        if (Setting.CertificatePath.IsNotNullAndEmpty())
        {
            var privateKeyFile = new PrivateKeyFile(Setting.CertificatePath, Setting.CertificatePassphrase);
            connectionInfo = new ConnectionInfo(
                Setting.Host,
                Setting.Port,
                Setting.UserName,
                new PrivateKeyAuthenticationMethod(Setting.UserName, privateKeyFile));
        }
        else
        {
            connectionInfo = new ConnectionInfo(
                Setting.Host,
                Setting.Port,
                Setting.UserName,
                new PasswordAuthenticationMethod(Setting.UserName, Setting.Password));
        }

        // 设置超时
        connectionInfo.Timeout = TimeSpan.FromMilliseconds(Setting.ConnectionTimeout);

        return new SftpClient(connectionInfo);
    }

    #region 连接管理

    public override bool IsConnected() => Client?.IsConnected ?? false;

    public override void Connect()
    {
        if (Client != null && !Client.IsConnected)
            Client.Connect();
    }

    public override void Disconnect()
    {
        if (Client?.IsConnected == true)
            Client.Disconnect();
    }

    public override void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 文件操作基本方法

    public override bool FileExists(string filePath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return Client.Exists(filePath) && Client.GetAttributes(filePath).IsRegularFile;
        }
        catch (Exception ex)
        {
            HandleException("Check file exists", ex, filePath);
            return false;
        }
    }

    public override bool DirectoryExists(string directoryPath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return Client.Exists(directoryPath) && Client.GetAttributes(directoryPath).IsDirectory;
        }
        catch (Exception ex)
        {
            HandleException("Check directory exists", ex, directoryPath);
            return false;
        }
    }

    public override void CreateDirectoryIfNotExists(string directoryPath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!DirectoryExists(directoryPath))
            {
                // SFTP可能需要递归创建目录
                string[] paths = directoryPath.Split(s_separator, StringSplitOptions.RemoveEmptyEntries);
                string currentPath = string.Empty;

                foreach (var path in paths)
                {
                    currentPath += "/" + path;
                    if (!DirectoryExists(currentPath))
                        Client.CreateDirectory(currentPath);
                }
            }
        }
        catch (Exception ex)
        {
            HandleException("Create directory", ex, directoryPath);
        }
    }

    public override void DeleteFileIfExists(string filePath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (FileExists(filePath))
                Client.DeleteFile(filePath);
        }
        catch (Exception ex)
        {
            HandleException("Delete file", ex, filePath);
        }
    }

    public override Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => FileExists(filePath), cancellationToken);
    }

    public override Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => DirectoryExists(directoryPath), cancellationToken);
    }

    public override Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => CreateDirectoryIfNotExists(directoryPath), cancellationToken);
    }

    public override Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => DeleteFileIfExists(filePath), cancellationToken);
    }

    #endregion

    #region 文件传输操作

    public override async Task<FileOperationResult> UploadAsync(Stream inputStream, string filePath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            // 分离路径和文件名
            var remoteDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
            var fileName = Path.GetFileName(filePath);

            // 确保目录存在
            CreateDirectoryIfNotExists(remoteDirectory);

            // 检查文件是否存在
            if (FileExists(filePath) && !overwrite)
                return FileOperationResult.CreateFailure($"远程文件已存在: {filePath}");

            // 执行上传
            await RetryHelper.ExecuteAsync(
                async () =>
                {
                    await Task.Run(() =>
                    {
                        using var ms = new MemoryStream();
                        inputStream.Position = 0;
                        inputStream.CopyTo(ms);
                        ms.Position = 0;

                        if (FileExists(filePath) && overwrite)
                            Client.DeleteFile(filePath);

                        Client.UploadFile(ms, filePath);
                        return true;
                    }, cancellationToken);
                    return true;
                },
                "Upload file", cancellationToken: cancellationToken);

            // 获取文件大小
            var fileSize = Client.GetAttributes(filePath).Size;

            return FileOperationResult.CreateSuccess(filePath, null, fileSize);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Destination: {filePath}");
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationPath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(localFilePath))
            return FileOperationResult.CreateFailure($"本地文件不存在: {localFilePath}");

        try
        {
            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
            var fileName = Path.GetFileName(localFilePath);
            // 使用正斜杠（/）确保与SFTP服务器路径格式一致
            var filePath = destinationPath.EndsWith("/") 
                ? $"{destinationPath}{fileName}" 
                : $"{destinationPath}/{fileName}";
                
            // 使用新的合并参数格式调用UploadAsync
            return await UploadAsync(fileStream, filePath, overwrite, cancellationToken);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Local: {localFilePath}, Destination: {destinationPath}");
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DownloadToStreamAsync(string filePath, Stream outputStream, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!FileExists(filePath))
                return FileOperationResult.CreateFailure($"文件不存在: {filePath}");

            await RetryHelper.ExecuteAsync(
                async () =>
                {
                    await Task.Run(() =>
                    {
                        Client.DownloadFile(filePath, outputStream);
                        return true;
                    }, cancellationToken);
                    return true;
                },
                "Download to stream", cancellationToken: cancellationToken);

            // 获取文件大小
            var fileSize = Client.GetAttributes(filePath).Size;

            return FileOperationResult.CreateSuccess(filePath, null, fileSize);
        }
        catch (Exception ex)
        {
            HandleException("Download to stream", ex, filePath);
            return FileOperationResult.CreateFailure($"下载文件到流失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DownloadFileAsync(string filePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default)
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

            await RetryHelper.ExecuteAsync(
                async () =>
                {
                    await Task.Run(() =>
                    {
                        if (File.Exists(localDestinationPath) && overwrite)
                            File.Delete(localDestinationPath);

                        using var fileStream = File.Create(localDestinationPath);
                        Client.DownloadFile(filePath, fileStream);
                        return true;
                    }, cancellationToken);
                    return true;
                },
                "Download file", cancellationToken: cancellationToken);

            var fileInfo = new FileInfo(localDestinationPath);
            return FileOperationResult.CreateSuccess(filePath, localDestinationPath, fileInfo.Length);
        }
        catch (Exception ex)
        {
            HandleException("Download file", ex, filePath);
            return FileOperationResult.CreateFailure($"下载文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!FileExists(filePath))
                return FileOperationResult.CreateSuccess(filePath); // 文件不存在也视为成功

            await Task.Run(() => Client.DeleteFile(filePath), cancellationToken);
            return FileOperationResult.CreateSuccess(filePath);
        }
        catch (Exception ex)
        {
            HandleException("Delete file", ex, filePath);
            return FileOperationResult.CreateFailure($"删除文件失败: {ex.Message}", ex);
        }
    }

    #endregion

    #region SFTP特有功能

    /// <summary>
    /// 列出目录中的文件
    /// </summary>
    public List<string> ListFiles(string directoryPath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!DirectoryExists(directoryPath))
                return new List<string>();

            var files = Client.ListDirectory(directoryPath)
                .Where(file => !file.IsDirectory && !file.Name.StartsWith('.'))
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

    /// <summary>
    /// 列出目录中的子目录
    /// </summary>
    public List<string> ListDirectories(string directoryPath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!DirectoryExists(directoryPath))
                return new List<string>();

            var directories = Client.ListDirectory(directoryPath)
                .Where(file => file.IsDirectory && !file.Name.StartsWith('.') && file.Name != "." && file.Name != "..")
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

    /// <summary>
    /// 设置工作目录
    /// </summary>
    public void SetWorkingDirectory(string directoryPath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            Client.ChangeDirectory(directoryPath);
        }
        catch (Exception ex)
        {
            HandleException("Set working directory", ex, directoryPath);
        }
    }

    /// <summary>
    /// 设置根目录为工作目录
    /// </summary>
    public void SetRootAsWorkingDirectory()
    {
        SetWorkingDirectory("/");
    }

    /// <summary>
    /// 获取文件最后修改时间
    /// </summary>
    /// <param name="remotePath">远程路径("/test/abc.txt")</param>
    /// <returns>最后修改时间</returns>
    public DateTime GetLastModifiedTime(string remotePath)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return Client.GetLastWriteTime(remotePath);
        }
        catch (Exception ex)
        {
            HandleException("Get last modified time", ex, remotePath);
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// 异步获取文件修改时间
    /// </summary>
    public Task<DateTime> GetLastModifiedTimeAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => GetLastModifiedTime(remotePath), cancellationToken);
    }

    /// <summary>
    /// 异步获取目录列表
    /// </summary>
    public Task<List<string>> ListFilesAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => ListFiles(directoryPath), cancellationToken);
    }

    /// <summary>
    /// 异步获取子目录列表
    /// </summary>
    public Task<List<string>> ListDirectoriesAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() => ListDirectories(directoryPath), cancellationToken);
    }

    #endregion
}
