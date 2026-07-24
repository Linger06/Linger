namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="decimal"/> extensions for conversion and decimal-specific formatting helpers.
/// </summary>
public static class DecimalExtensions
{
    #region decimal -> int (Int32)

    /// <summary>
    /// Tries to convert a decimal to <see cref="int"/>.
    /// Conversion succeeds only when the value is a mathematical integer and within range.
    /// </summary>
    public static bool TryToInt(this decimal value, out int result)
    {
        result = 0;
        if (!IsWholeNumberInRange(value, int.MinValue, int.MaxValue))
            return false;

        result = (int)value;
        return true;
    }

    /// <summary>
    /// Converts a decimal to <see cref="int"/> using strict semantics.
    /// </summary>
    public static int ToInt(this decimal value)
    {
        if (value.TryToInt(out var result)) return result;

        if (!HasNoFractionalPart(value))
        {
            throw new InvalidCastException(
                $"The value cannot be converted to Int32 because it contains a fractional part. value={FormatInvariant(value)}");
        }

        throw new OverflowException(
            $"The value is outside the range of Int32. value={FormatInvariant(value)}");
    }

    public static int? ToIntOrNull(this decimal value) => value.TryToInt(out var r) ? r : null;

    public static int ToIntOrDefault(this decimal value, int defaultValue = 0) =>
        value.TryToInt(out var r) ? r : defaultValue;

    #endregion

    #region decimal -> long (Int64)

    /// <summary>
    /// Tries to convert a decimal to <see cref="long"/>.
    /// Conversion succeeds only when the value is a mathematical integer and within range.
    /// </summary>
    public static bool TryToLong(this decimal value, out long result)
    {
        result = 0L;
        if (!IsWholeNumberInRange(value, long.MinValue, long.MaxValue))
            return false;

        result = (long)value;
        return true;
    }

    /// <summary>
    /// Converts a decimal to <see cref="long"/> using strict semantics.
    /// </summary>
    public static long ToLong(this decimal value)
    {
        if (value.TryToLong(out var result)) return result;

        if (!HasNoFractionalPart(value))
        {
            throw new InvalidCastException(
                $"The value cannot be converted to Int64 because it contains a fractional part. value={FormatInvariant(value)}");
        }

        throw new OverflowException(
            $"The value is outside the range of Int64. value={FormatInvariant(value)}");
    }

    public static long? ToLongOrNull(this decimal value) => value.TryToLong(out var r) ? r : null;

    public static long ToLongOrDefault(this decimal value, long defaultValue = 0L) =>
        value.TryToLong(out var r) ? r : defaultValue;

    #endregion

    #region decimal -> short (Int16)

    /// <summary>
    /// Tries to convert a decimal to <see cref="short"/>.
    /// Conversion succeeds only when the value is a mathematical integer and within range.
    /// </summary>
    public static bool TryToShort(this decimal value, out short result)
    {
        result = 0;
        if (!IsWholeNumberInRange(value, short.MinValue, short.MaxValue))
            return false;

        result = (short)value;
        return true;
    }

    /// <summary>
    /// Converts a decimal to <see cref="short"/> using strict semantics.
    /// </summary>
    public static short ToShort(this decimal value)
    {
        if (value.TryToShort(out var result)) return result;

        if (!HasNoFractionalPart(value))
        {
            throw new InvalidCastException(
                $"The value cannot be converted to Int16 because it contains a fractional part. value={FormatInvariant(value)}");
        }

        throw new OverflowException(
            $"The value is outside the range of Int16. value={FormatInvariant(value)}");
    }

    public static short? ToShortOrNull(this decimal value) => value.TryToShort(out var r) ? r : null;

    public static short ToShortOrDefault(this decimal value, short defaultValue = 0) =>
        value.TryToShort(out var r) ? r : defaultValue;

    #endregion

    #region Decimal rounding

    /// <summary>
    /// Rounds a decimal value to the specified number of fractional digits using conventional rounding.
    /// </summary>
    /// <param name="value">The decimal value to round.</param>
    /// <param name="decimals">The number of fractional digits to retain, from 0 through 28.</param>
    /// <returns>The rounded decimal value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="decimals"/> is outside the supported range.</exception>
    /// <example>
    /// <code>
    /// var rounded = 12.345m.Round(2); // 12.35m
    /// </code>
    /// </example>
    public static decimal Round(this decimal value, int decimals)
    {
        return decimal.Round(value, decimals, MidpointRounding.AwayFromZero);
    }

    #endregion

    #region Decimal formatting helpers

    /// <summary>
    /// Returns <see langword="true"/> when the decimal has no fractional part.
    /// </summary>
    public static bool IsInteger(this decimal value)
    {
        return HasNoFractionalPart(value);
    }

    /// <summary>
    /// Removes insignificant trailing zeros from the decimal representation.
    /// Kept as a compatibility helper; this is not part of the core conversion matrix.
    /// </summary>
    public static decimal DeleteZero(this decimal value)
    {
        return value / 1.0000000000000000000000000000m;
    }

    /// <summary>
    /// Formats a decimal without insignificant trailing zeros using invariant culture.
    /// Kept as a compatibility helper; this is not part of the core conversion matrix.
    /// </summary>
    public static string ToStringDeleteZero(this decimal value)
    {
        return value.DeleteZero().ToString("0.############################", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a nullable decimal without insignificant trailing zeros using invariant culture.
    /// </summary>
    public static string? ToStringDeleteZero(this decimal? value)
    {
        return value?.ToStringDeleteZero();
    }

    #endregion

    private static bool HasNoFractionalPart(decimal value)
    {
        return value % 1 == 0;
    }

    private static bool IsWholeNumberInRange(decimal value, decimal minValue, decimal maxValue)
    {
        return HasNoFractionalPart(value) && value >= minValue && value <= maxValue;
    }

    private static string FormatInvariant(decimal value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
