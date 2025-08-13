namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    /// <summary>
    /// Check if the specified string is null.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is null; otherwise, false.</returns>
    public static bool IsNull([NotNullWhen(false)] this string? value) => value is null;

    /// <summary>
    /// Check if the specified string is empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is empty; otherwise, false.</returns>
    public static bool IsEmpty(this string value) => value == string.Empty;

    /// <summary>
    /// Check if the specified string is null or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

    /// <summary>
    /// Check if the specified string is null or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is null or consists only of white-space characters; otherwise, false.</returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Check if the specified string consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string consists only of white-space characters; otherwise, false.</returns>
    public static bool IsWhiteSpace(this string? value)
    {
        return value switch
        {
            null => false,
            "" => true,
            _ => value.All(char.IsWhiteSpace)
        };
    }

    /// <summary>
    /// Check if the specified string is not null.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is not null; otherwise, false.</returns>
    public static bool IsNotNull([NotNullWhen(true)] this string? value)
    {
        return value != null;
    }

    /// <summary>
    /// Check if the specified string is not empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is not empty; otherwise, false.</returns>
    public static bool IsNotEmpty(this string value)
    {
        return value != string.Empty;
    }

    /// <summary>
    /// Check if the specified string is not null and not empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is not null and not empty; otherwise, false.</returns>
    public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? value) => !string.IsNullOrEmpty(value);

    public static bool IsNotNullOrWhiteSpace([NotNullWhen(true)] this string? value) => !string.IsNullOrWhiteSpace(value);

    [Obsolete("Use IsNotNullOrEmpty instead. Will be removed in 1.0.0.")]
    public static bool IsNotNullAndEmpty([NotNullWhen(true)] this string? value) => IsNotNullOrEmpty(value);

    [Obsolete("Use IsNotNullOrWhiteSpace instead. Will be removed in 1.0.0.")]
    public static bool IsNotNullAndWhiteSpace([NotNullWhen(true)] this string? value) => IsNotNullOrWhiteSpace(value);
}
