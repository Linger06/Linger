#pragma warning disable
#if !NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Linger.Extensions.Core;

namespace Linger;

/// <summary>
/// Polyfill for ArgumentException validation methods for older .NET frameworks.
/// 
/// Framework Support:
/// - .NET Framework/Standard 2.0/NET 5-7: This polyfill implementation
/// - .NET 8+: Use built-in System.ArgumentException methods
/// </summary>
/// <remarks>
/// This is a pure utility class providing static validation methods:
/// - Always throws standard System.ArgumentException/System.ArgumentNullException
/// - Cannot be instantiated (prevents confusion about exception types)
/// - API identical to System.ArgumentException in .NET 8+
/// 
/// Usage: ArgumentException.ThrowIfNullOrEmpty(value)
/// For throwing: throw new System.ArgumentException(message, paramName)
/// </remarks>
public static class ArgumentException
{
    /// <summary>
    /// Throws an ArgumentNullException if the string argument is null, 
    /// or ArgumentException if it's empty.
    /// This is a polyfill for .NET 8+'s ArgumentException.ThrowIfNullOrEmpty method.
    /// </summary>
    public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument is null)
        {
            throw new System.ArgumentNullException(paramName);
        }

        if (argument.IsEmpty())
        {
            throw new System.ArgumentException("The value cannot be an empty string.", paramName);
        }
    }

    /// <summary>
    /// Throws an ArgumentNullException if the string argument is null,
    /// or ArgumentException if it's whitespace only.
    /// This is a polyfill for .NET 8+'s ArgumentException.ThrowIfNullOrWhiteSpace method.
    /// </summary>
    public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument is null)
        {
            throw new System.ArgumentNullException(paramName);
        }

        if (argument.IsWhiteSpace())
        {
            throw new System.ArgumentException("The value cannot be a white space string.", paramName);
        }
    }
}
#endif
