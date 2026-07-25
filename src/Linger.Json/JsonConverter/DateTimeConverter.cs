#if !NETFRAMEWORK || NET462_OR_GREATER

using System.Text.Json.Serialization;

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
            if (DateTimeJsonParser.TryParse(reader.GetString(), out DateTime date))
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

            if (DateTimeJsonParser.TryParse(dateTime, out DateTime date))
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

internal static class DateTimeJsonParser
{
    private static readonly string[] s_extraFormats =
    {
        "M/d/yyyy h:mm:ss tt",
        "M/d/yyyy hh:mm:ss tt",
        "yyyy/M/d H:mm:ss",
        "yyyy/M/d h:mm:ss tt",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy/MM/dd HH:mm:ss"
    };

    private const DateTimeStyles ParseStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind;

    public static bool TryParse(string? value, out DateTime result)
    {
        result = default;
        if (value is null || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, ParseStyles, out result))
        {
            return true;
        }

        return DateTime.TryParseExact(
            trimmed,
            s_extraFormats,
            CultureInfo.InvariantCulture,
            ParseStyles,
            out result);
    }
}

#endif
