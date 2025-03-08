
namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="Array"/> extensions
/// </summary>
public static partial class ArrayExtensions
{
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
    public static string ToImageBase64String(this byte[] value)
    {
        return $"data:image/jpeg;base64,{Convert.ToBase64String(value, 0, value.Length)}";
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
    /// IEnumerable&lt;string&gt; enumerable = array.ToEnumerable();
    /// // enumerable contains "one", "two", "three"
    /// </code>
    /// </example>
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
    /// List&lt;string&gt; list = array.ToList();
    /// // list contains "one", "two", "three"
    /// </code>
    /// </example>
    public static List<string> ToList(this string[]? value)
    {
        return value.ToEnumerable().ToList();
    }

    public static string ToMd5HashCode(this byte[] inputHashBytes)
    {
#if NET5_0_OR_GREATER
        return Convert.ToHexString(inputHashBytes);
#elif NETFRAMEWORK || NETSTANDARD2_0
        return BitConverter.ToString(inputHashBytes).Replace("-", string.Empty);
#else
        return BitConverter.ToString(inputHashBytes).Replace("-", string.Empty, StringComparison.Ordinal);
#endif
    }
}
