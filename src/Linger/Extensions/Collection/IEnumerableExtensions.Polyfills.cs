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
        if (source == null) throw new System.ArgumentNullException(nameof(source));
        if (keySelector == null) throw new System.ArgumentNullException(nameof(keySelector));

        var hash = new HashSet<TKey>();
        return source.Where(p => hash.Add(keySelector(p)));
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

        return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, (o, i) => new { o, i })
            .SelectMany(x => x.i.DefaultIfEmpty(), (o, i) => new { outer = o.o, inner = i })
            .Select(x => resultSelector(x.outer, x.inner));
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
    /// Performs a left outer join on two sequences and returns a sequence of tuples.
    /// </summary>
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

        return LeftJoin(outer, inner, outerKeySelector, innerKeySelector, (o, i) => new Tuple<TOuter, TInner?>(o, i));
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
        IEnumerable<TResult> query = LeftJoin(inner, outer, innerKeySelector, outerKeySelector, (i, o) => resultSelector(o, i));
        return query;
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
        IEnumerable<TResult> query = LeftJoin(inner, outer, innerKeySelector, outerKeySelector, (i, o) => resultSelector(o, i), comparer);
        return query;
    }
#endif
}
