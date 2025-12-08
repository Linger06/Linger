#pragma warning disable
// Only define polyfill for frameworks that don't have ThrowIfNull
#if NETFRAMEWORK || NETSTANDARD2_0 || NET5_0
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Provides downlevel polyfills for static methods on ArgumentNullException.
/// Uses C# 14 extension members to extend the existing System.ArgumentNullException class.
/// </summary>
public static class ArgumentNullExceptionPolyfills
{
    extension(ArgumentNullException)
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.
        /// </summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                ThrowArgumentNullException(paramName);
            }
        }
    }

    [DoesNotReturn]
    private static void ThrowArgumentNullException(string? paramName) =>
        throw new ArgumentNullException(paramName);
}
#endif
