namespace Linger.FileSystem.Remote;

public interface IRemoteFileSystemContext : IFileSystem, IDisposable
{
    bool IsConnected();

    void Connect();

    void Disconnect();

    void SetWorkingDirectory(string path);

    void SetRootAsWorkingDirectory();

    void UploadFile(string localFilePath, string remoteFilePath);

    bool DownloadFile(string localFilePath, string remoteFilePath);

    string ServerDetails();

    DateTime GetLastModifiedTime(string remotePath);
}