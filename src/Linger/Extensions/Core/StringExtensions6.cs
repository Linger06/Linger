using System.Text.RegularExpressions;
namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="string"/> extensions.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// Splits the string into a list of strings using the specified delimiter.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <param name="symbol">The delimiter to use. Default is carriage return and line feed.</param>
    /// <returns>A list of strings.</returns>
    public static List<string> ToSplitList(this string? value, string symbol = "\r\n")
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }

        return Regex.Split(value, symbol, RegexOptions.IgnoreCase).ToList();
    }

    /// <summary>
    /// Splits the string into a list of strings using the specified delimiter.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <param name="symbol">The delimiter to use. Default is a comma.</param>
    /// <returns>A list of strings.</returns>
    public static IEnumerable<string> ToSplitList(this string value, char symbol = ',')
    {
        var value2 = value.ToSplitArray(symbol);
        return value2.ToEnumerable();
    }

    /// <summary>
    /// Splits the string into an array of strings using carriage return and line feed as the delimiter.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <returns>An array of strings.</returns>
    public static string[] ToSplitArrayByCrlf(this string? value)
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }

        return Regex.Split(value, Environment.NewLine, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Splits the string into an array of strings using the specified delimiter.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <param name="symbol">The delimiter to use. Default is a comma.</param>
    /// <returns>An array of strings.</returns>
    public static string[] ToSplitArray(this string? value, char symbol = ',')
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }

        return value.Split(symbol);
    }

    /// <summary>
    /// Appends a query string to the URL.
    /// </summary>
    /// <param name="self">The URL.</param>
    /// <param name="query">The query string to append.</param>
    /// <returns>The URL with the appended query string.</returns>
    public static string AppendQuery(this string self, string query)
    {
        if (self.Contains('?'))
        {
            self += "&" + query;
        }
        else
        {
            self += "?" + query;
        }

        return self;
    }

    /// <summary>
    /// Appends a query string to the URL using the specified dictionary.
    /// </summary>
    /// <param name="self">The URL.</param>
    /// <param name="data">The dictionary containing the query parameters.</param>
    /// <returns>The URL with the appended query string.</returns>
    public static string AppendQuery(this string self, IDictionary data)
    {
        var query = string.Empty;
        foreach (DictionaryEntry item in data)
        {
            query += item.Key + "=" + item.Value + "&";
        }

        if (self.Contains('?'))
        {
            self += "&" + query;
        }
        else
        {
            self += "?" + query;
        }

        return self;
    }

    /// <summary>
    /// Appends a query string to the URL using the specified list of key-value pairs.
    /// </summary>
    /// <param name="self">The URL.</param>
    /// <param name="data">The list of key-value pairs containing the query parameters.</param>
    /// <returns>The URL with the appended query string.</returns>
    public static string AppendQuery(this string self, List<KeyValuePair<string, string>> data)
    {
        var query = string.Empty;
        foreach (KeyValuePair<string, string> item in data)
        {
            query += item.Key + "=" + item.Value + "&";
        }

        if (self.Contains('?'))
        {
            self += "&" + query;
        }
        else
        {
            self += "?" + query;
        }

        return self;
    }
}