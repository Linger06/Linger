// Copyright (c) Linger. All rights reserved.
// Licensed under the MIT License.
using System.ComponentModel;
using System.Globalization;
using Linger.Extensions.Core;

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

        var sourceType = value.GetType();
        if (sourceType == targetType)
        {
            return value;
        }

        var actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (sourceType == actualType)
        {
            return value;
        }

        if (actualType.IsEnum)
        {
            if (EnumConversionHelper.TryConvertToEnum(value, actualType, out var enumResult))
            {
                return enumResult;
            }

            throw new InvalidCastException(
                $"Cannot convert value '{value}' (Type: {sourceType.Name}) to enum type '{actualType.Name}'.");
        }

        if (actualType == typeof(DateTime) && value is double oaDateValue)
        {
            return DateTime.FromOADate(oaDateValue);
        }

        if (IsKnownType(actualType))
        {
            return ConvertKnownType(value, actualType);
        }

        return ConvertUsingFallback(value, sourceType, actualType);
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

        if (TryConvertKnownType(value, actualType, out result))
        {
            return true;
        }

        return TryConvertUsingFallback(value, sourceType, actualType, out result);
    }

    private static bool TryConvertKnownType(object value, Type actualType, out object? result)
    {
        if (actualType == typeof(string))
        {
            result = value as string ?? value.ToString();
            return true;
        }

        if (actualType == typeof(short))
        {
            if (value.TryToShort(out var converted))
            {
                result = converted;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(int))
        {
            if (value.TryToInt(out var converted))
            {
                result = converted;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(long))
        {
            if (value.TryToLong(out var converted))
            {
                result = converted;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(decimal))
        {
            if (value.TryToDecimal(out var converted))
            {
                result = converted;
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

            if (value is double d)
            {
                result = d;
                return true;
            }

            if (value is string stringValue &&
                double.TryParse(stringValue.Trim(), NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(float))
        {
            if (value is float f)
            {
                result = f;
                return true;
            }

            if (value is string stringValue &&
                float.TryParse(stringValue.Trim(), NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture, out var r))
            {
                result = r;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(bool))
        {
            if (value.TryToBool(out var converted))
            {
                result = converted;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(DateTime))
        {
            if (value.TryToDateTime(out var converted))
            {
                result = converted;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(Guid))
        {
            if (value.TryToGuid(out var converted))
            {
                result = converted;
                return true;
            }

            result = null;
            return false;
        }

        if (actualType == typeof(TimeSpan))
        {
            if (TryConvertToTimeSpan(value, out var converted))
            {
                result = converted;
                return true;
            }

            result = null;
            return false;
        }

        result = null;
        return false;
    }

    private static bool IsKnownType(Type actualType)
        => actualType == typeof(string) ||
           actualType == typeof(short) ||
           actualType == typeof(int) ||
           actualType == typeof(long) ||
           actualType == typeof(decimal) ||
           actualType == typeof(double) ||
           actualType == typeof(float) ||
           actualType == typeof(bool) ||
           actualType == typeof(DateTime) ||
           actualType == typeof(Guid) ||
           actualType == typeof(TimeSpan);

    private static object ConvertKnownType(object value, Type actualType)
    {
        if (actualType == typeof(string))
        {
            return value as string ?? value.ToString() ?? string.Empty;
        }

        if (actualType == typeof(short))
        {
            return value.ToShort();
        }

        if (actualType == typeof(int))
        {
            return value.ToInt();
        }

        if (actualType == typeof(long))
        {
            return value.ToLong();
        }

        if (actualType == typeof(decimal))
        {
            return value.ToDecimal();
        }

        if (actualType == typeof(double))
        {
            return ConvertToDouble(value);
        }

        if (actualType == typeof(float))
        {
            return ConvertToSingle(value);
        }

        if (actualType == typeof(bool))
        {
            return value.ToBool();
        }

        if (actualType == typeof(DateTime))
        {
            return value.ToDateTime();
        }

        if (actualType == typeof(Guid))
        {
            return value.ToGuid();
        }

        if (actualType == typeof(TimeSpan))
        {
            return ConvertToTimeSpan(value);
        }

        throw new InvalidCastException(
            $"Cannot convert value '{value}' (Type: {value.GetType().Name}) to target type '{actualType.Name}'.");
    }

    private static bool TryConvertUsingFallback(object value, Type sourceType, Type actualType, out object? result)
    {
        try
        {
            var converted = ConvertUsingFallback(value, sourceType, actualType);
            if (converted is not null)
            {
                result = converted;
                return true;
            }

            result = null;
            return false;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    private static object ConvertUsingFallback(object value, Type sourceType, Type actualType)
    {
        var converter = TypeDescriptor.GetConverter(actualType);
        if (value is string stringValue && converter.CanConvertFrom(typeof(string)))
        {
            return converter.ConvertFromInvariantString(stringValue)!;
        }

        if (converter.CanConvertFrom(sourceType))
        {
            return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value)!;
        }

        return Convert.ChangeType(value, actualType, CultureInfo.InvariantCulture);
    }

    private static double ConvertToDouble(object value)
    {
        if (value is float f)
        {
            return f;
        }

        if (value is double d)
        {
            return d;
        }

        if (value is string stringValue)
        {
            return double.Parse(stringValue.Trim(), NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture);
        }

        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }

    private static float ConvertToSingle(object value)
    {
        if (value is float f)
        {
            return f;
        }

        if (value is string stringValue)
        {
            return float.Parse(stringValue.Trim(), NumberStyles.Float | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture);
        }

        return Convert.ToSingle(value, CultureInfo.InvariantCulture);
    }

    private static TimeSpan ConvertToTimeSpan(object value)
    {
        if (TryConvertToTimeSpan(value, out var result))
        {
            return result;
        }

        throw new FormatException($"String '{value}' was not recognized as a valid TimeSpan.");
    }

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
