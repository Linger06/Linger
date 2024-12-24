using Linger.UnitTests.Extensions.Collection;

namespace Linger.UnitTests.Extensions;

public class JsonExtensionsTest
{
    private readonly IEnumerable<TestClass> _class;
    private readonly ITestOutputHelper _outputHelper;

    public JsonExtensionsTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;

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
        _outputHelper = outputHelper;
    }

    [Fact]
    public void SerializeJson()
    {
        // Type
        var @this = new List<string> { "Fizz", "Buzz" };

        // Examples
        var result = @this.SerializeJson(); // Serialize the object into a string.
        Assert.NotNull(result);
        // Unit Test
        List<string>? result2 = result.DeserializeJson<List<string>>();
        Assert.NotNull(result2);
        Assert.Equal(2, result2.Count);
        Assert.Equal("Fizz", result2[0]);
        Assert.Equal("Buzz", result2[1]);

        var result3 = _class.SerializeJson();
        _outputHelper.WriteLine(result3);
        List<TestClass>? result4 = result3.DeserializeJson<List<TestClass>>();
        Assert.NotNull(result4);
        Assert.Equal(2, result4.Count);
        Assert.Equal(1, result4[0].Int);
        Assert.Null(result4[0].NullableInt);

        var result5 = result4[0].DateTime.IsDateEqual(new DateTime(2020, 1, 1));
        Assert.True(result5);
    }

    [Fact]
    public void DeserializeJson()
    {
        // Type
        var @this = "[\"Fizz\",\"Buzz\"]";

        // Examples
        List<string>? result = @this.DeserializeJson<List<string>>();

        // Unit Test
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Fizz", result[0]);
        Assert.Equal("Buzz", result[1]);

        var product = new Product { Name = "Apple", Expiry = new DateTime(2008, 12, 28), Sizes = new[] { "Small" } };

        // Serialize
        var resultProduct = product.SerializeJson();

        // Deserialize
        Product? product2 = resultProduct.DeserializeJson<Product>();

        // Unit Test
        Assert.NotNull(product2);
        Assert.Equal("Apple", product2.Name);
        Assert.Equal(new DateTime(2008, 12, 28).Date, product2.Expiry.Date);
        Assert.Equal("Small", product2.Sizes[0]);
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
        var result2 = _class.Serialize();
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

[Serializable]
public class Product
{
    public DateTime Expiry;
    public string? Name;
    public string[] Sizes = null!;
}