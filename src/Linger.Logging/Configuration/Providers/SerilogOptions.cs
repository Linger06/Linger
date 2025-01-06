using Linger.Logging.Abstractions;

namespace Linger.Logging.Configuration.Providers;

/// <summary>
/// Serilog配置选项
/// </summary>
public class SerilogOptions
{
    /// <summary>
    /// 最小日志级别
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

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
