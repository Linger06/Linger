using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    /// <summary>
    /// Removes all white spaces from the specified string.
    /// </summary>
    /// <param name="value">The specified string.</param>
    /// <returns>The string without any white spaces.</returns>
    public static string RemoveAllSpaces(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return Regex.Replace(value, @"\s+", "");
    }

    /// <summary>
    /// Removes parentheses (round brackets) and their contents from the string.
    /// Handles nested brackets correctly.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <returns>The string without parentheses and their contents.</returns>
    public static string DeleteBrackets(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

        // 替换中文括号为英文括号
        var str = value.Replace("（", "(").Replace("）", ")");

        // 使用平衡组处理可能的嵌套括号
        return Regex.Replace(str, @"\((?:[^()]+|(?<open>\()|(?<-open>\)))*(?(open)(?!))\)", "");
    }

    /// <summary>
    /// Extracts the content within the first pair of parentheses in the string.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <param name="includeBrackets">Whether to include the brackets in the result.</param>
    /// <returns>The content within the first pair of parentheses, or the original string if no complete brackets are found.</returns>
    public static string? GetBracketsContent(this string? value, bool includeBrackets = false)
    {
        if (value.IsNullOrEmpty())
            return value;

        // 替换中文括号为英文括号
        var normalized = value.Replace("（", "(").Replace("）", ")");

        int startIndex = normalized.IndexOf('(');
        int endIndex = normalized.IndexOf(')', startIndex + 1);

        if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
            return value;

        if (includeBrackets)
            return normalized.Substring(startIndex, endIndex - startIndex + 1);

        return normalized.Substring(startIndex + 1, endIndex - startIndex - 1);
    }

    /// <summary>
    /// Extracts all contents within parentheses in the string.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <returns>Array of contents found within parentheses.</returns>
    public static string[] GetAllBracketsContents(this string? value)
    {
        if (value.IsNullOrEmpty())
            return [];

        // 替换中文括号为英文括号
        var normalized = value.Replace("（", "(").Replace("）", ")");

        var matches = Regex.Matches(normalized, @"\(([^()]*)\)");
        return matches.Cast<Match>()
                     .Select(m => m.Groups[1].Value)
                     .ToArray();
    }

    /// <summary>
    /// Removes the specified prefix and suffix (single character) from the string.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <param name="value">The character to remove.</param>
    /// <returns>The string without the specified prefix and suffix.</returns>
    public static string DelPrefixAndSuffix(this string str, char value)
    {
        return str.DelPrefixAndSuffix(value.ToString());
    }

    /// <summary>
    /// Removes the specified prefix and suffix (multiple characters) from the string.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <param name="value">The string to remove.</param>
    /// <returns>The string without the specified prefix and suffix.</returns>
    public static string DelPrefixAndSuffix(this string str, string value)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(value))
            return str;

        if (str.StartsWith(value))
        {
            str = str.Substring(value.Length);
        }

        if (str.EndsWith(value))
        {
            str = str.Substring(0, str.LastIndexOf(value, StringComparison.Ordinal));
        }

        return str;
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

        return $"{input.Substring(0, actualMaxLength)}{suffix}";
    }

    /// <summary>
    /// Converts a string to its MD5 hash byte array.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>MD5 hash byte array.</returns>
    public static byte[] ToMd5HashByte(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return [];

        var inputBytes = Encoding.UTF8.GetBytes(input);
#if NET5_0_OR_GREATER
        return MD5.HashData(inputBytes);
#else
        using var md5 = MD5.Create();
        return md5.ComputeHash(inputBytes);
#endif
    }

    /// <summary>
    /// Converts a string to its MD5 hash string representation.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>MD5 hash string or empty string if input is null or empty.</returns>
    public static string ToMd5HashCode(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var hashBytes = input.ToMd5HashByte();
        return hashBytes.ToMd5HashCode();
    }

    /// <summary>
    /// Converts a string to its SHA256 hash byte array.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>SHA256 hash byte array.</returns>
    public static byte[] ToSha256HashByte(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return [];

        var inputBytes = Encoding.UTF8.GetBytes(input);
#if NET5_0_OR_GREATER
        return SHA256.HashData(inputBytes);
#else
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(inputBytes);
#endif
    }

    /// <summary>
    /// Converts a string to its SHA256 hash string representation.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>SHA256 hash string.</returns>
    public static string ToSha256HashCode(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var hashBytes = input.ToSha256HashByte();
#if NET9_0_OR_GREATER
        return Convert.ToHexStringLower(hashBytes);
#else
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
#endif
    }
}