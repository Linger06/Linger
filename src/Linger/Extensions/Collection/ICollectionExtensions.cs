namespace Linger.Extensions.Collection;

/// <summary>
/// ICollectionExtensions
/// </summary>
public static class ICollectionExtensions
{
    /// <summary>
    /// Removes all elements from the collection that match the conditions defined by the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="this">The collection from which elements should be removed.</param>
    /// <param name="predicate">The predicate that defines the conditions of the elements to remove.</param>
    /// <remarks>
    /// List 使用原生 RemoveAll；其他 IList 按索引移除；LinkedList 按节点线性遍历并移除。
    /// 其他 ICollection 依赖自身 Remove 的相等性规则和复杂度。
    /// </remarks>
    public static void RemoveAll<T>(this ICollection<T> @this, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(@this);
        ArgumentNullException.ThrowIfNull(predicate);

        // Check if the collection is a List<T> to use the optimized RemoveAll method
        if (@this is List<T> list)
        {
            list.RemoveAll(new Predicate<T>(predicate));
        }
        else if (@this is IList<T> indexedList)
        {
            // 先记录索引再倒序删除，避免 Equals 相等的保留项被误删，同时保持谓词调用顺序。
            var indexesToDelete = new List<int>();
            for (var i = 0; i < indexedList.Count; i++)
            {
                if (predicate(indexedList[i]))
                {
                    indexesToDelete.Add(i);
                }
            }

            for (var i = indexesToDelete.Count - 1; i >= 0; i--)
            {
                indexedList.RemoveAt(indexesToDelete[i]);
            }
        }
        else if (@this is LinkedList<T> linkedList)
        {
            var node = linkedList.First;
            while (node is not null)
            {
                var next = node.Next;
                if (predicate(node.Value))
                {
                    linkedList.Remove(node);
                }

                node = next;
            }
        }
        else
        {
            // For other ICollection<T> implementations, manually remove matching elements
            var itemsToDelete = @this.Where(predicate).ToList();
            foreach (T item in itemsToDelete)
            {
                @this.Remove(item);
            }
        }
    }
}
