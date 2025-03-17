using Linger.Extensions.Core;

namespace Linger.Extensions;

/// <summary>
/// Extensions for <see cref="Exception"/>.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Returns the innermost <see cref="Exception.InnerException"/> of the given <see cref="Exception"/>.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> object.</param>
    /// <returns>The innermost <see cref="Exception.InnerException"/>.</returns>
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
    public static Exception? GetInnerException(this Exception? exception)
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
    /// Returns the message of the innermost <see cref="Exception.InnerException"/>.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> object.</param>
    /// <returns>The message of the innermost <see cref="Exception.InnerException"/>.</returns>
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
    public static string ExceptionMessage(this Exception? exception)
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
    public static string ExtractAllStackTrace(this Exception exception, string? lastStackTrace = null, int exCount = 1)
    {
        Exception? ex = exception;
        const string EntryFormat = """
            #{0}: {1}
            {2}
            """;
        // Fix the last stack trace parameter
        lastStackTrace ??= string.Empty;
        // Add the stack trace of the exception
        lastStackTrace += string.Format(EntryFormat, exCount, ex.Message, ex.StackTrace);
        if (exception.Data.Count > 0)
        {
            lastStackTrace += """

                    Data:

                """;
            foreach (var item in exception.Data)
            {
                var entry = (DictionaryEntry)item;
                lastStackTrace += $"{entry.Key}: {exception.Data[entry.Key]}";
            }
        }

        // Recursively add the stack trace of inner exceptions
        if ((ex = ex.InnerException) != null)
        {
            return ex.ExtractAllStackTrace($"""
                {lastStackTrace}


                """, ++exCount);
        }

        return lastStackTrace;
    }
}
