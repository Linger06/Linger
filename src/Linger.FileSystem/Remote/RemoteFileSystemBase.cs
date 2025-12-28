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
    /// <summary>
    /// 服务器连接信息
    /// </summary>
    protected readonly RemoteSystemSetting Setting;

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
    /// <param name="setting">远程服务器连接设置。</param>
    /// <param name="retryOptions">重试选项（可选）。</param>
    /// <param name="logger">日志记录器（可选）。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="setting"/> 为 <c>null</c> 时抛出。</exception>
    /// <exception cref="ArgumentException">当 <see cref="RemoteSystemSetting.Host"/> 为空时抛出。</exception>
    protected RemoteFileSystemBase(RemoteSystemSetting setting, RetryOptions? retryOptions = null, ILogger? logger = null)
        : base(retryOptions, logger)
    {
        Setting = setting ?? throw new ArgumentNullException(nameof(setting));
        if (string.IsNullOrEmpty(setting.Host))
            throw new ArgumentException($"Host cannot be null or empty: {nameof(setting.Host)}");

        ServerDetailsString = FormatServerDetails();
        LogDebug("RemoteFileSystem initialized: {ServerDetails}", ServerDetailsString);
    }

    /// <summary>
    /// 远程文件系统标识
    /// </summary>
    public override bool IsRemoteFileSystem => true;

    protected virtual string FormatServerDetails()
    {
        return $"{Setting.Type}://{Setting.UserName}@{Setting.Host}:{Setting.Port}";
    }

    #region IRemoteFileSystem 实现
    public abstract bool IsConnected();
    public abstract Task ConnectAsync();
    public abstract Task DisconnectAsync();
    public abstract void Dispose();
    
    /// <summary>
    /// 异步释放资源
    /// </summary>
    /// <returns>表示异步释放操作的 <see cref="ValueTask"/>。</returns>
    public virtual async ValueTask DisposeAsync()
    {
        if (Disposed)
            return;

        LogDebug("Disposing remote file system: {ServerDetails}", ServerDetailsString);
        await DisconnectAsync().ConfigureAwait(false);
        Dispose();
        GC.SuppressFinalize(this);
    }
    
    public virtual string ServerDetails() => ServerDetailsString;
    #endregion

    /// <summary>
    /// 异步连接作用域，用于确保操作前已连接
    /// </summary>
    /// <remarks>
    /// <para>此作用域确保在执行操作前建立连接，但不会在作用域结束时断开连接。</para>
    /// <para>连接保持到实例被 Dispose，避免每次操作都重新连接的开销。</para>
    /// </remarks>
    protected sealed class AsyncConnectionScope : IAsyncDisposable
    {
        private bool _disposed;

        private AsyncConnectionScope()
        {
        }

        /// <summary>
        /// 创建异步连接作用域，确保已建立连接
        /// </summary>
        public static async Task<AsyncConnectionScope> CreateAsync(RemoteFileSystemBase fileSystem)
        {
            if (!fileSystem.IsConnected())
            {
                fileSystem.LogDebug("Connecting to {ServerDetails}...", fileSystem.ServerDetailsString);
                await fileSystem.ConnectAsync().ConfigureAwait(false);
                fileSystem.LogDebug("Connected to {ServerDetails}", fileSystem.ServerDetailsString);
            }

            return new AsyncConnectionScope();
        }

        /// <summary>
        /// 作用域结束时不断开连接，连接保持到实例被 Dispose
        /// </summary>
        public ValueTask DisposeAsync()
        {
            _disposed = true;
            return default;
        }
    }

    /// <summary>
    /// 创建异步连接作用域
    /// </summary>
    protected virtual Task<AsyncConnectionScope> CreateConnectionScopeAsync()
    {
        return AsyncConnectionScope.CreateAsync(this);
    }

    /// <summary>
    /// 创建带有服务器信息的文件系统异常
    /// </summary>
    protected FileSystemException CreateException(string operation, Exception ex, string? path = null, [CallerMemberName] string callerMethod = "")
    {
        var message = $"""
                          {operation} failed on {Setting.Host}:{Setting.Port}. 
                          {(path is not null ? $"Path: {path}. " : string.Empty)}
                          Type: {Setting.Type}, Method: {callerMethod}
                          """;

        return new FileSystemException(operation, path, ServerDetails(), message, ex);
    }

    /// <summary>
    /// 处理异常并抛出文件系统异常
    /// </summary>
    protected override void HandleException(string operation, Exception ex, string? path = null, [CallerMemberName] string callerMethod = "")
    {
        throw CreateException(operation, ex, path, callerMethod);
    }

    #region IBatchFileSystemOperations 实现

    /// <inheritdoc />
    public abstract Task<BatchOperationResult> UploadFilesAsync(
        IEnumerable<string> localFilePaths,
        string remoteDirectory,
        bool overwrite = false,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public virtual Task<BatchOperationResult> UploadFilesAsync(
        IEnumerable<string> localFilePaths,
        string remoteDirectory,
        bool overwrite,
        IProgress<BatchProgress>? progress,
        CancellationToken cancellationToken = default)
        => UploadFilesAsync(localFilePaths, remoteDirectory, overwrite, cancellationToken);

    /// <inheritdoc />
    public abstract Task<BatchOperationResult> DownloadFilesAsync(
        IEnumerable<string> remoteFilePaths,
        string localDirectory,
        bool overwrite = false,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public virtual Task<BatchOperationResult> DownloadFilesAsync(
        IEnumerable<string> remoteFilePaths,
        string localDirectory,
        bool overwrite,
        IProgress<BatchProgress>? progress,
        CancellationToken cancellationToken = default)
        => DownloadFilesAsync(remoteFilePaths, localDirectory, overwrite, cancellationToken);

    /// <inheritdoc />
    public abstract Task<BatchOperationResult> DeleteFilesAsync(
        IEnumerable<string> filePaths,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public virtual Task<BatchOperationResult> DeleteFilesAsync(
        IEnumerable<string> filePaths,
        IProgress<BatchProgress>? progress,
        CancellationToken cancellationToken = default)
        => DeleteFilesAsync(filePaths, cancellationToken);

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
    /// 当 <see cref="RemoteSystemSetting.BatchOperationRetryCount"/> 大于 0 时启用重试。
    /// 重试间隔使用线性退避策略：delay * attempts。
    /// </remarks>
    protected async Task<bool> ExecuteWithBatchRetryAsync(Func<Task<bool>> operation, CancellationToken cancellationToken)
    {
        var retryCount = Setting.BatchOperationRetryCount;
        if (retryCount <= 0)
        {
            return await operation().ConfigureAwait(false);
        }

        var delayMs = Setting.BatchOperationRetryDelayMilliseconds;
        var attempts = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempts++;

            try
            {
                var result = await operation().ConfigureAwait(false);
                if (result)
                {
                    return true;
                }

                // 操作返回 false，可能需要重试
                if (attempts > retryCount)
                {
                    return false;
                }

                LogDebug("Batch operation returned false, retrying (attempt {Attempt}/{MaxAttempts})...", attempts, retryCount + 1);
                await Task.Delay(delayMs * attempts, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (attempts > retryCount)
                {
                    LogDebug("Batch operation failed after {Attempts} attempts: {Error}", attempts, ex.Message);

                    throw;
                }

                LogDebug("Batch operation failed (attempt {Attempt}/{MaxAttempts}): {Error}, retrying...", attempts, retryCount + 1, ex.Message);
                await Task.Delay(delayMs * attempts, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// 执行带重试的单文件操作（异常版本）
    /// </summary>
    /// <param name="operation">要执行的异步操作</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <remarks>
    /// 当 <see cref="RemoteSystemSetting.BatchOperationRetryCount"/> 大于 0 时启用重试。
    /// 适用于抛出异常表示失败的操作。
    /// </remarks>
    protected async Task ExecuteWithBatchRetryAsync(Func<Task> operation, CancellationToken cancellationToken)
    {
        var retryCount = Setting.BatchOperationRetryCount;
        if (retryCount <= 0)
        {
            await operation().ConfigureAwait(false);

            return;
        }

        var delayMs = Setting.BatchOperationRetryDelayMilliseconds;
        var attempts = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempts++;

            try
            {
                await operation().ConfigureAwait(false);

                return;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (attempts > retryCount)
                {
                    LogDebug("Batch operation failed after {Attempts} attempts: {Error}", attempts, ex.Message);

                    throw;
                }

                LogDebug("Batch operation failed (attempt {Attempt}/{MaxAttempts}): {Error}, retrying...", attempts, retryCount + 1, ex.Message);
                await Task.Delay(delayMs * attempts, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    #endregion
}
