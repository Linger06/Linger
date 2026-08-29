using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Linger.FileSystem.Remote;

/// <summary>
/// 远程文件系统基类，实现连接管理和通用功能
/// </summary>
/// <remarks>
/// <para>此基类提供了远程文件系统的通用功能：</para>
/// <list type="bullet">
///   <item><description>连接管理：自动连接和断开连接</description></item>
///   <item><description>日志记录：记录连接状态、操作执行情况</description></item>
///   <item><description>异常处理：包含服务器信息的详细异常</description></item>
/// </list>
/// </remarks>
public abstract class RemoteFileSystemBase : FileSystemBase, IRemoteFileSystem
{
    private readonly SemaphoreSlim _connectionGate = new(1, 1);

    /// <summary>
    /// 服务器连接信息
    /// </summary>
    protected readonly RemoteFileSystemOptions Options;

    private readonly string _protocol;

    /// <summary>
    /// 服务器详情描述
    /// </summary>
    protected readonly string ServerDetailsString;

    private int _disposeState;

    /// <summary>
    /// 指示是否已开始释放资源。
    /// </summary>
    protected bool Disposed => Volatile.Read(ref _disposeState) != 0;

    /// <summary>
    /// 初始化 <see cref="RemoteFileSystemBase"/> 的新实例。
    /// </summary>
    /// <param name="options">远程服务器连接选项。</param>
    /// <param name="protocol">远程文件系统协议名称。</param>
    /// <param name="retryOptions">重试选项（可选）。</param>
    /// <param name="logger">日志记录器（可选）。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="options"/> 为 <c>null</c> 时抛出。</exception>
    /// <exception cref="ArgumentException">当主机或协议名称为空时抛出。</exception>
    protected RemoteFileSystemBase(RemoteFileSystemOptions options, string protocol, RetryOptions? retryOptions = null, ILogger? logger = null)
        : base(retryOptions, logger)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrEmpty(options.Host))
            throw new ArgumentException($"Host cannot be null or empty: {nameof(options.Host)}", nameof(options));
        if (string.IsNullOrWhiteSpace(protocol))
            throw new ArgumentException("Protocol cannot be null or empty.", nameof(protocol));

        _protocol = protocol;
        ServerDetailsString = $"{protocol}://{options.UserName}@{options.Host}:{options.Port}";
        Logger.LogDebug("RemoteFileSystem initialized: {ServerDetails}", ServerDetailsString);
    }

    /// <summary>
    /// 远程文件系统标识
    /// </summary>
    public override bool IsRemoteFileSystem => true;

    #region IRemoteFileSystem 实现
    protected abstract bool IsConnected();
    protected abstract Task ConnectAsync(CancellationToken cancellationToken);
    /// <summary>
    /// 释放派生类持有的客户端资源。
    /// </summary>
    protected abstract void DisposeCore();
    public abstract Task<DateTime> GetLastModifiedTimeAsync(string filePath, CancellationToken cancellationToken = default);
    public abstract Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);
    /// <inheritdoc />
    public abstract Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
    /// <inheritdoc />
    public abstract Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default);
    /// <inheritdoc />
    public abstract Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default);
    /// <inheritdoc />
    public abstract Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default);
    /// <inheritdoc />
    public abstract Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default);
    /// <inheritdoc />
    public abstract Task<Stream> OpenWriteAsync(string filePath, bool overwrite = false, CancellationToken cancellationToken = default);
    /// <inheritdoc />
    public abstract Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default);
    /// <inheritdoc />
    public abstract Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 释放远程文件系统及其连接管理资源。
    /// </summary>
    /// <remarks>
    /// 调用此方法前必须确保当前实例发起的所有操作已经完成，并释放实例返回的流和文本读写器。
    /// 此方法不等待并发操作，不得与实例操作同时调用。
    /// </remarks>
    public void Dispose()
    {
        if (!TryBeginDispose())
        {
            return;
        }

        DisposeCoreAndDependencies();
    }

    /// <inheritdoc />
    public virtual async Task<StreamReader> GetReaderAsync(string filePath, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        var stream = await OpenReadAsync(filePath, cancellationToken).ConfigureAwait(false);
#if NET6_0_OR_GREATER
        return new StreamReader(stream, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
#else
        return new StreamReader(stream, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: false);
#endif
    }

    /// <inheritdoc />
    public virtual async Task<StreamWriter> GetWriterAsync(string filePath, bool overwrite = false, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        var stream = await OpenWriteAsync(filePath, overwrite, cancellationToken).ConfigureAwait(false);
#if NET6_0_OR_GREATER
        return new StreamWriter(stream, encoding ?? Encoding.UTF8, leaveOpen: false);
#else
        return new StreamWriter(stream, encoding ?? Encoding.UTF8, bufferSize: 1024, leaveOpen: false);
#endif
    }

    /// <inheritdoc />
    public virtual string ServerDetails => ServerDetailsString;
    #endregion

    /// <summary>
    /// 尝试取得释放流程的执行权。
    /// </summary>
    /// <returns>当前调用负责执行释放时返回 <see langword="true"/>。</returns>
    protected bool TryBeginDispose()
    {
        return Interlocked.CompareExchange(ref _disposeState, 1, 0) == 0;
    }

    /// <summary>
    /// 执行派生类资源和基类连接闸门的统一释放。
    /// </summary>
    /// <remarks>
    /// 异步释放实现应先完成真实的异步断开，再调用此方法完成剩余释放。
    /// </remarks>
    protected void DisposeCoreAndDependencies()
    {
        try
        {
            Logger.LogDebug("Disposing remote file system: {ServerDetails}", ServerDetailsString);
            DisposeCore();
        }
        finally
        {
            _connectionGate.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// 确保已建立连接。如果尚未连接，则自动连接。
    /// </summary>
    /// <remarks>
    /// <para>此方法是远程操作推荐使用的连接检查方式。</para>
    /// <para>连接保持到实例被 Dispose，避免每次操作都重新连接的开销。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// await EnsureConnectedAsync();
    /// // 执行文件操作...
    /// </code>
    /// </example>
    protected async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (IsConnected())
        {
            return;
        }

        await _connectionGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!IsConnected())
            {
                Logger.LogDebug("Connecting to {ServerDetails}...", ServerDetailsString);
                await ConnectAsync(cancellationToken).ConfigureAwait(false);
                Logger.LogDebug("Connected to {ServerDetails}", ServerDetailsString);
            }
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    /// <summary>
    /// 在执行远程操作前检查实例是否已经释放。
    /// </summary>
    protected void ThrowIfDisposed()
    {
#if NET7_0_OR_GREATER
        ObjectDisposedException.ThrowIf(Disposed, this);
#else
        if (Disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
#endif
    }

    /// <summary>
    /// 创建带有服务器信息的文件系统异常
    /// </summary>
    protected FileSystemException CreateException(string operation, Exception ex, string? path = null, [CallerMemberName] string callerMethod = "")
    {
        var message = $"""
                          {operation} failed on {Options.Host}:{Options.Port}.
                          {(path is not null ? $"Path: {path}. " : string.Empty)}
                          Type: {_protocol}, Method: {callerMethod}
                          """;

        return new FileSystemException(operation, path, ServerDetails, message, ex);
    }

    /// <summary>
    /// 处理异常并抛出文件系统异常
    /// </summary>
    [DoesNotReturn]
    protected override void HandleException(string operation, Exception ex, string? path = null, [CallerMemberName] string callerMethod = "")
    {
        if (ex is OperationCanceledException)
        {
            base.HandleException(operation, ex, path, callerMethod);
        }

        if (ex is FileSystemException)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
        }

        var exception = CreateException(operation, ex, path, callerMethod);
        Logger.LogError(ex, "{Message}", exception.Message);
        throw exception;
    }

    /// <summary>
    /// 在目标文件所在的远程目录中生成临时文件路径。
    /// </summary>
    /// <param name="destinationFilePath">远程目标文件路径。</param>
    /// <param name="pathSeparator">远程路径分隔符。</param>
    /// <param name="operationName">临时文件所对应的操作名称。</param>
    /// <returns>与目标文件位于同一远程目录的临时文件路径。</returns>
    protected static string GetRemoteTemporaryFilePath(
        string destinationFilePath,
        char pathSeparator,
        string operationName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var separatorIndex = destinationFilePath.LastIndexOf(pathSeparator);
        var directoryPrefix = separatorIndex >= 0
            ? destinationFilePath.Substring(0, separatorIndex + 1)
            : string.Empty;

        return $"{directoryPrefix}.{operationName}-{Guid.NewGuid():N}.tmp";
    }

    /// <summary>
    /// 将同目录临时文件提交到目标路径。
    /// </summary>
    protected static void CommitTemporaryFile(string sourcePath, string destinationPath, bool overwrite)
    {
        if (!overwrite && File.Exists(destinationPath))
        {
            throw new DuplicateFileException(destinationPath);
        }

#if NET5_0_OR_GREATER
        try
        {
            File.Move(sourcePath, destinationPath, overwrite);
        }
        catch (IOException) when (!overwrite && File.Exists(destinationPath))
        {
            throw new DuplicateFileException(destinationPath);
        }
#else
        if (!overwrite)
        {
            try
            {
                File.Move(sourcePath, destinationPath);
            }
            catch (IOException) when (File.Exists(destinationPath))
            {
                throw new DuplicateFileException(destinationPath);
            }

            return;
        }

        if (File.Exists(destinationPath))
        {
            File.Replace(sourcePath, destinationPath, null);
            return;
        }

        try
        {
            File.Move(sourcePath, destinationPath);
        }
        catch (IOException) when (File.Exists(destinationPath))
        {
            File.Replace(sourcePath, destinationPath, null);
        }
#endif
    }

    /// <summary>
    /// 下载到同目录临时文件，并在成功后提交到本地目标路径。
    /// </summary>
    protected async Task<bool> DownloadFileAtomicallyAsync(
        string localDestinationPath,
        bool overwrite,
        Func<string, CancellationToken, Task<bool>> downloadAttempt,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localDestinationPath);
        ArgumentNullException.ThrowIfNull(downloadAttempt);

        var destinationFullPath = Path.GetFullPath(localDestinationPath);
        var destinationDirectory = Path.GetDirectoryName(destinationFullPath)!;
        Directory.CreateDirectory(destinationDirectory);
        var temporaryPath = Path.Combine(destinationDirectory, $".download-{Guid.NewGuid():N}.tmp");

        try
        {
            var success = await RetryHelper.ExecuteAsync(
                operationCancellationToken => downloadAttempt(temporaryPath, operationCancellationToken),
                "Download file",
                shouldRetry: IsRetryableException,
                shouldRetryResult: result => !result,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (!success)
            {
                return false;
            }

            CommitTemporaryFile(temporaryPath, destinationFullPath, overwrite);
            return true;
        }
        finally
        {
            try
            {
                File.Delete(temporaryPath);
            }
            catch (IOException ex)
            {
                Logger.LogWarning(ex, "Failed to clean up temporary download file: {FilePath}", temporaryPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.LogWarning(ex, "Failed to clean up temporary download file: {FilePath}", temporaryPath);
            }
        }
    }

    /// <summary>
    /// 尝试删除远程临时文件；删除失败仅记录警告，不向上抛出。
    /// </summary>
    /// <typeparam name="TClient">远程客户端类型。</typeparam>
    /// <param name="client">远程客户端实例。</param>
    /// <param name="temporaryPath">远程临时文件路径。</param>
    /// <param name="operationDescription">操作描述，用于日志。</param>
    /// <param name="existsAsync">检查文件是否存在的委托。</param>
    /// <param name="deleteAsync">删除文件的委托。</param>
    /// <param name="isExpectedException">判断异常是否为协议特定异常（如 FTP/SFTP 异常）的委托。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    protected async Task TryDeleteTemporaryFileAsync<TClient>(
        TClient client,
        string temporaryPath,
        string operationDescription,
        Func<TClient, string, CancellationToken, Task<bool>> existsAsync,
        Func<TClient, string, CancellationToken, Task> deleteAsync,
        Func<Exception, bool> isExpectedException,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(existsAsync);
        ArgumentNullException.ThrowIfNull(deleteAsync);
        ArgumentNullException.ThrowIfNull(isExpectedException);

        try
        {
            if (await existsAsync(client, temporaryPath, cancellationToken).ConfigureAwait(false))
            {
                await deleteAsync(client, temporaryPath, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (isExpectedException(ex) || ex is IOException)
        {
            Logger.LogWarning(ex, "Failed to clean up temporary {Operation} file: {FilePath}", operationDescription, temporaryPath);
        }
    }

    #region 远程目录操作

    /// <inheritdoc />
    public abstract Task<IReadOnlyList<string>> ListFilesAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task<IReadOnlyList<string>> ListDirectoriesAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);

    #endregion

    /// <summary>
    /// 判断远程操作异常是否适合重试。
    /// </summary>
    /// <param name="exception">操作抛出的异常。</param>
    /// <returns><see langword="true"/> 表示该故障可能是暂时性的。</returns>
    protected virtual bool IsRetryableException(Exception exception)
    {
        return exception is not (
            ArgumentException or
            UnauthorizedAccessException or
            FileNotFoundException or
            DirectoryNotFoundException or
            DuplicateFileException or
            NotSupportedException or
            ObjectDisposedException);
    }

}
