using System.Diagnostics.CodeAnalysis;

namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="object"/> extensions
/// </summary>
public static partial class ObjectExtensions
{
    /// <summary>
    /// Indicates whether the specified <see cref="object"/> is not null.
    /// </summary>
    /// <param name="value">The specified <see cref="object"/>.</param>
    /// <returns>true if the object is not null; otherwise, false.</returns>
    public static bool IsNotNull([NotNullWhen(true)] this object? value) => value is not null;

    /// <summary>
    /// Indicates whether the specified <see cref="object"/> is null.
    /// </summary>
    /// <param name="value">The specified <see cref="object"/>.</param>
    /// <returns>true if the object is null; otherwise, false.</returns>
    public static bool IsNull([NotNullWhen(false)] this object? value) => value is null;

    /// <summary>
    /// Indicates whether the specified <see cref="object"/> is not null and not an empty string.
    /// </summary>
    /// <param name="value">The specified <see cref="object"/>.</param>
    /// <returns>true if the object is not null and not an empty string; otherwise, false.</returns>
    public static bool IsNotNullAndEmpty([NotNullWhen(true)] this object? value) => value is not null && !string.IsNullOrEmpty(value.ToString());

    /// <summary>
    /// Indicates whether the specified <see cref="object"/> is null or an empty string.
    /// </summary>
    /// <param name="value">The specified <see cref="object"/>.</param>
    /// <returns>true if the object is null or an empty string; otherwise, false.</returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this object? value) => value is null || string.IsNullOrEmpty(value.ToString());

    /// <summary>
    /// Indicates whether the specified <see cref="object"/> is null or <see cref="DBNull"/>.
    /// </summary>
    /// <param name="value">The specified <see cref="object"/>.</param>
    /// <returns>true if the object is null or DBNull; otherwise, false.</returns>
    public static bool IsNullOrDbNull([NotNullWhen(false)] this object? value) => value is DBNull or null;
}