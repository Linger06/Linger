namespace Linger.FileSystem.Remote;

/// <summary>
/// 定义远程文件系统上下文接口
/// </summary>
public interface IRemoteFileSystem : IDisposable
{
    /// <summary>
    /// 检查是否已连接
    /// </summary>
    bool IsConnected();

    /// <summary>
    /// 异步连接到远程服务器
    /// </summary>
    Task ConnectAsync();

    /// <summary>
    /// 异步断开与远程服务器的连接
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// 获取服务器详细信息
    /// </summary>
    string ServerDetails();
}
