namespace Linger.UnitTests.Extensions.Collection;

public class DictionaryExtensionTests
{
    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void GetOrAdd_Overload_InvokesFactoryOnlyWhenMissing(bool exists, int expectedCalls)
    {
        var dictionary = new Dictionary<int, string>();
        if (exists)
        {
            dictionary.Add(1, "existing");
        }
        var calls = 0;

        dictionary.GetOrAdd(1, () => { calls++; return "created"; });

        Assert.Equal(expectedCalls, calls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddOrUpdate_Overload_InvokesOnlySelectedFactory(bool exists)
    {
        var dictionary = new Dictionary<int, string>();
        if (exists)
        {
            dictionary.Add(1, "existing");
        }
        var addCalls = 0;
        var updateCalls = 0;

        dictionary.AddOrUpdate(1,
            () => { addCalls++; return "created"; },
            value => { updateCalls++; return value; });

        Assert.Equal(exists ? (0, 1) : (1, 0), (addCalls, updateCalls));
    }

    [Fact]
    public void GetOrAdd_Overload_WithNullFactory_ThrowsEvenWhenKeyExists()
    {
        IDictionary<int, string> dictionary = new Dictionary<int, string> { [1] = "one" };

        Assert.Throws<ArgumentNullException>(() => dictionary.GetOrAdd(1, (Func<string>)null!));
    }

    [Fact]
    public void AddOrUpdate_Overload_WithNullUpdateFactory_ThrowsEvenWhenKeyMissing()
    {
        IDictionary<int, string> dictionary = new Dictionary<int, string>();

        Assert.Throws<ArgumentNullException>(() => dictionary.AddOrUpdate(1, () => "one", (Func<string, string>)null!));
    }
    [Fact]
    public void GetOrAdd_ReturnsExistingValue()
    {
        var dictionary = new Dictionary<int, string> { { 1, "one" } };
        var result = dictionary.GetOrAdd(1, _ => "new");
        Assert.Equal("one", result);
    }

    [Fact]
    public void GetOrAdd_AddsAndReturnsNewValue()
    {
        var dictionary = new Dictionary<int, string>();
        var result = dictionary.GetOrAdd(1, _ => "one");
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
        var result = dictionary.AddOrUpdate(1, _ => "new", (_, oldValue) => oldValue + " updated");
        Assert.Equal("one updated", result);
        Assert.Equal("one updated", dictionary[1]);
    }

    [Fact]
    public void AddOrUpdate_AddsAndReturnsNewValue()
    {
        var dictionary = new Dictionary<int, string>();
        var result = dictionary.AddOrUpdate(1, _ => "one", (_, oldValue) => oldValue + " updated");
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

    [Fact]
    public void GetOrAdd_WithNullFactory_ThrowsArgumentNullExceptionEvenWhenKeyExists()
    {
        IDictionary<int, string> dictionary = new Dictionary<int, string> { [1] = "one" };

        Assert.Throws<ArgumentNullException>(() => dictionary.GetOrAdd(1, (Func<int, string>)null!));
    }

    [Fact]
    public void AddOrUpdate_WithNullUpdateFactory_ThrowsArgumentNullException()
    {
        IDictionary<int, string> dictionary = new Dictionary<int, string>();

        Assert.Throws<ArgumentNullException>(() => dictionary.AddOrUpdate(
            1,
            _ => "one",
            (Func<int, string, string>)null!));
    }
}
