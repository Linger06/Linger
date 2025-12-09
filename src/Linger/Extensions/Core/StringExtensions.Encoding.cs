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
        {
            return [];
        }

        var inputBytes = Encoding.UTF8.GetBytes(input);
        return MD5.HashData(inputBytes);
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
        {
            return [];
        }

        var inputBytes = Encoding.UTF8.GetBytes(input);
        return SHA256.HashData(inputBytes);
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
        return Convert.ToHexStringLower(hashBytes);
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

    /// <summary>
    /// Converts the current string to its URL-safe Base64 string representation using UTF-8 encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>If the string is null or empty, returns <see cref="string.Empty"/>; otherwise, returns the URL-safe Base64 string representation.</returns>
    /// <remarks>
    /// URL-safe Base64 replaces '+' with '-' and '/' with '_', and removes padding '=' characters.
    /// This makes the output safe for use in URLs without additional encoding.
    /// </remarks>
    /// <example>
    /// <code>
    /// string value = "Hello World!";
    /// string base64Url = value.ToBase64UrlString();
    /// // base64Url is "SGVsbG8gV29ybGQh" (URL-safe)
    /// </code>
    /// </example>
    public static string ToBase64UrlString(this string? value)
    {
        return value.ToBase64UrlString(Encoding.UTF8);
    }

    /// <summary>
    /// Converts the current string to its URL-safe Base64 string representation using the specified encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <returns>If the string is null or empty, returns <see cref="string.Empty"/>; otherwise, returns the URL-safe Base64 string representation.</returns>
    /// <remarks>
    /// URL-safe Base64 replaces '+' with '-' and '/' with '_', and removes padding '=' characters.
    /// This makes the output safe for use in URLs without additional encoding.
    /// </remarks>
    /// <example>
    /// <code>
    /// string value = "Hello World!";
    /// string base64Url = value.ToBase64UrlString(Encoding.UTF8);
    /// // base64Url is "SGVsbG8gV29ybGQh" (URL-safe)
    /// </code>
    /// </example>
    public static string ToBase64UrlString(this string? value, Encoding encoding)
    {
        if (value.IsNullOrEmpty())
        {
            return string.Empty;
        }

        var bytes = encoding.GetBytes(value);
        return bytes.ToBase64UrlString();
    }

    /// <summary>
    /// Converts a URL-safe Base64 encoded string to its original string representation using UTF-8 encoding.
    /// </summary>
    /// <param name="value">The URL-safe Base64 encoded string to convert.</param>
    /// <returns>If the Base64 string is null or empty, returns <see cref="string.Empty"/>; otherwise, returns the equivalent string.</returns>
    /// <example>
    /// <code>
    /// string base64Url = "SGVsbG8gV29ybGQh";
    /// string original = base64Url.FromBase64UrlToString();
    /// // original is "Hello World!"
    /// </code>
    /// </example>
    public static string FromBase64UrlToString(this string? value)
    {
        return value.FromBase64UrlToString(Encoding.UTF8);
    }

    /// <summary>
    /// Converts a URL-safe Base64 encoded string to its original string representation using the specified encoding.
    /// </summary>
    /// <param name="value">The URL-safe Base64 encoded string to convert.</param>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <returns>If the Base64 string is null or empty, returns <see cref="string.Empty"/>; otherwise, returns the equivalent string.</returns>
    /// <example>
    /// <code>
    /// string base64Url = "SGVsbG8gV29ybGQh";
    /// string original = base64Url.FromBase64UrlToString(Encoding.UTF8);
    /// // original is "Hello World!"
    /// </code>
    /// </example>
    public static string FromBase64UrlToString(this string? value, Encoding encoding)
    {
        if (value.IsNullOrEmpty())
        {
            return string.Empty;
        }

        var bytes = System.Buffers.Text.Base64Url.DecodeFromChars(value.AsSpan());
        return encoding.GetString(bytes);
    }

    /// <summary>
    /// Converts a URL-safe Base64 encoded string to a byte array.
    /// </summary>
    /// <param name="value">The URL-safe Base64 encoded string to convert.</param>
    /// <returns>If the Base64 string is null or empty, returns an empty byte array; otherwise, returns the decoded byte array.</returns>
    /// <example>
    /// <code>
    /// string base64Url = "AQIDBA";
    /// byte[] bytes = base64Url.FromBase64UrlToBytes();
    /// // bytes is { 1, 2, 3, 4 }
    /// </code>
    /// </example>
    public static byte[] FromBase64UrlToBytes(this string? value)
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }

        return System.Buffers.Text.Base64Url.DecodeFromChars(value.AsSpan());
    }
}
