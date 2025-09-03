using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Linger.Extensions.Core;

namespace Linger.Helper;

/// <summary>
/// Helper class for property operations with performance optimizations.
/// </summary>
public static class PropertyHelper
{
    private static readonly ConcurrentDictionary<string, PropertyInfo?> s_cachedObjectProperties = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> s_typePropertiesCache = new();

    /// <summary>
    /// Gets the member expression for the specified member name.
    /// </summary>
    /// <param name="self">The parameter expression.</param>
    /// <param name="memberName">The member name.</param>
    /// <returns>The member expression.</returns>
    /// <example>
    /// <code>
    /// var param = Expression.Parameter(typeof(MyClass), "x");
    /// var memberExp = param.GetMemberExp("MyProperty");
    /// // returns the member expression for MyProperty
    /// </code>
    /// </example>
    public static Expression GetMemberExp(this ParameterExpression self, string memberName)
    {
        var names = memberName.Split(',');
        var rv = Expression.PropertyOrField(self, names[0]);
        for (var i = 1; i < names.Length; i++)
        {
            rv = Expression.PropertyOrField(rv, names[i]);
        }
        return rv;
    }

    /// <summary>
    /// Gets the property information from the expression.
    /// </summary>
    /// <param name="expression">The property expression.</param>
    /// <returns>The property information.</returns>
    /// <example>
    /// <code>
    /// PropertyInfo? propInfo = ExpressionHelper.GetPropertyInfo((MyClass x) => x.MyProperty);
    /// // returns the PropertyInfo for MyProperty
    /// </code>
    /// </example>
    public static PropertyInfo? GetPropertyInfo(this Expression expression)
    {
        MemberExpression? me = null;
        if (expression is MemberExpression memberExpression)
        {
            me = memberExpression;
        }

        if (expression is LambdaExpression le)
        {
            if (le.Body is MemberExpression body)
            {
                me = body;
            }

            if (le.Body is UnaryExpression unaryExpression)
            {
                me = unaryExpression.Operand as MemberExpression;
            }
        }

        PropertyInfo? rv = null;
        if (me != null)
        {
            var declaringType = me.Member.DeclaringType;
            if (declaringType != null)
            {
                // Use cached properties for better performance
                var properties = s_typePropertiesCache.GetOrAdd(declaringType, t => t.GetProperties());
                rv = properties.FirstOrDefault(x => x.Name == me.Member.Name);
            }
        }

        return rv;
    }

    /// <summary>
    /// Gets the property information for the specified expression.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="field">The property expression.</param>
    /// <returns>The property information.</returns>
    /// <example>
    /// <code>
    /// PropertyInfo? propInfo = PropertyHelper.GetPropertyInfo((MyClass x) => x.MyProperty);
    /// // returns the PropertyInfo for MyProperty
    /// </code>
    /// </example>
    public static PropertyInfo? GetPropertyInfo<T>(Expression<Func<T, object>> field)
    {
        return field.GetPropertyInfo();
    }

    /// <summary>
    /// Tries to set the property value using the specified value factory.
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="obj">The object.</param>
    /// <param name="propertySelector">The property selector expression.</param>
    /// <param name="valueFactory">The value factory function.</param>
    /// <param name="ignoreAttributeTypes">The attribute types to ignore.</param>
    /// <example>
    /// <code>
    /// PropertyHelper.TrySetProperty(myObject, x => x.MyProperty, () => "NewValue");
    /// // sets the value of MyProperty to "NewValue"
    /// </code>
    /// </example>
    public static void TrySetProperty<TObject, TValue>(
        TObject obj,
        Expression<Func<TObject, TValue>> propertySelector,
        Func<TValue> valueFactory,
        params Type[] ignoreAttributeTypes)
    {
        TrySetProperty(obj, propertySelector, _ => valueFactory(), ignoreAttributeTypes);
    }

    /// <summary>
    /// Tries to set the property value using the specified value factory.
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="obj">The object.</param>
    /// <param name="propertySelector">The property selector expression.</param>
    /// <param name="valueFactory">The value factory function.</param>
    /// <param name="ignoreAttributeTypes">The attribute types to ignore.</param>
    /// <example>
    /// <code>
    /// PropertyHelper.TrySetProperty(myObject, x => x.MyProperty, obj => "NewValue");
    /// // sets the value of MyProperty to "NewValue"
    /// </code>
    /// </example>
    public static void TrySetProperty<TObject, TValue>(
        TObject obj,
        Expression<Func<TObject, TValue>> propertySelector,
        Func<TObject, TValue> valueFactory,
        params Type[] ignoreAttributeTypes)
    {
        var cacheKey = $"{obj?.GetType().FullName}-" +
                       $"{propertySelector}-" +
                       $"{(ignoreAttributeTypes.IsNotNull() ? "-" + string.Join("-", ignoreAttributeTypes.Select(x => x.FullName)) : "")}";

        PropertyInfo? property = s_cachedObjectProperties.GetOrAdd(cacheKey, _ =>
        {
            if (propertySelector.Body.NodeType != ExpressionType.MemberAccess)
            {
                return null;
            }

            var memberExpression = (MemberExpression)propertySelector.Body;
            var objType = obj?.GetType();

            if (objType == null) return null;

            // Use cached properties for better performance
            var properties = s_typePropertiesCache.GetOrAdd(objType, t => t.GetProperties());
            PropertyInfo? propertyInfo = properties.FirstOrDefault(x =>
                x.Name == memberExpression.Member.Name &&
                x.GetSetMethod(true) != null);

            if (propertyInfo == null)
            {
                return null;
            }

            if (ignoreAttributeTypes.IsNotNull() &&
                ignoreAttributeTypes.Any(ignoreAttribute => propertyInfo.IsDefined(ignoreAttribute, true)))
            {
                return null;
            }

            return propertyInfo;
        });

        property?.SetValue(obj, valueFactory(obj), null);
    }
}
