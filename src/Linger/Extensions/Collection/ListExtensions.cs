using Linger.Extensions.Core;

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
    public static List<T> Paging<T>(this List<T>? list, int pageIndex, int pageSize)
    {
        return ((IEnumerable<T>?)list).Paging(pageIndex, pageSize).ToList();
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
            var newItem = notSpace ? item.Replace(" ", string.Empty) : item;
            return $"{quote}{newItem}{quote}";
        }
    }

    /// <summary>
    /// Converts the <see cref="List{T}"/> to a tree structure based on the specified conditions.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the <see cref="List{T}"/>.</typeparam>
    /// <param name="list">The <see cref="List{T}"/> to convert.</param>
    /// <param name="rootWhere">A function to determine the root elements.</param>
    /// <param name="childsWhere">A function to determine the child elements.</param>
    /// <param name="addChilds">An action to add child elements to a parent element.</param>
    /// <param name="entity">The root entity to start the tree from.</param>
    /// <returns>A tree-structured <see cref="List{T}"/>.</returns>
    /// <example>
    /// <code>
    /// var list = new List&lt;Node&gt; { new Node { Id = 1 }, new Node { Id = 2, ParentId = 1 }, new Node { Id = 3, ParentId = 1 } };
    /// var result = list.ToTree((parent, child) => parent.Id == 0, (parent, child) => parent.Id == child.ParentId, (parent, children) => parent.Children = children.ToList());
    /// // Output: Tree-structured list with root node having two children
    /// </code>
    /// </example>
    public static List<T> ToTree<T>(this List<T> list, Func<T, T, bool> rootWhere, Func<T, T, bool> childsWhere, Action<T, IEnumerable<T>> addChilds, T entity = default!)
    {
        var treeList = new List<T>();
        if (list.Count == 0)
        {
            return treeList;
        }
        if (!list.Any(e => rootWhere(entity, e)))
        {
            return treeList;
        }

        if (list.Any(e => rootWhere(entity, e)))
        {
            treeList.AddRange(list.Where(e => rootWhere(entity, e)));
        }

        foreach (T item in treeList)
        {
            if (list.Any(e => childsWhere(item, e)))
            {
                var nodeData = list.Where(e => childsWhere(item, e)).ToList();
                foreach (T child in nodeData)
                {
                    List<T> data = list.ToTree(childsWhere, childsWhere, addChilds, child);
                    addChilds(child, data);
                }
                addChilds(item, nodeData);
            }
        }

        return treeList;
    }
}
