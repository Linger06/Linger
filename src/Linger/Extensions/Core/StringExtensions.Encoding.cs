using System.Security.Cryptography;
using System.Text;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
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
}
