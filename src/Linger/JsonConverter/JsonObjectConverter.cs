#if !NETFRAMEWORK || NET462_OR_GREATER

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Linger.JsonConverter;

/// <summary>
/// A converter that converts System.Object similar to Newtonsoft's JSON.Net. Only primitives are
/// the same; arrays and objects do not result in the same types.
/// </summary>
/// <example>
/// <code>
/// var options = new JsonSerializerOptions
/// {
///     Converters = { new JsonObjectConverter() }
/// };
/// var json = JsonSerializer.Serialize(new { Name = "John", Age = 30 }, options);
/// // json will be "{\"Name\":\"John\",\"Age\":30}"
/// var obj = JsonSerializer.Deserialize&lt;object&gt;(json, options);
/// // obj will be a JsonElement representing the JSON object
/// </code>
/// </example>
public class JsonObjectConverter : JsonConverter<object>
{
    /// <summary>
    /// Reads and converts the JSON to an <see cref="object"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted <see cref="object"/>.</returns>
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Number when reader.TryGetInt64(out var l):
                return l;
            case JsonTokenType.Number:
                return reader.GetDouble();
            case JsonTokenType.String when reader.TryGetDateTime(out DateTime datetime):
                return datetime;
            case JsonTokenType.String:
                return reader.GetString();
            default:
                {
                    // Use JsonElement as fallback. Newtonsoft uses JArray or JObject.
                    using var document = JsonDocument.ParseValue(ref reader);
                    return document.RootElement.Clone();
                }
        }
    }

    /// <summary>
    /// Writes an <see cref="object"/> as JSON.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value.GetType() == typeof(object))
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
        }
        else
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}

#endif
