using Linger.Extensions.Core;

namespace Linger.Extensions.Exception;

/// <summary>
/// Extensions for <see cref="Exception"/>.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Returns the innermost <see cref="System.Exception.InnerException"/> of the given <see cref="Exception"/>.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> object.</param>
    /// <returns>The innermost <see cref="System.Exception.InnerException"/>.</returns>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     // Some code that throws an exception
    /// }
    /// catch (Exception ex)
    /// {
    ///     Exception innerEx = ex.GetInnerException();
    ///     Console.WriteLine(innerEx.Message);
    /// }
    /// </code>
    /// </example>
    public static System.Exception? GetInnerException(this System.Exception? exception)
    {
        if (exception.IsNull())
        {
            return null;
        }

        if (exception.InnerException != null)
        {
            return exception.InnerException.GetInnerException();
        }

        return exception;
    }

    /// <summary>
    /// Returns the message of the innermost <see cref="System.Exception.InnerException"/>.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> object.</param>
    /// <returns>The message of the innermost <see cref="System.Exception.InnerException"/>.</returns>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     // Some code that throws an exception
    /// }
    /// catch (Exception ex)
    /// {
    ///     string message = ex.ExceptionMessage();
    ///     Console.WriteLine(message);
    /// }
    /// </code>
    /// </example>
    public static string ExceptionMessage(this System.Exception? exception)
    {
        if (exception.IsNull())
        {
            return string.Empty;
        }

        if (exception.InnerException != null)
        {
            return exception.InnerException.ExceptionMessage();
        }

        return exception.Message;
    }

    /// <summary>
    /// Extracts the stack trace of the exception and all inner exceptions.
    /// </summary>
    /// <param name="exception">The exception to extract the stack trace from.</param>
    /// <param name="lastStackTrace">The last extracted stack trace (for recursion), String.Empty or null.</param>
    /// <param name="exCount">The count of extracted stack traces (for recursion).</param>
    /// <returns>A string containing the stack trace of the exception and all inner exceptions.</returns>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     // Some code that throws an exception
    /// }
    /// catch (Exception ex)
    /// {
    ///     string stackTrace = ex.ExtractAllStackTrace();
    ///     Console.WriteLine(stackTrace);
    /// }
    /// </code>
    /// </example>
    public static string ExtractAllStackTrace(this System.Exception exception, string? lastStackTrace = null,
        int exCount = 1)
    {
        System.Exception? ex = exception;
        const string EntryFormat = "#{0}: {1}\r\n{2}";
        // Fix the last stack trace parameter
        lastStackTrace ??= string.Empty;
        // Add the stack trace of the exception
        lastStackTrace += string.Format(EntryFormat, exCount, ex.Message, ex.StackTrace);
        if (exception.Data.Count > 0)
        {
            lastStackTrace += "\r\n    Data: ";
            foreach (var item in exception.Data)
            {
                var entry = (DictionaryEntry)item;
                lastStackTrace += $"\r\n\t{entry.Key}: {exception.Data[entry.Key]}";
            }
        }

        // Recursively add the stack trace of inner exceptions
        if ((ex = ex.InnerException) != null)
        {
            return ex.ExtractAllStackTrace($"{lastStackTrace}\r\n\r\n", ++exCount);
        }

        return lastStackTrace;
    }
}
