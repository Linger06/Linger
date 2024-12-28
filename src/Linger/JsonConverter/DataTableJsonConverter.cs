#if !NETFRAMEWORK || NET462_OR_GREATER

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Linger.JsonConverter;

/// <summary>
/// A custom JSON converter for <see cref="DataTable"/> objects.
/// </summary>
public class DataTableJsonConverter : JsonConverter<DataTable>
{
    /// <summary>
    /// Reads and converts the JSON to a <see cref="DataTable"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted <see cref="DataTable"/>.</returns>
    public override DataTable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DataTableJsonHelper.ReadDataTable(ref reader);
    }

    /// <summary>
    /// Writes a <see cref="DataTable"/> as JSON.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, DataTable value, JsonSerializerOptions options)
    {
        DataTableJsonHelper.WriteDataTable(writer, value);
    }
}

#endif
