// Copyright (c) Linger. All rights reserved.
// Licensed under the MIT License.
using System.ComponentModel;
using System.Globalization;

namespace Linger.Helper;

public static class TypeConverter
{
    public static object? ConvertTo(object? value, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        if (value is null || value is DBNull)
        {
            if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) is not null)
            {
                return null;
            }

            throw new InvalidCastException(
                $"Cannot convert null-like value to non-nullable target type '{targetType.Name}'.");
        }

        if (TryConvertTo(value, targetType, out var result))
        {
            return result;
        }

        throw new InvalidCastException(
            $"Cannot convert value '{value ?? "null"}' (Type: {value?.GetType().Name ?? "unknown"}) to target type '{targetType.Name}'.");
    }

    public static bool TryConvertTo(object? value, Type targetType, out object? result)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        if (value is null || value is DBNull)
        {
            result = null;
            return false;
        }

        var sourceType = value.GetType();
        if (sourceType == targetType)
        {
            result = value;
            return true;
        }

        var actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (sourceType == actualType)
        {
            result = value;
            return true;
        }

        try
        {
            if (actualType.IsEnum)
            {
                if (EnumConversionHelper.TryConvertToEnum(value, actualType, out var enumResult))
                {
                    result = enumResult;
                    return true;
                }

                result = null;
                return false;
            }

            if (actualType == typeof(DateTime) && value is double oaDateValue)
            {
                result = DateTime.FromOADate(oaDateValue);
                return true;
            }

            return TryConvertToType(value, sourceType, actualType, out result);
        }
        catch
        {
            result = null;
            return false;
        }
    }

    private static bool TryConvertToType(object value, Type sourceType, Type actualType, out object? result)
    {
        var stringValueCache = value as string;

        if (actualType == typeof(string))
        {
            result = stringValueCache ?? value.ToString();
            return true;
        }

        switch (value)
        {
            case int i when actualType == typeof(int):
                result = i;
                return true;
            case long l when actualType == typeof(long):
                result = l;
                return true;
            case decimal d when actualType == typeof(decimal):
                result = d;
                return true;
            case double d when actualType == typeof(double):
                result = d;
                return true;
            case float f when actualType == typeof(float):
                result = f;
                return true;
            case bool b when actualType == typeof(bool):
                result = b;
                return true;
            case DateTime dt when actualType == typeof(DateTime):
                result = dt;
                return true;
            case Guid g when actualType == typeof(Guid):
                result = g;
                return true;
        }

        stringValueCache ??= value.ToString() ?? string.Empty;

        if (actualType == typeof(int))
        {
            if (int.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(long))
        {
            if (long.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(decimal))
        {
            if (decimal.TryParse(stringValueCache, NumberStyles.Number, CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(double))
        {
            if (value is float f)
            {
                result = (double)f;
                return true;
            }

            if (double.TryParse(stringValueCache, NumberStyles.Float, CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(float))
        {
            if (float.TryParse(stringValueCache, NumberStyles.Float, CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(bool))
        {
            if (BoolConversionHelper.TryConvertStringToBool(stringValueCache, out var boolResult))
            {
                result = boolResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(DateTime))
        {
            if (DateTimeConversionHelper.TryConvertStringToDateTime(stringValueCache, out var dateResult))
            {
                result = dateResult;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(Guid))
        {
            if (Guid.TryParse(stringValueCache, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(short))
        {
            if (short.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(byte))
        {
            if (byte.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(sbyte))
        {
            if (sbyte.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(ushort))
        {
            if (ushort.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(uint))
        {
            if (uint.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(ulong))
        {
            if (ulong.TryParse(stringValueCache, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(TimeSpan))
        {
            if (TimeSpan.TryParse(stringValueCache.Trim(), CultureInfo.InvariantCulture, out var tsResult))
            {
                result = tsResult;
                return true;
            }

            result = null;
            return false;
        }

        var converter = TypeDescriptor.GetConverter(actualType);
        if (value is string && converter.CanConvertFrom(typeof(string)))
        {
            var converted = converter.ConvertFromInvariantString(stringValueCache);
            if (converted is not null)
            {
                result = converted;
                return true;
            }
        }

        if (converter.CanConvertFrom(sourceType))
        {
            var converted = converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
            if (converted is not null)
            {
                result = converted;
                return true;
            }
        }

        result = Convert.ChangeType(value, actualType, CultureInfo.InvariantCulture);
        return true;
    }

}
