using System.IO;
using System.Text;
using System.Text.Json;
using Linger.JsonConverter;
using Xunit.v3;

namespace Linger.UnitTests.JsonConverter;

public class JsonStringConverterTests
{
    private readonly JsonSerializerOptions _options;
    
    public JsonStringConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new JsonStringConverter() }
        };
    }
    
    [Fact]
    public void Write_NormalString_WritesStringValue()
    {
        // Arrange
        var value = "Hello, World!";
        
        // Act
        var json = JsonSerializer.Serialize(value, _options);
        
        // Assert
        Assert.Equal("\"Hello, World!\"", json);
    }
    
    [Fact]
    public void Write_NullString_WritesNullValue()
    {
        // Arrange
        string? value = null;
        
        // Act
        var json = JsonSerializer.Serialize(value, _options);
        
        // Assert
        Assert.Equal("null", json);
    }
    
    [Fact]
    public void Read_StringToken_ReturnsString()
    {
        // Arrange
        var json = "\"Hello, World!\"";
        
        // Act
        var result = JsonSerializer.Deserialize<string>(json, _options);
        
        // Assert
        Assert.Equal("Hello, World!", result);
    }
    
    [Fact]
    public void Read_NullToken_ReturnsNull()
    {
        // Arrange
        var json = "null";
        
        // Act
        var result = JsonSerializer.Deserialize<string>(json, _options);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void Read_NumberToken_ReturnsStringRepresentation()
    {
        // Arrange
        var json = "123.45";
        
        // Act
        var result = JsonSerializer.Deserialize<string>(json, _options);
        
        // Assert
        Assert.Equal("123.45", result);
    }
    
    [Fact]
    public void Read_BooleanToken_ReturnsStringRepresentation()
    {
        // Arrange
        var json = "true";
        
        // Act
        var result = JsonSerializer.Deserialize<string>(json, _options);
        
        // Assert
        Assert.Equal("True", result);
    }

    [Fact]
    public void Read_BooleanFalseToken_ReturnsStringRepresentation()
    {
        // Arrange
        var json = "false";
        
        // Act
        var result = JsonSerializer.Deserialize<string>(json, _options);
        
        // Assert
        Assert.Equal("False", result);
    }

    [Fact]
    public void Read_StartObjectToken_ReturnsNotSupportedMessage()
    {
        // Arrange
        var json = "{}";
        
        // Act
        var result = JsonSerializer.Deserialize<string>(json, _options);
        
        // Assert
        Assert.Equal("(not supported)", result);
    }

    [Fact]
    public void Read_ComplexObjectToken_ReturnsNotSupportedMessage()
    {
        // Arrange
        var json = "{\"name\":\"test\",\"value\":123}";
        
        // Act
        var result = JsonSerializer.Deserialize<string>(json, _options);
        
        // Assert
        Assert.Equal("(not supported)", result);
    }

    [Fact]
    public void Read_UnsupportedTokenType_ThrowsJsonException()
    {
        // Arrange
        var json = "[1,2,3]"; // Array will trigger unsupported token type
        
        // Act & Assert
        var exception = Assert.Throws<JsonException>(() => 
            JsonSerializer.Deserialize<string>(json, _options));
        Assert.Contains("Unsupported token type", exception.Message);
    }

    [Fact]
    public void Read_DirectNullTokenUsingUtf8JsonReader_ReturnsNull()
    {
        // Arrange
        var converter = new JsonStringConverter();
        var json = "null"u8;
        var reader = new Utf8JsonReader(json);
        reader.Read(); // Move to the token
        
        // Act
        var result = converter.Read(ref reader, typeof(string), _options);
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Write_DirectNullValueUsingUtf8JsonWriter_WritesNullValue()
    {
        // Arrange
        var converter = new JsonStringConverter();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        
        // Act
        converter.Write(writer, null, _options);
        writer.Flush();
        
        // Assert
        var json = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("null", json);
    }

    [Fact]
    public void Write_DirectStringValueUsingUtf8JsonWriter_WritesStringValue()
    {
        // Arrange
        var converter = new JsonStringConverter();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        var value = "test value";
        
        // Act
        converter.Write(writer, value, _options);
        writer.Flush();
        
        // Assert
        var json = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("\"test value\"", json);
    }

    [Fact]
    public void Read_UnsupportedToken_LogsAndThrowsException()
    {
        // Arrange
        var converter = new JsonStringConverter();
        // Create a reader with StartArray token which should trigger default case
        var json = "["u8;
        var reader = new Utf8JsonReader(json);
        reader.Read(); // Move to StartArray token
        
        // Capture console output
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        
        try
        {
            // Act & Assert
            JsonException? exception = null;
            try
            {
                converter.Read(ref reader, typeof(string), _options);
            }
            catch (JsonException ex)
            {
                exception = ex;
            }
            
            Assert.NotNull(exception);
            Assert.Contains("Unsupported token type", exception.Message);
            Assert.Contains("StartArray", exception.Message);
            
            // Check console output
            var consoleOutput = stringWriter.ToString();
            Assert.Contains("Unsupported token type: StartArray", consoleOutput);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}