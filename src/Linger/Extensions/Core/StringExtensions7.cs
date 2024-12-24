using System.Diagnostics.CodeAnalysis;
using System.Text;
using Linger.Helper;
namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    /// <summary>
    /// Converts the string to a safe string, returning a default value if the string is null.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null.</param>
    /// <returns>The original string if not null, otherwise the default value.</returns>
    public static string ToSafeString(this string? value, string defaultValue = "")
        => value.IsNull() ? defaultValue : value;

    /// <summary>
    /// Converts the string to a string or null, returning a default value if the string is null.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null.</param>
    /// <returns>The original string if not null, otherwise the default value.</returns>
    public static string? ToStringOrNull(this string? value, string? defaultValue = null)
        => value.ToStringOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to a string or null, using a function to provide the default value if the string is null.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultFunc">The function to provide the default value if the string is null.</param>
    /// <returns>The original string if not null, otherwise the result of the default function.</returns>
    public static string? ToStringOrNull(this string? value, Func<string?> defaultFunc) //=> value ?? defaultFunc?.Invoke();
    {
        GuardExtensions.EnsureIsNotNull(defaultFunc, nameof(defaultFunc));
        return value ?? defaultFunc.Invoke();
    }

    #region char

    /// <summary>
    /// Tries to convert the string to a char.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting char if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToChar(this string? value, [NotNullWhen(true)] out char? result)
    {
        if (value.IsNullOrWhiteSpace() || !char.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a char or null, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting char if the conversion is successful, otherwise the default value.</returns>
    public static char? ToCharOrNull(this string? value, char? defaultValue = null)
        => value.ToCharOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to a char or null, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <returns>The resulting char if the conversion is successful, otherwise the result of the default function.</returns>
    public static char? ToCharOrNull(this string? value, Func<char?> defaultValueFunc)
        => value.TryToChar(out var result) ? result.Value : defaultValueFunc.Invoke();

    /// <summary>
    /// Converts the string to a char, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting char if the conversion is successful, otherwise the default value.</returns>
    public static char ToChar(this string? value, char defaultValue = '\0')
        => value.ToChar(() => defaultValue);

    /// <summary>
    /// Converts the string to a char, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <returns>The resulting char if the conversion is successful, otherwise the result of the default function.</returns>
    public static char ToChar(this string? value, Func<char>? defaultValueFunc)
    {
        if (value.TryToChar(out var result))
        {
            return result.Value;
        }
        else
        {
            return defaultValueFunc != null ? defaultValueFunc.Invoke() : '\0';
        }
    }

    #endregion

    #region sbyte

    /// <summary>
    /// Tries to convert the string to an sbyte.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting sbyte if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToSByte(this string? value, [NotNullWhen(true)] out sbyte? result)
    {
        if (value.IsNullOrWhiteSpace() || !sbyte.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to an sbyte or null, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting sbyte if the conversion is successful, otherwise the default value.</returns>
    public static sbyte? ToSByteOrNull(this string? value, sbyte? defaultValue = null)
        => value.ToSByteOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to an sbyte or null, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <returns>The resulting sbyte if the conversion is successful, otherwise the result of the default function.</returns>
    public static sbyte? ToSByteOrNull(this string? value, Func<sbyte?>? defaultValueFunc)
        => value.TryToSByte(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to an sbyte, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting sbyte if the conversion is successful, otherwise the default value.</returns>
    public static sbyte ToSByte(this string? value, sbyte defaultValue = 0)
        => value.ToSByte(() => defaultValue);

    /// <summary>
    /// Converts the string to an sbyte, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <returns>The resulting sbyte if the conversion is successful, otherwise the result of the default function.</returns>
    public static sbyte ToSByte(this string? value, Func<sbyte>? defaultValueFunc)
        => value.TryToSByte(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? 0;

    #endregion

    #region byte

    /// <summary>
    /// Tries to convert the string to a byte.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting byte if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToByte(this string? value, [NotNullWhen(true)] out byte? result)
    {
        if (value.IsNullOrWhiteSpace() || !byte.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a byte or null, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting byte if the conversion is successful, otherwise the default value.</returns>
    public static byte? ToByteOrNull(this string? value, byte? defaultValue = null)
        => value.ToByteOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to a byte or null, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <returns>The resulting byte if the conversion is successful, otherwise the result of the default function.</returns>
    public static byte? ToByteOrNull(this string? value, Func<byte?>? defaultValueFunc)
        => value.TryToByte(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to a byte, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting byte if the conversion is successful, otherwise the default value.</returns>
    public static byte ToByte(this string? value, byte defaultValue = 0)
        => value.ToByte(() => defaultValue);

    /// <summary>
    /// Converts the string to a byte, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <returns>The resulting byte if the conversion is successful, otherwise the result of the default function.</returns>
    public static byte ToByte(this string? value, Func<byte>? defaultValueFunc)
        => value.TryToByte(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? 0;

    #endregion

    #region ushort

    /// <summary>
    /// Tries to convert the string to a ushort.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting ushort if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToUShort(this string? value, [NotNullWhen(true)] out ushort? result)
    {
        if (value.IsNullOrWhiteSpace() || !ushort.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a ushort or null, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting ushort if the conversion is successful, otherwise the default value.</returns>
    public static ushort? ToUShortOrNull(this string? value, ushort? defaultValue = null)
        => value.ToUShortOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to a ushort or null, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <returns>The resulting ushort if the conversion is successful, otherwise the result of the default function.</returns>
    public static ushort? ToUShortOrNull(this string? value, Func<ushort?>? defaultValueFunc)
        => value.TryToUShort(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to a ushort, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting ushort if the conversion is successful, otherwise the default value.</returns>
    public static ushort ToUShort(this string? value, ushort defaultValue = 0)
        => value.ToUShort(() => defaultValue);

    /// <summary>
    /// Converts the string to a ushort, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <returns>The resulting ushort if the conversion is successful, otherwise the result of the default function.</returns>
    public static ushort ToUShort(this string? value, Func<ushort>? defaultValueFunc)
        => value.TryToUShort(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? 0;

    #endregion

    #region short

    /// <summary>
    /// Tries to convert the string to a short.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting short if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToShort(this string? value, [NotNullWhen(true)] out short? result)
    {
        if (value.IsNullOrWhiteSpace() || !short.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a short or null, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting short if the conversion is successful, otherwise the default value.</returns>
    public static short? ToShortOrNull(this string? value, short? defaultValue = null)
        => value.ToShortOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to a short or null, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <returns>The resulting short if the conversion is successful, otherwise the result of the default function.</returns>
    public static short? ToShortOrNull(this string? value, Func<short?>? defaultValueFunc)
        => value.TryToShort(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to a short, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting short if the conversion is successful, otherwise the default value.</returns>
    public static short ToShort(this string? value, short defaultValue = 0)
        => value.ToShort(() => defaultValue);

    /// <summary>
    /// Converts the string to a short, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <returns>The resulting short if the conversion is successful, otherwise the result of the default function.</returns>
    public static short ToShort(this string? value, Func<short>? defaultValueFunc)
        => value.TryToShort(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? 0;

    #endregion

    #region bytes

    /// <summary>
    /// Tries to convert the string to a byte array using UTF-8 encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting byte array if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToBytes(this string? value, [NotNullWhen(true)] out byte[]? result)
        => value.TryToBytes(Encoding.UTF8, out result);

    /// <summary>
    /// Tries to convert the string to a byte array using the specified encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <param name="result">The resulting byte array if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToBytes(this string? value, Encoding encoding, [NotNullWhen(true)] out byte[]? result)
    {
        if (value.IsNullOrWhiteSpace())
        {
            result = null;
            return false;
        }

        result = encoding.GetBytes(value);
        return true;
    }

    /// <summary>
    /// Converts the string to a byte array or null, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting byte array if the conversion is successful, otherwise the default value.</returns>
    public static byte[]? ToBytesOrNull(this string? value, byte[]? defaultValue)
        => value.ToBytesOrNull(defaultValue, Encoding.UTF8);

    /// <summary>
    /// Converts the string to a byte array or null, using the specified encoding and returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <returns>The resulting byte array if the conversion is successful, otherwise the default value.</returns>
    public static byte[]? ToBytesOrNull(this string? value, byte[]? defaultValue, Encoding encoding)
        => value.ToBytesOrNull(() => defaultValue, encoding);

    /// <summary>
    /// Converts the string to a byte array or null, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <returns>The resulting byte array if the conversion is successful, otherwise the result of the default function.</returns>
    public static byte[]? ToBytesOrNull(this string? value, Func<byte[]?>? defaultValueFunc, Encoding encoding)
        => value.TryToBytes(encoding, out var result) ? result : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to a byte array using UTF-8 encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The resulting byte array.</returns>
    public static byte[] ToBytes(this string? value)
        => value.ToBytes(Encoding.UTF8);

    /// <summary>
    /// Converts the string to a byte array using the specified encoding.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <returns>The resulting byte array.</returns>
    public static byte[] ToBytes(this string? value, Encoding encoding)
        => value.ToBytes(() => [], encoding);

    /// <summary>
    /// Converts the string to a byte array using the specified encoding, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <returns>The resulting byte array if the conversion is successful, otherwise the default value.</returns>
    public static byte[] ToBytes(this string? value, byte[] defaultValue, Encoding encoding)
        => value.ToBytes(() => defaultValue, encoding);

    /// <summary>
    /// Converts the string to a byte array using the specified encoding, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <param name="encoding">The encoding to use for the conversion.</param>
    /// <returns>The resulting byte array if the conversion is successful, otherwise the result of the default function.</returns>
    public static byte[] ToBytes(this string? value, Func<byte[]>? defaultValueFunc, Encoding encoding)
        => value.TryToBytes(encoding, out var result) ? result : defaultValueFunc?.Invoke() ?? [];

    #endregion

    #region Guid

    /// <summary>
    /// Tries to convert the string to a Guid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting Guid if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToGuid(this string? value, [NotNullWhen(true)] out Guid? result)
    {
        if (value.IsNullOrWhiteSpace() || !Guid.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a Guid or null, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting Guid if the conversion is successful, otherwise the default value.</returns>
    public static Guid? ToGuidOrNull(this string? value, Guid? defaultValue = null)
        => value.ToGuidOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to a Guid or null, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <returns>The resulting Guid if the conversion is successful, otherwise the result of the default function.</returns>
    public static Guid? ToGuidOrNull(this string? value, Func<Guid?>? defaultValueFunc)
        => value.TryToGuid(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to a Guid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The resulting Guid.</returns>
    public static Guid ToGuid(this string? value)
        => value.ToGuid(() => Guid.Empty);

    /// <summary>
    /// Converts the string to a Guid, returning a default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the string is null or invalid.</param>
    /// <returns>The resulting Guid if the conversion is successful, otherwise the default value.</returns>
    public static Guid ToGuid(this string? value, Guid defaultValue)
        => value.ToGuid(() => defaultValue);

    /// <summary>
    /// Converts the string to a Guid, using a function to provide the default value if the string is null or invalid.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the string is null or invalid.</param>
    /// <returns>The resulting Guid if the conversion is successful, otherwise the result of the default function.</returns>
    public static Guid ToGuid(this string? value, Func<Guid>? defaultValueFunc)
        => value.TryToGuid(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? Guid.Empty;

    #endregion

    #region Stream

    /// <summary>
    /// Converts the string to a Stream.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The resulting Stream.</returns>
    public static Stream ToStream(this string value)
        => value.ToStreamOrNull() ?? Stream.Null;

    /// <summary>
    /// Converts the string to a Stream or null.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The resulting Stream if the conversion is successful, otherwise null.</returns>
    public static Stream? ToStreamOrNull(this string? value)
    {
        if (value.IsNullOrWhiteSpace())
            return null;

        var bytes = value.ToBytes();
        return new MemoryStream(bytes);
    }

    #endregion
}
