using Linger.Helper;

namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for array manipulation.
/// </summary>
public static partial class ArrayExtensions
{
    /// <summary>
    /// Executes the specified action on each element of the array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to iterate over.</param>
    /// <param name="action">The action to execute on each element.</param>
    /// <example>
    /// <code>
    /// int[] numbers = { 1, 2, 3 };
    /// numbers.ForEach(n => Console.WriteLine(n));
    /// // Output: 1 2 3
    /// </code>
    /// </example>
    public static void ForEach<T>(this T[] array, Action<T> action)
    {
        action.EnsureIsNotNull(nameof(action));
        Array.ForEach(array, action);
    }

    /// <summary>
    /// Executes the specified action on each element of the array, providing the element's index.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to iterate over.</param>
    /// <param name="action">The action to execute on each element, with the element's index.</param>
    /// <example>
    /// <code>
    /// int[] numbers = { 1, 2, 3 };
    /// numbers.ForEach((n, i) => Console.WriteLine($"{i}: {n}"));
    /// // Output: 0: 1 1: 2 2: 3
    /// </code>
    /// </example>
    public static void ForEach<T>(this T[] array, Action<T, int> action)
    {
        action.EnsureIsNotNull(nameof(action));
        for (var i = 0; i < array.Length; i++)
        {
            action(array[i], i);
        }
    }

    /// <summary>
    /// Determines whether the array contains a specific value.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to search.</param>
    /// <param name="value">The value to locate in the array.</param>
    /// <returns><value>true</value> if the value is found; otherwise, <value>false</value>.</returns>
    /// <example>
    /// <code>
    /// int[] numbers = { 1, 2, 3 };
    /// bool exists = numbers.Exists(2);
    /// // exists is true
    /// </code>
    /// </example>
    public static bool Exists<T>(this T[] array, T value)
    {
        bool Predicate(T item)
        {
            if (item == null)
            {
                if (value == null)
                {
                    return true;
                }

                return false;
            }

            return item.Equals(value);
        }

        return Array.Exists(array, Predicate);
    }

    /// <summary>
    /// Determines whether the array contains elements that match the conditions defined by the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to search.</param>
    /// <param name="match">The predicate that defines the conditions of the elements to search for.</param>
    /// <returns><value>true</value> if one or more elements match the conditions defined by the specified predicate; otherwise, <value>false</value>.</returns>
    /// <example>
    /// <code>
    /// int[] numbers = { 1, 2, 3 };
    /// bool exists = numbers.Exists(n => n > 2);
    /// // exists is true
    /// </code>
    /// </example>
    public static bool Exists<T>(this T[] array, Predicate<T> match)
    {
        return Array.Exists(array, match);
    }

    /// <summary>
    /// Inserts a value at the end of the array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to insert the value into.</param>
    /// <param name="value">The value to insert.</param>
    /// <returns>A new array with the value inserted at the end.</returns>
    /// <example>
    /// <code>
    /// int[] numbers = { 1, 2, 3 };
    /// int[] newNumbers = numbers.Insert(4);
    /// // newNumbers is { 1, 2, 3, 4 }
    /// </code>
    /// </example>
    public static T[] Insert<T>(this T[] array, T value)
    {
        var result = new T[array.Length + 1];
        Array.Copy(array, result, array.Length);
        result[array.Length] = value;
        return result;
    }

    /// <summary>
    /// Adds a value to the end of the array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to add the value to.</param>
    /// <param name="value">The value to add.</param>
    /// <returns>A new array with the value added at the end.</returns>
    /// <example>
    /// <code>
    /// int[] numbers = { 1, 2, 3 };
    /// int[] newNumbers = numbers.Add(4);
    /// // newNumbers is { 1, 2, 3, 4 }
    /// </code>
    /// </example>
    public static T[] Add<T>(this T[] array, T value)
    {
        var arr = new T[array.Length + 1];
        for (var i = 0; i < array.Length; i++)
        {
            arr[i] = array[i];
        }

        arr[array.Length] = value;
        return arr;
    }

    /// <summary>
    /// Removes the specific value from the array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to remove the value from.</param>
    /// <param name="value">The value to remove.</param>
    /// <returns>A new array with the value removed.</returns>
    /// <example>
    /// <code>
    /// int[] numbers = { 1, 2, 3, 2 };
    /// int[] newNumbers = numbers.Remove(2);
    /// // newNumbers is { 1, 3, 2 }
    /// </code>
    /// </example>
    public static T[] Remove<T>(this T[] array, T value)
    {
        bool Predicate(T item)
        {
            if (item == null)
            {
                if (value == null)
                {
                    return false;
                }

                return true;
            }

            return !item.Equals(value);
        }

        return Array.FindAll(array, Predicate);
    }

    /// <summary>
    /// Removes the element at the specified index of the array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to remove the element from.</param>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <returns>A new array with the element removed.</returns>
    /// <example>
    /// <code>
    /// int[] numbers = { 1, 2, 3 };
    /// int[] newNumbers = numbers.RemoveAt(1);
    /// // newNumbers is { 1, 3 }
    /// </code>
    /// </example>
    public static T[] RemoveAt<T>(this T[] array, int index)
    {
        if (index < 0 || index > array.Length - 1)
        {
            return [];
        }

        var newArray = new T[array.Length - 1];
        var index2 = 0;
        for (var i = 0; i < array.Length; i++)
        {
            if (i != index)
            {
                newArray[index2++] = array[i];
            }
        }

        return newArray;
    }

    /// <summary>
    /// Removes all elements from the specified start index to the end of the array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to remove the elements from.</param>
    /// <param name="startIndex">The zero-based starting index of the range of elements to remove.</param>
    /// <returns>A new array with the elements removed.</returns>
    /// <example>
    /// <code>
    /// int[] numbers = { 1, 2, 3, 4 };
    /// int[] newNumbers = numbers.RemoveRange(2);
    /// // newNumbers is { 1, 2 }
    /// </code>
    /// </example>
    public static T[] RemoveRange<T>(this T[] array, int startIndex)
    {
        if (startIndex < 0 || startIndex >= array.Length)
        {
            throw new IndexOutOfRangeException("The specified index is out of the array bounds.");
        }

        var newArray = new T[startIndex];
        for (var i = 0; i < startIndex; i++)
        {
            newArray[i] = array[i];
        }

        return newArray;
    }

    /// <summary>
    /// Removes a range of elements from the array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="array">The array to remove the elements from.</param>
    /// <param name="startIndex">The zero-based starting index of the range of elements to remove.</param>
    /// <param name="length">The number of elements to remove.</param>
    /// <returns>A new array with the elements removed.</returns>
    /// <example>
    /// <code>
    /// int[] numbers = { 1, 2, 3, 4, 5 };
    /// int[] newNumbers = numbers.RemoveRange(1, 3);
    /// // newNumbers is { 1, 5 }
    /// </code>
    /// </example>
    public static T[] RemoveRange<T>(this T[] array, int startIndex, int length)
    {
        if (startIndex < 0 || startIndex >= array.Length)
        {
            throw new IndexOutOfRangeException("The specified index is out of the array bounds.");
        }

        if (startIndex + length > array.Length)
        {
            throw new IndexOutOfRangeException("The range at the specified index is out of the array bounds.");
        }

        var arr = new T[array.Length - length];
        var j = 0;
        for (var i = 0; i < array.Length; i++)
        {
            if (i >= startIndex && i < startIndex + length)
            {
                continue;
            }

            arr[j++] = array[i];
        }

        return arr;
    }
}
