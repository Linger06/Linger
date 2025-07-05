using System.Collections.Concurrent;
using System.Reflection;
using Linger.Attributes;

namespace Linger.Extensions.Core;

/// <summary>
/// Extension methods for the Type class with performance optimizations
/// </summary>
public static class TypeExtension
{
    /// <summary>
    /// Thread-safe property cache for improved performance.
    /// </summary>
    /// <value>The property cache.</value>
    private static readonly ConcurrentDictionary<string, List<PropertyInfo>> PropertyCache = new();

    /// <summary>
    /// Cache for type properties to minimize reflection overhead.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> TypePropertyCache = new();

    /// <summary>
    /// Cache for column information to minimize reflection overhead.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, List<ColumnInfo>> ColumnInfoCache = new();

    /// <summary>
    /// Determines if the type is a generic type.
    /// </summary>
    /// <param name="self">The type to check.</param>
    /// <param name="innerType">The generic type definition.</param>
    /// <returns>True if the type is a generic type and matches the generic type definition, otherwise false.</returns>
    public static bool IsGeneric(this Type self, Type innerType)
    {
        var isGenericType = self.IsGenericType;
        return isGenericType && self.GetGenericTypeDefinition() == innerType;
    }

    /// <summary>
    /// Determines if the type is a nullable type.
    /// </summary>
    /// <param name="self">The type to check.</param>
    /// <returns>True if the type is a nullable type, otherwise false.</returns>
    public static bool IsNullable(this Type self)
    {
        return self.IsGeneric(typeof(Nullable<>));
    }

    /// <summary>
    /// Determines if the type inherits from a base type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="baseType">The base type.</param>
    /// <returns>True if the type inherits from the base type, otherwise false.</returns>
    public static bool IsInherits(this Type type, Type baseType)
    {
        return baseType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Determines if the type is a List type.
    /// </summary>
    /// <param name="self">The type to check.</param>
    /// <returns>True if the type is a List type, otherwise false.</returns>
    public static bool IsList(this Type self)
    {
        return self.IsGeneric(typeof(List<>));
    }

    /// <summary>
    /// Determines if the type is a primitive type.
    /// </summary>
    /// <param name="self">The type to check.</param>
    /// <returns>True if the type is a primitive type, otherwise false.</returns>
    public static bool IsPrimitive(this Type self)
    {
        var isPrimitive = self.IsPrimitive;
        return isPrimitive || self == typeof(decimal);
    }

    /// <summary>
    /// Determines if the type is a numeric type.
    /// </summary>
    /// <param name="self">The type to check.</param>
    /// <returns>True if the type is a numeric type, otherwise false.</returns>
    public static bool IsNumber(this Type self)
    {
        Type checkType = self.IsNullable() ? self.GetGenericArguments()[0] : self;
        return checkType == typeof(int) || checkType == typeof(short) || checkType == typeof(long) ||
               checkType == typeof(float) || checkType == typeof(decimal) || checkType == typeof(double);
    }

    /// <summary>
    /// Gets a single property by name with thread-safe caching.
    /// </summary>
    /// <param name="self">The type to check.</param>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property info if found, otherwise null.</returns>
    /// <example>
    /// <code>
    /// PropertyInfo? prop = typeof(Person).GetSingleProperty("Name");
    /// </code>
    /// </example>
    public static PropertyInfo? GetSingleProperty(this Type self, string name)
    {
        var fullName = self.FullName ?? throw new ArgumentException(nameof(self.FullName));

        var properties = PropertyCache.GetOrAdd(fullName, _ => self.GetProperties().ToList());
        return properties.FirstOrDefault(x => x.Name == name);
    }

    /// <summary>
    /// Gets the type name, including handling for nullable types.
    /// </summary>
    /// <param name="t">The type to get the name of.</param>
    /// <param name="flag">Whether to include the nullable indicator.</param>
    /// <returns>The type name.</returns>
    public static string GetTypeName(this Type t, bool flag = true)
    {
        if (t.IsGenericType)
        {
            if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return t.GetGenericArguments()[0].GetTypeName() + (flag ? "?" : string.Empty);
            }

            var result = $"{t.Name.Remove(t.Name.IndexOf('`'))}<{string.Join(",", t.GetGenericArguments().Select(at => at.GetTypeName()))}>";
            return result;
        }

        if (t.IsArray)
        {
            Type? type = t.GetElementType();
            return $"{type!.GetTypeName()}[{new string(',', t.GetArrayRank() - 1)}]";
        }

        return t.Name;
    }

    /// <summary>
    /// Gets the default value for a type.
    /// </summary>
    /// <param name="targetType">The type to get the default value for.</param>
    /// <returns>The default value.</returns>
    public static object? GetDefaultValue(this Type targetType)
    {
        return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
    }

    /// <summary>
    /// Gets a custom attribute of a specified type.
    /// </summary>
    /// <typeparam name="T">The type of the attribute.</typeparam>
    /// <param name="field">The type to get the attribute from.</param>
    /// <returns>The attribute if found, otherwise null.</returns>
    public static T? GetDescriptionValue<T>(this Type field) where T : Attribute
    {
        var customAttributes = field.GetCustomAttributes(typeof(T), false);
        return customAttributes.Length > 0 ? (T)customAttributes[0] : null;
    }

    /// <summary>
    /// Gets a custom attribute of a specified type from a field.
    /// </summary>
    /// <typeparam name="T">The type of the attribute.</typeparam>
    /// <param name="field">The field to get the attribute from.</param>
    /// <returns>The attribute if found, otherwise null.</returns>
    public static T? GetDescriptionValue<T>(this FieldInfo field) where T : Attribute
    {
        var customAttributes = field.GetCustomAttributes(typeof(T), false);
        return customAttributes.Length > 0 ? (T)customAttributes[0] : null;
    }

    /// <summary>
    /// Gets the properties of a type with caching for performance.
    /// </summary>
    /// <param name="type">The type to get the properties of.</param>
    /// <returns>The properties of the type.</returns>
    /// <example>
    /// <code>
    /// PropertyInfo[] properties = typeof(Person).Props().ToArray();
    /// </code>
    /// </example>
    public static IEnumerable<PropertyInfo> Props(this Type type)
    {
        return TypePropertyCache.GetOrAdd(type, t => t.GetProperties());
    }

    /// <summary>
    /// Gets the properties of an object with caching for performance.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to get the properties of.</param>
    /// <returns>The properties of the object.</returns>
    /// <example>
    /// <code>
    /// var person = new Person();
    /// PropertyInfo[] properties = person.Props()?.ToArray();
    /// </code>
    /// </example>
    public static IEnumerable<PropertyInfo>? Props<T>(this T obj)
    {
        return obj?.GetType().Props();
    }

    /// <summary>
    /// Determines if the type has a specified attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type has the attribute, otherwise false.</returns>
    public static bool HasAttribute<TAttribute>(this Type type)
    {
        var attribs = type.GetCustomAttributes(typeof(TAttribute), true);
        return attribs.Length > 0;
    }

    /// <summary>
    /// Gets a specified attribute from the type.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="type">The type to get the attribute from.</param>
    /// <returns>The attribute.</returns>
    public static TAttribute GetAttribute<TAttribute>(this Type type)
    {
        var attribs = type.GetCustomAttributes(typeof(TAttribute), true);
        return (TAttribute)attribs[0];
    }

    /// <summary>
    /// Determines if the type has a specified attribute that matches a predicate.
    /// </summary>
    /// <typeparam name="TAttr">The type of the attribute.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <param name="predicate">The predicate to apply to the attribute.</param>
    /// <returns>True if the type has a matching attribute, otherwise false.</returns>
    public static bool HasAttribute<TAttr>(this Type type, Func<TAttr, bool> predicate) where TAttr : Attribute
    {
        return ((TAttr[])type.GetCustomAttributes(typeof(TAttr), true)).Any(predicate);
    }

#if !NETFRAMEWORK || NET462_OR_GREATER

    /// <summary>
    /// Gets the properties of a type that have specified attributes.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="attrTypes">The attribute types to check for.</param>
    /// <returns>The properties that have the specified attributes.</returns>
    public static IEnumerable<PropertyInfo> AttrProps(this Type type, params Type[] attrTypes)
    {
        return type.Props().Where(item => item.HasAttribute(attrTypes));
    }

    /// <summary>
    /// Gets the properties of a type that have a specified attribute.
    /// </summary>
    /// <typeparam name="E">The type of the attribute.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns>The properties that have the specified attribute.</returns>
    public static IEnumerable<PropertyInfo> AttrProps<E>(this Type type) where E : Attribute
    {
        return type.AttrProps(typeof(E));
    }

    /// <summary>
    /// Determines if a member has a specified attribute.
    /// </summary>
    /// <typeparam name="T">The type of the attribute.</typeparam>
    /// <param name="info">The member to check.</param>
    /// <returns>True if the member has the attribute, otherwise false.</returns>
    public static bool HasAttribute<T>(this MemberInfo info)
    {
        return info.HasAttribute(typeof(T));
    }

    /// <summary>
    /// Determines if the type has specified attributes.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="attrTypes">The attribute types to check for.</param>
    /// <returns>True if the type has the attributes, otherwise false.</returns>
    public static bool HasAttribute(this Type type, params Type[] attrTypes)
    {
        return type.CustomAttributes.Any(attr => attrTypes.Contains(attr.AttributeType));
    }

    /// <summary>
    /// Determines if a member has specified attributes.
    /// </summary>
    /// <param name="info">The member to check.</param>
    /// <param name="attrTypes">The attribute types to check for.</param>
    /// <returns>True if the member has the attributes, otherwise false.</returns>
    public static bool HasAttribute(this MemberInfo info, params Type[] attrTypes)
    {
        return info.CustomAttributes.Any(attr => attrTypes.Contains(attr.AttributeType));
    }

    /// <summary>
    /// Gets the properties of a type that have a specified attribute and their attribute values.
    /// </summary>
    /// <typeparam name="E">The type of the attribute.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns>A dictionary of properties and their attribute values.</returns>
#if NET5_0_OR_GREATER
    public static Dictionary<PropertyInfo, E?> AttrValues<E>(this Type type) where E : Attribute
#else
    public static Dictionary<PropertyInfo, E> AttrValues<E>(this Type type) where E : System.Attribute
#endif
    {
        return type.AttrPropValues<E>();
    }

    /// <summary>
    /// Gets the properties of a type that have a specified attribute and their attribute values.
    /// </summary>
    /// <typeparam name="E">The type of the attribute.</typeparam>
    /// <param name="type">The type to check.</param>
    /// <returns>A dictionary of properties and their attribute values.</returns>
#if NET5_0_OR_GREATER
    public static Dictionary<PropertyInfo, E?> AttrPropValues<E>(this Type type) where E : Attribute
#else
    public static Dictionary<PropertyInfo, E> AttrPropValues<E>(this Type type) where E : System.Attribute
#endif
    {
        IEnumerable<PropertyInfo> props = type.AttrProps<E>();
        return props.ToDictionary(item => item, item => item.GetCustomAttribute<E>());
    }
#endif

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
    /// Gets a property info by name with caching for performance.
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

        // Use cached properties for better performance
        var properties = TypePropertyCache.GetOrAdd(objType, t => t.GetProperties());
        PropertyInfo? matchedProperty = properties.FirstOrDefault(p => p.Name == name);
        return matchedProperty ?? throw new InvalidOperationException($"Property '{name}' not found on type '{objType.FullName}'.");
    }

    /// <summary>
    /// Gets the column information for the specified type with caching for performance.
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
        ArgumentNullException.ThrowIfNull(type);

        return ColumnInfoCache.GetOrAdd(type, t =>
        {
            var columns = new List<ColumnInfo>();
            var counter = 0;

            // 使用缓存的属性信息以提高性能
            var properties = TypePropertyCache.GetOrAdd(t, pt => pt.GetProperties());

            foreach (var propertyInfo in properties)
            {
                counter++;

                // 使用GetCustomAttribute<T>方法直接获取特性，避免使用反射
                var attribute = propertyInfo.GetCustomAttribute<UserDefinedTableTypeColumnAttribute>(true);

                // 创建并添加列信息
                var column = new ColumnInfo
                {
                    PropertyName = attribute?.Name ?? propertyInfo.Name,
                    PropertyOrder = attribute?.Order > 0 ? attribute.Order : counter,
                    Property = propertyInfo,
                    PropertyType = propertyInfo.PropertyType
                };

                columns.Add(column);
            }

            // 按照PropertyOrder排序
            return columns.OrderBy(info => info.PropertyOrder).ToList();
        });
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

    /// <summary>
    /// Gets or sets the type of the property.
    /// </summary>
    /// <value>The type of the property.</value>
    public Type PropertyType { get; set; } = null!;
}
