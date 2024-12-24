#if !NETFRAMEWORK || NET462_OR_GREATER

using System.Text.Json;
using System.Text.Json.Serialization;
using Linger.Extensions.Core;

namespace Linger.JsonConverter;

/// <summary>
/// A custom JSON converter for <see cref="string"/> objects.
/// </summary>
/// <example>
/// <code>
/// var options = new JsonSerializerOptions
/// {
///     Converters = { new JsonStringConverter() }
/// };
/// var json = JsonSerializer.Serialize("Hello, World!", options);
/// // json will be "\"Hello, World!\""
/// var str = JsonSerializer.Deserialize&lt;string>(json, options);
/// // str will be "Hello, World!"
/// </code>
/// </example>
public class JsonStringConverter : JsonConverter<string>
{
    /// <summary>
    /// Reads and converts the JSON to a <see cref="string"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted <see cref="string"/>.</returns>
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                {
                    var stringValue = reader.GetDouble();
                    return stringValue.ToString(ExtensionMethodSetting.DefaultCulture);
                }
            case JsonTokenType.False or JsonTokenType.True:
                return reader.GetBoolean().ToString();
            case JsonTokenType.StartObject:
                reader.Skip();
                return "(not supported)";
            default:
                Console.WriteLine($"Unsupported token type: {reader.TokenType}");
                throw new JsonException($"Unsupported token type: {reader.TokenType}");
        }
    }

    /// <summary>
    /// Writes a <see cref="string"/> as JSON.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value.IsNull())
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}

#endif
