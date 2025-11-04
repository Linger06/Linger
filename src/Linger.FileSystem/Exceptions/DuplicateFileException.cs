namespace Linger.FileSystem.Exceptions;

/// <summary>
/// Thrown when attempting to create a file that already exists.
/// </summary>
public class DuplicateFileException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateFileException"/> class.
    /// </summary>
    public DuplicateFileException() : base("File already exists.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateFileException"/> class with a specified error message.
    /// </summary>
    public DuplicateFileException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateFileException"/> class with a specified error message and a reference to the inner exception.
    /// </summary>
    public DuplicateFileException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
