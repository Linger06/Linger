using System.Net;
using System.Text;
using FluentFTP;
using Linger.FileSystem.Helpers;

namespace Linger.FileSystem.Ftp;

public class FtpRemoteFileSystem : FtpContext
{
    private readonly string _serverDetails;
    private readonly RemoteSystemSetting _setting;

    public FtpRemoteFileSystem(RemoteSystemSetting setting, RetryOptions? retryOptions = null)
         : base(retryOptions)
    {
        ArgumentNullException.ThrowIfNull(setting);
        ArgumentNullException.ThrowIfNullOrEmpty(setting.Host);

        _setting = setting;
        _serverDetails = FtpHelper.ServerDetails(
            setting.Host,
            setting.Port.ToString(),
            setting.UserName,
            setting.Type);

        InitializeFtpClient();
    }

    private void InitializeFtpClient()
    {
        var client = new FtpClient(_setting.Host)
        {
            Credentials = new NetworkCredential(_setting.UserName, _setting.Password),
            Port = _setting.Port,
            Encoding = _setting.Encoding ?? Encoding.Default
        };

#if NET5_0_OR_GREATER
        client.Config = new FtpConfig
        {
            RetryAttempts = 0, // 使用基类中的重试机制
            TimeConversion = FtpDate.LocalTime,
            ServerTimeZone = TimeZoneInfo.Utc,
            ClientTimeZone = TimeZoneInfo.Local,
            ConnectTimeout = 60000,
            StaleDataCheck = true
        };
#endif

        FtpClient = client;
    }

    public override string ServerDetails() => _serverDetails;

    protected override void HandleException(string operation, Exception ex, string? path = null)
    {
        // 增强异常信息，添加服务器详情
        var message = $"{operation} failed on {_setting.Host}:{_setting.Port}. " +
                     $"{(path != null ? $"Path: {path}. " : string.Empty)}" +
                     $"Type: {_setting.Type}";

        base.HandleException(operation, ex, message);
    }
}