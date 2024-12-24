namespace Linger.UnitTests.Extensions.Collection;

public class DictionaryExtensionTests
{
    [Fact]
    public void GetOrAdd_ReturnsExistingValue()
    {
        var dictionary = new Dictionary<int, string> { { 1, "one" } };
        var result = dictionary.GetOrAdd(1, key => "new");
        Assert.Equal("one", result);
    }

    [Fact]
    public void GetOrAdd_AddsAndReturnsNewValue()
    {
        var dictionary = new Dictionary<int, string>();
        var result = dictionary.GetOrAdd(1, key => "one");
        Assert.Equal("one", result);
        Assert.Equal("one", dictionary[1]);
    }

    [Fact]
    public void GetOrAdd_Overload_ReturnsExistingValue()
    {
        var dictionary = new Dictionary<int, string> { { 1, "one" } };
        var result = dictionary.GetOrAdd(1, () => "new");
        Assert.Equal("one", result);
    }

    [Fact]
    public void GetOrAdd_Overload_AddsAndReturnsNewValue()
    {
        var dictionary = new Dictionary<int, string>();
        var result = dictionary.GetOrAdd(1, () => "one");
        Assert.Equal("one", result);
        Assert.Equal("one", dictionary[1]);
    }

    [Fact]
    public void AddOrUpdate_UpdatesExistingValue()
    {
        var dictionary = new Dictionary<int, string> { { 1, "one" } };
        var result = dictionary.AddOrUpdate(1, key => "new", (key, oldValue) => oldValue + " updated");
        Assert.Equal("one updated", result);
        Assert.Equal("one updated", dictionary[1]);
    }

    [Fact]
    public void AddOrUpdate_AddsAndReturnsNewValue()
    {
        var dictionary = new Dictionary<int, string>();
        var result = dictionary.AddOrUpdate(1, key => "one", (key, oldValue) => oldValue + " updated");
        Assert.Equal("one", result);
        Assert.Equal("one", dictionary[1]);
    }

    [Fact]
    public void AddOrUpdate_Overload_UpdatesExistingValue()
    {
        var dictionary = new Dictionary<int, string> { { 1, "one" } };
        var result = dictionary.AddOrUpdate(1, () => "new", oldValue => oldValue + " updated");
        Assert.Equal("one updated", result);
        Assert.Equal("one updated", dictionary[1]);
    }

    [Fact]
    public void AddOrUpdate_Overload_AddsAndReturnsNewValue()
    {
        var dictionary = new Dictionary<int, string>();
        var result = dictionary.AddOrUpdate(1, () => "one", oldValue => oldValue + " updated");
        Assert.Equal("one", result);
        Assert.Equal("one", dictionary[1]);
    }
}