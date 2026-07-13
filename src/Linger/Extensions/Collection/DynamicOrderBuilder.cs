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
        PropertyInfo? property = PropertyMetadataCache
            .GetProperties(type, BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(candidate => candidate.Name == propertyName);

        if (property is null)
        {
            throw new ArgumentException(
                $"Property '{propertyName}' was not found on type '{type.FullName}'.",
                nameof(propertyName));
        }

        ParameterExpression parameter = Expression.Parameter(type, "item");
        MemberExpression propertyAccess = Expression.Property(parameter, property);
        LambdaExpression keySelector = Expression.Lambda(propertyAccess, parameter);
        string methodName = thenBy
            ? ascending ? nameof(Queryable.ThenBy) : nameof(Queryable.ThenByDescending)
            : ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);

        MethodCallExpression call = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { type, property.PropertyType },
            source.Expression,
            Expression.Quote(keySelector));

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
    }
}
