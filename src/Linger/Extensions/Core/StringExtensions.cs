using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    /// <summary>
    /// Check if the specified string is null.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is null; otherwise, false.</returns>
    public static bool IsNull([NotNullWhen(false)] this string? value)
    {
        return value == null;
    }

    /// <summary>
    /// Check if the specified string is empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is empty; otherwise, false.</returns>
    public static bool IsEmpty(this string value)
    {
        return value == string.Empty;
    }

    /// <summary>
    /// Check if the specified string is null or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Check if the specified string is null or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is null or consists only of white-space characters; otherwise, false.</returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
    {
#if !NETFRAMEWORK || NET40_OR_GREATER
        return string.IsNullOrWhiteSpace(value);
#else
        if ((object)value == null)
        {
            return true;
        }

        for (int i = 0; i < value.Length; i++)
        {
            if (!char.IsWhiteSpace(value[i]))
            {
                return false;
            }
        }

        return true;
#endif
    }

    public static bool IsWhiteSpace(this string? value)
    {
        if (value.IsNull())
        {
            return false;
        }

        for (int i = 0; i < value.Length; i++)
        {
            if (!char.IsWhiteSpace(value[i]))
            {
                return false;
            }
        }

        return true;
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
    public static bool IsNotNullAndEmpty([NotNullWhen(true)] this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="short"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="short"/> type; otherwise, false.</returns>
    public static bool IsInt16(this string value)
    {
        return short.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to an <see cref="int"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to an <see cref="int"/> type; otherwise, false.</returns>
    public static bool IsInt(this string value)
    {
        return int.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="long"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="long"/> type; otherwise, false.</returns>
    public static bool IsInt64(this string value)
    {
        return long.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="decimal"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="decimal"/> type; otherwise, false.</returns>
    public static bool IsDecimal(this string value)
    {
        return decimal.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="float"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="float"/> type; otherwise, false.</returns>
    public static bool IsSingle(this string value)
    {
        return float.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="double"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="double"/> type; otherwise, false.</returns>
    public static bool IsDouble(this string value)
    {
        return double.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="DateTime"/> object; otherwise, false.</returns>
    public static bool IsDateTime(this string value)
    {
        return DateTime.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="bool"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="bool"/> type; otherwise, false.</returns>
    public static bool IsBoolean(this string value)
    {
        return bool.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="Guid"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="Guid"/> type; otherwise, false.</returns>
    public static bool IsGuid(this string value)
    {
        return Guid.TryParse(value, out _);
    }

    /// <summary>
    /// Check if the specified string is equivalent to a <see cref="Guid"/> type.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="format">The exact format to use when interpreting the input: "N", "D", "B", "P", or "X".</param>
    /// <returns>Returns true if the string is equivalent to a <see cref="Guid"/> type; otherwise, false.</returns>
    public static bool IsGuid(this string value, string format)
    {
        return Guid.TryParseExact(value, format, out _);
    }

    /// <summary>
    /// Determines whether the specified string is a positive integer.
    /// </summary>
    /// <param name="s">The string to validate.</param>
    /// <returns>True if the string is a positive integer; otherwise, false.</returns>
    public static bool IsPositiveInteger(this string s)
    {
        var pattern = @"^\d*$";
        return Regex.IsMatch(s, pattern);
    }

    /// <summary>
    /// Determines whether the specified string is an integer.
    /// </summary>
    /// <param name="s">The string to validate.</param>
    /// <returns>True if the string is an integer; otherwise, false.</returns>
    public static bool IsInteger(this string s)
    {
        var pattern = @"^-?\d+$";
        return Regex.IsMatch(s, pattern);
    }

    /// <summary>
    /// Determines whether the specified string is a valid number with the specified precision and scale.
    /// </summary>
    /// <param name="s">The string to validate.</param>
    /// <param name="precision">The maximum number of digits.</param>
    /// <param name="scale">The maximum number of decimal places.</param>
    /// <returns>True if the string is a valid number; otherwise, false.</returns>
    public static bool IsNumber(this string s, int precision = 32, int scale = 0)
    {
        if (precision == 0 && scale == 0)
        {
            return false;
        }

        var pattern = @"(^\d{1," + precision + "}";
        if (scale > 0)
        {
            pattern += @"\.\d{0," + scale + "}$)|" + pattern;
        }

        pattern += "$)";
        return Regex.IsMatch(s, pattern);
    }
}