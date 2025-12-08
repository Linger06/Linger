#pragma warning disable
#if !NET9_0_OR_GREATER

namespace System;

/// <summary>
/// Provides ToHexStringLower polyfill for .NET versions before 9.0.
/// </summary>
public static class ConvertPolyfills
{
    extension(Convert)
    {
        /// <summary>
        /// Converts an array of bytes to its equivalent string representation encoded with lowercase hex characters.
        /// </summary>
        /// <param name="inArray">An array of bytes.</param>
        /// <returns>The string representation in lowercase hex of the elements in <paramref name="inArray"/>.</returns>
        public static string ToHexStringLower(byte[] inArray)
        {
            if (inArray is null)
            {
                throw new ArgumentNullException(nameof(inArray));
            }

#if NET5_0_OR_GREATER
            return Convert.ToHexString(inArray).ToLowerInvariant();
#else
            return BitConverter.ToString(inArray).Replace("-", "").ToLowerInvariant();
#endif
        }
    }
}
#endif
