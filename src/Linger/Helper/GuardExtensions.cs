using System.Diagnostics.CodeAnalysis;
using Linger.Extensions.Core;

namespace Linger.Helper;

/// <summary>
/// Extension methods for guard clauses.
/// </summary>
public static class GuardExtensions
{
    /// <summary>
    /// Ensures that the specified value is not null.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="message">The message to include in the exception if the value is null.</param>
    /// <example>
    /// <code>
    /// object obj = null;
    /// obj.EnsureIsNotNull(nameof(obj), "Object cannot be null");
    /// // Output: Throws ArgumentNullException
    /// </code>
    /// </example>
    public static void EnsureIsNotNull([NotNull] this object? value, string? paramName = null, string? message = null)
    {
        if (value == null)
            throw new ArgumentNullException(paramName ?? nameof(value), message);
    }

    /// <summary>
    /// Ensures that the specified string is not null or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="message">The message to include in the exception if the string is null or empty.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <example>
    /// <code>
    /// string str = "";
    /// str.EnsureStringIsNotNullOrEmpty("String cannot be empty", nameof(str));
    /// // Output: Throws ArgumentException
    /// </code>
    /// </example>
    public static void EnsureStringIsNotNullAndEmpty(this string? value, string? message = null, string? paramName = null)
    {
        EnsureIsNotNull(value, paramName);
        if (value.IsEmpty())
            throw new ArgumentException(message ?? "Value cannot be an empty string", paramName);
    }

    /// <summary>
    /// Ensures that the specified value is null.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="message">The message to include in the exception if the value is not null.</param>
    /// <example>
    /// <code>
    /// object obj = new object();
    /// obj.EnsureIsNull(nameof(obj), "Object must be null");
    /// // Output: Throws ArgumentException
    /// </code>
    /// </example>
    public static void EnsureIsNull(this object? value, string? paramName = null, string? message = null)
    {
        if (value != null)
            throw new ArgumentException(paramName ?? nameof(value), message);
    }

    /// <summary>
    /// Ensures that the specified file path exists.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="message">The message to include in the exception if the file does not exist.</param>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown when the file path is empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the file path is null.</exception>
    /// <example>
    /// <code>
    /// string path = "nonexistent.txt";
    /// path.EnsureFileExist(nameof(path), "File must exist");
    /// // Output: Throws FileNotFoundException
    /// </code>
    /// </example>
    public static void EnsureFileExist(this string? filePath, string? paramName = null, string? message = null)
    {
        EnsureIsNotNull(filePath);

        if (filePath.IsEmpty())
        {
            throw new ArgumentException("File path cannot be empty", paramName ?? nameof(filePath));
        }

        if (!FileHelper.IsExistFile(filePath))
        {
            throw new FileNotFoundException(message ?? "File Not Found", paramName ?? nameof(filePath));
        }
    }

    /// <summary>
    /// Ensures that the specified directory path exists.
    /// </summary>
    /// <param name="directory">The directory path to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="message">The message to include in the exception if the directory does not exist.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the directory path is null.</exception>
    /// <example>
    /// <code>
    /// string path = "nonexistent/directory";
    /// path.EnsureDirectoryExist(nameof(path), "Directory must exist");
    /// // Output: Throws DirectoryNotFoundException
    /// </code>
    /// </example>
    public static void EnsureDirectoryExist(this string? directory, string? paramName = null, string? message = null)
    {
        EnsureIsNotNull(directory);
        if (!FileHelper.IsExistDirectory(directory))
        {
            throw new DirectoryNotFoundException(message ?? $"Directory Not Found:{paramName}");
        }
    }
}