namespace Linger.Exceptions;

/// <summary>
/// Thrown when a retry operation has exhausted the configured maximum number of attempts.
/// </summary>
public class OutOfRetryCountException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OutOfRetryCountException"/> class.
    /// </summary>
    public OutOfRetryCountException() : base("Retry attempts exhausted.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutOfRetryCountException"/> class with a specified error message.
    /// </summary>
    public OutOfRetryCountException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutOfRetryCountException"/> class with a specified error message and a reference to the inner exception.
    /// </summary>
    public OutOfRetryCountException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Obsolete legacy name kept for backwards compatibility. Use <see cref="OutOfRetryCountException"/> instead.
/// </summary>
[Obsolete("Use OutOfRetryCountException instead. Will be removed in 1.0.0.")]
public class OutOfReTryCountException : OutOfRetryCountException
{
    public OutOfReTryCountException()
    { }
    public OutOfReTryCountException(string? message) : base(message) { }
    public OutOfReTryCountException(string? message, Exception? innerException) : base(message, innerException) { }
}
