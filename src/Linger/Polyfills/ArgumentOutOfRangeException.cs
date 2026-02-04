#pragma warning disable
#if !NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Provides downlevel polyfills for static methods on ArgumentOutOfRangeException.
/// Uses C# 14 extension members to extend the existing System.ArgumentOutOfRangeException class.
/// </summary>
public static class ArgumentOutOfRangeExceptionPolyfills
{
    extension(ArgumentOutOfRangeException)
    {
        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
#if NET7_0
            where T : INumberBase<T>
#else
            where T : struct, IComparable<T>
#endif
        {
#if NET7_0
            if (T.IsNegative(value))
#else
            if (value.CompareTo(default) < 0)
#endif
            {
                ThrowNegative(value, paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
#if NET7_0
            where T : INumberBase<T>
#else
            where T : struct, IComparable<T>
#endif
        {
#if NET7_0
            if (T.IsZero(value))
#else
            if (value.CompareTo(default) == 0)
#endif
            {
                ThrowZero(value, paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative or zero.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfNegativeOrZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
#if NET7_0
            where T : INumberBase<T>
#else
            where T : struct, IComparable<T>
#endif
        {
#if NET7_0
            if (T.IsNegative(value) || T.IsZero(value))
#else
            if (value.CompareTo(default) <= 0)
#endif
            {
                ThrowNegativeOrZero(value, paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="other">The value to compare against.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfGreaterThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) > 0)
            {
                ThrowGreater(value, other, paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than or equal to <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="other">The value to compare against.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) >= 0)
            {
                ThrowGreaterEqual(value, other, paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="other">The value to compare against.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) < 0)
            {
                ThrowLess(value, other, paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal to <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="other">The value to compare against.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfLessThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) <= 0)
            {
                ThrowLessEqual(value, other, paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="other">The value to compare against.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IEquatable<T>?
        {
            if (EqualityComparer<T>.Default.Equals(value, other))
            {
                ThrowEqual(value, other, paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="other">The value to compare against.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfNotEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IEquatable<T>?
        {
            if (!EqualityComparer<T>.Default.Equals(value, other))
            {
                ThrowNotEqual(value, other, paramName);
            }
        }
    }

    [DoesNotReturn]
    private static void ThrowNegative<T>(T value, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be a non-negative value.");

    [DoesNotReturn]
    private static void ThrowZero<T>(T value, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be a non-zero value.");

    [DoesNotReturn]
    private static void ThrowNegativeOrZero<T>(T value, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be a non-negative and non-zero value.");

    [DoesNotReturn]
    private static void ThrowGreater<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be less than or equal to '{other}'.");

    [DoesNotReturn]
    private static void ThrowGreaterEqual<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be less than '{other}'.");

    [DoesNotReturn]
    private static void ThrowLess<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be greater than or equal to '{other}'.");

    [DoesNotReturn]
    private static void ThrowLessEqual<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be greater than '{other}'.");

    [DoesNotReturn]
    private static void ThrowEqual<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must not be equal to '{other}'.");

    [DoesNotReturn]
    private static void ThrowNotEqual<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be equal to '{other}'.");
}
#endif
