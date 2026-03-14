#pragma warning disable
#if !NET9_0_OR_GREATER

namespace System.Buffers.Text;

/// <summary>
/// Provides Base64Url polyfill for .NET versions before 10.0.
/// Converts between binary data and URL-safe ASCII encoded text that's represented in Base64Url characters.
/// </summary>
/// <remarks>
/// Base64Url encoding uses the same alphabet as standard Base64 encoding, except
/// that the characters '+' and '/' are replaced with '-' and '_' respectively to make
/// the output URL-safe. Padding characters '=' are also removed.
/// </remarks>
public static class Base64Url
{
    /// <summary>
    /// Encodes the span of binary data into a string represented as Base64Url ASCII chars.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <returns>A string containing the Base64Url encoded data.</returns>
    /// <example>
    /// <code>
    /// byte[] data = { 1, 2, 3 };
    /// string encoded = Base64Url.EncodeToString(data);
    /// // encoded is "AQID"
    /// </code>
    /// </example>
    public static string EncodeToString(ReadOnlySpan<byte> source)
    {
        if (source.IsEmpty)
        {
            return string.Empty;
        }

        return Convert.ToBase64String(source.ToArray())
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>
    /// Decodes the span of unicode ASCII chars represented as Base64Url into binary data.
    /// </summary>
    /// <param name="source">The Base64Url encoded characters to decode.</param>
    /// <returns>A byte array containing the decoded data.</returns>
    /// <example>
    /// <code>
    /// string encoded = "AQID";
    /// byte[] decoded = Base64Url.DecodeFromChars(encoded.AsSpan());
    /// // decoded is { 1, 2, 3 }
    /// </code>
    /// </example>
    public static byte[] DecodeFromChars(ReadOnlySpan<char> source)
    {
        if (source.IsEmpty)
        {
            return [];
        }

        // Convert URL-safe Base64 to standard Base64
        var base64 = source.ToString()
            .Replace('-', '+')
            .Replace('_', '/');

        // Add padding if necessary
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }

    /// <summary>
    /// Validates that the specified span of text is comprised of valid base-64 URL encoded data.
    /// </summary>
    /// <param name="base64UrlText">The Base64Url text to validate.</param>
    /// <returns><see langword="true"/> if <paramref name="base64UrlText"/> is valid; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// bool isValid = Base64Url.IsValid("AQID".AsSpan());
    /// // isValid is true
    /// </code>
    /// </example>
    public static bool IsValid(ReadOnlySpan<char> base64UrlText)
    {
        if (base64UrlText.IsEmpty)
        {
            return true;
        }

        // Base64Url 编码长度不可能 mod 4 == 1
        if (base64UrlText.Length % 4 == 1)
        {
            return false;
        }

        foreach (var c in base64UrlText)
        {
            if (!IsValidBase64UrlChar(c))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns the length (in chars) of the result if you were to encode binary data within a byte span of size <paramref name="bytesLength"/>.
    /// </summary>
    /// <param name="bytesLength">The number of bytes to encode.</param>
    /// <returns>The length of the encoded string.</returns>
    public static int GetEncodedLength(int bytesLength)
    {
        if (bytesLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesLength));
        }

        // Base64Url encoding: 4 chars for every 3 bytes, no padding
        // Formula: ceil(bytesLength * 4 / 3)
        return (bytesLength * 4 + 2) / 3;
    }

    /// <summary>
    /// Returns the maximum length (in bytes) of the result if you were to decode base 64 URL encoded text from a span of size <paramref name="base64Length"/>.
    /// </summary>
    /// <param name="base64Length">The length of the Base64Url encoded text.</param>
    /// <returns>The maximum length of the decoded bytes.</returns>
    public static int GetMaxDecodedLength(int base64Length)
    {
        if (base64Length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(base64Length));
        }

        // Base64 decoding: 3 bytes for every 4 chars
        return (base64Length * 3 + 3) / 4;
    }

    private static bool IsValidBase64UrlChar(char c)
    {
        return c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-' or '_';
    }
}

#endif
