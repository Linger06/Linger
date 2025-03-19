using System.Text;
#if !NETFRAMEWORK || NET462_OR_GREATER
using System.Text.Json;
using Linger.JsonConverter;
#endif

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    /// <summary>
    /// Converts a Base64 string representation to its equivalent string using UTF-8 encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>If the Base64 string is null or empty, returns <see cref="string.Empty"/>; otherwise, returns the equivalent string.</returns>
    public static string FromBase64ToString(this string? value)
    {
        return value.FromBase64ToString(Encoding.UTF8);
    }

    /// <summary>
    /// Converts a Base64 string representation to its equivalent string using the specified encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <returns>If the Base64 string is null or empty, returns <see cref="string.Empty"/>; otherwise, returns the equivalent string.</returns>
    public static string FromBase64ToString(this string? value, Encoding encoding)
    {
        if (value.IsNullOrEmpty())
        {
            return string.Empty;
        }

        var result = Convert.FromBase64String(value);
        return encoding.GetString(result);
    }

    /// <summary>
    /// Converts the current string to its Base64 string representation using UTF-8 encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>If the string is null or empty, returns <see cref="string.Empty"/>; otherwise, returns the Base64 string representation.</returns>
    public static string ToBase64String(this string? value)
    {
        return value.ToBase64String(Encoding.UTF8);
    }

    /// <summary>
    /// Converts the current string to its Base64 string representation using the specified encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <returns>If the string is null or empty, returns <see cref="string.Empty"/>; otherwise, returns the Base64 string representation.</returns>
    public static string ToBase64String(this string? value, Encoding encoding)
    {
        if (value.IsNullOrEmpty())
        {
            return string.Empty;
        }

        var result = encoding.GetBytes(value);
        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Gets the prefix of the specified email string.
    /// </summary>
    /// <param name="value">The email string to get the prefix from.</param>
    /// <returns>If the string is null or empty, returns <see cref="string.Empty"/>; if the string is not a valid email, returns <see cref="string.Empty"/>; otherwise, returns the substring before the "@" symbol.</returns>
    public static string EmailPrefix(this string? value)
    {
        if (value.IsNullOrEmpty())
        {
            return string.Empty;
        }

        if (!value.IsEmail())
        {
            return string.Empty;
        }

        int atIndex = value.IndexOf('@');
        return atIndex > 0 ? value.Substring(0, atIndex) : string.Empty;
    }

    /// <summary>
    /// Removes the last newline character from the specified string (using Environment.NewLine).
    /// </summary>
    /// <param name="value">The string to remove the last newline from.</param>
    /// <returns>The string without the last newline character.</returns>
    public static string DelLastNewLine(this string value)
    {
        return value.TrimEnd('\r', '\n');
    }

    /// <summary>
    /// Removes all newline characters from the specified string.
    /// </summary>
    /// <param name="value">The string to remove all newlines from.</param>
    /// <returns>The string without any newline characters.</returns>
    public static string DelAllNewLine(this string? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return value.Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
    }

    /// <summary>
    /// Removes the last comma from the specified string.
    /// </summary>
    /// <param name="str">The string to remove the last comma from.</param>
    /// <returns>The string without the last comma.</returns>
    public static string DelLastComma(this string str)
    {
        return str.DelLastChar(",");
    }

    /// <summary>
    /// Removes the specified character from the end of the string.
    /// </summary>
    /// <param name="str">The string to remove the character from.</param>
    /// <param name="character">The character to remove.</param>
    /// <returns>The string without the specified character at the end.</returns>
    public static string DelLastChar(this string str, string character)
    {
        if (str.IsNullOrEmpty() || character.IsNullOrEmpty())
        {
            return str ?? string.Empty;
        }

        return str.TrimEnd(character.ToCharArray());
    }

#if !NETFRAMEWORK || NET462_OR_GREATER

    private static readonly JsonSerializerOptions s_readOptions = new()
    {
        WriteIndented = true,
        Converters = { new DataTableJsonConverter() }
    };

    /// <summary>
    /// Converts the specified JSON string to a DataTable.
    /// </summary>
    /// <param name="json">The JSON data.</param>
    /// <returns>The DataTable representation of the JSON data.</returns>
    public static DataTable? ToDataTable(this string json)
    {
        if (json.IsNullOrEmpty()) return null;
        return JsonSerializer.Deserialize<DataTable>(json, s_readOptions);
    }

#endif

#if !NET8_0_OR_GREATER
    public static bool StartsWith(this string value, char prefix)
    {
        return value.StartsWith(prefix.ToString());
    }

    public static bool EndsWith(this string value, char suffix)
    {
        return value.EndsWith(suffix.ToString());
    }
#endif

    /// <summary>
    /// Ensures the string starts with the specified prefix.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="prefix">The prefix value to check for.</param>
    /// <returns>The string value including the prefix.</returns>
    public static string EnsureStartsWith(this string value, string prefix)
    {
        if (value == null) return prefix ?? string.Empty;
        if (prefix == null) return value;

        return value.StartsWith(prefix) ? value : string.Concat(prefix, value);
    }

    /// <summary>
    /// Ensures the string ends with the specified suffix.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="suffix">The suffix value to check for.</param>
    /// <returns>The string value including the suffix.</returns>
    public static string EnsureEndsWith(this string value, string suffix)
    {
        if (value == null) return suffix ?? string.Empty;
        if (suffix == null) return value;

        return value.EndsWith(suffix) ? value : string.Concat(value, suffix);
    }

    /// <summary>
    /// Truncates the string to the specified length, or returns the entire string if it is shorter than the specified length.
    /// </summary>
    /// <param name="self">The string to truncate.</param>
    /// <param name="length">The length to truncate to.</param>
    /// <returns>The truncated string.</returns>
    public static string TruncateFromStart(this string self, int length)
    {
        // 复用 StringExtensions2.cs 中的 Truncate 方法，传入空后缀
        return self.Truncate(length, string.Empty);
    }

    /// <summary>
    /// Truncates the string to the specified length from the end, or returns the entire string if it is shorter than the specified length.
    /// </summary>
    /// <param name="self">The string to truncate.</param>
    /// <param name="length">The length to truncate to.</param>
    /// <returns>The truncated string.</returns>
    public static string TakeLast(this string self, int length)
    {
        if (self == null) return string.Empty;
        if (length <= 0) return string.Empty;

        if (length >= self.Length)
        {
            return self;
        }

        return self.Substring(self.Length - length);
    }

    // 保留旧方法名称以保持兼容性
    /// <summary>
    /// Truncates the string to the specified length, or returns the entire string if it is shorter than the specified length.
    /// </summary>
    /// <param name="self">The string to truncate.</param>
    /// <param name="length">The length to truncate to.</param>
    /// <returns>The truncated string.</returns>
    [Obsolete("Use TruncateFromStart instead")]
    public static string Substring2(this string self, int length) => TruncateFromStart(self, length);

    /// <summary>
    /// Truncates the string to the specified length from the end, or returns the entire string if it is shorter than the specified length.
    /// </summary>
    /// <param name="self">The string to truncate.</param>
    /// <param name="length">The length to truncate to.</param>
    /// <returns>The truncated string.</returns>
    [Obsolete("Use TakeLast instead")]
    public static string Substring3(this string self, int length) => TakeLast(self, length);
}
