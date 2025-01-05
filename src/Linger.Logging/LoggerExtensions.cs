namespace Linger.Logging;

/// <summary>
/// 日志扩展方法
/// </summary>
public static class LoggerExtensions
{
    public static ILingerLogger<T> CreateLogger<T>(this ILingerLoggerProvider provider)
        => new LingerLogger<T>(provider.GetLogger(typeof(T).FullName!));

    public static async Task LogOperationAsync(
        this ILingerLogger logger,
        string operation,
        Func<Task> action,
        params object[] args)
    {
        try
        {
            logger.Information($"开始{operation}", args);
            await action();
            logger.Information($"完成{operation}", args);
        }
        catch (Exception ex)
        {
            logger.Error($"{operation}失败", ex, args);
            throw;
        }
    }

    public static IDisposable BeginScope(this ILingerLogger logger, string messageFormat, params object[] args)
    {
        return new LoggerScope(logger, messageFormat, args);
    }
}

