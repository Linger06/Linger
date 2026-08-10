using Linger.Extensions.Core;
using Linger.FileSystem.Exceptions;
using Linger.FileSystem.Remote;
using Linger.Helper;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;

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

    protected override bool IsConnected() => Client?.IsConnected ?? false;

    /// <inheritdoc />
    protected override async Task ConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (Client is { IsConnected: false })
            {
                Logger.LogInformation("Connecting to SFTP server: {Host}:{Port}", Options.Host, Options.Port);
                await Client.ConnectAsync(cancellationToken).ConfigureAwait(false);
                Logger.LogInformation("Connected to SFTP server: {Host}:{Port}", Options.Host, Options.Port);
            }
        }
        catch (Exception ex)
        {
            HandleException("Connect", ex);
        }
    }

    protected override Task DisconnectAsync()
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

            await ExecuteStreamOperationAsync(
                inputStream,
                async operationCancellationToken =>
                {
                    await UploadStreamAtomicallyAsync(
                        Client,
                        inputStream,
                        destinationFilePath,
                        overwrite,
                        operationCancellationToken).ConfigureAwait(false);

                    return true;
                },
                "Upload file",
                restoreLength: false,
                shouldRetry: IsRetryableException,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            Logger.LogInformation("SFTP Upload completed: {Destination}", destinationFilePath);

            return FileOperationResult.CreateSuccess(destinationFilePath);
        }
        catch (DuplicateFileException ex)
        {
            return FileOperationResult.CreateFailure($"远程文件已存在 {destinationFilePath}", ex);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Destination: {destinationFilePath}");
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
                shouldRetry: IsRetryableException,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return FileOperationResult.CreateSuccess(remoteFilePath);
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

            await DownloadFileAtomicallyAsync(
                localDestinationPath,
                overwrite,
                (temporaryPath, operationCancellationToken) => DownloadToTemporaryFileAsync(
                    Client,
                    remoteFilePath,
                    temporaryPath,
                    operationCancellationToken),
                cancellationToken).ConfigureAwait(false);

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
                if (!file.IsDirectory)
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
                if (file.IsDirectory && file.Name is not "." and not "..")
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
    /// 异步设置工作目录
    /// </summary>
    public override async Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

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

    private async Task<long> GetRequiredFileSizeAsync(string filePath, CancellationToken cancellationToken)
    {
        var attributes = await Client.GetAttributesAsync(filePath, cancellationToken).ConfigureAwait(false);

        return attributes.Size;
    }

    private async Task UploadStreamAtomicallyAsync(
        SftpClient client,
        Stream inputStream,
        string destinationFilePath,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        var remoteDirectory = GetSftpDirectoryPath(destinationFilePath);
        var temporaryPath = string.IsNullOrEmpty(remoteDirectory)
            ? $".upload-{Guid.NewGuid():N}.tmp"
            : $"{remoteDirectory}{SftpPathSeparator}.upload-{Guid.NewGuid():N}.tmp";

        try
        {
            await client.UploadFileAsync(inputStream, temporaryPath, cancellationToken).ConfigureAwait(false);
            var destinationExists = await IsRegularFileAsync(
                client,
                destinationFilePath,
                cancellationToken).ConfigureAwait(false);
            if (destinationExists && !overwrite)
            {
                throw new DuplicateFileException(destinationFilePath);
            }

            if (destinationExists)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // SSH.NET 尚无异步 POSIX rename；单次元数据调用可在覆盖时保留原子提交语义。
                client.RenameFile(temporaryPath, destinationFilePath, isPosix: true);
            }
            else
            {
                await client.RenameFileAsync(
                    temporaryPath,
                    destinationFilePath,
                    cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            await TryDeleteTemporaryFileAsync(client, temporaryPath, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task TryDeleteTemporaryFileAsync(
        SftpClient client,
        string temporaryPath,
        CancellationToken cancellationToken)
    {
        try
        {
            if (await client.ExistsAsync(temporaryPath, cancellationToken).ConfigureAwait(false))
            {
                await client.DeleteFileAsync(temporaryPath, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (SshException ex)
        {
            Logger.LogWarning(ex, "Failed to clean up temporary SFTP upload: {FilePath}", temporaryPath);
        }
        catch (IOException ex)
        {
            Logger.LogWarning(ex, "Failed to clean up temporary SFTP upload: {FilePath}", temporaryPath);
        }
    }

    private static async Task<bool> DownloadToTemporaryFileAsync(
        SftpClient client,
        string remoteFilePath,
        string temporaryPath,
        CancellationToken cancellationToken)
    {
        using var fileStream = new FileStream(
            temporaryPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        await client.DownloadFileAsync(remoteFilePath, fileStream, cancellationToken).ConfigureAwait(false);
        await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }

    /// <inheritdoc />
    protected override bool IsRetryableException(Exception exception)
    {
        if (exception is SftpPermissionDeniedException or
            SftpPathNotFoundException or
            SshAuthenticationException)
        {
            return false;
        }

        return base.IsRetryableException(exception);
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
        var currentPath = directoryPath.StartsWith(SftpPathSeparator) ? SftpPathSeparator.ToString() : string.Empty;

        foreach (var path in paths)
        {
            currentPath = string.IsNullOrEmpty(currentPath)
                ? path
                : currentPath == SftpPathSeparator.ToString()
                    ? currentPath + path
                    : currentPath + SftpPathSeparator + path;

            if (!await IsRemoteDirectoryAsync(client, currentPath, cancellationToken).ConfigureAwait(false))
            {
                await client.CreateDirectoryAsync(currentPath, cancellationToken).ConfigureAwait(false);
            }
        }
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

