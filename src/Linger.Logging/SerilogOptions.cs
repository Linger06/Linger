namespace Linger.Logging;
/// <summary>
/// Serilog特定配置选项
/// </summary>
public class SerilogOptions
{
    /// <summary>
    /// 最小日志级别
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// 是否启用结构化日志
    /// </summary>
    public bool EnableStructuredLogging { get; set; } = true;

    /// <summary>
    /// 是否启用Elasticsearch
    /// </summary>
    public bool EnableElasticLogging { get; set; }

    /// <summary>
    /// Elasticsearch端点
    /// </summary>
    public Uri? ElasticsearchEndpoint { get; set; }

    /// <summary>
    /// 是否启用Seq
    /// </summary>
    public bool EnableSeqLogging { get; set; }

    /// <summary>
    /// Seq服务器URL
    /// </summary>
    public string? SeqServerUrl { get; set; }

    /// <summary>
    /// 自定义属性
    /// </summary>
    public Dictionary<string, string> CustomProperties { get; set; } = new();

    /// <summary>
    /// 控制台主题设置
    /// </summary>
    public ConsoleThemeSettings ConsoleTheme { get; set; } = new();
}

/// <summary>
/// 控制台主题设置
/// </summary>
public class ConsoleThemeSettings
{
    public ConsoleColor TextColor { get; set; } = ConsoleColor.White;
    public ConsoleColor DebugColor { get; set; } = ConsoleColor.Gray;
    public ConsoleColor InformationColor { get; set; } = ConsoleColor.White;
    public ConsoleColor WarningColor { get; set; } = ConsoleColor.Yellow;
    public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
    public ConsoleColor FatalColor { get; set; } = ConsoleColor.DarkRed;
}
