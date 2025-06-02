using System.Data;
using System.Text.Json;
using Linger.JsonConverter;

namespace Linger.Extensions.Core;

public static partial class StringExtensions
{
    /// <summary>
    /// Gets the prefix of the specified email string.
    /// </summary>
    /// <param name="value">The email string to get the prefix from.</param>
    /// <returns>If the string is null or empty, returns <see cref="string.Empty"/>; if the string is not a valid email, returns <see cref="string.Empty"/>; otherwise, returns the substring before the "@" symbol.</returns>
    public static string GetEmailPrefix(this string? value)
    {
        if (value.IsNullOrEmpty() || !value.IsEmail())
            return string.Empty;

        int atIndex = value.IndexOf('@');
        if (atIndex <= 0)
            return string.Empty;

#if NET6_0_OR_GREATER
        return value.AsSpan(0, atIndex).ToString();
#else
        return value.Substring(0, atIndex);
#endif
    }

#if !NETFRAMEWORK || NET462_OR_GREATER
    private static readonly JsonSerializerOptions s_readOptions = new()
    {
        WriteIndented = true,
        Converters = { new DataTableJsonConverter() }
    };

    /// <summary>
    /// Converts the specified JSON string to a DataTable.
    /// </summary>
    /// <param name="json">The JSON data.</param>
    /// <returns>The DataTable representation of the JSON data.</returns>
    public static DataTable? ToDataTable(this string json)
    {
        if (json.IsNullOrEmpty()) return null;
        return JsonSerializer.Deserialize<DataTable>(json, s_readOptions);
    }
#endif

#if !NET8_0_OR_GREATER
    public static bool StartsWith(this string value, char prefix)
    {
        return value.StartsWith(prefix.ToString());
    }

    public static bool EndsWith(this string value, char suffix)
    {
        return value.EndsWith(suffix.ToString());
    }
#endif
}
