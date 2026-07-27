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
    private const string Protocol = "FTP";
    private const char FtpPathSeparator = '/';
    private readonly FtpFileSystemOptions _options;

    /// <summary>
    /// FTP客户端
    /// </summary>
    protected AsyncFtpClient Client { get; }

    /// <summary>
    /// 初始化 <see cref="FtpFileSystem"/> 的新实例。
    /// </summary>
    /// <param name="options">FTP 服务器连接选项。</param>
    /// <param name="retryOptions">重试选项（可选）。</param>
    /// <param name="logger">日志记录器（可选）。</param>
    public FtpFileSystem(FtpFileSystemOptions options, RetryOptions? retryOptions = null, ILogger<FtpFileSystem>? logger = null)
        : base(options, Protocol, retryOptions, logger)
    {
        _options = options;
        Client = CreateClient();
        Logger.LogDebug("FtpFileSystem created for {Host}:{Port}", options.Host, options.Port);
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
            ConnectTimeout = Options.ConnectionTimeout,
            DataConnectionConnectTimeout = Options.ConnectionTimeout,
            DataConnectionReadTimeout = Options.OperationTimeout,
            ReadTimeout = Options.OperationTimeout,
            StaleDataCheck = true
        };

        // 创建AsyncFtpClient
        var client = new AsyncFtpClient(
            Options.Host,
            new NetworkCredential(Options.UserName, Options.Password),
            Options.Port)
        {
            Config = config,
            Encoding = _options.Encoding ?? Encoding.UTF8
        };

        return client;
    }

    #region 连接管理

    public override bool IsConnected() => Client.IsConnected;

    /// <inheritdoc />
    public override Task ConnectAsync()
    {
        return ConnectAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public override async Task ConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!Client.IsConnected)
            {
                Logger.LogInformation("Connecting to FTP server: {Host}:{Port}", Options.Host, Options.Port);
                await Client.AutoConnect(cancellationToken).ConfigureAwait(false);
                Logger.LogInformation("Connected to FTP server: {Host}:{Port}", Options.Host, Options.Port);
            }
        }
        catch (Exception ex)
        {
            HandleException("Connect", ex);
        }
    }

    public override async Task DisconnectAsync()
    {
        if (Client.IsConnected)
        {
            Logger.LogDebug("Disconnecting from FTP server: {Host}:{Port}", Options.Host, Options.Port);
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
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
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
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
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
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
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
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
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

        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
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
            var result = await ExecuteStreamOperationAsync(
                inputStream,
                async operationCancellationToken =>
                {
                    var status = await Client.UploadStream(
                        inputStream,
                        destinationFilePath,
                        overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                        createRemoteDir: true,
                        token: operationCancellationToken).ConfigureAwait(false);

                    return status == FtpStatus.Success;
                },
                "Upload file",
                restoreLength: false,
                cancellationToken: cancellationToken).ConfigureAwait(false);

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

        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var result = await RetryHelper.ExecuteAsync(
                async operationCancellationToken =>
                {
                    var status = await Client.UploadFile(
                        localFilePath,
                        destinationFilePath,
                        overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                        createRemoteDir: true,
                        token: operationCancellationToken).ConfigureAwait(false);

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

    public override async Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteFilePath);
        ArgumentNullException.ThrowIfNull(outputStream);

        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!await FileExistsAsync(remoteFilePath, cancellationToken).ConfigureAwait(false))
            {
                return FileOperationResult.CreateFailure($"文件不存在: {remoteFilePath}");
            }

            // 使用AsyncFtpClient执行下载
            var result = await ExecuteStreamOperationAsync(
                outputStream,
                async operationCancellationToken =>
                {
                    var status = await Client.DownloadStream(
                        outputStream,
                        remoteFilePath,
                        token: operationCancellationToken).ConfigureAwait(false);

                    return status;
                },
                "Download to stream",
                restoreLength: true,
                cancellationToken: cancellationToken).ConfigureAwait(false);

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

        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
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
                async operationCancellationToken =>
                {
                    var status = await Client.DownloadFile(
                        localDestinationPath,
                        remoteFilePath,
                        overwrite ? FtpLocalExists.Overwrite : FtpLocalExists.Skip,
                        token: operationCancellationToken).ConfigureAwait(false);

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
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!await FileExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
            {
                return FileOperationResult.CreateSuccess(filePath); // 文件不存在也视为成功
            }

            // 执行删除
            await RetryHelper.ExecuteAsync(
                async operationCancellationToken =>
                {
                    await Client.DeleteFile(filePath, operationCancellationToken).ConfigureAwait(false);
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
    public override async Task<DateTime> GetLastModifiedTimeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
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
    /// 设置工作目录
    /// </summary>
    public override async Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!await DirectoryExistsAsync(directoryPath, cancellationToken).ConfigureAwait(false))
            {
                throw new DirectoryNotFoundException($"Remote directory not found: {directoryPath}");
            }

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

        var tracker = new BatchOperationTracker(filePaths.Count, progress);

        async Task UploadFileAsync(AsyncFtpClient client, string localPath)
        {
            if (!File.Exists(localPath))
            {
                tracker.AddFailure(localPath, "本地文件不存在");
                return;
            }

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
                tracker.AddSuccess(localPath);
            }
            else
            {
                tracker.AddFailure(localPath, "上传失败");
            }
        }

        var degree = Options.MaxDegreeOfParallelism;
        if (degree <= 1)
        {
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
            foreach (var localPath in filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ExecuteBatchItemAsync(localPath, () => UploadFileAsync(Client, localPath), tracker).ConfigureAwait(false);
            }
        }
        else
        {
            await ExecuteInParallelAsync(
                filePaths,
                degree,
                UploadFileAsync,
                (localPath, ex) => tracker.AddFailure(localPath, ex.Message, ex),
                tracker.ReportCompleted,
                cancellationToken).ConfigureAwait(false);
        }

        var result = tracker.Complete();
        Logger.LogInformation("FTP Batch upload completed: {Succeeded} succeeded, {Failed} failed", result.SucceededFiles.Count, result.FailedFiles.Count);

        return result;
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

        var tracker = new BatchOperationTracker(filePaths.Count, progress);

        async Task DownloadFileAsync(AsyncFtpClient client, string remotePath)
        {
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
                tracker.AddSuccess(remotePath);
            }
            else
            {
                tracker.AddFailure(remotePath, "下载失败");
            }
        }

        var degree = Options.MaxDegreeOfParallelism;
        if (degree <= 1)
        {
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
            foreach (var remotePath in filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ExecuteBatchItemAsync(remotePath, () => DownloadFileAsync(Client, remotePath), tracker).ConfigureAwait(false);
            }
        }
        else
        {
            await ExecuteInParallelAsync(
                filePaths,
                degree,
                DownloadFileAsync,
                (remotePath, ex) => tracker.AddFailure(remotePath, ex.Message, ex),
                tracker.ReportCompleted,
                cancellationToken).ConfigureAwait(false);
        }

        var result = tracker.Complete();
        Logger.LogInformation("FTP Batch download completed: {Succeeded} succeeded, {Failed} failed", result.SucceededFiles.Count, result.FailedFiles.Count);

        return result;
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

        var tracker = new BatchOperationTracker(paths.Count, progress);

        async Task DeleteFileAsync(AsyncFtpClient client, string filePath)
        {
            await ExecuteWithBatchRetryAsync(async () =>
            {
                if (await client.FileExists(filePath, cancellationToken).ConfigureAwait(false))
                {
                    await client.DeleteFile(filePath, cancellationToken).ConfigureAwait(false);
                }
            }, cancellationToken).ConfigureAwait(false);

            tracker.AddSuccess(filePath);
        }

        var degree = Options.MaxDegreeOfParallelism;
        if (degree <= 1)
        {
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
            foreach (var filePath in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ExecuteBatchItemAsync(filePath, () => DeleteFileAsync(Client, filePath), tracker).ConfigureAwait(false);
            }
        }
        else
        {
            await ExecuteInParallelAsync(
                paths,
                degree,
                DeleteFileAsync,
                (filePath, ex) => tracker.AddFailure(filePath, ex.Message, ex),
                tracker.ReportCompleted,
                cancellationToken).ConfigureAwait(false);
        }

        var result = tracker.Complete();
        Logger.LogInformation("FTP Batch delete completed: {Succeeded} succeeded, {Failed} failed", result.SucceededFiles.Count, result.FailedFiles.Count);

        return result;
    }

    /// <summary>
    /// 列出目录中的文件
    /// </summary>
    public override async Task<IReadOnlyList<string>> ListFilesAsync(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
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
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
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

    private async Task ExecuteInParallelAsync(
        IReadOnlyCollection<string> filePaths,
        int degree,
        Func<AsyncFtpClient, string, Task> operation,
        Action<string, Exception> onError,
        Action<string> onCompleted,
        CancellationToken cancellationToken)
    {
        var queue = new ConcurrentQueue<string>(filePaths);
        var workerCount = Math.Min(degree, filePaths.Count);
        var workers = Enumerable.Range(0, workerCount).Select(async _ =>
        {
            var client = CreateClient();
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!queue.TryDequeue(out var filePath))
                    {
                        break;
                    }

                    var shouldReportCompletion = true;
                    try
                    {
                        if (!client.IsConnected)
                        {
                            await client.AutoConnect(cancellationToken).ConfigureAwait(false);
                        }

                        await operation(client, filePath).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        shouldReportCompletion = false;
                        throw;
                    }
                    catch (Exception ex)
                    {
                        onError(filePath, ex);
                    }
                    finally
                    {
                        if (shouldReportCompletion)
                        {
                            onCompleted(filePath);
                        }
                    }
                }
            }
            finally
            {
                if (client.IsConnected)
                {
                    try
                    {
                        await client.Disconnect().ConfigureAwait(false);
                    }
                    catch
                    {
                        // Cleanup must not hide the operation result.
                    }
                }

                client.Dispose();
            }
        });

        await Task.WhenAll(workers).ConfigureAwait(false);
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Unable to get FTP file size: {FilePath}", filePath);

            return 0;
        }
    }

    #endregion

    #region 流工厂与元数据方法

    /// <inheritdoc />
    public override async Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
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
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
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
    public override async Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!await FileExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            return await Client.GetFileSize(filePath, token: cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Unable to get FTP file size: {FilePath}", filePath);

            return null;
        }
    }

    #endregion
}
