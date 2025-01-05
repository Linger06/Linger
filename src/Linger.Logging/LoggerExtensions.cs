namespace Linger.Logging;

/// <summary>
/// ��־��չ����
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
            logger.Information($"��ʼ{operation}", args);
            await action();
            logger.Information($"���{operation}", args);
        }
        catch (Exception ex)
        {
            logger.Error($"{operation}ʧ��", ex, args);
            throw;
        }
    }

    public static IDisposable BeginScope(this ILingerLogger logger, string messageFormat, params object[] args)
    {
        return new LoggerScope(logger, messageFormat, args);
    }
}

