#if !NETFRAMEWORK || NET462_OR_GREATER

using Linger.Json.JsonConverter;

namespace Linger.Extensions.Data;

/// <summary>
/// JSON extensions for <see cref="DataTable"/> values.
/// </summary>
public static class DataTableJsonExtensions
{
    /// <summary>
    /// Converts the current <see cref="DataTable"/> to a JSON string.
    /// </summary>
    /// <param name="dataTable">The <see cref="DataTable"/> to convert.</param>
    /// <returns>A JSON string representing <paramref name="dataTable"/>.</returns>
    public static string ToJsonString(this DataTable? dataTable)
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new DataTableJsonConverter(), new DateTimeConverter() }
        };

        return JsonSerializer.Serialize(dataTable, options);
    }
}

#endif
