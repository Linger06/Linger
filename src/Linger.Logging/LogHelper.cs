using Serilog;

namespace Linger.Logging;

public static class LogHelper
{
    static LogHelper()
    {
        Logging.InitializeLogging();
    }

    public static void Info(string msg)
    {
        Log.Information(msg);
    }

    public static void Info(string messageTemplate, params object?[]? propertyValues)
    {
        Log.Information(messageTemplate, propertyValues);
    }

    public static void Error(string msg)
    {
        Log.Error(msg);
    }

    public static void Error(Exception? exception, string messageTemplate)
    {
        Log.Error(exception, messageTemplate);
    }

    public static void Error(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        Log.Error(exception, messageTemplate, propertyValues);
    }

    public static void Debug(string msg)
    {
        Log.Debug(msg);
    }

    public static void Debug(string messageTemplate, params object?[]? propertyValues)
    {
        Log.Debug(messageTemplate, propertyValues);
    }

    public static void Warning(string msg)
    {
        Log.Warning(msg);
    }

    public static void Warning(string messageTemplate, params object?[]? propertyValues)
    {
        Log.Warning(messageTemplate, propertyValues);
    }
}