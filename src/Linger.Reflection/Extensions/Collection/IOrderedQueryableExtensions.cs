namespace Linger.Extensions.Collection;

/// <summary>
/// Provides extension methods for adding secondary sorting criteria to IOrderedQueryable.
/// </summary>
#if NET5_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Uses runtime reflection to resolve sorting property paths. This API is not compatible with trimming.")]
#endif
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
        params KeyValuePair<string, bool>[] orderByPropertyList)
    {
        ArgumentNullException.ThrowIfNull(orderedQueryable);
        ArgumentNullException.ThrowIfNull(orderByPropertyList);

        if (orderByPropertyList.Length != 0)
        {
            foreach (KeyValuePair<string, bool> t in orderByPropertyList)
            {
                orderedQueryable = DynamicOrderBuilder.Apply(orderedQueryable, t.Key, t.Value, thenBy: true);
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
        bool isOrderByAsc = true)
    {
        return DynamicOrderBuilder.Apply(orderedQueryable, orderByPropertyName, isOrderByAsc, thenBy: true);
    }

#endif
}
