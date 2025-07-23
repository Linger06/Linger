using System.Net;
using System.Text;
using FluentFTP;
using Linger.Extensions.Core;
using Linger.FileSystem.Remote;
using Linger.Helper;

namespace Linger.FileSystem.Ftp;

/// <summary>
/// FTP文件系统实现
/// </summary>
public class FtpFileSystem : RemoteFileSystemBase
{
    /// <summary>
    /// FTP客户端
    /// </summary>
    protected AsyncFtpClient Client { get; }

    public FtpFileSystem(RemoteSystemSetting setting, RetryOptions? retryOptions = null)
        : base(setting, retryOptions)
    {
        Client = CreateClient();
    }

    private AsyncFtpClient CreateClient()
    {
        // 创建FTP客户端配置
        var config = new FtpConfig
        {
            RetryAttempts = 0, // 使用基类中的重试机制
            TimeConversion = FtpDate.LocalTime,
            ServerTimeZone = TimeZoneInfo.Utc,
            ClientTimeZone = TimeZoneInfo.Local,
            ConnectTimeout = Setting.ConnectionTimeout,
            DataConnectionConnectTimeout = Setting.ConnectionTimeout,
            DataConnectionReadTimeout = Setting.OperationTimeout,
            ReadTimeout = Setting.OperationTimeout,
            StaleDataCheck = true
        };

        // 创建AsyncFtpClient
        var client = new AsyncFtpClient(
            Setting.Host,
            new NetworkCredential(Setting.UserName, Setting.Password),
            Setting.Port)
        {
            Config = config,
            Encoding = Setting.Encoding ?? Encoding.Default
        };

        return client;
    }

    #region 连接管理

    public override bool IsConnected() => Client.IsConnected;

    public override void Connect()
    {
        if (!Client.IsConnected)
            Client.Connect();
    }

    public override void Disconnect()
    {
        if (Client.IsConnected)
            Client.Disconnect();
    }

    public override void Dispose()
    {
        if (!Client.IsDisposed)
            Client.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion

    #region 文件操作基本方法

    public override bool FileExists(string filePath)
    {
        return FileExistsAsync(filePath).GetAwaiter().GetResult();
    }

    public override bool DirectoryExists(string directoryPath)
    {
        return DirectoryExistsAsync(directoryPath).GetAwaiter().GetResult();
    }

    public override void CreateDirectoryIfNotExists(string directoryPath)
    {
        CreateDirectoryIfNotExistsAsync(directoryPath).GetAwaiter().GetResult();
    }

    public override void DeleteFileIfExists(string filePath)
    {
        DeleteFileIfExistsAsync(filePath).GetAwaiter().GetResult();
    }

    public override async Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return await Client.FileExists(filePath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Check file exists", ex, filePath);
            return false;
        }
    }

    public override async Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return await Client.DirectoryExists(directoryPath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Check directory exists", ex, directoryPath);
            return false;
        }
    }

    public override async Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!await DirectoryExistsAsync(directoryPath, cancellationToken).ConfigureAwait(false))
                _ = await Client.CreateDirectory(directoryPath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Create directory", ex, directoryPath);
        }
    }

    public override async Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (await FileExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
                await Client.DeleteFile(filePath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Delete file", ex, filePath);
        }
    }

    #endregion

    #region 文件传输操作

    public override async Task<FileOperationResult> UploadAsync(Stream inputStream, string filePath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            // 使用适合FTP的路径分隔逻辑
            string remoteDirectory;
            string fileName;

            // FTP路径始终使用正斜杠
            if (filePath.Contains('/'))
            {
                var lastSlashIndex = filePath.LastIndexOf('/');
                remoteDirectory = filePath.Take(lastSlashIndex);
                fileName = filePath.Substring(lastSlashIndex + 1);
            }
            else
            {
                // 没有找到斜杠，整个路径被视为文件名，目录为空
                remoteDirectory = string.Empty;
                fileName = filePath;
            }

            // 确保目录存在
            await CreateDirectoryIfNotExistsAsync(remoteDirectory, cancellationToken).ConfigureAwait(false);

            // 执行上传
            var result = await RetryHelper.ExecuteAsync(
                async () =>
                {
                    inputStream.Position = 0;
                    var status = await Client.UploadStream(
                        inputStream,
                        filePath,
                        overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                        createRemoteDir: true,
                        token: cancellationToken).ConfigureAwait(false);

                    return status == FtpStatus.Success;
                },
                "Upload file", cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result)
                return FileOperationResult.CreateFailure($"上传文件失败: {filePath}");

            long fileSize = 0;
            try { fileSize = await Client.GetFileSize(filePath, token: cancellationToken).ConfigureAwait(false); } catch { /* 忽略 */ }

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
        {
            return FileOperationResult.CreateFailure($"本地文件不存在: {localFilePath}");
        }

        using var scope = CreateConnectionScope();
        try
        {
            var fileName = Path.GetFileName(localFilePath);
            // 使用正斜杠（/）确保与FTP服务器路径格式一致
            var filePath = destinationPath.EndsWith('/')
                ? $"{destinationPath}{fileName}"
                : $"{destinationPath}/{fileName}";

            // 执行上传
            var result = await RetryHelper.ExecuteAsync(
                async () =>
                {
                    var status = await Client.UploadFile(
                        localFilePath,
                        filePath,
                        overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                        createRemoteDir: true,
                        token: cancellationToken).ConfigureAwait(false);

                    return status == FtpStatus.Success;
                },
                "Upload file", cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result)
                return FileOperationResult.CreateFailure($"上传文件失败: {filePath}");

            var fileInfo = new FileInfo(localFilePath);
            return FileOperationResult.CreateSuccess(filePath, null, fileInfo.Length);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Local: {localFilePath}, Destination: {destinationPath}");
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!await FileExistsAsync(remoteFilePath, cancellationToken).ConfigureAwait(false))
            {
                return FileOperationResult.CreateFailure($"文件不存在: {remoteFilePath}");
            }

            // 使用AsyncFtpClient执行下载
            var result = await RetryHelper.ExecuteAsync(
                async () =>
                {
                    var status = await Client.DownloadStream(
                        outputStream,
                        remoteFilePath,
                        token: cancellationToken).ConfigureAwait(false);

                    return status;
                },
                "Download to stream", cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result)
                return FileOperationResult.CreateFailure($"下载文件到流失败: {remoteFilePath}");

            // 获取文件大小
            long fileSize = 0;
            try { fileSize = await Client.GetFileSize(remoteFilePath, token: cancellationToken).ConfigureAwait(false); } catch { /* 忽略获取大小失败 */ }

            return FileOperationResult.CreateSuccess(remoteFilePath, null, fileSize);
        }
        catch (Exception ex)
        {
            HandleException("Download to stream", ex, remoteFilePath);
            return FileOperationResult.CreateFailure($"下载文件到流失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 从FTP服务器下载文件到本地文件系统
    /// </summary>
    /// <param name="remoteFilePath">FTP服务器上文件的完整路径或相对路径，例如："/htdocs/MyVideo_2.mp4"</param>
    /// <param name="localDestinationPath">本地文件系统的完整路径或相对路径，例如：@"C:\MyVideo_2.mp4"</param>
    /// <param name="overwrite">如果目标文件已存在，是否覆盖。默认为 false</param>
    /// <param name="cancellationToken">用于取消操作的令牌</param>
    /// <returns>包含下载操作结果的 <see cref="FileOperationResult"/>，包括是否成功、文件路径和大小等信息</returns>
    /// <exception cref="FileSystemException">当文件操作失败时抛出</exception>
    public override async Task<FileOperationResult> DownloadFileAsync(string remoteFilePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!await FileExistsAsync(remoteFilePath, cancellationToken).ConfigureAwait(false))
            {
                return FileOperationResult.CreateFailure($"文件不存在: {remoteFilePath}");
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

            // 执行下载
            var result = await RetryHelper.ExecuteAsync(
                async () =>
                {
                    var status = await Client.DownloadFile(
                        localDestinationPath,
                        remoteFilePath,
                        overwrite ? FtpLocalExists.Overwrite : FtpLocalExists.Skip,
                        token: cancellationToken).ConfigureAwait(false);

                    return status == FtpStatus.Success;
                },
                "Download file", cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result)
                return FileOperationResult.CreateFailure($"下载文件失败: {remoteFilePath}");

            var fileInfo = new FileInfo(localDestinationPath);
            return FileOperationResult.CreateSuccess(remoteFilePath, localDestinationPath, fileInfo.Length);
        }
        catch (Exception ex)
        {
            HandleException("Download file", ex, remoteFilePath);
            return FileOperationResult.CreateFailure($"下载文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!await FileExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
            {
                return FileOperationResult.CreateSuccess(filePath); // 文件不存在也视为成功
            }

            // 执行删除
            await RetryHelper.ExecuteAsync(
                async () =>
                {
                    await Client.DeleteFile(filePath, cancellationToken).ConfigureAwait(false);
                    return true;
                },
                "Delete file", cancellationToken: cancellationToken).ConfigureAwait(false);

            return FileOperationResult.CreateSuccess(filePath);
        }
        catch (Exception ex)
        {
            HandleException("Delete file", ex, filePath);
            return FileOperationResult.CreateFailure($"删除文件失败: {ex.Message}", ex);
        }
    }

    #endregion

    #region 其他FTP特定功能

    /// <summary>
    /// 获取文件最后修改时间
    /// </summary>
    public async Task<DateTime> GetLastModifiedTimeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            return await Client.GetModifiedTime(filePath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Get last modified time", ex, filePath);
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// 列出目录内容
    /// </summary>
    public async Task<IEnumerable<string>> ListDirectoryAsync(string? directoryPath = null, FtpObjectType type = FtpObjectType.File, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            FtpListItem[] items = directoryPath == null ?
                await Client.GetListing(token: cancellationToken).ConfigureAwait(false) :
                await Client.GetListing(directoryPath, token: cancellationToken).ConfigureAwait(false);

            return items.Where(f => f.Type == type)
                       .Select(f => f.Name);
        }
        catch (Exception ex)
        {
            HandleException("List directory", ex, directoryPath);
            return [];
        }
    }

    /// <summary>
    /// 设置工作目录
    /// </summary>
    public async Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!string.IsNullOrWhiteSpace(directoryPath) && await DirectoryExistsAsync(directoryPath, cancellationToken).ConfigureAwait(false))
                await Client.SetWorkingDirectory(directoryPath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Set working directory", ex, directoryPath);
        }
    }

    /// <summary>
    /// 批量上传文件
    /// </summary>
    public async Task<int> UploadFilesAsync(IEnumerable<string> localFiles, string remoteDirectory, FtpRemoteExists remoteExistsMode = FtpRemoteExists.Overwrite, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            var fileInfos = localFiles.Where(File.Exists)
                                      .Select(f => new FileInfo(f))
                                      .ToList();

            if (fileInfos.Count == 0)
                return 0;

            List<FtpResult> results = await Client.UploadFiles(fileInfos, remoteDirectory, remoteExistsMode, token: cancellationToken).ConfigureAwait(false);
            return results.Count;
        }
        catch (Exception ex)
        {
            HandleException("Upload files", ex, remoteDirectory);
            return 0;
        }
    }

    /// <summary>
    /// 批量下载文件
    /// </summary>
    public async Task<int> DownloadFilesAsync(string localDirectory, IEnumerable<string> remoteFiles, CancellationToken cancellationToken = default)
    {
        using var scope = CreateConnectionScope();
        try
        {
            if (!Directory.Exists(localDirectory))
                Directory.CreateDirectory(localDirectory);

            List<FtpResult> results = await Client.DownloadFiles(localDirectory, remoteFiles, token: cancellationToken).ConfigureAwait(false);
            return results.Count;
        }
        catch (Exception ex)
        {
            HandleException("Download files", ex, localDirectory);
            return 0;
        }
    }

    #endregion
}
