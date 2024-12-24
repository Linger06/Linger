#if !NETFRAMEWORK || NET462_OR_GREATER

using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linger.Extensions;

namespace Linger.JsonConverter;

/// <summary>
/// A custom JSON converter for <see cref="DataSet"/> objects.
/// </summary>
public class DataSetConverter : JsonConverter<DataSet>
{
    /// <summary>
    /// Reads and converts the JSON to a <see cref="DataSet"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted <see cref="DataSet"/>.</returns>
    public override DataSet Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        JsonElement rootElement = jsonDoc.RootElement;
        var dataSet = new DataSet();

        foreach (JsonElement tableElement in rootElement.EnumerateArray())
        {
            DataTable dataTable = tableElement.JsonElementToDataTable();
            dataSet.Tables.Add(dataTable);
        }

        return dataSet;
    }

    /// <summary>
    /// Writes a <see cref="DataSet"/> as JSON.
    /// </summary>
    /// <param name="jsonWriter">The writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter jsonWriter, DataSet value, JsonSerializerOptions options)
    {
        jsonWriter.WriteStartArray();
        foreach (DataTable table in value.Tables)
        {
            DataTableJsonHelper.WriteDataTable(jsonWriter, table);
        }
        jsonWriter.WriteEndArray();
    }
}

#endif
