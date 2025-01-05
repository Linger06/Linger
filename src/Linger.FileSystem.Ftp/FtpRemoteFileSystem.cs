using System.Net;
using System.Text;
using FluentFTP;
using Linger.FileSystem.Helpers;

namespace Linger.FileSystem.Ftp;

public class FtpRemoteFileSystem : FtpContext
{
    private readonly string _serverDetails;

    public FtpRemoteFileSystem(RemoteSystemSetting setting)
    {
        _serverDetails = FtpHelper.ServerDetails(setting.Host, setting.Port.ToString(), setting.UserName, setting.Type);
        FtpClient = new FtpClient(setting.Host)
        {
            Credentials = new NetworkCredential(setting.UserName, setting.Password),
            Port = setting.Port,
            Encoding = setting.Encoding ?? Encoding.Default
        };

#if NET5_0_OR_GREATER
        var config = new FtpConfig
        {
            RetryAttempts = 3,
            TimeConversion = FtpDate.LocalTime,
            ServerTimeZone = TimeZoneInfo.Utc,
            ClientTimeZone = TimeZoneInfo.Local,
            ConnectTimeout = 60000
        };
        FtpClient.Config = config;
#endif
    }

    public override string ServerDetails()
    {
        return _serverDetails;
    }
}