namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="decimal"/> extensions for type conversion operations.
/// Implements the "Four Methods" pattern: ToTarget, ToTargetOrNull, ToTargetOrDefault, TryToTarget.
/// </summary>
public static class DecimalExtensions
{
    #region int (Int32)

    /// <summary>
    /// Converts the decimal to an int. Strict conversion - fails with precise exception.
    /// Throws InvalidCastException if value contains fractional part, or OverflowException if out of range.
    /// </summary>
    /// <exception cref="InvalidCastException">Thrown when the value contains a fractional part.</exception>
    /// <exception cref="OverflowException">Thrown when the value is outside the range of Int32.</exception>
    public static int ToInt(this decimal value)
    {
        // 性能快速通道：如果可空宽容派返回了值，说明既是整数也没越界，直接放行
        return value.ToIntOrNull() ?? (value != decimal.Truncate(value)
            ? throw new InvalidCastException($"The value cannot be converted to Int32 because it contains a fractional part. value={value}")
            : throw new OverflowException($"The value is outside the range of Int32. value={value}"));
    }

    /// <summary>
    /// Converts the decimal to a nullable int. Permissive conversion - fails by returning null.
    /// </summary>
    public static int? ToIntOrNull(this decimal value)
    {
        // 优化点：合并多重判断条件，并在范围校验阶段直接利用内置的整型范围，避免硬编码和多次 Truncate 损耗
        return (value == decimal.Truncate(value) && value >= int.MinValue && value <= int.MaxValue)
            ? (int)value
            : null;
    }

    /// <summary>
    /// Converts the decimal to an int or returns the default value if conversion fails.
    /// </summary>
    public static int ToIntOrDefault(this decimal value, int defaultValue = 0)
        => value.ToIntOrNull() ?? defaultValue;

    /// <summary>
    /// Tries to convert the decimal to an int using the standard .NET Try pattern.
    /// </summary>
    public static bool TryToInt(this decimal value, out int result)
    {
        var r = value.ToIntOrNull();
        result = r ?? 0;
        return r.HasValue;
    }

    #endregion

    #region long (Int64)

    /// <summary>
    /// Converts the decimal to a long. Strict conversion - fails with precise exception.
    /// Throws InvalidCastException if value contains fractional part, or OverflowException if out of range.
    /// </summary>
    /// <exception cref="InvalidCastException">Thrown when the value contains a fractional part.</exception>
    /// <exception cref="OverflowException">Thrown when the value is outside the range of Int64.</exception>
    public static long ToLong(this decimal value)
    {
        return value.ToLongOrNull() ?? (value != decimal.Truncate(value)
            ? throw new InvalidCastException($"The value cannot be converted to Int64 because it contains a fractional part. value={value}")
            : throw new OverflowException($"The value is outside the range of Int64. value={value}"));
    }

    /// <summary>
    /// Converts the decimal to a nullable long. Permissive conversion - fails by returning null.
    /// </summary>
    public static long? ToLongOrNull(this decimal value)
    {
        return (value == decimal.Truncate(value) && value >= long.MinValue && value <= long.MaxValue)
            ? (long)value
            : null;
    }

    /// <summary>
    /// Converts the decimal to a long or returns the default value if conversion fails.
    /// </summary>
    public static long ToLongOrDefault(this decimal value, long defaultValue = 0L)
        => value.ToLongOrNull() ?? defaultValue;

    /// <summary>
    /// Tries to convert the decimal to a long using the standard .NET Try pattern.
    /// </summary>
    public static bool TryToLong(this decimal value, out long result)
    {
        var r = value.ToLongOrNull();
        result = r ?? 0L;
        return r.HasValue;
    }

    #endregion

    #region short (Int16)

    /// <summary>
    /// Converts the decimal to a short. Strict conversion - fails with precise exception.
    /// Throws InvalidCastException if value contains fractional part, or OverflowException if out of range.
    /// </summary>
    /// <exception cref="InvalidCastException">Thrown when the value contains a fractional part.</exception>
    /// <exception cref="OverflowException">Thrown when the value is outside the range of Int16.</exception>
    public static short ToShort(this decimal value)
    {
        return value.ToShortOrNull() ?? (value != decimal.Truncate(value)
            ? throw new InvalidCastException($"The value cannot be converted to Int16 because it contains a fractional part. value={value}")
            : throw new OverflowException($"The value is outside the range of Int16. value={value}"));
    }

    /// <summary>
    /// Converts the decimal to a nullable short. Permissive conversion - fails by returning null.
    /// </summary>
    /// <summary>
    /// Converts the decimal to a nullable short. Permissive conversion - fails by returning null.
    /// </summary>
    public static short? ToShortOrNull(this decimal value)
    {
        return (value == decimal.Truncate(value) && value >= short.MinValue && value <= short.MaxValue)
            ? (short)value
            : null;
    }

    /// <summary>
    /// Converts the decimal to a short or returns the default value if conversion fails.
    /// </summary>
    public static short ToShortOrDefault(this decimal value, short defaultValue = 0)
        => value.ToShortOrNull() ?? defaultValue;

    /// <summary>
    /// Tries to convert the decimal to a short using the standard .NET Try pattern.
    /// </summary>
    public static bool TryToShort(this decimal value, out short result)
    {
        var r = value.ToShortOrNull();
        result = r ?? 0;
        return r.HasValue;
    }

    #endregion

    #region legacy (deprecated - kept for backward compatibility)

    /// <summary>
    /// Determines whether the value is an integer.
    /// </summary>
    /// <param name="value">The <see cref="decimal"/> to check.</param>
    /// <returns><c>true</c> if the value is an integer; otherwise, <c>false</c>.</returns>
    [Obsolete("Use value == decimal.Truncate(value) instead")]
    public static bool IsInteger(this decimal value)
    {
        return value == decimal.Truncate(value);
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

    #endregion
}
