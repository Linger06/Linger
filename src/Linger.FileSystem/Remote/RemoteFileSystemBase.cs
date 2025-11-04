using System.Runtime.CompilerServices;

namespace Linger.FileSystem.Remote;

/// <summary>
/// 远程文件系统基类，实现连接管理和通用功能
/// </summary>
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

    protected RemoteFileSystemBase(RemoteSystemSetting setting, RetryOptions? retryOptions = null)
        : base(retryOptions)
    {
        Setting = setting ?? throw new System.ArgumentNullException(nameof(setting));
        if (string.IsNullOrEmpty(setting.Host))
            throw new System.ArgumentException($"Host cannot be null or empty: {nameof(setting.Host)}");

        ServerDetailsString = FormatServerDetails();
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
    //public abstract void Connect();
    public abstract Task ConnectAsync();
    //public abstract void Disconnect();
    public abstract Task DisconnectAsync();
    public abstract void Dispose();
    public virtual string ServerDetails() => ServerDetailsString;
    #endregion

    /// <summary>
    /// 异步连接作用域，用于自动连接和释放连接
    /// </summary>
    protected sealed class AsyncConnectionScope : IAsyncDisposable
    {
        private readonly RemoteFileSystemBase _fileSystem;
        private readonly bool _wasConnected;
        private bool _disposed;

        private AsyncConnectionScope(RemoteFileSystemBase fileSystem, bool wasConnected)
        {
            _fileSystem = fileSystem;
            _wasConnected = wasConnected;
        }

        /// <summary>
        /// 创建异步连接作用域
        /// </summary>
        public static async Task<AsyncConnectionScope> CreateAsync(RemoteFileSystemBase fileSystem)
        {
            var wasConnected = fileSystem.IsConnected();

            if (!wasConnected)
            {
                await fileSystem.ConnectAsync().ConfigureAwait(false);
            }

            return new AsyncConnectionScope(fileSystem, wasConnected);
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            if (!_wasConnected && _fileSystem.IsConnected())
            {
                await _fileSystem.DisconnectAsync().ConfigureAwait(false);
            }

            _disposed = true;
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
}
