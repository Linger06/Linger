using System.Collections.Specialized;

namespace Linger.UnitTests.Extensions.Collection;

public class NameValueCollectionExtensionsTests
{
    [Fact]
    public void ForEach_PerformsActionOnEachElement()
    {
        var collection = new NameValueCollection
    {
        { "key1", "value1" },
        { "key2", "value2" }
    };
        var result = new List<string>();

        collection.ForEach((key, value) => result.Add($"{key}:{value}"));

        Assert.Equal(new List<string> { "key1:value1", "key2:value2" }, result);
    }

    [Fact]
    public void ForEach_WithIndex_PerformsActionOnEachElement()
    {
        var collection = new NameValueCollection
    {
        { "key1", "value1" },
        { "key2", "value2" }
    };
        var result = new List<string>();

        collection.ForEach((key, value, index) => result.Add($"{index}:{key}:{value}"));

        Assert.Equal(new List<string> { "0:key1:value1", "1:key2:value2" }, result);
    }

    [Fact]
    public void ForEach_DoesNotPerformActionOnEmptyCollection()
    {
        var collection = new NameValueCollection();
        var result = new List<string>();

        collection.ForEach((key, value) => result.Add($"{key}:{value}"));

        Assert.Empty(result);
    }

    [Fact]
    public void ForEach_WithIndex_DoesNotPerformActionOnEmptyCollection()
    {
        var collection = new NameValueCollection();
        var result = new List<string>();

        collection.ForEach((key, value, index) => result.Add($"{index}:{key}:{value}"));

        Assert.Empty(result);
    }

    [Fact]
    public void ForEach_IgnoresNullKeysAndValues()
    {
        var collection = new NameValueCollection
    {
        { "key1", "value1" },
        { null, "value2" },
        { "key3", null }
    };
        var result = new List<string>();

        collection.ForEach((key, value) => result.Add($"{key}:{value}"));

        Assert.Equal(new List<string> { "key1:value1" }, result);
    }

    [Fact]
    public void ForEach_WithIndex_IgnoresNullKeysAndValues()
    {
        var collection = new NameValueCollection
    {
        { "key1", "value1" },
        { null, "value2" },
        { "key3", null }
    };
        var result = new List<string>();

        collection.ForEach((key, value, index) => result.Add($"{index}:{key}:{value}"));

        Assert.Equal(new List<string> { "0:key1:value1" }, result);
    }
}