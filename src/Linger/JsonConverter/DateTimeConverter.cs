#if !NETFRAMEWORK || NET462_OR_GREATER

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Linger.JsonConverter;

/// <summary>
/// A custom JSON converter for <see cref="DateTime"/> objects.
/// </summary>
/// <example>
/// <code>
/// var options = new JsonSerializerOptions
/// {
///     Converters = { new DateTimeConverter() }
/// };
/// var json = JsonSerializer.Serialize(DateTime.Now, options);
/// // json will be a string representation of the current date and time
/// var dateTime = JsonSerializer.Deserialize&lt;DateTime>(json, options);
/// // dateTime will be the deserialized DateTime object
/// </code>
/// </example>
public class DateTimeConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// Reads and converts the JSON to a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted <see cref="DateTime"/>.</returns>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (DateTime.TryParse(reader.GetString(), out DateTime date))
            {
                return date;
            }
        }

        return reader.GetDateTime();
    }

    /// <summary>
    /// Writes a <see cref="DateTime"/> as JSON.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        if (value.Hour == 0 && value is { Minute: 0, Second: 0 })
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd", ExtensionMethodSetting.DefaultCulture));
        }
        else
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss", ExtensionMethodSetting.DefaultCulture));
        }
    }
}

/// <summary>
/// A custom JSON converter for nullable <see cref="DateTime"/> objects.
/// </summary>
/// <example>
/// <code>
/// var options = new JsonSerializerOptions
/// {
///     Converters = { new DateTimeNullConverter() }
/// };
/// var json = JsonSerializer.Serialize&lt;DateTime?>(DateTime.Now, options);
/// // json will be a string representation of the current date and time
/// var dateTime = JsonSerializer.Deserialize&lt;DateTime?>(json, options);
/// // dateTime will be the deserialized nullable DateTime object
/// </code>
/// </example>
public class DateTimeNullConverter : JsonConverter<DateTime?>
{
    /// <summary>
    /// Reads and converts the JSON to a nullable <see cref="DateTime"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted nullable <see cref="DateTime"/>.</returns>
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateTime = reader.GetString();
        return string.IsNullOrEmpty(dateTime) ? null : DateTime.Parse(dateTime, ExtensionMethodSetting.DefaultCulture);
    }

    /// <summary>
    /// Writes a nullable <see cref="DateTime"/> as JSON.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is { Hour: 0, Minute: 0, Second: 0 })
        {
            writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd", ExtensionMethodSetting.DefaultCulture));
        }
        else
        {
            writer.WriteStringValue(value?.ToString("yyyy-MM-dd HH:mm:ss", ExtensionMethodSetting.DefaultCulture));
        }
    }
}

#endif
