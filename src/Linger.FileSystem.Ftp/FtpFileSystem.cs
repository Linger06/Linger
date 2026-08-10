using System.Net;
using System.Text;
using FluentFTP;
using FluentFTP.Exceptions;
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

            var result = await ExecuteStreamOperationAsync(
                inputStream,
                async operationCancellationToken =>
                {
                    return await UploadStreamAtomicallyAsync(
                        Client,
                        inputStream,
                        destinationFilePath,
                        overwrite,
                        operationCancellationToken).ConfigureAwait(false);
                },
                "Upload file",
                restoreLength: false,
                shouldRetry: IsBatchRetryableException,
                shouldRetryResult: success => !success,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!result)
            {
                return FileOperationResult.CreateFailure($"上传文件失败: {destinationFilePath}");
            }

            Logger.LogInformation("FTP Upload completed: {Destination}", destinationFilePath);

            return FileOperationResult.CreateSuccess(destinationFilePath);
        }
        catch (DuplicateFileException ex)
        {
            return FileOperationResult.CreateFailure($"远程文件已存在: {destinationFilePath}", ex);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Destination: {destinationFilePath}");
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

            var result = await ExecuteStreamOperationAsync(
                outputStream,
                async operationCancellationToken =>
                {
                    return await Client.DownloadStream(
                        outputStream,
                        remoteFilePath,
                        token: operationCancellationToken).ConfigureAwait(false);
                },
                "Download to stream",
                restoreLength: true,
                shouldRetry: IsBatchRetryableException,
                shouldRetryResult: success => !success,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!result)
                return FileOperationResult.CreateFailure($"下载文件到流失败: {remoteFilePath}");

            return FileOperationResult.CreateSuccess(remoteFilePath);
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

            var result = await DownloadFileAtomicallyAsync(
                localDestinationPath,
                overwrite,
                async (temporaryPath, operationCancellationToken) =>
                {
                    var status = await Client.DownloadFile(
                        temporaryPath,
                        remoteFilePath,
                        FtpLocalExists.Overwrite,
                        token: operationCancellationToken).ConfigureAwait(false);

                    return status == FtpStatus.Success;
                },
                useBatchRetry: false,
                cancellationToken).ConfigureAwait(false);

            if (!result)
                return FileOperationResult.CreateFailure($"下载文件失败: {remoteFilePath}");

            return FileOperationResult.CreateSuccess(remoteFilePath);
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
                "Delete file",
                IsBatchRetryableException,
                cancellationToken: cancellationToken).ConfigureAwait(false);

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

        var duplicateResult = CreateDuplicateTargetResult(
            filePaths,
            localPath => $"{remoteDirectory.TrimEnd(FtpPathSeparator)}{FtpPathSeparator}{Path.GetFileName(localPath)}",
            StringComparer.Ordinal,
            progress);
        if (duplicateResult is not null)
        {
            return duplicateResult;
        }

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
            using var fileStream = new FileStream(
                localPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var success = await ExecuteWithBatchRetryAsync(async () =>
            {
                fileStream.Position = 0;
                return await UploadStreamAtomicallyAsync(
                    client,
                    fileStream,
                    remotePath,
                    overwrite,
                    cancellationToken).ConfigureAwait(false);
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

        var duplicateResult = CreateDuplicateTargetResult(
            filePaths,
            remotePath => Path.Combine(localDirectory, Path.GetFileName(remotePath)),
            Path.DirectorySeparatorChar == '\\' ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal,
            progress);
        if (duplicateResult is not null)
        {
            return duplicateResult;
        }

        var tracker = new BatchOperationTracker(filePaths.Count, progress);

        async Task DownloadFileAsync(AsyncFtpClient client, string remotePath)
        {
            var fileName = Path.GetFileName(remotePath);
            var localPath = Path.Combine(localDirectory, fileName);
            var success = await DownloadFileAtomicallyAsync(
                localPath,
                overwrite,
                async (temporaryPath, operationCancellationToken) =>
                {
                    var status = await client.DownloadFile(
                        temporaryPath,
                        remotePath,
                        FtpLocalExists.Overwrite,
                        token: operationCancellationToken).ConfigureAwait(false);

                    return status == FtpStatus.Success;
                },
                useBatchRetry: true,
                cancellationToken).ConfigureAwait(false);

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
        CancellationToken cancellationToken)
    {
        await ExecuteParallelBatchAsync(
            filePaths,
            degree,
            CreateClient,
            async (client, filePath, operationCancellationToken) =>
            {
                if (!client.IsConnected)
                {
                    await client.AutoConnect(operationCancellationToken).ConfigureAwait(false);
                }

                await operation(client, filePath).ConfigureAwait(false);
            },
            DisposeBatchClientAsync,
            onError,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task DisposeBatchClientAsync(AsyncFtpClient client)
    {
        if (client.IsConnected)
        {
            try
            {
                await client.Disconnect().ConfigureAwait(false);
            }
            catch (FtpException ex)
            {
                Logger.LogWarning(ex, "Failed to disconnect FTP batch client.");
            }
            catch (IOException ex)
            {
                Logger.LogWarning(ex, "Failed to disconnect FTP batch client.");
            }
        }

        client.Dispose();
    }

    private async Task<long> GetRequiredFileSizeAsync(string filePath, CancellationToken cancellationToken)
    {
        var fileSize = await Client.GetFileSize(
            filePath,
            defaultValue: -1,
            token: cancellationToken).ConfigureAwait(false);
        if (fileSize < 0)
        {
            throw new IOException($"Unable to get FTP file size: {filePath}");
        }

        return fileSize;
    }

    private async Task<bool> UploadStreamAtomicallyAsync(
        AsyncFtpClient client,
        Stream inputStream,
        string destinationFilePath,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        var separatorIndex = destinationFilePath.LastIndexOf(FtpPathSeparator);
        var remoteDirectory = separatorIndex >= 0
            ? destinationFilePath.Substring(0, separatorIndex + 1)
            : string.Empty;
        var temporaryPath = $"{remoteDirectory}.upload-{Guid.NewGuid():N}.tmp";

        try
        {
            var status = await client.UploadStream(
                inputStream,
                temporaryPath,
                FtpRemoteExists.Overwrite,
                createRemoteDir: true,
                token: cancellationToken).ConfigureAwait(false);
            if (status != FtpStatus.Success)
            {
                return false;
            }

            var moved = await client.MoveFile(
                temporaryPath,
                destinationFilePath,
                overwrite ? FtpRemoteExists.Overwrite : FtpRemoteExists.Skip,
                cancellationToken).ConfigureAwait(false);
            if (!moved)
            {
                if (!overwrite && await client.FileExists(destinationFilePath, cancellationToken).ConfigureAwait(false))
                {
                    throw new DuplicateFileException(destinationFilePath);
                }

                return false;
            }

            return true;
        }
        finally
        {
            await TryDeleteTemporaryFileAsync(client, temporaryPath, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task TryDeleteTemporaryFileAsync(
        AsyncFtpClient client,
        string temporaryPath,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await client.FileExists(temporaryPath, cancellationToken).ConfigureAwait(false))
            {
                await client.DeleteFile(temporaryPath, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (FtpException ex)
        {
            Logger.LogWarning(ex, "Failed to clean up temporary FTP upload: {FilePath}", temporaryPath);
        }
        catch (IOException ex)
        {
            Logger.LogWarning(ex, "Failed to clean up temporary FTP upload: {FilePath}", temporaryPath);
        }
    }

    /// <inheritdoc />
    protected override bool IsBatchRetryableException(Exception exception)
    {
        if (exception is FtpCommandException commandException)
        {
            return commandException.CompletionCode.StartsWith("4", StringComparison.Ordinal);
        }

        return base.IsBatchRetryableException(exception);
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
    public override async Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!await FileExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            return await GetRequiredFileSizeAsync(filePath, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (FileSystemException)
        {
            throw;
        }
        catch (Exception ex)
        {
            HandleException("Get file size", ex, filePath);
            return null; // 不会执行，HandleException 始终抛出异常
        }
    }

    #endregion
}
