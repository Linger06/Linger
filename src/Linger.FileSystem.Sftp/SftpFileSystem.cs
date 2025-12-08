using Linger.Extensions.Core;
using Linger.FileSystem.Exceptions;
using Linger.FileSystem.Remote;
using Linger.Helper;
using Renci.SshNet;

namespace Linger.FileSystem.Sftp;

/// <summary>
/// SFTP文件系统实现
/// </summary>
public class SftpFileSystem : RemoteFileSystemBase
{
    private const char SftpPathSeparator = '/';
    private const string SftpRootPath = "/";
    private bool _disposed;

    /// <summary>
    /// SFTP客户端
    /// </summary>
    protected SftpClient Client { get; }
    private static readonly char[] s_separator = new char[] { '/', '\\' };

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
            Client.Connect();
    }

    public override Task ConnectAsync()
    {
        Connect();
        return Task.CompletedTask;
    }

    public void Disconnect()
    {
        if (Client?.IsConnected == true)
            Client.Disconnect();
    }

    public override Task DisconnectAsync()
    {
        Disconnect();
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        if (_disposed)
            return;

        Client?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 文件操作基本方法

    /// <summary>
    /// 检查文件是否存在（同步方法 - 已移除）
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
    /// 检查目录是否存在（同步方法 - 已移除）
    /// </summary>
    /// <remarks>
    /// ⚠️ 此同步方法已被移除以避免死锁风险。
    /// 请使用 <see cref="DirectoryExistsAsync"/> 异步版本。
    /// <exception cref="NotSupportedException">同步方法不再受支持</exception>
    /// </remarks>
    [Obsolete("此同步方法可能导致死锁，请使用 DirectoryExistsAsync 异步版本", true)]
    public override bool DirectoryExists(string directoryPath)
    {
        throw new NotSupportedException(
            "同步方法 DirectoryExists 已被移除以避免死锁。请使用 DirectoryExistsAsync 异步版本。" +
            "参考迁移指南: https://github.com/Linger06/Linger/blob/develop/src/Linger.FileSystem/README.zh-CN.md#迁移指南");
    }

    /// <summary>
    /// 创建目录（同步方法 - 已移除）
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
    /// 删除文件（同步方法 - 已移除）
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
            return await Task.Run(() => Client.Exists(filePath) && Client.GetAttributes(filePath).IsRegularFile, cancellationToken).ConfigureAwait(false);
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
            return await Task.Run(() => Client.Exists(directoryPath) && Client.GetAttributes(directoryPath).IsDirectory, cancellationToken).ConfigureAwait(false);
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
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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

        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
                return FileOperationResult.CreateFailure($"远程文件已存在 {destinationFilePath}");

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

#pragma warning disable CS0618 // 类型或成员已过时
                        if (FileExists(destinationFilePath) && overwrite)
#pragma warning restore CS0618 // 类型或成员已过时
                            Client.DeleteFile(destinationFilePath);

                        Client.UploadFile(inputStream, destinationFilePath);
                        return true;
                    }, cancellationToken).ConfigureAwait(false);
                    return true;
                },
                "Upload file", cancellationToken: cancellationToken).ConfigureAwait(false);

            // 获取文件大小
            var fileSize = TryGetFileSize(destinationFilePath);

            return FileOperationResult.CreateSuccess(destinationFilePath, null, fileSize);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Destination: {destinationFilePath}");
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
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
            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
            return await UploadAsync(fileStream, destinationFilePath, overwrite, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("Upload file", ex, $"Local: {localFilePath}, Destination: {destinationFilePath}");
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
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
            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
            return await UploadAsync(fileStream, remotePath, overwrite, cancellationToken).ConfigureAwait(false);
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

        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
            return FileOperationResult.CreateFailure($"下载文件到流失败: {ex.Message}", ex);
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

        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
            return FileOperationResult.CreateFailure($"下载文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
            return FileOperationResult.CreateFailure($"删除文件失败: {ex.Message}", ex);
        }
    }

    #endregion

    #region SFTP特有功能

    /// <summary>
    /// 列出目录中的文件（同步方法 - 已过时）
    /// </summary>
    /// <remarks>
    /// ⚠️ 请使用 <see cref="ListFilesAsync"/> 异步版本。
    /// </remarks>
    [Obsolete("请使用 ListFilesAsync 异步版本", true)]
    public List<string> ListFiles(string directoryPath)
    {
        throw new NotSupportedException(
            "同步方法 ListFiles 已被移除。请使用 ListFilesAsync 异步版本。");
    }

    /// <summary>
    /// 列出目录中的子目录（同步方法 - 已过时）
    /// </summary>
    /// <remarks>
    /// ⚠️ 请使用 <see cref="ListDirectoriesAsync"/> 异步版本。
    /// </remarks>
    [Obsolete("请使用 ListDirectoriesAsync 异步版本", true)]
    public List<string> ListDirectories(string directoryPath)
    {
        throw new NotSupportedException(
            "同步方法 ListDirectories 已被移除。请使用 ListDirectoriesAsync 异步版本。");
    }

    /// <summary>
    /// 设置工作目录（同步方法 - 已过时）
    /// </summary>
    /// <remarks>
    /// ⚠️ 请使用 <see cref="SetWorkingDirectoryAsync"/> 异步版本。
    /// </remarks>
    [Obsolete("请使用 SetWorkingDirectoryAsync 异步版本", true)]
    public void SetWorkingDirectory(string directoryPath)
    {
        throw new NotSupportedException(
            "同步方法 SetWorkingDirectory 已被移除。请使用 SetWorkingDirectoryAsync 异步版本。");
    }

    /// <summary>
    /// 设置根目录为工作目录（同步方法 - 已过时）
    /// </summary>
    [Obsolete("请使用 SetRootAsWorkingDirectoryAsync 异步版本", true)]
    public void SetRootAsWorkingDirectory()
    {
        throw new NotSupportedException(
            "同步方法 SetRootAsWorkingDirectory 已被移除。请使用 SetRootAsWorkingDirectoryAsync 异步版本。");
    }

    /// <summary>
    /// 获取文件最后修改时间（同步方法 - 已过时）
    /// </summary>
    /// <remarks>
    /// ⚠️ 请使用 <see cref="GetLastModifiedTimeAsync"/> 异步版本。
    /// </remarks>
    [Obsolete("请使用 GetLastModifiedTimeAsync 异步版本", true)]
    public DateTime GetLastModifiedTime(string remotePath)
    {
        throw new NotSupportedException(
            "同步方法 GetLastModifiedTime 已被移除。请使用 GetLastModifiedTimeAsync 异步版本。");
    }

    /// <summary>
    /// 异步获取文件修改时间
    /// </summary>
    public async Task<DateTime> GetLastModifiedTimeAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// 异步获取目录文件列表
    /// </summary>
    public async Task<List<string>> ListFilesAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!Client.Exists(directoryPath) || !Client.GetAttributes(directoryPath).IsDirectory)
                    return new List<string>();

                return Client.ListDirectory(directoryPath)
                    .Where(file => !file.IsDirectory && !file.Name.StartsWith('.'))
                    .Select(file => file.Name)
                    .ToList();
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("List files", ex, directoryPath);
            return new List<string>();
        }
    }

    /// <summary>
    /// 异步获取子目录列表
    /// </summary>
    public async Task<List<string>> ListDirectoriesAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
        try
        {
            return await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!Client.Exists(directoryPath) || !Client.GetAttributes(directoryPath).IsDirectory)
                    return new List<string>();

                return Client.ListDirectory(directoryPath)
                    .Where(file => file.IsDirectory && !file.Name.StartsWith('.') && file.Name != "." && file.Name != "..")
                    .Select(file => file.Name)
                    .ToList();
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException("List directories", ex, directoryPath);
            return new List<string>();
        }
    }

    /// <summary>
    /// 异步设置工作目录
    /// </summary>
    public async Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        await using var scope = await CreateConnectionScopeAsync().ConfigureAwait(false);
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
}

