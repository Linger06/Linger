namespace Linger.UnitTests;

public class ParameterListTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmptyParameters()
    {
        var paramList = new ParameterList();
        Assert.Empty(paramList.Parameters);
    }

    [Fact]
    public void Constructor_WithSingleKeyValuePair_ShouldAddParameter()
    {
        var paramList = new ParameterList("key1", "value1");
        Assert.Single(paramList.Parameters);
        Assert.Equal("value1", paramList.Parameters["key1"]);
    }

    [Fact]
    public void Constructor_WithMultipleKeyValuePairs_ShouldAddParameters()
    {
        var keys = new string[] { "key1", "key2" };
        var values = new object[] { "value1", "value2" };
        var paramList = new ParameterList(keys, values);

        Assert.Equal(2, paramList.Parameters.Count);
        Assert.Equal("value1", paramList.Parameters["key1"]);
        Assert.Equal("value2", paramList.Parameters["key2"]);
    }

    [Fact]
    public void Constructor_WithMismatchedArrays_ShouldThrowException()
    {
        var keys = new string[] { "key1" };
        var values = new object[] { "value1", "value2" };

        Assert.Throws<ArgumentException>(() => new ParameterList(keys, values));
    }

    [Fact]
    public void Add_ShouldAddParameter()
    {
        var paramList = new ParameterList();
        paramList.Add("key1", "value1");

        Assert.Single(paramList.Parameters);
        Assert.Equal("value1", paramList.Parameters["key1"]);
    }

    [Fact]
    public void Add_DuplicateKey_ShouldThrowException()
    {
        var paramList = new ParameterList();
        paramList.Add("key1", "value1");

        Assert.Throws<ArgumentException>(() => paramList.Add("key1", "value2"));
    }

    [Fact]
    public void Get_ShouldReturnParameterValue()
    {
        var paramList = new ParameterList();
        paramList.Add("key1", "value1");

        var value = paramList.Get<string>("key1");
        Assert.Equal("value1", value);
    }

    [Fact]
    public void Get_NonExistentKey_ShouldThrowException()
    {
        var paramList = new ParameterList();

        Assert.Throws<KeyNotFoundException>(() => paramList.Get<string>("key1"));
    }

    [Fact]
    public void Remove_ShouldRemoveParameter()
    {
        var paramList = new ParameterList();
        paramList.Add("key1", "value1");

        var removed = paramList.Remove("key1");
        Assert.True(removed);
        Assert.Empty(paramList.Parameters);
    }

    [Fact]
    public void Remove_NonExistentKey_ShouldReturnFalse()
    {
        var paramList = new ParameterList();
        var removed = paramList.Remove("key1");
        Assert.False(removed);
    }

    [Fact]
    public void Clear_ShouldRemoveAllParameters()
    {
        var paramList = new ParameterList();
        paramList.Add("key1", "value1");
        paramList.Add("key2", "value2");

        paramList.Clear();
        Assert.Empty(paramList.Parameters);
    }

    [Fact]
    public void ContainsKey_ShouldReturnTrueIfKeyExists()
    {
        var paramList = new ParameterList();
        paramList.Add("key1", "value1");

        var contains = paramList.ContainsKey("key1");
        Assert.True(contains);
    }

    [Fact]
    public void ContainsKey_ShouldReturnFalseIfKeyDoesNotExist()
    {
        var paramList = new ParameterList();

        var contains = paramList.ContainsKey("key1");
        Assert.False(contains);
    }

    [Fact]
    public void SetValue_ShouldUpdateParameterValue()
    {
        var paramList = new ParameterList();
        paramList.Add("key1", "value1");

        paramList.SetValue("key1", "newValue");
        Assert.Equal("newValue", paramList.Parameters["key1"]);
    }

    [Fact]
    public void SetValue_NonExistentKey_ShouldAddParameter()
    {
        var paramList = new ParameterList();
        paramList.SetValue("key1", "value1");

        Assert.Single(paramList.Parameters);
        Assert.Equal("value1", paramList.Parameters["key1"]);
    }

    [Fact]
    public void Enumerator_ShouldIterateOverParameters()
    {
        var paramList = new ParameterList();
        paramList.Add("key1", "value1");
        paramList.Add("key2", "value2");

        IEnumerator<KeyValuePair<string, object>>? enumerator = paramList.GetEnumerator();
        var items = new List<KeyValuePair<string, object>>();

        while (enumerator.MoveNext())
        {
            items.Add(enumerator.Current);
        }

        Assert.Equal(2, items.Count);
        Assert.Contains(new KeyValuePair<string, object>("key1", "value1"), items);
        Assert.Contains(new KeyValuePair<string, object>("key2", "value2"), items);
    }

    [Fact]
    public void NonGenericEnumerator_ShouldIterateOverParameters()
    {
        var paramList = new ParameterList();
        paramList.Add("key1", "value1");
        paramList.Add("key2", "value2");

        IEnumerator? enumerator = ((IEnumerable)paramList).GetEnumerator();
        var items = new List<KeyValuePair<string, object>>();

        while (enumerator.MoveNext())
        {
            var entry = (KeyValuePair<string, object>)enumerator.Current;
            items.Add(entry);
        }

        Assert.Equal(2, items.Count);
        Assert.Contains(new KeyValuePair<string, object>("key1", "value1"), items);
        Assert.Contains(new KeyValuePair<string, object>("key2", "value2"), items);
    }
}