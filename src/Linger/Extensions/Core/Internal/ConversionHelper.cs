namespace Linger.Extensions.Core.Internal;

/// <summary>
/// Internal helper for string conversion operations.
///
/// DEPRECATED: All conversion methods in StringExtensions.Conversion.cs
/// have been refactored to direct implementations following the "Four Methods"
/// pattern (ToTarget, ToTargetOrNull, ToTargetOrDefault, TryToTarget)
/// and the "Funnel Model" architecture. This helper class is kept for
/// backward compatibility but is no longer used internally for main conversions.
/// However, IsType<T> is still used by StringExtensions.Numeric.cs for type checking.
/// </summary>
internal static class ConversionHelper
{
    /// <summary>
    /// Generic type checking helper used by StringExtensions.Numeric.cs IsXxx methods.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <param name="value">The string value to check.</param>
    /// <param name="tryParseDelegate">The TryParse delegate for the target type.</param>
    /// <returns>True if the value can be parsed as type T; otherwise false.</returns>
    internal static bool IsType<T>(this string? value, ConversionHelper.TryParseDelegate<T> tryParseDelegate) where T : struct
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return tryParseDelegate(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _);
    }

    /// <summary>
    /// Delegate for TryParse methods with NumberStyles and CultureInfo parameters.
    /// </summary>
    /// <typeparam name="T">The type being parsed.</typeparam>
    internal delegate bool TryParseDelegate<T>(string? value, System.Globalization.NumberStyles styles, System.Globalization.CultureInfo? culture, out T result) where T : struct;
}
