#if !NETFRAMEWORK || NET462_OR_GREATER
using System.Text.Json;
using Linger.Extensions;

namespace Linger.JsonConverter;

/// <summary>
/// DataTableJsonHelper
/// </summary>
public static class DataTableJsonHelper
{
    /// <summary>
    /// Reads a DataTable from a Utf8JsonReader.
    /// </summary>
    /// <param name="reader">The Utf8JsonReader to read from.</param>
    /// <returns>A DataTable populated with the data from the JSON.</returns>
    public static DataTable ReadDataTable(ref Utf8JsonReader reader)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        JsonElement rootElement = jsonDoc.RootElement;
        return rootElement.JsonElementToDataTable();
    }

    /// <summary>
    /// Writes a DataTable to a Utf8JsonWriter.
    /// </summary>
    /// <param name="jsonWriter">The Utf8JsonWriter to write to.</param>
    /// <param name="value">The DataTable to write.</param>
    public static void WriteDataTable(Utf8JsonWriter jsonWriter, DataTable value)
    {
        jsonWriter.WriteStartArray();
        foreach (DataRow dr in value.Rows)
        {
            jsonWriter.WriteStartObject();
            foreach (DataColumn col in value.Columns)
            {
                string key = col.ColumnName.Trim();

                Action<string> action = GetWriteAction(dr, col, jsonWriter);
                action.Invoke(key);

                static Action<string> GetWriteAction(DataRow row, DataColumn column, Utf8JsonWriter writer) =>
                    row[column] switch
                    {
                        // bool
                        bool value => key => writer.WriteBoolean(key, value),

                        // numbers
                        byte value => key => writer.WriteNumber(key, value),
                        sbyte value => key => writer.WriteNumber(key, value),
                        decimal value => key => writer.WriteNumber(key, value),
                        double value => key => writer.WriteNumber(key, value),
                        float value => key => writer.WriteNumber(key, value),
                        short value => key => writer.WriteNumber(key, value),
                        int value => key => writer.WriteNumber(key, value),
                        ushort value => key => writer.WriteNumber(key, value),
                        uint value => key => writer.WriteNumber(key, value),
                        ulong value => key => writer.WriteNumber(key, value),
                        long value => key => writer.WriteNumber(key, value),

                        // strings
                        DateTime value => key => writer.WriteString(key, value),
                        Guid value => key => writer.WriteString(key, value),

                        _ => key => writer.WriteString(key, row[column].ToString())
                    };
            }
            jsonWriter.WriteEndObject();
        }
        jsonWriter.WriteEndArray();
    }
}
#endif
