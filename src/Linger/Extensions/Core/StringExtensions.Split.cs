using System.Text;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    public static IEnumerable<string> SplitToList(this string? value, char separator = ',', StringSplitOptions options = StringSplitOptions.None)
    {
        return value.SplitToArray(separator, options);
    }

    /// <summary>
    /// Splits the string into a list of strings using the specified delimiter.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <param name="separator">The delimiter to use. Default is carriage return and line feed.</param>
    /// <param name="options"></param>
    /// <returns>A list of strings.</returns>
    public static IEnumerable<string> SplitToList(this string? value, string separator, StringSplitOptions options = StringSplitOptions.None)
    {
        return value.SplitToArray(separator, options);
    }

    /// <summary>
    /// Splits the string into an array of strings using the specified delimiter.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <param name="separator">The delimiter to use. Default is a comma.</param>
    /// <param name="options"></param>
    /// <returns>An array of strings.</returns>
    public static string[] SplitToArray(this string? value, char separator = ',', StringSplitOptions options = StringSplitOptions.None)
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }

#if NET8_0_OR_GREATER
        return value.Split(separator, options);
#else
        return value.Split([separator], options);
#endif
    }

    public static string[] SplitToArray(this string? value, string separator, StringSplitOptions options = StringSplitOptions.None)
    {
        if (value.IsNullOrEmpty())
        {
            return [];
        }
#if NET8_0_OR_GREATER
        return value.Split(separator, options);
#else
        return value.Split([separator], options);
#endif
    }

    /// <summary>
    /// Appends a query string to the URL.
    /// </summary>
    /// <param name="self">The URL.</param>
    /// <param name="query">The query string to append.</param>
    /// <returns>The URL with the appended query string.</returns>
    public static string AppendQuery(this string self, string query)
    {
        ArgumentNullException.ThrowIfNull(self);

        if (string.IsNullOrEmpty(query))
        {
            return self;
        }

        var queryStart = 0;
        while (queryStart < query.Length && (query[queryStart] == '?' || query[queryStart] == '&'))
        {
            queryStart++;
        }

        if (queryStart == query.Length)
        {
            return self;
        }

        var fragmentStart = self.IndexOf('#');
        if (fragmentStart < 0)
        {
            fragmentStart = self.Length;
        }

        var hasQuery = self.IndexOf('?', 0, fragmentStart) >= 0;
        var hasTrailingSeparator = fragmentStart > 0
            && (self[fragmentStart - 1] == '?' || self[fragmentStart - 1] == '&');
        var separator = hasQuery
            ? hasTrailingSeparator ? string.Empty : "&"
            : "?";

        var builder = new StringBuilder(self.Length + query.Length + separator.Length);
        builder.Append(self, 0, fragmentStart);
        builder.Append(separator);
        builder.Append(query, queryStart, query.Length - queryStart);
        builder.Append(self, fragmentStart, self.Length - fragmentStart);
        return builder.ToString();
    }

    /// <summary>
    /// Appends a query string to the URL using the specified dictionary.
    /// </summary>
    /// <param name="self">The URL.</param>
    /// <param name="data">The dictionary containing the query parameters.</param>
    /// <returns>The URL with the appended query string.</returns>
    public static string AppendQuery(this string self, IDictionary data)
    {
        if (data.Count == 0)
        {
            return self;
        }

        return AppendQuery(self, EnumerateQueryParameters(data), data.Count);
    }

    /// <summary>
    /// Appends a query string to the URL using the specified list of key-value pairs.
    /// </summary>
    /// <param name="self">The URL.</param>
    /// <param name="data">The list of key-value pairs containing the query parameters.</param>
    /// <returns>The URL with the appended query string.</returns>
    public static string AppendQuery(this string self, List<KeyValuePair<string, string>> data)
    {
        if (data.Count == 0)
        {
            return self;
        }

        return AppendQuery(self, data, data.Count);
    }

    private static string AppendQuery(
        string self,
        IEnumerable<KeyValuePair<string, string>> data,
        int count)
    {
        var estimatedCapacity = (count * 20) + 10;
        var sb = new StringBuilder(estimatedCapacity);

        var isFirst = true;
        foreach (var item in data)
        {
            if (!isFirst)
            {
                sb.Append('&');
            }

            sb.Append(Uri.EscapeDataString(item.Key ?? string.Empty));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(item.Value ?? string.Empty));
            isFirst = false;
        }

        return self.AppendQuery(sb.ToString());
    }

    private static IEnumerable<KeyValuePair<string, string>> EnumerateQueryParameters(IDictionary data)
    {
        foreach (DictionaryEntry item in data)
        {
            yield return new KeyValuePair<string, string>(
                item.Key?.ToString() ?? string.Empty,
                item.Value?.ToString() ?? string.Empty);
        }
    }
}
