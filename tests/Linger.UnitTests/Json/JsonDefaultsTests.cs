using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Linger.Json;

namespace Linger.UnitTests.Json;

public class JsonDefaultsTests
{
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public DateTime Birthday { get; set; }
        public string? NullableProperty { get; set; }
        public TestModel? Child { get; set; }
    }

    #region CreateResponseOptions Tests

    [Fact]
    public void CreateResponseOptions_ShouldReturnValidOptions()
    {
        // Act
        var options = JsonDefaults.CreateResponseOptions();

        // Assert
        Assert.NotNull(options);
    }

    [Fact]
    public void CreateResponseOptions_ShouldAllowReadingNumberFromString()
    {
        // Arrange
        var options = JsonDefaults.CreateResponseOptions();
        var json = """{"Name":"Test","Age":"25"}""";

        // Act
        var result = JsonSerializer.Deserialize<TestModel>(json, options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(25, result.Age);
    }

    [Fact]
    public void CreateResponseOptions_ShouldHandleCyclicReferences()
    {
        // Arrange
        var options = JsonDefaults.CreateResponseOptions();
        var model = new TestModel { Name = "Parent" };
        model.Child = new TestModel { Name = "Child", Child = model }; // 循环引用

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("Parent", json);
    }

    [Fact]
    public void CreateResponseOptions_ShouldContainDateTimeConverter()
    {
        // Arrange
        var options = JsonDefaults.CreateResponseOptions();

        // Assert
        Assert.True(options.Converters.Count > 0);
    }

    [Fact]
    public void CreateResponseOptions_ShouldDeserializeDateTimeCorrectly()
    {
        // Arrange
        var options = JsonDefaults.CreateResponseOptions();
        var date = new DateTime(2023, 12, 25, 10, 30, 0);
        var json = $"{{\"Name\":\"Test\",\"Age\":25,\"Birthday\":\"{date:yyyy-MM-dd HH:mm:ss}\"}}";

        // Act
        var result = JsonSerializer.Deserialize<TestModel>(json, options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(date, result.Birthday);
    }

    #endregion

    #region CreateRequestOptions Tests

    [Fact]
    public void CreateRequestOptions_ShouldReturnValidOptions()
    {
        // Act
        var options = JsonDefaults.CreateRequestOptions();

        // Assert
        Assert.NotNull(options);
    }

    [Fact]
    public void CreateRequestOptions_ShouldIgnoreNullValues()
    {
        // Arrange
        var options = JsonDefaults.CreateRequestOptions();
        var model = new TestModel
        {
            Name = "Test",
            Age = 25,
            NullableProperty = null
        };

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert
        Assert.DoesNotContain("nullableProperty", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CreateRequestOptions_ShouldSerializeDateTimeCorrectly()
    {
        // Arrange
        var options = JsonDefaults.CreateRequestOptions();
        var model = new TestModel
        {
            Name = "Test",
            Age = 25,
            Birthday = new DateTime(2023, 12, 25, 10, 30, 0)
        };

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert
        Assert.Contains("2023-12-25", json);
    }

    [Fact]
    public void CreateRequestOptions_ShouldContainDateTimeConverters()
    {
        // Arrange
        var options = JsonDefaults.CreateRequestOptions();

        // Assert
        Assert.True(options.Converters.Count >= 2);
    }

    #endregion

    #region ApplyDefaultConfiguration Tests

    [Fact]
    public void ApplyDefaultConfiguration_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => JsonDefaults.ApplyDefaultConfiguration(null!));
    }

    [Fact]
    public void ApplyDefaultConfiguration_ShouldApplyAllSettings()
    {
        // Arrange
        var options = new JsonSerializerOptions();

        // Act
        JsonDefaults.ApplyDefaultConfiguration(options);

        // Assert
        Assert.Equal(ReferenceHandler.IgnoreCycles, options.ReferenceHandler);
        Assert.Equal(JsonNumberHandling.AllowReadingFromString, options.NumberHandling);
        Assert.Equal(JsonIgnoreCondition.WhenWritingNull, options.DefaultIgnoreCondition);
    }

    [Fact]
    public void ApplyDefaultConfiguration_ShouldAddConverters()
    {
        // Arrange
        var options = new JsonSerializerOptions();

        // Act
        JsonDefaults.ApplyDefaultConfiguration(options);

        // Assert
        Assert.True(options.Converters.Count >= 4);
    }

    [Fact]
    public void ApplyDefaultConfiguration_ShouldNotDuplicateConverters()
    {
        // Arrange
        var options = new JsonSerializerOptions();

        // Act - 调用两次
        JsonDefaults.ApplyDefaultConfiguration(options);
        var countAfterFirst = options.Converters.Count;
        JsonDefaults.ApplyDefaultConfiguration(options);
        var countAfterSecond = options.Converters.Count;

        // Assert - 转换器数量应该相同
        Assert.Equal(countAfterFirst, countAfterSecond);
    }

    [Fact]
    public void ApplyDefaultConfiguration_ShouldAllowReadingNumberFromString()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        JsonDefaults.ApplyDefaultConfiguration(options);
        var json = """{"Name":"Test","Age":"30"}""";

        // Act
        var result = JsonSerializer.Deserialize<TestModel>(json, options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void ApplyDefaultConfiguration_ShouldIgnoreNullInOutput()
    {
        // Arrange
        var options = new JsonSerializerOptions();
        JsonDefaults.ApplyDefaultConfiguration(options);
        var model = new TestModel
        {
            Name = "Test",
            Age = 25,
            NullableProperty = null
        };

        // Act
        var json = JsonSerializer.Serialize(model, options);

        // Assert
        Assert.DoesNotContain("nullableProperty", json, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
