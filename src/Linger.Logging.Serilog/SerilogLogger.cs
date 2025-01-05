using Serilog;
using Serilog.Events;

namespace Linger.Logging.Serilog;

/// <summary>
/// Serilog日志实现
/// </summary>
public class SerilogLogger : ILingerLogger
{
    private readonly ILogger _logger;

    public SerilogLogger(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Debug(string message, params object[] args)
    {
        if (_logger.IsEnabled(LogEventLevel.Debug))
            _logger.Debug(message, args);
    }

    public void Information(string message, params object[] args)
    {
        if (_logger.IsEnabled(LogEventLevel.Information))
            _logger.Information(message, args);
    }

    public void Warning(string message, params object[] args)
    {
        if (_logger.IsEnabled(LogEventLevel.Warning))
            _logger.Warning(message, args);
    }

    public void Error(string message, Exception? exception = null, params object[] args)
    {
        if (!_logger.IsEnabled(LogEventLevel.Error)) return;

        if (exception != null)
            _logger.Error(exception, message, args);
        else
            _logger.Error(message, args);
    }

    public void Critical(string message, Exception? exception = null, params object[] args)
    {
        if (!_logger.IsEnabled(LogEventLevel.Fatal)) return;

        if (exception != null)
            _logger.Fatal(exception, message, args);
        else
            _logger.Fatal(message, args);
    }
}


