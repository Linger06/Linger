using System.Collections.Concurrent;
using Linger.Logging.Abstractions;
using Linger.Logging.Configuration;

namespace Linger.Logging.Core;

/// <summary>
/// 默认日志提供程序
/// </summary>
internal sealed class DefaultLoggerProvider : ILingerLoggerProvider
{
    private readonly ConcurrentDictionary<string, ILingerLogger> _loggers = new();
    private readonly DefaultLoggerOptions _options;
    private bool _disposed;

    public DefaultLoggerProvider(LingerLoggerConfiguration config)
    {
        _options = new DefaultLoggerOptions
        {
            MinimumLevel = LogLevel.Information,
            LogPath = config.LogPath,
            WriteToTempPath = config.WriteToTempPath,
            SoftwareName = config.SoftwareName,
            EnableConsoleLogging = config.EnableConsoleLogging,
            FileSizeLimitBytes = config.FileSizeLimitMB * 1024 * 1024,
            RetainedFileCount = config.RetainedFileDays
        };
    }

    public ILingerLogger GetLogger(string categoryName)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DefaultLoggerProvider));

        return _loggers.GetOrAdd(categoryName, name => new DefaultLogger(name, _options));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _loggers.Clear();
    }
}

