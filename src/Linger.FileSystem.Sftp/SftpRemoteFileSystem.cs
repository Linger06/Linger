using System.Text;
using Linger.FileSystem.Helpers;
using Renci.SshNet;

namespace Linger.FileSystem.Sftp;

public class SftpRemoteFileSystem : SftpContext
{
    private readonly string _serverDetails;

    public SftpRemoteFileSystem(RemoteSystemSetting setting)
    {
        _serverDetails = FtpHelper.ServerDetails(setting.Host, setting.Port.ToString(), setting.UserName, setting.Type);
        var connectionInfo =
            new ConnectionInfo(setting.Host, setting.Port, setting.UserName,
                new PasswordAuthenticationMethod(setting.UserName, setting.Password))
            {
                Encoding = setting.Encoding ?? Encoding.Default
            };
        SftpClient = new SftpClient(connectionInfo);
    }

    public override string ServerDetails()
    {
        return _serverDetails;
    }
}