using Linger.Logging.Abstractions;
using Linger.Logging.Configuration.Providers;

namespace Linger.Logging.Configuration;

/// <summary>
/// 日志配置
/// </summary>
public class LingerLoggerConfiguration
{
    /// <summary>
    /// 日志提供程序类型
    /// </summary>
    public LoggerType LoggerType { get; set; } = LoggerType.Serilog;

    /// <summary>
    /// 应用程序名称
    /// </summary>
    public string SoftwareName { get; set; } = "Application";

    /// <summary>
    /// 日志路径
    /// </summary>
    public string LogPath { get; set; } = "Logs";

    /// <summary>
    /// 是否写入临时目录
    /// </summary>
    public bool WriteToTempPath { get; set; }

    /// <summary>
    /// 是否启用控制台日志
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// 控制台主题设置
    /// </summary>
    public ConsoleThemeSettings ConsoleTheme { get; set; } = new();

    /// <summary>
    /// 日志输出模板
    /// </summary>
    public string OutputTemplate { get; set; } =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}";

    /// <summary>
    /// 保留文件天数
    /// </summary>
    public int RetainedFileDays { get; set; } = 30;

    /// <summary>
    /// 文件大小限制(MB)
    /// </summary>
    public long FileSizeLimitMB { get; set; } = 100;

    /// <summary>
    /// Serilog特定配置
    /// </summary>
    public SerilogOptions? SerilogOptions { get; set; }

    /// <summary>
    /// NLog特定配置
    /// </summary>
    public NLogOptions? NLogOptions { get; set; }

    /// <summary>
    /// Log4Net特定配置
    /// </summary>
    public Log4NetOptions? Log4NetOptions { get; set; }

    /// <summary>
    /// Microsoft.Extensions.Logging特定配置
    /// </summary>
    public MicrosoftLoggingOptions? MicrosoftLoggingOptions { get; set; }

}
