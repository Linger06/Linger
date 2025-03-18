using System;
using System.Net;
using System.Text;
using FluentFTP;
using Linger.FileSystem.Helpers;
using Linger.FileSystem.Remote;
using Linger.Helper;

namespace Linger.FileSystem.Ftp;

public class FtpRemoteFileSystem : FtpContext
{
    private readonly string _serverDetails;
    private readonly RemoteSystemSetting _setting;

    public FtpRemoteFileSystem(RemoteSystemSetting setting, RetryOptions? retryOptions = null)
         : base(retryOptions)
    {
        ArgumentNullException.ThrowIfNull(setting);
        
        if (string.IsNullOrEmpty(setting.Host))
            throw new ArgumentException("Host cannot be null or empty", nameof(setting.Host));

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
        // 创建FTP客户端配置
        FtpConfig config = new FtpConfig
        {
            RetryAttempts = 0, // 使用基类中的重试机制
            TimeConversion = FtpDate.LocalTime,
            ServerTimeZone = TimeZoneInfo.Utc,
            ClientTimeZone = TimeZoneInfo.Local,
            ConnectTimeout = 60000,
            StaleDataCheck = true
        };

        // 仅创建AsyncFtpClient
        var asyncClient = new AsyncFtpClient(_setting.Host, 
            new NetworkCredential(_setting.UserName, _setting.Password),
            _setting.Port);
        
        asyncClient.Config = config;
        asyncClient.Encoding = _setting.Encoding ?? Encoding.Default;
        
        FtpClient = asyncClient;
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