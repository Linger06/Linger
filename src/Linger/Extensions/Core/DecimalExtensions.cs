namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="decimal"/> extensions
/// </summary>
public static class DecimalExtensions
{
    /// <summary>
    /// Determines whether the value is an integer.
    /// </summary>
    /// <param name="value">The <see cref="decimal"/> to check.</param>
    /// <returns><c>true</c> if the value is an integer; otherwise, <c>false</c>.</returns>
    public static bool IsInteger(this decimal value)
    {
        return value == Math.Truncate(value);
    }

    /// <summary>
    /// Converts the value to an integer if it is an integer.
    /// </summary>
    /// <param name="value">The <see cref="decimal"/> to convert.</param>
    /// <returns>The integer representation of the value.</returns>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to an integer.</exception>
    public static int ToInt(this decimal value)
    {
        if (value.IsInteger() && value is >= int.MinValue and <= int.MaxValue)
        {
            return (int)value;
        }

        throw new InvalidCastException($"The value cannot be converted to an integer. value={value.ToString(CultureInfo.InvariantCulture)}");
    }

    /// <summary>
    /// Converts the value to a nullable integer if it is an integer within range.
    /// </summary>
    /// <param name="value">The <see cref="decimal"/> to convert.</param>
    /// <returns>The integer representation of the value, or null if conversion fails.</returns>
    public static int? ToIntOrNull(this decimal value)
    {
        if (value.IsInteger() && value is >= int.MinValue and <= int.MaxValue)
        {
            return (int)value;
        }

        return null;
    }

    /// <summary>
    /// Converts the value to a nullable integer if it is an integer within range.
    /// </summary>
    /// <param name="value">The nullable <see cref="decimal"/> to convert.</param>
    /// <returns>The integer representation of the value, or null if conversion fails.</returns>
    public static int? ToIntOrNull(this decimal? value)
    {
        if (value is null)
        {
            return null;
        }

        return value.Value.ToIntOrNull();
    }

    /// <summary>
    /// Converts the value to an integer or returns the default value if conversion fails.
    /// </summary>
    /// <param name="value">The <see cref="decimal"/> to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The integer representation of the value, or the default value if conversion fails.</returns>
    public static int ToIntOrDefault(this decimal value, int defaultValue = 0)
    {
        return value.ToIntOrNull() ?? defaultValue;
    }

    /// <summary>
    /// Converts the nullable value to an integer or returns the default value if conversion fails.
    /// </summary>
    /// <param name="value">The nullable <see cref="decimal"/> to convert.</param>
    /// <param name="defaultValue">The default value to return if conversion fails.</param>
    /// <returns>The integer representation of the value, or the default value if conversion fails.</returns>
    public static int ToIntOrDefault(this decimal? value, int defaultValue = 0)
    {
        return value.ToIntOrNull() ?? defaultValue;
    }

    /// <summary>
    /// Tries to convert the value to an integer if it is an integer within range.
    /// </summary>
    /// <param name="value">The <see cref="decimal"/> to convert.</param>
    /// <param name="result">The converted integer value if successful, or 0 if failed.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    public static bool TryToInt(this decimal value, out int result)
    {
        var converted = value.ToIntOrNull();
        if (converted.HasValue)
        {
            result = converted.Value;
            return true;
        }

        result = 0;
        return false;
    }

    /// <summary>
    /// Tries to convert the nullable value to an integer if it is an integer within range.
    /// </summary>
    /// <param name="value">The nullable <see cref="decimal"/> to convert.</param>
    /// <param name="result">The converted integer value if successful, or 0 if failed.</param>
    /// <returns>true if the conversion succeeded; otherwise, false.</returns>
    public static bool TryToInt(this decimal? value, out int result)
    {
        var converted = value.ToIntOrNull();
        if (converted.HasValue)
        {
            result = converted.Value;
            return true;
        }

        result = 0;
        return false;
    }

    /// <summary>
    /// Rounds the value to the specified number of decimal places.
    /// </summary>
    /// <param name="value">The <see cref="decimal"/> to round.</param>
    /// <param name="n">The number of decimal places to round to.</param>
    /// <returns>The rounded value.</returns>
    public static decimal ToRounding(this decimal value, int n = 0)
    {
        return decimal.Round(value, n, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Removes trailing zeros from the decimal value.
    /// </summary>
    /// <param name="value">The <see cref="decimal"/> to process.</param>
    /// <returns>The value without trailing zeros.</returns>
    public static decimal DeleteZero(this decimal value)
    {
        var value2 = (double)value;
        return (decimal)value2;
    }

    /// <summary>
    /// Converts the nullable decimal value to a string without trailing zeros.
    /// </summary>
    /// <param name="value">The nullable <see cref="decimal"/> to convert.</param>
    /// <returns>A string representation of the value without trailing zeros.</returns>
    public static string? ToStringDeleteZero(this decimal? value)
    {
        return value?.DeleteZero().ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the decimal value to a string without trailing zeros.
    /// </summary>
    /// <param name="value">The <see cref="decimal"/> to convert.</param>
    /// <returns>A string representation of the value without trailing zeros.</returns>
    public static string ToStringDeleteZero(this decimal value)
    {
        return value.DeleteZero().ToString(CultureInfo.InvariantCulture);
    }
}