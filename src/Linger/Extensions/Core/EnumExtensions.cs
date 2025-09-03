using System.Collections.Concurrent;
using System.ComponentModel;
#if NET5_0_OR_GREATER
using System.ComponentModel.DataAnnotations;
#endif
using System.Reflection;

namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for enum operations with performance optimizations.
/// </summary>
public static class EnumExtensions
{
    // Performance optimization: Cache enum descriptions to avoid repeated reflection calls
    private static readonly ConcurrentDictionary<Enum, string> s_descriptionCache = new();

#if NET5_0_OR_GREATER
    // Cache for Display attributes (NET5+ only)
    private static readonly ConcurrentDictionary<Enum, string> s_displayCache = new();
#endif

    // Cache for enum member info to reduce reflection overhead
    private static readonly ConcurrentDictionary<(Type, string), MemberInfo[]> s_memberInfoCache = new();

    /// <summary>
    /// Determines if the enum value has the specified flag.
    /// </summary>
    /// <param name="variable">The enum value to check.</param>
    /// <param name="value">The flag to check for.</param>
    /// <returns>True if the enum has the flag; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// [Flags]
    /// enum MyFlags { A = 1, B = 2, C = 4 }
    /// var flags = MyFlags.A | MyFlags.B;
    /// bool hasA = flags.HasFlag(MyFlags.A); // Returns true
    /// </code>
    /// </example>
    public static bool HasFlag(this Enum variable, Enum value)
    {
        // Performance optimization: Use bitwise operations instead of Enum.HasFlag for better performance
        if (variable == null || value == null)
            return false;

        // Convert to underlying type for bitwise operations (avoids boxing)
        var variableValue = Convert.ToUInt64(variable, CultureInfo.InvariantCulture);
        var flagValue = Convert.ToUInt64(value, CultureInfo.InvariantCulture);

        return (variableValue & flagValue) == flagValue;
    }

    /// <summary>
    /// Gets the <see cref="DescriptionAttribute"/> of the enum value with caching for performance.
    /// </summary>
    /// <param name="item">The enum value.</param>
    /// <returns>The description of the enum value, or the enum name if no description is found.</returns>
    /// <example>
    /// <code>
    /// enum Status 
    /// { 
    ///     [Description("Processing data")]
    ///     Processing,
    ///     Complete 
    /// }
    /// string desc = Status.Processing.GetDescription(); // Returns "Processing data"
    /// </code>
    /// </example>
    public static string GetDescription(this Enum item)
    {
        if (item == null)
            return string.Empty;

        // Prefer source-generated provider when available
        if (Linger.Enums.EnumGeneratedRegistry.TryGetProvider(item.GetType(), out var provider))
        {
            return provider.GetDisplayName(item);
        }

        // Fallback: cached reflection path
        return s_descriptionCache.GetOrAdd(item, GetDescriptionInternal);
    }

#if NET5_0_OR_GREATER
    /// <summary>
    /// Gets the <see cref="DisplayAttribute"/> of the enum value with caching for performance.
    /// </summary>
    /// <param name="item">The enum value.</param>
    /// <returns>The display name of the enum value, or the enum name if no display attribute is found.</returns>
    /// <example>
    /// <code>
    /// enum Priority 
    /// { 
    ///     [Display(Name = "High Priority")]
    ///     High,
    ///     Normal 
    /// }
    /// string display = Priority.High.GetDisplay(); // Returns "High Priority"
    /// </code>
    /// </example>
    public static string GetDisplay(this Enum item)
    {
        if (item == null)
            return string.Empty;

        // Prefer source-generated provider when available
        if (Linger.Enums.EnumGeneratedRegistry.TryGetProvider(item.GetType(), out var provider))
        {
            return provider.GetDisplayName(item);
        }

        // Fallback
        return s_displayCache.GetOrAdd(item, GetDisplayInternal);
    }
#endif

    /// <summary>
    /// Gets the enum value based on the name.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemName">The name of the enum value.</param>
    /// <returns>The enum value corresponding to the name.</returns>
    public static T GetEnum<T>(this string itemName) where T : struct, Enum
    {
#if NET8_0_OR_GREATER
        // Prefer source-generated provider
        if (Linger.Enums.EnumGeneratedRegistry.TryGetProvider(typeof(T), out var provider))
        {
            var (ok, v) = provider.TryGetByName(itemName);
            if (ok && v is T t) return t;
        }
        // Fallback
        return Enum.Parse<T>(itemName);
#else
        if (Linger.Enums.EnumGeneratedRegistry.TryGetProvider(typeof(T), out var provider))
        {
            var (ok, v) = provider.TryGetByName(itemName);
            if (ok && v is T t) return t;
        }
        return (T)Enum.Parse(typeof(T), itemName);
#endif
    }

    /// <summary>
    /// Attempts to parse the specified string to the enum value of type <typeparamref name="T"/>.
    /// A non-throwing alternative to <see cref="GetEnum{T}(string)"/> / <see cref="ToEnum{T}(string)"/>.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="itemName">The enum name text.</param>
    /// <param name="value">When this method returns, contains the parsed enum value if successful; otherwise default.</param>
    /// <param name="ignoreCase">Whether to ignore case when parsing.</param>
    /// <returns><c>true</c> if parse succeeded; otherwise <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// if ("Sunday".TryGetEnum(out DayOfWeek day))
    /// {
    ///     // use day
    /// }
    /// </code>
    /// </example>
    public static bool TryGetEnum<T>(this string? itemName, out T value, bool ignoreCase = false) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            value = default;
            return false;
        }
#if NET8_0_OR_GREATER
        if (Linger.Enums.EnumGeneratedRegistry.TryGetProvider(typeof(T), out var provider) && !ignoreCase)
        {
            System.Diagnostics.Debug.Assert(itemName is not null);
            var (ok, v) = provider.TryGetByName(itemName!);
            if (ok && v is T t) { value = t; return true; }
        }
        return Enum.TryParse(itemName, ignoreCase, out value);
#else
        if (Linger.Enums.EnumGeneratedRegistry.TryGetProvider(typeof(T), out var provider) && !ignoreCase)
        {
            System.Diagnostics.Debug.Assert(itemName is not null);
            var (ok, v) = provider.TryGetByName(itemName!);
            if (ok && v is T t) { value = t; return true; }
        }
        return Enum.TryParse<T>(itemName, ignoreCase, out value);
#endif
    }

    /// <summary>
    /// Converts the string to the corresponding enum value.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemName">The name of the enum value.</param>
    /// <returns>The enum value corresponding to the name.</returns>
    public static T ToEnum<T>(this string itemName) where T : struct, Enum
    {
        return itemName.GetEnum<T>();
    }

    /// <summary>
    /// Gets the enum value based on the integer value.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemValue">The integer value of the enum.</param>
    /// <returns>The enum value corresponding to the integer value.</returns>
    public static T GetEnum<T>(this int itemValue) where T : struct, Enum
    {
#if NET8_0_OR_GREATER
        if (Linger.Enums.EnumGeneratedRegistry.TryGetProvider(typeof(T), out var provider))
        {
            var (ok, v) = provider.TryGetByInt(itemValue);
            if (ok && v is T t) return t;
        }
        var name = Enum.GetName(typeof(T), itemValue) ?? throw new InvalidOperationException();
        return Enum.Parse<T>(name);
#else
        if (Linger.Enums.EnumGeneratedRegistry.TryGetProvider(typeof(T), out var provider))
        {
            var (ok, v) = provider.TryGetByInt(itemValue);
            if (ok && v is T t) return t;
        }
        return (T)Enum.Parse(typeof(T), Enum.GetName(typeof(T), itemValue) ?? throw new InvalidOperationException());
#endif
    }

    /// <summary>
    /// Attempts to convert an integral raw value to the enum <typeparamref name="T"/> without throwing.
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="itemValue">Underlying integral value.</param>
    /// <param name="value">Resulting enum value if defined.</param>
    /// <returns><c>true</c> if <paramref name="itemValue"/> maps to a defined enum member; otherwise <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// if (5.TryGetEnum(out MyFlags flags)) { /* ... */ }
    /// </code>
    /// </example>
    public static bool TryGetEnum<T>(this int itemValue, out T value) where T : struct, Enum
    {
        if (Linger.Enums.EnumGeneratedRegistry.TryGetProvider(typeof(T), out var provider))
        {
            var (ok, v) = provider.TryGetByInt(itemValue);
            if (ok && v is T t) { value = t; return true; }
        }

        if (Enum.IsDefined(typeof(T), itemValue))
        {
            value = (T)Enum.ToObject(typeof(T), itemValue);
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>
    /// Gets the name of the enum value based on the integer value.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemValue">The integer value of the enum.</param>
    /// <returns>The name of the enum value corresponding to the integer value.</returns>
    public static string? GetEnumName<T>(this int itemValue) where T : struct, Enum
    {
        return Enum.GetName(typeof(T), itemValue);
    }

    // Private helper methods for cache implementation
    private static string GetDescriptionInternal(Enum item)
    {
        var type = item.GetType();
        var memberName = item.ToString();

        // Use cached member info if available
        var memInfo = s_memberInfoCache.GetOrAdd((type, memberName),
            key => key.Item1.GetMember(key.Item2));

        if (memInfo.Length > 0)
        {
            var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs.Length > 0 && attrs[0] is DescriptionAttribute descAttr)
            {
                return descAttr.Description;
            }
        }
        return memberName;
    }

#if NET5_0_OR_GREATER
    private static string GetDisplayInternal(Enum item)
    {
        var type = item.GetType();
        var memberName = item.ToString();

        // Use cached member info if available
        var memInfo = s_memberInfoCache.GetOrAdd((type, memberName),
            key => key.Item1.GetMember(key.Item2));

        if (memInfo.Length > 0)
        {
            var attrs = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attrs.Length > 0 && attrs[0] is DisplayAttribute displayAttr)
            {
                return displayAttr.Name ?? memberName;
            }
        }
        return memberName;
    }
#endif
}
