using Linger.Logging.Abstractions;

namespace Linger.Logging.Configuration.Providers;

/// <summary>
/// Microsoft.Extensions.Logging配置选项
/// </summary>
public class MicrosoftLoggingOptions
{
    /// <summary>
    /// 是否包含作用域信息
    /// </summary>
    public bool IncludeScopes { get; set; } = true;

    /// <summary>
    /// 是否使用彩色控制台
    /// </summary>
    public bool UseColoredConsole { get; set; } = true;

    /// <summary>
    /// 最小日志级别
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
}
