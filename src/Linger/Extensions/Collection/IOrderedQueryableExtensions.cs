#if NET5_0_OR_GREATER

using System.Linq.Expressions;

#endif

namespace Linger.Extensions.Collection;

/// <summary>
/// Provides extension methods for adding secondary sorting criteria to IOrderedQueryable.
/// </summary>
public static class IOrderedQueryableExtensions
{
#if NET5_0_OR_GREATER

    /// <summary>
    /// Adds secondary sorting criteria to an <see cref="IOrderedQueryable{T}"/> based on a list of property names and sort directions.
    /// </summary>
    /// <typeparam name="T">The type of the elements of <see cref="IOrderedQueryable{T}"/>.</typeparam>
    /// <param name="orderedQueryable">The <see cref="IOrderedQueryable{T}"/> to add the secondary sorting criteria to.</param>
    /// <param name="orderByPropertyList">A list of <see cref="KeyValuePair{TKey, TValue}"/> where the key is the property name and the value is a boolean indicating ascending (true) or descending (false) order.</param>
    /// <returns><value>An <see cref="IOrderedQueryable{T}"/> with the secondary sorting criteria applied.</value></returns>
    /// <example>
    /// <code>
    /// var orderByList = new[] { new KeyValuePair&lt;string, bool&gt;("Name", true), new KeyValuePair&lt;string, bool&gt;("Age", false) };
    /// var sortedList = myOrderedQueryable.ThenBy(orderByList);
    /// // sortedList is first sorted by Name in ascending order, then by Age in descending order
    /// </code>
    /// </example>
    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> orderedQueryable,
        params KeyValuePair<string, bool>[] orderByPropertyList) where T : class
    {
        if (orderByPropertyList.Length != 0)
        {
            Type type = typeof(T);
            ParameterExpression param = Expression.Parameter(type, type.Name);

            Expression<Func<T, object>> KeySelectorFunc(string propertyName)
            {
                MemberExpression property = Expression.Property(param, propertyName);
                UnaryExpression converted = Expression.Convert(property, typeof(object));
                return Expression.Lambda<Func<T, object>>(converted, param);
            }

            foreach (KeyValuePair<string, bool> t in orderByPropertyList)
            {
                orderedQueryable = t.Value
                    ? orderedQueryable.ThenBy(KeySelectorFunc(t.Key))
                    : orderedQueryable.ThenByDescending(KeySelectorFunc(t.Key));
            }

            return orderedQueryable;
        }

        return orderedQueryable;
    }

    /// <summary>
    /// Adds secondary sorting criteria to an <see cref="IOrderedQueryable{T}"/> based on a single property name and sort direction.
    /// </summary>
    /// <typeparam name="T">The type of the elements of <see cref="IOrderedQueryable{T}"/>.</typeparam>
    /// <param name="orderedQueryable">The <see cref="IOrderedQueryable{T}"/> to add the secondary sorting criteria to.</param>
    /// <param name="orderByPropertyName">The name of the property to sort by.</param>
    /// <param name="isOrderByAsc">A boolean indicating whether to sort in ascending (true) or descending (false) order. Default is true.</param>
    /// <returns><value>An <see cref="IOrderedQueryable{T}"/> with the secondary sorting criteria applied.</value></returns>
    /// <example>
    /// <code>
    /// var sortedList = myOrderedQueryable.ThenBy("Name", true);
    /// // sortedList is sorted by Name in ascending order
    /// </code>
    /// </example>
    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> orderedQueryable, string orderByPropertyName,
        bool isOrderByAsc = true) where T : class
    {
        Type type = typeof(T);
        ParameterExpression param = Expression.Parameter(type, type.Name);
        MemberExpression body = Expression.Property(param, orderByPropertyName);
        var keySelector = Expression.Lambda<Func<T, object>>(Expression.Convert(body, typeof(object)), param);

        return isOrderByAsc
            ? orderedQueryable.ThenBy(keySelector)
            : orderedQueryable.ThenByDescending(keySelector);
    }

#endif
}