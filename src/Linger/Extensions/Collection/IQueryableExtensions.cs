using System.Linq.Expressions;
using System.Reflection;

#if NET5_0_OR_GREATER

using Linger.Extensions.Core;
using Linger.Helper;

#endif

namespace Linger.Extensions.Collection;

/// <summary>
/// Provides extension methods for IQueryable for dynamic sorting.
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// Dynamically creates an OrderBy or OrderByDescending expression based on the specified property name.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">An IQueryable to sort.</param>
    /// <param name="orderByPropertyName">The name of the property to sort by.</param>
    /// <param name="isOrderByAsc">Specifies whether to sort in ascending order. Default is true.</param>
    /// <returns><value>An IQueryable whose elements are sorted according to a key.</value></returns>
    /// <example>
    /// <code>
    /// var sortedList = myQueryable.CreateOrderBy("Name", true);
    /// // sortedList is sorted by Name in ascending order
    /// </code>
    /// </example>
    public static IQueryable<T> CreateOrderBy<T>(this IQueryable<T> source, string orderByPropertyName,
        bool isOrderByAsc = true)
    {
        var command = isOrderByAsc ? "OrderBy" : "OrderByDescending";
        Type type = typeof(T);
        PropertyInfo? property = type.GetProperty(orderByPropertyName);
        if (property == null)
        {
            throw new ArgumentException($"Cannot find Property:{nameof(orderByPropertyName)} in {nameof(T)}");
        }

        ParameterExpression parameter = Expression.Parameter(type, "p");
        MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, property);
        LambdaExpression orderByExpression = Expression.Lambda(propertyAccess, parameter);
        MethodCallExpression resultExpression = Expression.Call(typeof(Queryable), command,
            [type, property.PropertyType],
            source.Expression, Expression.Quote(orderByExpression));
        return source.Provider.CreateQuery<T>(resultExpression);
    }

    /// <summary>
    /// Conditionally applies an OrderBy expression to the IQueryable.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <typeparam name="TQueryable">The type of the IQueryable.</typeparam>
    /// <param name="query">An IQueryable to sort.</param>
    /// <param name="condition">A boolean value to determine whether to apply the sorting.</param>
    /// <param name="sorting">The name of the property to sort by.</param>
    /// <returns><value>An IQueryable whose elements are sorted according to a key if the condition is true; otherwise, the original IQueryable.</value></returns>
    /// <example>
    /// <code>
    /// var sortedList = myQueryable.OrderByIf(true, "Name");
    /// // sortedList is sorted by Name
    /// </code>
    /// </example>
    public static TQueryable OrderByIf<T, TQueryable>(this TQueryable query, bool condition, string sorting)
        where TQueryable : IQueryable<T>
    {
        return condition
            ? (TQueryable)query.CreateOrderBy(sorting)
            : query;
    }

#if NET5_0_OR_GREATER

    /// <summary>
    /// Dynamically creates an OrderBy expression based on a list of SortInfo.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">An IQueryable to sort.</param>
    /// <param name="sortList">A list of SortInfo containing property names and sort directions.</param>
    /// <returns><value>An IQueryable whose elements are sorted according to the specified sort list.</value></returns>
    /// <example>
    /// <code>
    /// var sortList = new List&lt;SortInfo&gt; { new SortInfo("Name", SortDir.Asc) };
    /// var sortedList = myQueryable.CreateOrderBy(sortList);
    /// // sortedList is sorted by Name in ascending order
    /// </code>
    /// </example>
    public static IQueryable<T> CreateOrderBy<T>(this IQueryable<T> source, List<SortInfo> sortList)
    {
        if (sortList.IsNotNull())
        {
            if (sortList.Count != 0)
            {
                var orderByPropertyList = new List<KeyValuePair<string, bool>>();
                foreach (SortInfo sortInfo in sortList)
                {
                    var propertyName = sortInfo.Property;
                    var isAsc = sortInfo.Direction == SortDir.Asc;
                    orderByPropertyList.Add(new KeyValuePair<string, bool>(propertyName, isAsc));
                }
                return source.CreateOrderBy(orderByPropertyList.ToArray());
            }
        }

        return source;
    }

    /// <summary>
    /// Dynamically creates an OrderBy expression based on an array of property names and sort directions.
    /// </summary>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    /// <param name="source">An IQueryable to sort.</param>
    /// <param name="orderByPropertyList">An array of KeyValuePair containing property names and sort directions.</param>
    /// <returns><value>An IQueryable whose elements are sorted according to the specified property list.</value></returns>
    /// <example>
    /// <code>
    /// var orderByList = new[] { new KeyValuePair&lt;string, bool&gt;("Name", true) };
    /// var sortedList = myQueryable.CreateOrderBy(orderByList);
    /// // sortedList is sorted by Name in ascending order
    /// </code>
    /// </example>
    public static IQueryable<T> CreateOrderBy<T>(this IQueryable<T> source,
        params KeyValuePair<string, bool>[] orderByPropertyList)
    {
        orderByPropertyList.EnsureIsNotNull();

        if (orderByPropertyList.Length == 0)
        {
            throw new ArgumentException("The length of params cannot be zero", nameof(orderByPropertyList));
        }

        if (orderByPropertyList.Length == 1)
        {
            return source.CreateOrderBy(orderByPropertyList[0].Key, orderByPropertyList[0].Value);
        }

        Type type = typeof(T);
        ParameterExpression param = Expression.Parameter(type, type.Name);

        Expression<Func<T, object>> KeySelectorFunc(string propertyName)
        {
            propertyName.EnsureStringIsNotNullAndEmpty(message: "The parameter cannot be null or empty");
            MemberExpression property = Expression.Property(param, propertyName);
            UnaryExpression converted = Expression.Convert(property, typeof(object));
            return Expression.Lambda<Func<T, object>>(converted, param);
        }

        IOrderedQueryable<T> orderedQueryable = orderByPropertyList[0].Value
            ? source.OrderBy(KeySelectorFunc(orderByPropertyList[0].Key))
            : source.OrderByDescending(KeySelectorFunc(orderByPropertyList[0].Key));
        for (var i = 1; i < orderByPropertyList.Length; i++)
        {
            orderedQueryable = orderByPropertyList[i].Value
                ? orderedQueryable.ThenBy(KeySelectorFunc(orderByPropertyList[i].Key))
                : orderedQueryable.ThenByDescending(KeySelectorFunc(orderByPropertyList[i].Key));
        }

        return orderedQueryable;
    }

#endif
}
