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

    /// <summary>
    /// Attempts to convert a value using the built-in conversion rules supported by <c>Linger.Utils</c>.
    /// Supported targets are strings, numeric primitives, booleans, <see cref="DateTime"/>, <see cref="Guid"/>,
    /// <see cref="TimeSpan"/>, enums, and their nullable counterparts. This method does not discover
    /// custom type converters.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="result">The converted value when conversion succeeds; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when conversion succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="targetType"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// if (TypeConverter.TryConvert("42", typeof(int), out var value))
    /// {
    ///     int number = (int)value!;
    /// }
    /// </code>
    /// </example>
    public static bool TryConvert(object? value, Type targetType, out object? result)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        if (value is null || value is DBNull)
        {
            result = null;
            return false;
        }

        var sourceType = value.GetType();
        if (sourceType == targetType || targetType.IsInstanceOfType(value))
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
            try
            {
                result = DateTime.FromOADate(oaDateValue);
                return true;
            }
            catch (ArgumentException)
            {
                result = null;
                return false;
            }
        }

        return TryConvertSupportedScalar(value, actualType, out result);
    }

    private static bool TryConvertSupportedScalar(object value, Type actualType, out object? result)
    {
        switch (Type.GetTypeCode(actualType))
        {
            case TypeCode.String:
                return TryConvertToString(value, out result);
            case TypeCode.Int16:
            {
                var succeeded = value.TryToShort(out var converted);
                return SetResult(succeeded, converted, out result);
            }
            case TypeCode.Int32:
            {
                var succeeded = value.TryToInt(out var converted);
                return SetResult(succeeded, converted, out result);
            }
            case TypeCode.Int64:
            {
                var succeeded = value.TryToLong(out var converted);
                return SetResult(succeeded, converted, out result);
            }
            case TypeCode.Decimal:
            {
                var succeeded = value.TryToDecimal(out var converted);
                return SetResult(succeeded, converted, out result);
            }
            case TypeCode.Double:
            {
                var succeeded = value.TryToDouble(out var converted);
                return SetResult(succeeded, converted, out result);
            }
            case TypeCode.Single:
            {
                var succeeded = value.TryToFloat(out var converted);
                return SetResult(succeeded, converted, out result);
            }
            case TypeCode.Byte:
                return TryConvertNumeric(value, static (input, provider) => Convert.ToByte(input, provider), out result);
            case TypeCode.SByte:
                return TryConvertNumeric(value, static (input, provider) => Convert.ToSByte(input, provider), out result);
            case TypeCode.UInt16:
                return TryConvertNumeric(value, static (input, provider) => Convert.ToUInt16(input, provider), out result);
            case TypeCode.UInt32:
                return TryConvertNumeric(value, static (input, provider) => Convert.ToUInt32(input, provider), out result);
            case TypeCode.UInt64:
                return TryConvertNumeric(value, static (input, provider) => Convert.ToUInt64(input, provider), out result);
            case TypeCode.Boolean:
            {
                var succeeded = value.TryToBool(out var converted);
                return SetResult(succeeded, converted, out result);
            }
            case TypeCode.DateTime:
            {
                var succeeded = value.TryToDateTime(out var converted);
                return SetResult(succeeded, converted, out result);
            }
            case TypeCode.Object when actualType == typeof(Guid):
            {
                var succeeded = value.TryToGuid(out var converted);
                return SetResult(succeeded, converted, out result);
            }
            case TypeCode.Object when actualType == typeof(TimeSpan):
            {
                var succeeded = TryConvertToTimeSpan(value, out var converted);
                return SetResult(succeeded, converted, out result);
            }
            default:
                result = null;
                return false;
        }
    }

    private static bool SetResult<T>(bool succeeded, T convertedValue, out object? result)
    {
        if (succeeded)
        {
            result = convertedValue;
            return true;
        }

        result = null;
        return false;
    }

    private static bool TryConvertNumeric<T>(
        object value,
        Func<object, IFormatProvider, T> converter,
        out object? result)
    {
        if (value is not string && !value.IsNumeric())
        {
            result = null;
            return false;
        }

        try
        {
            result = converter(value, CultureInfo.InvariantCulture);
            return true;
        }
        catch (FormatException)
        {
            result = null;
            return false;
        }
        catch (InvalidCastException)
        {
            result = null;
            return false;
        }
        catch (OverflowException)
        {
            result = null;
            return false;
        }
    }

    private static bool TryConvertToString(object value, out object? result)
    {
        if (value is not string &&
            !value.IsNumeric() &&
            value is not bool and not DateTime and not DateTimeOffset and not Guid and not TimeSpan and not Enum)
        {
            result = null;
            return false;
        }

        result = Convert.ToString(value, CultureInfo.InvariantCulture);
        return result is not null;
    }

    [Obsolete("Use TryConvert for runtime target types, or a type-specific conversion API. This API will be removed in 2.0.0.")]
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

    [Obsolete("Use TryConvert instead. This API will be removed in 2.0.0.")]
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
