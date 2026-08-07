#if !NETFRAMEWORK || NET462_OR_GREATER
namespace Linger.Extensions;

/// <summary>
/// Extensions for converting <see cref="JsonElement"/> values to <see cref="DataTable"/> instances.
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    /// Converts a JSON array to a <see cref="DataTable"/> with inferred column types.
    /// </summary>
    /// <param name="dataRoot">The JSON array to convert.</param>
    /// <returns>A <see cref="DataTable"/> containing one row for each JSON object.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="dataRoot"/> is not a JSON array.</exception>
    /// <exception cref="NotSupportedException">A JSON object or array is encountered as a column value.</exception>
    public static DataTable JsonElementToDataTable(this JsonElement dataRoot)
    {
        var dataTable = new DataTable();

        var columnTypes = new Dictionary<string, Type?>(StringComparer.Ordinal);

        foreach (JsonElement element in dataRoot.EnumerateArray())
        {
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
            var dataColumn = new DataColumn(columnType.Key, columnType.Value ?? typeof(object))
            {
                AllowDBNull = true
            };
            dataTable.Columns.Add(dataColumn);
        }

        foreach (JsonElement element in dataRoot.EnumerateArray())
        {
            DataRow row = dataTable.NewRow();

            foreach (JsonProperty col in element.EnumerateObject())
            {
                row[col.Name] = col.Value.JsonElementToTypedValue() ?? DBNull.Value;
            }

            dataTable.Rows.Add(row);
        }

        return dataTable;
    }

    private static Type? InferColumnType(this JsonElement jsonElement)
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
                return null;
            case JsonValueKind.Object:
            case JsonValueKind.Array:
                throw new NotSupportedException();
            default:
                return typeof(object);
        }
    }

    private static Type? MergeColumnType(Type? currentType, Type? candidateType)
    {
        if (currentType is null)
        {
            return candidateType;
        }

        if (candidateType is null || currentType == candidateType)
        {
            return currentType;
        }

        if (currentType == typeof(object))
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
