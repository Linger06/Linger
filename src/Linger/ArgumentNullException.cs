#pragma warning disable
// Only define custom ArgumentNullException for frameworks that don't have ThrowIfNull
#if NETFRAMEWORK || NETSTANDARD2_0 || NET5_0
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Linger;

/// <summary>
/// Polyfill ArgumentNullException with ThrowIfNull method for older .NET frameworks.
/// This provides backward compatibility for .NET Framework, .NET Standard 2.0, and .NET 5 
/// that don't have ThrowIfNull method. In .NET 6+, use the built-in System.ArgumentNullException.ThrowIfNull instead.
/// </summary>
/// <remarks>
/// This class only provides ThrowIfNull polyfill. For string validation methods
/// (ThrowIfNullOrEmpty/ThrowIfNullOrWhiteSpace), use Linger.ArgumentException class.
/// </remarks>
public static class ArgumentNullException
{
    /// <summary>
    /// Throws an ArgumentNullException if the argument is null.
    /// This is a polyfill for .NET 6+'s ArgumentNullException.ThrowIfNull method.
    /// </summary>
    /// <param name="argument">The argument to check.</param>
    /// <param name="paramName">The name of the parameter (automatically captured).</param>
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument == null)
            Throw(paramName);
    }

    [DoesNotReturn]
    static void Throw(string? paramName) =>
        throw new System.ArgumentNullException(paramName);
}
#endif

