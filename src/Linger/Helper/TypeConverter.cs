// Copyright (c) Linger. All rights reserved.
// Licensed under the MIT License.
using Linger.Extensions.Core;

namespace Linger.Helper;

public static class TypeConverter
{
    /// <summary>
    /// Attempts to convert a value using the built-in conversion rules supported by <c>Linger.Utils</c>.
    /// Supported targets are strings, numeric primitives, booleans, dates, <see cref="Guid"/>,
    /// <see cref="TimeSpan"/>, enums, and their nullable counterparts. This path does not discover
    /// custom type converters and is suitable for AOT-friendly mappings.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="result">The converted value when conversion succeeds; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when conversion succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="targetType"/> is <see langword="null"/>.</exception>
    public static bool TryConvert(object? value, Type targetType, out object? result)
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

        if (targetType.IsInstanceOfType(value))
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

    private static bool TryConvertSupportedScalar(
        object value,
        Type actualType,
        out object? result)
    {
        switch (Type.GetTypeCode(actualType))
        {
            case TypeCode.String:
            {
                return TryConvertToString(value, out result);
            }
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

    private static bool SetResult<T>(
        bool succeeded,
        T convertedValue,
        out object? result)
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

    private static bool TryConvertToTimeSpan(object value, out TimeSpan result)
    {
        result = default;

        return value is string stringValue &&
               TimeSpan.TryParse(stringValue.Trim(), CultureInfo.InvariantCulture, out result);
    }

}
