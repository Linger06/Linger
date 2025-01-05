using Linger.FileSystem.Remote;
using Renci.SshNet;

namespace Linger.FileSystem.Sftp;

/*Renci.SshNet example : https://github.com/sshnet/SSH.NET
 * Local Directory:             @"C:\Files\Temp\file.csv"
 *                              @"C:\Files\Temp"
 * Ftp Directory(FluentFTP):    @"/Files/Temp/file.csv"
 *                              @"/Files/Temp/
 */

public abstract class SftpContext : IRemoteFileSystemContext
{
    protected SftpClient SftpClient { get; set; } = default!;

    public void Connect()
    {
        SftpClient.Connect();
    }

    public void Disconnect()
    {
        SftpClient.Disconnect();
    }

    public void Dispose()
    {
        SftpClient.Dispose();
    }

    /*actions*/

    public bool FileExists(string filePath)
    {
        return SftpClient.Exists(filePath);
    }

    public void DeleteFileIfExists(string filePath)
    {
        if (!FileExists(filePath))
        {
            SftpClient.DeleteFile(filePath);
        }
    }

    public void UploadFile(string localFilePath, string remoteFilePath)
    {
        var fileStream = new FileStream(localFilePath, FileMode.Open);
        SftpClient.UploadFile(fileStream, remoteFilePath);
    }

    public bool DirectoryExists(string directoryPath)
    {
        return SftpClient.Exists(directoryPath);
    }

    public void CreateDirectoryIfNotExists(string directoryPath)
    {
        if (!DirectoryExists(directoryPath))
        {
            SftpClient.CreateDirectory(directoryPath);
        }
    }

    public bool DownloadFile(string localFilePath, string remoteFilePath)
    {
        using Stream fileStream = File.Create(localFilePath);
        SftpClient.DownloadFile(remoteFilePath, fileStream);
        return true;
    }

    public bool IsConnected()
    {
        return SftpClient.IsConnected;
    }

    public void SetWorkingDirectory(string directoryPath)
    {
        SftpClient.ChangeDirectory(directoryPath);
    }

    public void SetRootAsWorkingDirectory()
    {
        SetWorkingDirectory(string.Empty);
    }

    public abstract string ServerDetails();

    /// <summary>
    ///     获取文件最后修改时间
    /// </summary>
    /// <param name="remotePath">远程路径("/test/abc.txt")</param>
    /// <returns></returns>
    public DateTime GetLastModifiedTime(string remotePath)
    {
        DateTime dateTime = SftpClient.GetLastWriteTime(remotePath);
        return dateTime;
    }
}