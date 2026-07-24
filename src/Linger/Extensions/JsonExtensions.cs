#if !NETFRAMEWORK || NET462_OR_GREATER
using System.Text.Json.Serialization;
using Linger.Extensions.Core;

namespace Linger.Extensions;

/// <summary>
/// Json extensions
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    /// Converts an object to a JSON string using the specified JsonSerializerOptions.
    /// </summary>
    /// <param name="data">The object to serialize.</param>
    /// <param name="jsonSerializerOptions">The options to use for serialization.</param>
    /// <returns>A JSON string representation of the object.</returns>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John", Age = 30 };
    /// var json = obj.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    /// // json: "{\n  \"Name\": \"John\",\n  \"Age\": 30\n}"
    /// </code>
    /// </example>
    public static string ToJsonString(this object? data, JsonSerializerOptions? jsonSerializerOptions)
    {
        return JsonSerializer.Serialize(data, jsonSerializerOptions);
    }

    /// <summary>
    /// Converts an object to a JSON string using default JsonSerializerOptions.
    /// </summary>
    /// <param name="data">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John", Age = 30 };
    /// var json = obj.ToJsonString();
    /// // json: "{\n  \"Name\": \"John\",\n  \"Age\": 30\n}"
    /// </code>
    /// </example>
    public static string ToJsonString(this object? data)
    {
        var serializeOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        return data.ToJsonString(serializeOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of type T using the specified JsonSerializerOptions.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The JSON string to deserialize.</param>
    /// <param name="jsonSerializerOptions">The options to use for deserialization.</param>
    /// <returns>The deserialized object.</returns>
    /// <example>
    /// <code>
    /// var json = "{\"Name\":\"John\",\"Age\":30}";
    /// var obj = json.Deserialize&lt;Person&gt;(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    /// // obj: Person { Name = "John", Age = 30 }
    /// </code>
    /// </example>
    public static T? Deserialize<T>(this string value, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.Deserialize<T>(value, jsonSerializerOptions);
    }

    public static DataTable JsonElementToDataTable(this JsonElement dataRoot)
    {
        var dataTable = new DataTable();

        var elements = new List<JsonElement>();
        var columnTypes = new Dictionary<string, Type>(StringComparer.Ordinal);

        foreach (JsonElement element in dataRoot.EnumerateArray())
        {
            elements.Add(element);

            foreach (JsonProperty col in element.EnumerateObject())
            {
                var inferredType = InferColumnType(col.Value);
                if (columnTypes.TryGetValue(col.Name, out var existingType))
                {
                    columnTypes[col.Name] = MergeColumnType(existingType, inferredType);
                }
                else
                {
                    columnTypes[col.Name] = inferredType;
                }
            }
        }

        foreach (var columnType in columnTypes)
        {
            var dataColumn = new DataColumn(columnType.Key, columnType.Value)
            {
                AllowDBNull = true
            };
            dataTable.Columns.Add(dataColumn);
        }

        foreach (JsonElement element in elements)
        {
            DataRow row = dataTable.NewRow();
            foreach (DataColumn column in dataTable.Columns)
            {
                row[column] = DBNull.Value;
            }

            foreach (JsonProperty col in element.EnumerateObject())
            {
                row[col.Name] = col.Value.JsonElementToTypedValue() ?? DBNull.Value;
            }

            dataTable.Rows.Add(row);
        }

        return dataTable;
    }

    private static Type InferColumnType(this JsonElement jsonElement)
    {
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.String:
                return typeof(string);
            case JsonValueKind.Number:
                if (jsonElement.TryGetInt64(out _))
                {
                    return typeof(long);
                }

                return typeof(double);
            case JsonValueKind.True:
            case JsonValueKind.False:
                return typeof(bool);
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return typeof(object);
            case JsonValueKind.Object:
            case JsonValueKind.Array:
                throw new NotSupportedException();
            default:
                return typeof(object);
        }
    }

    private static Type MergeColumnType(Type currentType, Type candidateType)
    {
        if (currentType == candidateType)
        {
            return currentType;
        }

        if (currentType == typeof(object))
        {
            return candidateType;
        }

        if (candidateType == typeof(object))
        {
            return currentType;
        }

        if ((currentType == typeof(long) && candidateType == typeof(double)) ||
            (currentType == typeof(double) && candidateType == typeof(long)))
        {
            return typeof(double);
        }

        return typeof(object);
    }

    private static object? JsonElementToTypedValue(this JsonElement jsonElement)
    {
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Object:
            case JsonValueKind.Array:
                throw new NotSupportedException();
            case JsonValueKind.String:
                return jsonElement.ToString();
            case JsonValueKind.Number:
                if (jsonElement.TryGetInt64(out var longValue))
                {
                    return longValue;
                }
                else
                {
                    return jsonElement.GetDouble();
                }
            case JsonValueKind.True:
            case JsonValueKind.False:
                return jsonElement.GetBoolean();
            case JsonValueKind.Undefined:
            case JsonValueKind.Null:
                return null;
            default:
                return jsonElement.ToString();
        }
    }

}

#endif
