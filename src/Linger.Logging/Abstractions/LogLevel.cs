namespace Linger.Logging.Abstractions;

/// <summary>
/// 日志级别
/// </summary>
public enum LogLevel
{
    Verbose,
    Debug,
    Information,
    Warning,
    Error,
    Fatal,
    Critical,
    None
}

/// <summary>
/// 日志提供程序类型
/// </summary>
public enum LoggerType
{
    // <summary>
    /// 默认日志实现
    /// </summary>
    Default,
    Serilog,
    NLog,
    Log4Net,
    MicrosoftLogging,
    Custom
}
