using System.Reflection;
using Linger.Enums;
using Linger.Extensions.Collection;

namespace Linger.Helper;

public static partial class ExpressionHelper
{
    /// <summary>  
    /// Generates a function to order a queryable collection based on a list of sort information.  
    /// </summary>  
    /// <typeparam name="T">The type of the elements in the queryable collection.</typeparam>  
    /// <param name="sortList">A list of <see cref="SortInfo"/> objects containing the sorting information.</param>  
    /// <returns>A function that orders a queryable collection, or null if the sort list is null or empty.</returns>  
    /// <example>  
    /// <code>  
    /// var sortList = new List&lt;SortInfo&gt; { new SortInfo { Property = "Name", Direction = SortDirection.Ascending } };  
    /// var orderByFunc = ExpressionHelper.GetOrderBy&lt;MyClass&gt;(sortList);  
    /// var orderedQueryable = orderByFunc(myQueryable);  
    /// </code>  
    /// </example>  
    public static Func<IQueryable<T>, IOrderedQueryable<T>>? GetOrderBy<T>(List<SortInfo>? sortList)
    {
        if (sortList is null)
        {
            return null;
        }

        if (sortList.Count == 0)
        {
            return null;
        }

        var propertyList = new List<string>();
        var dirList = new List<string>();
        foreach (SortInfo sortInfo in sortList)
        {
            var propertyName = sortInfo.Property;
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(SortInfo.Property));
            var dir = sortInfo.Direction.ToString();
            propertyList.Add(propertyName);
            dirList.Add(dir);
        }

        return GetOrderBy<T>(propertyList, dirList);
    }

    /// <summary>  
    /// Generates a function to order a queryable collection based on specified columns and directions.  
    /// </summary>  
    /// <typeparam name="T">The type of the elements in the queryable collection.</typeparam>  
    /// <param name="orderColumn">A list of column names to sort by.</param>  
    /// <param name="orderDir">A list of sort directions corresponding to the columns.</param>  
    /// <returns>A function that orders a queryable collection.</returns>  
    /// <example>  
    /// <code>  
    /// var orderColumns = new List&lt;string&gt; { "Name", "Age" };  
    /// var orderDirs = new List&lt;string&gt; { "asc", "desc" };  
    /// var orderByFunc = ExpressionHelper.GetOrderBy&lt;MyClass&gt;(orderColumns, orderDirs);  
    /// var orderedQueryable = orderByFunc(myQueryable);  
    /// </code>  
    /// </example>  
    public static Func<IQueryable<T>, IOrderedQueryable<T>>? GetOrderBy<T>(List<string> orderColumn, List<string> orderDir)
    {
        ArgumentNullException.ThrowIfNull(orderColumn);
        ArgumentNullException.ThrowIfNull(orderDir);

        if (orderColumn.Count != orderDir.Count)
        {
            throw new ArgumentException($"{nameof(orderColumn)} and {nameof(orderDir)} must have the same number of elements.");
        }

        if (orderColumn.Count == 0)
        {
            return null;
        }

        var orderings = new KeyValuePair<string, bool>[orderColumn.Count];

        for (var i = 0; i < orderColumn.Count; i++)
        {
            var columnName = orderColumn[i];
            try
            {
                DynamicOrderBuilder.ValidatePropertyPath<T>(columnName);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException(nameof(PropertyInfo), ex);
            }

            bool ascending = GetSortDirection(orderDir[i], nameof(orderDir));
            orderings[i] = new KeyValuePair<string, bool>(columnName, ascending);
        }

#if NET5_0_OR_GREATER
        return query => (IOrderedQueryable<T>)query.CreateOrderBy(orderings);
#else
        return query =>
        {
            IOrderedQueryable<T> orderedQuery = DynamicOrderBuilder.Apply(
                query,
                orderings[0].Key,
                orderings[0].Value,
                thenBy: false);

            for (var i = 1; i < orderings.Length; i++)
            {
                orderedQuery = DynamicOrderBuilder.Apply(
                    orderedQuery,
                    orderings[i].Key,
                    orderings[i].Value,
                    thenBy: true);
            }

            return orderedQuery;
        };
#endif
    }

}
