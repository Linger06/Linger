using System.Runtime.CompilerServices;
using Linger.Helper;

namespace Linger.Extensions.Core;

public static class ObjectExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull([NotNullWhen(false)] this object? value) => value is null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNull([NotNullWhen(true)] this object? value) => value is not null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrDbNull([NotNullWhen(false)] this object? value) => value is DBNull or null;

    public static bool IsNumeric(this object? value) =>
        value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;

    /// <summary>
    /// Determines whether the specified value equals any value in the supplied collection.
    /// </summary>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    /// <param name="obj">The value to compare.</param>
    /// <param name="values">The values to compare against.</param>
    /// <returns><see langword="true"/> when <paramref name="obj"/> equals any supplied value; otherwise, <see langword="false"/>.</returns>
    public static bool In<T>(this T obj, params T[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return Array.IndexOf(values, obj) >= 0;
    }

    /// <summary>
    /// Determines whether the specified value equals none of the supplied values.
    /// </summary>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    /// <param name="obj">The value to compare.</param>
    /// <param name="values">The values to compare against.</param>
    /// <returns><see langword="true"/> when <paramref name="obj"/> does not equal any supplied value; otherwise, <see langword="false"/>.</returns>
    public static bool NotIn<T>(this T obj, params T[] values)
    {
        return !obj.In(values);
    }

    public static string? ToNormalizedString(this object? input, bool trim = false, bool treatEmptyAsNull = false)
    {
        if (input is null) return null;

        var result = input.ToString();
        if (trim)
        {
            result = result?.Trim();
        }

        if (treatEmptyAsNull && string.IsNullOrWhiteSpace(result))
        {
            return null;
        }

        return result;
    }

    public static string ToTrimmedString(this object? input)
    {
        return input.ToNormalizedString(trim: true) ?? string.Empty;
    }

    public static string ToStringOrDefault(this object? input, string defaultValue = "")
    {
        return input?.ToString() ?? defaultValue;
    }

    public static bool TryToShort(this object? value, out short result)
    {
        result = 0;
        switch (value)
        {
            case null or DBNull:
                return false;
            case short exact:
                result = exact;
                return true;
            case string str:
                return str.TryToShort(out result);
            case decimal dec:
                return dec.TryToShort(out result);
            case double db when IsWholeNumberInRange(db, short.MinValue):
                result = (short)db;
                return true;
            case float fl when IsWholeNumberInRange(fl, short.MinValue):
                result = (short)fl;
                return true;
            case double or float:
                return false;
        }

        if (TryGetIntegralValue(value, out var integral))
        {
            return integral.TryToShort(out result);
        }

        return FormatInvariant(value).TryToShort(out result);
    }

    public static short ToShort(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToShort(out var result)) return result;

        return value switch
        {
            string str => str.ToShort(),
            decimal dec => dec.ToShort(),
            double db => ConvertToWholeNumberOrThrow(db, short.MinValue, nameof(Double), "Int16", static x => (short)x),
            float fl => ConvertToWholeNumberOrThrow(fl, short.MinValue, nameof(Single), "Int16", static x => (short)x),
            _ => ThrowConversionFailure<short>(value, "Int16"),
        };
    }

    public static short? ToShortOrNull(this object? value) => value.TryToShort(out var r) ? r : null;
    public static short ToShortOrDefault(this object? value, short defaultValue = 0) => value.TryToShort(out var r) ? r : defaultValue;

    public static bool TryToLong(this object? value, out long result)
    {
        result = 0L;
        switch (value)
        {
            case null or DBNull:
                return false;
            case long exact:
                result = exact;
                return true;
            case string str:
                return str.TryToLong(out result);
            case decimal dec:
                return dec.TryToLong(out result);
            case double db when IsWholeNumberInRange(db, long.MinValue):
                result = (long)db;
                return true;
            case float fl when IsWholeNumberInRange(fl, long.MinValue):
                result = (long)fl;
                return true;
            case double or float:
                return false;
        }

        if (TryGetIntegralValue(value, out var integral))
        {
            return integral.TryToLong(out result);
        }

        return FormatInvariant(value).TryToLong(out result);
    }

    public static long ToLong(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToLong(out var result)) return result;

        return value switch
        {
            string str => str.ToLong(),
            decimal dec => dec.ToLong(),
            double db => ConvertToWholeNumberOrThrow(db, long.MinValue, nameof(Double), "Int64", static x => (long)x),
            float fl => ConvertToWholeNumberOrThrow(fl, long.MinValue, nameof(Single), "Int64", static x => (long)x),
            _ => ThrowConversionFailure<long>(value, "Int64"),
        };
    }

    public static long? ToLongOrNull(this object? value) => value.TryToLong(out var r) ? r : null;
    public static long ToLongOrDefault(this object? value, long defaultValue = 0L) => value.TryToLong(out var r) ? r : defaultValue;

    public static bool TryToDecimal(this object? value, out decimal result)
    {
        result = 0m;
        switch (value)
        {
            case null or DBNull:
                return false;
            case decimal dec:
                result = dec;
                return true;
            case string str:
                return str.TryToDecimal(out result);
            case double db:
                // (double)decimal.MaxValue 会被舍入成 2^96，所以上下界都用开区间
                if (double.IsNaN(db) || double.IsInfinity(db) || db >= (double)decimal.MaxValue || db <= (double)decimal.MinValue)
                {
                    return false;
                }

                result = (decimal)db;
                return true;
            case float fl:
                if (float.IsNaN(fl) || float.IsInfinity(fl) || fl >= (float)decimal.MaxValue || fl <= (float)decimal.MinValue)
                {
                    return false;
                }

                result = (decimal)fl;
                return true;
        }

        if (TryGetIntegralValue(value, out var integral))
        {
            result = integral;
            return true;
        }

        return FormatInvariant(value).TryToDecimal(out result);
    }

    public static decimal ToDecimal(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToDecimal(out var result)) return result;

        return value switch
        {
            string str => str.ToDecimal(),
            double or float => throw new OverflowException($"The value '{FormatInvariant(value)}' is outside the range of Decimal."),
            _ => ThrowConversionFailure<decimal>(value, "Decimal"),
        };
    }

    public static decimal? ToDecimalOrNull(this object? value) => value.TryToDecimal(out var r) ? r : null;
    public static decimal ToDecimalOrDefault(this object? value, decimal defaultValue = default) => value.TryToDecimal(out var r) ? r : defaultValue;

    /// <summary>
    /// Attempts to convert the specified value to a <see cref="double"/> using the invariant culture.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">The converted value when the conversion succeeds; otherwise, zero.</param>
    /// <returns><see langword="true"/> when the value can be converted; otherwise, <see langword="false"/>.</returns>
    public static bool TryToDouble(this object? value, out double result)
    {
        result = 0d;
        switch (value)
        {
            case null or DBNull:
                return false;
            case double exact:
                result = exact;
                return true;
            case string str:
                return str.TryToDouble(out result);
            case float fl:
                result = fl;
                return true;
            case decimal dec:
                result = (double)dec;
                return true;
            case byte b:
                result = b;
                return true;
            case sbyte sb:
                result = sb;
                return true;
            case short s:
                result = s;
                return true;
            case ushort us:
                result = us;
                return true;
            case int i:
                result = i;
                return true;
            case uint ui:
                result = ui;
                return true;
            case long l:
                result = l;
                return true;
            case ulong ul:
                result = ul;
                return true;
            case Enum:
                result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return true;
        }

        return FormatInvariant(value).TryToDouble(out result);
    }

    /// <summary>
    /// Converts the specified value to a <see cref="double"/> using the invariant culture.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/> or <see cref="DBNull"/>.</exception>
    public static double ToDouble(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToDouble(out var result)) return result;

        return value switch
        {
            string str => str.ToDouble(),
            _ => ThrowConversionFailure<double>(value, "Double"),
        };
    }

    /// <summary>
    /// Converts the specified value to a nullable <see cref="double"/> using the invariant culture.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value, or <see langword="null"/> when conversion fails.</returns>
    public static double? ToDoubleOrNull(this object? value) => value.TryToDouble(out var r) ? r : null;

    /// <summary>
    /// Converts the specified value to a <see cref="double"/> using the invariant culture, or returns a default value when conversion fails.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The value returned when conversion fails.</param>
    /// <returns>The converted value or <paramref name="defaultValue"/>.</returns>
    public static double ToDoubleOrDefault(this object? value, double defaultValue = default) => value.TryToDouble(out var r) ? r : defaultValue;

    /// <summary>
    /// Attempts to convert the specified value to a <see cref="float"/> using the invariant culture.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">The converted value when the conversion succeeds; otherwise, zero.</param>
    /// <returns><see langword="true"/> when the value can be converted; otherwise, <see langword="false"/>.</returns>
    public static bool TryToFloat(this object? value, out float result)
    {
        result = 0f;
        switch (value)
        {
            case null or DBNull:
                return false;
            case float exact:
                result = exact;
                return true;
            case string str:
                return str.TryToFloat(out result);
            case double db when double.IsNaN(db) || double.IsInfinity(db):
                result = (float)db;
                return true;
            case double db when db >= -float.MaxValue && db <= float.MaxValue:
                result = (float)db;
                return true;
            case double:
                // 超出 float 范围：直接判失败，不必再格式化成字符串走下面的回退解析
                return false;
            case decimal dec:
                result = (float)dec;
                return true;
            case byte b:
                result = b;
                return true;
            case sbyte sb:
                result = sb;
                return true;
            case short s:
                result = s;
                return true;
            case ushort us:
                result = us;
                return true;
            case int i:
                result = i;
                return true;
            case uint ui:
                result = ui;
                return true;
            case long l:
                result = l;
                return true;
            case ulong ul:
                result = ul;
                return true;
            case Enum:
                result = Convert.ToSingle(value, CultureInfo.InvariantCulture);
                return true;
        }

        return FormatInvariant(value).TryToFloat(out result);
    }

    /// <summary>
    /// Converts the specified value to a <see cref="float"/> using the invariant culture.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/> or <see cref="DBNull"/>.</exception>
    public static float ToFloat(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToFloat(out var result)) return result;

        return value switch
        {
            string str => str.ToFloat(),
            double => throw new OverflowException($"The value '{FormatInvariant(value)}' is outside the range of Single."),
            _ => ThrowConversionFailure<float>(value, "Single"),
        };
    }

    /// <summary>
    /// Converts the specified value to a nullable <see cref="float"/> using the invariant culture.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value, or <see langword="null"/> when conversion fails.</returns>
    public static float? ToFloatOrNull(this object? value) => value.TryToFloat(out var r) ? r : null;

    /// <summary>
    /// Converts the specified value to a <see cref="float"/> using the invariant culture, or returns a default value when conversion fails.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The value returned when conversion fails.</param>
    /// <returns>The converted value or <paramref name="defaultValue"/>.</returns>
    public static float ToFloatOrDefault(this object? value, float defaultValue = default) => value.TryToFloat(out var r) ? r : defaultValue;

    public static bool TryToInt(this object? value, out int result)
    {
        result = 0;
        switch (value)
        {
            case null or DBNull:
                return false;
            case int exact:
                result = exact;
                return true;
            case string str:
                return str.TryToInt(out result);
            case decimal dec:
                return dec.TryToInt(out result);
            case double db when IsWholeNumberInRange(db, int.MinValue):
                result = (int)db;
                return true;
            case float fl when IsWholeNumberInRange(fl, int.MinValue):
                result = (int)fl;
                return true;
            case double or float:
                return false;
        }

        if (TryGetIntegralValue(value, out var integral))
        {
            return integral.TryToInt(out result);
        }

        return FormatInvariant(value).TryToInt(out result);
    }

    public static int ToInt(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToInt(out var result)) return result;

        return value switch
        {
            string str => str.ToInt(),
            decimal dec => dec.ToInt(),
            double db => ConvertToWholeNumberOrThrow(db, int.MinValue, nameof(Double), "Int32", static x => (int)x),
            float fl => ConvertToWholeNumberOrThrow(fl, int.MinValue, nameof(Single), "Int32", static x => (int)x),
            _ => ThrowConversionFailure<int>(value, "Int32"),
        };
    }

    /// <summary>
    /// Converts the specified value to a nullable <see cref="int"/> using the invariant culture.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>
    /// The converted value, or <see langword="null"/> when <paramref name="value"/> is <see langword="null"/>,
    /// <see cref="DBNull"/>, has an invalid format, is outside the range of <see cref="int"/>, or has a fractional part.
    /// </returns>
    /// <example>
    /// <code>
    /// object value = "42";
    /// int? result = value.ToIntOrNull();
    /// </code>
    /// </example>
    public static int? ToIntOrNull(this object? value) => value.TryToInt(out var r) ? r : null;

    /// <summary>
    /// Converts the specified value to an <see cref="int"/> using the invariant culture, or returns a default value when conversion fails.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="defaultValue">The value returned when conversion fails.</param>
    /// <returns>
    /// The converted value, or <paramref name="defaultValue"/> when <paramref name="value"/> is <see langword="null"/>,
    /// <see cref="DBNull"/>, has an invalid format, is outside the range of <see cref="int"/>, or has a fractional part.
    /// </returns>
    /// <example>
    /// <code>
    /// object value = "not a number";
    /// int result = value.ToIntOrDefault(-1);
    /// </code>
    /// </example>
    public static int ToIntOrDefault(this object? value, int defaultValue = 0) => value.TryToInt(out var r) ? r : defaultValue;

    public static bool TryToDateTime(this object? value, out DateTime result)
    {
        result = default;
        if (value is null || value is DBNull) return false;
        if (value is DateTime dt)
        {
            result = dt;
            return true;
        }

        switch (value)
        {
            case DateTimeOffset dto:
                result = dto.UtcDateTime;
                return true;
            case string str:
                return str.TryToDateTime(out result);
#if NET6_0_OR_GREATER
            case DateOnly dOnly:
                result = dOnly.ToDateTime(TimeOnly.MinValue);
                return true;
#endif
        }

        var fallbackStr = value.ToString();
        return fallbackStr is not null && fallbackStr.TryToDateTime(out result);
    }

    public static DateTime ToDateTime(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToDateTime(out var result)) return result;

        // 字符串走严格解析，抛出带原文的 FormatException；其他类型交给 Convert 判断是否支持
        return value is string str
            ? str.ToDateTime()
            : Convert.ToDateTime(value, CultureInfo.InvariantCulture);
    }

    public static DateTime? ToDateTimeOrNull(this object? value) => value.TryToDateTime(out var r) ? r : null;
    public static DateTime ToDateTimeOrDefault(this object? value, DateTime defaultValue = default) => value.TryToDateTime(out var r) ? r : defaultValue;

    public static bool TryToBool(this object? value, out bool result)
    {
        result = false;
        switch (value)
        {
            case null or DBNull:
                return false;
            case bool flag:
                result = flag;
                return true;
            case string str:
                return BoolConversionHelper.TryConvertStringToBool(str, out result);
            case decimal dec:
                return TryMapZeroOrOne(dec, out result);
            case double db:
                if (db == 1.0) { result = true; return true; }
                if (db == 0.0) { result = false; return true; }
                return false;
            case float fl:
                if (fl == 1f) { result = true; return true; }
                if (fl == 0f) { result = false; return true; }
                return false;
        }

        // 枚举保持按名称解析（如 Yes/No），其余整数类型统一只接受 0 和 1
        if (value is not Enum && TryGetIntegralValue(value, out var integral))
        {
            return TryMapZeroOrOne(integral, out result);
        }

        return BoolConversionHelper.TryConvertStringToBool(value.ToString(), out result);
    }

    public static bool ToBool(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToBool(out var result)) return result;

        if (value is string str)
        {
            return str.ToBool();
        }

        if (value is decimal or double or float || (value is not Enum && TryGetIntegralValue(value, out _)))
        {
            throw new InvalidCastException($"The {value.GetType().Name} value '{FormatInvariant(value)}' is invalid for Boolean conversion. Only 1 and 0 are allowed.");
        }

        throw new InvalidCastException($"Cannot convert a value of type '{value.GetType().FullName}' to Boolean.");
    }

    public static bool? ToBoolOrNull(this object? value) => value.TryToBool(out var r) ? r : null;
    public static bool ToBoolOrDefault(this object? value, bool defaultValue = default) => value.TryToBool(out var r) ? r : defaultValue;

    public static bool TryToGuid(this object? value, out Guid result)
    {
        result = Guid.Empty;
        if (value is null || value is DBNull) return false;
        if (value is Guid g)
        {
            result = g;
            return true;
        }

        switch (value)
        {
            case string str:
                return str.TryToGuid(out result);
            case byte[] bytes when bytes.Length == 16:
                result = new Guid(bytes);
                return true;
        }

        var fallbackStr = value.ToString();
        return fallbackStr is not null && fallbackStr.TryToGuid(out result);
    }

    public static Guid ToGuid(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToGuid(out var result)) return result;

        return value switch
        {
            string str => str.ToGuid(),
            byte[] bytes => throw new FormatException($"Byte array length must be exactly 16 bytes to convert to Guid. Actual length: {bytes.Length}"),
            _ => throw new InvalidCastException($"Cannot convert a value of type '{value.GetType().FullName}' to Guid."),
        };
    }

    public static Guid? ToGuidOrNull(this object? value) => value.TryToGuid(out var r) ? r : null;
    public static Guid ToGuidOrDefault(this object? value, Guid defaultValue = default) => value.TryToGuid(out var r) ? r : defaultValue;

    private static TResult ConvertToWholeNumberOrThrow<TResult>(
        double value,
        double minValue,
        string sourceTypeName,
        string targetTypeName,
        Func<double, TResult> converter)
    {
        // NaN 和无穷大不是"带小数"而是不可表示，与 Convert.ToInt32(double.NaN) 一致按溢出处理
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new OverflowException($"The {sourceTypeName.ToLowerInvariant()} value '{FormatInvariant(value)}' is outside the range of {targetTypeName}.");
        if (value % 1 != 0)
            throw new InvalidCastException($"The {sourceTypeName.ToLowerInvariant()} value '{FormatInvariant(value)}' cannot be converted to {targetTypeName} because it contains a fractional part.");
        if (value < minValue || value >= -minValue)
            throw new OverflowException($"The {sourceTypeName.ToLowerInvariant()} value '{FormatInvariant(value)}' is outside the range of {targetTypeName}.");
        return converter(value);
    }

    private static TResult ConvertToWholeNumberOrThrow<TResult>(
        float value,
        float minValue,
        string sourceTypeName,
        string targetTypeName,
        Func<float, TResult> converter)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            throw new OverflowException($"The {sourceTypeName.ToLowerInvariant()} value '{FormatInvariant(value)}' is outside the range of {targetTypeName}.");
        if (value % 1 != 0)
            throw new InvalidCastException($"The {sourceTypeName.ToLowerInvariant()} value '{FormatInvariant(value)}' cannot be converted to {targetTypeName} because it contains a fractional part.");
        if (value < minValue || value >= -minValue)
            throw new OverflowException($"The {sourceTypeName.ToLowerInvariant()} value '{FormatInvariant(value)}' is outside the range of {targetTypeName}.");
        return converter(value);
    }

    // 上界取 -minValue 的开区间：(double)int.MaxValue 和 (double)long.MaxValue 会被舍入成 2^31、2^63，用闭区间会把越界值放进来
    private static bool IsWholeNumberInRange(double value, double minValue)
        => !double.IsNaN(value) && !double.IsInfinity(value) && value % 1 == 0 && value >= minValue && value < -minValue;

    private static bool IsWholeNumberInRange(float value, float minValue)
        => !float.IsNaN(value) && !float.IsInfinity(value) && value % 1 == 0 && value >= minValue && value < -minValue;

    // 整数类型和枚举统一折算成 decimal（bool 不算整数，与 ToIntOrDefault 的既有约定一致）：decimal 能精确表示所有 64 位整数，之后复用 decimal 的范围检查
    private static bool TryGetIntegralValue(object value, out decimal integral)
    {
        switch (value)
        {
            case sbyte v:
                integral = v;
                return true;
            case byte v:
                integral = v;
                return true;
            case short v:
                integral = v;
                return true;
            case ushort v:
                integral = v;
                return true;
            case int v:
                integral = v;
                return true;
            case uint v:
                integral = v;
                return true;
            case long v:
                integral = v;
                return true;
            case ulong v:
                integral = v;
                return true;
            case Enum:
                integral = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                return true;
            default:
                integral = 0m;
                return false;
        }
    }

    // TryToXxx 已覆盖所有支持的输入：整数类型或枚举转换失败只可能是超出范围，其余类型视为不支持
    [DoesNotReturn]
    private static T ThrowConversionFailure<T>(object value, string targetTypeName)
    {
        if (TryGetIntegralValue(value, out _))
        {
            throw new OverflowException($"The {value.GetType().Name} value '{FormatInvariant(value)}' is outside the range of {targetTypeName}.");
        }

        throw new InvalidCastException($"Cannot convert a value of type '{value.GetType().FullName}' to {targetTypeName}.");
    }

    private static bool TryMapZeroOrOne(decimal value, out bool result)
    {
        result = value == 1m;
        return value == 1m || value == 0m;
    }

    private static string FormatInvariant(object value)
    {
        return value switch
        {
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}
