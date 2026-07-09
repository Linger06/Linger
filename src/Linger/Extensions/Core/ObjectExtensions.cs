using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Linger.Helper;

namespace Linger.Extensions.Core;

public static class ObjectExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> s_propertyCache = new();

    public static PropertyInfo? GetPropertyInfo(this object obj, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(propertyName);

        var type = obj.GetType();
        var map = s_propertyCache.GetOrAdd(type, CreatePropertyMap);
        return map.TryGetValue(propertyName, out var pi) ? pi : null;
    }

    public static object? GetPropertyValue(this object obj, string propertyName)
    {
        var pi = obj.GetPropertyInfo(propertyName);
        return pi?.GetValue(obj, null);
    }

    private static Dictionary<string, PropertyInfo> CreatePropertyMap(Type type)
    {
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var dict = new Dictionary<string, PropertyInfo>(props.Length, StringComparer.Ordinal);

        foreach (var p in props)
        {
            dict[p.Name] = p;
        }

        return dict;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull([NotNullWhen(false)] this object? value) => value is null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNull([NotNullWhen(true)] this object? value) => value is not null;

    public static bool IsNullOrEmpty([NotNullWhen(false)] this object? value)
    {
        if (value is null) return true;
        if (value is string str) return string.IsNullOrEmpty(str);

        var objectStr = value.ToString();
        return string.IsNullOrEmpty(objectStr);
    }

    public static bool IsNotNullOrEmpty([NotNullWhen(true)] this object? value)
    {
        if (value is null) return false;
        if (value is string str) return !string.IsNullOrEmpty(str);

        var objectStr = value.ToString();
        return !string.IsNullOrEmpty(objectStr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrDbNull([NotNullWhen(false)] this object? value) => value is DBNull or null;

    public static bool IsNumeric(this object? value) =>
        value is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;

    public static string? ToNormalizedString(this object? input, bool trim = false, bool treatEmptyAsNull = false)
    {
        if (input == null) return null;

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
        if (value is null || value is DBNull) return false;
        if (value is short s)
        {
            result = s;
            return true;
        }

        switch (value)
        {
            case int i:
                if (i < short.MinValue || i > short.MaxValue) return false;
                result = (short)i;
                return true;
            case long l:
                if (l < short.MinValue || l > short.MaxValue) return false;
                result = (short)l;
                return true;
            case byte b:
                result = b;
                return true;
            case decimal dec:
                return dec.TryToShort(out result);
            case string str:
                return str.TryToShort(out result);
            case double db:
                return TryConvertToWholeNumber(db, short.MinValue, short.MaxValue, out result);
            case float fl:
                return TryConvertToWholeNumber(fl, short.MinValue, short.MaxValue, out result);
        }

        var fallbackStr = value.ToString();
        return fallbackStr != null && fallbackStr.TryToShort(out result);
    }

    public static short ToShort(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToShort(out var result)) return result;

        switch (value)
        {
            case decimal dec:
                return dec.ToShort();
            case double db:
                return ConvertToWholeNumberOrThrow(db, short.MinValue, short.MaxValue, nameof(Double), "Int16", static x => (short)x);
            case float fl:
                return ConvertToWholeNumberOrThrow(fl, short.MinValue, short.MaxValue, nameof(Single), "Int16", static x => (short)x);
        }

        return Convert.ToInt16(value, CultureInfo.InvariantCulture);
    }

    public static short? ToShortOrNull(this object? value) => value.TryToShort(out var r) ? r : null;
    public static short ToShortOrDefault(this object? value, short defaultValue = 0) => value.TryToShort(out var r) ? r : defaultValue;

    public static bool TryToLong(this object? value, out long result)
    {
        result = 0L;
        if (value is null || value is DBNull) return false;
        if (value is long l)
        {
            result = l;
            return true;
        }

        switch (value)
        {
            case int i:
                result = i;
                return true;
            case short s:
                result = s;
                return true;
            case byte b:
                result = b;
                return true;
            case decimal dec:
                return dec.TryToLong(out result);
            case string str:
                return str.TryToLong(out result);
            case double db:
                return TryConvertToWholeNumber(db, long.MinValue, long.MaxValue, out result);
            case float fl:
                return TryConvertToWholeNumber(fl, long.MinValue, long.MaxValue, out result);
        }

        var fallbackStr = value.ToString();
        return fallbackStr != null && fallbackStr.TryToLong(out result);
    }

    public static long ToLong(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToLong(out var result)) return result;

        switch (value)
        {
            case decimal dec:
                return dec.ToLong();
            case double db:
                return ConvertToWholeNumberOrThrow(db, long.MinValue, long.MaxValue, nameof(Double), "Int64", static x => (long)x);
            case float fl:
                return ConvertToWholeNumberOrThrow(fl, long.MinValue, long.MaxValue, nameof(Single), "Int64", static x => (long)x);
        }

        return Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    public static long? ToLongOrNull(this object? value) => value.TryToLong(out var r) ? r : null;
    public static long ToLongOrDefault(this object? value, long defaultValue = 0L) => value.TryToLong(out var r) ? r : defaultValue;

    public static bool TryToDecimal(this object? value, out decimal result)
    {
        result = 0m;
        if (value is null || value is DBNull) return false;
        if (value is decimal dec)
        {
            result = dec;
            return true;
        }

        switch (value)
        {
            case string str:
                return str.TryToDecimal(out result);
            case int i:
                result = i;
                return true;
            case long l:
                result = l;
                return true;
            case short s:
                result = s;
                return true;
            case byte b:
                result = b;
                return true;
            case double db:
                if (db < (double)decimal.MinValue || db > (double)decimal.MaxValue) return false;
                result = (decimal)db;
                return true;
            case float fl:
                if (fl < (float)decimal.MinValue || fl > (float)decimal.MaxValue) return false;
                result = (decimal)fl;
                return true;
        }

        var fallbackStr = value.ToString();
        return fallbackStr != null && fallbackStr.TryToDecimal(out result);
    }

    public static decimal ToDecimal(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToDecimal(out var result)) return result;

        return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
    }

    public static decimal? ToDecimalOrNull(this object? value) => value.TryToDecimal(out var r) ? r : null;
    public static decimal ToDecimalOrDefault(this object? value, decimal defaultValue = default) => value.TryToDecimal(out var r) ? r : defaultValue;

    public static bool TryToInt(this object? value, out int result)
    {
        result = 0;
        if (value is null || value is DBNull) return false;
        if (value is int i)
        {
            result = i;
            return true;
        }

        switch (value)
        {
            case long l:
                if (l < int.MinValue || l > int.MaxValue) return false;
                result = (int)l;
                return true;
            case short s:
                result = s;
                return true;
            case byte b:
                result = b;
                return true;
            case decimal dec:
                return dec.TryToInt(out result);
            case string str:
                return str.TryToInt(out result);
            case double db:
                return TryConvertToWholeNumber(db, int.MinValue, int.MaxValue, out result);
            case float fl:
                return TryConvertToWholeNumber(fl, int.MinValue, int.MaxValue, out result);
        }

        var fallbackStr = value.ToString();
        return fallbackStr != null && fallbackStr.TryToInt(out result);
    }

    public static int ToInt(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToInt(out var result)) return result;

        switch (value)
        {
            case decimal dec:
                return dec.ToInt();
            case double db:
                return ConvertToWholeNumberOrThrow(db, int.MinValue, int.MaxValue, nameof(Double), "Int32", static x => (int)x);
            case float fl:
                return ConvertToWholeNumberOrThrow(fl, int.MinValue, int.MaxValue, nameof(Single), "Int32", static x => (int)x);
        }

        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    public static int? ToIntOrNull(this object? value) => value.TryToInt(out var r) ? r : null;
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
                result = dto.DateTime;
                return true;
            case string str:
                return str.TryToDateTime(out result);
#if NETCOREAPP
            case DateOnly dOnly:
                result = dOnly.ToDateTime(TimeOnly.MinValue);
                return true;
#endif
        }

        var fallbackStr = value.ToString();
        return fallbackStr != null && fallbackStr.TryToDateTime(out result);
    }

    public static DateTime ToDateTime(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToDateTime(out var result)) return result;

        return Convert.ToDateTime(value, CultureInfo.InvariantCulture);
    }

    public static DateTime? ToDateTimeOrNull(this object? value) => value.TryToDateTime(out var r) ? r : null;
    public static DateTime ToDateTimeOrDefault(this object? value, DateTime defaultValue = default) => value.TryToDateTime(out var r) ? r : defaultValue;

    public static bool TryToBool(this object? value, out bool result)
    {
        result = false;
        if (value is null || value is DBNull) return false;
        if (value is bool b)
        {
            result = b;
            return true;
        }

        switch (value)
        {
            case string str:
                return BoolConversionHelper.TryConvertStringToBool(str, out result);
            case int i:
                if (i == 1) { result = true; return true; }
                if (i == 0) { result = false; return true; }
                return false;
            case long l:
                if (l == 1L) { result = true; return true; }
                if (l == 0L) { result = false; return true; }
                return false;
            case short s:
                if (s == 1) { result = true; return true; }
                if (s == 0) { result = false; return true; }
                return false;
            case decimal dec:
                if (dec == 1m) { result = true; return true; }
                if (dec == 0m) { result = false; return true; }
                return false;
            case double db:
                if (db == 1.0) { result = true; return true; }
                if (db == 0.0) { result = false; return true; }
                return false;
        }

        var fallbackStr = value.ToString();
        return BoolConversionHelper.TryConvertStringToBool(fallbackStr, out result);
    }

    public static bool ToBool(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToBool(out var result)) return result;

        switch (value)
        {
            case int i:
                throw new InvalidCastException($"The integer value '{i}' is invalid for Boolean conversion. Only 1 and 0 are allowed.");
            case decimal dec:
                throw new InvalidCastException($"The decimal value '{dec}' is invalid for Boolean conversion. Only 1.0 and 0.0 are allowed.");
            case long l:
                throw new InvalidCastException($"The long value '{l}' is invalid for Boolean conversion. Only 1 and 0 are allowed.");
            case short s:
                throw new InvalidCastException($"The short value '{s}' is invalid for Boolean conversion. Only 1 and 0 are allowed.");
            case double db:
                throw new InvalidCastException($"The double value '{db}' is invalid for Boolean conversion. Only 1.0 and 0.0 are allowed.");
        }

        return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
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
        return fallbackStr != null && fallbackStr.TryToGuid(out result);
    }

    public static Guid ToGuid(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value), "Strict conversion requires a non-null input.");
        if (value.TryToGuid(out var result)) return result;

        if (value is byte[] bytes)
            throw new FormatException($"Byte array length must be exactly 16 bytes to convert to Guid. Actual length: {bytes.Length}");

        var finalStr = value.ToString();
        if (finalStr == null)
            throw new InvalidCastException($"Cannot convert type {value.GetType().FullName} to Guid.");
        return Guid.Parse(finalStr);
    }

    public static Guid? ToGuidOrNull(this object? value) => value.TryToGuid(out var r) ? r : null;
    public static Guid ToGuidOrDefault(this object? value, Guid defaultValue = default) => value.TryToGuid(out var r) ? r : defaultValue;

    public static bool TryToTarget<T>(this object? value, [NotNullWhen(true)] out T? result)
    {
        result = default;
        var targetType = typeof(T);
        if (!TypeConverter.TryConvertTo(value, targetType, out var converted) || converted is null)
        {
            return false;
        }

        if (converted is T typedValue)
        {
            result = typedValue;
            return true;
        }

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        result = CastToTarget<T>(converted, targetType, underlyingType);
        return true;
    }

    public static T ToTarget<T>(this object? value)
    {
        if (value is null || value is DBNull)
            throw new ArgumentNullException(nameof(value),
                $"Strict conversion requires a non-null input for target type '{typeof(T).Name}'.");

        var targetType = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        var converted = TypeConverter.ConvertTo(value, targetType);
        if (converted is null)
            throw new InvalidCastException(
                $"Cannot convert value '{value}' (Type: {value.GetType().Name}) to target type '{targetType.Name}'.");
        if (converted is T typedValue)
            return typedValue;
        return CastToTarget<T>(converted, targetType, underlyingType);
    }

    public static T ToTargetOrDefault<T>(this object? value, T defaultValue = default!)
        => value.TryToTarget<T>(out var result) ? result! : defaultValue;

    public static T? ToTargetOrNull<T>(this object? value) where T : struct
        => value.TryToTarget<T>(out var result) ? result : null;

    [return: NotNull]
    private static T CastToTarget<T>([DisallowNull] object value, Type targetType, Type underlyingType)
    {
        return targetType == underlyingType
            ? (T)value
            : (T)Activator.CreateInstance(targetType, value)!;
    }

    private static bool TryConvertToWholeNumber(double value, short minValue, short maxValue, out short result)
    {
        if (IsWholeNumberInRange(value, minValue, maxValue))
        {
            result = (short)value;
            return true;
        }

        result = 0;
        return false;
    }

    private static bool TryConvertToWholeNumber(float value, short minValue, short maxValue, out short result)
    {
        if (IsWholeNumberInRange(value, minValue, maxValue))
        {
            result = (short)value;
            return true;
        }

        result = 0;
        return false;
    }

    private static bool TryConvertToWholeNumber(double value, int minValue, int maxValue, out int result)
    {
        if (IsWholeNumberInRange(value, minValue, maxValue))
        {
            result = (int)value;
            return true;
        }

        result = 0;
        return false;
    }

    private static bool TryConvertToWholeNumber(float value, int minValue, int maxValue, out int result)
    {
        if (IsWholeNumberInRange(value, minValue, maxValue))
        {
            result = (int)value;
            return true;
        }

        result = 0;
        return false;
    }

    private static bool TryConvertToWholeNumber(double value, long minValue, long maxValue, out long result)
    {
        if (IsWholeNumberInRange(value, minValue, maxValue))
        {
            result = (long)value;
            return true;
        }

        result = 0;
        return false;
    }

    private static bool TryConvertToWholeNumber(float value, long minValue, long maxValue, out long result)
    {
        if (IsWholeNumberInRange(value, minValue, maxValue))
        {
            result = (long)value;
            return true;
        }

        result = 0;
        return false;
    }

    private static TResult ConvertToWholeNumberOrThrow<TResult>(
        double value,
        double minValue,
        double maxValue,
        string sourceTypeName,
        string targetTypeName,
        Func<double, TResult> converter)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value % 1 != 0)
            throw new InvalidCastException($"The {sourceTypeName.ToLowerInvariant()} value '{value}' cannot be converted to {targetTypeName} because it contains a fractional part.");
        if (value < minValue || value > maxValue)
            throw new OverflowException($"The {sourceTypeName.ToLowerInvariant()} value '{value}' is outside the range of {targetTypeName}.");
        return converter(value);
    }

    private static TResult ConvertToWholeNumberOrThrow<TResult>(
        float value,
        float minValue,
        float maxValue,
        string sourceTypeName,
        string targetTypeName,
        Func<float, TResult> converter)
    {
        if (float.IsNaN(value) || float.IsInfinity(value) || value % 1 != 0)
            throw new InvalidCastException($"The {sourceTypeName.ToLowerInvariant()} value '{value}' cannot be converted to {targetTypeName} because it contains a fractional part.");
        if (value < minValue || value > maxValue)
            throw new OverflowException($"The {sourceTypeName.ToLowerInvariant()} value '{value}' is outside the range of {targetTypeName}.");
        return converter(value);
    }

    private static bool IsWholeNumberInRange(double value, double minValue, double maxValue)
        => !double.IsNaN(value) && !double.IsInfinity(value) && value % 1 == 0 && value >= minValue && value <= maxValue;

    private static bool IsWholeNumberInRange(float value, float minValue, float maxValue)
        => !float.IsNaN(value) && !float.IsInfinity(value) && value % 1 == 0 && value >= minValue && value <= maxValue;

    private static bool TryConvertToTimeSpan(object value, out TimeSpan result)
    {
        result = default;

        if (value is TimeSpan timeSpan)
        {
            result = timeSpan;
            return true;
        }

        var stringValue = value as string ?? value.ToString();
        return stringValue is not null &&
               TimeSpan.TryParse(stringValue.Trim(), CultureInfo.InvariantCulture, out result);
    }
}
