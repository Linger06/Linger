namespace Linger.UnitTests.JsonConverter;

public class JsonObjectConverterTests
{
    private readonly JsonSerializerOptions _options;

    public JsonObjectConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new JsonObjectConverter() }
        };
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("123", 123L)]
    [InlineData("123.45", 123.45)]
    [InlineData("\"2023-10-01T00:00:00\"", "2023-10-01T00:00:00")]
    [InlineData("\"Hello, World!\"", "Hello, World!")]
    public void Read_ValidJson_ReturnsExpectedObject(string json, object expected)
    {
        var result = JsonSerializer.Deserialize<object>(json, _options);

        if (result is DateTime dateTimeResult)
        {
            result = dateTimeResult.ToString("yyyy-MM-ddTHH:mm:ss");
        }
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Read_JsonArray_ReturnsJsonElement()
    {
        var json = "[1, 2, 3]";
        var result = JsonSerializer.Deserialize<object>(json, _options);
        Assert.IsType<JsonElement>(result);
    }

    [Fact]
    public void Read_JsonObject_ReturnsJsonElement()
    {
        var json = "{\"key\":\"value\"}";
        var result = JsonSerializer.Deserialize<object>(json, _options);
        Assert.IsType<JsonElement>(result);
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData(123, "123")]
    [InlineData(123.45, "123.45")]
    [InlineData("Hello, World!", "\"Hello, World!\"")]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Write_ValidObject_WritesExpectedJson(object? value, string expectedJson)
    {
        var json = JsonSerializer.Serialize(value, _options);
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void Write_NullObject_WritesNull()
    {
        object? value = null;
        var json = JsonSerializer.Serialize(value, _options);
        Assert.Equal("null", json);
    }

    [Fact]
    public void Write_ObjectType_WritesEmptyObject()
    {
        var value = new object();
        var json = JsonSerializer.Serialize(value, _options);
        Assert.Equal("{}", json);
    }
}