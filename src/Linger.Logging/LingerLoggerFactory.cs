namespace Linger.Logging;

public static class LingerLoggerFactory
{
    private static readonly object Lock = new();
    private static volatile ILingerLoggerProvider? _provider;
    private static readonly Dictionary<LoggerType, Func<LingerLoggerConfiguration, ILingerLoggerProvider>> _providerFactories = new();

    /// <summary>
    /// 注册日志提供程序工厂
    /// </summary>
    public static void RegisterProvider(LoggerType type, Func<LingerLoggerConfiguration, ILingerLoggerProvider> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _providerFactories[type] = factory;
    }

    public static ILingerLogger CreateLogger<T>()
        => CreateLogger(typeof(T).FullName!);

    public static ILingerLogger CreateLogger(string categoryName)
    {
        if (_provider == null)
            throw new InvalidOperationException("Logger not initialized. Call Initialize() first.");

        return _provider.GetLogger(categoryName);
    }

    public static void Initialize(LingerLoggerConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (_provider != null) return;

        lock (Lock)
        {
            if (_provider != null) return;

            if (!_providerFactories.TryGetValue(config.LoggerType, out var factory))
            {
                throw new ArgumentException($"No provider registered for type: {config.LoggerType}");
            }

            _provider = factory(config);
        }
    }

    public static void Shutdown()
    {
        if (_provider == null) return;

        lock (Lock)
        {
            if (_provider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _provider = null;
        }
    }
}
