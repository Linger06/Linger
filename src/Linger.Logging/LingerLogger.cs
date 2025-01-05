namespace Linger.Logging;

/// <summary>
/// 日志记录范围
/// </summary>
internal sealed class LoggerScope : IDisposable
{
    private readonly ILingerLogger _logger;
    private readonly string _message;

    public LoggerScope(ILingerLogger logger, string messageFormat, object[] args)
    {
        _logger = logger;
        _message = string.Format(messageFormat, args);
        _logger.Debug("开始: " + _message);
    }

    public void Dispose()
    {
        _logger.Debug("结束: " + _message);
    }
}

/// <summary>
/// 泛型日志记录器实现
/// </summary>
internal class LingerLogger<T> : ILingerLogger<T>
{
    private readonly ILingerLogger _logger;

    public LingerLogger(ILingerLogger logger)
    {
        _logger = logger;
    }

    public void Debug(string message, params object[] args)
        => _logger.Debug(message, args);

    public void Information(string message, params object[] args)
        => _logger.Information(message, args);

    public void Warning(string message, params object[] args)
        => _logger.Warning(message, args);

    public void Error(string message, Exception? exception = null, params object[] args)
        => _logger.Error(message, exception, args);

    public void Critical(string message, Exception? exception = null, params object[] args)
        => _logger.Critical(message, exception, args);
}

