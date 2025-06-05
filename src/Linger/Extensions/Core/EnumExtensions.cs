using System.ComponentModel;
using System.Reflection;
#if NET5_0_OR_GREATER

using System.ComponentModel.DataAnnotations;

#endif

namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for enumerations.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the enum value based on the name.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemName">The name of the enum value.</param>
    /// <returns>The enum value corresponding to the name.</returns>
    public static T GetEnum<T>(this string itemName)
    {
        return (T)Enum.Parse(typeof(T), itemName);
    }

    /// <summary>
    /// Converts the string to the corresponding enum value.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemName">The name of the enum value.</param>
    /// <returns>The enum value corresponding to the name.</returns>
    public static T ToEnum<T>(this string itemName)
    {
        return itemName.GetEnum<T>();
    }

    /// <summary>
    /// Gets the enum value based on the integer value.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemValue">The integer value of the enum.</param>
    /// <returns>The enum value corresponding to the integer value.</returns>
    public static T GetEnum<T>(this int itemValue)
    {
        return (T)Enum.Parse(typeof(T),
            Enum.GetName(typeof(T), itemValue) ?? throw new InvalidOperationException());
    }

    /// <summary>
    /// Gets the name of the enum value based on the integer value.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="itemValue">The integer value of the enum.</param>
    /// <returns>The name of the enum value corresponding to the integer value.</returns>
    public static string? GetEnumName<T>(this int itemValue)
    {
        return Enum.GetName(typeof(T), itemValue);
    }

    /// <summary>
    /// Gets the <see cref="DescriptionAttribute"/> of the enum value.
    /// </summary>
    /// <param name="item">The enum value.</param>
    /// <returns>The description of the enum value.</returns>
    public static string GetDescription(this Enum item)
    {
        Type type = item.GetType();
        MemberInfo[] memInfo = type.GetMember(item.ToString());
        if (memInfo.Length > 0)
        {
            var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs.Length > 0)
            {
                return ((DescriptionAttribute)attrs[0]).Description;
            }
        }

        return item.ToString();  // If no Description attribute exists, return the enum name
    }

#if NET5_0_OR_GREATER

    /// <summary>
    /// Gets the <see cref="DisplayAttribute"/> of the enum value.
    /// </summary>
    /// <param name="item">The enum value.</param>
    /// <returns>The display attribute of the enum value.</returns>
    public static string GetDisplay(this Enum item)
    {
        Type type = item.GetType();
        MemberInfo[] memInfo = type.GetMember(item.ToString());
        if (memInfo.IsNotNull() && memInfo.Length > 0)
        {
            var attrs = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attrs.IsNotNull() && attrs.Length > 0)
            {
                return ((DisplayAttribute)attrs[0]).Name!;
            }
        }

        return item.ToString();  // If no display attribute exists, return the enum name
    }

#endif
}