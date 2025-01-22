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
        return (int)value == value;
    }

    /// <summary>
    /// Converts the value to an integer if it is an integer.
    /// </summary>
    /// <param name="value">The <see cref="decimal"/> to convert.</param>
    /// <returns>The integer representation of the value.</returns>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to an integer.</exception>
    public static int ToInt(this decimal value)
    {
        if (value.IsInteger())
        {
            return (int)value;
        }

        throw new InvalidCastException("The value cannot be converted to an integer.");
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
        return value?.DeleteZero().ToString(ExtensionMethodSetting.DefaultCulture);
    }

    /// <summary>
    /// Converts the decimal value to a string without trailing zeros.
    /// </summary>
    /// <param name="value">The <see cref="decimal"/> to convert.</param>
    /// <returns>A string representation of the value without trailing zeros.</returns>
    public static string ToStringDeleteZero(this decimal value)
    {
        return value.DeleteZero().ToString(ExtensionMethodSetting.DefaultCulture);
    }
}