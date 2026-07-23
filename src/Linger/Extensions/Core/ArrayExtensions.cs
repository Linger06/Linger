namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for array manipulation.
/// </summary>
public static class ArrayExtensions
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
    [Obsolete("Use Array.ForEach(array, action) or the IEnumerable<T> ForEach extension instead.")]
    public static void ForEach<T>(this T[] array, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
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
    public static void ForEach<T>(this T[] array, [NotNull] Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(action);
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
        return Array.Exists(array, Predicate);

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
        return array.Add(value);
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
        var result = new T[array.Length + 1];
        Array.Copy(array, result, array.Length);
        result[array.Length] = value;

        return result;
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
    /// // newNumbers is { 1, 3 }
    /// </code>
    /// </example>
    public static T[] Remove<T>(this T[] array, T value)
    {
        return Array.FindAll(array, Predicate);

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
            throw new ArgumentOutOfRangeException(nameof(index), index, "Index was out of range. Must be non-negative and less than the size of the array.");
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
            throw new ArgumentOutOfRangeException(nameof(startIndex), "The specified index is out of the array bounds.");
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
            throw new ArgumentOutOfRangeException(nameof(startIndex), "The specified index is out of the array bounds.");
        }

        if (startIndex + length > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "The range at the specified index is out of the array bounds.");
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

    #region 转换方法

    /// <summary>
    /// Converts the current Byte[] sequence to a Base64 encoded string.
    /// </summary>
    /// <param name="value">The Byte[] to convert.</param>
    /// <returns>The converted string representation in Base64.</returns>
    /// <example>
    /// <code>
    /// byte[] bytes = { 1, 2, 3, 4, 5 };
    /// string base64String = bytes.ToBase64String();
    /// // base64String is "AQIDBAU="
    /// </code>
    /// </example>
    public static string ToBase64String(this byte[] value)
    {
        return Convert.ToBase64String(value, 0, value.Length);
    }

    /// <summary>
    /// Converts the current Byte[] sequence to a URL-safe Base64 encoded string.
    /// </summary>
    /// <param name="value">The Byte[] to convert.</param>
    /// <returns>The converted string representation in URL-safe Base64.</returns>
    /// <remarks>
    /// URL-safe Base64 replaces '+' with '-' and '/' with '_', and removes padding '=' characters.
    /// This makes the output safe for use in URLs without additional encoding.
    /// </remarks>
    /// <example>
    /// <code>
    /// byte[] bytes = { 1, 2, 3, 4, 5 };
    /// string base64UrlString = bytes.ToBase64UrlString();
    /// // base64UrlString is "AQIDBAU" (URL-safe, no padding)
    /// </code>
    /// </example>
    public static string ToBase64UrlString(this byte[] value)
    {
        return System.Buffers.Text.Base64Url.EncodeToString(value);
    }

    /// <summary>
    /// Converts the current Byte[] sequence to a Base64 encoded string for an image.
    /// </summary>
    /// <param name="value">The Byte[] to convert.</param>
    /// <returns>The converted string representation of the image in Base64.</returns>
    /// <example>
    /// <code>
    /// byte[] imageBytes = { 1, 2, 3, 4, 5 };
    /// string imageBase64String = imageBytes.ToImageBase64String();
    /// // imageBase64String is "data:image/jpeg;base64,AQIDBAU="
    /// </code>
    /// </example>
    [Obsolete("Use ToImageDataUri(value, mediaType) so the data URI contains the correct image media type.")]
    public static string ToImageBase64String(this byte[] value)
    {
        return value.ToImageDataUri("image/jpeg");
    }

    /// <summary>
    /// Converts image bytes to a Base64 data URI with the specified media type.
    /// </summary>
    /// <param name="value">The image bytes to encode.</param>
    /// <param name="mediaType">The image media type, such as <c>image/png</c> or <c>image/jpeg</c>.</param>
    /// <returns>A Base64 data URI containing the specified image media type.</returns>
    /// <example>
    /// <code>
    /// byte[] imageBytes = File.ReadAllBytes("image.png");
    /// string dataUri = imageBytes.ToImageDataUri("image/png");
    /// </code>
    /// </example>
    public static string ToImageDataUri(this byte[] value, string mediaType)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentException.ThrowIfNullOrWhiteSpace(mediaType);

        if (!mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The media type must be an image media type.", nameof(mediaType));
        }

        return $"data:{mediaType};base64,{Convert.ToBase64String(value)}";
    }

    /// <summary>
    /// Converts the current Byte[] sequence to a MemoryStream.
    /// </summary>
    /// <param name="value">The Byte[] to convert.</param>
    /// <returns>A MemoryStream created from the Byte[].</returns>
    /// <example>
    /// <code>
    /// byte[] bytes = { 1, 2, 3, 4, 5 };
    /// MemoryStream stream = bytes.ToMemoryStream();
    /// // stream is a MemoryStream containing the bytes
    /// </code>
    /// </example>
    public static MemoryStream ToMemoryStream(this byte[] value)
    {
        return new MemoryStream(value);
    }

    /// <summary>
    /// Converts the current System.String[] to DataTable columns.
    /// </summary>
    /// <param name="stringArray">The string array to convert.</param>
    /// <returns>A DataTable with columns named after the strings in the array.</returns>
    /// <example>
    /// <code>
    /// string[] columns = { "Name", "Age", "Gender" };
    /// DataTable table = columns.ToDataTableColumns();
    /// // table has columns "Name", "Age", "Gender"
    /// </code>
    /// </example>
    public static DataTable ToDataTableColumns(this string[] stringArray)
    {
        var table = new DataTable();

        foreach (var item in stringArray)
        {
            table.Columns.Add(new DataColumn(item));
        }

        return table;
    }

    /// <summary>
    /// Converts the current <see cref="string"/>[] to an <see cref="IEnumerable{String}"/>.
    /// </summary>
    /// <param name="value">The <see cref="string"/>[] to convert.</param>
    /// <returns>An <see cref="IEnumerable{String}"/> created from the array.</returns>
    /// <example>
    /// <code>
    /// string[] array = { "one", "two", "three" };
    /// IEnumerable&lt;string&gt; enumerable = array;
    /// // enumerable contains "one", "two", "three"
    /// </code>
    /// </example>
    [Obsolete("Arrays already implement IEnumerable<string>. Use value ?? Array.Empty<string>() when null normalization is required.")]
    public static IEnumerable<string> ToEnumerable(this string[]? value)
    {
        return value.IsNull() ? [] : new List<string>(value);
    }

    /// <summary>
    /// Converts the current <see cref="string"/>[] to a <see cref="List{String}"/>.
    /// </summary>
    /// <param name="value">The <see cref="string"/>[] to convert.</param>
    /// <returns>A <see cref="List{String}"/> created from the array.</returns>
    /// <example>
    /// <code>
    /// string[] array = { "one", "two", "three" };
    /// List&lt;string&gt; list = Enumerable.ToList(array);
    /// // list contains "one", "two", "three"
    /// </code>
    /// </example>
    [Obsolete("Use new List<string>(value ?? Array.Empty<string>()) for null-to-empty normalization, or Enumerable.ToList(value) for non-null arrays.")]
    public static List<string> ToList(this string[]? value)
    {
        return new List<string>(value ?? []);
    }

    /// <summary>
    /// Converts a nullable string array to a list, returning an empty list when the array is null.
    /// </summary>
    /// <param name="value">The nullable string array to convert.</param>
    /// <returns>A list containing the array values, or an empty list when <paramref name="value"/> is null.</returns>
    /// <example>
    /// <code>
    /// string[]? values = null;
    /// List&lt;string&gt; result = new(values ?? Array.Empty&lt;string&gt;());
    /// </code>
    /// </example>
    [Obsolete("Use new List<string>(value ?? Array.Empty<string>()) instead.")]
    public static List<string> ToListOrEmpty(this string[]? value)
    {
        return new List<string>(value ?? []);
    }

    /// <summary>
    /// Converts a byte array to an MD5 hash code string.
    /// </summary>
    /// <param name="inputHashBytes">The byte array to convert.</param>
    /// <returns>The MD5 hash code string.</returns>
    public static string ToMd5HashCode(this byte[] inputHashBytes)
    {
        return Convert.ToHexStringLower(inputHashBytes);
    }

    #endregion
}
