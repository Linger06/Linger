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
    public static void RemoveAll<T>(this ICollection<T> @this, Func<T, bool> predicate)
    {
        // Check if the collection is a List<T> to use the optimized RemoveAll method
        if (@this is List<T> list)
        {
            list.RemoveAll(new Predicate<T>(predicate));
        }
        else
        {
            // For other ICollection<T> implementations, manually remove matching elements
            var itemsToDelete = @this.Where(predicate).ToList();
            foreach (T? item in itemsToDelete)
            {
                @this.Remove(item);
            }
        }
    }
}
