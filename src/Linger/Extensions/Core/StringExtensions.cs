// This file contains string extension methods that have been split into multiple files by functionality.
// The actual implementations are in the following files:
//   - StringExtensions.Validation.cs    : String null/empty/whitespace validation methods
//   - StringExtensions.Numeric.cs       : Numeric type validation methods
//   - StringExtensions.Manipulation.cs  : String manipulation and transformation methods
//   - StringExtensions.Split.cs         : String splitting and query appending methods
//   - StringExtensions.Encoding.cs      : Hash and Base64 encoding methods
//   - StringExtensions.Regex.cs         : Regular expression validation methods
//   - StringExtensions.Special.cs       : Special utility methods

using System.Text.RegularExpressions;

namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for string operations including validation, manipulation, encoding, and regex validation.
/// This is a partial class with implementations distributed across multiple files for better organization.
/// </summary>
public static partial class StringExtensions
{
    // All methods are implemented in separate files based on functionality.
    // This main file serves as documentation for the class structure.

    /// <summary>
    /// Removes parentheses and the content within them from the string.
    /// Supports both full-width parentheses (（）) and half-width parentheses ().
    /// </summary>
    /// <param name="value">The string to process.</param>
    /// <returns>The string with all parentheses and their contents removed.</returns>
    /// <example>
    /// <code>
    /// string result = "Hello (World)".DeleteBrackets();
    /// // result: "Hello "
    /// 
    /// string result2 = "北京（中国）".DeleteBrackets();
    /// // result2: "北京"
    /// </code>
    /// </example>
    public static string DeleteBrackets(this string value)
    {
        var str = value.Replace("（", "(").Replace("）", ")");
        return Regex.Replace(str.Replace("（", "(").Replace("）", ")"), @"\([^\(]*\)", "");
    }

    /// <summary>
    /// Splits the current <see cref="string"/> by the specified delimiter and returns a List&lt;<see cref="string"/>&gt;.
    /// </summary>
    /// <param name="value">The <see cref="string"/> to split.</param>
    /// <param name="symbol">The delimiter pattern (supports regex). Defaults to CRLF (\r\n).</param>
    /// <returns>A List&lt;<see cref="string"/>&gt; resulting from the split operation. Returns an empty list if the input is null or empty.</returns>
    /// <example>
    /// <code>
    /// string text = "line1\r\nline2\r\nline3";
    /// List&lt;string&gt; lines = text.ToSplitList();
    /// // lines: ["line1", "line2", "line3"]
    /// </code>
    /// </example>
    public static List<string> ToSplitList(this string value, string symbol = "\r\n")
    {
        if (value.IsNullOrEmpty())
        {
            return Enumerable.Empty<string>().ToList();
        }

        return Regex.Split(value, symbol, RegexOptions.IgnoreCase).ToList();
    }

    /// <summary>
    /// Splits the current <see cref="string"/> by the specified <see cref="char"/> delimiter and returns an IEnumerable&lt;<see cref="string"/>&gt;.
    /// </summary>
    /// <param name="value">The <see cref="string"/> to split.</param>
    /// <param name="symbol">The <see cref="char"/> delimiter. Defaults to comma (',').</param>
    /// <returns>An IEnumerable&lt;<see cref="string"/>&gt; resulting from the split operation.</returns>
    /// <example>
    /// <code>
    /// string csv = "apple,banana,cherry";
    /// IEnumerable&lt;string&gt; fruits = csv.ToSplitList(',');
    /// // fruits: ["apple", "banana", "cherry"]
    /// </code>
    /// </example>
    public static IEnumerable<string> ToSplitList(this string value, char symbol = ',')
    {
        var value2 = value.ToSplitArray(symbol);
        return value2.ToEnumerable();
    }

    /// <summary>
    /// Splits the current <see cref="string"/> by the system's newline character(s) and returns an array of <see cref="string"/>.
    /// </summary>
    /// <param name="value">The <see cref="string"/> to split.</param>
    /// <returns>An array of <see cref="string"/> resulting from the split operation. Returns an empty array if the input is null or empty.</returns>
    /// <example>
    /// <code>
    /// string text = "line1\r\nline2\r\nline3";
    /// string[] lines = text.ToSplitArrayByCrlf();
    /// // lines: ["line1", "line2", "line3"]
    /// </code>
    /// </example>
    public static string[] ToSplitArrayByCrlf(this string value)
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }

        return Regex.Split(value, Environment.NewLine, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Splits the current <see cref="string"/> by the specified <see cref="char"/> delimiter and returns an array of <see cref="string"/>.
    /// </summary>
    /// <param name="value">The <see cref="string"/> to split.</param>
    /// <param name="symbol">The <see cref="char"/> delimiter. Defaults to comma (',').</param>
    /// <returns>An array of <see cref="string"/> resulting from the split operation. Returns an empty array if the input is null or empty.</returns>
    /// <example>
    /// <code>
    /// string csv = "apple,banana,cherry";
    /// string[] fruits = csv.ToSplitArray(',');
    /// // fruits: ["apple", "banana", "cherry"]
    /// </code>
    /// </example>
    public static string[] ToSplitArray(this string value, char symbol = ',')
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }

        return value.Split(symbol);
    }

}
