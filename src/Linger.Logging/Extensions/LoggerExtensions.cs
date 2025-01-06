using System.Diagnostics;
using Linger.Logging.Abstractions;

namespace Linger.Logging.Extensions;

public static class LoggerExtensions
{
    /// <summary>
    /// 记录操作日志
    /// </summary>
    public static async Task LogOperationAsync(
        this ILingerLogger logger,
        string operation,
        Func<Task> action,
        params object[] args)
    {
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
    /// 创建日志作用域
    /// </summary>
    public static IDisposable BeginNamedScope(this ILingerLogger logger, string name, params object[] args)
        => logger.BeginScope(new Dictionary<string, object>
        {
            ["ScopeName"] = name,
            ["Args"] = args
        });
}

