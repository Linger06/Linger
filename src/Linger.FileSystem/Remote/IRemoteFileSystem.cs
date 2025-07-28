namespace Linger.FileSystem.Remote;

/// <summary>
/// 定义远程文件系统上下文接口
/// </summary>
public interface IRemoteFileSystem : IDisposable
{
    bool IsConnected();
    //void Connect();
    Task ConnectAsync();
    //void Disconnect();
    Task DisconnectAsync();
    string ServerDetails();
}
