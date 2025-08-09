namespace Linger.Exceptions;

/// <summary>
/// Thrown when a retry operation has exhausted the configured maximum number of attempts.
/// </summary>
public class OutOfRetryCountException(string? message, Exception? innerException) : Exception(message, innerException)
{
}

/// <summary>
/// Obsolete legacy name kept for backwards compatibility. Use <see cref="OutOfRetryCountException"/> instead.
/// </summary>
[Obsolete("Use OutOfRetryCountException instead.")]
public class OutOfReTryCountException(string? message, Exception? innerException) : OutOfRetryCountException(message, innerException)
{
}
