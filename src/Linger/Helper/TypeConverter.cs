// Copyright (c) Linger. All rights reserved.
// Licensed under the MIT License.
using System.ComponentModel;
using Linger.Extensions.Core;

namespace Linger.Helper;

public static class TypeConverter
{
    private enum KnownTypeConversionStatus
    {
        NotSupported,
        Success,
        Failed
    }

    public static object? ConvertTo(object? value, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        if (value is null || value is DBNull)
        {
            if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) is not null)
            {
                return null;
            }

            throw new ArgumentNullException(
                nameof(value),
                $"Strict conversion requires a non-null input for target type '{targetType.Name}'.");
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

        var knownTypeStatus = ConvertKnownType(
            value,
            actualType,
            throwOnFailure: true,
            out var knownTypeResult);
        if (knownTypeStatus is not KnownTypeConversionStatus.NotSupported)
        {
            return knownTypeResult;
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

        var knownTypeStatus = ConvertKnownType(
            value,
            actualType,
            throwOnFailure: false,
            out result);
        if (knownTypeStatus is not KnownTypeConversionStatus.NotSupported)
        {
            return knownTypeStatus is KnownTypeConversionStatus.Success;
        }

        return TryConvertUsingFallback(value, sourceType, actualType, out result);
    }

    private static KnownTypeConversionStatus ConvertKnownType(
        object value,
        Type actualType,
        bool throwOnFailure,
        out object? result)
    {
        switch (Type.GetTypeCode(actualType))
        {
            case TypeCode.String:
            {
                result = value as string ?? value.ToString();
                if (throwOnFailure && result is null)
                {
                    result = string.Empty;
                }

                return KnownTypeConversionStatus.Success;
            }
            case TypeCode.Int16:
            {
                var succeeded = value.TryToShort(out var converted);
                return CompleteKnownTypeConversion(
                    value,
                    succeeded,
                    converted,
                    throwOnFailure,
                    static input => input.ToShort(),
                    out result);
            }
            case TypeCode.Int32:
            {
                var succeeded = value.TryToInt(out var converted);
                return CompleteKnownTypeConversion(
                    value,
                    succeeded,
                    converted,
                    throwOnFailure,
                    static input => input.ToInt(),
                    out result);
            }
            case TypeCode.Int64:
            {
                var succeeded = value.TryToLong(out var converted);
                return CompleteKnownTypeConversion(
                    value,
                    succeeded,
                    converted,
                    throwOnFailure,
                    static input => input.ToLong(),
                    out result);
            }
            case TypeCode.Decimal:
            {
                var succeeded = value.TryToDecimal(out var converted);
                return CompleteKnownTypeConversion(
                    value,
                    succeeded,
                    converted,
                    throwOnFailure,
                    static input => input.ToDecimal(),
                    out result);
            }
            case TypeCode.Double:
            {
                var succeeded = value.TryToDouble(out var converted);
                return CompleteKnownTypeConversion(
                    value,
                    succeeded,
                    converted,
                    throwOnFailure,
                    static input => input.ToDouble(),
                    out result);
            }
            case TypeCode.Single:
            {
                var succeeded = value.TryToFloat(out var converted);
                return CompleteKnownTypeConversion(
                    value,
                    succeeded,
                    converted,
                    throwOnFailure,
                    static input => input.ToFloat(),
                    out result);
            }
            case TypeCode.Boolean:
            {
                var succeeded = value.TryToBool(out var converted);
                return CompleteKnownTypeConversion(
                    value,
                    succeeded,
                    converted,
                    throwOnFailure,
                    static input => input.ToBool(),
                    out result);
            }
            case TypeCode.DateTime:
            {
                var succeeded = value.TryToDateTime(out var converted);
                return CompleteKnownTypeConversion(
                    value,
                    succeeded,
                    converted,
                    throwOnFailure,
                    static input => input.ToDateTime(),
                    out result);
            }
            case TypeCode.Object when actualType == typeof(Guid):
            {
                var succeeded = value.TryToGuid(out var converted);
                return CompleteKnownTypeConversion(
                    value,
                    succeeded,
                    converted,
                    throwOnFailure,
                    static input => input.ToGuid(),
                    out result);
            }
            case TypeCode.Object when actualType == typeof(TimeSpan):
            {
                var succeeded = TryConvertToTimeSpan(value, out var converted);
                return CompleteKnownTypeConversion(
                    value,
                    succeeded,
                    converted,
                    throwOnFailure,
                    ConvertToTimeSpan,
                    out result);
            }
            default:
                result = null;
                return KnownTypeConversionStatus.NotSupported;
        }
    }

    private static KnownTypeConversionStatus CompleteKnownTypeConversion<T>(
        object value,
        bool succeeded,
        T convertedValue,
        bool throwOnFailure,
        Func<object, T> strictConverter,
        out object? result)
    {
        if (succeeded)
        {
            result = convertedValue;
            return KnownTypeConversionStatus.Success;
        }

        if (throwOnFailure)
        {
            result = strictConverter(value);
            return KnownTypeConversionStatus.Success;
        }

        result = null;
        return KnownTypeConversionStatus.Failed;
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
