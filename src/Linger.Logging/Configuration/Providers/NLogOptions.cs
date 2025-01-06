namespace Linger.Logging.Configuration.Providers;

/// <summary>
/// NLog配置选项
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
    public string InternalLogLevel { get; set; } = "Error";

    /// <summary>
    /// 内部日志文件
    /// </summary>
    public string? InternalLogFile { get; set; }
}
