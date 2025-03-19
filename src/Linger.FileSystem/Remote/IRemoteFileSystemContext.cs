namespace Linger.FileSystem.Remote;

/// <summary>
/// 定义远程文件系统上下文接口
/// </summary>
public interface IRemoteFileSystemContext : IDisposable
{
    bool IsConnected();
    void Connect();
    void Disconnect();
    string ServerDetails();
}