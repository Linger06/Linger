using System.Text;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    public static List<string> SplitToList(this string? value, char separator = ',', StringSplitOptions options = StringSplitOptions.None)
    {
        return value.SplitToArray(separator, options).ToList();
    }

    /// <summary>
    /// Splits the string into a list of strings using the specified delimiter.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <param name="separator">The delimiter to use. Default is carriage return and line feed.</param>
    /// <param name="options"></param>
    /// <returns>A list of strings.</returns>
    public static List<string> SplitToList(this string? value, string separator, StringSplitOptions options = StringSplitOptions.None)
    {
        return value.SplitToArray(separator, options).ToList();
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
        return value.Split(new[] { separator }, options);
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
        return value.Split(new[] { separator }, options);
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
        if (string.IsNullOrEmpty(query))
        {
            return self;
        }

#if NET6_0_OR_GREATER
        bool hasQuery = self.Contains('?');
        char separator = hasQuery ? '&' : '?';
        int totalLength = self.Length + 1 + query.Length;

        return string.Create(totalLength, (self, separator, query), (span, state) =>
        {
            state.self.AsSpan().CopyTo(span);
            int position = state.self.Length;
            span[position++] = state.separator;
            state.query.AsSpan().CopyTo(span[position..]);
        });
#else
        var sb = new StringBuilder(self.Length + query.Length + 1);
        sb.Append(self);
        sb.Append(self.Contains('?') ? '&' : '?');
        sb.Append(query);
        return sb.ToString();
#endif
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

        var estimatedCapacity = self.Length + (data.Count * 20) + 10;
        var sb = new StringBuilder(estimatedCapacity);
        sb.Append(self);
        sb.Append(self.Contains('?') ? '&' : '?');

        bool isFirst = true;
        foreach (DictionaryEntry item in data)
        {
            if (!isFirst)
            {
                sb.Append('&');
            }

            sb.Append(item.Key);
            sb.Append('=');
            sb.Append(item.Value);
            isFirst = false;
        }

        return sb.ToString();
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

        var estimatedCapacity = self.Length + (data.Count * 20) + 10;
        var sb = new StringBuilder(estimatedCapacity);
        sb.Append(self);
        sb.Append(self.Contains('?') ? '&' : '?');

#if NET5_0_OR_GREATER
        for (int i = 0; i < data.Count; i++)
        {
            if (i > 0)
            {
                sb.Append('&');
            }

            var item = data[i];
            sb.Append(item.Key);
            sb.Append('=');
            sb.Append(item.Value);
        }
#else
        bool isFirst = true;
        foreach (var item in data)
        {
            if (!isFirst)
            {
                sb.Append('&');
            }

            sb.Append(item.Key);
            sb.Append('=');
            sb.Append(item.Value);
            isFirst = false;
        }
#endif

        return sb.ToString();
    }
}
