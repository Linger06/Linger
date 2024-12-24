
namespace Linger.Extensions.Collection;

public static partial class IEnumerableExtensions
{
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
    /// <example>
    /// <code>
    /// var left = new List&lt;int&gt; { 1, 2, 3 };
    /// var right = new List&lt;int&gt; { 3, 4, 5 };
    /// var result = left.LeftOuterJoin(right, l => l, r => r, (l, r) => new { Left = l, Right = r });
    /// // Output: [{ Left = 1, Right = null }, { Left = 2, Right = null }, { Left = 3, Right = 3 }]
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
    /// <example>
    /// <code>
    /// var left = new List&lt;int&gt; { 1, 2, 3 };
    /// var right = new List&lt;int&gt; { 3, 4, 5 };
    /// var result = left.RightOuterJoin(right, l => l, r => r, (l, r) => new { Left = l, Right = r });
    /// // Output: [{ Left = null, Right = 4 }, { Left = null, Right = 5 }, { Left = 3, Right = 3 }]
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
    /// <example>
    /// <code>
    /// var left = new List&lt;int&gt; { 1, 2, 3 };
    /// var right = new List&lt;int&gt; { 3, 4, 5 };
    /// var result = left.InnerJoin(right, l => l, r => r, (l, r) => new { Left = l, Right = r });
    /// // Output: [{ Left = 3, Right = 3 }]
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
    /// <example>
    /// <code>
    /// var left = new List&lt;int&gt; { 1, 2, 3 };
    /// var right = new List&lt;int&gt; { 3, 4, 5 };
    /// var result = left.FullOuterJoin(right, l => l, r => r, (l, r) => new { Left = l, Right = r });
    /// // Output: [{ Left = 1, Right = null }, { Left = 2, Right = null }, { Left = 3, Right = 3 }, { Left = null, Right = 4 }, { Left = null, Right = 5 }]
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

        return result.ToList();
    }
}
