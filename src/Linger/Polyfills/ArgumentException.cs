#pragma warning disable
#if !NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Linger.Extensions.Core;

namespace System;

/// <summary>
/// Provides downlevel polyfills for static methods on ArgumentException.
/// Uses C# 14 extension members to extend the existing System.ArgumentException class.
/// </summary>
public static class ArgumentExceptionPolyfills
{
    extension(ArgumentException)
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null,
        /// or an <see cref="ArgumentException"/> if it's empty.
        /// </summary>
        /// <param name="argument">The string argument to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.IsEmpty())
            {
                throw new ArgumentException("The value cannot be an empty string.", paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null,
        /// or an <see cref="ArgumentException"/> if it's whitespace only.
        /// </summary>
        /// <param name="argument">The string argument to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (argument.IsWhiteSpace())
            {
                throw new ArgumentException("The value cannot be a white space string.", paramName);
            }
        }
    }
}
#endif
