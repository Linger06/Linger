#if !NETFRAMEWORK || NET462_OR_GREATER

using System.Dynamic;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
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
    public static string ToJsonString(this object data)
    {
        var serializeOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        return data.ToJsonString(serializeOptions);
    }

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John", Age = 30 };
    /// var json = obj.SerializeJson();
    /// // json: "{\"Name\":\"John\",\"Age\":30}"
    /// </code>
    /// </example>
    public static string SerializeJson<T>(this T value)
    {
        return value.SerializeJson(Encoding.Default);
    }

    /// <summary>
    /// Serializes an object to a JSON string using the specified encoding.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="encoding">The encoding to use.</param>
    /// <returns>A JSON string representation of the object.</returns>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John", Age = 30 };
    /// var json = obj.SerializeJson(Encoding.UTF8);
    /// // json: "{\"Name\":\"John\",\"Age\":30}"
    /// </code>
    /// </example>
    public static string SerializeJson<T>(this T value, Encoding encoding)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var serializer = new DataContractJsonSerializer(typeof(T));
        using var memoryStream = new MemoryStream();
        serializer.WriteObject(memoryStream, value);
        return encoding.GetString(memoryStream.ToArray());
    }

    /// <summary>
    /// Serializes an object to a JSON string using the specified JsonSerializerOptions.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="jsonSerializerOptions">The options to use for serialization.</param>
    /// <returns>A JSON string representation of the object.</returns>
    /// <example>
    /// <code>
    /// var obj = new { Name = "John", Age = 30 };
    /// var json = obj.Serialize(new JsonSerializerOptions { WriteIndented = true });
    /// // json: "{\n  \"Name\": \"John\",\n  \"Age\": 30\n}"
    /// </code>
    /// </example>
    public static string? Serialize<T>(this T value, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return value?.ToJsonString(jsonSerializerOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The JSON string to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    /// <example>
    /// <code>
    /// var json = "{\"Name\":\"John\",\"Age\":30}";
    /// var obj = json.DeserializeJson&lt;Person&gt;();
    /// // obj: Person { Name = "John", Age = 30 }
    /// </code>
    /// </example>
    public static T? DeserializeJson<T>(this string value)
    {
        var serializer = new DataContractJsonSerializer(typeof(T));

        using var stream = new MemoryStream(Encoding.Default.GetBytes(value));
        return (T?)serializer.ReadObject(stream);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of type T using the specified encoding.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The JSON string to deserialize.</param>
    /// <param name="encoding">The encoding to use.</param>
    /// <returns>The deserialized object.</returns>
    /// <example>
    /// <code>
    /// var json = "{\"Name\":\"John\",\"Age\":30}";
    /// var obj = json.DeserializeJson&lt;Person&gt;(Encoding.UTF8);
    /// // obj: Person { Name = "John", Age = 30 }
    /// </code>
    /// </example>
    public static T? DeserializeJson<T>(this string value, Encoding encoding)
    {
        var serializer = new DataContractJsonSerializer(typeof(T));

        using var stream = new MemoryStream(encoding.GetBytes(value));
        return (T?)serializer.ReadObject(stream);
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

    /// <summary>
    /// Deserializes a JSON string to a dynamic object.
    /// </summary>
    /// <param name="data">The JSON string to deserialize.</param>
    /// <returns>A dynamic object representation of the JSON string.</returns>
    /// <example>
    /// <code>
    /// var json = "{\"Name\":\"John\",\"Age\":30}";
    /// dynamic obj = json.DeserializeDynamicJsonObject();
    /// // obj.Name: "John"
    /// // obj.Age: 30
    /// </code>
    /// </example>
    public static dynamic DeserializeDynamicJsonObject(this string data)
    {
        return new JsonTextAccessor(JsonSerializer.Deserialize<JsonElement>(data));
    }

    public static DataTable JsonElementToDataTable(this JsonElement dataRoot)
    {
        var dataTable = new DataTable();
        bool firstPass = true;
        foreach (JsonElement element in dataRoot.EnumerateArray())
        {
            DataRow row = dataTable.NewRow();
            dataTable.Rows.Add(row);
            foreach (JsonProperty col in element.EnumerateObject())
            {
                if (firstPass)
                {
                    JsonElement colValue = col.Value;
                    dataTable.Columns.Add(new DataColumn(col.Name, colValue.ValueKind.ValueKindToType(colValue.ToString())));
                }
                row[col.Name] = col.Value.JsonElementToTypedValue();
            }
            firstPass = false;
        }

        return dataTable;
    }

    private static Type ValueKindToType(this JsonValueKind valueKind, string value)
    {
        switch (valueKind)
        {
            case JsonValueKind.String:
                return typeof(string);
            case JsonValueKind.Number:
                if (long.TryParse(value, out _))
                {
                    return typeof(long);
                }
                else
                {
                    return typeof(double);
                }
            case JsonValueKind.True:
            case JsonValueKind.False:
                return typeof(bool);
            case JsonValueKind.Undefined:
                throw new NotSupportedException();
            case JsonValueKind.Object:
                return typeof(object);
            case JsonValueKind.Array:
                return typeof(Array);
            case JsonValueKind.Null:
                throw new NotSupportedException();
            default:
                return typeof(object);
        }
    }

    private static object? JsonElementToTypedValue(this JsonElement jsonElement)
    {
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Object:
            case JsonValueKind.Array:
                throw new NotSupportedException();
            case JsonValueKind.String:
                if (jsonElement.TryGetGuid(out Guid guidValue))
                {
                    return guidValue;
                }
                else
                {
                    if (jsonElement.TryGetDateTime(out DateTime datetime))
                    {
                        // If an offset was provided, use DateTimeOffset.
                        if (datetime.Kind == DateTimeKind.Local)
                        {
                            if (jsonElement.TryGetDateTimeOffset(out DateTimeOffset datetimeOffset))
                            {
                                return datetimeOffset;
                            }
                        }
                        return datetime;
                    }
                    return jsonElement.ToString();
                }
            case JsonValueKind.Number:
                if (jsonElement.TryGetInt64(out long longValue))
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

    /// <summary>
    /// Provides dynamic access to JSON properties.
    /// </summary>
    /// <param name="content">The JsonElement to wrap.</param>
    /// <example>
    /// <code>
    /// var json = "{\"Name\":\"John\",\"Age\":30}";
    /// var element = JsonSerializer.Deserialize&lt;JsonElement&gt;(json);
    /// dynamic accessor = new JsonTextAccessor(element);
    /// // accessor.Name: "John"
    /// // accessor.Age: 30
    /// </code>
    /// </example>
    public class JsonTextAccessor(JsonElement content) : DynamicObject
    {
        /// <summary>
        /// Tries to get a member by name.
        /// </summary>
        /// <param name="binder">The binder containing the member name.</param>
        /// <param name="result">The result of the member access.</param>
        /// <returns>True if the member was found; otherwise, false.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = null;

            if (content.TryGetProperty(binder.Name, out JsonElement value))
            {
                result = Obtain(value);
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Obtains the value of a JsonElement.
        /// </summary>
        /// <param name="element">The JsonElement to obtain the value from.</param>
        /// <returns>The corresponding value.</returns>
        private static object? Obtain(in JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Null => null,
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.Array => element.EnumerateArray().Select(item => Obtain(item)).ToList(),
                _ => new JsonTextAccessor(element)
            };
        }
    }
}

#endif
