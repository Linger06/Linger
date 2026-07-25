namespace Linger.Reflection.UnitTests.Extensions.Core;

public class ObjectReflectionExtensionsTests
{
    [Fact]
    public void ForEachProperty_ShouldEnumerateReadableNonIndexedProperties()
    {
        var value = new PropertyEnumerationTestModel { Name = "Linger", Count = 6 };
        var properties = new Dictionary<string, object?>();

        value.ForEachProperty((name, propertyValue) => properties.Add(name, propertyValue));

        Assert.Equal("Linger", properties[nameof(PropertyEnumerationTestModel.Name)]);
        Assert.Equal(6, properties[nameof(PropertyEnumerationTestModel.Count)]);
        Assert.DoesNotContain("Item", properties.Keys);
    }

    [Fact]
    public void ForEachProperty_ShouldNotInvokeAction_WhenValueIsNull()
    {
        object? value = null;
        var invoked = false;

        value.ForEachProperty((_, _) => invoked = true);

        Assert.False(invoked);
    }

    [Fact]
    public void GetPropertyInfo_ShouldReturnPropertyInfo_WhenPropertyExists()
    {
        var obj = new { Name = "John" };

        var propertyInfo = obj.GetPropertyInfo(nameof(obj.Name));

        Assert.NotNull(propertyInfo);
        Assert.Equal(nameof(obj.Name), propertyInfo.Name);
    }

    [Fact]
    public void GetPropertyInfo_ShouldReturnNull_WhenPropertyDoesNotExist()
    {
        var obj = new { Name = "John" };

        Assert.Null(obj.GetPropertyInfo("Age"));
    }

    [Fact]
    public void GetPropertyValue_ShouldReturnValue_WhenPropertyExists()
    {
        var obj = new { Name = "John" };

        var value = obj.GetPropertyValue(nameof(obj.Name));

        Assert.Equal("John", value);
    }

    [Fact]
    public void GetPropertyValue_ShouldReturnNull_WhenPropertyDoesNotExist()
    {
        var obj = new { Name = "John" };

        Assert.Null(obj.GetPropertyValue("Age"));
    }

    private sealed class PropertyEnumerationTestModel
    {
        public string Name { get; init; } = string.Empty;

        public int Count { get; init; }

        public string this[int index] => index.ToString();
    }
}
