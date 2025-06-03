using System.Diagnostics.CodeAnalysis;

namespace Linger.Extensions.Core.Internal;

/// <summary>
/// Internal helper for string conversion operations
/// </summary>
internal static class ConversionHelper
{
    /// <summary>
    /// Generic method for trying to convert string to a value type
    /// </summary>
    /// <typeparam name="T">The target value type</typeparam>
    /// <param name="value">The string to convert</param>
    /// <param name="tryParse">The TryParse function for the target type</param>
    /// <param name="result">The conversion result</param>
    /// <returns>True if conversion succeeds, false otherwise</returns>
    public static bool TryConvert<T>(
        this string? value,
        TryParseDelegate<T> tryParse,
        [NotNullWhen(true)] out T? result)
        where T : struct
    {
        if (value.IsNullOrWhiteSpace())
        {
            result = null;
            return false;
        }

#if NET6_0_OR_GREATER
        if (tryParse(value.AsSpan(), out var parseResult))
        {
            result = parseResult;
            return true;
        }
#else
        if (tryParse(value, out var parseResult))
        {
            result = parseResult;
            return true;
        }
#endif

        result = null;
        return false;
    }

    /// <summary>
    /// Generic method for converting string to nullable value type
    /// </summary>
    /// <typeparam name="T">The target value type</typeparam>
    /// <param name="value">The string to convert</param>
    /// <param name="tryParse">The TryParse function for the target type</param>
    /// <param name="defaultValue">The default value if conversion fails</param>
    /// <returns>The converted value or default value</returns>
    public static T? ToNullable<T>(
        this string? value,
        TryParseDelegate<T> tryParse,
        T? defaultValue = null)
        where T : struct
    {
        return value.TryConvert(tryParse, out var result) ? result.Value : defaultValue;
    }

    /// <summary>
    /// Generic method for converting string to nullable value type with function provider
    /// </summary>
    /// <typeparam name="T">The target value type</typeparam>
    /// <param name="value">The string to convert</param>
    /// <param name="tryParse">The TryParse function for the target type</param>
    /// <param name="defaultValueFunc">Function to provide default value if conversion fails</param>
    /// <returns>The converted value or result of default function</returns>
    public static T? ToNullable<T>(
        this string? value,
        TryParseDelegate<T> tryParse,
        Func<T?>? defaultValueFunc)
        where T : struct
    {
        return value.TryConvert(tryParse, out var result) ? result.Value : defaultValueFunc?.Invoke();
    }

    /// <summary>
    /// Generic method for converting string to value type
    /// </summary>
    /// <typeparam name="T">The target value type</typeparam>
    /// <param name="value">The string to convert</param>
    /// <param name="tryParse">The TryParse function for the target type</param>
    /// <param name="defaultValue">The default value if conversion fails</param>
    /// <returns>The converted value or default value</returns>
    public static T ToValue<T>(
        this string? value,
        TryParseDelegate<T> tryParse,
        T defaultValue = default)
        where T : struct
    {
        return value.TryConvert(tryParse, out var result) ? result.Value : defaultValue;
    }

    /// <summary>
    /// Generic method for converting string to value type with function provider
    /// </summary>
    /// <typeparam name="T">The target value type</typeparam>
    /// <param name="value">The string to convert</param>
    /// <param name="tryParse">The TryParse function for the target type</param>
    /// <param name="defaultValueFunc">Function to provide default value if conversion fails</param>
    /// <returns>The converted value or result of default function</returns>
    public static T ToValue<T>(
        this string? value,
        TryParseDelegate<T> tryParse,
        Func<T>? defaultValueFunc)
        where T : struct
    {
        return value.TryConvert(tryParse, out var result) ? result.Value : defaultValueFunc?.Invoke() ?? default;
    }

    /// <summary>
    /// Generic method for checking if string can be converted to a specific type
    /// </summary>
    /// <typeparam name="T">The target value type</typeparam>
    /// <param name="value">The string to check</param>
    /// <param name="tryParse">The TryParse function for the target type</param>
    /// <returns>True if the string can be converted to the target type, false otherwise</returns>
    public static bool IsType<T>(
        this string? value,
        TryParseDelegate<T> tryParse)
        where T : struct
    {
        if (value is null) return false;

#if NET6_0_OR_GREATER
        return tryParse(value.AsSpan(), out _);
#else
        return tryParse(value, out _);
#endif
    }

    // Delegate definitions for different .NET versions
#if NET6_0_OR_GREATER
    /// <summary>
    /// Delegate for TryParse methods that support ReadOnlySpan (modern .NET)
    /// </summary>
    public delegate bool TryParseDelegate<T>(ReadOnlySpan<char> value, out T result) where T : struct;
#else
    /// <summary>
    /// Delegate for TryParse methods (legacy .NET Framework)
    /// </summary>
    public delegate bool TryParseDelegate<T>(string value, out T result) where T : struct;
#endif
}
