using Microsoft.Extensions.Logging;

namespace Linger.Logging.Abstractions;

/// <summary>
/// 日志记录接口
/// </summary>
public interface ILingerLogger : ILogger
{
    void Debug(string message, params object[] args);
    void Information(string message, params object[] args);
    void Warning(string message, params object[] args);
    void Error(string message, Exception? exception = null, params object[] args);
    void Critical(string message, Exception? exception = null, params object[] args);
}

/// <summary>
/// 泛型日志记录接口
/// </summary>
public interface ILingerLogger<T> : ILingerLogger, ILogger<T>
{
}