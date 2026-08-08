namespace Linger.FileSystem.Remote;

/// <summary>
/// 定义远程文件系统上下文接口
/// </summary>
/// <remarks>
/// <para>此接口扩展了 <see cref="IFileSystemOperations"/> 和 <see cref="IBatchFileSystemOperations"/>，添加了远程连接管理功能。</para>
/// <para>实现类应支持 <see cref="IAsyncDisposable"/> 以便正确释放异步资源。</para>
/// <para>远程实现通常在实例内复用一个有状态客户端。不要在同一实例上并发执行非批量操作，
/// 也不要在其他操作运行期间调用 <see cref="SetWorkingDirectoryAsync"/>。</para>
/// </remarks>
public interface IRemoteFileSystem : IFileSystemOperations, IBatchFileSystemOperations, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 获取一个值，指示当前是否已连接到远程服务器。
    /// </summary>
    /// <returns>如果已连接，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    bool IsConnected();

    /// <summary>
    /// 异步连接到远程服务器。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    /// <exception cref="FileSystemException">当连接失败时抛出。</exception>
    Task ConnectAsync();

    /// <summary>
    /// 异步连接到远程服务器。
    /// </summary>
    /// <param name="cancellationToken">用于取消连接操作的令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    /// <exception cref="FileSystemException">当连接失败时抛出。</exception>
    /// <exception cref="OperationCanceledException">当连接操作被取消时抛出。</exception>
    Task ConnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 异步断开与远程服务器的连接。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    Task DisconnectAsync();

    /// <summary>
    /// Asynchronously gets the last modified time of a remote file.
    /// </summary>
    /// <param name="filePath">The remote file path.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The remote file's last modified time.</returns>
    Task<DateTime> GetLastModifiedTimeAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously changes the current working directory on the remote server.
    /// </summary>
    /// <param name="directoryPath">The remote directory path.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取服务器详细信息的描述字符串。
    /// </summary>
    /// <value>包含服务器类型、主机、端口等信息的字符串。</value>
    string ServerDetails { get; }
}
