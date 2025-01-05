namespace Linger.Logging;

/// <summary>
/// ��־����ѡ��
/// </summary>
public class LingerLoggerOptions
{
    private LingerLoggerConfiguration _configuration = new();

    /// <summary>
    /// ʹ��Serilog
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
    /// ʹ��NLog
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
    /// ʹ��Log4Net
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
    /// ʹ��Microsoft.Extensions.Logging
    /// </summary>
    public LingerLoggerOptions UseMicrosoftLogging()
    {
        _configuration.LoggerType = LoggerType.MicrosoftLogging;
        return this;
    }

    /// <summary>
    /// ����Ӧ�ó�������
    /// </summary>
    public LingerLoggerOptions SetSoftwareName(string name)
    {
        _configuration.SoftwareName = name;
        return this;
    }

    /// <summary>
    /// ������־�ļ�·��
    /// </summary>
    public LingerLoggerOptions SetLogPath(string path)
    {
        _configuration.LogPath = path;
        return this;
    }

    /// <summary>
    /// �����Ƿ�д����ʱĿ¼
    /// </summary>
    public LingerLoggerOptions SetWriteToTempPath(bool writeToTemp)
    {
        _configuration.WriteToTempPath = writeToTemp;
        return this;
    }

    /// <summary>
    /// �����Ƿ����ÿ���̨��־
    /// </summary>
    public LingerLoggerOptions SetEnableConsoleLogging(bool enable)
    {
        _configuration.EnableConsoleLogging = enable;
        return this;
    }

    /// <summary>
    /// �����ļ���С���ƣ�MB��
    /// </summary>
    public LingerLoggerOptions SetFileSizeLimit(long sizeMB)
    {
        _configuration.FileSizeLimitMB = sizeMB;
        return this;
    }

    /// <summary>
    /// ���ñ����ļ�����
    /// </summary>
    public LingerLoggerOptions SetRetainedDays(int days)
    {
        _configuration.RetainedFileDays = days;
        return this;
    }

    /// <summary>
    /// �������ģ��
    /// </summary>
    public LingerLoggerOptions SetOutputTemplate(string template)
    {
        _configuration.OutputTemplate = template;
        return this;
    }

    /// <summary>
    /// ��������
    /// </summary>
    internal LingerLoggerConfiguration Build() => _configuration;
}

