using System.Runtime.CompilerServices;
using System.Text;

namespace Linger.Extensions.Core;

/// <summary>
/// High-performance string extensions using modern C# features
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Creates a substring using Span for better performance (available in .NET Core and .NET 5+)
    /// </summary>
    /// <param name="value">The input string</param>
    /// <param name="startIndex">The starting index</param>
    /// <param name="length">The length of the substring</param>
    /// <returns>A new string containing the substring</returns>
#if NET5_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FastSubstring(this string value, int startIndex, int length)
    {
        ArgumentNullException.ThrowIfNull(value);
        
        if (startIndex < 0 || startIndex >= value.Length)
            throw new ArgumentOutOfRangeException(nameof(startIndex));
            
        if (length < 0 || startIndex + length > value.Length)
            throw new ArgumentOutOfRangeException(nameof(length));
            
        return value.AsSpan(startIndex, length).ToString();
    }
#endif

    /// <summary>
    /// Efficiently builds a string using StringBuilder with capacity optimization
    /// </summary>
    /// <param name="parts">String parts to concatenate</param>
    /// <returns>Concatenated string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FastConcat(params ReadOnlySpan<string> parts)
    {
        if (parts.IsEmpty)
            return string.Empty;

        if (parts.Length == 1)
            return parts[0] ?? string.Empty;

        // Calculate total capacity to avoid StringBuilder reallocations
        var totalLength = 0;
        foreach (var part in parts)
        {
            if (part is not null)
                totalLength += part.Length;
        }

        if (totalLength == 0)
            return string.Empty;

        var sb = new StringBuilder(totalLength);
        foreach (var part in parts)
        {
            if (part is not null)
                sb.Append(part);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Checks if string contains any of the specified characters using Span for better performance
    /// </summary>
    /// <param name="value">The string to check</param>
    /// <param name="chars">Characters to search for</param>
    /// <returns>True if any character is found</returns>
#if NET5_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny(this string value, ReadOnlySpan<char> chars)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.AsSpan().IndexOfAny(chars) >= 0;
    }
#endif

    /// <summary>
    /// Efficient string trimming that avoids allocation when no trimming is needed
    /// </summary>
    /// <param name="value">The string to trim</param>
    /// <returns>Trimmed string</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FastTrim(this string value)
    {
        if (value.IsNullOrEmpty())
            return value;

#if NET5_0_OR_GREATER
        var span = value.AsSpan().Trim();
        return span.Length == value.Length ? value : span.ToString();
#else
        var trimmed = value.Trim();
        return ReferenceEquals(trimmed, value) ? value : trimmed;
#endif
    }
}
