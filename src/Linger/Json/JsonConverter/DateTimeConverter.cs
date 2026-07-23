#if !NETFRAMEWORK || NET462_OR_GREATER

using System.Text.Json.Serialization;
using Linger.Helper;

namespace Linger.Json.JsonConverter;

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
            if (DateTimeConversionHelper.TryConvertStringToDateTime(reader.GetString(), out DateTime date))
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
        DateTimeJsonFormatting.Write(writer, value);
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
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateTime = reader.GetString();

            if (string.IsNullOrEmpty(dateTime))
            {
                return null;
            }

            if (DateTimeConversionHelper.TryConvertStringToDateTime(dateTime, out DateTime date))
            {
                return date;
            }
        }

        return reader.GetDateTime();
    }

    /// <summary>
    /// Writes a nullable <see cref="DateTime"/> as JSON.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        DateTimeJsonFormatting.Write(writer, value.Value);
    }
}

internal static class DateTimeJsonFormatting
{
    public static void Write(Utf8JsonWriter writer, DateTime value)
    {
        if (value.Kind != DateTimeKind.Unspecified || value.Ticks % TimeSpan.TicksPerSecond != 0)
        {
            writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture));
        }
        else if (value.TimeOfDay == TimeSpan.Zero)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }
    }
}

#endif
