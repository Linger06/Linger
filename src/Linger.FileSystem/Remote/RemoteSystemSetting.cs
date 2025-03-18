using System.Security;
using System.Text;

namespace Linger.FileSystem.Remote;

/// <summary>
/// 远程文件系统连接设置
/// </summary>
public class RemoteSystemSetting
{
    /// <summary>
    /// 服务器主机地址
    /// </summary>
    public string Host { get; set; } = null!;
    
    /// <summary>
    /// 服务器端口
    /// </summary>
    public int Port { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = null!;
    
    private string? _password;
    private SecureString? _securePassword;
    
    /// <summary>
    /// 密码（不推荐直接使用字符串密码，优先使用SecurePassword）
    /// </summary>
    public string Password 
    { 
        get => _password ?? string.Empty;
        set 
        {
            _password = value;
            // 当设置明文密码时，同时更新SecureString
            if (!string.IsNullOrEmpty(value))
            {
                _securePassword = new SecureString();
                foreach (char c in value)
                {
                    _securePassword.AppendChar(c);
                }
                _securePassword.MakeReadOnly();
            }
        }
    }
    
    /// <summary>
    /// 安全密码对象
    /// </summary>
    public SecureString SecurePassword
    {
        get => _securePassword ?? new SecureString();
        set => _securePassword = value;
    }
    
    /// <summary>
    /// 编码
    /// </summary>
    public Encoding? Encoding { get; set; }
    
    /// <summary>
    /// 连接类型 (FTP/SFTP)
    /// </summary>
    public string Type { get; set; } = "FTP";
    
    /// <summary>
    /// 使用证书进行SFTP身份验证
    /// </summary>
    public string? CertificatePath { get; set; }
    
    /// <summary>
    /// 证书密码
    /// </summary>
    public string? CertificatePassphrase { get; set; }
    
    /// <summary>
    /// 连接超时（毫秒）
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30000;
    
    /// <summary>
    /// 读写超时（毫秒）
    /// </summary>
    public int OperationTimeout { get; set; } = 60000;
    
    /// <summary>
    /// 初始化FTP设置
    /// </summary>
    public static RemoteSystemSetting CreateFtp(string host, int port, string userName, string password)
    {
        return new RemoteSystemSetting
        {
            Host = host,
            Port = port,
            UserName = userName,
            Password = password,
            Type = "FTP"
        };
    }
    
    /// <summary>
    /// 初始化SFTP设置
    /// </summary>
    public static RemoteSystemSetting CreateSftp(string host, int port, string userName, string password)
    {
        return new RemoteSystemSetting
        {
            Host = host,
            Port = port,
            UserName = userName,
            Password = password,
            Type = "SFTP"
        };
    }
}
