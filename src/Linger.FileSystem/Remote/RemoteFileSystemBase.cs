using System.Runtime.CompilerServices;

namespace Linger.FileSystem.Remote;

/// <summary>
/// 远程文件系统基类，实现连接管理和通用功能
/// </summary>
public abstract class RemoteFileSystemBase : FileSystemBase, IRemoteFileSystemContext
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
        Setting = setting ?? throw new ArgumentNullException(nameof(setting));
        if (string.IsNullOrEmpty(setting.Host))
            throw new Exception($"Host cannot be null or empty: {nameof(setting.Host)}");

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

    #region IRemoteFileSystemContext 实现
    public abstract bool IsConnected();
    public abstract void Connect();
    public abstract void Disconnect();
    public abstract void Dispose();
    public virtual string ServerDetails() => ServerDetailsString;
    #endregion

    /// <summary>
    /// 连接作用域，用于自动连接和释放连接
    /// </summary>
    protected class ConnectionScope : IDisposable
    {
        private readonly RemoteFileSystemBase _fileSystem;
        private readonly bool _wasConnected;
        private bool _disposed;

        public ConnectionScope(RemoteFileSystemBase fileSystem)
        {
            _fileSystem = fileSystem;
            _wasConnected = _fileSystem.IsConnected();

            if (!_wasConnected)
                _fileSystem.Connect();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (!_wasConnected)
                _fileSystem.Disconnect();

            _disposed = true;
        }
    }

    /// <summary>
    /// 创建连接作用域
    /// </summary>
    protected virtual IDisposable CreateConnectionScope()
    {
        return new ConnectionScope(this);
    }

    /// <summary>
    /// 增强异常处理，添加服务器信息
    /// </summary>
    protected override void HandleException(string operation, Exception ex, string? path = null, [CallerMemberName] string callerMethod = "")
    {
        // 添加服务器详情到异常信息
        string message = $"""
                          {operation} failed on {Setting.Host}:{Setting.Port}. 
                          {(path != null ? $"Path: {path}. " : string.Empty)}.
                          Type: {Setting.Type}, Method: {callerMethod}
                          """;

        throw new FileSystemException(operation, path, ServerDetails(), message, ex);
    }
}
