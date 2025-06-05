#if !NET6_0_OR_GREATER
using System.Text;
#endif

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    /// <summary>
    /// Removes the specified prefix and suffix (single character) from the string.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <param name="value">The character to remove.</param>
    /// <returns>The string without the specified prefix and suffix.</returns>
    public static string? RemovePrefixAndSuffix(this string? str, char value)
    {
        return str.RemovePrefixAndSuffix(value.ToString());
    }

    /// <summary>
    /// Removes the specified prefix and suffix (multiple characters) from the string.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <param name="value">The string to remove.</param>
    /// <returns>The string without the specified prefix and suffix.</returns>    
    public static string? RemovePrefixAndSuffix(this string? str, string value)
    {
        if (str is null)
            return null;
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(value))
            return str;

#if NET6_0_OR_GREATER
        ReadOnlySpan<char> span = str.AsSpan();
        ReadOnlySpan<char> valueSpan = value.AsSpan();

        // 检查前缀
        if (span.StartsWith(valueSpan))
        {
            span = span[valueSpan.Length..];
        }

        // 检查后缀
        if (span.EndsWith(valueSpan))
        {
            span = span[..^valueSpan.Length];
        }

        return span.ToString();
#else
        string result = str;

        if (result.StartsWith(value))
        {
            result = result.Substring(value.Length);
        }

        if (result.EndsWith(value))
        {
            result = result.Substring(0, result.Length - value.Length);
        }

        return result;
#endif
    }

    /// <summary>
    /// Safely truncates a string to the specified maximum length.
    /// </summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <param name="suffix">Optional suffix to append when truncated.</param>
    /// <returns>The truncated string.</returns>
    public static string Truncate(this string input, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(input) || maxLength <= 0)
            return string.Empty;

        if (input.Length <= maxLength)
            return input;

        int actualMaxLength = maxLength - suffix.Length;
        if (actualMaxLength <= 0)
            return suffix;

#if NET6_0_OR_GREATER
        int totalLength = actualMaxLength + suffix.Length;
        return string.Create(totalLength, (input, actualMaxLength, suffix), (span, state) =>
        {
            state.input.AsSpan(0, state.actualMaxLength).CopyTo(span);
            state.suffix.AsSpan().CopyTo(span[state.actualMaxLength..]);
        });
#else
        var sb = new StringBuilder(maxLength);
        sb.Append(input, 0, actualMaxLength);
        sb.Append(suffix);
        return sb.ToString();
#endif
    }

    /// <summary>
    /// Removes the last newline character from the specified string (using Environment.NewLine).
    /// </summary>
    /// <param name="value">The string to remove the last newline from.</param>
    /// <returns>The string without the last newline character.</returns>
    public static string RemoveLastNewLine(this string value)
    {
        return value.TrimEnd('\r', '\n');
    }

    /// <summary>
    /// Removes all newline characters from the specified string.
    /// </summary>
    /// <param name="value">The string to remove all newlines from.</param>
    /// <returns>The string without any newline characters.</returns>
    public static string RemoveAllNewLine(this string? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

#if NET6_0_OR_GREATER
        if (value.Length == 0)
            return string.Empty;

        // 先计算结果长度
        int resultLength = 0;
        ReadOnlySpan<char> source = value.AsSpan();
        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] != '\r' && source[i] != '\n')
                resultLength++;
        }

        if (resultLength == value.Length)
            return value; // 没有换行符，直接返回原字符串

        if (resultLength == 0)
            return string.Empty;

        // 创建结果字符串
        return string.Create(resultLength, value, static (span, str) =>
        {
            ReadOnlySpan<char> source = str.AsSpan();
            int writeIndex = 0;

            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if (c != '\r' && c != '\n')
                {
                    span[writeIndex++] = c;
                }
            }
        });
#else
        var result = new StringBuilder(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (c != '\r' && c != '\n')
            {
                result.Append(c);
            }
        }

        return result.ToString();
#endif
    }

    /// <summary>
    /// Removes the last comma from the specified string.
    /// </summary>
    /// <param name="str">The string to remove the last comma from.</param>
    /// <returns>The string without the last comma.</returns>
    public static string RemoveLastComma(this string str)
    {
        return str.RemoveLastChar(",");
    }

    /// <summary>
    /// Removes the specified character from the end of the string.
    /// </summary>
    /// <param name="str">The string to remove the character from.</param>
    /// <param name="character">The character to remove.</param>
    /// <returns>The string without the specified character at the end.</returns>
    public static string RemoveLastChar(this string str, string character)
    {
        if (str.IsNullOrEmpty() || character.IsNullOrEmpty())
        {
            return str ?? string.Empty;
        }

        return str.TrimEnd(character.ToCharArray());
    }

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

#if NET6_0_OR_GREATER
        if (value.AsSpan().StartsWith(prefix.AsSpan()))
            return value;

        return string.Create(value.Length + prefix.Length, (prefix, value), (span, state) =>
        {
            state.prefix.AsSpan().CopyTo(span);
            state.value.AsSpan().CopyTo(span[state.prefix.Length..]);
        });
#else
        return value.StartsWith(prefix) ? value : prefix + value;
#endif
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

#if NET6_0_OR_GREATER
        if (value.AsSpan().EndsWith(suffix.AsSpan()))
            return value;

        return string.Create(value.Length + suffix.Length, (value, suffix), (span, state) =>
        {
            state.value.AsSpan().CopyTo(span);
            state.suffix.AsSpan().CopyTo(span[state.value.Length..]);
        });
#else
        return value.EndsWith(suffix) ? value : value + suffix;
#endif
    }

    /// <summary>
    /// Truncates the string to the specified length, or returns the entire string if it is shorter than the specified length.
    /// </summary>
    /// <param name="self">The string to truncate.</param>
    /// <param name="length">The length to truncate to.</param>
    /// <returns>The truncated string.</returns>
    public static string Take(this string self, int length)
    {
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
        if (self == null || length <= 0)
            return string.Empty;

        if (length >= self.Length)
            return self;

#if NET6_0_OR_GREATER
        return self.AsSpan(self.Length - length).ToString();
#else
        return self.Substring(self.Length - length);
#endif
    }

    /// <summary>
    /// Truncates the string to the specified length, or returns the entire string if it is shorter than the specified length.
    /// </summary>
    /// <param name="self">The string to truncate.</param>
    /// <param name="length">The length to truncate to.</param>
    /// <returns>The truncated string.</returns>
    [Obsolete("Use Take instead")]
    public static string Substring2(this string self, int length) => Take(self, length);

    /// <summary>
    /// Truncates the string to the specified length from the end, or returns the entire string if it is shorter than the specified length.
    /// </summary>
    /// <param name="self">The string to truncate.</param>
    /// <param name="length">The length to truncate to.</param>
    /// <returns>The truncated string.</returns>
    [Obsolete("Use TakeLast instead")]
    public static string Substring3(this string self, int length) => TakeLast(self, length);
}
