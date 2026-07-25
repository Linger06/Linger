#if !NETFRAMEWORK || NET462_OR_GREATER

using Linger.Json.JsonConverter;

namespace Linger.Extensions.Core;

/// <summary>
/// JSON extensions for <see cref="string"/> values.
/// </summary>
public static class StringJsonExtensions
{
    private static readonly JsonSerializerOptions s_readOptions = new()
    {
        WriteIndented = true,
        Converters = { new DataTableJsonConverter() }
    };

    /// <summary>
    /// Converts the specified JSON string to a <see cref="DataTable"/>.
    /// </summary>
    /// <param name="json">The JSON data.</param>
    /// <returns>The <see cref="DataTable"/> representation of the JSON data, or <see langword="null"/> when <paramref name="json"/> is empty.</returns>
    public static DataTable? ToDataTable(this string? json)
    {
        if (json is null || json.Length == 0)
        {
            return null;
        }

        return JsonSerializer.Deserialize<DataTable>(json, s_readOptions);
    }
}

#endif
