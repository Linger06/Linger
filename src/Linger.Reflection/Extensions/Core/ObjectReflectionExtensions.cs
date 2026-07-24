using System.Reflection;
using Linger.Helper;

namespace Linger.Extensions.Core;

/// <summary>
/// Provides reflection-based extension methods for object property access.
/// </summary>
#if NET5_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Uses runtime reflection to inspect object properties. This API is not compatible with trimming.")]
#endif
public static class ObjectReflectionExtensions
{
    /// <summary>
    /// Gets a public instance property by name.
    /// </summary>
    /// <param name="obj">The object whose type is inspected.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The property metadata, or <see langword="null"/> when no matching property exists.</returns>
    public static PropertyInfo? GetPropertyInfo(this object obj, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(propertyName);

        var propertyMap = PropertyMetadataCache.GetPropertyMap(obj.GetType());

        return propertyMap.TryGetValue(propertyName, out var property) ? property : null;
    }

    /// <summary>
    /// Gets a public instance property value by name.
    /// </summary>
    /// <param name="obj">The object whose property is read.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The property value, or <see langword="null"/> when no matching property exists.</returns>
    public static object? GetPropertyValue(this object obj, string propertyName)
    {
        return obj.GetPropertyInfo(propertyName)?.GetValue(obj);
    }

    /// <summary>
    /// Executes an action for each readable, non-indexed public property of an object.
    /// </summary>
    /// <param name="value">The object whose properties are enumerated.</param>
    /// <param name="action">The action to execute with each property name and value.</param>
    /// <remarks>Exceptions thrown by a property getter or <paramref name="action"/> propagate to the caller.</remarks>
    public static void ForEachProperty(this object? value, Action<string, object?> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (value is null)
        {
            return;
        }

        foreach (var property in PropertyMetadataCache.GetProperties(value.GetType()))
        {
            if (property.GetMethod?.IsPublic != true || property.GetIndexParameters().Length != 0)
            {
                continue;
            }

            action(property.Name, property.GetValue(value));
        }
    }

}
