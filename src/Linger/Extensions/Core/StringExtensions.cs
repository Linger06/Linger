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
    /// 删除圆括号和括号里的内容
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string DeleteBrackets(this string value)
    {
        var str = value.Replace("（", "(").Replace("）", ")");
        return Regex.Replace(str.Replace("（", "(").Replace("）", ")"), @"\([^\(]*\)", "");
    }

    /// <summary>
    /// 将当前 <see cref="string"/> 转换为 List&lt; <see cref="string"/>&gt;
    /// </summary>
    /// <param name="value"><see cref="string"/></param>
    /// <param name="symbol">分隔符，默认为回车换行</param>
    /// <returns>List&lt;string&gt;</returns>
    public static List<string> ToSplitList(this string value, string symbol = "\r\n")
    {
        if (value.IsNullOrEmpty())
        {
            return Enumerable.Empty<string>().ToList();
        }

        return Regex.Split(value, symbol, RegexOptions.IgnoreCase).ToList();
    }

    /// <summary>
    /// 将当前 <see cref="string"/> 转换为 List&lt; <see cref="string"/>&gt;
    /// </summary>
    /// <param name="value"><see cref="string"/></param>
    /// <param name="symbol">分隔符，默认为英文逗号 <see cref="char"/></param>
    /// <returns>List&lt; <see cref="string"/>&gt;</returns>
    public static IEnumerable<string> ToSplitList(this string value, char symbol = ',')
    {
        var value2 = value.ToSplitArray(symbol);
        return value2.ToEnumerable();
    }

    public static string[] ToSplitArrayByCrlf(this string value)
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }

        return Regex.Split(value, Environment.NewLine, RegexOptions.IgnoreCase);
    }

    public static string[] ToSplitArray(this string value, char symbol = ',')
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }

        return value.Split(symbol);
    }

}
