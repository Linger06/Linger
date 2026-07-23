using System.Reflection;
using System.Runtime.CompilerServices;
using Linger.Helper;

namespace Linger.Extensions.Core;

public static class ObjectExtensions
{
    public static PropertyInfo? GetPropertyInfo(this object obj, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(propertyName);

        var type = obj.GetType();
        var map = PropertyMetadataCache.GetPropertyMap(type);
        return map.TryGetValue(propertyName, out var pi) ? pi : null;
    }

    public static object? GetPropertyValue(this object obj, string propertyName)
    {
        var pi = obj.GetPropertyInfo(propertyName);
        return pi?.GetValue(obj, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull([NotNullWhen(false)] this object? value) => value is null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNull([NotNullWhen(true)] this object? value) => value is not null;

    [Obsolete("Use a type-specific null or empty check. Arbitrary objects do not have a consistent empty-state definition.")]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this object? value)
    {
        if (value is null) return true;
        if (value is string str) return string.IsNullOrEmpty(str);

        var objectStr = value.ToString();
        return string.IsNullOrEmpty(objectStr);
    }

    [Obsolete("Use a type-specific null or empty check. Arbitrary objects do not have a consistent empty-state definition.")]
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

    /// <summary>
    /// Executes an action for each readable, non-indexed public property of the specified object.
    /// </summary>
    /// <param name="value">The object whose properties are enumerated.</param>
    /// <param name="action">The action to execute with each property name and value.</param>
    /// <remarks>Exceptions thrown by a property getter or <paramref name="action"/> propagate to the caller.</remarks>
    public static void ForEachProperty(this object? value, Action<string, object?> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (value is null)
        {
            return;
        }

        foreach (var property in PropertyMetadataCache.GetProperties(value.GetType()))
        {
            if (property.GetMethod?.IsPublic != true || property.GetIndexParameters().Length != 0)
            {
                continue;
            }

            action(property.Name, property.GetValue(value));
        }
    }

    /// <summary>
    /// Executes an action for each readable, non-indexed public property of the specified object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object whose properties are enumerated.</param>
    /// <param name="action">The action to execute with each property name and value.</param>
    /// <remarks>Use <see cref="ForEachProperty"/> instead.</remarks>
    [Obsolete]
    public static void ForIn<T>(this T? value, Action<string, object?> action)
        where T : class
    {
        value.ForEachProperty(action);
    }

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
            case string str:
                return str.ToShort();
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
            case string str:
                return str.ToLong();
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
                if (double.IsNaN(db) || double.IsInfinity(db)) return false;
                if (db < (double)decimal.MinValue || db > (double)decimal.MaxValue) return false;
                result = (decimal)db;
                return true;
            case float fl:
                if (float.IsNaN(fl) || float.IsInfinity(fl)) return false;
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

    /// <summary>
    /// Attempts to convert the specified value to a <see cref="double"/> using the invariant culture.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">The converted value when the conversion succeeds; otherwise, zero.</param>
    /// <returns><see langword="true"/> when the value can be converted; otherwise, <see langword="false"/>.</returns>
    public static bool TryToDouble(this object? value, out double result)
    {
        result = 0d;
        if (value is null || value is DBNull) return false;
        if (value is double db)
        {
            result = db;
            return true;
        }

        switch (value)
        {
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
        }

        return false;
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

        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
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
        if (value is null || value is DBNull) return false;
        if (value is float fl)
        {
            result = fl;
            return true;
        }

        switch (value)
        {
            case string str:
                return str.TryToFloat(out result);
            case double db when double.IsNaN(db) || double.IsInfinity(db):
                result = (float)db;
                return true;
            case double db when db >= -float.MaxValue && db <= float.MaxValue:
                result = (float)db;
                return true;
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
        }

        return false;
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

        return Convert.ToSingle(value, CultureInfo.InvariantCulture);
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
            case string str:
                return str.ToInt();
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
                result = dto.UtcDateTime;
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

        switch (value)
        {
            case DateTimeOffset dto:
                return dto.UtcDateTime;
            case string str:
                return str.ToDateTime();
#if NETCOREAPP
            case DateOnly dOnly:
                return dOnly.ToDateTime(TimeOnly.MinValue);
#endif
        }

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
            case float fl:
                if (fl == 1f) { result = true; return true; }
                if (fl == 0f) { result = false; return true; }
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
                throw new InvalidCastException($"The integer value '{FormatInvariant(i)}' is invalid for Boolean conversion. Only 1 and 0 are allowed.");
            case decimal dec:
                throw new InvalidCastException($"The decimal value '{FormatInvariant(dec)}' is invalid for Boolean conversion. Only 1.0 and 0.0 are allowed.");
            case long l:
                throw new InvalidCastException($"The long value '{FormatInvariant(l)}' is invalid for Boolean conversion. Only 1 and 0 are allowed.");
            case short s:
                throw new InvalidCastException($"The short value '{FormatInvariant(s)}' is invalid for Boolean conversion. Only 1 and 0 are allowed.");
            case double db:
                throw new InvalidCastException($"The double value '{FormatInvariant(db)}' is invalid for Boolean conversion. Only 1.0 and 0.0 are allowed.");
            case float fl:
                throw new InvalidCastException($"The float value '{FormatInvariant(fl)}' is invalid for Boolean conversion. Only 1.0 and 0.0 are allowed.");
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
        {
            throw new ArgumentNullException(nameof(value),
                $"Strict conversion requires a non-null input for target type '{typeof(T).Name}'.");
        }

        var targetType = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        var converted = TypeConverter.ConvertTo(value, targetType);
        if (converted is null)
        {
            throw new InvalidCastException(
                $"Cannot convert value '{FormatInvariant(value)}' (Type: {value.GetType().Name}) to target type '{targetType.Name}'.");
        }

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
            throw new InvalidCastException($"The {sourceTypeName.ToLowerInvariant()} value '{FormatInvariant(value)}' cannot be converted to {targetTypeName} because it contains a fractional part.");
        if (value < minValue || value > maxValue)
            throw new OverflowException($"The {sourceTypeName.ToLowerInvariant()} value '{FormatInvariant(value)}' is outside the range of {targetTypeName}.");
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
            throw new InvalidCastException($"The {sourceTypeName.ToLowerInvariant()} value '{FormatInvariant(value)}' cannot be converted to {targetTypeName} because it contains a fractional part.");
        if (value < minValue || value > maxValue)
            throw new OverflowException($"The {sourceTypeName.ToLowerInvariant()} value '{FormatInvariant(value)}' is outside the range of {targetTypeName}.");
        return converter(value);
    }

    private static bool IsWholeNumberInRange(double value, double minValue, double maxValue)
        => !double.IsNaN(value) && !double.IsInfinity(value) && value % 1 == 0 && value >= minValue && value <= maxValue;

    private static bool IsWholeNumberInRange(float value, float minValue, float maxValue)
        => !float.IsNaN(value) && !float.IsInfinity(value) && value % 1 == 0 && value >= minValue && value <= maxValue;

    private static string FormatInvariant(object value)
    {
        return value switch
        {
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}
