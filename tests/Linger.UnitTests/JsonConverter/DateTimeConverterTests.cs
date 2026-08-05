using System.Globalization;
using System.Text.Json;
using Linger.Json.JsonConverter;
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
    public void DateTimeConverter_ReadWrite_PreservesUtcKindAndFractionalSeconds()
    {
        var value = new DateTime(2025, 4, 11, 0, 0, 0, 500, DateTimeKind.Utc);

        var json = JsonSerializer.Serialize(value, _options);
        var result = JsonSerializer.Deserialize<DateTime>(json, _options);

        Assert.Equal(value, result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
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
    public void DateTimeConverter_Read_WithZhCnCulture_ReturnsDateTime()
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("zh-CN");
            var json = "\"2024/1/15 下午 3:04:05\"";

            var result = JsonSerializer.Deserialize<DateTime>(json, _options);

            Assert.Equal(new DateTime(2024, 1, 15, 15, 4, 5), result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void DateTimeConverter_Read_WithUtcRoundtripFormat_PreservesUtcKind()
    {
        var json = "\"2019-01-30T12:01:02Z\"";

        var result = JsonSerializer.Deserialize<DateTime>(json, _options);

        Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc), result);
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
    public void DateTimeNullConverter_ReadWrite_PreservesFractionalSeconds()
    {
        DateTime? value = new DateTime(2025, 4, 11, 14, 30, 45, 123, DateTimeKind.Unspecified);

        var json = JsonSerializer.Serialize(value, _nullableOptions);
        var result = JsonSerializer.Deserialize<DateTime?>(json, _nullableOptions);

        Assert.Equal(value, result);
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

    [Fact]
    public void DateTimeNullConverter_Read_WithUtcRoundtripFormat_PreservesUtcKind()
    {
        var json = "\"2019-01-30T12:01:02Z\"";

        var result = JsonSerializer.Deserialize<DateTime?>(json, _nullableOptions);

        Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc), result);
    }
}
