#if !NETFRAMEWORK || NET462_OR_GREATER
using Linger.Extensions;

namespace Linger.Json.JsonConverter;

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
        for (var i = 0; i < value.Columns.Count; i++)
        {
            columnNameMap[i] = value.Columns[i].ColumnName.Trim();
        }

        // 预先确定每列的类型，避免在每个单元格都进行类型判断
        var writeActions = new Action<Utf8JsonWriter, string, object>[value.Columns.Count];
        for (var i = 0; i < value.Columns.Count; i++)
        {
            writeActions[i] = CreateWriteAction(value.Columns[i].DataType);
        }

        // 遍历行和列
        foreach (DataRow row in value.Rows)
        {
            jsonWriter.WriteStartObject();

            for (var i = 0; i < value.Columns.Count; i++)
            {
                var key = columnNameMap[i];
                var cellValue = row[i];

                // 处理DBNull值
                if (cellValue is DBNull)
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

    private static Action<Utf8JsonWriter, string, object> CreateWriteAction(Type columnType)
    {
        if (columnType.IsEnum)
        {
            return static (writer, key, value) => writer.WriteString(key, value.ToString());
        }

        if (columnType == typeof(Guid))
        {
            return static (writer, key, value) => writer.WriteString(key, (Guid)value);
        }

        return Type.GetTypeCode(columnType) switch
        {
            TypeCode.Boolean => static (writer, key, value) => writer.WriteBoolean(key, (bool)value),
            TypeCode.Byte => static (writer, key, value) => writer.WriteNumber(key, (byte)value),
            TypeCode.SByte => static (writer, key, value) => writer.WriteNumber(key, (sbyte)value),
            TypeCode.Decimal => static (writer, key, value) => writer.WriteNumber(key, (decimal)value),
            TypeCode.Double => static (writer, key, value) => writer.WriteNumber(key, (double)value),
            TypeCode.Single => static (writer, key, value) => writer.WriteNumber(key, (float)value),
            TypeCode.Int16 => static (writer, key, value) => writer.WriteNumber(key, (short)value),
            TypeCode.Int32 => static (writer, key, value) => writer.WriteNumber(key, (int)value),
            TypeCode.UInt16 => static (writer, key, value) => writer.WriteNumber(key, (ushort)value),
            TypeCode.UInt32 => static (writer, key, value) => writer.WriteNumber(key, (uint)value),
            TypeCode.UInt64 => static (writer, key, value) => writer.WriteNumber(key, (ulong)value),
            TypeCode.Int64 => static (writer, key, value) => writer.WriteNumber(key, (long)value),
            TypeCode.DateTime => static (writer, key, value) => writer.WriteString(key, (DateTime)value),
            _ => static (writer, key, value) => writer.WriteString(key, value.ToString() ?? string.Empty)
        };
    }
}
#endif
