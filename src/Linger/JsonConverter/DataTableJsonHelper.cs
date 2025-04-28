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
        ArgumentNullException.ThrowIfNull(jsonWriter);
        ArgumentNullException.ThrowIfNull(value);

        jsonWriter.WriteStartArray();

        // 预先创建列到列名的映射，避免在每行循环中重复获取
        var columnNameMap = new string[value.Columns.Count];
        for (int i = 0; i < value.Columns.Count; i++)
        {
            columnNameMap[i] = value.Columns[i].ColumnName.Trim();
        }

        // 预先确定每列的类型，避免在每个单元格都进行类型判断
        var writeActions = new Dictionary<int, Action<Utf8JsonWriter, string, object>>(value.Columns.Count);
        for (int i = 0; i < value.Columns.Count; i++)
        {
            var columnType = value.Columns[i].DataType;

            // 根据列类型选择合适的写入方法
            if (columnType == typeof(bool)) writeActions[i] = (writer, key, val) => writer.WriteBoolean(key, (bool)val);
            else if (columnType == typeof(byte)) writeActions[i] = (writer, key, val) => writer.WriteNumber(key, (byte)val);
            else if (columnType == typeof(sbyte)) writeActions[i] = (writer, key, val) => writer.WriteNumber(key, (sbyte)val);
            else if (columnType == typeof(decimal)) writeActions[i] = (writer, key, val) => writer.WriteNumber(key, (decimal)val);
            else if (columnType == typeof(double)) writeActions[i] = (writer, key, val) => writer.WriteNumber(key, (double)val);
            else if (columnType == typeof(float)) writeActions[i] = (writer, key, val) => writer.WriteNumber(key, (float)val);
            else if (columnType == typeof(short)) writeActions[i] = (writer, key, val) => writer.WriteNumber(key, (short)val);
            else if (columnType == typeof(int)) writeActions[i] = (writer, key, val) => writer.WriteNumber(key, (int)val);
            else if (columnType == typeof(ushort)) writeActions[i] = (writer, key, val) => writer.WriteNumber(key, (ushort)val);
            else if (columnType == typeof(uint)) writeActions[i] = (writer, key, val) => writer.WriteNumber(key, (uint)val);
            else if (columnType == typeof(ulong)) writeActions[i] = (writer, key, val) => writer.WriteNumber(key, (ulong)val);
            else if (columnType == typeof(long)) writeActions[i] = (writer, key, val) => writer.WriteNumber(key, (long)val);
            else if (columnType == typeof(DateTime)) writeActions[i] = (writer, key, val) => writer.WriteString(key, (DateTime)val);
            else if (columnType == typeof(Guid)) writeActions[i] = (writer, key, val) => writer.WriteString(key, (Guid)val);
            else writeActions[i] = (writer, key, val) => writer.WriteString(key, val?.ToString() ?? string.Empty);
        }

        // 遍历行和列
        foreach (DataRow row in value.Rows)
        {
            jsonWriter.WriteStartObject();

            for (int i = 0; i < value.Columns.Count; i++)
            {
                string key = columnNameMap[i];
                object cellValue = row[i];

                // 处理DBNull值
                if (cellValue == DBNull.Value)
                {
                    jsonWriter.WriteNull(key);
                    continue;
                }

                // 使用预先确定的写入方法
                writeActions[i](jsonWriter, key, cellValue);
            }

            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndArray();
    }
}
#endif
