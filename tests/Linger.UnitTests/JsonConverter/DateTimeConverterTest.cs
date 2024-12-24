namespace Linger.UnitTests.JsonConverter;

/// <summary>
/// JSON 日期转换器测试
/// </summary>
public partial class DateTimeConverterTests
{
    private readonly JsonSerializerOptions _options = new();

    public DateTimeConverterTests()
    {
        _options.Converters.Add(new DateTimeConverter());
    }

    [Fact]
    public void SerializeTest()
    {
        var p = new Test { Time = new DateTime(2020, 9, 22, 10, 51, 00) };

        var str = JsonSerializer.Serialize(p, _options);

        Assert.Equal("{\"Time\":\"2020-09-22 10:51:00\"}", str);
    }

    [Fact]
    public void DeserializeTest()
    {
        var str = "{\"Time\":\"2020-09-22 10:51:00\"}";

        Test? t = JsonSerializer.Deserialize<Test>(str, _options);
        Assert.NotNull(t);
        Assert.Equal(new DateTime(2020, 9, 22, 10, 51, 00), t.Time);
    }

    [Fact]
    public void JsonDateTimeConvertorTest()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new DateTimeConverter());
        //options.Converters.Add(new ObjectConverter());

        {
            var value = new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc);

            var json = JsonSerializer.Serialize(value, options);
            Assert.Equal("""
                         "2019-01-30 12:01:02"
                         """, json);
        }

        {
            const string Value = """
                                 "2019-01-30 12:01:02"
                                 """;

            object obj = JsonSerializer.Deserialize<DateTime>(Value, options);
            _ = Assert.IsType<DateTime>(obj);
            Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc), obj);
        }
    }
}

public class Test
{
    public DateTime Time { get; set; }
}