
namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for enum operations with performance optimizations.
/// </summary>
public static class EnumExtensions
{
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
        {
            throw new ArgumentException("Enum name cannot be null or empty", nameof(itemName));
        }

        if (!Helper.EnumConversionHelper.TryConvertToEnum(itemName, typeof(T), out var result))
        {
            throw new ArgumentException($"Value '{itemName}' is not valid for enum {typeof(T).Name}", nameof(itemName));
        }

        return (T)result!;
    }

    /// <summary>
    /// Gets the enum value based on the name (alias for GetEnum).
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemName">The name of the enum value.</param>
    /// <returns>The enum value.</returns>
    [Obsolete("Use GetEnum<T>() instead.")]
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
        if (!Helper.EnumConversionHelper.TryConvertToEnum(itemValue, typeof(T), out var result))
        {
            throw new InvalidOperationException($"Value {itemValue} is not defined for enum {typeof(T).Name}");
        }

        return (T)result!;
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
        if (itemName is null || itemName.Length == 0)
        {
            return false;
        }

        if (!Helper.EnumConversionHelper.TryConvertToEnum(itemName, typeof(T), out var result))
        {
            return false;
        }

        value = (T)result!;
        return true;
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

        if (!Helper.EnumConversionHelper.TryConvertToEnum(itemValue, typeof(T), out var result))
        {
            return false;
        }

        value = (T)result!;
        return true;
    }

}
