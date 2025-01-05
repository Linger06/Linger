namespace Linger.Logging;


/// <summary>
/// NLog特定配置选项
/// </summary>
public class NLogOptions
{
    /// <summary>
    /// 配置文件路径
    /// </summary>
    public string? ConfigFilePath { get; set; }

    /// <summary>
    /// 是否自动重载配置
    /// </summary>
    public bool AutoReload { get; set; } = true;

    /// <summary>
    /// 抛出异常时是否包含调用栈
    /// </summary>
    public bool IncludeCallSite { get; set; } = true;

    /// <summary>
    /// 是否启用内部日志
    /// </summary>
    public bool EnableInternalLog { get; set; }

    /// <summary>
    /// 内部日志级别
    /// </summary>
    public NLog.LogLevel InternalLogLevel { get; set; } = NLog.LogLevel.Error;

    /// <summary>
    /// 内部日志文件
    /// </summary>
    public string? InternalLogFile { get; set; }
}

/// <summary>
/// Log4Net特定配置选项
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
