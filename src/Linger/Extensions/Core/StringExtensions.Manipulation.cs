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
    [return: NotNullIfNotNull(nameof(str))]
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
    [return: NotNullIfNotNull(nameof(str))]
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
        var result = str;

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
    /// Removes the specified <paramref name="value"/> as prefix and suffix (一次各移除一次) using the provided <see cref="StringComparison"/>.
    /// </summary>
    /// <param name="str">Source string.</param>
    /// <param name="value">Prefix &amp; suffix token to remove.</param>
    /// <param name="comparison">Comparison type.</param>
    /// <returns>Processed string; returns original reference if no change.</returns>
    [return: NotNullIfNotNull(nameof(str))]
    public static string? RemovePrefixAndSuffix(this string? str, string value, StringComparison comparison)
    {
        if (str is null) return null;
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(value)) return str;
        var start = str.StartsWith(value, comparison);
        var end = str.EndsWith(value, comparison);
        if (!start && !end) return str;
        var startIndex = start ? value.Length : 0;
        var endLength = end ? str.Length - value.Length : str.Length;
        if (startIndex == 0 && endLength == str.Length) return str; // defensive
        var len = endLength - startIndex;
        return len <= 0 ? string.Empty : str.Substring(startIndex, len);
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

        var actualMaxLength = maxLength - suffix.Length;
        if (actualMaxLength <= 0)
            return suffix;

#if NET6_0_OR_GREATER
        var totalLength = actualMaxLength + suffix.Length;
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
        if (value is null)
            return string.Empty;
        if (value.Length == 0)
            return string.Empty;

        // 快速检测是否包含换行符，若无则直接返回原引用，避免分配
        var firstIdx = value.IndexOfAny(WhitespaceChars.NewLineChars);
        if (firstIdx < 0)
            return value;

#if NET6_0_OR_GREATER
        ReadOnlySpan<char> src = value.AsSpan();
        // 计算非换行字符数
        var count = 0;
        for (int i = 0; i < src.Length; i++)
        {
            var c = src[i];
            if (c != '\r' && c != '\n')
                count++;
        }
        if (count == 0)
            return string.Empty;
        return string.Create(count, value, static (dest, state) =>
        {
            ReadOnlySpan<char> s = state.AsSpan();
            int w = 0;
            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c != '\r' && c != '\n')
                    dest[w++] = c;
            }
        });
#else
        var sb = new StringBuilder(value.Length - 1); // 已知至少一个换行
        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c != '\r' && c != '\n')
                sb.Append(c);
        }
        return sb.ToString();
#endif
    }

    /// <summary>
    /// Removes the last comma from the specified string.
    /// </summary>
    /// <param name="str">The string to remove the last comma from.</param>
    /// <returns>The string without the last comma.</returns>
    public static string RemoveLastComma(this string str)
    {
    return str.RemoveLastChar(',');
    }

    /// <summary>
    /// Removes the specified character from the end of the string.
    /// </summary>
    /// <param name="str">The string to remove the character from.</param>
    /// <param name="character">The character to remove.</param>
    /// <returns>The string without the specified character at the end.</returns>
    /// <remarks>
    /// This overload precisely removes one character if and only if the last character of <paramref name="str"/> equals <paramref name="character"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// "abc".RemoveLastChar('c') // => "ab"
    /// "abc".RemoveLastChar('x') // => "abc"
    /// </code>
    /// </example>
    public static string RemoveLastChar(this string str, char character)
    {
        if (string.IsNullOrEmpty(str))
            return str ?? string.Empty;
#if NET6_0_OR_GREATER
        return str[^1] == character ? str[..^1] : str;
#else
        return str[str.Length - 1] == character ? str.Substring(0, str.Length - 1) : str;
#endif
    }

    /// <summary>
    /// Removes the specified character(s) from the end of the string.
    /// </summary>
    /// <param name="str">The string to remove the character from.</param>
    /// <param name="character">The character(s) to remove.</param>
    /// <returns>The string without the specified character(s) at the end.</returns>
    /// <remarks>
    /// Behavioral note for multi-character tokens: when <paramref name="character"/> has length greater than 1, this method trims by character set semantics
    /// (equivalent to <see cref="string.TrimEnd(char[])"/>), which can be surprising for callers expecting exact suffix removal.
    /// Prefer <see cref="RemoveSuffixOnce(string, StringComparison)"/> for precise single-suffix removal with comparison control.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Single-char: precise one-character removal
    /// "abc".RemoveLastChar("c")    // => "ab"
    /// "abc".RemoveLastChar("x")    // => "abc"
    ///
    /// // Multi-char: trims by character set (legacy behavior)
    /// "abcc".RemoveLastChar("bc")  // => "a"   (because both 'b' and 'c' are trimmed from the end)
    ///
    /// // Prefer precise API for exact suffix once
    /// "abcc".RemoveSuffixOnce("bc") // => "abcc" (no exact "bc" suffix)
    /// "hello.txt".RemoveSuffixOnce(".txt") // => "hello"
    /// </code>
    /// </example>
    [Obsolete("For single-character removal prefer RemoveLastChar(char). For multi-character exact suffix removal use RemoveSuffixOnce(string, StringComparison). The multi-character overload trims by character set (legacy).", false)]
    public static string RemoveLastChar(this string str, string character)
    {
        if (str.IsNullOrEmpty() || character.IsNullOrEmpty())
        {
            return str ?? string.Empty;
        }
        if (character!.Length == 1)
        {
            var ch = character[0];
#if NET6_0_OR_GREATER
            if (str.Length > 0 && str[^1] == ch)
                return str[..^1];
#else
            if (str.Length > 0 && str[str.Length - 1] == ch)
                return str.Substring(0, str.Length - 1);
#endif
            return str; // 未匹配则返回原引用
        }
        // 保留旧语义（作为字符集合裁剪），但避免重复分配：使用内部缓存或临时 span 操作
        // 当前简化仍调用 ToCharArray，未来可考虑缓存策略
        return str.TrimEnd(character.ToCharArray());
    }

    /// <summary>
    /// 如果字符串以指定 <paramref name="suffix"/> 结尾，则精确移除一次该后缀（区分大小写可控）。
    /// </summary>
    /// <param name="str">源字符串。</param>
    /// <param name="suffix">要匹配的后缀。</param>
    /// <param name="comparison">字符串比较方式，默认 Ordinal。</param>
    /// <returns>去掉一次后缀后的字符串；若不以该后缀结尾则返回原字符串。</returns>
    public static string RemoveSuffixOnce(this string str, string suffix, StringComparison comparison = StringComparison.Ordinal)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(suffix))
            return str ?? string.Empty;
        if (str.EndsWith(suffix, comparison))
        {
#if NET6_0_OR_GREATER
            return str[..^suffix.Length];
#else
            return str.Substring(0, str.Length - suffix.Length);
#endif
        }
        return str;
    }

    /// <summary>
    /// 若不已包含指定前缀，则以指定比较方式添加该前缀。
    /// </summary>
    public static string EnsureStartsWith(this string value, string prefix, StringComparison comparison) => EnsureAffixCore(value, prefix, true, comparison);

    /// <summary>
    /// Ensures the string starts with the specified prefix.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="prefix">The prefix value to check for.</param>
    /// <returns>The string value including the prefix.</returns>
    public static string EnsureStartsWith(this string value, string prefix) => EnsureAffixCore(value, prefix, true, null);

    /// <summary>
    /// Ensures the string ends with the specified suffix.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="suffix">The suffix value to check for.</param>
    /// <returns>The string value including the suffix.</returns>
    public static string EnsureEndsWith(this string value, string suffix) => EnsureAffixCore(value, suffix, false, null);

    /// <summary>
    /// 若不已包含指定后缀，则以指定比较方式添加该后缀。
    /// </summary>
    public static string EnsureEndsWith(this string value, string suffix, StringComparison comparison) => EnsureAffixCore(value, suffix, false, comparison);

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
    [Obsolete("Use Take instead. Will be removed in 1.0.0.")]
    public static string Substring2(this string self, int length) => Take(self, length);

    /// <summary>
    /// Truncates the string to the specified length from the end, or returns the entire string if it is shorter than the specified length.
    /// </summary>
    /// <param name="self">The string to truncate.</param>
    /// <param name="length">The length to truncate to.</param>
    /// <returns>The truncated string.</returns>
    [Obsolete("Use TakeLast instead. Will be removed in 1.0.0.")]
    public static string Substring3(this string self, int length) => TakeLast(self, length);
    private static string EnsureAffixCore(string value, string affix, bool isPrefix, StringComparison? comparison)
    {
        if (value == null) return affix ?? string.Empty;
        if (affix == null) return value;
        bool has = comparison.HasValue
            ? (isPrefix ? value.StartsWith(affix, comparison.Value) : value.EndsWith(affix, comparison.Value))
            : (isPrefix ? value.StartsWith(affix) : value.EndsWith(affix));
        if (has)
            return value;
#if NET6_0_OR_GREATER
        return isPrefix
            ? string.Create(value.Length + affix.Length, (affix, value), static (span, state) =>
            {
                state.affix.AsSpan().CopyTo(span);
                state.value.AsSpan().CopyTo(span[state.affix.Length..]);
            })
            : string.Create(value.Length + affix.Length, (value, affix), static (span, state) =>
            {
                state.value.AsSpan().CopyTo(span);
                state.affix.AsSpan().CopyTo(span[state.value.Length..]);
            });
#else
        return isPrefix ? affix + value : value + affix;
#endif
    }
}

internal static class WhitespaceChars
{
    // 复用以避免重复分配数组 (用于 IndexOfAny)
    public static readonly char[] NewLineChars = ['\r', '\n'];
}
