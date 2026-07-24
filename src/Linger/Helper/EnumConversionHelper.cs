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

        try
        {
            if (!Enum.IsDefined(enumType, value))
            {
                return false;
            }

            result = Enum.ToObject(enumType, value);
            return true;
        }
        catch (ArgumentException)
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
