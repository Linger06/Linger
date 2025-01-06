using System.Collections.Concurrent;
using System.Diagnostics;
using Linger.Logging.Abstractions;
using Linger.Logging.Configuration;

namespace Linger.Logging.Core;

/// <summary>
/// 日志帮助类
/// </summary>
public static class LogHelper
{
    static LogHelper()
    {
        // 如果没有初始化，使用默认配置
        if (!LingerLoggerFactory.IsInitialized)
        {
            LingerLoggerFactory.Initialize(new LingerLoggerConfiguration
            {
                LoggerType = LoggerType.Default,  // 默认使用 Serilog
                SoftwareName = AppDomain.CurrentDomain.FriendlyName,
                EnableConsoleLogging = true
            });
        }
    }

    private static readonly ConcurrentDictionary<string, ILingerLogger> Loggers = new();

    #region Logger Factory Methods

    public static ILingerLogger GetLogger<T>() =>
        GetLogger(typeof(T).FullName!);

    public static ILingerLogger GetLogger(string name)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(name);
        return Loggers.GetOrAdd(name, LingerLoggerFactory.CreateLogger);
    }

    #endregion

    #region Log Methods

    public static void Debug(string message, params object[] args) =>
        GetLogger("Default").Debug(message, args);

    public static void Information(string message, params object[] args) =>
        GetLogger("Default").Information(message, args);

    public static void Warning(string message, params object[] args) =>
        GetLogger("Default").Warning(message, args);

    public static void Error(string message, Exception? exception = null, params object[] args) =>
        GetLogger("Default").Error(message, exception, args);

    public static void Critical(string message, Exception? exception = null, params object[] args) =>
        GetLogger("Default").Critical(message, exception, args);

    #endregion

    #region Operation Logging

    /// <summary>
    /// 记录一个同步操作的执行过程
    /// </summary>
    public static void LogOperation(
        string operation,
        Action action,
        params object[] args)
    {
        var logger = GetLogger("Operation");
        try
        {
            logger.Information($"开始{operation}", args);
            var sw = Stopwatch.StartNew();

            action();

            sw.Stop();
            logger.Information($"完成{operation}, 耗时: {sw.ElapsedMilliseconds}ms", args);
        }
        catch (Exception ex)
        {
            logger.Error($"{operation}失败", ex, args);
            throw;
        }
    }

    /// <summary>
    /// 记录一个异步操作的执行过程
    /// </summary>
    public static async Task LogOperationAsync(
        string operation,
        Func<Task> action,
        params object[] args)
    {
        var logger = GetLogger("Operation");
        try
        {
            logger.Information($"开始{operation}", args);
            var sw = Stopwatch.StartNew();

            await action();

            sw.Stop();
            logger.Information($"完成{operation}, 耗时: {sw.ElapsedMilliseconds}ms", args);
        }
        catch (Exception ex)
        {
            logger.Error($"{operation}失败", ex, args);
            throw;
        }
    }

    /// <summary>
    /// 记录一个具有返回值的异步操作的执行过程
    /// </summary>
    public static async Task<T> LogOperationAsync<T>(
        string operation,
        Func<Task<T>> action,
        params object[] args)
    {
        var logger = GetLogger("Operation");
        try
        {
            logger.Information($"开始{operation}", args);
            var sw = Stopwatch.StartNew();

            var result = await action();

            sw.Stop();
            logger.Information($"完成{operation}, 耗时: {sw.ElapsedMilliseconds}ms, 结果: {result}", args);
            return result;
        }
        catch (Exception ex)
        {
            logger.Error($"{operation}失败", ex, args);
            throw;
        }
    }

    #endregion

    #region Conditional Logging

#if DEBUG
    /// <summary>
    /// Debug模式下的日志记录
    /// </summary>
    public static void DebugOnly(string message, params object[] args) =>
        GetLogger("Debug").Debug(message, args);
#endif

    /// <summary>
    /// 条件日志记录
    /// </summary>
    public static void LogIf(bool condition, string message, params object[] args)
    {
        if (condition)
        {
            GetLogger("Conditional").Information(message, args);
        }
    }

    #endregion

    #region Scope Logging

    /// <summary>
    /// 创建一个日志作用域
    /// </summary>
    public static IDisposable BeginScope(string message, params object[] args) =>
        new LoggerScope(GetLogger("Scope"), message, args);

    private sealed class LoggerScope : IDisposable
    {
        private readonly ILingerLogger _logger;
        private readonly string _message;
        private readonly Stopwatch _stopwatch;

        public LoggerScope(ILingerLogger logger, string message, object[] args)
        {
            _logger = logger;
            _message = string.Format(message, args);
            _stopwatch = Stopwatch.StartNew();
            _logger.Debug($"开始: {_message}");
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.Debug($"结束: {_message}, 耗时: {_stopwatch.ElapsedMilliseconds}ms");
        }
    }

    #endregion
}
