using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Linger.Extensions.Core;

namespace Linger.Helper;

/// <summary>
/// Helper class for property operations with performance optimizations.
/// </summary>
#if NET5_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Uses runtime reflection to inspect and set properties. This API is not compatible with trimming.")]
#endif
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
    /// 获取属性名
    /// </summary>
    /// <param name="expression">属性表达式</param>
    /// <param name="getAll">是否获取全部级别名称，比如a.b.c</param>
    /// <returns>属性名</returns>
    public static string GetPropertyName(this Expression expression, bool getAll = true)
    {
        MemberExpression? me = GetMemberExpression(expression);
        if (me is null)
        {
            return string.Empty;
        }

        var rv = me.Member.Name;

        while (getAll)
        {
            Expression? exp = me.Expression;
            if (exp is MemberExpression exp1)
            {
                rv = exp1.Member.Name + "." + rv;
                me = exp1;
            }
            else if (exp is MethodCallExpression { Method.Name: "get_Item", Object: MemberExpression member } call &&
                     call.Arguments.Count == 1)
            {
                var index = call.Arguments[0] switch
                {
                    ConstantExpression constant => constant.Value,
                    MemberExpression { Member: FieldInfo field, Expression: ConstantExpression owner } => field.GetValue(owner.Value),
                    _ => null
                };
                rv = member.Member.Name + "[" + index + "]." + rv;
                me = member;
            }
            else
            {
                break;
            }
        }

        return rv;
    }

    private static MemberExpression? GetMemberExpression(Expression? expression)
    {
        return expression switch
        {
            MemberExpression memberExpression => memberExpression,
            LambdaExpression { Body: MemberExpression memberExpression } => memberExpression,
            LambdaExpression { Body: UnaryExpression { Operand: MemberExpression memberExpression } } => memberExpression,
            _ => null
        };
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
        MemberExpression? me = GetMemberExpression(expression);

        PropertyInfo? rv = null;
        if (me is not null)
        {
            var declaringType = me.Member.DeclaringType;
            if (declaringType is not null)
            {
                // Use cached properties for better performance
                var properties = PropertyMetadataCache.GetProperties(declaringType);
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
        // 只设置当前实例的直接属性，不能将嵌套或捕获对象的属性按名称映射到根对象。
        if (propertySelector.Body is not MemberExpression { Member: PropertyInfo } memberExpression ||
            memberExpression.Expression != propertySelector.Parameters[0])
        {
            return;
        }

        var cacheKey = $"{obj?.GetType().FullName}-" +
                       $"{propertySelector}-" +
                       $"{(ignoreAttributeTypes.IsNotNull() ? "-" + string.Join("-", ignoreAttributeTypes.Select(x => x.FullName)) : "")}";

        PropertyInfo? property = s_cachedObjectProperties.GetOrAdd(cacheKey, _ =>
        {
            var objType = obj?.GetType();

            if (objType == null) return null;

            // Use cached properties for better performance
            var properties = PropertyMetadataCache.GetProperties(objType);
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
