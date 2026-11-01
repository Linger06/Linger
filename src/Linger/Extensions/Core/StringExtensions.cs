// This file contains string extension methods that have been split into multiple files by functionality.
// The actual implementations are in the following files:
//   - StringExtensions.Validation.cs    : String null/empty/whitespace validation methods
//   - StringExtensions.Numeric.cs       : Numeric type validation methods
//   - StringExtensions.Manipulation.cs  : String manipulation and transformation methods
//   - StringExtensions.Split.cs         : String splitting and query appending methods
//   - StringExtensions.Encoding.cs      : Hash and Base64 encoding methods
//   - StringExtensions.Regex.cs         : Regular expression validation methods
//   - StringExtensions.Special.cs       : Special utility methods

using System.Text;
using System.Text.RegularExpressions;

namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for string operations including validation, manipulation, encoding, and regex validation.
/// This is a partial class with implementations distributed across multiple files for better organization.
/// </summary>
public static partial class StringExtensions
{
    private static readonly TimeSpan s_regexSplitTimeout = TimeSpan.FromSeconds(1);

    // All methods are implemented in separate files based on functionality.
    // This main file serves as documentation for the class structure.

    /// <summary>
    /// Removes parentheses and the content within them from the string.
    /// Supports both full-width parentheses (（）) and half-width parentheses ().
    /// </summary>
    /// <param name="value">The string to process.</param>
    /// <returns>
    /// The string with every balanced pair of parentheses removed, including nested pairs and their content.
    /// Unmatched parentheses are kept, and any remaining full-width parentheses are converted to half-width.
    /// </returns>
    /// <example>
    /// <code>
    /// string result = "Hello (World)".DeleteBrackets();
    /// // result: "Hello "
    ///
    /// string result2 = "北京（中国）".DeleteBrackets();
    /// // result2: "北京"
    ///
    /// string result3 = "a(b(c)d)e".DeleteBrackets();
    /// // result3: "ae"
    /// </code>
    /// </example>
    public static string DeleteBrackets(this string value)
    {
        var str = value.Replace("（", "(").Replace("）", ")");
        if (str.IndexOf('(') < 0)
        {
            return str;
        }

        // 用栈配对括号：每个右括号与最近的未配对左括号配对，没有配对的括号原样保留
        var closeIndexOf = new int[str.Length];
        var openIndexes = new Stack<int>();
        for (var i = 0; i < str.Length; i++)
        {
            if (str[i] == '(')
            {
                openIndexes.Push(i);
            }
            else if (str[i] == ')' && openIndexes.Count > 0)
            {
                closeIndexOf[openIndexes.Pop()] = i;
            }
        }

        // 从左到右扫描，遇到已配对的左括号就整体跳到它的右括号，嵌套的配对随之一并跳过
        var result = new StringBuilder(str.Length);
        for (var i = 0; i < str.Length; i++)
        {
            if (closeIndexOf[i] > 0)
            {
                i = closeIndexOf[i];
                continue;
            }

            result.Append(str[i]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Splits the current <see cref="string"/> by a regular expression and returns a List&lt;<see cref="string"/>&gt;.
    /// </summary>
    /// <param name="value">The <see cref="string"/> to split.</param>
    /// <param name="symbol">
    /// The delimiter as a regular expression, matched case-insensitively with a one-second timeout. Defaults to CRLF (\r\n).
    /// Regex metacharacters such as <c>.</c> and <c>|</c> keep their pattern meaning. For a literal separator,
    /// use <see cref="SplitToList(string?, string, StringSplitOptions)"/> or escape it with <see cref="Regex.Escape(string)"/>.
    /// </param>
    /// <returns>A List&lt;<see cref="string"/>&gt; resulting from the split operation. Returns an empty list if the input is null or empty.</returns>
    /// <remarks>
    /// <c>"1.2.3".ToSplitList(".")</c> returns six empty strings because <c>.</c> matches every character,
    /// and <c>"a|b|c".ToSplitList("|")</c> returns seven items because an empty alternation matches everywhere.
    /// </remarks>
    /// <example>
    /// <code>
    /// string text = "line1\r\nline2\r\nline3";
    /// List&lt;string&gt; lines = text.ToSplitList();
    /// // lines: ["line1", "line2", "line3"]
    ///
    /// List&lt;string&gt; parts = "part1part2part3".ToSplitList(@"\d");
    /// // parts: ["part", "part", "part", ""]
    /// </code>
    /// </example>
    public static List<string> ToSplitList(this string value, string symbol = "\r\n")
    {
        if (value.IsNullOrEmpty())
        {
            return Enumerable.Empty<string>().ToList();
        }

        return new List<string>(Regex.Split(value, symbol, RegexOptions.IgnoreCase, s_regexSplitTimeout));
    }

}
