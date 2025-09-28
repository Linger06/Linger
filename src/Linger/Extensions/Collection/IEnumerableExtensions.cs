using System.Reflection;
using Linger.Extensions.Core;

namespace Linger.Extensions.Collection;

/// <summary>
/// Provides extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
public static partial class IEnumerableExtensions
{
    /// <summary>
    /// Executes a given <see cref="Action"/> for each entry in the <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> on whose elements the action is to be performed.</param>
    /// <param name="action">The <see cref="Action"/> delegate to perform on each element of the <see cref="IEnumerable{T}"/>.</param>
    /// <example>
    /// <code>
    /// var list = new List&lt;int&gt; { 1, 2, 3 };
    /// list.ForEach(Console.WriteLine);
    /// // Output: 1 2 3
    /// </code>
    /// </example>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (T item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes a given <see cref="Action"/> for each entry in the <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> on whose elements the action is to be performed.</param>
    /// <param name="action">The <see cref="Action"/> delegate to perform on each element of the <see cref="IEnumerable{T}"/>, with the element's index.</param>
    /// <example>
    /// <code>
    /// var list = new List&lt;int&gt; { 1, 2, 3 };
    /// list.ForEach((item, index) => Console.WriteLine($"{index}: {item}"));
    /// // Output: 0: 1 1: 2 2: 3
    /// </code>
    /// </example>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        var index = 0;
        foreach (T item in source)
        {
            action(item, index);
            index++;
        }
    }

#if !NETFRAMEWORK || NET462_OR_GREATER

    /// <summary>
    /// Check if the <see cref="CustomAttributeData"/> enumerable contains the specified <see cref="Attribute"/>.
    /// </summary>
    /// <param name="customs">The source enumerable of <see cref="CustomAttributeData"/>.</param>
    /// <param name="type">The type of the <see cref="Attribute"/> to check for.</param>
    /// <returns><c>true</c> if the specified <see cref="Attribute"/> is found; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// var attributes = typeof(MyClass).CustomAttributes;
    /// bool hasAttribute = attributes.HasAttribute(typeof(MyAttribute));
    /// // Output: true or false
    /// </code>
    /// </example>
    public static bool HasAttribute(this IEnumerable<CustomAttributeData> customs, Type type)
    {
        ArgumentNullException.ThrowIfNull(customs);
        ArgumentNullException.ThrowIfNull(type);

        return customs.Any(a => a.AttributeType == type);
    }

#endif

    /// <summary>
    /// Converts an <see cref="IEnumerable{T}"/> to a <see cref="DataTable"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="recordList">The source enumerable.</param>
    /// <param name="actionColumn">An optional action to perform on each <see cref="DataColumn"/>.</param>
    /// <param name="actionRow">An optional action to perform on each <see cref="DataRow"/>.</param>
    /// <returns>A <see cref="DataTable"/> that contains the elements of the source enumerable.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="recordList"/> is null.</exception>
    /// <example>
    /// <code>
    /// var list = new List&lt;Person&gt; { new Person { Id = 1, Name = "John" }, new Person { Id = 2, Name = "Jane" } };
    /// DataTable dt = list.ToDataTable();
    /// // Output: DataTable with columns "Id" and "Name" and rows [{1, "John"}, {2, "Jane"}]
    /// </code>
    /// </example>
    public static DataTable ToDataTable<T>(this IEnumerable<T> recordList, Action<DataColumn, ColumnInfo>? actionColumn = null, Action<DataRow, ColumnInfo, T>? actionRow = null) // where T : new()
    {
        ArgumentNullException.ThrowIfNull(recordList);

        Type type = typeof(T);
        var columns = type.GetColumnsInfo();
        DataTable dt = new DataTable()
            .AddColumns(columns, actionColumn)
            .AddRows(columns, recordList, actionRow);
        return dt;
    }

    /// <summary>
    /// Adds columns to the <see cref="DataTable"/> based on the specified column information.
    /// </summary>
    /// <param name="dt">The <see cref="DataTable"/> to add columns to.</param>
    /// <param name="columns">The column information.</param>
    /// <param name="action">An optional action to perform on each <see cref="DataColumn"/>.</param>
    /// <returns>The <see cref="DataTable"/> with the added columns.</returns>
    /// <example>
    /// <code>
    /// var dt = new DataTable();
    /// var columns = new List&lt;ColumnInfo&gt; { new ColumnInfo { PropertyName = "Id", Property = typeof(Person).GetProperty("Id") } };
    /// dt.AddColumns(columns);
    /// // Output: DataTable with column "Id"
    /// </code>
    /// </example>
    private static DataTable AddColumns(this DataTable dt, IEnumerable<ColumnInfo> columns, Action<DataColumn, ColumnInfo>? action = null)
    {
        foreach (ColumnInfo column in columns)
        {
            Type type = column.Property.PropertyType;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }

            var dataColumn = new DataColumn(column.PropertyName, type);
            action?.Invoke(dataColumn, column);

            dt.Columns.Add(dataColumn);
        }

        return dt;
    }

    /// <summary>
    /// Adds rows to the <see cref="DataTable"/> based on the specified column information and source enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source enumerable.</typeparam>
    /// <param name="dt">The <see cref="DataTable"/> to add rows to.</param>
    /// <param name="columns">The column information.</param>
    /// <param name="list">The source enumerable.</param>
    /// <param name="action">An optional action to perform on each <see cref="DataRow"/>.</param>
    /// <returns>The <see cref="DataTable"/> with the added rows.</returns>
    /// <example>
    /// <code>
    /// var dt = new DataTable();
    /// var columns = new List&lt;ColumnInfo&gt; { new ColumnInfo { PropertyName = "Id", Property = typeof(Person).GetProperty("Id") } };
    /// var list = new List&lt;Person&gt; { new Person { Id = 1 }, new Person { Id = 2 } };
    /// dt.AddRows(columns, list);
    /// // Output: DataTable with rows [{1}, {2}]
    /// </code>
    /// </example>
    private static DataTable AddRows<T>(this DataTable dt, IEnumerable<ColumnInfo> columns, IEnumerable<T> list, Action<DataRow, ColumnInfo, T>? action = null) //where T : new()
    {
        foreach (T item in list)
        {
            DataRow row = dt.NewRow();
            foreach (ColumnInfo column in columns)
            {
                var value = column.Property.GetValue(item, null);
                row[column.PropertyName] = value ?? DBNull.Value;
                action?.Invoke(row, column, item);
            }

            dt.Rows.Add(row);
        }

        return dt;
    }

    /// <summary>
    /// Converts the <see cref="IEnumerable{T}"/> to a string, with each element separated by the specified separator.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="self">The enumerable to convert.</param>
    /// <param name="separator">The separator to use between elements.</param>
    /// <param name="format">A function to format each element. If not provided, ToString() will be used.</param>
    /// <returns>A string representation of the enumerable with elements separated by the specified separator.</returns>
    /// <exception cref="ArgumentNullException">Thrown when separator is null.</exception>
    /// <example>
    /// <code>
    /// var list = new List&lt;int?&gt; { 1, null, 3 };
    /// // Using custom format
    /// string result1 = list.ToSeparatedString(", ", x => x?.ToString() ?? "N/A");
    /// // Output: "1, N/A, 3"
    /// 
    /// // Using default format
    /// string result2 = list.ToSeparatedString(", ");
    /// // Output: "1, , 3"
    /// </code>
    /// </example>
    public static string ToSeparatedString<T>(this IEnumerable<T>? self, string separator = ",", Func<T, string>? format = null)
    {
        ArgumentNullException.ThrowIfNull(separator);
        if (self == null) return string.Empty;

        var formattedList = self.Select(item => format?.Invoke(item) ?? item?.ToString() ?? string.Empty);
        return string.Join(separator, formattedList);
    }

    /// <summary>
    /// Determines whether the enumerable is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="query">The source enumerable.</param>
    /// <returns><c>true</c> if the enumerable is null or empty; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// var list = new List&lt;int&gt;();
    /// bool result = list.IsNullOrEmpty();
    /// // Output: true
    /// </code>
    /// </example>
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? query)
    {
        if (query is null)
            return true;

        // Fast-paths for collections
        if (query is ICollection<T> c)
            return c.Count == 0;
        if (query is IReadOnlyCollection<T> rc)
            return rc.Count == 0;

        using var e = query.GetEnumerator();
        return !e.MoveNext();
    }

    /// <summary>
    /// Performs a full outer join on two sequences.
    /// </summary>
    /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
    /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="outer">The first sequence to join.</param>
    /// <param name="inner">The second sequence to join.</param>
    /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
    /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains elements of type <typeparamref name="TResult"/> that are obtained by performing a full outer join on two sequences.</returns>
    /// <remarks>
    /// The result contains rows for keys that appear in either sequence. Where no match exists, the corresponding side is <c>null</c>.
    /// Duplicate keys produce multiple rows.
    /// </remarks>
    /// <example>
    /// <code>
    /// var outer = new List&lt;int&gt; { 1, 2, 3 };
    /// var inner = new List&lt;int&gt; { 3, 4, 5 };
    /// var result = outer.FullJoin(inner, o => o, i => i, (o, i) => new { Outer = o, Inner = i });
    /// // Output: [{ Outer = 1, Inner = null }, { Outer = 2, Inner = null }, { Outer = 3, Inner = 3 }, { Outer = null, Inner = 4 }, { Outer = null, Inner = 5 }]
    ///
    /// // Duplicate keys
    /// var i2 = new List&lt;int&gt; { 2, 2 };
    /// var all = new List&lt;int&gt; { 2 }.FullJoin(i2, x => x, y => y, (x, y) => (x, y)).ToList();
    /// // Output: [(2, 2), (2, 2)]
    /// </code>
    /// </example>
    public static IEnumerable<TResult> FullJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter?, TInner?, TResult> resultSelector)
    {
        //IEnumerable<TResult>? leftResult = LeftOuterJoin(left, right, leftKey, rightKey, result);
        //IEnumerable<TResult>? rightResult = RightOuterJoin(left, right, leftKey, rightKey, result);
        //return leftResult.Union(rightResult);

        var outerLookup = outer.ToLookup(outerKeySelector);
        var innerLookup = inner.ToLookup(innerKeySelector);

        var keys = new HashSet<TKey>(outerLookup.Select(p => p.Key));
        keys.UnionWith(innerLookup.Select(p => p.Key));

        IEnumerable<TResult> result = from key in keys
                                      from xOuter in outerLookup[key].DefaultIfEmpty()
                                      from xInner in innerLookup[key].DefaultIfEmpty()
                                      select resultSelector(xOuter, xInner);

        return result;
    }

    /// <summary>
    /// Paginates the <see cref="IEnumerable{T}"/> based on the specified page index and page size.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to paginate.</param>
    /// <param name="pageIndex">The index of the page to retrieve (1-based).</param>
    /// <param name="pageSize">The size of the page to retrieve.</param>
    /// <returns>A paginated <see cref="IEnumerable{T}"/>.</returns>
    /// <remarks>
    /// When <paramref name="source"/> is null, or <paramref name="pageIndex"/> &lt;= 0, or <paramref name="pageSize"/> &lt;= 0,
    /// this method returns an empty sequence.
    /// </remarks>
    /// <example>
    /// <code>
    /// var enumerable = new[] { 1, 2, 3, 4, 5 };
    /// var result = enumerable.Paging(2, 2);
    /// // Output: [3, 4]
    ///
    /// // boundary cases
    /// var empty1 = enumerable.Paging(0, 2);     // [] (pageIndex <= 0)
    /// var empty2 = enumerable.Paging(1, 0);     // [] (pageSize <= 0)
    /// IEnumerable<int>? none = null;
    /// var empty3 = none.Paging(1, 10);          // [] (null source)
    /// </code>
    /// </example>
    public static IEnumerable<T> Paging<T>(this IEnumerable<T>? source, int pageIndex, int pageSize)
    {
        if (source is null)
            return [];

        if (pageIndex <= 0)
            return [];

        if (pageSize <= 0)
            return [];

        return source.Skip((pageIndex - 1) * pageSize).Take(pageSize);
    }

    /// <summary>
    /// Returns an <see cref="ICollection{T}"/> view for the given sequence.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    /// <param name="source">Source sequence.</param>
    /// <returns>
    /// If the source already implements <see cref="ICollection{T}"/>, it's returned directly;
    /// otherwise a <see cref="List{T}"/> is materialized. When source is null, returns <see cref="Array.Empty{T}"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// IEnumerable<int> it = Enumerable.Range(1, 3).Where(x => x > 1);
    /// ICollection<int> col = it.ToCollection(); // materialized as List<int>
    /// IReadOnlyCollection<int> empty = ((IEnumerable<int>?)null).ToCollection(); // Array.Empty<int>()
    /// </code>
    /// </example>
    public static ICollection<T> ToCollection<T>(this IEnumerable<T>? source)
    {
        if (source is null)
            return Array.Empty<T>();

        // Avoid invalid cast for non-collection enumerables (e.g., LINQ WhereIterator)
        if (source is ICollection<T> collection)
            return collection;

        // Materialize once into a List<T> to provide ICollection semantics
        return source.ToList();
    }
}
