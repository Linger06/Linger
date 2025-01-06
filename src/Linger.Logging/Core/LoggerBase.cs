using Linger.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using LogLevel = Linger.Logging.Abstractions.LogLevel;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;  // 为Microsoft的LogLevel创建别名

namespace Linger.Logging.Core;

/// <summary>
/// 基础日志适配器
/// </summary>
public abstract class LoggerBase : ILingerLogger
{
    /// <summary>
    /// 开始日志作用域
    /// </summary>
    public abstract IDisposable? BeginScope<TState>(TState state) where TState : notnull;

    /// <summary>
    /// 检查日志级别是否启用
    /// </summary>
    public abstract bool IsEnabled(MSLogLevel logLevel);

    /// <summary>
    /// 记录日志
    /// </summary>
    public abstract void Log<TState>(
        MSLogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter);

    /// <summary>
    /// 记录调试信息
    /// </summary>
    public virtual void Debug(string message, params object[] args)
    {
        if (IsEnabled(MSLogLevel.Debug))
            Log(MSLogLevel.Debug, default, args, null, (s, e) => string.Format(message, s));
    }

    /// <summary>
    /// 记录一般信息
    /// </summary>
    public virtual void Information(string message, params object[] args)
    {
        if (IsEnabled(MSLogLevel.Information))
            Log(MSLogLevel.Information, default, args, null, (s, e) => string.Format(message, s));
    }

    /// <summary>
    /// 记录警告信息
    /// </summary>
    public virtual void Warning(string message, params object[] args)
    {
        if (IsEnabled(MSLogLevel.Warning))
            Log(MSLogLevel.Warning, default, args, null, (s, e) => string.Format(message, s));
    }

    /// <summary>
    /// 记录错误信息
    /// </summary>
    public virtual void Error(string message, Exception? exception = null, params object[] args)
    {
        if (IsEnabled(MSLogLevel.Error))
            Log(MSLogLevel.Error, default, args, exception, (s, e) => string.Format(message, s));
    }

    /// <summary>
    /// 记录严重错误信息
    /// </summary>
    public virtual void Critical(string message, Exception? exception = null, params object[] args)
    {
        if (IsEnabled(MSLogLevel.Critical))
            Log(MSLogLevel.Critical, default, args, exception, (s, e) => string.Format(message, s));
    }

    /// <summary>
    /// 将 Microsoft.Extensions.Logging.LogLevel 映射到 Linger.Logging.LogLevel
    /// </summary>
    protected static LogLevel MapToLingerLogLevel(MSLogLevel logLevel) => logLevel switch
    {
        MSLogLevel.Trace => LogLevel.Verbose,
        MSLogLevel.Debug => LogLevel.Debug,
        MSLogLevel.Information => LogLevel.Information,
        MSLogLevel.Warning => LogLevel.Warning,
        MSLogLevel.Error => LogLevel.Error,
        MSLogLevel.Critical => LogLevel.Critical,
        MSLogLevel.None => LogLevel.None,
        _ => LogLevel.Information
    };

    /// <summary>
    /// 将 Linger.Logging.LogLevel 映射到 Microsoft.Extensions.Logging.LogLevel
    /// </summary>
    protected static MSLogLevel MapToMicrosoftLogLevel(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Verbose => MSLogLevel.Trace,
        LogLevel.Debug => MSLogLevel.Debug,
        LogLevel.Information => MSLogLevel.Information,
        LogLevel.Warning => MSLogLevel.Warning,
        LogLevel.Error => MSLogLevel.Error,
        LogLevel.Critical => MSLogLevel.Critical,
        LogLevel.Fatal => MSLogLevel.Critical,
        LogLevel.None => MSLogLevel.None,
        _ => MSLogLevel.Information
    };
}
