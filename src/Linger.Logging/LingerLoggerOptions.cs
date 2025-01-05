namespace Linger.Logging;

/// <summary>
/// 日志配置选项
/// </summary>
public class LingerLoggerOptions
{
    private LingerLoggerConfiguration _configuration = new();

    /// <summary>
    /// 使用Serilog
    /// </summary>
    public LingerLoggerOptions UseSerilog(Action<SerilogOptions>? configure = null)
    {
        _configuration.LoggerType = LoggerType.Serilog;
        if (configure != null)
        {
            _configuration.SerilogOptions ??= new SerilogOptions();
            configure(_configuration.SerilogOptions);
        }
        return this;
    }

    /// <summary>
    /// 使用NLog
    /// </summary>
    public LingerLoggerOptions UseNLog(Action<NLogOptions>? configure = null)
    {
        _configuration.LoggerType = LoggerType.NLog;
        if (configure != null)
        {
            _configuration.NLogOptions ??= new NLogOptions();
            configure(_configuration.NLogOptions);
        }
        return this;
    }

    /// <summary>
    /// 使用Log4Net
    /// </summary>
    public LingerLoggerOptions UseLog4Net(Action<Log4NetOptions>? configure = null)
    {
        _configuration.LoggerType = LoggerType.Log4Net;
        if (configure != null)
        {
            _configuration.Log4NetOptions ??= new Log4NetOptions();
            configure(_configuration.Log4NetOptions);
        }
        return this;
    }

    /// <summary>
    /// 使用Microsoft.Extensions.Logging
    /// </summary>
    public LingerLoggerOptions UseMicrosoftLogging()
    {
        _configuration.LoggerType = LoggerType.MicrosoftLogging;
        return this;
    }

    /// <summary>
    /// 设置应用程序名称
    /// </summary>
    public LingerLoggerOptions SetSoftwareName(string name)
    {
        _configuration.SoftwareName = name;
        return this;
    }

    /// <summary>
    /// 设置日志文件路径
    /// </summary>
    public LingerLoggerOptions SetLogPath(string path)
    {
        _configuration.LogPath = path;
        return this;
    }

    /// <summary>
    /// 设置是否写入临时目录
    /// </summary>
    public LingerLoggerOptions SetWriteToTempPath(bool writeToTemp)
    {
        _configuration.WriteToTempPath = writeToTemp;
        return this;
    }

    /// <summary>
    /// 设置是否启用控制台日志
    /// </summary>
    public LingerLoggerOptions SetEnableConsoleLogging(bool enable)
    {
        _configuration.EnableConsoleLogging = enable;
        return this;
    }

    /// <summary>
    /// 设置文件大小限制（MB）
    /// </summary>
    public LingerLoggerOptions SetFileSizeLimit(long sizeMB)
    {
        _configuration.FileSizeLimitMB = sizeMB;
        return this;
    }

    /// <summary>
    /// 设置保留文件天数
    /// </summary>
    public LingerLoggerOptions SetRetainedDays(int days)
    {
        _configuration.RetainedFileDays = days;
        return this;
    }

    /// <summary>
    /// 设置输出模板
    /// </summary>
    public LingerLoggerOptions SetOutputTemplate(string template)
    {
        _configuration.OutputTemplate = template;
        return this;
    }

    /// <summary>
    /// 构建配置
    /// </summary>
    internal LingerLoggerConfiguration Build() => _configuration;
}

