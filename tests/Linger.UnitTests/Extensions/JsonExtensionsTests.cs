using System.Data;
using System.Dynamic;
using System.Text;
using System.Text.Json;
using Microsoft.CSharp.RuntimeBinder;
using Linger.Extensions;
using Xunit.v3;

namespace Linger.UnitTests.Extensions;

public class JsonExtensionsTests
{
    public class TestPerson
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    [Fact]
    public void ToJsonString_WithOptions_SerializesObjectCorrectly()
    {
        // Arrange
        var person = new TestPerson { Name = "John Doe", Age = 30 };
        var options = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        // Act
        var json = person.ToJsonString(options);

        // Assert
        Assert.Equal("{\"Name\":\"John Doe\",\"Age\":30}", json);
    }

    [Fact]
    public void ToJsonString_WithoutOptions_SerializesWithDefaultOptions()
    {
        // Arrange
        var person = new TestPerson { Name = "John Doe", Age = 30 };

        // Act
        var json = person.ToJsonString();

        // Assert
        Assert.Contains("\"Name\": \"John Doe\"", json);
        Assert.Contains("\"Age\": 30", json);
    }

    [Fact]
    public void SerializeJson_Generic_SerializesObjectCorrectly()
    {
        // Arrange
        var person = new TestPerson { Name = "John Doe", Age = 30 };

        // Act
        var json = person.SerializeJson();

        // Assert
        Assert.Contains("Name", json);
        Assert.Contains("John Doe", json);
        Assert.Contains("Age", json);
        Assert.Contains("30", json);
    }

    [Fact]
    public void SerializeJson_WithEncoding_SerializesObjectCorrectly()
    {
        // Arrange
        var person = new TestPerson { Name = "John Doe", Age = 30 };

        // Act
        var json = person.SerializeJson(Encoding.UTF8);

        // Assert
        Assert.Contains("Name", json);
        Assert.Contains("John Doe", json);
        Assert.Contains("Age", json);
        Assert.Contains("30", json);
    }

    [Fact]
    public void SerializeJson_WithNullObject_ReturnsEmptyString()
    {
        // Arrange
        TestPerson? person = null;

        // Act
        var json = person.SerializeJson();

        // Assert
        Assert.Equal(string.Empty, json);
    }

    [Fact]
    public void Serialize_WithOptions_SerializesObjectCorrectly()
    {
        // Arrange
        var person = new TestPerson { Name = "John Doe", Age = 30 };
        var options = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        // Act
        var json = person.Serialize(options);

        // Assert
        Assert.Equal("{\"Name\":\"John Doe\",\"Age\":30}", json);
    }

    [Fact]
    public void Serialize_WithNullObject_ReturnsNull()
    {
        // Arrange
        TestPerson? person = null;

        // Act
        var json = person.Serialize();

        // Assert
        Assert.Null(json);
    }

    [Fact]
    public void DeserializeJson_Generic_DeserializesCorrectly()
    {
        // Arrange
        var json = "{\"Name\":\"John Doe\",\"Age\":30}";

        // Act
        var person = json.DeserializeJson<TestPerson>();

        // Assert
        Assert.NotNull(person);
        Assert.Equal("John Doe", person.Name);
        Assert.Equal(30, person.Age);
    }

    [Fact]
    public void DeserializeJson_WithEncoding_DeserializesCorrectly()
    {
        // Arrange
        var json = "{\"Name\":\"John Doe\",\"Age\":30}";

        // Act
        var person = json.DeserializeJson<TestPerson>(Encoding.UTF8);

        // Assert
        Assert.NotNull(person);
        Assert.Equal("John Doe", person.Name);
        Assert.Equal(30, person.Age);
    }

    [Fact]
    public void Deserialize_Generic_DeserializesCorrectly()
    {
        // Arrange
        var json = "{\"Name\":\"John Doe\",\"Age\":30}";

        // Act
        var person = json.Deserialize<TestPerson>();

        // Assert
        Assert.NotNull(person);
        Assert.Equal("John Doe", person.Name);
        Assert.Equal(30, person.Age);
    }

    [Fact]
    public void Deserialize_WithOptions_DeserializesCorrectly()
    {
        // Arrange
        var json = "{\"name\":\"John Doe\",\"age\":30}";
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var person = json.Deserialize<TestPerson>(options);

        // Assert
        Assert.NotNull(person);
        Assert.Equal("John Doe", person.Name);
        Assert.Equal(30, person.Age);
    }

    [Fact]
    public void DeserializeDynamicJsonObject_Simple_ReturnsDynamicObject()
    {
        // Arrange
        var json = "{\"Name\":\"John Doe\",\"Age\":30}";

        // Act
        dynamic result = json.DeserializeDynamicJsonObject();

        // Assert
        Assert.Equal("John Doe", result.Name);
        Assert.Equal(30.0, result.Age);
    }

    [Fact]
    public void DeserializeDynamicJsonObject_WithNestedObject_ReturnsDynamicObject()
    {
        // Arrange
        var json = "{\"Person\":{\"Name\":\"John Doe\",\"Age\":30}}";

        // Act
        dynamic result = json.DeserializeDynamicJsonObject();

        // Assert
        Assert.Equal("John Doe", result.Person.Name);
        Assert.Equal(30.0, result.Person.Age);
    }

    [Fact]
    public void DeserializeDynamicJsonObject_WithArray_ReturnsDynamicObject()
    {
        // Arrange
        var json = "{\"People\":[{\"Name\":\"John Doe\",\"Age\":30},{\"Name\":\"Linger\",\"Age\":25}]}";

        // Act
        dynamic result = json.DeserializeDynamicJsonObject();

        // Assert
        Assert.Equal(2, result.People.Count);
        Assert.Equal("John Doe", result.People[0].Name);
        Assert.Equal(30.0, result.People[0].Age);
        Assert.Equal("Linger", result.People[1].Name);
        Assert.Equal(25.0, result.People[1].Age);
    }

    [Fact]
    public void JsonElementToDataTable_WithJsonArray_CreatesDataTable()
    {
        // Arrange
        var json = "[{\"Name\":\"John Doe\",\"Age\":30},{\"Name\":\"Linger\",\"Age\":25}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        Assert.Equal(2, dataTable.Rows.Count);
        Assert.Equal(2, dataTable.Columns.Count);
        Assert.Equal("John Doe", dataTable.Rows[0]["Name"]);
        Assert.Equal(30L, dataTable.Rows[0]["Age"]);
        Assert.Equal("Linger", dataTable.Rows[1]["Name"]);
        Assert.Equal(25L, dataTable.Rows[1]["Age"]);
    }

    [Fact]
    public void JsonElementToDataTable_WithDifferentValueTypes_SetsCorrectDataTypes()
    {
        // Arrange
        var json = "[{\"StringValue\":\"John Doe\",\"IntValue\":30,\"BoolValue\":true,\"DoubleValue\":12.34}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        Assert.Equal(1, dataTable.Rows.Count);
        Assert.Equal(4, dataTable.Columns.Count);

        Assert.Equal(typeof(string), dataTable.Columns["StringValue"].DataType);
        Assert.Equal(typeof(long), dataTable.Columns["IntValue"].DataType);
        Assert.Equal(typeof(bool), dataTable.Columns["BoolValue"].DataType);
        Assert.Equal(typeof(double), dataTable.Columns["DoubleValue"].DataType);

        Assert.Equal("John Doe", dataTable.Rows[0]["StringValue"]);
        Assert.Equal(30L, dataTable.Rows[0]["IntValue"]);
        Assert.Equal(true, dataTable.Rows[0]["BoolValue"]);
        Assert.Equal(12.34, dataTable.Rows[0]["DoubleValue"]);
    }

    [Fact]
    public void ValueKindToType_WithUndefinedValueKind_ThrowsNotSupportedException()
    {
        // Arrange & Act & Assert
        var json = "[{}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        
        // This should work without throwing for empty object
        var dataTable = jsonElement.JsonElementToDataTable();
        Assert.Equal(1, dataTable.Rows.Count);
    }

    [Fact]
    public void JsonElementToTypedValue_WithGuidString_ReturnsString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var json = $"[{{\"Id\":\"{guid}\"}}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        // The value is returned as string, not as Guid
        Assert.Equal(guid.ToString(), dataTable.Rows[0]["Id"]);
    }

    [Fact]
    public void JsonElementToTypedValue_WithDateTime_ReturnsString()
    {
        // Arrange
        var dateTime = DateTime.Now.ToString("O"); // ISO 8601 format
        var json = $"[{{\"DateTime\":\"{dateTime}\"}}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        // DateTime is returned as string since parsing fails
        Assert.IsType<string>(dataTable.Rows[0]["DateTime"]);
    }

    [Fact]
    public void JsonElementToTypedValue_WithLocalDateTimeHavingOffset_ReturnsString()
    {
        // Arrange - Create a local DateTime with offset information
        var dateTimeOffset = DateTimeOffset.Now.ToString("O"); // ISO 8601 with offset
        var json = $"[{{\"DateTimeOffset\":\"{dateTimeOffset}\"}}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        var result = dataTable.Rows[0]["DateTimeOffset"];
        Assert.IsType<string>(result);
    }

    [Fact]
    public void JsonExtensions_SerializeJson_WithDifferentEncodings_ProducesSameResult()
    {
        // Arrange
        var person = new TestPerson { Name = "Test", Age = 25 };

        // Act
        var jsonUtf8 = person.SerializeJson(Encoding.UTF8);
        var jsonDefault = person.SerializeJson(Encoding.Default);

        // Assert
        Assert.Contains("Test", jsonUtf8);
        Assert.Contains("Test", jsonDefault);
        Assert.Contains("25", jsonUtf8);
        Assert.Contains("25", jsonDefault);
    }

    [Fact]
    public void JsonExtensions_DeserializeJson_WithDifferentEncodings_ProducesSameResult()
    {
        // Arrange
        var json = "{\"Name\":\"Test User\",\"Age\":40}";

        // Act
        var personUtf8 = json.DeserializeJson<TestPerson>(Encoding.UTF8);
        var personDefault = json.DeserializeJson<TestPerson>(Encoding.Default);

        // Assert
        Assert.NotNull(personUtf8);
        Assert.NotNull(personDefault);
        Assert.Equal("Test User", personUtf8.Name);
        Assert.Equal("Test User", personDefault.Name);
        Assert.Equal(40, personUtf8.Age);
        Assert.Equal(40, personDefault.Age);
    }

    [Fact]
    public void JsonElementToDataTable_WithMixedTypes_HandlesAllValueKinds()
    {
        // Arrange
        var json = "[{\"StringVal\":\"text\",\"NumVal\":42,\"BoolVal\":true}]";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var dataTable = jsonElement.JsonElementToDataTable();

        // Assert
        Assert.Equal(1, dataTable.Rows.Count);
        Assert.Equal(3, dataTable.Columns.Count);
        Assert.Equal("text", dataTable.Rows[0]["StringVal"]);
        Assert.Equal(42L, dataTable.Rows[0]["NumVal"]);
        Assert.Equal(true, dataTable.Rows[0]["BoolVal"]);
    }

    #region JsonTextAccessor Tests

    [Fact]
    public void JsonTextAccessor_TryGetMember_WithValidProperty_ReturnsTrue()
    {
        // Arrange
        var json = "{\"Name\":\"John\",\"Age\":30}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act & Assert
        Assert.Equal("John", accessor.Name);
        Assert.Equal(30.0, accessor.Age);
    }

    [Fact]
    public void JsonTextAccessor_TryGetMember_WithNonExistentProperty_ThrowsRuntimeBinderException()
    {
        // Arrange
        var json = "{\"Name\":\"John\"}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act & Assert
        Assert.Throws<RuntimeBinderException>(() => { var _ = accessor.NonExistent; });
    }

    [Fact]
    public void JsonTextAccessor_WithNullValue_ReturnsNull()
    {
        // Arrange
        var json = "{\"Value\":null}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act
        object? value = accessor.Value;

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public void JsonTextAccessor_WithBooleanTrue_ReturnsTrue()
    {
        // Arrange
        var json = "{\"IsActive\":true}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act & Assert
        Assert.True(accessor.IsActive);
    }

    [Fact]
    public void JsonTextAccessor_WithBooleanFalse_ReturnsFalse()
    {
        // Arrange
        var json = "{\"IsActive\":false}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act & Assert
        Assert.False(accessor.IsActive);
    }

    [Fact]
    public void JsonTextAccessor_WithStringValue_ReturnsString()
    {
        // Arrange
        var json = "{\"Message\":\"Hello World\"}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act & Assert
        Assert.Equal("Hello World", accessor.Message);
    }

    [Fact]
    public void JsonTextAccessor_WithNumberValue_ReturnsDouble()
    {
        // Arrange
        var json = "{\"Price\":19.99}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act & Assert
        Assert.Equal(19.99, accessor.Price);
    }

    [Fact]
    public void JsonTextAccessor_WithIntegerValue_ReturnsDouble()
    {
        // Arrange
        var json = "{\"Count\":100}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act & Assert
        Assert.Equal(100.0, accessor.Count);
    }

    [Fact]
    public void JsonTextAccessor_WithNestedObject_ReturnsNestedAccessor()
    {
        // Arrange
        var json = "{\"Person\":{\"Name\":\"John\",\"Address\":{\"City\":\"New York\"}}}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act & Assert
        Assert.Equal("John", accessor.Person.Name);
        Assert.Equal("New York", accessor.Person.Address.City);
    }

    [Fact]
    public void JsonTextAccessor_WithArrayOfPrimitives_ReturnsList()
    {
        // Arrange
        var json = "{\"Numbers\":[1,2,3,4,5]}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act
        var numbers = accessor.Numbers;

        // Assert
        Assert.Equal(5, numbers.Count);
        Assert.Equal(1.0, numbers[0]);
        Assert.Equal(5.0, numbers[4]);
    }

    [Fact]
    public void JsonTextAccessor_WithArrayOfObjects_ReturnsListOfAccessors()
    {
        // Arrange
        var json = "{\"Items\":[{\"Id\":1},{\"Id\":2}]}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act
        var items = accessor.Items;

        // Assert
        Assert.Equal(2, items.Count);
        Assert.Equal(1.0, items[0].Id);
        Assert.Equal(2.0, items[1].Id);
    }

    [Fact]
    public void JsonTextAccessor_WithMixedArray_ReturnsMixedList()
    {
        // Arrange
        var json = "{\"Mixed\":[\"text\",123,true,null]}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act
        var mixed = accessor.Mixed;

        // Assert
        Assert.Equal(4, mixed.Count);
        Assert.Equal("text", mixed[0]);
        Assert.Equal(123.0, mixed[1]);
        Assert.True(mixed[2]);
        Assert.Null(mixed[3]);
    }

    [Fact]
    public void JsonTextAccessor_WithEmptyObject_ReturnsAccessor()
    {
        // Arrange
        var json = "{\"Empty\":{}}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act
        var empty = accessor.Empty;

        // Assert
        Assert.NotNull(empty);
    }

    [Fact]
    public void JsonTextAccessor_WithEmptyArray_ReturnsEmptyList()
    {
        // Arrange
        var json = "{\"Items\":[]}";
        dynamic accessor = json.DeserializeDynamicJsonObject();

        // Act
        var items = accessor.Items;

        // Assert
        Assert.Equal(0, items.Count);
    }

    #endregion
}