using Linger.Logging.Abstractions;
using Linger.Logging.Core;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Linger.Logging.Serilog;

/// <summary>
/// Serilog日志实现
/// </summary>
internal class SerilogLogger : LoggerBase
{
    private readonly global::Serilog.ILogger _logger;

    public SerilogLogger(global::Serilog.ILogger logger, string categoryName)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNullOrEmpty(categoryName);

        _logger = logger.ForContext("SourceContext", categoryName);
    }

    public override IDisposable? BeginScope<TState>(TState state)  // 移除了 where TState : notnull 约束
    {
        if (state == null) return null;

        var messageTemplate = state.ToString();
        return _logger
            .ForContext("Scope", messageTemplate)
            .BeginScopeWith(messageTemplate);
    }

    public override bool IsEnabled(MSLogLevel logLevel)
    {
        var serilogLevel = MapToSerilogLevel(logLevel);
        return _logger.IsEnabled(serilogLevel);
    }

    public override void Log<TState>(
        MSLogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        ArgumentNullException.ThrowIfNull(formatter);

        var level = MapToSerilogLevel(logLevel);
        var message = formatter(state, exception);

        if (exception != null)
            _logger.Write(level, exception, message);
        else
            _logger.Write(level, message);
    }

    public override void Debug(string message, params object[] args)
    {
        if (_logger.IsEnabled(LogEventLevel.Debug))
            _logger.Debug(message, args);
    }

    public override void Information(string message, params object[] args)
    {
        if (_logger.IsEnabled(LogEventLevel.Information))
            _logger.Information(message, args);
    }

    public override void Warning(string message, params object[] args)
    {
        if (_logger.IsEnabled(LogEventLevel.Warning))
            _logger.Warning(message, args);
    }

    public override void Error(string message, Exception? exception = null, params object[] args)
    {
        if (!_logger.IsEnabled(LogEventLevel.Error)) return;

        if (exception != null)
            _logger.Error(exception, message, args);
        else
            _logger.Error(message, args);
    }

    public override void Critical(string message, Exception? exception = null, params object[] args)
    {
        if (!_logger.IsEnabled(LogEventLevel.Fatal)) return;

        if (exception != null)
            _logger.Fatal(exception, message, args);
        else
            _logger.Fatal(message, args);
    }

    private static LogEventLevel MapToSerilogLevel(MSLogLevel logLevel) => logLevel switch
    {
        MSLogLevel.Trace => LogEventLevel.Verbose,
        MSLogLevel.Debug => LogEventLevel.Debug,
        MSLogLevel.Information => LogEventLevel.Information,
        MSLogLevel.Warning => LogEventLevel.Warning,
        MSLogLevel.Error => LogEventLevel.Error,
        MSLogLevel.Critical => LogEventLevel.Fatal,
        MSLogLevel.None => LogEventLevel.Fatal,
        _ => LogEventLevel.Information
    };
}

/// <summary>
/// Serilog日志作用域扩展
/// </summary>
internal static class SerilogLoggerScopeExtensions
{
    private sealed class SerilogLoggerScope : IDisposable
    {
        private readonly IDisposable _disposable;

        public SerilogLoggerScope(IDisposable disposable)
        {
            _disposable = disposable;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }

    public static IDisposable BeginScopeWith(this global::Serilog.ILogger logger, string messageTemplate)
    {
        var pushProperty = global::Serilog.Context.LogContext.PushProperty("Scope", messageTemplate);
        return new SerilogLoggerScope(pushProperty);
    }
}

/// <summary>
/// Serilog泛型日志实现
/// </summary>
internal class SerilogLogger<T> : SerilogLogger, ILingerLogger<T>
{
    public SerilogLogger(global::Serilog.ILogger logger)
        : base(logger, typeof(T).FullName!)
    {
    }
}
