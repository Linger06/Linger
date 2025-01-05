using System.Collections.Concurrent;
using System.Diagnostics;

namespace Linger.Logging;

/// <summary>
/// 日志帮助类
/// </summary>
public static class LogHelper
{
    private static readonly ConcurrentDictionary<string, ILingerLogger> Loggers = new();

    /// <summary>
    /// 获取指定名称的日志记录器
    /// </summary>
    public static ILingerLogger GetLogger(string name)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(name);
        return Loggers.GetOrAdd(name, n => LingerLoggerFactory.CreateLogger(n));
    }

    /// <summary>
    /// 获取指定类型的日志记录器
    /// </summary>
    public static ILingerLogger GetLogger<T>() =>
        GetLogger(typeof(T).FullName!);

    /// <summary>
    /// 记录一个同步操作的执行过程
    /// </summary>
    public static void LogOperation(
        this ILingerLogger logger,
        string operation,
        Action action,
        params object[] args)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNullOrEmpty(operation);
        ArgumentNullException.ThrowIfNull(action);

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
        this ILingerLogger logger,
        string operation,
        Func<Task> action,
        params object[] args)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNullOrEmpty(operation);
        ArgumentNullException.ThrowIfNull(action);

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
        this ILingerLogger logger,
        string operation,
        Func<Task<T>> action,
        params object[] args)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNullOrEmpty(operation);
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            logger.Information($"开始{operation}", args);
            var sw = Stopwatch.StartNew();

            var result = await action();

            sw.Stop();
            logger.Information($"完成{operation}, 耗时: {sw.ElapsedMilliseconds}ms", args);

            return result;
        }
        catch (Exception ex)
        {
            logger.Error($"{operation}失败", ex, args);
            throw;
        }
    }

    /// <summary>
    /// 记录一个具有返回值的同步操作的执行过程
    /// </summary>
    public static T LogOperation<T>(
        this ILingerLogger logger,
        string operation,
        Func<T> action,
        params object[] args)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNullOrEmpty(operation);
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            logger.Information($"开始{operation}", args);
            var sw = Stopwatch.StartNew();

            var result = action();

            sw.Stop();
            logger.Information($"完成{operation}, 耗时: {sw.ElapsedMilliseconds}ms", args);

            return result;
        }
        catch (Exception ex)
        {
            logger.Error($"{operation}失败", ex, args);
            throw;
        }
    }

#if DEBUG
    /// <summary>
    /// 条件输出Debug日志
    /// </summary>
    public static void LogDebug(
        this ILingerLogger logger,
        string message,
        params object[] args)
    {
        logger.Debug(message, args);
    }
#endif

    /// <summary>
    /// 记录警告日志（带异常信息）
    /// </summary>
    public static void LogWarning(
        this ILingerLogger logger,
        string message,
        Exception? exception = null,
        params object[] args)
    {
        if (exception != null)
        {
            logger.Warning($"{message}\nException: {exception.Message}", args);
        }
        else
        {
            logger.Warning(message, args);
        }
    }

    /// <summary>
    /// 记录指标数据
    /// </summary>
    public static void LogMetric(
        this ILingerLogger logger,
        string metricName,
        double value,
        Dictionary<string, object>? dimensions = null)
    {
        var message = $"Metric: {metricName}, Value: {value}";
        if (dimensions != null && dimensions.Count > 0)
        {
            message += $", Dimensions: {string.Join(", ", dimensions.Select(kv => $"{kv.Key}={kv.Value}"))}";
        }
        logger.Information(message);
    }

    /// <summary>
    /// 带作用域的日志记录
    /// </summary>
    public static IDisposable BeginScope(
        this ILingerLogger logger,
        string message,
        params object[] args)
    {
        return new LoggerScope(logger, message, args);
    }

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
}

