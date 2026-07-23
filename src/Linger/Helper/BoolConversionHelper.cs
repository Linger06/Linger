namespace Linger.Helper;

internal static class BoolConversionHelper
{
    private static readonly Dictionary<string, bool> s_boolMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "0", false }, { "false", false }, { "no", false }, { "n", false }, { "off", false },
        { "1", true }, { "true", true }, { "yes", true }, { "y", true }, { "on", true }
    };

    public static bool TryConvertStringToBool(string? value, out bool result)
    {
        result = false;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value!.Trim();
        if (bool.TryParse(trimmed, out result))
            return true;

        return s_boolMap.TryGetValue(trimmed, out result);
    }
}
