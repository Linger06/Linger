using Linger.Logging.Abstractions;

namespace Linger.Logging.Core;

/// <summary>
/// 默认日志配置选项
/// </summary>
internal class DefaultLoggerOptions
{
    public LogLevel MinimumLevel { get; set; }
    public string LogPath { get; set; } = "Logs";
    public bool WriteToTempPath { get; set; }
    public string SoftwareName { get; set; } = "Application";
    public bool EnableConsoleLogging { get; set; } = true;
    public long FileSizeLimitBytes { get; set; } = 10 * 1024 * 1024; // 10MB
    public int RetainedFileCount { get; set; } = 31;
}

