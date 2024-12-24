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
    /// <param name="source">The DataTable to write.</param>
    public static void WriteDataTable(Utf8JsonWriter jsonWriter, DataTable source)
    {
        jsonWriter.WriteStartArray();
        foreach (DataRow dr in source.Rows)
        {
            jsonWriter.WriteStartObject();
            foreach (DataColumn col in source.Columns)
            {
                var key = col.ColumnName.Trim();
                var value = dr[col];
                if (value is DBNull)
                {
                    jsonWriter.WriteNull(key);
                }
                else
                {
                    var valueString = value.ToString();
                    switch (col.DataType.FullName)
                    {
                        case "System.Guid":
                        case "System.Char":
                        case "System.String":
                            jsonWriter.WriteString(key, valueString);
                            break;
                        case "System.Boolean":
                            _ = bool.TryParse(valueString, out var boolValue);
                            jsonWriter.WriteBoolean(key, boolValue);
                            break;
                        case "System.DateTime":
                            _ = DateTime.TryParse(valueString, out DateTime dateValue);
                            jsonWriter.WriteString(key, dateValue);
                            break;
                        case "System.TimeSpan":
                            _ = TimeSpan.TryParse(valueString, out TimeSpan timeSpanValue);
                            jsonWriter.WriteString(key, timeSpanValue.ToString());
                            break;
                        case "System.Double":
                            _ = double.TryParse(valueString, out var doubleValue2);
                            jsonWriter.WriteNumber(key, doubleValue2);
                            break;
                        case "System.Single":
                            _ = float.TryParse(valueString, out var floatValue);
                            jsonWriter.WriteNumber(key, floatValue);
                            break;
                        case "System.Byte":
                        case "System.SByte":
                        case "System.Decimal":
                        case "System.Int16":
                        case "System.Int32":
                        case "System.Int64":
                        case "System.UInt16":
                        case "System.UInt32":
                        case "System.UInt64":
                            if (long.TryParse(valueString, out var intValue))
                            {
                                jsonWriter.WriteNumber(key, intValue);
                            }
                            else
                            {
                                _ = double.TryParse(valueString, out var doubleValue);
                                jsonWriter.WriteNumber(key, doubleValue);
                            }
                            break;
                        default:
                            jsonWriter.WriteString(key, valueString);
                            break;
                    }
                }
            }

            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndArray();
    }
}
#endif
