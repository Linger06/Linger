using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    /// <summary>
    /// Removes white spaces from the specified string.
    /// </summary>
    /// <param name="value">The specified string.</param>
    /// <returns>The string without white spaces.</returns>
    public static string ToNotSpaceString(this string? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return value.Trim();
    }

    /// <summary>
    /// Converts a scientific notation string to its equivalent <see cref="decimal"/> value.
    /// </summary>
    /// <param name="strData">The input string in scientific notation.</param>
    /// <returns>The equivalent <see cref="decimal"/> value.</returns>
    /// <exception cref="FormatException">Thrown when the input string is not in scientific notation.</exception>
    public static decimal ToDecimal2(this string strData)
    {
        decimal dData;
        if (strData.ToUpper().Contains('E'))
        {
            dData = Convert.ToDecimal(decimal.Parse(strData, NumberStyles.Float));
        }
        else
        {
            throw new FormatException(nameof(strData));
        }

        return dData;
    }

    /// <summary>
    /// Removes parentheses and their contents from the string.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <returns>The string without parentheses and their contents.</returns>
    public static string DeleteBrackets(this string value)
    {
        var str = value.Replace("（", "(").Replace("）", ")");
        return Regex.Replace(str.Replace("（", "(").Replace("）", ")"), @"\([^\(]*\)", "");
    }

    /// <summary>
    /// Extracts the content within the first pair of parentheses in the string.
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <returns>The content within the first pair of parentheses, or the original string if no complete brackets are found.</returns>
    public static string? GetBracketsContent(this string? value)
    {
        if (value.IsNullOrEmpty())
            return value;

        int startIndex = value.IndexOf('(');
        int endIndex = value.IndexOf(')', startIndex + 1);

        if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
            return value;

        return value.Substring(startIndex + 1, endIndex - startIndex - 1);
    }

    /// <summary>
    /// Removes the specified prefix and suffix (single character) from the string.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <param name="value">The character to remove.</param>
    /// <returns>The string without the specified prefix and suffix.</returns>
    public static string DelPrefixAndSuffix(this string str, char value)
    {
        return str.DelPrefixAndSuffix(value.ToString());
    }

    /// <summary>
    /// Removes the specified prefix and suffix (multiple characters) from the string.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <param name="value">The string to remove.</param>
    /// <returns>The string without the specified prefix and suffix.</returns>
    public static string DelPrefixAndSuffix(this string str, string value)
    {
        if (str.StartsWith(value))
        {
            str = str.Substring(value.Length);
        }

        if (str.EndsWith(value))
        {
            str = str.Substring(0, str.LastIndexOf(value, StringComparison.Ordinal));
        }

        return str;
    }

    public static byte[] ToMd5HashByte(this string input)
    {
        var inputBytes = Encoding.ASCII.GetBytes(input);
#if NET5_0_OR_GREATER
        return MD5.HashData(inputBytes);
#else
        using var md5 = MD5.Create();
        return md5.ComputeHash(inputBytes);
#endif
    }

    public static string ToMd5HashCode(this string input)
    {
        var hashBytes = input.ToMd5HashByte();
        return hashBytes.ToMd5HashCode();
    }
}