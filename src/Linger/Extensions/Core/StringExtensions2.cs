using System.Security.Cryptography;
using System.Text;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
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