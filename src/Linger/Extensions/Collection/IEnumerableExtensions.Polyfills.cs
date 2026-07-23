namespace Linger.Extensions.Collection;

/// <summary>
/// Polyfill methods for IEnumerable to align with BCL APIs on older target frameworks.
/// </summary>
public static partial class IEnumerableExtensions
{
#if !NET6_0_OR_GREATER
    /// <summary>
    /// Returns distinct elements from a sequence by using a specified key selector.
    /// </summary>
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

        return Iterator();

        IEnumerable<TSource> Iterator()
        {
            var seenKeys = new HashSet<TKey>();
            foreach (TSource item in source)
            {
                if (seenKeys.Add(keySelector(item)))
                {
                    yield return item;
                }
            }
        }
    }
#endif

#if !NET10_0_OR_GREATER
    /// <summary>
    /// Performs a left outer join on two sequences.
    /// </summary>
    public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
        this IEnumerable<TOuter> outer,
        IEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner?, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(outer);
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(outerKeySelector);
        ArgumentNullException.ThrowIfNull(innerKeySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        return LeftJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
    }

    /// <summary>
    /// Performs a left outer join on two sequences using a specified equality comparer.
    /// </summary>
    public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
        this IEnumerable<TOuter> outer,
        IEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner?, TResult> resultSelector,
        IEqualityComparer<TKey>? comparer)
    {
        ArgumentNullException.ThrowIfNull(outer);
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(outerKeySelector);
        ArgumentNullException.ThrowIfNull(innerKeySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, (o, i) => new { o, i }, comparer)
            .SelectMany(x => x.i.DefaultIfEmpty(), (o, i) => new { outer = o.o, inner = i })
            .Select(x => resultSelector(x.outer, x.inner));
    }

    /// <summary>
    /// Performs a right outer join on two sequences.
    /// </summary>
    public static IEnumerable<TResult> RightJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter?, TInner, TResult> resultSelector
        )
    {
        ArgumentNullException.ThrowIfNull(outer);
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(outerKeySelector);
        ArgumentNullException.ThrowIfNull(innerKeySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        return RightJoin(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
    }

    /// <summary>
    /// Performs a right outer join on two sequences using a specified equality comparer.
    /// </summary>
    public static IEnumerable<TResult> RightJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter?, TInner, TResult> resultSelector,
            IEqualityComparer<TKey>? comparer
        )
    {
        ArgumentNullException.ThrowIfNull(outer);
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(outerKeySelector);
        ArgumentNullException.ThrowIfNull(innerKeySelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        return LeftJoin(inner, outer, innerKeySelector, outerKeySelector, (i, o) => resultSelector(o, i), comparer);
    }
#endif

    /// <summary>
    /// Performs a left outer join and returns each match as a tuple.
    /// </summary>
    /// <typeparam name="TOuter">The type of elements in the outer sequence.</typeparam>
    /// <typeparam name="TInner">The type of elements in the inner sequence.</typeparam>
    /// <typeparam name="TKey">The type of the join key.</typeparam>
    /// <param name="outer">The outer sequence.</param>
    /// <param name="inner">The inner sequence.</param>
    /// <param name="outerKeySelector">The function used to extract a key from an outer element.</param>
    /// <param name="innerKeySelector">The function used to extract a key from an inner element.</param>
    /// <returns>A sequence of tuples containing outer elements and their matching inner elements.</returns>
    /// <example>
    /// <code>
    /// var result = employees.LeftJoin(departments, employee => employee.DepartmentId, department => department.Id);
    /// </code>
    /// </example>
    public static IEnumerable<Tuple<TOuter, TInner?>> LeftJoin<TOuter, TInner, TKey>(
        this IEnumerable<TOuter> outer,
        IEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector)
    {
        ArgumentNullException.ThrowIfNull(outer);
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(outerKeySelector);
        ArgumentNullException.ThrowIfNull(innerKeySelector);

        return outer.LeftJoin(
            inner,
            outerKeySelector,
            innerKeySelector,
            static (outerItem, innerItem) => new Tuple<TOuter, TInner?>(outerItem, innerItem));
    }

#if !NET9_0_OR_GREATER
    /// <summary>
    /// Returns an enumerable that incorporates the element's index into a tuple.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">The source enumerable providing the elements.</param>
    /// <returns>An enumerable that incorporates each element's index into a tuple.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// var items = new[] { "a", "b", "c" };
    /// foreach (var (index, item) in items.Index())
    /// {
    ///     Console.WriteLine($"{index}: {item}");
    /// }
    /// // Output:
    /// // 0: a
    /// // 1: b
    /// // 2: c
    /// </code>
    /// </example>
    public static IEnumerable<(int Index, TSource Item)> Index<TSource>(this IEnumerable<TSource> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return IndexIterator(source);
    }

    private static IEnumerable<(int Index, TSource Item)> IndexIterator<TSource>(IEnumerable<TSource> source)
    {
        var index = 0;
        foreach (var item in source)
        {
            yield return (index++, item);
        }
    }
#endif
}
