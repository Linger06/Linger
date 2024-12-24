namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for object conversions and manipulations.
/// </summary>
public static partial class ObjectExtensions
{
    /// <summary>
    /// Converts the input object to a trimmed string. Returns an empty string if the input is null.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A trimmed string representation of the input object, or an empty string if the input is null.</returns>
    public static string ToNotSpaceString(this object? input) => input?.ToString()?.Trim() ?? string.Empty;

    /// <summary>
    /// Converts the input object to a string. Returns the specified default value if the input is null.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the input is null.</param>
    /// <returns>A string representation of the input object, or the specified default value if the input is null.</returns>
    public static string ToSafeString(this object? input, string defaultValue = "") => input != null ? input.ToString() ?? defaultValue : defaultValue;

    /// <summary>
    /// Converts the input object to a string. Returns an empty string if the input is null.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A string representation of the input object, or an empty string if the input is null.</returns>
    public static string ToStringOrEmpty(this object? input) => input?.ToString() ?? string.Empty;

    /// <summary>
    /// Converts the input object to a string. Returns null if the input is null.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A string representation of the input object, or null if the input is null.</returns>
    public static string? ToStringOrNull(this object? input) => input?.ToString();

    /// <summary>
    /// Converts the input object to a short. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A short representation of the input object, or the specified default value if the conversion fails.</returns>
    public static short ToShort(this object? input, short defaultValue = 0)
    {
        return ToShortOrNull(input) ?? defaultValue;
    }

    /// <summary>
    /// Converts the input object to a nullable short. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable short representation of the input object, or null if the conversion fails.</returns>
    public static short? ToShortOrNull(this object? input)
    {
        return input.ToStringOrNull().ToShortOrNull();
    }

    /// <summary>
    /// Converts the input object to a long. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A long representation of the input object, or the specified default value if the conversion fails.</returns>
    public static long ToLong(this object? input, long defaultValue = 0)
    {
        return ToLongOrNull(input) ?? defaultValue;
    }

    /// <summary>
    /// Converts the input object to a nullable long. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable long representation of the input object, or null if the conversion fails.</returns>
    public static long? ToLongOrNull(this object? input)
    {
        return input.ToStringOrNull().ToLongOrNull();
    }

    /// <summary>
    /// Converts the input object to a decimal. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A decimal representation of the input object, or the specified default value if the conversion fails.</returns>
    public static decimal ToDecimal(this object? input, int? digits = null)
    {
        return ToDecimal(input, 0, digits);
    }

    /// <summary>
    /// Converts the input object to a decimal. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A decimal representation of the input object, or the specified default value if the conversion fails.</returns>
    public static decimal ToDecimal(this object? input, decimal defaultValue, int? digits = null)
    {
        return input.ToStringOrNull().ToDecimal(defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to a nullable decimal. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A nullable decimal representation of the input object, or null if the conversion fails.</returns>
    public static decimal? ToDecimalOrNull(this object? input, decimal? defaultValue = null, int? digits = null)
    {
        return input.ToStringOrNull().ToDecimalOrNull(defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to an integer. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>An integer representation of the input object, or the specified default value if the conversion fails.</returns>
    public static int ToInt(this object? input, int defaultValue = 0)
    {
        return ToIntOrNull(input) ?? defaultValue;
    }

    /// <summary>
    /// Converts the input object to a nullable integer. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable integer representation of the input object, or null if the conversion fails.</returns>
    public static int? ToIntOrNull(this object? input)
    {
        return input.ToStringOrNull().ToIntOrNull();
    }

    /// <summary>
    /// Converts the input object to a double. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A double representation of the input object, or the specified default value if the conversion fails.</returns>
    public static double ToDouble(this object? input, double defaultValue = 0, int? digits = null)
    {
        return input.ToStringOrNull().ToDouble(defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to a nullable double. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A nullable double representation of the input object, or null if the conversion fails.</returns>
    public static double? ToDoubleOrNull(this object? input, double? defaultValue = null, int? digits = null)
    {
        return input.ToStringOrNull().ToDoubleOrNull(defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to a float. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A float representation of the input object, or the specified default value if the conversion fails.</returns>
    public static float ToFloat(this object? input, int? digits = null)
    {
        return ToFloat(input, 0, digits);
    }

    /// <summary>
    /// Converts the input object to a float. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A float representation of the input object, or the specified default value if the conversion fails.</returns>
    public static float ToFloat(this object? input, float defaultValue, int? digits = null)
    {
        return input.ToStringOrNull().ToFloat(defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to a nullable float. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>A nullable float representation of the input object, or null if the conversion fails.</returns>
    public static float? ToFloatOrNull(this object? input, float? defaultValue = null, int? digits = null)
    {
        return input.ToStringOrNull().ToFloatOrNull(defaultValue, digits);
    }

    /// <summary>
    /// Converts the input object to a DateTime. Returns DateTime.MinValue if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A DateTime representation of the input object, or DateTime.MinValue if the conversion fails.</returns>
    public static DateTime ToDateTime(this object input)
    {
        return ToDateTimeOrNull(input) ?? DateTime.MinValue;
    }

    /// <summary>
    /// Converts the input object to a nullable DateTime. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable DateTime representation of the input object, or null if the conversion fails.</returns>
    public static DateTime? ToDateTimeOrNull(this object? input)
    {
        if (input == null) return null;
        if (!DateTime.TryParse(input.ToString(), out DateTime result))
        {
            return null;
        }
        return result;
    }

    /// <summary>
    /// Converts the input object to a boolean. Returns the specified default value if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>A boolean representation of the input object, or the specified default value if the conversion fails.</returns>
    public static bool ToBool(this object? input, bool defaultValue = false)
    {
        return ToBoolOrNull(input) ?? defaultValue;
    }

    /// <summary>
    /// Converts the input object to a nullable boolean. Returns null if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A nullable boolean representation of the input object, or null if the conversion fails.</returns>
    public static bool? ToBoolOrNull(this object? input)
    {
        return input.ToStringOrNull().ToBoolOrNull();
    }

    /// <summary>
    /// Converts the input object to a Guid. Returns Guid.Empty if the conversion fails.
    /// </summary>
    /// <param name="input">The input object.</param>
    /// <returns>A Guid representation of the input object, or Guid.Empty if the conversion fails.</returns>
    public static Guid ToGuid(this object? input)
    {
        return ToGuidOrNull(input) ?? Guid.Empty;
    }

    /// <summary>
    /// Converts the input object to a nullable Guid. Returns null if the conversion fails.
    /// </summary>
    /// <param name="value">The input object.</param>
    /// <returns>A nullable Guid representation of the input object, or null if the conversion fails.</returns>
    public static Guid? ToGuidOrNull(this object? value)
    {
        return value.ToStringOrNull().ToGuidOrNull();
    }
}