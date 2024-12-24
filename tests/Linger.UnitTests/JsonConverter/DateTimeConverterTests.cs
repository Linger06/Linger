namespace Linger.UnitTests.JsonConverter;

public partial class DateTimeConverterTests
{
    [Fact]
    public void Read_ValidDateString_ReturnsDateTime()
    {
        var json = "\"2023-10-01\"";
        DateTime result = JsonSerializer.Deserialize<DateTime>(json, _options);
        Assert.Equal(new DateTime(2023, 10, 1), result);
    }

    [Fact]
    public void Read_InvalidDateString_ThrowsException()
    {
        var json = "\"invalid-date\"";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DateTime>(json, _options));
    }

    [Fact]
    public void Write_DateTimeWithoutTime_WritesCorrectString()
    {
        var date = new DateTime(2023, 10, 1);
        var json = JsonSerializer.Serialize(date, _options);
        Assert.Equal("\"2023-10-01\"", json);
    }

    [Fact]
    public void Write_DateTimeWithTime_WritesCorrectString()
    {
        var date = new DateTime(2023, 10, 1, 14, 30, 0);
        var json = JsonSerializer.Serialize(date, _options);
        Assert.Equal("\"2023-10-01 14:30:00\"", json);
    }
}

public class DateTimeNullConverterTests
{
    private readonly JsonSerializerOptions _options;

    public DateTimeNullConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new DateTimeNullConverter() }
        };
    }

    [Fact]
    public void Read_ValidDateString_ReturnsNullableDateTime()
    {
        var json = "\"2023-10-01\"";
        DateTime? result = JsonSerializer.Deserialize<DateTime?>(json, _options);
        Assert.Equal(new DateTime(2023, 10, 1), result);
    }

    [Fact]
    public void Read_NullString_ReturnsNull()
    {
        var json = "null";
        DateTime? result = JsonSerializer.Deserialize<DateTime?>(json, _options);
        Assert.Null(result);
    }

    [Fact]
    public void Write_NullableDateTimeWithoutTime_WritesCorrectString()
    {
        DateTime? date = new DateTime(2023, 10, 1);
        var json = JsonSerializer.Serialize(date, _options);
        Assert.Equal("\"2023-10-01\"", json);
    }

    [Fact]
    public void Write_NullableDateTimeWithTime_WritesCorrectString()
    {
        DateTime? date = new DateTime(2023, 10, 1, 14, 30, 0);
        var json = JsonSerializer.Serialize(date, _options);
        Assert.Equal("\"2023-10-01 14:30:00\"", json);
    }

    [Fact]
    public void Write_NullableDateTimeNull_WritesNullString()
    {
        DateTime? date = null;
        var json = JsonSerializer.Serialize(date, _options);
        Assert.Equal("null", json);
    }
}