using System.Text;
using Linger.Logging.Abstractions;

namespace Linger.Logging.Core;

/// <summary>
/// 默认日志记录器实现
/// </summary>
internal sealed class DefaultLogger : ILingerLogger
{
    private readonly string _categoryName;
    private readonly DefaultLoggerOptions _options;
    private readonly object _lock = new();

    public DefaultLogger(string categoryName, DefaultLoggerOptions options)
    {
        _categoryName = categoryName;
        _options = options;
    }

    public void Debug(string message, params object[] args) =>
        Log(LogLevel.Debug, null, message, args);

    public void Information(string message, params object[] args) =>
        Log(LogLevel.Information, null, message, args);

    public void Warning(string message, params object[] args) =>
        Log(LogLevel.Warning, null, message, args);

    public void Error(string message, Exception? exception = null, params object[] args) =>
        Log(LogLevel.Error, exception, message, args);

    public void Critical(string message, Exception? exception = null, params object[] args) =>
        Log(LogLevel.Critical, exception, message, args);

    private void Log(LogLevel level, Exception? exception, string message, params object[] args)
    {
        if (level < _options.MinimumLevel)
            return;

        var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        var timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logBuilder = new StringBuilder();

        // 构建日志条目
        logBuilder.Append($"[{timeStamp}] [{level,-11}] [{_categoryName}] {formattedMessage}");

        if (exception != null)
        {
            logBuilder.AppendLine()
                     .Append("Exception: ")
                     .AppendLine(exception.ToString());
        }

        logBuilder.AppendLine();

        // 写入控制台
        if (_options.EnableConsoleLogging)
        {
            WriteToConsole(level, logBuilder.ToString());
        }

        // 写入文件
        WriteToFile(logBuilder.ToString());
    }

    private void WriteToConsole(LogLevel level, string message)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = GetColorForLevel(level);
        Console.Write(message);
        Console.ForegroundColor = originalColor;
    }

    private void WriteToFile(string message)
    {
        var logPath = _options.WriteToTempPath
            ? Path.Combine(Path.GetTempPath(), _options.LogPath)
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _options.LogPath);

        var logFile = Path.Combine(logPath, $"{_options.SoftwareName}-.log");

        try
        {
            lock (_lock)
            {
                Directory.CreateDirectory(logPath);

                // 检查文件大小
                var fileInfo = new FileInfo(logFile);
                if (fileInfo.Exists && fileInfo.Length > _options.FileSizeLimitBytes)
                {
                    RollFile(logFile);
                }

                File.AppendAllText(logFile, message);
            }
        }
        catch
        {
            // 忽略写入失败
        }
    }

    private void RollFile(string logFile)
    {
        try
        {
            var directory = Path.GetDirectoryName(logFile)!;
            var fileName = Path.GetFileNameWithoutExtension(logFile);
            var extension = Path.GetExtension(logFile);
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var newName = Path.Combine(directory, $"{fileName}{timestamp}{extension}");

            File.Move(logFile, newName);

            // 清理旧文件
            var files = Directory.GetFiles(directory, $"{fileName}*{extension}")
                               .OrderByDescending(f => f)
                               .Skip(_options.RetainedFileCount);

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // 忽略删除失败
                }
            }
        }
        catch
        {
            // 忽略文件滚动失败
        }
    }

    private static ConsoleColor GetColorForLevel(LogLevel level) => level switch
    {
        LogLevel.Debug => ConsoleColor.Gray,
        LogLevel.Information => ConsoleColor.White,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Critical => ConsoleColor.DarkRed,
        _ => ConsoleColor.White
    };
}

