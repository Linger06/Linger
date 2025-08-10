using System.Runtime.CompilerServices;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T EnsureIsNotNull<T>([NotNull] this T? value, string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value!;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string EnsureIsNotNullOrEmpty([NotNull] this string? value, string? paramName = null)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(value, paramName);
        return value!;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string EnsureIsNotNullOrWhiteSpace([NotNull] this string? value, string? paramName = null)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value!;
    }

    [Obsolete("Use EnsureIsNotNullOrEmpty instead.")]
    public static void EnsureIsNotNullAndEmpty([NotNull] this string? value, string? paramName = null) => EnsureIsNotNullOrEmpty(value, paramName);

    [Obsolete("Use EnsureIsNotNullOrWhiteSpace instead.")]
    public static void EnsureIsNotNullAndWhiteSpace([NotNull] this string? value, string? paramName = null) => EnsureIsNotNullOrWhiteSpace(value, paramName);

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object? EnsureIsNull(this object? value, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
    {
        if (value != null)
            throw new ArgumentException(message ?? "Value should be null", paramName ?? nameof(value));
        return value;
    }

    /// <summary>
    /// Ensures that the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="message">The message to include in the exception if the condition is false.</param>
    /// <exception cref="ArgumentException">Thrown when the condition is false.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EnsureIsTrue(this bool condition, [CallerArgumentExpression(nameof(condition))] string? paramName = null, string? message = null)
    {
        if (!condition)
            throw new ArgumentException(message ?? "Condition must be true", paramName);
        return condition;
    }

    /// <summary>
    /// Ensures that the specified condition is false.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="message">The message to include in the exception if the condition is true.</param>
    /// <exception cref="ArgumentException">Thrown when the condition is true.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EnsureIsFalse(this bool condition, [CallerArgumentExpression(nameof(condition))] string? paramName = null, string? message = null)
    {
        if (condition)
            throw new ArgumentException(message ?? "Condition must be false", paramName);
        return condition;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T EnsureIsInRange<T>(this T value, T min, T max, [CallerArgumentExpression(nameof(value))] string? paramName = null, string? message = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName ?? nameof(value),
                message ?? $"Value must be between {min} and {max} (inclusive)");
        }
        return value;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string EnsureFileExists(this string? filePath, [CallerArgumentExpression(nameof(filePath))] string? paramName = null, string? message = null)
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
        return filePath!;
    }

    /// <summary>
    /// Ensures that the specified directory path exists.
    /// </summary>
    /// <param name="directory">The directory path to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="message">The message to include in the exception if the directory does not exist.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the directory path is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string EnsureDirectoryExists(this string? directory, [CallerArgumentExpression(nameof(directory))] string? paramName = null, string? message = null)
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
        return directory!;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> EnsureIsNotNullOrEmpty<T>(this IEnumerable<T>? collection, [CallerArgumentExpression(nameof(collection))] string? paramName = null, string? message = null)
    {
        EnsureIsNotNull(collection, paramName);
        // Fast paths for common collection types with O(1) Count access.
        if (collection is ICollection<T> c)
        {
            if (c.Count == 0)
            {
                throw new ArgumentException(message ?? "Collection cannot be empty", paramName ?? nameof(collection));
            }
            return collection;
        }
        if (collection is IReadOnlyCollection<T> roc)
        {
            if (roc.Count == 0)
            {
                throw new ArgumentException(message ?? "Collection cannot be empty", paramName ?? nameof(collection));
            }
            return collection;
        }

        // Fall back to enumeration only when necessary.
        using var e = collection.GetEnumerator();
        if (!e.MoveNext())
        {
            throw new ArgumentException(message ?? "Collection cannot be empty", paramName ?? nameof(collection));
        }
        return collection;
    }

    #region Obsolete Backwards Compatibility
    [Obsolete("Use EnsureFileExists instead.")]
    public static string EnsureFileExist(this string? filePath, string? paramName = null, string? message = null) => EnsureFileExists(filePath, paramName, message);

    [Obsolete("Use EnsureDirectoryExists instead.")]
    public static string EnsureDirectoryExist(this string? directory, string? paramName = null, string? message = null) => EnsureDirectoryExists(directory, paramName, message);
    #endregion
}
