using System.Diagnostics.CodeAnalysis;
using Linger.Extensions.Core;
using Linger.Helper.PathHelpers;

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
    public static void EnsureIsNotNull([NotNull] this object? value, string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
    }

    /// <summary>
    /// Ensures that the specified string is not null or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <example>
    /// <code>
    /// string str = "";
    /// str.EnsureIsNotNullAndEmpty(nameof(str));
    /// // Output: Throws ArgumentNullException
    /// </code>
    /// </example>
    public static void EnsureIsNotNullAndEmpty([NotNull] this string? value, string? paramName = null)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(value, paramName);
    }

    /// <summary>
    /// Ensures that the specified string is not null or whitespace.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">Thrown when the string is null, empty, or consists only of white-space characters.</exception>
    /// <example>
    /// <code>
    /// string str = "   ";
    /// str.EnsureIsNotNullAndWhiteSpace(nameof(str));
    /// // Output: Throws ArgumentNullException
    /// </code>
    /// </example>
    public static void EnsureIsNotNullAndWhiteSpace([NotNull] this string? value, string? paramName = null)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(value, paramName);
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
            throw new ArgumentException(message ?? "Value should be null", paramName ?? nameof(value));
    }

    /// <summary>
    /// Ensures that the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="message">The message to include in the exception if the condition is false.</param>
    /// <exception cref="ArgumentException">Thrown when the condition is false.</exception>
    public static void EnsureIsTrue(this bool condition, string? paramName = null, string? message = null)
    {
        if (!condition)
            throw new ArgumentException(message ?? "Condition must be true", paramName);
    }

    /// <summary>
    /// Ensures that the specified condition is false.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="message">The message to include in the exception if the condition is true.</param>
    /// <exception cref="ArgumentException">Thrown when the condition is true.</exception>
    public static void EnsureIsFalse(this bool condition, string? paramName = null, string? message = null)
    {
        if (condition)
            throw new ArgumentException(message ?? "Condition must be false", paramName);
    }

    /// <summary>
    /// Ensures that the specified value is in the given range (inclusive).
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum acceptable value (inclusive).</param>
    /// <param name="max">The maximum acceptable value (inclusive).</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="message">The message to include in the exception if the value is out of range.</param>
    public static void EnsureIsInRange<T>(this T value, T min, T max, string? paramName = null, string? message = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(
                paramName ?? nameof(value),
                message ?? $"Value must be between {min} and {max} (inclusive)");
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
    public static void EnsureFileExist(this string? filePath, string? paramName = null, string? message = null)
    {
        EnsureIsNotNull(filePath);

        if (filePath.IsEmpty())
        {
            throw new ArgumentException("File path cannot be empty", paramName ?? nameof(filePath));
        }

        if (!StandardPathHelper.Exists(filePath, true))
        {
            throw new FileNotFoundException(message ?? $"File not found: {filePath}", paramName ?? nameof(filePath));
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
    public static void EnsureDirectoryExist(this string? directory, string? paramName = null, string? message = null)
    {
        EnsureIsNotNull(directory);

        if (directory.IsEmpty())
        {
            throw new ArgumentException("Directory path cannot be empty", paramName ?? nameof(directory));
        }

        if (!StandardPathHelper.Exists(directory, false))
        {
            throw new DirectoryNotFoundException(message ?? $"Directory not found: {directory}");
        }
    }

    /// <summary>
    /// Ensures that the specified collection is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="message">The message to include in the exception if the collection is null or empty.</param>
    /// <exception cref="ArgumentNullException">Thrown when the collection is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the collection is empty.</exception>
    public static void EnsureIsNotNullOrEmpty<T>(this IEnumerable<T>? collection, string? paramName = null, string? message = null)
    {
        EnsureIsNotNull(collection, paramName);

        if (!collection.Any())
        {
            throw new ArgumentException(message ?? "Collection cannot be empty", paramName ?? nameof(collection));
        }
    }
}