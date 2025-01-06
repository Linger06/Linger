namespace Linger.Logging.Configuration.Providers;

/// <summary>
/// Log4Net配置选项
/// </summary>
public class Log4NetOptions
{
    /// <summary>
    /// 配置文件路径
    /// </summary>
    public string? ConfigFilePath { get; set; }

    /// <summary>
    /// 是否监视配置文件变化
    /// </summary>
    public bool WatchConfig { get; set; } = true;

    /// <summary>
    /// 仓库名称
    /// </summary>
    public string RepositoryName { get; set; } = "DefaultRepository";

    /// <summary>
    /// 是否使用全限定名称作为日志名
    /// </summary>
    public bool UseFullyQualifiedLoggerName { get; set; } = true;
}
