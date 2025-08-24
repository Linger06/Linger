using System.Reflection;
using Linger.Extensions.Core;

namespace Linger.Extensions.Collection;

/// <summary>
/// Provides extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
public static class IEnumerableExtensions
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

#if !NET6_0_OR_GREATER

    /// <summary>
    /// Returns distinct elements from a sequence by using a specified key selector.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function to extract the key for each element.
    /// x =&gt; new { x.PurchaseOrder, x.BusinessPartner, x.Buyer, x.Currency }
    ///</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains distinct elements from the source sequence.</returns>
    /// <remarks>
    /// This method keeps the first occurrence for each key as it iterates the <paramref name="source"/>.
    /// Throws <see cref="ArgumentNullException"/> when <paramref name="source"/> or <paramref name="keySelector"/> is null.
    /// </remarks>
    /// <example>
    /// <code>
    /// var list = new List&lt;Person&gt; { new Person { Id = 1 }, new Person { Id = 1 }, new Person { Id = 2 } };
    /// var distinctList = list.DistinctBy(p => p.Id);
    /// // Output: [{ Id = 1 }, { Id = 2 }]
    /// </code>
    /// </example>
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
    {
        if (source == null) throw new System.ArgumentNullException(nameof(source));
        if (keySelector == null) throw new System.ArgumentNullException(nameof(keySelector));

        var hash = new HashSet<TKey>();
        return source.Where(p => hash.Add(keySelector(p)));
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
    /// Performs a left outer join on two sequences.
    /// </summary>
    /// <typeparam name="TLeft">The type of the elements of the first sequence.</typeparam>
    /// <typeparam name="TRight">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="left">The first sequence to join.</param>
    /// <param name="right">The second sequence to join.</param>
    /// <param name="leftKey">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="rightKey">A function to extract the join key from each element of the second sequence.</param>
    /// <param name="result">A function to create a result element from two matching elements.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains elements of type <typeparamref name="TResult"/> that are obtained by performing a left outer join on two sequences.</returns>
    /// <remarks>
    /// When no <paramref name="right"/> element matches a <paramref name="left"/> element by key, the result selector receives <c>null</c> for the right value.
    /// If any argument is <c>null</c>, an exception will be thrown.
    /// </remarks>
    /// <example>
    /// <code>
    /// var left = new List&lt;int&gt; { 1, 2, 3 };
    /// var right = new List&lt;int&gt; { 3, 4, 5 };
    /// var result = left.LeftOuterJoin(right, l => l, r => r, (l, r) => new { Left = l, Right = r });
    /// // Output: [{ Left = 1, Right = null }, { Left = 2, Right = null }, { Left = 3, Right = 3 }]
    ///
    /// // Duplicate key on right results in multiple rows for the same left key
    /// var rightDup = new List&lt;int&gt; { 2, 2 };
    /// var r2 = new List&lt;int&gt; { 1, 2 }.LeftOuterJoin(rightDup, l => l, r => r, (l, r) => (l, r)).ToList();
    /// // Output: [(1, null), (2, 2), (2, 2)]
    /// </code>
    /// </example>
    public static IEnumerable<TResult> LeftOuterJoin<TLeft, TRight, TKey, TResult>(
        this IEnumerable<TLeft> left,
        IEnumerable<TRight> right,
        Func<TLeft, TKey> leftKey,
        Func<TRight, TKey> rightKey,
        Func<TLeft, TRight?, TResult> result)
    {
        return left.GroupJoin(right, leftKey, rightKey, (l, r) => new { l, r })
            .SelectMany(o => o.r.DefaultIfEmpty(), (l, r) => new { lft = l.l, rght = r })
            .Select(o => result(o.lft, o.rght));
    }

    /// <summary>
    /// Performs a left outer join on two sequences and returns a sequence of tuples.
    /// </summary>
    /// <typeparam name="TLeft">The type of the elements of the first sequence.</typeparam>
    /// <typeparam name="TRight">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector functions.</typeparam>
    /// <param name="left">The first sequence to join.</param>
    /// <param name="right">The second sequence to join.</param>
    /// <param name="leftKey">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="rightKey">A function to extract the join key from each element of the second sequence.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains tuples of elements from the first and second sequences.</returns>
    /// <remarks>
    /// Same semantics as the selector-overload, with the right value being <c>null</c> when no match exists.
    /// </remarks>
    /// <example>
    /// <code>
    /// var left = new List&lt;int&gt; { 1, 2, 3 };
    /// var right = new List&lt;int&gt; { 3, 4, 5 };
    /// var result = left.LeftOuterJoin(right, l => l, r => r);
    /// // Output: [(1, null), (2, null), (3, 3)]
    /// </code>
    /// </example>
    public static IEnumerable<Tuple<TLeft, TRight?>> LeftOuterJoin<TLeft, TRight, TKey>(
        this IEnumerable<TLeft> left,
        IEnumerable<TRight> right,
        Func<TLeft, TKey> leftKey,
        Func<TRight, TKey> rightKey)
    {
        return LeftOuterJoin(left, right, leftKey, rightKey, (l, r) => new Tuple<TLeft, TRight?>(l, r));
    }

    /// <summary>
    /// Performs a right outer join on two sequences.
    /// </summary>
    /// <typeparam name="TLeft">The type of the elements of the first sequence.</typeparam>
    /// <typeparam name="TRight">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="left">The first sequence to join.</param>
    /// <param name="right">The second sequence to join.</param>
    /// <param name="leftKey">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="rightKey">A function to extract the join key from each element of the second sequence.</param>
    /// <param name="resultFunc">A function to create a result element from two matching elements.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains elements of type <typeparamref name="TResult"/> that are obtained by performing a right outer join on two sequences.</returns>
    /// <remarks>
    /// When no <paramref name="left"/> element matches a <paramref name="right"/> element by key, the result selector receives <c>null</c> for the left value.
    /// If any argument is <c>null</c>, an exception will be thrown.
    /// </remarks>
    /// <example>
    /// <code>
    /// var left = new List&lt;int&gt; { 1, 2, 3 };
    /// var right = new List&lt;int&gt; { 3, 4, 5 };
    /// var result = left.RightOuterJoin(right, l => l, r => r, (l, r) => new { Left = l, Right = r });
    /// // Output: [{ Left = null, Right = 4 }, { Left = null, Right = 5 }, { Left = 3, Right = 3 }]
    ///
    /// // Duplicate key on left results in multiple rows for the same right key
    /// var leftDup = new List&lt;int&gt; { 2, 2 };
    /// var r2 = leftDup.RightOuterJoin(new List&lt;int&gt; { 2 }, l => l, r => r, (l, r) => (l, r)).ToList();
    /// // Output: [(2, 2), (2, 2)]
    /// </code>
    /// </example>
    public static IEnumerable<TResult> RightOuterJoin<TLeft, TRight, TKey, TResult>(
            this IEnumerable<TLeft> left,
            IEnumerable<TRight> right,
            Func<TLeft, TKey> leftKey,
            Func<TRight, TKey> rightKey,
            Func<TLeft?, TRight, TResult> resultFunc
        )
    {
        IEnumerable<TResult> query = LeftOuterJoin(right, left, rightKey, leftKey, (i, o) => resultFunc(o, i));
        return query;
    }

    /// <summary>
    /// Performs an inner join on two sequences.
    /// </summary>
    /// <typeparam name="TLeft">The type of the elements of the first sequence.</typeparam>
    /// <typeparam name="TRight">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="left">The first sequence to join.</param>
    /// <param name="right">The second sequence to join.</param>
    /// <param name="leftKey">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="rightKey">A function to extract the join key from each element of the second sequence.</param>
    /// <param name="resultFunc">A function to create a result element from two matching elements.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains elements of type <typeparamref name="TResult"/> that are obtained by performing an inner join on two sequences.</returns>
    /// <remarks>
    /// Only pairs with matching keys appear in the results. Duplicate keys will produce multiple rows (Cartesian product per key).
    /// </remarks>
    /// <example>
    /// <code>
    /// var left = new List&lt;int&gt; { 1, 2, 3 };
    /// var right = new List&lt;int&gt; { 3, 4, 5 };
    /// var result = left.InnerJoin(right, l => l, r => r, (l, r) => new { Left = l, Right = r });
    /// // Output: [{ Left = 3, Right = 3 }]
    ///
    /// // Duplicate keys
    /// var l2 = new List&lt;int&gt; { 2, 2 };
    /// var r2 = new List&lt;int&gt; { 2, 2 };
    /// var dup = l2.InnerJoin(r2, x => x, y => y, (x, y) => (x, y)).ToList();
    /// // Output count: 4
    /// </code>
    /// </example>
    public static IEnumerable<TResult> InnerJoin<TLeft, TRight, TKey, TResult>(
            this IEnumerable<TLeft> left,
            IEnumerable<TRight> right,
            Func<TLeft, TKey> leftKey,
            Func<TRight, TKey> rightKey,
            Func<TLeft, TRight, TResult> resultFunc
        )
    {
        IEnumerable<TResult> result = LeftOuterJoin(left, right, leftKey, rightKey)
                .Where(a => a.Item2 != null)
                .Select(a => resultFunc(a.Item1, a.Item2!))
            ;
        return result;
    }

    /// <summary>
    /// Performs a full outer join on two sequences.
    /// </summary>
    /// <typeparam name="TLeft">The type of the elements of the first sequence.</typeparam>
    /// <typeparam name="TRight">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="left">The first sequence to join.</param>
    /// <param name="right">The second sequence to join.</param>
    /// <param name="leftKey">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="rightKey">A function to extract the join key from each element of the second sequence.</param>
    /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains elements of type <typeparamref name="TResult"/> that are obtained by performing a full outer join on two sequences.</returns>
    /// <remarks>
    /// The result contains rows for keys that appear in either sequence. Where no match exists, the corresponding side is <c>null</c>.
    /// Duplicate keys produce multiple rows.
    /// </remarks>
    /// <example>
    /// <code>
    /// var left = new List&lt;int&gt; { 1, 2, 3 };
    /// var right = new List&lt;int&gt; { 3, 4, 5 };
    /// var result = left.FullOuterJoin(right, l => l, r => r, (l, r) => new { Left = l, Right = r });
    /// // Output: [{ Left = 1, Right = null }, { Left = 2, Right = null }, { Left = 3, Right = 3 }, { Left = null, Right = 4 }, { Left = null, Right = 5 }]
    ///
    /// // Duplicate keys
    /// var r2 = new List&lt;int&gt; { 2, 2 };
    /// var all = new List&lt;int&gt; { 2 }.FullOuterJoin(r2, x => x, y => y, (x, y) => (x, y)).ToList();
    /// // Output: [(2, 2), (2, 2)]
    /// </code>
    /// </example>
    public static IEnumerable<TResult> FullOuterJoin<TLeft, TRight, TKey, TResult>(
            this IEnumerable<TLeft> left,
            IEnumerable<TRight> right,
            Func<TLeft, TKey> leftKey,
            Func<TRight, TKey> rightKey,
            Func<TLeft?, TRight?, TResult> resultSelector)
    {
        //IEnumerable<TResult>? leftResult = LeftOuterJoin(left, right, leftKey, rightKey, result);
        //IEnumerable<TResult>? rightResult = RightOuterJoin(left, right, leftKey, rightKey, result);
        //return leftResult.Union(rightResult);

        var leftLookup = left.ToLookup(leftKey);
        var rightLookup = right.ToLookup(rightKey);

        var keys = new HashSet<TKey>(leftLookup.Select(p => p.Key));
        keys.UnionWith(rightLookup.Select(p => p.Key));

        IEnumerable<TResult> result = from key in keys
                                      from xLeft in leftLookup[key].DefaultIfEmpty()
                                      from xRight in rightLookup[key].DefaultIfEmpty()
                                      select resultSelector(xLeft, xRight);

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
