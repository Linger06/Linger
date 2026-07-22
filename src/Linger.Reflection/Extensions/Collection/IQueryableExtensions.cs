namespace Linger.Extensions.Collection;

/// <summary>
/// Provides extension methods for IQueryable for dynamic sorting.
/// </summary>
#if NET5_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Uses runtime reflection to resolve sorting property paths. This API is not compatible with trimming.")]
#endif
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
        return DynamicOrderBuilder.Apply(source, orderByPropertyName, isOrderByAsc, thenBy: false);
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
    [Obsolete("Use OrderByIf<T>(IQueryable<T>, bool, string) so query providers may return their own ordered query type.")]
    public static TQueryable OrderByIf<T, TQueryable>(this TQueryable query, bool condition, string sorting)
        where TQueryable : IQueryable<T>
    {
        ArgumentNullException.ThrowIfNull(query);
        if (!condition)
        {
            return query;
        }

        IQueryable<T> orderedQuery = query.CreateOrderBy(sorting);
        return orderedQuery is TQueryable typedQuery
            ? typedQuery
            : throw new InvalidOperationException(
                $"The query provider returned '{orderedQuery.GetType().FullName}', which cannot be converted to '{typeof(TQueryable).FullName}'. Use the IQueryable<T> OrderByIf overload instead.");
    }

    /// <summary>
    /// Conditionally applies dynamic sorting without requiring the provider to preserve its concrete query type.
    /// </summary>
    public static IQueryable<T> OrderByIf<T>(this IQueryable<T> query, bool condition, string sorting)
    {
        ArgumentNullException.ThrowIfNull(query);
        return condition ? query.CreateOrderBy(sorting) : query;
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
    public static IQueryable<T> CreateOrderBy<T>(this IQueryable<T> source, List<SortInfo>? sortList)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (sortList is { Count: > 0 })
        {
            var orderByPropertyList = new List<KeyValuePair<string, bool>>(sortList.Count);
            foreach (SortInfo sortInfo in sortList)
            {
                var propertyName = sortInfo.Property;
                ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(SortInfo.Property));
                var isAsc = sortInfo.Direction == SortDir.Asc;
                orderByPropertyList.Add(new KeyValuePair<string, bool>(propertyName, isAsc));
            }

            return source.CreateOrderBy(orderByPropertyList.ToArray());
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
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        ArgumentNullException.ThrowIfNull(orderByPropertyList);

        if (orderByPropertyList.Length == 0)
        {
            return source;
        }

        if (orderByPropertyList.Length == 1)
        {
            return source.CreateOrderBy(orderByPropertyList[0].Key, orderByPropertyList[0].Value);
        }

        IOrderedQueryable<T> orderedQueryable = DynamicOrderBuilder.Apply(
            source,
            orderByPropertyList[0].Key,
            orderByPropertyList[0].Value,
            thenBy: false);

        for (var i = 1; i < orderByPropertyList.Length; i++)
        {
            orderedQueryable = DynamicOrderBuilder.Apply(
                orderedQueryable,
                orderByPropertyList[i].Key,
                orderByPropertyList[i].Value,
                thenBy: true);
        }

        return orderedQueryable;
    }

#endif
}
