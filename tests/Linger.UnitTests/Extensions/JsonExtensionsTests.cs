namespace Linger.UnitTests.Extensions;

public class JsonExtensionsTests
{
    private readonly Person _person = new Person { Name = "John Doe", Age = 30 };

    [Fact]
    public void ToJsonString_WithOptions_ShouldSerialize()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = _person.ToJsonString(options);
        Assert.Contains("\"Name\": \"John Doe\"", json);
        Assert.Contains("\"Age\": 30", json);
    }

    [Fact]
    public void ToJsonString_WithoutOptions_ShouldSerialize()
    {
        var json = _person.ToJsonString();
        Assert.Contains("\"Name\": \"John Doe\"", json);
        Assert.Contains("\"Age\": 30", json);
    }

    [Fact]
    public void SerializeJson_ShouldSerialize()
    {
        var json = _person.SerializeJson();
        Assert.Contains("\"Name\":\"John Doe\"", json);
        Assert.Contains("\"Age\":30", json);
    }

    [Fact]
    public void SerializeJson_WithEncoding_ShouldSerialize()
    {
        var json = _person.SerializeJson(Encoding.UTF8);
        Assert.Contains("\"Name\":\"John Doe\"", json);
        Assert.Contains("\"Age\":30", json);
    }

    [Fact]
    public void SerializeJson_ReturnStringEmpty_WhenValueIsNull()
    {
        Person? nullPerson = null;
        var json = nullPerson.SerializeJson(Encoding.UTF8);
        Assert.Empty(json);
    }

    [Fact]
    public void Serialize_WithOptions_ShouldSerialize()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = _person.Serialize(options);
        Assert.Contains("\"Name\": \"John Doe\"", json);
        Assert.Contains("\"Age\": 30", json);
    }

    [Fact]
    public void Serialize_WithoutOptions_ShouldSerialize()
    {
        var json = _person.Serialize();
        Assert.Contains("\"Name\":\"John Doe\"", json);
        Assert.Contains("\"Age\":30", json);
    }

    [Fact]
    public void DeserializeJson_ShouldDeserialize()
    {
        var json = "{\"Name\":\"John Doe\",\"Age\":30}";
        Person? person = json.DeserializeJson<Person>();
        Assert.NotNull(person);
        Assert.Equal("John Doe", person.Name);
        Assert.Equal(30, person.Age);
    }

    [Fact]
    public void DeserializeJson_WithEncoding_ShouldDeserialize()
    {
        var json = "{\"Name\":\"John Doe\",\"Age\":30}";
        Person? person = json.DeserializeJson<Person>(Encoding.UTF8);
        Assert.NotNull(person);
        Assert.Equal("John Doe", person.Name);
        Assert.Equal(30, person.Age);
    }

    [Fact]
    public void Deserialize_WithOptions_ShouldDeserialize()
    {
        var json = "{\"Name\":\"John Doe\",\"Age\":30}";
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Person? person = json.Deserialize<Person>(options);
        Assert.NotNull(person);
        Assert.Equal("John Doe", person.Name);
        Assert.Equal(30, person.Age);
    }

    [Fact]
    public void Deserialize_WithoutOptions_ShouldDeserialize()
    {
        var json = "{\"Name\":\"John Doe\",\"Age\":30}";
        Person? person = json.Deserialize<Person>();
        Assert.NotNull(person);
        Assert.Equal("John Doe", person.Name);
        Assert.Equal(30, person.Age);
    }

    [Fact]
    public void DeserializeDynamicJsonObject_ShouldDeserialize()
    {
        var json = "{\"Name\":\"John Doe\",\"Age\":30}";
        var obj = json.DeserializeDynamicJsonObject();
        Assert.Equal("John Doe", (string)obj.Name);
        Assert.Equal(30, (int)obj.Age);
    }

    [Fact]
    public void JsonElementToDataTable_ShouldConvert()
    {
        var json = "[{\"Name\":\"John Doe\",\"Age\":30}]";
        JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        DataTable? dataTable = jsonElement.JsonElementToDataTable();
        Assert.Single(dataTable.Rows);
        Assert.Equal("John Doe", dataTable.Rows[0]["Name"]);
        Assert.Equal(30, dataTable.Rows[0]["Age"].ToInt());
    }
}

public class Person
{
    public string? Name { get; set; }
    public int Age { get; set; }
}