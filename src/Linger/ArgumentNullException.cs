#pragma warning disable
#if NETFRAMEWORK || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Linger.Extensions.Core;
namespace Linger;

/// <summary>
/// Enhanced ArgumentNullException with additional validation methods.
/// This extends the system ArgumentNullException to provide backward compatibility
/// for older .NET versions that don't have ThrowIfNull methods.
/// </summary>
public class ArgumentNullException : System.ArgumentNullException
{
    public ArgumentNullException(string paramName) : base(paramName) { }

    public ArgumentNullException(string? paramName, string? message) : base(paramName, message) { }

#if NETFRAMEWORK || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        if (argument == null)
            Throw(paramName);
    }

    [DoesNotReturn]
    static void Throw(string? paramName) =>
        throw new System.ArgumentNullException(paramName);
#endif

    public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        ThrowIfNull(argument, paramName);
        if (argument.IsEmpty())
            throw new System.ArgumentException("The value cannot be an empty string.", paramName);
    }

    public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression("argument")] string? paramName = null)
    {
        ThrowIfNull(argument, paramName);
        if (argument.IsWhiteSpace())
            throw new System.ArgumentException("The value cannot be a white space", paramName);
    }
}
#endif

