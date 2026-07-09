using System.Globalization;

namespace Linger.Helper;

internal static class DateTimeConversionHelper
{
    private static readonly string[] s_extraFormats =
    {
        "M/d/yyyy h:mm:ss tt",
        "M/d/yyyy hh:mm:ss tt",
        "yyyy/M/d H:mm:ss",
        "yyyy/M/d h:mm:ss tt",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy/MM/dd HH:mm:ss"
    };

    private const DateTimeStyles ParseStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind;

    public static bool TryConvertStringToDateTime(string? value, out DateTime result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value!.Trim();
        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, ParseStyles, out result))
        {
            return true;
        }

        return DateTime.TryParseExact(
            trimmed,
            s_extraFormats,
            CultureInfo.InvariantCulture,
            ParseStyles,
            out result);
    }

    public static DateTime ParseStringToDateTime(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (TryConvertStringToDateTime(value, out var result))
        {
            return result;
        }

        throw new FormatException($"String '{value}' was not recognized as a valid DateTime.");
    }
}
