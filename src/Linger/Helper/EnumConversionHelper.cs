namespace Linger.Helper;

internal static class EnumConversionHelper
{
    public static bool TryConvertToEnum(object value, Type enumType, out object? result)
    {
        result = null;

        if (value is string stringValue)
        {
            var trimmed = stringValue.Trim();
            if (trimmed.Length == 0)
            {
                return false;
            }

#if NET8_0_OR_GREATER
            if (!Enum.TryParse(enumType, trimmed, true, out var parsed) || parsed is null)
            {
                return false;
            }
#else
            object parsed;
            try
            {
                parsed = Enum.Parse(enumType, trimmed, true);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
#endif

            if (IsNumericEnumText(trimmed) && !Enum.IsDefined(enumType, parsed))
            {
                return false;
            }

            result = parsed;
            return true;
        }

        // Enum.IsDefined 只接受整数类型，且要求类型与枚举底层类型完全一致：long 传给 int 枚举会抛 ArgumentException，
        // double、decimal、bool 会抛 InvalidOperationException。所以只放行整数，并先换算到底层类型
        if (value is not (sbyte or byte or short or ushort or int or uint or long or ulong))
        {
            return false;
        }

        try
        {
            // 不用 Enum.ToObject 直接换算：它会静默截断越界值（如 300 变成 byte 枚举的 44），Convert.ChangeType 则会抛 OverflowException
            var underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(enumType), CultureInfo.InvariantCulture);
            if (!Enum.IsDefined(enumType, underlyingValue))
            {
                return false;
            }

            result = Enum.ToObject(enumType, underlyingValue);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }

    }

    private static bool IsNumericEnumText(string value)
    {
        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) ||
               ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
    }
}
