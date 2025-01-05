using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace Linger.Logging;

public class Logging
{
    private const string DefaultOutputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}";

    public static void InitializeLogging(ILoggingConfiguration? config = null)
    {
        config ??= new SerilogConfig();

        InitializeLogging(config, null, null);
    }

    public static void InitializeLogging(ILoggingConfiguration config, ILogEventEnricher[]? enrichers, ILogEventSink[]? sinks)
    {
        Log.Logger = CreateLogger(config, enrichers, sinks);

        //Log.ForContext<Logging>()
        //   .Information("Configuration: {@config}", config);

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                Log.ForContext<Logging>().Error(ex, ex.Message);
            }
        };
    }

    public static ILogger CreateLogger(ILoggingConfiguration? config = null, ILogEventEnricher[]? enrichers = null,
        ILogEventSink[]? sinks = null)
    {
        config ??= new SerilogConfig();

        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var logPath = Path.Combine(baseDirectory, "Logs");

        // Use local temp path for Desktop Applications
        if (config.WriteToTempPath)
        {
            logPath = Path.Combine(Path.GetTempPath(), "project_logs");
        }

        LoggerConfiguration logConfig = new LoggerConfiguration().MinimumLevel.Debug()
            //.Enrich.WithProperty("softwareName", config.SoftwareName)
            .Enrich.FromLogContext();

        if (enrichers != null)
        {
            _ = logConfig.Enrich.With(enrichers);
        }

        _ = logConfig.WriteTo.File(Path.Combine(logPath, $"{config.SoftwareName}.log"),
            rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, outputTemplate: DefaultOutputTemplate);

        //var customThemeStyles =
        //new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
        //{
        //    [ConsoleThemeStyle.Text] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Green },
        //    [ConsoleThemeStyle.String] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
        //    [ConsoleThemeStyle.LevelVerbose] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
        //    [ConsoleThemeStyle.LevelDebug] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
        //    [ConsoleThemeStyle.LevelInformation] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
        //    [ConsoleThemeStyle.LevelWarning] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
        //    [ConsoleThemeStyle.LevelError] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
        //    [ConsoleThemeStyle.LevelFatal] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
        //};

        //var customTheme = new SystemConsoleTheme(customThemeStyles);

        if (config.EnableConsoleLogging)
        {
            _ = logConfig.WriteTo.Console(theme: SystemConsoleTheme.Literate, outputTemplate: DefaultOutputTemplate);
        }

        //if (config.EnableElasticLogging)
        //{
        //    logConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(config.LoggingEndpoint)
        //    {
        //        MinimumLogEventLevel = config.ElasticLoggingLevel,
        //        AutoRegisterTemplate = true,
        //    });
        //}

        if (sinks != null)
        {
            foreach (ILogEventSink sink in sinks)
            {
                _ = logConfig.WriteTo.Sink(sink);
            }
        }

        Logger logger = logConfig.CreateLogger();

        return logger;
    }
}