using System.Diagnostics.CodeAnalysis;
using System.Text;
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
    public static string? ToStringOrNull(this string? value, Func<string?> defaultFunc)
    {
        ArgumentNullException.ThrowIfNull(defaultFunc);
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

        return defaultValueFunc?.Invoke() ?? '\0';
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

    #region int

    /// <summary>
    /// Tries to convert the string to an integer.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting integer if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToInt(this string? value, [NotNullWhen(true)] out int? result)
    {
        if (value.IsNullOrWhiteSpace() || !int.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to an integer or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted integer or the default value.</returns>
    public static int? ToIntOrNull(this string? value, int? defaultValue = null)
        => value.ToIntOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to an integer or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted integer or the result of the default value function.</returns>
    public static int? ToIntOrNull(this string? value, Func<int?>? defaultValueFunc)
        => value.TryToInt(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to an integer or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted integer or the default value.</returns>
    public static int ToInt(this string? value, int defaultValue = 0)
        => value.ToInt(() => defaultValue);

    /// <summary>
    /// Converts the string to an integer or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted integer or the result of the default value function.</returns>
    public static int ToInt(this string? value, Func<int>? defaultValueFunc)
        => value.TryToInt(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? 0;

    #endregion

    #region long

    /// <summary>
    /// Tries to convert the string to a long integer.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting long integer if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToLong(this string? value, [NotNullWhen(true)] out long? result)
    {
        if (value.IsNullOrWhiteSpace() || !long.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a long integer or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted long integer or the default value.</returns>
    public static long? ToLongOrNull(this string? value, long? defaultValue = null)
        => value.ToLongOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to a long integer or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted long integer or the result of the default value function.</returns>
    public static long? ToLongOrNull(this string? value, Func<long?>? defaultValueFunc)
        => value.TryToLong(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to a long integer or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted long integer or the default value.</returns>
    public static long ToLong(this string? value, long defaultValue = 0)
        => value.ToLong(() => defaultValue);

    /// <summary>
    /// Converts the string to a long integer or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted long integer or the result of the default value function.</returns>
    public static long ToLong(this string? value, Func<long>? defaultValueFunc)
        => value.TryToLong(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? 0;

    #endregion

    #region decimal

    /// <summary>
    /// Tries to convert the string to a decimal.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting decimal if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToDecimal(this string? value, [NotNullWhen(true)] out decimal? result)
    {
        if (value.IsNullOrWhiteSpace() || !decimal.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a decimal or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted decimal or the default value.</returns>
    public static decimal? ToDecimalOrNull(this string? value, decimal? defaultValue = null, int? digits = null)
        => value.ToDecimalOrNull(() => defaultValue, digits);

    /// <summary>
    /// Converts the string to a decimal or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted decimal or the result of the default value function.</returns>
    public static decimal? ToDecimalOrNull(this string? value, Func<decimal?>? defaultValueFunc, int? digits = null)
    {
        if (value.TryToDecimal(out var result))
        {
            if (digits == null)
            {
                return result.Value;
            }
            return Math.Round(result.Value, digits.Value);
        }

        return defaultValueFunc?.Invoke();
    }

    /// <summary>
    /// Converts the string to a decimal or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted decimal or the default value.</returns>
    public static decimal ToDecimal(this string? value, decimal defaultValue = 0, int? digits = null)
        => value.ToDecimal(() => defaultValue, digits);

    /// <summary>
    /// Converts the string to a decimal or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted decimal or the result of the default value function.</returns>
    public static decimal ToDecimal(this string? value, Func<decimal>? defaultValueFunc, int? digits = null)
    {
        if (value.TryToDecimal(out var result))
        {
            if (digits == null)
            {
                return result.Value;
            }
            return Math.Round(result.Value, digits.Value);
        }

        return defaultValueFunc?.Invoke() ?? 0;
    }

    #endregion

    #region float

    /// <summary>
    /// Tries to convert the string to a float.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting float if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToFloat(this string? value, [NotNullWhen(true)] out float? result)
    {
        if (value.IsNullOrWhiteSpace() || !float.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a float or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted float or the default value.</returns>
    public static float? ToFloatOrNull(this string? value, float? defaultValue = null, int? digits = null)
        => value.ToFloatOrNull(() => defaultValue, digits);

    /// <summary>
    /// Converts the string to a float or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted float or the result of the default value function.</returns>
    public static float? ToFloatOrNull(this string? value, Func<float?>? defaultValueFunc, int? digits = null)
    {
        if (value.TryToFloat(out var result))
        {
            if (digits == null)
            {
                return result.Value;
            }
            return (float)Math.Round(result.Value, digits.Value);
        }

        return defaultValueFunc?.Invoke();
    }

    /// <summary>
    /// Converts the string to a float or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted float or the default value.</returns>
    public static float ToFloat(this string? value, float defaultValue = 0, int? digits = null)
        => value.ToFloat(() => defaultValue, digits);

    /// <summary>
    /// Converts the string to a float or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted float or the result of the default value function.</returns>
    public static float ToFloat(this string? value, Func<float>? defaultValueFunc, int? digits = null)
    {
        if (value.TryToFloat(out var result))
        {
            if (digits == null)
            {
                return result.Value;
            }
            return (float)Math.Round(result.Value, digits.Value);
        }

        return defaultValueFunc?.Invoke() ?? 0;
    }

    #endregion

    #region double

    /// <summary>
    /// Tries to convert the string to a double.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting double if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToDouble(this string? value,
        [NotNullWhen(true)]
        out double? result)
    {
        if (value.IsNullOrWhiteSpace() || !double.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a double or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted double or the default value.</returns>
    public static double? ToDoubleOrNull(this string? value, double? defaultValue = null, int? digits = null)
        => value.ToDoubleOrNull(() => defaultValue, digits);

    /// <summary>
    /// Converts the string to a double or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted double or the result of the default value function.</returns>
    public static double? ToDoubleOrNull(this string? value, Func<double?>? defaultValueFunc, int? digits = null)
    {
        if (value.TryToDouble(out var result))
        {
            if (digits == null)
            {
                return result.Value;
            }
            return Math.Round(result.Value, digits.Value);
        }

        return defaultValueFunc?.Invoke();
    }

    /// <summary>
    /// Converts the string to a double or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted double or the default value.</returns>
    public static double ToDouble(this string? value, double defaultValue, int? digits = null)
        => value.ToDouble(() => defaultValue, digits);

    /// <summary>
    /// Converts the string to a double or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted double or the result of the default value function.</returns>
    public static double ToDouble(this string? value, Func<double>? defaultValueFunc, int? digits = null)
    {
        if (value.TryToDouble(out var result))
        {
            if (digits == null)
            {
                return result.Value;
            }
            return Math.Round(result.Value, digits.Value);
        }

        return defaultValueFunc?.Invoke() ?? 0;
    }

    #endregion

    #region datetime

    /// <summary>
    /// Tries to convert the string to a DateTime.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting DateTime if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToDateTime(this string? value, [NotNullWhen(true)] out DateTime? result)
    {
        if (value.IsNullOrWhiteSpace() || !DateTime.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a DateTime or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted DateTime or the default value.</returns>
    public static DateTime? ToDateTimeOrNull(this string? value, DateTime? defaultValue = null)
        => value.ToDateTimeOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to a DateTime or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted DateTime or the result of the default value function.</returns>
    public static DateTime? ToDateTimeOrNull(this string? value, Func<DateTime?>? defaultValueFunc)
        => value.TryToDateTime(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to a DateTime or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The converted DateTime or the default value.</returns>
    public static DateTime ToDateTime(this string? value)
        => value.ToDateTime(() => new DateTime());

    /// <summary>
    /// Converts the string to a DateTime or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted DateTime or the default value.</returns>
    public static DateTime ToDateTime(this string? value, DateTime defaultValue)
        => value.ToDateTime(() => defaultValue);

    /// <summary>
    /// Converts the string to a DateTime or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted DateTime or the result of the default value function.</returns>
    public static DateTime ToDateTime(this string? value, Func<DateTime>? defaultValueFunc)
        => value.TryToDateTime(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? new DateTime();

    ///// <summary>
    ///// Converts the string to a DateTime
    ///// </summary>
    ///// <param name="DateString">The date string.</param>
    ///// <param name="DateFormat">The date format.</param>
    ///// <returns></returns>
    //public static DateTime ToDateTime(this string DateString, string DateFormat)
    //{
    //    return DateTime.ParseExact(DateString, DateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.NoCurrentDateDefault);
    //}

    ///// <summary>
    ///// Converts the string to a DateTime
    ///// </summary>
    ///// <param name="DateString">The date string.</param>
    ///// <param name="DateFormats">Array of date formats.</param>
    ///// <returns></returns>
    //public static DateTime ToDateTime(this string DateString, string[] DateFormats)
    //{
    //    return DateTime.ParseExact(DateString, DateFormats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.NoCurrentDateDefault);
    //}

    #endregion

    #region bool

    private static readonly Dictionary<string, bool> s_boolMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "0", false },
        { "false", false },
        { "no", false },
        { "fail", false },
        { "lose", false },
        { "true", true },
        { "1", true },
        { "ok", true },
        { "yes", true },
        { "success", true }
    };

    /// <summary>
    /// Tries to convert the string to a boolean.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting boolean if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToBool(this string? value, [NotNullWhen(true)] out bool? result)
    {
        if (!value.IsNullOrWhiteSpace() &&
            (s_boolMap.TryGetValue(value, out var valResult) || bool.TryParse(value, out valResult)))
        {
            result = valResult;
            return true;
        }
        result = null;
        return false;
    }

    /// <summary>
    /// Converts the string to a boolean or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted boolean or the default value.</returns>
    public static bool? ToBoolOrNull(this string? value, bool? defaultValue = null)
        => value.ToBoolOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to a boolean or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted boolean or the result of the default value function.</returns>
    public static bool? ToBoolOrNull(this string? value, Func<bool?>? defaultValueFunc)
        => value.TryToBool(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to a boolean or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted boolean or the default value.</returns>
    public static bool ToBool(this string? value, bool defaultValue = false)
        => value.ToBool(() => defaultValue);

    /// <summary>
    /// Converts the string to a boolean or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted boolean or the result of the default value function.</returns>
    public static bool ToBool(this string? value, Func<bool>? defaultValueFunc)
        => value.TryToBool(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? false;

    #endregion
}
