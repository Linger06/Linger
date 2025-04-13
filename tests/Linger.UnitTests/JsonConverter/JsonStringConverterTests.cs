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
}