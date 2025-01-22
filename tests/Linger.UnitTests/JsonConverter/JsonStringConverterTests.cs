namespace Linger.UnitTests.JsonConverter;

public class JsonStringConverterTests
{
    private readonly JsonStringConverter _converter = new JsonStringConverter();
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions();

    [Fact]
    public void Read_ShouldReturnNull_WhenTokenTypeIsNull()
    {
        var json = "null";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();

        var result = _converter.Read(ref reader, typeof(string), _options);

        Assert.Null(result);
    }

    [Fact]
    public void Read_ShouldReturnString_WhenTokenTypeIsString()
    {
        var json = "\"test\"";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();

        var result = _converter.Read(ref reader, typeof(string), _options);

        Assert.Equal("test", result);
    }

    [Fact]
    public void Read_ShouldReturnNumberAsString_WhenTokenTypeIsNumber()
    {
        var json = "123.45";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();

        var result = _converter.Read(ref reader, typeof(string), _options);

        Assert.Equal("123.45", result);
    }

    [Fact]
    public void Read_ShouldReturnBooleanAsString_WhenTokenTypeIsBoolean()
    {
        var json = "true";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();

        var result = _converter.Read(ref reader, typeof(string), _options);

        Assert.Equal("True", result);
    }

    [Fact]
    public void Read_ShouldReturnNotSupported_WhenTokenTypeIsStartObject()
    {
        var json = "{}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();

        var result = _converter.Read(ref reader, typeof(string), _options);

        Assert.Equal("(not supported)", result);
    }

    [Fact]
    public void Read_ShouldThrowJsonException_WhenTokenTypeIsUnsupported()
    {
        var json = "[]";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        try
        {
            _converter.Read(ref reader, typeof(string), _options);
        }
        catch (JsonException ex)
        {
            Assert.Equal("Unsupported token type: StartArray", ex.Message);
        }
    }

    [Fact]
    public void Write_ShouldWriteNullValue_WhenValueIsNull()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        _converter.Write(writer, null, _options);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("null", json);
    }

    [Fact]
    public void Write_ShouldWriteStringValue_WhenValueIsNotNull()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        _converter.Write(writer, "test", _options);
        writer.Flush();

        var json = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("\"test\"", json);
    }
}