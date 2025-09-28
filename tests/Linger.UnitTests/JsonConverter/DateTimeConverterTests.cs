using System.Text.Json;
using Linger.JsonConverter;
using Xunit.v3;

namespace Linger.UnitTests.JsonConverter;

public class DateTimeConverterTests
{
    private readonly JsonSerializerOptions _options;
    private readonly JsonSerializerOptions _nullableOptions;
    
    public DateTimeConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new DateTimeConverter() }
        };
        
        _nullableOptions = new JsonSerializerOptions
        {
            Converters = { new DateTimeNullConverter() }
        };
    }
    
    [Fact]
    public void DateTimeConverter_Write_WithDateOnly_WritesDateFormat()
    {
        // Arrange
        var date = new DateTime(2025, 4, 11, 0, 0, 0);
        
        // Act
        var json = JsonSerializer.Serialize(date, _options);
        
        // Assert
        Assert.Equal("\"2025-04-11\"", json);
    }
    
    [Fact]
    public void DateTimeConverter_Write_WithTime_WritesDateTimeFormat()
    {
        // Arrange
        var dateTime = new DateTime(2025, 4, 11, 14, 30, 45);
        
        // Act
        var json = JsonSerializer.Serialize(dateTime, _options);
        
        // Assert
        Assert.Equal("\"2025-04-11 14:30:45\"", json);
    }
    
    [Fact]
    public void DateTimeConverter_Read_WithDateFormat_ReturnsDateTime()
    {
        // Arrange
        var json = "\"2025-04-11\"";
        
        // Act
        var result = JsonSerializer.Deserialize<DateTime>(json, _options);
        
        // Assert
        Assert.Equal(new DateTime(2025, 4, 11), result);
    }
    
    [Fact]
    public void DateTimeConverter_Read_WithDateTimeFormat_ReturnsDateTime()
    {
        // Arrange
        var json = "\"2025-04-11 14:30:45\"";
        
        // Act
        var result = JsonSerializer.Deserialize<DateTime>(json, _options);
        
        // Assert
        Assert.Equal(new DateTime(2025, 4, 11, 14, 30, 45), result);
    }
    
    [Fact]
    public void DateTimeNullConverter_Write_WithNull_WritesNull()
    {
        // Arrange
        DateTime? value = null;
        
        // Act
        var json = JsonSerializer.Serialize<DateTime?>(value, _nullableOptions);
        
        // Assert
        Assert.Equal("null", json);
    }

    [Fact]
    public void DateTimeNullConverter_ReadWrite_RoundTrip_Null()
    {
        // Arrange
        DateTime? value = null;

        // Act
        var json = JsonSerializer.Serialize(value, _nullableOptions);
        var back = JsonSerializer.Deserialize<DateTime?>(json, _nullableOptions);

        // Assert
        Assert.Null(back);
    }
    
    [Fact]
    public void DateTimeNullConverter_Write_WithDateOnly_WritesDateFormat()
    {
        // Arrange
        DateTime? date = new DateTime(2025, 4, 11, 0, 0, 0);
        
        // Act
        var json = JsonSerializer.Serialize<DateTime?>(date, _nullableOptions);
        
        // Assert
        Assert.Equal("\"2025-04-11\"", json);
    }
    
    [Fact]
    public void DateTimeNullConverter_Write_WithTime_WritesDateTimeFormat()
    {
        // Arrange
        DateTime? dateTime = new DateTime(2025, 4, 11, 14, 30, 45);
        
        // Act
        var json = JsonSerializer.Serialize<DateTime?>(dateTime, _nullableOptions);
        
        // Assert
        Assert.Equal("\"2025-04-11 14:30:45\"", json);
    }
    
    [Fact]
    public void DateTimeNullConverter_Read_WithNull_ReturnsNull()
    {
        // Arrange
        var json = "null";
        
        // Act
        var result = JsonSerializer.Deserialize<DateTime?>(json, _nullableOptions);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void DateTimeNullConverter_Read_WithEmptyString_ReturnsNull()
    {
        // Arrange
        var json = "\"\"";
        
        // Act
        var result = JsonSerializer.Deserialize<DateTime?>(json, _nullableOptions);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void DateTimeNullConverter_Read_WithDateFormat_ReturnsDateTime()
    {
        // Arrange
        var json = "\"2025-04-11\"";
        
        // Act
        var result = JsonSerializer.Deserialize<DateTime?>(json, _nullableOptions);
        
        // Assert
        Assert.Equal(new DateTime(2025, 4, 11), result);
    }
}