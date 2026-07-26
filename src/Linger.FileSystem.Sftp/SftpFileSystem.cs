using Linger.Extensions.Core;
using Linger.FileSystem.Exceptions;
using Linger.FileSystem.Remote;
using Linger.Helper;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System.Collections.Concurrent;

namespace Linger.FileSystem.Sftp;

/// <summary>
/// SFTP文件系统实现
/// </summary>
/// <remarks>
/// <para>此实现基于 SSH.NET 库，提供完整的 SFTP 文件操作支持。</para>
/// <para>支持密码认证和私钥证书认证两种方式。</para>
/// </remarks>
public class SftpFileSystem : RemoteFileSystemBase
{
    private const string Protocol = "SFTP";
    private const char SftpPathSeparator = '/';
    private const string SftpRootPath = "/";
    private readonly SftpFileSystemOptions _options;

    /// <summary>
    /// SFTP客户端
    /// </summary>
    protected SftpClient Client { get; }
    private static readonly char[] s_separator = ['/', '\\'];

    /// <summary>
    /// 初始化 <see cref="SftpFileSystem"/> 的新实例。
    /// </summary>
    /// <param name="options">SFTP 服务器连接选项。</param>
    /// <param name="retryOptions">重试选项（可选）。</param>
    /// <param name="logger">日志记录器（可选）。</param>
    public SftpFileSystem(SftpFileSystemOptions options, RetryOptions? retryOptions = null, ILogger<SftpFileSystem>? logger = null)
        : base(options, Protocol, retryOptions, logger)
    {
        _options = options;
        Client = CreateClient();
        Logger.LogDebug("SftpFileSystem created for {Host}:{Port}, Auth: {AuthType}",
            options.Host,
            options.Port,
            options.CertificatePath.IsNotNullOrEmpty() ? "Certificate" : "Password");
    }

    private SftpClient CreateClient()
    {
        // 创建连接信息，考虑证书认证选项
        ConnectionInfo connectionInfo;

        if (_options.CertificatePath.IsNotNullOrEmpty())
        {
            var privateKeyFile = new PrivateKeyFile(_options.CertificatePath, _options.CertificatePassphrase);
            connectionInfo = new ConnectionInfo(
                Options.Host,
                Options.Port,
                Options.UserName,
                new PrivateKeyAuthenticationMethod(Options.UserName, privateKeyFile));
        }
        else
        {
            connectionInfo = new ConnectionInfo(
                Options.Host,
                Options.Port,
                Options.UserName,
                new PasswordAuthenticationMethod(Options.UserName, Options.Password));
        }

        // 设置超时
        connectionInfo.Timeout = TimeSpan.FromMilliseconds(Options.ConnectionTimeout);

        return new SftpClient(connectionInfo)
        {
            OperationTimeout = TimeSpan.FromMilliseconds(Options.OperationTimeout)
        };
    }

    #region 连接管理

    public override bool IsConnected() => Client?.IsConnected ?? false;

    /// <inheritdoc />
    public override Task ConnectAsync()
    {
        return ConnectAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public override async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (Client is { IsConnected: false })
        {
            Logger.LogInformation("Connecting to SFTP server: {Host}:{Port}", Options.Host, Options.Port);
            await Client.ConnectAsync(cancellationToken).ConfigureAwait(false);
            Logger.LogInformation("Connected to SFTP server: {Host}:{Port}", Options.Host, Options.Port);
        }
    }

    public override Task DisconnectAsync()
    {
        if (Client?.IsConnected == true)
        {
            Logger.LogDebug("Disconnecting from SFTP server: {Host}:{Port}", Options.Host, Options.Port);
            Client.Disconnect();
        }

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        if (Disposed)
            return;

        Client?.Dispose();

        Disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 异步释放 SFTP 客户端资源
    /// </summary>
    public override async ValueTask DisposeAsync()
    {
        if (Disposed)
            return;

        await DisconnectAsync().ConfigureAwait(false);
        Client?.Dispose();

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
            return await IsRegularFileAsync(Client, filePath, cancellationToken).ConfigureAwait(false);
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
            return await IsRemoteDirectoryAsync(Client, directoryPath, cancellationToken).ConfigureAwait(false);
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
            await EnsureDirectoryExistsAsync(Client, directoryPath, cancellationToken).ConfigureAwait(false);
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
            if (await IsRegularFileAsync(Client, filePath, cancellationToken).ConfigureAwait(false))
            {
                await Client.DeleteFileAsync(filePath, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            HandleException("Delete file", ex, filePath);
        }
    }

    #endregion

    #region 文件传输操作

    /// <summary>
    /// 将提供的 <see cref="Stream"/> 上传到 SFTP 服务器指定路径。
    /// </summary>
    /// <param name="inputStream">源数据流, 必须支持读取</param>
    /// <param name="destinationFilePath">目标文件路径, 使用 Unix 风格的正斜杠分隔符</param>
    /// <param name="overwrite">当目标文件已存在时是否覆盖</param>
    /// <param name="cancellationToken">用于取消上传操作的标记</param>
    /// <returns>返回包含上传结果的 <see cref="FileOperationResult"/>。</returns>
    /// <example>
    /// <code>
    /// await using var stream = File.OpenRead("./assets/logo.png");
    /// var result = await sftpFileSystem.UploadAsync(stream, "/images/logo.png", overwrite: true);
    /// if (result.Success) { /* 处理成功 */ }
    /// </code>
    /// </example>
    public override async Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFilePath);

        Logger.LogDebug("SFTP Upload starting: {Destination}, Overwrite: {Overwrite}", destinationFilePath, overwrite);

        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // 提取目录路径
            var remoteDirectory = GetSftpDirectoryPath(destinationFilePath);

            // 确保目录存在
            if (!string.IsNullOrEmpty(remoteDirectory))
            {
                await CreateDirectoryIfNotExistsAsync(remoteDirectory, cancellationToken).ConfigureAwait(false);
            }

            // 检查文件是否存在
            if (await FileExistsAsync(destinationFilePath, cancellationToken).ConfigureAwait(false) && !overwrite)
            {
                Logger.LogWarning("SFTP Upload failed - file already exists: {Destination}", destinationFilePath);
                return FileOperationResult.CreateFailure($"远程文件已存在 {destinationFilePath}");
            }

            // 执行上传
            await ExecuteStreamOperationAsync(
                inputStream,
                async operationCancellationToken =>
                {
                    if (await IsRegularFileAsync(Client, destinationFilePath, operationCancellationToken).ConfigureAwait(false) && overwrite)
                    {
                        await Client.DeleteFileAsync(destinationFilePath, operationCancellationToken).ConfigureAwait(false);
                    }

                    await Client.UploadFileAsync(inputStream, destinationFilePath, operationCancellationToken).ConfigureAwait(false);
                    return true;
                },
                "Upload file",
                restoreLength: false,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            // 获取文件大小
            var fileSize = await TryGetFileSizeAsync(destinationFilePath, cancellationToken).ConfigureAwait(false);
            Logger.LogInformation("SFTP Upload completed: {Destination}, Size: {Size} bytes", destinationFilePath, fileSize);

            return FileOperationResult.CreateSuccess(destinationFilePath, null, fileSize);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Destination: {destinationFilePath}");
            return default; // 不会执行，HandleException 始终抛出异常
        }
    }

    /// <summary>
    /// 将本地文件上传到 SFTP 服务器并返回操作结果。
    /// </summary>
    /// <param name="localFilePath">要上传的本地文件完整路径</param>
    /// <param name="destinationFilePath">SFTP 目标文件路径，包含文件名</param>
    /// <param name="overwrite">当目标文件已存在时是否覆盖</param>
    /// <param name="cancellationToken">用于取消上传操作的标记</param>
    /// <returns>返回包含上传结果的 <see cref="FileOperationResult"/>。</returns>
    /// <example>
    /// <code>
    /// var result = await sftpFileSystem.UploadFileAsync("C:/backup/data.json", "/remote/backup/data.json", overwrite: false);
    /// if (!result.Success) { logger.LogWarning(result.ErrorMessage); }
    /// </code>
    /// </example>
    public override async Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFilePath);

        if (!File.Exists(localFilePath))
            return FileOperationResult.CreateFailure($"本地文件不存在 {localFilePath}");

        try
        {
            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return await UploadAsync(fileStream, destinationFilePath, overwrite, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Local: {localFilePath}, Destination: {destinationFilePath}");
            return default; // 不会执行，HandleException 始终抛出异常
        }
    }

    /// <summary>
    /// 从 SFTP 服务器下载文件到流
    /// </summary>
    /// <param name="remoteFilePath">SFTP 服务器上文件的路径</param>
    /// <param name="outputStream">目标输出</param>
    /// <param name="cancellationToken">用于取消操作的令牌</param>
    /// <returns>返回包含下载结果?<see cref="FileOperationResult"/></returns>
    public override async Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteFilePath);
        ArgumentNullException.ThrowIfNull(outputStream);

        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!await FileExistsAsync(remoteFilePath, cancellationToken).ConfigureAwait(false))
                return FileOperationResult.CreateFailure($"文件不存在 {remoteFilePath}");

            await ExecuteStreamOperationAsync(
                outputStream,
                async operationCancellationToken =>
                {
                    await Client.DownloadFileAsync(remoteFilePath, outputStream, operationCancellationToken).ConfigureAwait(false);
                    return true;
                },
                "Download to stream",
                restoreLength: true,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            // 获取文件大小
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
    /// 从 SFTP 服务器下载文件到本地文件系统
    /// </summary>
    /// <param name="remoteFilePath">SFTP 服务器上文件的完整路径或相对路径</param>
    /// <param name="localDestinationPath">本地文件系统的完整路径或相对路径</param>
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
                return FileOperationResult.CreateFailure($"文件不存在 {remoteFilePath}");

            // 确保目标目录存在
            var destDir = Path.GetDirectoryName(localDestinationPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            // 检查文件是否已存在
            if (File.Exists(localDestinationPath) && !overwrite)
                return FileOperationResult.CreateFailure($"目标文件已存在 {localDestinationPath}");

            await RetryHelper.ExecuteAsync(
                async operationCancellationToken =>
                {
                    using var fileStream = new FileStream(
                        localDestinationPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        4096,
                        FileOptions.Asynchronous | FileOptions.SequentialScan);
                    await Client.DownloadFileAsync(remoteFilePath, fileStream, operationCancellationToken).ConfigureAwait(false);

                    return true;
                },
                "Download file", cancellationToken: cancellationToken).ConfigureAwait(false);

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
                return FileOperationResult.CreateSuccess(filePath); // 文件不存在也视为成功

            await Client.DeleteFileAsync(filePath, cancellationToken).ConfigureAwait(false);
            return FileOperationResult.CreateSuccess(filePath);
        }
        catch (Exception ex)
        {
            HandleException("Delete file", ex, filePath);
            return default; // 不会执行，HandleException 始终抛出异常
        }
    }

    #endregion

    #region SFTP特有功能

    /// <summary>
    /// 异步获取文件修改时间
    /// </summary>
    public override async Task<DateTime> GetLastModifiedTimeAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var attributes = await Client.GetAttributesAsync(remotePath, cancellationToken).ConfigureAwait(false);

            return attributes.LastWriteTime;
        }
        catch (Exception ex)
        {
            HandleException("Get last modified time", ex, remotePath);
            return DateTime.MinValue; // 不会执行，HandleException 始终抛出异常
        }
    }

    /// <summary>
    /// 异步获取目录文件列表
    /// </summary>
    public override async Task<IReadOnlyList<string>> ListFilesAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!await IsRemoteDirectoryAsync(Client, directoryPath, cancellationToken).ConfigureAwait(false))
            {
                return [];
            }

            var files = new List<string>();
            await foreach (var file in Client.ListDirectoryAsync(directoryPath, cancellationToken).ConfigureAwait(false))
            {
                if (!file.IsDirectory && !file.Name.StartsWith('.'))
                {
                    files.Add(file.Name);
                }
            }

            return files;
        }
        catch (Exception ex)
        {
            HandleException("List files", ex, directoryPath);
            return []; // 不会执行，HandleException 始终抛出异常
        }
    }

    /// <summary>
    /// 异步获取子目录列表
    /// </summary>
    public override async Task<IReadOnlyList<string>> ListDirectoriesAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!await IsRemoteDirectoryAsync(Client, directoryPath, cancellationToken).ConfigureAwait(false))
            {
                return [];
            }

            var directories = new List<string>();
            await foreach (var file in Client.ListDirectoryAsync(directoryPath, cancellationToken).ConfigureAwait(false))
            {
                if (file.IsDirectory && !file.Name.StartsWith('.') && file.Name is not "." and not "..")
                {
                    directories.Add(file.Name);
                }
            }

            return directories;
        }
        catch (Exception ex)
        {
            HandleException("List directories", ex, directoryPath);
            return []; // 不会执行，HandleException 始终抛出异常
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

        Logger.LogDebug("SFTP Batch upload starting: {Count} files to {Directory}", filePaths.Count, remoteDirectory);

        // 先确保远程目录存在（使用主连接串行执行一次）
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        await CreateDirectoryIfNotExistsAsync(remoteDirectory, cancellationToken).ConfigureAwait(false);

        var tracker = new BatchOperationTracker(filePaths.Count, progress);

        async Task UploadFileAsync(SftpClient client, string localPath)
        {
            if (!File.Exists(localPath))
            {
                tracker.AddFailure(localPath, "本地文件不存在");
                return;
            }

            var fileName = Path.GetFileName(localPath);
            var remotePath = $"{remoteDirectory.TrimEnd(SftpPathSeparator)}{SftpPathSeparator}{fileName}";
            await ExecuteWithBatchRetryAsync(async () =>
            {
                var exists = await IsRegularFileAsync(client, remotePath, cancellationToken).ConfigureAwait(false);
                if (exists && !overwrite)
                {
                    throw new InvalidOperationException($"远程文件已存在: {remotePath}");
                }

                if (exists)
                {
                    await client.DeleteFileAsync(remotePath, cancellationToken).ConfigureAwait(false);
                }

                using var fileStream = new FileStream(
                    localPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    4096,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
                await client.UploadFileAsync(fileStream, remotePath, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            tracker.AddSuccess(localPath);
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
        Logger.LogInformation("SFTP Batch upload completed: {Succeeded} succeeded, {Failed} failed", result.SucceededFiles.Count, result.FailedFiles.Count);

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

        Logger.LogDebug("SFTP Batch download starting: {Count} files to {Directory}", filePaths.Count, localDirectory);

        if (!Directory.Exists(localDirectory))
            Directory.CreateDirectory(localDirectory);

        var tracker = new BatchOperationTracker(filePaths.Count, progress);

        async Task DownloadFileAsync(SftpClient client, string remotePath)
        {
            var fileName = Path.GetFileName(remotePath);
            var localPath = Path.Combine(localDirectory, fileName);
            if (File.Exists(localPath) && !overwrite)
            {
                tracker.AddFailure(remotePath, $"本地文件已存在: {localPath}");
                return;
            }

            await ExecuteWithBatchRetryAsync(async () =>
            {
                if (!await IsRegularFileAsync(client, remotePath, cancellationToken).ConfigureAwait(false))
                {
                    throw new FileNotFoundException("远程文件不存在", remotePath);
                }

                using var fileStream = new FileStream(
                    localPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
                await client.DownloadFileAsync(remotePath, fileStream, cancellationToken).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            tracker.AddSuccess(remotePath);
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
        Logger.LogInformation("SFTP Batch download completed: {Succeeded} succeeded, {Failed} failed", result.SucceededFiles.Count, result.FailedFiles.Count);

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

        Logger.LogDebug("SFTP Batch delete starting: {Count} files", paths.Count);

        var tracker = new BatchOperationTracker(paths.Count, progress);

        async Task DeleteFileAsync(SftpClient client, string filePath)
        {
            await ExecuteWithBatchRetryAsync(async () =>
            {
                if (await IsRegularFileAsync(client, filePath, cancellationToken).ConfigureAwait(false))
                {
                    await client.DeleteFileAsync(filePath, cancellationToken).ConfigureAwait(false);
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
        Logger.LogInformation("SFTP Batch delete completed: {Succeeded} succeeded, {Failed} failed", result.SucceededFiles.Count, result.FailedFiles.Count);

        return result;
    }

    /// <summary>
    /// 异步设置工作目录
    /// </summary>
    public override async Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await Client.ChangeDirectoryAsync(directoryPath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Set working directory", ex, directoryPath);
        }
    }

    #endregion

    #region 辅助方法

    private async Task ExecuteInParallelAsync(
        IReadOnlyCollection<string> filePaths,
        int degree,
        Func<SftpClient, string, Task> operation,
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
                            await client.ConnectAsync(cancellationToken).ConfigureAwait(false);
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
                        client.Disconnect();
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
    /// 从 SFTP 文件路径中提取目录路径
    /// </summary>
    /// <param name="filePath">完整的文件路径</param>
    /// <returns>目录路径，如果没有目录则返回空字符串</returns>
    private static string GetSftpDirectoryPath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        // SFTP 路径使用正斜杠，类似 Unix
        var lastSlashIndex = filePath.LastIndexOf(SftpPathSeparator);
        return lastSlashIndex > 0 ? filePath.Substring(0, lastSlashIndex) : string.Empty;
    }

    private static async Task<bool> IsRegularFileAsync(SftpClient client, string filePath, CancellationToken cancellationToken)
    {
        if (!await client.ExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        var attributes = await client.GetAttributesAsync(filePath, cancellationToken).ConfigureAwait(false);

        return attributes.IsRegularFile;
    }

    private static async Task<bool> IsRemoteDirectoryAsync(SftpClient client, string directoryPath, CancellationToken cancellationToken)
    {
        if (!await client.ExistsAsync(directoryPath, cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        var attributes = await client.GetAttributesAsync(directoryPath, cancellationToken).ConfigureAwait(false);

        return attributes.IsDirectory;
    }

    /// <summary>
    /// 安全地获取文件大小，失败时返回 0
    /// </summary>
    private async Task<long> TryGetFileSizeAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var attributes = await Client.GetAttributesAsync(filePath, cancellationToken).ConfigureAwait(false);

            return attributes.Size;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Unable to get SFTP file size: {FilePath}", filePath);

            return 0;
        }
    }

    /// <summary>
    /// 构建远程文件路径
    /// </summary>
    /// <param name="destinationDirectory">目标目录</param>
    /// <param name="fileName">文件</param>
    /// <returns>完整的远程文件路径</returns>
    private static string BuildRemoteFilePath(string destinationDirectory, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        // 规范化文件名：移除路径分隔符
        var sanitizedFileName = fileName.Replace('\\', SftpPathSeparator).Trim(SftpPathSeparator);
        if (string.IsNullOrWhiteSpace(sanitizedFileName))
        {
            throw new ArgumentException("File name cannot be empty after sanitization.", nameof(fileName));
        }

        // 如果目录为空，直接返回文件名
        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            return sanitizedFileName;
        }

        // 规范化目录路径
        var normalizedDirectory = destinationDirectory.Replace('\\', SftpPathSeparator).Trim();

        // 处理根目录的特殊情况
        return normalizedDirectory switch
        {
            "" => sanitizedFileName,
            SftpRootPath => $"{SftpRootPath}{sanitizedFileName}",
            _ => normalizedDirectory.EndsWith(SftpPathSeparator)
                ? $"{normalizedDirectory}{sanitizedFileName}"
                : $"{normalizedDirectory}{SftpPathSeparator}{sanitizedFileName}"
        };
    }

    #endregion

    #region 流工厂与元数据方法

    /// <inheritdoc />
    public override async Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!await IsRegularFileAsync(Client, filePath, cancellationToken).ConfigureAwait(false))
            {
                throw new FileNotFoundException("Remote file not found", filePath);
            }

            return await Client.OpenAsync(filePath, FileMode.Open, FileAccess.Read, cancellationToken).ConfigureAwait(false);
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
            if (!overwrite && await IsRegularFileAsync(Client, filePath, cancellationToken).ConfigureAwait(false))
            {
                throw new FileSystemException("Open file for writing", filePath, $"File already exists: {filePath}");
            }

            var directory = GetSftpDirectoryPath(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                await EnsureDirectoryExistsAsync(Client, directory, cancellationToken).ConfigureAwait(false);
            }

            var fileMode = overwrite ? FileMode.Create : FileMode.CreateNew;

            return await Client.OpenAsync(filePath, fileMode, FileAccess.Write, cancellationToken).ConfigureAwait(false);
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

    private static async Task EnsureDirectoryExistsAsync(SftpClient client, string directoryPath, CancellationToken cancellationToken)
    {
        if (await IsRemoteDirectoryAsync(client, directoryPath, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var paths = directoryPath.Split(s_separator, StringSplitOptions.RemoveEmptyEntries);
        var currentPath = string.Empty;

        foreach (var path in paths)
        {
            currentPath += SftpPathSeparator + path;

            if (!await IsRemoteDirectoryAsync(client, currentPath, cancellationToken).ConfigureAwait(false))
            {
                await client.CreateDirectoryAsync(currentPath, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public override async Task<StreamReader> GetReaderAsync(string filePath, System.Text.Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        var stream = await OpenReadAsync(filePath, cancellationToken).ConfigureAwait(false);
#if NET6_0_OR_GREATER
        return new StreamReader(stream, encoding ?? System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
#else
        return new StreamReader(stream, encoding ?? System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false);
#endif
    }

    /// <inheritdoc />
    public override async Task<StreamWriter> GetWriterAsync(string filePath, bool overwrite = false, System.Text.Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        var stream = await OpenWriteAsync(filePath, overwrite, cancellationToken).ConfigureAwait(false);
#if NET6_0_OR_GREATER
        return new StreamWriter(stream, encoding ?? System.Text.Encoding.UTF8, leaveOpen: false);
#else
        return new StreamWriter(stream, encoding ?? System.Text.Encoding.UTF8, bufferSize: 1024, leaveOpen: false);
#endif
    }

    /// <inheritdoc />
    public override async Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!await Client.ExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            var attributes = await Client.GetAttributesAsync(filePath, cancellationToken).ConfigureAwait(false);

            return attributes.IsRegularFile ? attributes.Size : null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Unable to get SFTP file size: {FilePath}", filePath);

            return null;
        }
    }

    #endregion
}

