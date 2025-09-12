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
    /// Gets the description attribute of the enum value with caching for performance.
    /// If no description attribute is found, returns the enum name.
    /// </summary>
    /// <param name="item">The enum value.</param>
    /// <returns>The description of the enum value, or the enum name if no description is found.</returns>
    /// <example>
    /// <code>
    /// enum Status 
    /// { 
    ///     [Description("Currently Active")]
    ///     Active,
    ///     Inactive 
    /// }
    /// string desc = Status.Active.GetDescription(); // Returns "Currently Active"
    /// </code>
    /// </example>
    public static string GetDescription(this Enum item)
    {
        if (item == null)
            return string.Empty;

        // Use cached reflection path
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

        // Use cached reflection path
        return s_displayCache.GetOrAdd(item, GetDisplayInternal);
    }
#endif

    /// <summary>
    /// Gets the enum value based on the name (extension method version).
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemName">The name of the enum value.</param>
    /// <returns>The enum value.</returns>
    /// <exception cref="ArgumentException">Thrown when the name is not found.</exception>
    public static T GetEnum<T>(this string itemName) where T : struct, Enum
    {
        if (string.IsNullOrEmpty(itemName))
            throw new System.ArgumentException("Enum name cannot be null or empty", nameof(itemName));
#if NET9_0_OR_GREATER
        return Enum.Parse<T>(itemName, true);
#else
        return (T)Enum.Parse(typeof(T), itemName, true);
#endif
    }

    /// <summary>
    /// Gets the enum value based on the name (alias for GetEnum).
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemName">The name of the enum value.</param>
    /// <returns>The enum value.</returns>
    public static T ToEnum<T>(this string itemName) where T : struct, Enum
    {
        return itemName.GetEnum<T>();
    }

    /// <summary>
    /// Gets the enum value based on the integer value (extension method version).
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemValue">The integer value of the enum.</param>
    /// <returns>The enum value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the value is not valid for the enum type.</exception>
    public static T GetEnum<T>(this int itemValue) where T : struct, Enum
    {
        if (!Enum.IsDefined(typeof(T), itemValue))
            throw new InvalidOperationException($"Value {itemValue} is not defined for enum {typeof(T).Name}");

        return (T)Enum.ToObject(typeof(T), itemValue);
    }

    /// <summary>
    /// Gets the enum name based on the integer value.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemValue">The integer value of the enum.</param>
    /// <returns>The enum name, or null if not found.</returns>
    public static string? GetEnumName<T>(this int itemValue) where T : struct, Enum
    {
        if (!Enum.IsDefined(typeof(T), itemValue))
            return null;

        var enumValue = (T)Enum.ToObject(typeof(T), itemValue);
        return enumValue.ToString();
    }

    /// <summary>
    /// Tries to get the enum value based on the name (extension method version).
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemName">The name of the enum value.</param>
    /// <param name="value">The parsed enum value.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryGetEnum<T>(this string? itemName, out T value) where T : struct, Enum
    {
        value = default;
        if (string.IsNullOrEmpty(itemName))
            return false;

        return Enum.TryParse<T>(itemName, true, out value);
    }

    /// <summary>
    /// Tries to get the enum value based on the integer value (extension method version).
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemValue">The integer value of the enum.</param>
    /// <param name="value">The parsed enum value.</param>
    /// <returns>True if the value is valid for the enum type; otherwise, false.</returns>
    public static bool TryGetEnum<T>(this int itemValue, out T value) where T : struct, Enum
    {
        value = default;

        if (!Enum.IsDefined(typeof(T), itemValue))
            return false;

        value = (T)Enum.ToObject(typeof(T), itemValue);
        return true;
    }

    /// <summary>
    /// Internal method to get description using reflection.
    /// </summary>
    /// <param name="enumValue">The enum value.</param>
    /// <returns>The description or enum name.</returns>
    private static string GetDescriptionInternal(Enum enumValue)
    {
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
        var descriptionAttribute = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();
        return descriptionAttribute?.Description ?? enumValue.ToString();
    }

#if NET5_0_OR_GREATER
    /// <summary>
    /// Internal method to get display name using reflection.
    /// </summary>
    /// <param name="enumValue">The enum value.</param>
    /// <returns>The display name or enum name.</returns>
    private static string GetDisplayInternal(Enum enumValue)
    {
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
        var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.Name ?? enumValue.ToString();
    }
#endif
}
