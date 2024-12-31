using System.Reflection;

namespace Linger.Extensions.Core;

/// <summary>
/// Extension methods for the Type class
/// </summary>
public static partial class TypeExtension
{
    /// <summary>
    /// Gets or sets the property cache.
    /// </summary>
    /// <value>The property cache.</value>
    public static Dictionary<string, List<PropertyInfo>> PropertyCache { get; set; } = [];

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
    /// Gets a single property by name.
    /// </summary>
    /// <param name="self">The type to check.</param>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property info if found, otherwise null.</returns>
    public static PropertyInfo? GetSingleProperty(this Type self, string name)
    {
        var fullName = self.FullName ?? throw new NullReferenceException(nameof(self.FullName));

        if (!PropertyCache.TryGetValue(fullName, out List<PropertyInfo>? value))
        {
            value = self.GetProperties().ToList();
            PropertyCache[fullName] = value;
        }

        return value.FirstOrDefault(x => x.Name == name);
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
    public static T? GetDescriptionValue<T>(this Type field) where T : System.Attribute
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
    public static T? GetDescriptionValue<T>(this FieldInfo field) where T : System.Attribute
    {
        var customAttributes = field.GetCustomAttributes(typeof(T), false);
        return customAttributes.Length > 0 ? (T)customAttributes[0] : null;
    }

    /// <summary>
    /// Gets the properties of a type.
    /// </summary>
    /// <param name="type">The type to get the properties of.</param>
    /// <returns>The properties of the type.</returns>
    public static IEnumerable<PropertyInfo> Props(this Type type)
    {
        return type.GetProperties();
    }

    /// <summary>
    /// Gets the properties of an object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to get the properties of.</param>
    /// <returns>The properties of the object.</returns>
    public static IEnumerable<PropertyInfo>? Props<T>(this T obj)
    {
        return obj?.GetType().GetProperties();
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
    public static bool HasAttribute<TAttr>(this Type type, Func<TAttr, bool> predicate) where TAttr : System.Attribute
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
    public static IEnumerable<PropertyInfo> AttrProps<E>(this Type type) where E : System.Attribute
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
    public static Dictionary<PropertyInfo, E?> AttrValues<E>(this Type type) where E : System.Attribute
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
    public static Dictionary<PropertyInfo, E?> AttrPropValues<E>(this Type type) where E : System.Attribute
#else
    public static Dictionary<PropertyInfo, E> AttrPropValues<E>(this Type type) where E : System.Attribute
#endif
    {
        IEnumerable<PropertyInfo> props = type.AttrProps<E>();
        return props.ToDictionary(item => item, item => item.GetCustomAttribute<E>());
    }
#endif
}