using System.Collections.Concurrent;
using System.Net;
using System.Text;
using FluentFTP;
using Linger.Extensions.Core;
using Linger.FileSystem.Exceptions;
using Linger.FileSystem.Remote;
using Linger.Helper;
using Microsoft.Extensions.Logging;

namespace Linger.FileSystem.Ftp;

/// <summary>
/// FTP文件系统实现
/// </summary>
/// <remarks>
/// <para>此实现基于 FluentFTP 库，提供完整的 FTP 文件操作支持。</para>
/// <para>支持的功能包括：文件上传/下载、目录操作、批量操作等。</para>
/// </remarks>
public class FtpFileSystem : RemoteFileSystemBase
{
    private const char FtpPathSeparator = '/';
    private const string FtpRootPath = "/";

    /// <summary>
    /// FTP客户端
    /// </summary>
    protected AsyncFtpClient Client { get; }

    /// <summary>
    /// 初始化 <see cref="FtpFileSystem"/> 的新实例。
    /// </summary>
    /// <param name="setting">远程服务器连接设置。</param>
    /// <param name="retryOptions">重试选项（可选）。</param>
    /// <param name="logger">日志记录器（可选）。</param>
    public FtpFileSystem(RemoteSystemSetting setting, RetryOptions? retryOptions = null, ILogger<FtpFileSystem>? logger = null)
        : base(setting, retryOptions, logger)
    {
        Client = CreateClient();
        Logger.LogDebug("FtpFileSystem created for {Host}:{Port}", setting.Host, setting.Port);
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
        {
            Logger.LogInformation("Connecting to FTP server: {Host}:{Port}", Setting.Host, Setting.Port);
            await Client.AutoConnect().ConfigureAwait(false);
            Logger.LogInformation("Connected to FTP server: {Host}:{Port}", Setting.Host, Setting.Port);
        }
    }

    public override async Task DisconnectAsync()
    {
        if (Client.IsConnected)
        {
            Logger.LogDebug("Disconnecting from FTP server: {Host}:{Port}", Setting.Host, Setting.Port);
            await Client.Disconnect().ConfigureAwait(false);
        }
    }

    public override void Dispose()
    {
        if (Disposed)
            return;

        if (!Client.IsDisposed)
            Client.Dispose();

        Disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 异步释放 FTP 客户端资源
    /// </summary>
    public override async ValueTask DisposeAsync()
    {
        if (Disposed)
            return;

        await DisconnectAsync().ConfigureAwait(false);

        if (!Client.IsDisposed)
            Client.Dispose();

        Disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 文件操作基本方法

    public override async Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            return await Client.FileExists(filePath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Check file exists", ex, filePath);
            return false; // 不会执行，HandleException 始终抛出异常
        }
    }

    public override async Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            return await Client.DirectoryExists(directoryPath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Check directory exists", ex, directoryPath);
            return false; // 不会执行，HandleException 始终抛出异常
        }
    }

    public override async Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
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
        await EnsureConnectedAsync().ConfigureAwait(false);
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

        Logger.LogDebug("FTP Upload starting: {Destination}, Overwrite: {Overwrite}", destinationFilePath, overwrite);

        await EnsureConnectedAsync().ConfigureAwait(false);
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

            // 检查文件是否已存在（overwrite=false 时明确报错）
            if (!overwrite && await FileExistsAsync(destinationFilePath, cancellationToken).ConfigureAwait(false))
            {
                return FileOperationResult.CreateFailure($"远程文件已存在: {destinationFilePath}");
            }

            // 执行上传
            var result = await RetryHelper.ExecuteAsync(
                async () =>
                {
                    if (inputStream.CanSeek)
                    {
                        inputStream.Position = 0;
                    }

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
            {
                Logger.LogWarning("FTP Upload failed: {Destination}", destinationFilePath);
                return FileOperationResult.CreateFailure($"上传文件失败: {destinationFilePath}");
            }

            var fileSize = await TryGetFileSizeAsync(destinationFilePath, cancellationToken).ConfigureAwait(false);
            Logger.LogInformation("FTP Upload completed: {Destination}, Size: {Size} bytes", destinationFilePath, fileSize);

            return FileOperationResult.CreateSuccess(destinationFilePath, null, fileSize);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Destination: {destinationFilePath}");
            return default; // 不会执行，HandleException 始终抛出异常
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

        await EnsureConnectedAsync().ConfigureAwait(false);
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
            return default; // 不会执行，HandleException 始终抛出异常
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

        await EnsureConnectedAsync().ConfigureAwait(false);
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
            return default; // 不会执行，HandleException 始终抛出异常
        }
    }

    public override async Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteFilePath);
        ArgumentNullException.ThrowIfNull(outputStream);

        await EnsureConnectedAsync().ConfigureAwait(false);
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
            return default; // 不会执行，HandleException 始终抛出异常
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

        await EnsureConnectedAsync().ConfigureAwait(false);
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
            return default; // 不会执行，HandleException 始终抛出异常
        }
    }

    public override async Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
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
            return default; // 不会执行，HandleException 始终抛出异常
        }
    }

    #endregion

    #region 其他FTP特定功能

    /// <summary>
    /// 获取文件最后修改时间
    /// </summary>
    public async Task<DateTime> GetLastModifiedTimeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            return await Client.GetModifiedTime(filePath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Get last modified time", ex, filePath);
            return DateTime.MinValue; // 不会执行，HandleException 始终抛出异常
        }
    }

    /// <summary>
    /// 列出目录内容
    /// </summary>
    public async Task<IEnumerable<string>> ListDirectoryAsync(string? directoryPath = null, FtpObjectType type = FtpObjectType.File, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            FtpListItem[] items = directoryPath is null ?
                await Client.GetListing(token: cancellationToken).ConfigureAwait(false) :
                await Client.GetListing(directoryPath, token: cancellationToken).ConfigureAwait(false);

            return items.Where(f => f.Type == type)
                       .Select(f => f.Name);
        }
        catch (Exception ex)
        {
            HandleException("List directory", ex, directoryPath);
            return []; // 不会执行，HandleException 始终抛出异常
        }
    }

    /// <summary>
    /// 设置工作目录
    /// </summary>
    public async Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
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
    public override async Task<BatchOperationResult> UploadFilesAsync(
        IEnumerable<string> localFilePaths,
        string remoteDirectory,
        bool overwrite = false,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var filePaths = localFilePaths.ToList();
        if (filePaths.Count == 0)
            return BatchOperationResult.Empty;

        Logger.LogDebug("FTP Batch upload starting: {Count} files to {Directory}", filePaths.Count, remoteDirectory);

        var succeeded = new ConcurrentBag<string>();
        var failed = new ConcurrentBag<BatchOperationFailure>();
        var total = filePaths.Count;
        var completed = 0;

        var degree = Setting.MaxDegreeOfParallelism;
        if (degree <= 1)
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
            foreach (var localPath in filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!File.Exists(localPath))
                {
                    failed.Add(new BatchOperationFailure(localPath, "本地文件不存在"));
                    var c1 = Interlocked.Increment(ref completed);
                    progress?.Report(new BatchProgress(c1, total, localPath, succeeded.Count, failed.Count));
                    continue;
                }

                try
                {
                    var fileName = Path.GetFileName(localPath);
                    var remotePath = $"{remoteDirectory.TrimEnd(FtpPathSeparator)}{FtpPathSeparator}{fileName}";

                    var success = await ExecuteWithBatchRetryAsync(async () =>
                    {
                        var status = await Client.UploadFile(
                            localPath,
                            remotePath,
                            overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                            createRemoteDir: true,
                            token: cancellationToken).ConfigureAwait(false);
                        return status == FtpStatus.Success;
                    }, cancellationToken).ConfigureAwait(false);

                    if (success)
                    {
                        succeeded.Add(localPath);
                    }
                    else
                    {
                        failed.Add(new BatchOperationFailure(localPath, "上传失败"));
                    }
                }
                catch (Exception ex)
                {
                    failed.Add(new BatchOperationFailure(localPath, ex.Message, ex));
                }

                var c2 = Interlocked.Increment(ref completed);
                progress?.Report(new BatchProgress(c2, total, localPath, succeeded.Count, failed.Count));
            }
        }
        else
        {
            await using var pool = CreateConnectionPool(degree);
            var tasks = filePaths.Select(async localPath =>
            {
                AsyncFtpClient? client = null;
                try
                {
                    if (!File.Exists(localPath))
                    {
                        failed.Add(new BatchOperationFailure(localPath, "本地文件不存在"));
                        return;
                    }

                    client = await pool.RentAsync(cancellationToken).ConfigureAwait(false);

                    var fileName = Path.GetFileName(localPath);
                    var remotePath = $"{remoteDirectory.TrimEnd(FtpPathSeparator)}{FtpPathSeparator}{fileName}";

                    var success = await ExecuteWithBatchRetryAsync(async () =>
                    {
                        var status = await client.UploadFile(
                            localPath,
                            remotePath,
                            overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                            createRemoteDir: true,
                            token: cancellationToken).ConfigureAwait(false);
                        return status == FtpStatus.Success;
                    }, cancellationToken).ConfigureAwait(false);

                    if (success)
                    {
                        succeeded.Add(localPath);
                    }
                    else
                    {
                        failed.Add(new BatchOperationFailure(localPath, "上传失败"));
                    }
                }
                catch (Exception ex)
                {
                    failed.Add(new BatchOperationFailure(localPath, ex.Message, ex));
                }
                finally
                {
                    if (client is not null)
                    {
                        pool.Return(client);
                    }

                    var currentCompleted = Interlocked.Increment(ref completed);
                    progress?.Report(new BatchProgress(currentCompleted, total, localPath, succeeded.Count, failed.Count));
                }
            }).ToArray();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        // 最终进度报告
        progress?.Report(new BatchProgress(total, total, string.Empty, succeeded.Count, failed.Count));

        var succeededList = succeeded.ToList();
        var failedList = failed.ToList();

        Logger.LogInformation("FTP Batch upload completed: {Succeeded} succeeded, {Failed} failed", succeededList.Count, failedList.Count);

        return new BatchOperationResult
        {
            SucceededFiles = succeededList,
            FailedFiles = failedList
        };
    }

    /// <summary>
    /// 批量下载文件
    /// </summary>
    public override async Task<BatchOperationResult> DownloadFilesAsync(
        IEnumerable<string> remoteFilePaths,
        string localDirectory,
        bool overwrite = false,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var filePaths = remoteFilePaths.ToList();
        if (filePaths.Count == 0)
            return BatchOperationResult.Empty;

        Logger.LogDebug("FTP Batch download starting: {Count} files to {Directory}", filePaths.Count, localDirectory);

        if (!Directory.Exists(localDirectory))
            Directory.CreateDirectory(localDirectory);

        var succeeded = new ConcurrentBag<string>();
        var failed = new ConcurrentBag<BatchOperationFailure>();
        var total = filePaths.Count;
        var completed = 0;

        var degree = Setting.MaxDegreeOfParallelism;
        if (degree <= 1)
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
            foreach (var remotePath in filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var fileName = Path.GetFileName(remotePath);
                    var localPath = Path.Combine(localDirectory, fileName);

                    var success = await ExecuteWithBatchRetryAsync(async () =>
                    {
                        var status = await Client.DownloadFile(
                            localPath,
                            remotePath,
                            overwrite ? FtpLocalExists.Overwrite : FtpLocalExists.Skip,
                            token: cancellationToken).ConfigureAwait(false);
                        return status == FtpStatus.Success;
                    }, cancellationToken).ConfigureAwait(false);

                    if (success)
                    {
                        succeeded.Add(remotePath);
                    }
                    else
                    {
                        failed.Add(new BatchOperationFailure(remotePath, "下载失败"));
                    }
                }
                catch (Exception ex)
                {
                    failed.Add(new BatchOperationFailure(remotePath, ex.Message, ex));
                }

                var c = Interlocked.Increment(ref completed);
                progress?.Report(new BatchProgress(c, total, remotePath, succeeded.Count, failed.Count));
            }
        }
        else
        {
            await using var pool = CreateConnectionPool(degree);
            var tasks = filePaths.Select(async remotePath =>
            {
                AsyncFtpClient? client = null;
                try
                {
                    client = await pool.RentAsync(cancellationToken).ConfigureAwait(false);

                    var fileName = Path.GetFileName(remotePath);
                    var localPath = Path.Combine(localDirectory, fileName);

                    var success = await ExecuteWithBatchRetryAsync(async () =>
                    {
                        var status = await client.DownloadFile(
                            localPath,
                            remotePath,
                            overwrite ? FtpLocalExists.Overwrite : FtpLocalExists.Skip,
                            token: cancellationToken).ConfigureAwait(false);
                        return status == FtpStatus.Success;
                    }, cancellationToken).ConfigureAwait(false);

                    if (success)
                    {
                        succeeded.Add(remotePath);
                    }
                    else
                    {
                        failed.Add(new BatchOperationFailure(remotePath, "下载失败"));
                    }
                }
                catch (Exception ex)
                {
                    failed.Add(new BatchOperationFailure(remotePath, ex.Message, ex));
                }
                finally
                {
                    if (client is not null)
                    {
                        pool.Return(client);
                    }

                    var currentCompleted = Interlocked.Increment(ref completed);
                    progress?.Report(new BatchProgress(currentCompleted, total, remotePath, succeeded.Count, failed.Count));
                }
            }).ToArray();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        progress?.Report(new BatchProgress(total, total, string.Empty, succeeded.Count, failed.Count));

        var succeededList = succeeded.ToList();
        var failedList = failed.ToList();

        Logger.LogInformation("FTP Batch download completed: {Succeeded} succeeded, {Failed} failed", succeededList.Count, failedList.Count);

        return new BatchOperationResult
        {
            SucceededFiles = succeededList,
            FailedFiles = failedList
        };
    }

    /// <summary>
    /// 批量删除文件
    /// </summary>
    public override async Task<BatchOperationResult> DeleteFilesAsync(
        IEnumerable<string> filePaths,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var paths = filePaths.ToList();
        if (paths.Count == 0)
            return BatchOperationResult.Empty;

        Logger.LogDebug("FTP Batch delete starting: {Count} files", paths.Count);

        var succeeded = new ConcurrentBag<string>();
        var failed = new ConcurrentBag<BatchOperationFailure>();
        var total = paths.Count;
        var completed = 0;

        var degree = Setting.MaxDegreeOfParallelism;
        if (degree <= 1)
        {
            await EnsureConnectedAsync().ConfigureAwait(false);
            foreach (var filePath in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await ExecuteWithBatchRetryAsync(async () =>
                    {
                        if (await Client.FileExists(filePath, cancellationToken).ConfigureAwait(false))
                        {
                            await Client.DeleteFile(filePath, cancellationToken).ConfigureAwait(false);
                        }
                    }, cancellationToken).ConfigureAwait(false);
                    succeeded.Add(filePath);
                }
                catch (Exception ex)
                {
                    failed.Add(new BatchOperationFailure(filePath, ex.Message, ex));
                }

                var c = Interlocked.Increment(ref completed);
                progress?.Report(new BatchProgress(c, total, filePath, succeeded.Count, failed.Count));
            }
        }
        else
        {
            await using var pool = CreateConnectionPool(degree);
            var tasks = paths.Select(async filePath =>
            {
                AsyncFtpClient? client = null;
                try
                {
                    client = await pool.RentAsync(cancellationToken).ConfigureAwait(false);

                    await ExecuteWithBatchRetryAsync(async () =>
                    {
                        if (await client.FileExists(filePath, cancellationToken).ConfigureAwait(false))
                        {
                            await client.DeleteFile(filePath, cancellationToken).ConfigureAwait(false);
                        }
                    }, cancellationToken).ConfigureAwait(false);
                    succeeded.Add(filePath);
                }
                catch (Exception ex)
                {
                    failed.Add(new BatchOperationFailure(filePath, ex.Message, ex));
                }
                finally
                {
                    if (client is not null)
                    {
                        pool.Return(client);
                    }

                    var currentCompleted = Interlocked.Increment(ref completed);
                    progress?.Report(new BatchProgress(currentCompleted, total, filePath, succeeded.Count, failed.Count));
                }
            }).ToArray();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        progress?.Report(new BatchProgress(total, total, string.Empty, succeeded.Count, failed.Count));

        var succeededList = succeeded.ToList();
        var failedList = failed.ToList();

        Logger.LogInformation("FTP Batch delete completed: {Succeeded} succeeded, {Failed} failed", succeededList.Count, failedList.Count);

        return new BatchOperationResult
        {
            SucceededFiles = succeededList,
            FailedFiles = failedList
        };
    }

    /// <summary>
    /// 列出目录中的文件
    /// </summary>
    public override async Task<IReadOnlyList<string>> ListFilesAsync(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            var items = await Client.GetListing(directoryPath, token: cancellationToken).ConfigureAwait(false);
            return items
                .Where(f => f.Type == FtpObjectType.File)
                .Select(f => f.Name)
                .ToList();
        }
        catch (Exception ex)
        {
            HandleException("List files", ex, directoryPath);
            return []; // 不会执行，HandleException 始终抛出异常
        }
    }

    /// <summary>
    /// 列出目录中的子目录
    /// </summary>
    public override async Task<IReadOnlyList<string>> ListDirectoriesAsync(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            var items = await Client.GetListing(directoryPath, token: cancellationToken).ConfigureAwait(false);
            return items
                .Where(f => f.Type == FtpObjectType.Directory)
                .Select(f => f.Name)
                .ToList();
        }
        catch (Exception ex)
        {
            HandleException("List directories", ex, directoryPath);
            return []; // 不会执行，HandleException 始终抛出异常
        }
    }
    #endregion

    #region 辅助方法

    /// <summary>
    /// 创建 FTP 连接池，用于批量操作中复用连接
    /// </summary>
    /// <param name="poolSize">池大小，通常与 MaxDegreeOfParallelism 一致</param>
    /// <returns>FTP 连接池</returns>
    private ConnectionPool<AsyncFtpClient> CreateConnectionPool(int poolSize)
    {
        return new ConnectionPool<AsyncFtpClient>(
            poolSize,
            factory: async ct =>
            {
                var client = CreateClient();
                await client.AutoConnect(ct).ConfigureAwait(false);

                return client;
            },
            healthCheck: c => c.IsConnected,
            disposeAsync: async c =>
            {
                try
                {
                    await c.Disconnect().ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }

                c.Dispose();
            },
            maxIdleTime: Setting.ConnectionPoolIdleTimeout);
    }

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
            throw new ArgumentException("File name cannot be empty after sanitization.", nameof(fileName));
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

    #region 流工厂与元数据方法

    /// <inheritdoc />
    public override async Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            if (!await FileExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
            {
                throw new FileNotFoundException("Remote file not found", filePath);
            }

            return await Client.OpenRead(filePath, token: cancellationToken).ConfigureAwait(false);
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            HandleException("Open file for reading", ex, filePath);
            throw;
        }
    }

    /// <inheritdoc />
    public override async Task<Stream> OpenWriteAsync(string filePath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            if (!overwrite && await FileExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
            {
                throw new FileSystemException("Open file for writing", filePath, $"File already exists: {filePath}");
            }

            // 确保目录存在
            var directory = filePath.Contains(FtpPathSeparator)
                ? filePath.Take(filePath.LastIndexOf(FtpPathSeparator))
                : string.Empty;

            if (!string.IsNullOrEmpty(directory))
            {
                await CreateDirectoryIfNotExistsAsync(directory, cancellationToken).ConfigureAwait(false);
            }

            return await Client.OpenWrite(filePath, token: cancellationToken).ConfigureAwait(false);
        }
        catch (FileSystemException)
        {
            throw;
        }
        catch (Exception ex)
        {
            HandleException("Open file for writing", ex, filePath);
            throw;
        }
    }

    /// <inheritdoc />
    public override async Task<StreamReader> GetReaderAsync(string filePath, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        var stream = await OpenReadAsync(filePath, cancellationToken).ConfigureAwait(false);
#if NET6_0_OR_GREATER
        return new StreamReader(stream, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
#else
        return new StreamReader(stream, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false);
#endif
    }

    /// <inheritdoc />
    public override async Task<StreamWriter> GetWriterAsync(string filePath, bool overwrite = false, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        var stream = await OpenWriteAsync(filePath, overwrite, cancellationToken).ConfigureAwait(false);
#if NET6_0_OR_GREATER
        return new StreamWriter(stream, encoding ?? Encoding.UTF8, leaveOpen: false);
#else
        return new StreamWriter(stream, encoding ?? Encoding.UTF8, bufferSize: 1024, leaveOpen: false);
#endif
    }

    /// <inheritdoc />
    public override async Task<bool> IsDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        return await DirectoryExistsAsync(directoryPath, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            if (!await FileExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            return await Client.GetFileSize(filePath, token: cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
