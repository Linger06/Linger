namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for the <see cref="short"/> data type.
/// </summary>
public static class ShortExtensions
{
    /// <summary>
    /// Converts the value to a string representation with thousand separators.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A string representation of the value with thousand separators.</returns>
    public static string ToThousand(this short value)
    {
        return $"{value:N}";
    }
}