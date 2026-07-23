namespace Linger.Helper;

internal static class EnumConversionHelper
{
    public static bool TryConvertToEnum(object value, Type enumType, out object? result)
    {
        result = null;

        try
        {
            if (value is string stringValue)
            {
                var trimmed = stringValue.Trim();
                if (trimmed.Length == 0)
                    return false;

#if NET8_0_OR_GREATER
                if (!Enum.TryParse(enumType, trimmed, ignoreCase: true, out var parsed) || parsed is null)
                    return false;
#else
                var parsed = Enum.Parse(enumType, trimmed, ignoreCase: true);
#endif
                if (IsNumericEnumText(trimmed) && !Enum.IsDefined(enumType, parsed))
                    return false;

                result = parsed;
                return true;
            }

            if (Enum.IsDefined(enumType, value))
            {
                result = Enum.ToObject(enumType, value);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool IsNumericEnumText(string value)
    {
        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) ||
               ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
    }
}
