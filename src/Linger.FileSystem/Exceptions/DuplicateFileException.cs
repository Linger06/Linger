namespace Linger.FileSystem.Exceptions;

/// <summary>
/// Thrown when attempting to create a file that already exists.
/// </summary>
public class DuplicateFileException : FileSystemException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateFileException"/> class.
    /// </summary>
    public DuplicateFileException() : base("DuplicateFile", "File already exists.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateFileException"/> class with a specified file path.
    /// </summary>
    /// <param name="filePath">The path of the duplicate file.</param>
    public DuplicateFileException(string? filePath) : base("DuplicateFile", filePath, $"File already exists: {filePath}")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateFileException"/> class with a specified file path and inner exception.
    /// </summary>
    /// <param name="filePath">The path of the duplicate file.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DuplicateFileException(string? filePath, Exception? innerException) 
        : base("DuplicateFile", filePath, $"File already exists: {filePath}", innerException)
    {
    }
}
