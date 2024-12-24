
#if NET462_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Linger.JsonConverter;

/// <summary>  
/// A custom JSON converter for <see cref="Type"/> objects.  
/// </summary>  
/// <example>  
/// <code>  
/// var options = new JsonSerializerOptions  
/// {  
///     Converters = { new TypeConverter() }  
/// };  
/// var json = JsonSerializer.Serialize(typeof(string), options);  
/// // json will be "null"  
/// var type = JsonSerializer.Deserialize&lt;Type&gt;(json, options);  
/// // type will be null  
/// </code>  
/// </example>  
public class TypeConverter : JsonConverter<Type>
{
    /// <summary>  
    /// Reads and converts the JSON to a <see cref="Type"/>.  
    /// </summary>  
    /// <param name="reader">The reader.</param>  
    /// <param name="typeToConvert">The type to convert.</param>  
    /// <param name="options">The serializer options.</param>  
    /// <returns>The converted <see cref="Type"/>.</returns>  
    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return null;
    }

    /// <summary>  
    /// Writes a <see cref="Type"/> as JSON.  
    /// </summary>  
    /// <param name="writer">The writer.</param>  
    /// <param name="value">The value to write.</param>  
    /// <param name="options">The serializer options.</param>  
    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        writer.WriteNullValue();
    }
}
#endif
