
namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="Array"/> extensions
/// </summary>
public static partial class ArrayExtensions
{
    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="Array"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The one-dimensional, zero-based <see cref="Array"/> to search.</param>
    /// <param name="match">The <see cref="Predicate{T}"/> that defines the conditions of the element to search for.</param>
    /// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null or the match is null.</exception>
    /// <example>
    /// <code>
    /// int[] numbers = { 1, 2, 3, 4, 5 };
    /// int result = numbers.Find(n => n > 3);
    /// // result is 4
    /// </code>
    /// </example>
    public static T? Find<T>(this T[] array, Predicate<T> match)
    {
        return Array.Find(array, match);
    }

    /// <summary>
    /// Retrieves all the elements that match the conditions defined by the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The one-dimensional, zero-based <see cref="Array"/> to search.</param>
    /// <param name="match">The <see cref="Predicate{T}"/> that defines the conditions of the elements to search for.</param>
    /// <returns>An <see cref="Array"/> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="Array"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null or the match is null.</exception>
    /// <example>
    /// <code>
    /// int[] numbers = { 1, 2, 3, 4, 5 };
    /// int[] results = numbers.FindAll(n => n > 2);
    /// // results is { 3, 4, 5 }
    /// </code>
    /// </example>
    public static T[] FindAll<T>(this T[] array, Predicate<T> match)
    {
        return Array.FindAll(array, match);
    }
}
