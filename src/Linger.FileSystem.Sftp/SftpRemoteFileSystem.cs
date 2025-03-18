using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Linger.FileSystem.Helpers;
using Linger.FileSystem.Remote;
using Linger.Helper;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Linger.FileSystem.Sftp;

public class SftpRemoteFileSystem : SftpContext
{
    private readonly string _serverDetails;
    private readonly RemoteSystemSetting _setting;

    public SftpRemoteFileSystem(RemoteSystemSetting setting, RetryOptions? retryOptions = null)
        : base(retryOptions)
    {
        ArgumentNullException.ThrowIfNull(setting);
        
        // 修复: 替换 ArgumentNullException.ThrowIfNullOrEmpty 为手动检查
        if (string.IsNullOrEmpty(setting.Host))
            throw new ArgumentException("Host cannot be null or empty", nameof(setting.Host));

        _setting = setting;
        _serverDetails = FtpHelper.ServerDetails(
            setting.Host,
            setting.Port.ToString(),
            setting.UserName,
            setting.Type);

        InitializeClient();
    }

    private void InitializeClient()
    {
        var connectionInfo = new ConnectionInfo(
            _setting.Host,
            _setting.Port,
            _setting.UserName,
            new PasswordAuthenticationMethod(_setting.UserName, _setting.Password));

        SftpClient = new SftpClient(connectionInfo);
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