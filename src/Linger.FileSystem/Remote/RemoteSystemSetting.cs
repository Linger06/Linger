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
    
    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = null!;
    
    /// <summary>
    /// 编码
    /// </summary>
    public Encoding? Encoding { get; set; }
    
    /// <summary>
    /// 连接类型 (FTP/SFTP)
    /// </summary>
    public string Type { get; set; } = "FTP";
    
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
