namespace Linger.Helper;

internal static class DateTimeConversionHelper
{
    private const DateTimeStyles ParseStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind;

    public static bool TryConvertStringToDateTime(string? value, out DateTime result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return DateTime.TryParse(value!.Trim(), CultureInfo.InvariantCulture, ParseStyles, out result);
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
