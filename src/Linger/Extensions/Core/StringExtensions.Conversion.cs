using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Linger.Helper;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    #region string? -> string

    public static string ToStringOrDefault(this string? value, string defaultValue = "")
    {
        return value ?? defaultValue;
    }

    public static string ToStringOrDefault(this string? value, Func<string> defaultValueFunc)
    {
        ArgumentNullException.ThrowIfNull(defaultValueFunc);
        return value ?? defaultValueFunc();
    }

    public static string ToSafeString(this string? value, string defaultValue = "")
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value!;
    }

    #endregion

    #region string? -> short

    public static bool TryToShort(this string? value, out short result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var trimmed = value!.Trim();
        if (short.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            return true;

        return decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec) &&
               dec.TryToShort(out result);
    }

    public static short ToShort(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.TryToShort(out var result)) return result;

        const NumberStyles strictStyles = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite |
                                          NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint |
                                          NumberStyles.AllowThousands;
        return decimal.Parse(value, strictStyles, CultureInfo.InvariantCulture).ToShort();
    }

    public static short? ToShortOrNull(this string? value) => value.TryToShort(out var r) ? r : null;
    public static short ToShortOrDefault(this string? value, short defaultValue = 0) => value.TryToShort(out var r) ? r : defaultValue;

    #endregion

    #region string? -> Guid

    public static bool TryToGuid(this string? value, out Guid result)
    {
        result = Guid.Empty;
        if (string.IsNullOrWhiteSpace(value)) return false;
        return Guid.TryParse(value!.Trim(), out result);
    }

    public static Guid ToGuid(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.TryToGuid(out var result)) return result;

        return Guid.Parse(value);
    }

    public static Guid? ToGuidOrNull(this string? value) => value.TryToGuid(out var r) ? r : null;
    public static Guid ToGuidOrDefault(this string? value, Guid defaultValue = default) => value.TryToGuid(out var r) ? r : defaultValue;

    #endregion

    #region string? -> int

    public static bool TryToInt(this string? value, out int result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var trimmed = value!.Trim();
        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            return true;

        return decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec) &&
               dec.TryToInt(out result);
    }

    public static int ToInt(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.TryToInt(out var result)) return result;

        const NumberStyles strictStyles = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite |
                                          NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint |
                                          NumberStyles.AllowThousands;
        return decimal.Parse(value, strictStyles, CultureInfo.InvariantCulture).ToInt();
    }

    public static int? ToIntOrNull(this string? value) => value.TryToInt(out var r) ? r : null;
    public static int ToIntOrDefault(this string? value, int defaultValue = 0) => value.TryToInt(out var r) ? r : defaultValue;

    #endregion

    #region string? -> long

    public static bool TryToLong(this string? value, out long result)
    {
        result = 0L;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var trimmed = value!.Trim();
        if (long.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            return true;

        return decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec) &&
               dec.TryToLong(out result);
    }

    public static long ToLong(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.TryToLong(out var result)) return result;

        const NumberStyles strictStyles = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite |
                                          NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint |
                                          NumberStyles.AllowThousands;
        return decimal.Parse(value, strictStyles, CultureInfo.InvariantCulture).ToLong();
    }

    public static long? ToLongOrNull(this string? value) => value.TryToLong(out var r) ? r : null;
    public static long ToLongOrDefault(this string? value, long defaultValue = 0L) => value.TryToLong(out var r) ? r : defaultValue;

    #endregion

    #region string? -> decimal

    public static bool TryToDecimal(this string? value, out decimal result)
    {
        result = 0m;
        if (string.IsNullOrWhiteSpace(value)) return false;

        const NumberStyles permissiveStyles = NumberStyles.Number | NumberStyles.AllowExponent;
        return decimal.TryParse(value!.Trim(), permissiveStyles, CultureInfo.InvariantCulture, out result);
    }

    public static decimal ToDecimal(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.TryToDecimal(out var result)) return result;

        const NumberStyles strictStyles = NumberStyles.Number | NumberStyles.AllowExponent;
        return decimal.Parse(value, strictStyles, CultureInfo.InvariantCulture);
    }

    public static decimal? ToDecimalOrNull(this string? value) => value.TryToDecimal(out var r) ? r : null;
    public static decimal ToDecimalOrDefault(this string? value, decimal defaultValue = default) => value.TryToDecimal(out var r) ? r : defaultValue;

    #endregion

    #region string? -> DateTime

    public static bool TryToDateTime(this string? value, out DateTime result)
    {
        return DateTimeConversionHelper.TryConvertStringToDateTime(value, out result);
    }

    public static DateTime ToDateTime(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return DateTimeConversionHelper.ParseStringToDateTime(value);
    }

    public static DateTime? ToDateTimeOrNull(this string? value)
        => value.TryToDateTime(out var r) ? r : null;

    public static DateTime ToDateTimeOrDefault(this string? value, DateTime defaultValue = default)
        => value.TryToDateTime(out var r) ? r : defaultValue;

    #endregion

    #region string? -> bool

    public static bool TryToBool(this string? value, out bool result)
    {
        return BoolConversionHelper.TryConvertStringToBool(value, out result);
    }

    public static bool ToBool(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var nonNullValue = value;
        if (nonNullValue.TryToBool(out var result)) return result;

        return bool.Parse(nonNullValue);
    }

    public static bool? ToBoolOrNull(this string? value)
        => value.TryToBool(out var r) ? r : null;

    public static bool ToBoolOrDefault(this string? value, bool defaultValue = default)
        => value.TryToBool(out var r) ? r : defaultValue;

    public static bool ToBoolOrDefault(this string? value, Func<bool>? defaultValueFunc)
        => value.TryToBool(out var r) ? r : (defaultValueFunc != null ? defaultValueFunc() : default);

    #endregion

    #region string? -> byte[]

    public static bool TryToBytes(this string? value, [NotNullWhen(true)] out byte[]? result)
        => value.TryToBytes(Encoding.UTF8, out result);

    public static bool TryToBytes(this string? value, Encoding encoding, [NotNullWhen(true)] out byte[]? result)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        result = null;
        if (string.IsNullOrEmpty(value))
            return false;
        result = encoding.GetBytes(value);
        return true;
    }

    public static byte[] ToBytes(this string? value, Encoding? encoding = null)
    {
        if (value.TryToBytes(encoding ?? Encoding.UTF8, out var result))
            return result;

        throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-empty string input.");
    }

    public static byte[]? ToBytesOrNull(this string? value, Encoding? encoding = null)
        => value.TryToBytes(encoding ?? Encoding.UTF8, out var result) ? result : null;

    public static byte[] ToBytesOrDefault(this string? value, byte[] defaultValue, Encoding? encoding = null)
        => value.TryToBytes(encoding ?? Encoding.UTF8, out var result) ? result : defaultValue;

    public static byte[] ToBytesOrDefault(this string? value, Func<byte[]> defaultValueFunc, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(defaultValueFunc);
        return value.TryToBytes(encoding ?? Encoding.UTF8, out var result) ? result : defaultValueFunc();
    }

    #endregion

    #region Stream

    public static Stream ToStream(this string? value, Encoding? encoding = null)
        => value.ToStreamOrNull(encoding) ?? Stream.Null;

    public static Stream? ToStreamOrNull(this string? value, Encoding? encoding = null)
    {
        if (string.IsNullOrEmpty(value)) return null;

        var bytes = value.ToBytes(encoding ?? Encoding.UTF8);
        return new MemoryStream(bytes, writable: false);
    }

    #endregion
}
