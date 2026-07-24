using Linger.UnitTests.Extensions.Collection;

namespace Linger.UnitTests.Extensions;

public class JsonExtensionsTest
{
    private readonly IEnumerable<TestClass> _class;

    public JsonExtensionsTest()
    {
        _class = new[]
        {
            new TestClass
            {
                Int = 1,
                NullableInt = null,
                String = Guid.NewGuid().ToString(),
                Guid = Guid.NewGuid(),
                NullableGuid = null,
                DateTime = new DateTime(2020, 1, 1),
                NullableDateTime = null,
                Binary = Guid.NewGuid().ToByteArray(),
                Boolean = true,
                Int16 = 2,
                Int64 = 3,
                Decimal = new decimal(1.1),
                Single = 4,
                Double = 5
            },
            new TestClass
            {
                Int = 11,
                NullableInt = null,
                String = Guid.NewGuid().ToString(),
                Guid = Guid.NewGuid(),
                NullableGuid = null,
                DateTime = new DateTime(2021, 2, 2),
                NullableDateTime = null,
                Binary = Guid.NewGuid().ToByteArray(),
                Boolean = true,
                Int16 = 21,
                Int64 = 31,
                Decimal = new decimal(11.11),
                Single = 41,
                Double = 51
            }
        };
    }

    [Fact]
    public void Deserialize()
    {
        // Type
        var @this = "[\"Fizz\",\"Buzz\"]";

        // Examples
        List<string>? result = @this.Deserialize<List<string>>();

        // Unit Test
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Fizz", result[0]);
        Assert.Equal("Buzz", result[1]);

        // Serialize
        var result2 = _class.ToJsonString();
        Assert.NotNull(result2);
        // Deserialize
        List<TestClass>? result4 = result2.Deserialize<List<TestClass>>();

        // Unit Test
        Assert.NotNull(result4);
        Assert.Equal(2, result4.Count);
        Assert.Equal(1, result4[0].Int);
        Assert.Null(result4[0].NullableInt);

        var result5 = result4[0].DateTime.IsDateEqual(new DateTime(2020, 1, 1));
        Assert.True(result5);
    }
}
