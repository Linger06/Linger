#pragma warning disable
// Define custom ArgumentException for frameworks that need enhanced string validation methods
#if NETFRAMEWORK || NETSTANDARD2_0 || NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Linger.Extensions.Core;

namespace Linger;

/// <summary>
/// Enhanced ArgumentException with additional string validation methods.
/// 
/// Framework Support:
/// - .NET Framework/Standard 2.0/NET 5-7: Custom implementation for string validation
/// - .NET 8+: Delegates to built-in methods for optimal performance
/// </summary>
/// <remarks>
/// This class provides:
/// 1. ThrowIfNullOrEmpty/ThrowIfNullOrWhiteSpace for older frameworks  
/// 2. Consistent API across all .NET versions
/// 3. Optimal performance by using built-in methods when available (.NET 8+)
/// </remarks>
public static class ArgumentException
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Throws an ArgumentNullException if the string argument is null, 
    /// or ArgumentException if it's empty.
    /// Uses built-in .NET 8+ validation but throws Linger.ArgumentException for consistency.
    /// </summary>
    public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument is null)
        {
            throw new System.ArgumentNullException(paramName);
        }
        
        if (string.IsNullOrEmpty(argument))
        {
            throw new System.ArgumentException("The value cannot be an empty string.", paramName);
        }
    }

    /// <summary>
    /// Throws an ArgumentNullException if the string argument is null,
    /// or ArgumentException if it's whitespace only.
    /// Uses built-in .NET 8+ validation but throws Linger.ArgumentException for consistency.
    /// </summary>
    public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument is null)
        {
            throw new System.ArgumentNullException(paramName);
        }
        
        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new System.ArgumentException("The value cannot be an empty string or composed entirely of whitespace.", paramName);
        }
    }
#else
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
#endif
}
#endif
