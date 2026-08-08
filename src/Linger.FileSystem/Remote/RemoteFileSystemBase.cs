using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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

    /// <summary>
    /// 指示是否已释放资源
    /// </summary>
    protected bool Disposed;

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
    public abstract bool IsConnected();
    public abstract Task ConnectAsync();

    /// <inheritdoc />
    public abstract Task ConnectAsync(CancellationToken cancellationToken);
    public abstract Task DisconnectAsync();
    public abstract Task<DateTime> GetLastModifiedTimeAsync(string filePath, CancellationToken cancellationToken = default);
    public abstract Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);
    public abstract void Dispose();

    /// <summary>
    /// 异步释放资源
    /// </summary>
    /// <returns>表示异步释放操作的 <see cref="ValueTask"/>。</returns>
    public virtual async ValueTask DisposeAsync()
    {
        if (Disposed)
            return;

        Logger.LogDebug("Disposing remote file system: {ServerDetails}", ServerDetailsString);
        try
        {
            await DisconnectAsync().ConfigureAwait(false);
        }
        finally
        {
            Dispose();
            GC.SuppressFinalize(this);
        }
    }

    /// <inheritdoc />
    public virtual string ServerDetails => ServerDetailsString;
    #endregion

    /// <summary>
    /// 确保已建立连接。如果尚未连接，则自动连接。
    /// </summary>
    /// <remarks>
    /// <para>此方法是推荐的连接检查方式，比 <see cref="CreateConnectionScopeAsync"/> 更简洁。</para>
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

    #region IBatchFileSystemOperations 实现

    /// <inheritdoc />
    public abstract Task<BatchOperationResult> UploadFilesAsync(
        IEnumerable<string> localFilePaths,
        string remoteDirectory,
        bool overwrite = false,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task<BatchOperationResult> DownloadFilesAsync(
        IEnumerable<string> remoteFilePaths,
        string localDirectory,
        bool overwrite = false,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task<BatchOperationResult> DeleteFilesAsync(
        IEnumerable<string> filePaths,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task<IReadOnlyList<string>> ListFilesAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task<IReadOnlyList<string>> ListDirectoriesAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);

    #endregion

    #region 批量操作辅助方法

    /// <summary>
    /// 执行带重试的单文件操作
    /// </summary>
    /// <param name="operation">要执行的异步操作，返回 true 表示成功</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作是否成功</returns>
    /// <remarks>
    /// 当 <see cref="RemoteFileSystemOptions.BatchRetryOptions"/> 不为 <c>null</c> 时启用重试。
    /// </remarks>
    protected async Task<bool> ExecuteWithBatchRetryAsync(Func<Task<bool>> operation, CancellationToken cancellationToken)
    {
        var retryOptions = Options.BatchRetryOptions;
        if (retryOptions is null)
        {
            return await operation().ConfigureAwait(false);
        }

        var helper = new RetryHelper(retryOptions);
        return await helper.ExecuteAsync(
            _ => operation(),
            "batch operation",
            shouldRetry: IsBatchRetryableException,
            shouldRetryResult: success => !success,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 执行带重试的单文件操作（异常版本）
    /// </summary>
    /// <param name="operation">要执行的异步操作</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <remarks>
    /// 当 <see cref="RemoteFileSystemOptions.BatchRetryOptions"/> 不为 <c>null</c> 时启用重试。
    /// 适用于抛出异常表示失败的操作。
    /// </remarks>
    protected async Task ExecuteWithBatchRetryAsync(Func<Task> operation, CancellationToken cancellationToken)
    {
        var retryOptions = Options.BatchRetryOptions;
        if (retryOptions is null)
        {
            await operation().ConfigureAwait(false);
            return;
        }

        var helper = new RetryHelper(retryOptions);
        await helper.ExecuteAsync(
            _ => operation(),
            "batch operation",
            IsBatchRetryableException,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 判断批量操作异常是否适合重试。
    /// </summary>
    /// <param name="exception">操作抛出的异常。</param>
    /// <returns><see langword="true"/> 表示该故障可能是暂时性的。</returns>
    protected virtual bool IsBatchRetryableException(Exception exception)
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

    #endregion
}
