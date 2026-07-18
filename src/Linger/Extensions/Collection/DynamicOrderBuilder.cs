using System.Linq.Expressions;
using System.Reflection;
using Linger.Helper;

namespace Linger.Extensions.Collection;

internal static class DynamicOrderBuilder
{
    public static IOrderedQueryable<T> Apply<T>(
        IQueryable<T> source,
        string propertyName,
        bool ascending,
        bool thenBy)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        Type type = typeof(T);
        PropertyInfo[] propertyPath = ResolvePropertyPath(type, propertyName);

        ParameterExpression parameter = Expression.Parameter(type, "item");
        Expression propertyAccess = parameter;
        foreach (PropertyInfo property in propertyPath)
        {
            propertyAccess = Expression.Property(propertyAccess, property);
        }

        LambdaExpression keySelector = Expression.Lambda(propertyAccess, parameter);
        Type propertyType = propertyPath[propertyPath.Length - 1].PropertyType;
        string methodName = thenBy
            ? ascending ? nameof(Queryable.ThenBy) : nameof(Queryable.ThenByDescending)
            : ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);

        MethodCallExpression call = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { type, propertyType },
            source.Expression,
            Expression.Quote(keySelector));

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
    }

    internal static void ValidatePropertyPath<T>(string propertyName)
    {
        _ = ResolvePropertyPath(typeof(T), propertyName);
    }

    private static PropertyInfo[] ResolvePropertyPath(Type rootType, string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        string[] segments = propertyName.Split('.');
        var properties = new PropertyInfo[segments.Length];
        Type currentType = rootType;

        for (var i = 0; i < segments.Length; i++)
        {
            IReadOnlyDictionary<string, PropertyInfo> propertyMap = PropertyMetadataCache.GetPropertyMap(
                currentType,
                ignoreCase: true,
                flags: BindingFlags.Public | BindingFlags.Instance);

            if (!propertyMap.TryGetValue(segments[i], out PropertyInfo? property))
            {
                throw new ArgumentException(
                    $"Property path '{propertyName}' was not found on type '{rootType.FullName}'.",
                    nameof(propertyName));
            }

            properties[i] = property;
            currentType = property.PropertyType;
        }

        return properties;
    }
}
