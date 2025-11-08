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
    private const char FtpPathSeparator = '/';
    private const string FtpRootPath = "/";
    private bool _disposed;

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
            Encoding = Setting.Encoding ?? Encoding.UTF8
        };

        return client;
    }

    #region 连接管理

    public override bool IsConnected() => Client.IsConnected;

    public override async Task ConnectAsync()
    {
        if (!Client.IsConnected)
            await Client.AutoConnect().ConfigureAwait(false);
    }

    public override async Task DisconnectAsync()
    {
        if (Client.IsConnected)
            await Client.Disconnect().ConfigureAwait(false);
    }

    public override void Dispose()
    {
        if (_disposed)
            return;

        if (!Client.IsDisposed)
            Client.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 文件操作基本方法

    /// <summary>
    /// 检查文件是否存在（同步方法）
    /// </summary>
    /// <remarks>
    /// ⚠️ 此同步方法已被移除以避免死锁风险。
    /// 请使用 <see cref="FileExistsAsync"/> 异步版本。
    /// </remarks>
    /// <exception cref="NotSupportedException">同步方法不再受支持</exception>
    [Obsolete("此同步方法可能导致死锁，请使用 FileExistsAsync 异步版本", true)]
    public override bool FileExists(string filePath)
    {
        throw new NotSupportedException(
            "同步方法 FileExists 已被移除以避免死锁。请使用 FileExistsAsync 异步版本。" +
            "参考迁移指南: https://github.com/Linger06/Linger/blob/develop/src/Linger.FileSystem/README.zh-CN.md#迁移指南");
    }

    /// <summary>
    /// 检查目录是否存在（同步方法）
    /// </summary>
    /// <remarks>
    /// ⚠️ 此同步方法已被移除以避免死锁风险。
    /// 请使用 <see cref="DirectoryExistsAsync"/> 异步版本。
    /// </remarks>
    /// <exception cref="NotSupportedException">同步方法不再受支持</exception>
    [Obsolete("此同步方法可能导致死锁，请使用 DirectoryExistsAsync 异步版本", true)]
    public override bool DirectoryExists(string directoryPath)
    {
        throw new NotSupportedException(
            "同步方法 DirectoryExists 已被移除以避免死锁。请使用 DirectoryExistsAsync 异步版本。" +
            "参考迁移指南: https://github.com/Linger06/Linger/blob/develop/src/Linger.FileSystem/README.zh-CN.md#迁移指南");
    }

    /// <summary>
    /// 创建目录（同步方法）
    /// </summary>
    /// <remarks>
    /// ⚠️ 此同步方法已被移除以避免死锁风险。
    /// 请使用 <see cref="CreateDirectoryIfNotExistsAsync"/> 异步版本。
    /// </remarks>
    /// <exception cref="NotSupportedException">同步方法不再受支持</exception>
    [Obsolete("此同步方法可能导致死锁，请使用 CreateDirectoryIfNotExistsAsync 异步版本", true)]
    public override void CreateDirectoryIfNotExists(string directoryPath)
    {
        throw new NotSupportedException(
            "同步方法 CreateDirectoryIfNotExists 已被移除以避免死锁。请使用 CreateDirectoryIfNotExistsAsync 异步版本。" +
            "参考迁移指南: https://github.com/Linger06/Linger/blob/develop/src/Linger.FileSystem/README.zh-CN.md#迁移指南");
    }

    /// <summary>
    /// 删除文件（同步方法）
    /// </summary>
    /// <remarks>
    /// ⚠️ 此同步方法已被移除以避免死锁风险。
    /// 请使用 <see cref="DeleteFileIfExistsAsync"/> 异步版本。
    /// </remarks>
    /// <exception cref="NotSupportedException">同步方法不再受支持</exception>
    [Obsolete("此同步方法可能导致死锁，请使用 DeleteFileIfExistsAsync 异步版本", true)]
    public override void DeleteFileIfExists(string filePath)
    {
        throw new NotSupportedException(
            "同步方法 DeleteFileIfExists 已被移除以避免死锁。请使用 DeleteFileIfExistsAsync 异步版本。" +
            "参考迁移指南: https://github.com/Linger06/Linger/blob/develop/src/Linger.FileSystem/README.zh-CN.md#迁移指南");
    }

    public override async Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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

    /// <summary>
    /// 将提供的 <see cref="Stream"/> 上传到 FTP 服务器指定路径。
    /// </summary>
    /// <param name="inputStream">源数据流, 必须支持读取。</param>
    /// <param name="destinationFilePath">目标文件路径, 使用 FTP 规范的正斜杠分隔符。</param>
    /// <param name="overwrite">当目标文件已存在时是否覆盖。</param>
    /// <param name="cancellationToken">用于取消上传操作的标记。</param>
    /// <returns>返回包含上传结果的 <see cref="FileOperationResult"/>。</returns>
    /// <example>
    /// <code>
    /// await using var stream = File.OpenRead("./assets/logo.png");
    /// var result = await ftpFileSystem.UploadAsync(stream, "/images/logo.png", overwrite: true);
    /// if (result.Success) { /* 处理成功 */ }
    /// </code>
    /// </example>
    public override async Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFilePath);

        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
        try
        {
            // 提取目录路径并确保目录存在
            // FTP路径始终使用正斜杠
            var remoteDirectory = destinationFilePath.Contains(FtpPathSeparator)
                ? destinationFilePath.Take(destinationFilePath.LastIndexOf(FtpPathSeparator))
                : string.Empty;

            if (!string.IsNullOrEmpty(remoteDirectory))
            {
                await CreateDirectoryIfNotExistsAsync(remoteDirectory, cancellationToken).ConfigureAwait(false);
            }

            // 执行上传
            var result = await RetryHelper.ExecuteAsync(
                async () =>
                {
                    inputStream.Position = 0;
                    var status = await Client.UploadStream(
                        inputStream,
                        destinationFilePath,
                        overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                        createRemoteDir: true,
                        token: cancellationToken).ConfigureAwait(false);

                    return status == FtpStatus.Success;
                },
                "Upload file", cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result)
                return FileOperationResult.CreateFailure($"上传文件失败: {destinationFilePath}");

            var fileSize = await TryGetFileSizeAsync(destinationFilePath, cancellationToken).ConfigureAwait(false);

            return FileOperationResult.CreateSuccess(destinationFilePath, null, fileSize);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Destination: {destinationFilePath}");
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 将本地文件上传到 FTP 服务器并返回操作结果。
    /// </summary>
    /// <param name="localFilePath">要上传的本地文件完整路径。</param>
    /// <param name="destinationFilePath">FTP 目标文件路径，包含文件名</param>
    /// <param name="overwrite">当目标文件已存在时是否覆盖。</param>
    /// <param name="cancellationToken">用于取消上传操作的标记。</param>
    /// <returns>返回包含上传结果的 <see cref="FileOperationResult"/>。</returns>
    /// <example>
    /// <code>
    /// var result = await ftpFileSystem.UploadFileAsync("C:/backup/data.json", "/remote/backup/data.json", overwrite: false);
    /// if (!result.Success) { logger.LogWarning(result.ErrorMessage); }
    /// </code>
    /// </example>
    public override async Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(localFilePath))
        {
            return FileOperationResult.CreateFailure($"本地文件不存在: {localFilePath}");
        }

        ArgumentException.ThrowIfNullOrEmpty(destinationFilePath);

        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
        try
        {
            var result = await RetryHelper.ExecuteAsync(
                async () =>
                {
                    var status = await Client.UploadFile(
                        localFilePath,
                        destinationFilePath,
                        overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                        createRemoteDir: true,
                        token: cancellationToken).ConfigureAwait(false);

                    return status == FtpStatus.Success;
                },
                "Upload file",
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result)
            {
                return FileOperationResult.CreateFailure($"上传文件失败: {destinationFilePath}");
            }

            var fileInfo = new FileInfo(localFilePath);
            return FileOperationResult.CreateSuccess(destinationFilePath, null, fileInfo.Length);
        }
        catch (Exception ex)
        {
            HandleException(
                "Upload file",
                ex,
                $"Local: {localFilePath}, FileName: {destinationFilePath}");
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 将本地文件上传到 FTP 服务器指定目录并使用自定义文件名保存。
    /// </summary>
    /// <param name="localFilePath">要上传的本地文件完整路径。</param>
    /// <param name="destinationDirectory">FTP 目标目录, 可为空或相对路径。</param>
    /// <param name="destinationFileName">保存到 FTP 的文件名。</param>
    /// <param name="overwrite">当目标文件已存在时是否覆盖。</param>
    /// <param name="cancellationToken">用于取消上传操作的标记。</param>
    /// <returns>返回包含上传结果的 <see cref="FileOperationResult"/>。</returns>
    /// <example>
    /// <code>
    /// var result = await ftpFileSystem.UploadFileAsync("./logs/app.log", "/archive", "app-20251028.log", overwrite: true);
    /// if (result.Success) { Console.WriteLine($"Uploaded to {result.RemotePath}"); }
    /// </code>
    /// </example>
    public async Task<FileOperationResult> UploadFileAsync(
        string localFilePath,
        string destinationDirectory,
        string destinationFileName,
        bool overwrite = false,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(localFilePath))
        {
            return FileOperationResult.CreateFailure($"本地文件不存在: {localFilePath}");
        }

        ArgumentException.ThrowIfNullOrEmpty(destinationFileName);

        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
        try
        {
            var sanitizedFileName = Path.GetFileName(destinationFileName);
            var remotePath = BuildRemoteFilePath(destinationDirectory, sanitizedFileName);

            var result = await RetryHelper.ExecuteAsync(
                async () =>
                {
                    var status = await Client.UploadFile(
                        localFilePath,
                        remotePath,
                        overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                        createRemoteDir: true,
                        token: cancellationToken).ConfigureAwait(false);

                    return status == FtpStatus.Success;
                },
                "Upload file",
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result)
            {
                return FileOperationResult.CreateFailure($"上传文件失败: {remotePath}");
            }

            var fileInfo = new FileInfo(localFilePath);
            return FileOperationResult.CreateSuccess(remotePath, null, fileInfo.Length);
        }
        catch (Exception ex)
        {
            HandleException(
                "Upload file",
                ex,
                $"Local: {localFilePath}, Directory: {destinationDirectory}, FileName: {destinationFileName}");
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteFilePath);
        ArgumentNullException.ThrowIfNull(outputStream);

        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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

            var fileSize = await TryGetFileSizeAsync(remoteFilePath, cancellationToken).ConfigureAwait(false);

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
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(localDestinationPath);

        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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

    #region 辅助方法

    /// <summary>
    /// 安全地获取文件大小,失败时返回 0
    /// </summary>
    private async Task<long> TryGetFileSizeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Client.GetFileSize(filePath, token: cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return 0;
        }
    }

    private static string BuildRemoteFilePath(string destinationDirectory, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        // 规范化文件名: 移除路径分隔符
        var sanitizedFileName = fileName.Replace('\\', FtpPathSeparator).Trim(FtpPathSeparator);
        if (string.IsNullOrWhiteSpace(sanitizedFileName))
        {
            throw new System.ArgumentException("File name cannot be empty after sanitization.", nameof(fileName));
        }

        // 如果目录为空,直接返回文件名
        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            return sanitizedFileName;
        }

        // 规范化目录路径
        var normalizedDirectory = destinationDirectory.Replace('\\', FtpPathSeparator).Trim();

        // 处理根目录的特殊情况
        return normalizedDirectory switch
        {
            "" => sanitizedFileName,
            FtpRootPath => $"{FtpRootPath}{sanitizedFileName}",
            _ => normalizedDirectory.EndsWith(FtpPathSeparator)
                ? $"{normalizedDirectory}{sanitizedFileName}"
                : $"{normalizedDirectory}{FtpPathSeparator}{sanitizedFileName}"
        };
    }
    #endregion
}

