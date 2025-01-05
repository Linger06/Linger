using System.Collections.Concurrent;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Linger.Logging.Serilog;

/// <summary>
/// Serilog日志提供程序
/// </summary>
internal sealed class SerilogLoggerProvider : ILingerLoggerProvider, IDisposable
{
    private readonly ILogger _rootLogger;
    private readonly ConcurrentDictionary<string, ILingerLogger> _loggers = new();
    private bool _disposed;

    public SerilogLoggerProvider(LingerLoggerConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _rootLogger = CreateSerilogLogger(config);
    }

    public ILingerLogger GetLogger(string categoryName)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SerilogLoggerProvider));

        ArgumentNullException.ThrowIfNull(categoryName);

        return _loggers.GetOrAdd(categoryName, name =>
            new SerilogLogger(_rootLogger.ForContext("SourceContext", name)));
    }

    private static ILogger CreateSerilogLogger(LingerLoggerConfiguration config)
    {
        var loggerConfig = new LoggerConfiguration();
        var options = config.SerilogOptions ?? new SerilogOptions();

        // 设置基本配置
        ConfigureBasicSettings(loggerConfig, config, options);

        // 配置文件日志
        ConfigureFileLogging(loggerConfig, config);

        // 配置控制台日志
        ConfigureConsoleLogging(loggerConfig, config, options);

        // 配置Seq日志
        ConfigureSeqLogging(loggerConfig, options);

        // 配置结构化日志和自定义属性
        ConfigureEnrichment(loggerConfig, config, options);

        return loggerConfig.CreateLogger();
    }

    private static void ConfigureBasicSettings(
        LoggerConfiguration loggerConfig,
        LingerLoggerConfiguration config,
        SerilogOptions options)
    {
        loggerConfig
            .MinimumLevel.Is(MapLogLevel(options.MinimumLevel))
            .Enrich.FromLogContext();
    }

    private static void ConfigureFileLogging(
        LoggerConfiguration loggerConfig,
        LingerLoggerConfiguration config)
    {
        var logPath = config.WriteToTempPath
            ? Path.Combine(Path.GetTempPath(), config.LogPath)
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.LogPath);

        Directory.CreateDirectory(logPath);

        loggerConfig.WriteTo.File(
            Path.Combine(logPath, $"{config.SoftwareName}-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: config.RetainedFileDays,
            fileSizeLimitBytes: config.FileSizeLimitMB * 1024 * 1024,
            outputTemplate: config.OutputTemplate,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1));
    }

    private static void ConfigureConsoleLogging(
        LoggerConfiguration loggerConfig,
        LingerLoggerConfiguration config,
        SerilogOptions options)
    {
        if (config.EnableConsoleLogging)
        {
            var theme = CreateConsoleTheme(options.ConsoleTheme);
            loggerConfig.WriteTo.Console(
                theme: theme,
                outputTemplate: config.OutputTemplate);
        }
    }

    private static void ConfigureSeqLogging(
        LoggerConfiguration loggerConfig,
        SerilogOptions options)
    {
        if (options.EnableSeqLogging && !string.IsNullOrEmpty(options.SeqServerUrl))
        {
            loggerConfig.WriteTo.Seq(options.SeqServerUrl);
        }
    }

    private static void ConfigureEnrichment(
        LoggerConfiguration loggerConfig,
        LingerLoggerConfiguration config,
        SerilogOptions options)
    {
        foreach (var property in options.CustomProperties)
        {
            loggerConfig.Enrich.WithProperty(property.Key, property.Value);
        }

        if (options.EnableStructuredLogging)
        {
            loggerConfig
                .Enrich.WithProperty("Timestamp", DateTime.UtcNow)
                .Enrich.WithProperty("Application", config.SoftwareName);
        }
    }

    private static LogEventLevel MapLogLevel(LogLevel level) => level switch
    {
        LogLevel.Verbose => LogEventLevel.Verbose,
        LogLevel.Debug => LogEventLevel.Debug,
        LogLevel.Information => LogEventLevel.Information,
        LogLevel.Warning => LogEventLevel.Warning,
        LogLevel.Error => LogEventLevel.Error,
        LogLevel.Fatal => LogEventLevel.Fatal,
        LogLevel.None => LogEventLevel.Fatal,
        _ => LogEventLevel.Information
    };

    private static SystemConsoleTheme CreateConsoleTheme(ConsoleThemeSettings settings)
    {
        var themeStyles = new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
        {
            [ConsoleThemeStyle.Text] = new SystemConsoleThemeStyle { Foreground = settings.TextColor },
            [ConsoleThemeStyle.String] = new SystemConsoleThemeStyle { Foreground = settings.TextColor },
            [ConsoleThemeStyle.Number] = new SystemConsoleThemeStyle { Foreground = settings.TextColor },
            [ConsoleThemeStyle.Boolean] = new SystemConsoleThemeStyle { Foreground = settings.TextColor },
            [ConsoleThemeStyle.Scalar] = new SystemConsoleThemeStyle { Foreground = settings.TextColor },
            [ConsoleThemeStyle.LevelVerbose] = new SystemConsoleThemeStyle { Foreground = settings.DebugColor },
            [ConsoleThemeStyle.LevelDebug] = new SystemConsoleThemeStyle { Foreground = settings.DebugColor },
            [ConsoleThemeStyle.LevelInformation] = new SystemConsoleThemeStyle { Foreground = settings.InformationColor },
            [ConsoleThemeStyle.LevelWarning] = new SystemConsoleThemeStyle { Foreground = settings.WarningColor },
            [ConsoleThemeStyle.LevelError] = new SystemConsoleThemeStyle { Foreground = settings.ErrorColor },
            [ConsoleThemeStyle.LevelFatal] = new SystemConsoleThemeStyle { Foreground = settings.FatalColor }
        };

        return new SystemConsoleTheme(themeStyles);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _loggers.Clear();
        (_rootLogger as IDisposable)?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}