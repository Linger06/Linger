namespace Linger.Extensions.Collection;

/// <summary>
/// Provides extension methods for <see cref="List{T}"/>.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Paginates the <see cref="List{T}"/> based on the specified page index and page size.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to paginate.</param>
    /// <param name="pageIndex">The index of the page to retrieve (1-based).</param>
    /// <param name="pageSize">The size of the page to retrieve.</param>
    /// <returns>A paginated <see cref="List{T}"/>.</returns>
    /// <example>
    /// <code>
    /// var list = new List&lt;int&gt; { 1, 2, 3, 4, 5 };
    /// var result = list.Paging(2, 2);
    /// // Output: [3, 4]
    /// </code>
    /// </example>
    public static IEnumerable<T> Paging<T>(this List<T>? list, int pageIndex, int pageSize)
    {
        return ((IEnumerable<T>?)list).Paging(pageIndex, pageSize);
    }

    /// <summary>
    /// Converts the <see cref="List{T}"/> to a string, with each element separated by the specified separator.
    /// </summary>
    /// <param name="list">The <see cref="List{T}"/> to convert.</param>
    /// <param name="separator">The separator to use between elements.</param>
    /// <param name="singleQuoted">Whether to enclose each element in single quotes.</param>
    /// <param name="notSpace">Whether to remove spaces from each element.</param>
    /// <returns>A string representation of the <see cref="List{T}"/> with elements separated by the specified separator.</returns>
    /// <example>
    /// <code>
    /// var list = new List&lt;string&gt; { "a", "b", "c" };
    /// var result = list.ToSeparatedString(",", true, false);
    /// // Output: "'a','b','c'"
    /// </code>
    /// </example>
    public static string ToSeparatedString(this List<string> list, string separator = ",", bool singleQuoted = true, bool notSpace = false)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(separator);

        return list.ToSeparatedString(separator, Format);

        string Format(string item)
        {
            var quote = singleQuoted ? "'" : string.Empty;
            string value = item ?? string.Empty;
            var newItem = notSpace ? value.Replace(" ", string.Empty) : value;
            return $"{quote}{newItem}{quote}";
        }
    }

}
