namespace Linger.Logging;

/// <summary>
/// 日志记录接口
/// </summary>
public interface ILingerLogger
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
public interface ILingerLogger<T> : ILingerLogger { }

/// <summary>
/// 日志提供程序接口
/// </summary>
public interface ILingerLoggerProvider : IDisposable
{
    /// <summary>
    /// 获取指定类别的日志记录器
    /// </summary>
    /// <param name="categoryName">日志类别名称</param>
    /// <returns>日志记录器实例</returns>
    ILingerLogger GetLogger(string categoryName);
}

// 在核心库中定义Provider工厂
public class LoggerProviderFactory
{
    private static readonly Dictionary<string, Func<ILingerLoggerProvider>> _providers = new();

    public static void RegisterProvider(string name, Func<ILingerLoggerProvider> factory)
    {
        _providers[name] = factory;
    }

    public static ILingerLoggerProvider CreateProvider(string name)
    {
        if (_providers.TryGetValue(name, out var factory))
        {
            return factory();
        }
        throw new ArgumentException($"Provider '{name}' not found");
    }
}