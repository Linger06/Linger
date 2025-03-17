using System.Reflection;
using Linger.Attributes;

namespace Linger.Extensions.Core;

/// <summary>
/// Extension methods for the Type class
/// </summary>
public static partial class TypeExtension
{
    /// <summary>
    /// Determines if the type is an enum.
    /// </summary>
    /// <param name="self">The type to check.</param>
    /// <returns>True if the type is an enum, otherwise false.</returns>
    /// <example>
    /// <code>
    /// bool result = typeof(DayOfWeek).IsEnum();
    /// // Output: true
    /// </code>
    /// </example>
    public static bool IsEnum(this Type self)
    {
#if NET40
        return self.IsEnum;
#else
        return self.GetTypeInfo().IsEnum;
#endif
    }

    /// <summary>
    /// Determines if the type is an enum or a nullable enum.
    /// </summary>
    /// <param name="self">The type to check.</param>
    /// <returns>True if the type is an enum or a nullable enum, otherwise false.</returns>
    /// <example>
    /// <code>
    /// bool result = typeof(DayOfWeek?).IsEnumOrNullableEnum();
    /// // Output: true
    /// </code>
    /// </example>
    public static bool IsEnumOrNullableEnum(this Type self)
    {
        return self.IsEnum || (self.IsGenericType && self.GetGenericTypeDefinition() == typeof(Nullable<>) && self.GetGenericArguments()[0].IsEnum);
    }

    /// <summary>
    /// Determines if the type is a boolean.
    /// </summary>
    /// <param name="self">The type to check.</param>
    /// <returns>True if the type is a boolean, otherwise false.</returns>
    /// <example>
    /// <code>
    /// bool result = typeof(bool).IsBool();
    /// // Output: true
    /// </code>
    /// </example>
    public static bool IsBool(this Type self)
    {
        return self == typeof(bool);
    }

    /// <summary>
    /// Determines if the type is a boolean or a nullable boolean.
    /// </summary>
    /// <param name="self">The type to check.</param>
    /// <returns>True if the type is a boolean or a nullable boolean, otherwise false.</returns>
    /// <example>
    /// <code>
    /// bool result = typeof(bool?).IsBoolOrNullableBool();
    /// // Output: true
    /// </code>
    /// </example>
    public static bool IsBoolOrNullableBool(this Type self)
    {
        return self == typeof(bool) || self == typeof(bool?);
    }

    /// <summary>
    /// Gets a property info by name.
    /// </summary>
    /// <param name="objType">The type to get the property from.</param>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property info.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the objType or name is null.</exception>
    /// <exception cref="NullReferenceException">Thrown if the property is not found.</exception>
    /// <example>
    /// <code>
    /// PropertyInfo property = typeof(Person).GetPropertyInfo("Name");
    /// // Output: PropertyInfo object for "Name" property
    /// </code>
    /// </example>
    public static PropertyInfo GetPropertyInfo(this Type objType, string name)
    {
        ArgumentNullException.ThrowIfNull(objType);
        ArgumentNullException.ThrowIfNull(name);

        PropertyInfo? matchedProperty = objType.GetProperties().FirstOrDefault(p => p.Name == name);
        if (matchedProperty == null)
            throw new NullReferenceException($"Property '{name}' not found on type '{objType.FullName}'.");

        return matchedProperty;
    }

    /// <summary>
    /// Gets the column information for the specified type.
    /// </summary>
    /// <param name="type">The type to get column information for.</param>
    /// <returns>A list of <see cref="ColumnInfo"/> objects that represent the columns of the specified type.</returns>
    /// <example>
    /// <code>
    /// var columns = GetColumnsInfo(typeof(Person));
    /// // Output: List of ColumnInfo objects for properties of Person class
    /// </code>
    /// </example>
    public static List<ColumnInfo> GetColumnsInfo(this Type type)
    {
        var columns = new List<ColumnInfo>();
        var counter = 1;
        foreach (PropertyInfo propertyInfo in type.GetProperties())
        {
            UserDefinedTableTypeColumnAttribute? attribute = GetAttribute<UserDefinedTableTypeColumnAttribute>(propertyInfo);
            var propertyName = attribute?.Name ?? propertyInfo.Name;

            var column = new ColumnInfo
            {
                PropertyName = propertyName,
                PropertyOrder = attribute?.Order ?? counter,
                Property = propertyInfo
            };
            columns.Add(column);
            counter++;
        }

        return columns.OrderBy(info => info.PropertyOrder).ToList();
    }

    /// <summary>
    /// Gets the specified attribute from the property.
    /// </summary>
    /// <typeparam name="T">The type of the attribute.</typeparam>
    /// <param name="propertyInfo">The property to get the attribute from.</param>
    /// <returns>The attribute if found; otherwise, <c>null</c>.</returns>
    /// <example>
    /// <code>
    /// var attribute = GetAttribute&lt;MyAttribute&gt;(typeof(Person).GetProperty("Id"));
    /// // Output: MyAttribute object or null
    /// </code>
    /// </example>
    private static T? GetAttribute<T>(PropertyInfo propertyInfo) where T : System.Attribute
    {
        return propertyInfo.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
    }
}

/// <summary>
/// Represents information about a column.
/// </summary>
public class ColumnInfo
{
    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    /// <value>The name of the property.</value>
    public string PropertyName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the order of the property.
    /// </summary>
    /// <value>The order of the property.</value>
    public int PropertyOrder { get; set; }

    /// <summary>
    /// Gets or sets the property information.
    /// </summary>
    /// <value>The property information.</value>
    public PropertyInfo Property { get; set; } = null!;
}