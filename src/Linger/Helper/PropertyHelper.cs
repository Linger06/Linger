using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Linger.Extensions.Core;

namespace Linger.Helper;

/// <summary>
/// Helper class for property operations.
/// </summary>
public static class PropertyHelper
{
    private static readonly ConcurrentDictionary<string, PropertyInfo?> s_cachedObjectProperties = new();

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
    /// Gets the property name from the expression.
    /// </summary>
    /// <param name="expression">The property expression.</param>
    /// <param name="getAll">Whether to get the full property path, e.g., a.b.c.</param>
    /// <returns>The property name.</returns>
    /// <example>
    /// <code>
    /// string propName = ExpressionHelper.GetPropertyName((MyClass x) => x.MyProperty);
    /// // returns "MyProperty"
    /// </code>
    /// </example>
    public static string GetPropertyName(this Expression expression, bool getAll = true)
    {
        if (expression.IsNull())
        {
            return string.Empty;
        }

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

        var rv = string.Empty;
        if (me != null)
        {
            rv = me.Member.Name;
        }

        while (me != null && getAll && me.NodeType == ExpressionType.MemberAccess)
        {
            Expression? exp = me.Expression;
            if (exp is MemberExpression exp1)
            {
                rv = exp1.Member.Name + "." + rv;
                me = exp1;
            }
            else if (exp is MethodCallExpression mexp)
            {
                if (mexp.Method.Name == "get_Item")
                {
                    object? index = null;
                    if (mexp.Arguments[0] is MemberExpression memberExpression1)
                    {
                        if (memberExpression1.Expression is ConstantExpression constantExpression)
                        {
                            var obj = constantExpression.Value!;
                            FieldInfo? field = obj.GetType().GetField(memberExpression1.Member.Name);
                            index = field!.GetValue(obj);
                        }
                    }
                    else if (mexp.Arguments[0] is ConstantExpression constantExpression)
                    {
                        index = constantExpression.Value;
                    }

                    if (mexp.Object is MemberExpression member)
                    {
                        rv = member.Member.Name + "[" + index + "]." + rv;
                        me = member;
                    }
                }
            }
            else
            {
                break;
            }
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
            rv = me.Member.DeclaringType?.GetProperties().FirstOrDefault(x => x.Name == me.Member.Name);
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
        TrySetProperty(obj, propertySelector, x => valueFactory(), ignoreAttributeTypes);
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

            PropertyInfo? propertyInfo = obj?.GetType().GetProperties().FirstOrDefault(x =>
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
