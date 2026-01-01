using System.Text;
using Linger.Helper;

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
    /// <remarks>
    /// ⚠️ 安全建议：
    /// <list type="bullet">
    /// <item>避免在代码中硬编码密码</item>
    /// <item>生产环境使用密钥管理服务（如 Azure Key Vault、AWS Secrets Manager）</item>
    /// <item>使用环境变量或加密的配置文件</item>
    /// <item>优先考虑使用证书认证（<see cref="CertificatePath"/>）而不是密码</item>
    /// </list>
    /// </remarks>
    public string Password { get; set; } = string.Empty;

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
    /// 批量操作的最大并发度（默认 1 表示串行）。
    /// </summary>
    /// <remarks>
    /// 并发度过高可能触发服务器限流或连接数限制。
    /// 建议在 FTP/SFTP 服务端允许的范围内谨慎调整。
    /// </remarks>
    public int MaxDegreeOfParallelism { get; set; } = 1;

    /// <summary>
    /// 连接池中连接的最大空闲时间（可选）。
    /// </summary>
    /// <remarks>
    /// <para>超过此时间未使用的连接会在下次租借时被丢弃并重新创建。</para>
    /// <para>设置为 <c>null</c> 表示不限制空闲时间（默认）。</para>
    /// <para>建议设置为 1-5 分钟，以平衡连接复用和资源释放。</para>
    /// </remarks>
    public TimeSpan? ConnectionPoolIdleTimeout { get; set; }

    /// <summary>
    /// 批量操作的重试选项（可选）。
    /// </summary>
    /// <remarks>
    /// <para>当批量上传/下载/删除时，若单个文件操作失败，可自动重试。</para>
    /// <para>设置为 <c>null</c> 表示不进行重试（默认）。</para>
    /// <para>示例：</para>
    /// <code>
    /// BatchRetryOptions = new RetryOptions
    /// {
    ///     MaxRetryAttempts = 3,
    ///     DelayMilliseconds = 1000,
    ///     UseExponentialBackoff = true
    /// }
    /// </code>
    /// </remarks>
    public RetryOptions? BatchRetryOptions { get; set; }

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

    /// <summary>
    /// 初始化SFTP设置（使用私钥文件）
    /// </summary>
    public static RemoteSystemSetting CreateSftpWithCertificate(string host, int port, string userName, string certificatePath, string? certificatePassphrase = null)
    {
        return new RemoteSystemSetting
        {
            Host = host,
            Port = port,
            UserName = userName,
            CertificatePath = certificatePath,
            CertificatePassphrase = certificatePassphrase,
            Type = "SFTP"
        };
    }

    /// <summary>
    /// 创建连接设置的克隆
    /// </summary>
    public RemoteSystemSetting Clone()
    {
        return new RemoteSystemSetting
        {
            Host = Host,
            Port = Port,
            UserName = UserName,
            Password = Password,
            Type = Type,
            CertificatePath = CertificatePath,
            CertificatePassphrase = CertificatePassphrase,
            ConnectionTimeout = ConnectionTimeout,
            OperationTimeout = OperationTimeout,
            Encoding = Encoding,
            MaxDegreeOfParallelism = MaxDegreeOfParallelism,
            ConnectionPoolIdleTimeout = ConnectionPoolIdleTimeout,
            BatchRetryOptions = BatchRetryOptions
        };
    }

    /// <summary>
    /// 检查连接设置是否有效
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Host) || Port <= 0 || string.IsNullOrEmpty(UserName))
            return false;

        // 检查认证方式：必须有密码或者证书路径
        return !string.IsNullOrEmpty(Password) || !string.IsNullOrEmpty(CertificatePath);
    }

    /// <summary>
    /// 获取不包含敏感信息的连接字符串（用于日志记录）
    /// </summary>
    public string ToSafeString()
    {
        return $"{Type}://{UserName}@{Host}:{Port}";
    }
}
