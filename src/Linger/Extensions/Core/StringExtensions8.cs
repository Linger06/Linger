using System.Diagnostics.CodeAnalysis;

namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="string"/> extensions.
/// </summary>
public static partial class StringExtensions
{
    #region int

    /// <summary>
    /// Tries to convert the string to an integer.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting integer if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToInt(this string? value, [NotNullWhen(true)] out int? result)
    {
        if (value.IsNullOrWhiteSpace() || !int.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to an integer or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted integer or the default value.</returns>
    public static int? ToIntOrNull(this string? value, int? defaultValue = null)
        => value.ToIntOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to an integer or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted integer or the result of the default value function.</returns>
    public static int? ToIntOrNull(this string? value, Func<int?>? defaultValueFunc)
        => value.TryToInt(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to an integer or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted integer or the default value.</returns>
    public static int ToInt(this string? value, int defaultValue = 0)
        => value.ToInt(() => defaultValue);

    /// <summary>
    /// Converts the string to an integer or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted integer or the result of the default value function.</returns>
    public static int ToInt(this string? value, Func<int>? defaultValueFunc)
        => value.TryToInt(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? 0;

    #endregion

    #region long

    /// <summary>
    /// Tries to convert the string to a long integer.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting long integer if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToLong(this string? value, [NotNullWhen(true)] out long? result)
    {
        if (value.IsNullOrWhiteSpace() || !long.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a long integer or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted long integer or the default value.</returns>
    public static long? ToLongOrNull(this string? value, long? defaultValue = null)
        => value.ToLongOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to a long integer or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted long integer or the result of the default value function.</returns>
    public static long? ToLongOrNull(this string? value, Func<long?>? defaultValueFunc)
        => value.TryToLong(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to a long integer or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted long integer or the default value.</returns>
    public static long ToLong(this string? value, long defaultValue = 0)
        => value.ToLong(() => defaultValue);

    /// <summary>
    /// Converts the string to a long integer or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted long integer or the result of the default value function.</returns>
    public static long ToLong(this string? value, Func<long>? defaultValueFunc)
        => value.TryToLong(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? 0;

    #endregion

    #region decimal

    /// <summary>
    /// Tries to convert the string to a decimal.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting decimal if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToDecimal(this string? value, [NotNullWhen(true)] out decimal? result)
    {
        if (value.IsNullOrWhiteSpace() || !decimal.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a decimal or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted decimal or the default value.</returns>
    public static decimal? ToDecimalOrNull(this string? value, decimal? defaultValue = null, int? digits = null)
        => value.ToDecimalOrNull(() => defaultValue, digits);

    /// <summary>
    /// Converts the string to a decimal or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted decimal or the result of the default value function.</returns>
    public static decimal? ToDecimalOrNull(this string? value, Func<decimal?>? defaultValueFunc, int? digits = null)
    {
        if (value.TryToDecimal(out var result))
        {
            if (digits == null)
            {
                return result.Value;
            }
            return Math.Round(result.Value, digits.Value);
        }

        return defaultValueFunc?.Invoke();
    }

    /// <summary>
    /// Converts the string to a decimal or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted decimal or the default value.</returns>
    public static decimal ToDecimal(this string? value, decimal defaultValue = 0, int? digits = null)
        => value.ToDecimal(() => defaultValue, digits);

    /// <summary>
    /// Converts the string to a decimal or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted decimal or the result of the default value function.</returns>
    public static decimal ToDecimal(this string? value, Func<decimal>? defaultValueFunc, int? digits = null)
    {
        if (value.TryToDecimal(out var result))
        {
            if (digits == null)
            {
                return result.Value;
            }
            return Math.Round(result.Value, digits.Value);
        }

        return defaultValueFunc?.Invoke() ?? 0;
    }

    #endregion

    #region float

    /// <summary>
    /// Tries to convert the string to a float.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting float if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToFloat(this string? value, [NotNullWhen(true)] out float? result)
    {
        if (value.IsNullOrWhiteSpace() || !float.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a float or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted float or the default value.</returns>
    public static float? ToFloatOrNull(this string? value, float? defaultValue = null, int? digits = null)
        => value.ToFloatOrNull(() => defaultValue, digits);

    /// <summary>
    /// Converts the string to a float or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted float or the result of the default value function.</returns>
    public static float? ToFloatOrNull(this string? value, Func<float?>? defaultValueFunc, int? digits = null)
    {
        if (value.TryToFloat(out var result))
        {
            if (digits == null)
            {
                return result.Value;
            }
            return (float)Math.Round(result.Value, digits.Value);
        }

        return defaultValueFunc?.Invoke();
    }

    /// <summary>
    /// Converts the string to a float or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted float or the default value.</returns>
    public static float ToFloat(this string? value, float defaultValue = 0, int? digits = null)
        => value.ToFloat(() => defaultValue, digits);

    /// <summary>
    /// Converts the string to a float or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted float or the result of the default value function.</returns>
    public static float ToFloat(this string? value, Func<float>? defaultValueFunc, int? digits = null)
    {
        if (value.TryToFloat(out var result))
        {
            if (digits == null)
            {
                return result.Value;
            }
            return (float)Math.Round(result.Value, digits.Value);
        }

        return defaultValueFunc?.Invoke() ?? 0;
    }

    #endregion

    #region double

    /// <summary>
    /// Tries to convert the string to a double.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting double if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToDouble(this string? value,
        [NotNullWhen(true)]
        out double? result)
    {
        if (value.IsNullOrWhiteSpace() || !double.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a double or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted double or the default value.</returns>
    public static double? ToDoubleOrNull(this string? value, double? defaultValue = null, int? digits = null)
        => value.ToDoubleOrNull(() => defaultValue, digits);

    /// <summary>
    /// Converts the string to a double or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted double or the result of the default value function.</returns>
    public static double? ToDoubleOrNull(this string? value, Func<double?>? defaultValueFunc, int? digits = null)
    {
        if (value.TryToDouble(out var result))
        {
            if (digits == null)
            {
                return result.Value;
            }
            return Math.Round(result.Value, digits.Value);
        }

        return defaultValueFunc?.Invoke();
    }

    /// <summary>
    /// Converts the string to a double or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted double or the default value.</returns>
    public static double ToDouble(this string? value, double defaultValue, int? digits = null)
        => value.ToDouble(() => defaultValue, digits);

    /// <summary>
    /// Converts the string to a double or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The converted double or the result of the default value function.</returns>
    public static double ToDouble(this string? value, Func<double>? defaultValueFunc, int? digits = null)
    {
        if (value.TryToDouble(out var result))
        {
            if (digits == null)
            {
                return result.Value;
            }
            return Math.Round(result.Value, digits.Value);
        }

        return defaultValueFunc?.Invoke() ?? 0;
    }

    #endregion

    #region datetime

    /// <summary>
    /// Tries to convert the string to a DateTime.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting DateTime if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToDateTime(this string? value, [NotNullWhen(true)] out DateTime? result)
    {
        if (value.IsNullOrWhiteSpace() || !DateTime.TryParse(value, out var valResult))
        {
            result = null;
            return false;
        }
        result = valResult;
        return true;
    }

    /// <summary>
    /// Converts the string to a DateTime or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted DateTime or the default value.</returns>
    public static DateTime? ToDateTimeOrNull(this string? value, DateTime? defaultValue = null)
        => value.ToDateTimeOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to a DateTime or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted DateTime or the result of the default value function.</returns>
    public static DateTime? ToDateTimeOrNull(this string? value, Func<DateTime?>? defaultValueFunc)
        => value.TryToDateTime(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to a DateTime or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The converted DateTime or the default value.</returns>
    public static DateTime ToDateTime(this string? value)
        => value.ToDateTime(() => new DateTime());

    /// <summary>
    /// Converts the string to a DateTime or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted DateTime or the default value.</returns>
    public static DateTime ToDateTime(this string? value, DateTime defaultValue)
        => value.ToDateTime(() => defaultValue);

    /// <summary>
    /// Converts the string to a DateTime or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted DateTime or the result of the default value function.</returns>
    public static DateTime ToDateTime(this string? value, Func<DateTime>? defaultValueFunc)
        => value.TryToDateTime(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? new DateTime();

    #endregion

    #region bool

    private static readonly Dictionary<string, bool> s_boolMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "0", false },
        { "false", false },
        { "no", false },
        { "fail", false },
        { "lose", false },
        { "true", true },
        { "1", true },
        { "ok", true },
        { "yes", true },
        { "success", true }
    };

    /// <summary>
    /// Tries to convert the string to a boolean.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="result">The resulting boolean if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, otherwise false.</returns>
    public static bool TryToBool(this string? value, [NotNullWhen(true)] out bool? result)
    {
        if (!value.IsNullOrWhiteSpace() &&
            (s_boolMap.TryGetValue(value, out var valResult) || bool.TryParse(value, out valResult)))
        {
            result = valResult;
            return true;
        }
        result = null;
        return false;
    }

    /// <summary>
    /// Converts the string to a boolean or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted boolean or the default value.</returns>
    public static bool? ToBoolOrNull(this string? value, bool? defaultValue = null)
        => value.ToBoolOrNull(() => defaultValue);

    /// <summary>
    /// Converts the string to a boolean or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted boolean or the result of the default value function.</returns>
    public static bool? ToBoolOrNull(this string? value, Func<bool?>? defaultValueFunc)
        => value.TryToBool(out var result) ? result.Value : defaultValueFunc?.Invoke();

    /// <summary>
    /// Converts the string to a boolean or returns the default value if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValue">The default value to return if the conversion fails.</param>
    /// <returns>The converted boolean or the default value.</returns>
    public static bool ToBool(this string? value, bool defaultValue = false)
        => value.ToBool(() => defaultValue);

    /// <summary>
    /// Converts the string to a boolean or returns the result of the default value function if the conversion fails.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <param name="defaultValueFunc">The function to provide the default value if the conversion fails.</param>
    /// <returns>The converted boolean or the result of the default value function.</returns>
    public static bool ToBool(this string? value, Func<bool>? defaultValueFunc)
        => value.TryToBool(out var result) ? result.Value : defaultValueFunc?.Invoke() ?? false;

    #endregion
}
