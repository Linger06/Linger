using System.Collections.Concurrent;
using System.ComponentModel;
#if NET5_0_OR_GREATER
using System.ComponentModel.DataAnnotations;
#endif
using System.Reflection;

namespace Linger.Extensions.Core;

/// <summary>
/// Provides reflection-based attribute access for enum values.
/// </summary>
#if NET5_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Uses runtime reflection to inspect enum member attributes. This API is not compatible with trimming.")]
#endif
public static class EnumReflectionExtensions
{
    private static readonly ConcurrentDictionary<Enum, string> s_descriptionCache = new();

#if NET5_0_OR_GREATER
    private static readonly ConcurrentDictionary<Enum, string> s_displayCache = new();
#endif

    /// <summary>
    /// Gets the <see cref="DescriptionAttribute"/> value for an enum member.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <returns>The description, or the enum member name when the attribute is absent.</returns>
    public static string GetDescription(this Enum value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return s_descriptionCache.GetOrAdd(value, GetDescriptionInternal);
    }

#if NET5_0_OR_GREATER
    /// <summary>
    /// Gets the <see cref="DisplayAttribute.Name"/> value for an enum member.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <returns>The display name, or the enum member name when the attribute is absent.</returns>
    public static string GetDisplay(this Enum value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return s_displayCache.GetOrAdd(value, GetDisplayInternal);
    }
#endif

    private static string GetDescriptionInternal(Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        DescriptionAttribute? attribute = field?.GetCustomAttribute<DescriptionAttribute>();

        return attribute?.Description ?? value.ToString();
    }

#if NET5_0_OR_GREATER
    private static string GetDisplayInternal(Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        DisplayAttribute? attribute = field?.GetCustomAttribute<DisplayAttribute>();

        return attribute?.Name ?? value.ToString();
    }
#endif
}
