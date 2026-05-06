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
    private const char SftpPathSeparator = '/';
    private const string SftpRootPath = "/";

    /// <summary>
    /// SFTP客户端
    /// </summary>
    protected SftpClient Client { get; }
    private static readonly char[] s_separator = ['/', '\\'];

    /// <summary>
    /// 初始化 <see cref="SftpFileSystem"/> 的新实例。
    /// </summary>
    /// <param name="setting">远程服务器连接设置。</param>
    /// <param name="retryOptions">重试选项（可选）。</param>
    /// <param name="logger">日志记录器（可选）。</param>
    public SftpFileSystem(RemoteSystemSetting setting, RetryOptions? retryOptions = null, ILogger<SftpFileSystem>? logger = null)
        : base(setting, retryOptions, logger)
    {
        Client = CreateClient();
        Logger.LogDebug("SftpFileSystem created for {Host}:{Port}, Auth: {AuthType}",
            setting.Host,
            setting.Port,
            setting.CertificatePath.IsNotNullOrEmpty() ? "Certificate" : "Password");
    }

    private SftpClient CreateClient()
    {
        // 创建连接信息，考虑证书认证选项
        ConnectionInfo connectionInfo;

        if (Setting.CertificatePath.IsNotNullOrEmpty())
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

    public void Connect()
    {
        if (Client is { IsConnected: false })
        {
            Logger.LogInformation("Connecting to SFTP server: {Host}:{Port}", Setting.Host, Setting.Port);
            Client.Connect();
            Logger.LogInformation("Connected to SFTP server: {Host}:{Port}", Setting.Host, Setting.Port);
        }
    }

    public override Task ConnectAsync()
    {
        Connect();
        return Task.CompletedTask;
    }

    public void Disconnect()
    {
        if (Client?.IsConnected == true)
        {
            Logger.LogDebug("Disconnecting from SFTP server: {Host}:{Port}", Setting.Host, Setting.Port);
            Client.Disconnect();
        }
    }

    public override Task DisconnectAsync()
    {
        Disconnect();
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
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            return await Task.Run(() => Client.Exists(filePath) && Client.GetAttributes(filePath).IsRegularFile, cancellationToken).ConfigureAwait(false);
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
            return await Task.Run(() => Client.Exists(directoryPath) && Client.GetAttributes(directoryPath).IsDirectory, cancellationToken).ConfigureAwait(false);
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
            await Task.Run(() =>
            {
                if (Client.Exists(directoryPath) && Client.GetAttributes(directoryPath).IsDirectory)
                    return;

                // SFTP可能需要递归创建目录
                var paths = directoryPath.Split(s_separator, StringSplitOptions.RemoveEmptyEntries);
                var currentPath = string.Empty;

                foreach (var path in paths)
                {
                    currentPath += SftpPathSeparator + path;

                    if (!Client.Exists(currentPath) || !Client.GetAttributes(currentPath).IsDirectory)
                        Client.CreateDirectory(currentPath);
                }
            }, cancellationToken).ConfigureAwait(false);
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
            await Task.Run(() =>
            {
                if (Client.Exists(filePath) && Client.GetAttributes(filePath).IsRegularFile)
                    Client.DeleteFile(filePath);
            }, cancellationToken).ConfigureAwait(false);
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

        await EnsureConnectedAsync().ConfigureAwait(false);
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
            await RetryHelper.ExecuteAsync(
                async () =>
                {
                    await Task.Run(() =>
                    {
                        // 确保流位置在开始
                        if (inputStream.CanSeek)
                        {
                            inputStream.Position = 0;
                        }

                        if (Client.Exists(destinationFilePath) && overwrite)
                            Client.DeleteFile(destinationFilePath);

                        Client.UploadFile(inputStream, destinationFilePath);
                        return true;
                    }, cancellationToken).ConfigureAwait(false);
                    return true;
                },
                "Upload file", cancellationToken: cancellationToken).ConfigureAwait(false);

            // 获取文件大小
            var fileSize = TryGetFileSize(destinationFilePath);
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
    /// 将本地文件上传到 SFTP 服务器指定目录并使用自定义文件名保存
    /// </summary>
    /// <param name="localFilePath">要上传的本地文件完整路径</param>
    /// <param name="destinationDirectory">SFTP 目标目录, 可为空或相对路径</param>
    /// <param name="destinationFileName">保存在SFTP 的文件名</param>
    /// <param name="overwrite">当目标文件已存在时是否覆盖</param>
    /// <param name="cancellationToken">用于取消上传操作的标记</param>
    /// <returns>返回包含上传结果的 <see cref="FileOperationResult"/>。</returns>
    /// <example>
    /// <code>
    /// var result = await sftpFileSystem.UploadFileAsync("./logs/app.log", "/archive", "app-20251028.log", overwrite: true);
    /// if (result.Success) { Console.WriteLine($"Uploaded to {result.FilePath}"); }
    /// </code>
    /// </example>
    public async Task<FileOperationResult> UploadFileAsync(
        string localFilePath,
        string destinationDirectory,
        string destinationFileName,
        bool overwrite = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFileName);

        if (!File.Exists(localFilePath))
        {
            return FileOperationResult.CreateFailure($"本地文件不存在 {localFilePath}");
        }

        // 构建完整的远程文件路径
        var sanitizedFileName = Path.GetFileName(destinationFileName);
        var remotePath = BuildRemoteFilePath(destinationDirectory, sanitizedFileName);

        try
        {
            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return await UploadAsync(fileStream, remotePath, overwrite, cancellationToken).ConfigureAwait(false);
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

        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            if (!await FileExistsAsync(remoteFilePath, cancellationToken).ConfigureAwait(false))
                return FileOperationResult.CreateFailure($"文件不存在 {remoteFilePath}");

            await RetryHelper.ExecuteAsync(
                async () =>
                {
                    await Task.Run(() =>
                    {
                        Client.DownloadFile(remoteFilePath, outputStream);
                        return true;
                    }, cancellationToken).ConfigureAwait(false);
                    return true;
                },
                "Download to stream", cancellationToken: cancellationToken).ConfigureAwait(false);

            // 获取文件大小
            var fileSize = TryGetFileSize(remoteFilePath);

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

        await EnsureConnectedAsync().ConfigureAwait(false);
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
                async () =>
                {
                    await Task.Run(() =>
                    {
                        if (File.Exists(localDestinationPath) && overwrite)
                            File.Delete(localDestinationPath);

                        using var fileStream = File.Create(localDestinationPath);
                        Client.DownloadFile(remoteFilePath, fileStream);
                        return true;
                    }, cancellationToken).ConfigureAwait(false);
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
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            if (!await FileExistsAsync(filePath, cancellationToken).ConfigureAwait(false))
                return FileOperationResult.CreateSuccess(filePath); // 文件不存在也视为成功

            await Task.Run(() => Client.DeleteFile(filePath), cancellationToken).ConfigureAwait(false);
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
    public async Task<DateTime> GetLastModifiedTimeAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Client.GetLastWriteTime(remotePath);
            }, cancellationToken).ConfigureAwait(false);
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
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!Client.Exists(directoryPath) || !Client.GetAttributes(directoryPath).IsDirectory)
                    return (IReadOnlyList<string>)[];

                return (IReadOnlyList<string>)Client.ListDirectory(directoryPath)
                    .Where(file => !file.IsDirectory && !file.Name.StartsWith('.'))
                    .Select(file => file.Name)
                    .ToList();
            }, cancellationToken).ConfigureAwait(false);
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
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!Client.Exists(directoryPath) || !Client.GetAttributes(directoryPath).IsDirectory)
                    return (IReadOnlyList<string>)[];

                return (IReadOnlyList<string>)Client.ListDirectory(directoryPath)
                    .Where(file => file.IsDirectory && !file.Name.StartsWith('.') && file.Name != "." && file.Name != "..")
                    .Select(file => file.Name)
                    .ToList();
            }, cancellationToken).ConfigureAwait(false);
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
        await EnsureConnectedAsync().ConfigureAwait(false);
        await CreateDirectoryIfNotExistsAsync(remoteDirectory, cancellationToken).ConfigureAwait(false);

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
                    var remotePath = $"{remoteDirectory.TrimEnd(SftpPathSeparator)}{SftpPathSeparator}{fileName}";

                    await ExecuteWithBatchRetryAsync(async () =>
                    {
                        await Task.Run(() =>
                        {
                            if (Client.Exists(remotePath) && !overwrite)
                            {
                                throw new InvalidOperationException($"远程文件已存在: {remotePath}");
                            }

                            if (Client.Exists(remotePath) && overwrite)
                            {
                                Client.DeleteFile(remotePath);
                            }

                            using var fileStream = File.OpenRead(localPath);
                            Client.UploadFile(fileStream, remotePath);
                        }, cancellationToken).ConfigureAwait(false);
                        return true;
                    }, cancellationToken).ConfigureAwait(false);

                    succeeded.Add(localPath);
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
            var tasks = filePaths.Select(localPath => Task.Run(async () =>
            {
                SftpClient? client = null;
                try
                {
                    if (!File.Exists(localPath))
                    {
                        failed.Add(new BatchOperationFailure(localPath, "本地文件不存在"));
                        return;
                    }

                    client = await pool.RentAsync(cancellationToken).ConfigureAwait(false);

                    var fileName = Path.GetFileName(localPath);
                    var remotePath = $"{remoteDirectory.TrimEnd(SftpPathSeparator)}{SftpPathSeparator}{fileName}";

                    var currentClient = client;
                    await ExecuteWithBatchRetryAsync(async () =>
                    {
                        await Task.Run(() =>
                        {
                            if (currentClient.Exists(remotePath) && !overwrite)
                            {
                                throw new InvalidOperationException($"远程文件已存在: {remotePath}");
                            }
                            if (currentClient.Exists(remotePath) && overwrite)
                            {
                                currentClient.DeleteFile(remotePath);
                            }

                            using var fileStream = File.OpenRead(localPath);
                            currentClient.UploadFile(fileStream, remotePath);
                        }, cancellationToken).ConfigureAwait(false);
                        return true;
                    }, cancellationToken).ConfigureAwait(false);

                    succeeded.Add(localPath);
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
            }, cancellationToken)).ToArray();
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        progress?.Report(new BatchProgress(total, total, string.Empty, succeeded.Count, failed.Count));

        var succeededList = succeeded.ToList();
        var failedList = failed.ToList();

        Logger.LogInformation("SFTP Batch upload completed: {Succeeded} succeeded, {Failed} failed", succeededList.Count, failedList.Count);

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

        Logger.LogDebug("SFTP Batch download starting: {Count} files to {Directory}", filePaths.Count, localDirectory);

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

                    if (File.Exists(localPath) && !overwrite)
                    {
                        failed.Add(new BatchOperationFailure(remotePath, $"本地文件已存在: {localPath}"));
                        var c1 = Interlocked.Increment(ref completed);
                        progress?.Report(new BatchProgress(c1, total, remotePath, succeeded.Count, failed.Count));
                        continue;
                    }

                    await ExecuteWithBatchRetryAsync(async () =>
                    {
                        await Task.Run(() =>
                        {
                            if (!Client.Exists(remotePath) || !Client.GetAttributes(remotePath).IsRegularFile)
                            {
                                throw new FileNotFoundException("远程文件不存在", remotePath);
                            }

                            using var fileStream = File.Create(localPath);
                            Client.DownloadFile(remotePath, fileStream);
                        }, cancellationToken).ConfigureAwait(false);
                        return true;
                    }, cancellationToken).ConfigureAwait(false);

                    succeeded.Add(remotePath);
                }
                catch (Exception ex)
                {
                    failed.Add(new BatchOperationFailure(remotePath, ex.Message, ex));
                }

                var c2 = Interlocked.Increment(ref completed);
                progress?.Report(new BatchProgress(c2, total, remotePath, succeeded.Count, failed.Count));
            }
        }
        else
        {
            await using var pool = CreateConnectionPool(degree);
            var tasks = filePaths.Select(remotePath => Task.Run(async () =>
            {
                SftpClient? client = null;
                try
                {
                    var fileName = Path.GetFileName(remotePath);
                    var localPath = Path.Combine(localDirectory, fileName);

                    if (File.Exists(localPath) && !overwrite)
                    {
                        failed.Add(new BatchOperationFailure(remotePath, $"本地文件已存在: {localPath}"));
                        return;
                    }

                    client = await pool.RentAsync(cancellationToken).ConfigureAwait(false);

                    var currentClient = client;
                    await ExecuteWithBatchRetryAsync(async () =>
                    {
                        await Task.Run(() =>
                        {
                            if (!currentClient.Exists(remotePath) || !currentClient.GetAttributes(remotePath).IsRegularFile)
                            {
                                throw new FileNotFoundException("远程文件不存在", remotePath);
                            }

                            using var fileStream = File.Create(localPath);
                            currentClient.DownloadFile(remotePath, fileStream);
                        }, cancellationToken).ConfigureAwait(false);
                        return true;
                    }, cancellationToken).ConfigureAwait(false);

                    succeeded.Add(remotePath);
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
            }, cancellationToken)).ToArray();
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        progress?.Report(new BatchProgress(total, total, string.Empty, succeeded.Count, failed.Count));

        var succeededList = succeeded.ToList();
        var failedList = failed.ToList();

        Logger.LogInformation("SFTP Batch download completed: {Succeeded} succeeded, {Failed} failed", succeededList.Count, failedList.Count);

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

        Logger.LogDebug("SFTP Batch delete starting: {Count} files", paths.Count);

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
                        await Task.Run(() =>
                        {
                            if (Client.Exists(filePath) && Client.GetAttributes(filePath).IsRegularFile)
                            {
                                Client.DeleteFile(filePath);
                            }
                        }, cancellationToken).ConfigureAwait(false);
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
            var tasks = paths.Select(filePath => Task.Run(async () =>
            {
                SftpClient? client = null;
                try
                {
                    client = await pool.RentAsync(cancellationToken).ConfigureAwait(false);

                    var currentClient = client;
                    await ExecuteWithBatchRetryAsync(async () =>
                    {
                        await Task.Run(() =>
                        {
                            if (currentClient.Exists(filePath) && currentClient.GetAttributes(filePath).IsRegularFile)
                            {
                                currentClient.DeleteFile(filePath);
                            }
                        }, cancellationToken).ConfigureAwait(false);
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
            }, cancellationToken)).ToArray();
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        progress?.Report(new BatchProgress(total, total, string.Empty, succeeded.Count, failed.Count));

        var succeededList = succeeded.ToList();
        var failedList = failed.ToList();

        Logger.LogInformation("SFTP Batch delete completed: {Succeeded} succeeded, {Failed} failed", succeededList.Count, failedList.Count);

        return new BatchOperationResult
        {
            SucceededFiles = succeededList,
            FailedFiles = failedList
        };
    }

    /// <summary>
    /// 异步设置工作目录
    /// </summary>
    public async Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Client.ChangeDirectory(directoryPath);
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Set working directory", ex, directoryPath);
        }
    }

    /// <summary>
    /// 异步设置根目录为工作目录
    /// </summary>
    public Task SetRootAsWorkingDirectoryAsync(CancellationToken cancellationToken = default)
    {
        return SetWorkingDirectoryAsync(SftpRootPath, cancellationToken);
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 创建 SFTP 连接池，用于批量操作中复用连接
    /// </summary>
    /// <param name="poolSize">池大小，通常与 MaxDegreeOfParallelism 一致</param>
    /// <returns>SFTP 连接池</returns>
    private ConnectionPool<SftpClient> CreateConnectionPool(int poolSize)
    {
        return new ConnectionPool<SftpClient>(
            poolSize,
            factory: ct =>
            {
                ct.ThrowIfCancellationRequested();
                var client = CreateClient();
                client.Connect();

                return Task.FromResult(client);
            },
            healthCheck: c => c.IsConnected,
            disposeSync: c =>
            {
                try
                {
                    c.Disconnect();
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

    /// <summary>
    /// 安全地获取文件大小，失败时返回 0
    /// </summary>
    private long TryGetFileSize(string filePath)
    {
        try
        {
            return Client.GetAttributes(filePath).Size;
        }
        catch
        {
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
        await EnsureConnectedAsync().ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                if (!Client.Exists(filePath) || !Client.GetAttributes(filePath).IsRegularFile)
                {
                    throw new FileNotFoundException("Remote file not found", filePath);
                }

                return Client.OpenRead(filePath);
            }, cancellationToken).ConfigureAwait(false);
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
            return await Task.Run(() =>
            {
                if (!overwrite && Client.Exists(filePath) && Client.GetAttributes(filePath).IsRegularFile)
                {
                    throw new FileSystemException("Open file for writing", filePath, $"File already exists: {filePath}");
                }

                // 确保目录存在
                var directory = GetSftpDirectoryPath(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    EnsureDirectoryExists(directory);
                }

                return Client.OpenWrite(filePath);
            }, cancellationToken).ConfigureAwait(false);
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

    private void EnsureDirectoryExists(string directoryPath)
    {
        if (Client.Exists(directoryPath) && Client.GetAttributes(directoryPath).IsDirectory)
            return;

        var paths = directoryPath.Split(s_separator, StringSplitOptions.RemoveEmptyEntries);
        var currentPath = string.Empty;

        foreach (var path in paths)
        {
            currentPath += SftpPathSeparator + path;

            if (!Client.Exists(currentPath) || !Client.GetAttributes(currentPath).IsDirectory)
                Client.CreateDirectory(currentPath);
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
            return await Task.Run(() =>
            {
                if (!Client.Exists(filePath) || !Client.GetAttributes(filePath).IsRegularFile)
                {
                    return (long?)null;
                }

                return Client.GetAttributes(filePath).Size;
            }, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    #endregion
}

