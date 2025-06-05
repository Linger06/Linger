namespace Linger.UnitTests.Extensions.Core;

public partial class StringExtensionsTests
{
    [Theory]
    [InlineData("line1\r\nline2", "\r\n", new[] { "line1", "line2" })]
    [InlineData("line1,line2", ",", new[] { "line1", "line2" })]
    [InlineData("", ",", new string[] { })]
    [InlineData(null, ",", new string[] { })]
    public void ToSplitList_String_ShouldReturnExpectedResult(string? value, string symbol, string[] expected)
    {
        var result = value.SplitToList(symbol).ToArray();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("line1,line2", ',', new[] { "line1", "line2" })]
    public void ToSplitList_Char_ShouldReturnExpectedResult(string value, char symbol, string[] expected)
    {
        var result = value.SplitToList(symbol).ToArray();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("a,b,c", ",", new[] { "a", "b", "c" })]
    [InlineData("a|b|c", "|", new[] { "a", "b", "c" })]
    [InlineData("", ",", new string[] { })]
    [InlineData(null, ",", new string[] { })]
    public void ToSplitList_StringOverload_ShouldReturnExpectedResult(string? input, string symbol, string[] expected)
    {
        var result = input.SplitToList(symbol);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("a,b,c", ',', new[] { "a", "b", "c" })]
    [InlineData("a|b|c", '|', new[] { "a", "b", "c" })]
    [InlineData("", ',', new string[] { })]
    [InlineData(null, ',', new string[] { })]
    public void ToSplitArray_ShouldReturnExpectedResult(string? input, char symbol, string[] expected)
    {
        var result = input.SplitToArray(symbol);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("http://example.com", "param1=value1", "http://example.com?param1=value1")]
    [InlineData("http://example.com?existing=param", "param1=value1", "http://example.com?existing=param&param1=value1")]
    [InlineData("http://example.com", "", "http://example.com")]
    [InlineData("http://example.com?existing=param", "", "http://example.com?existing=param")]
    public void AppendQuery_String_ShouldAppendQueryString(string url, string query, string expected)
    {
        // Act
        var result = url.AppendQuery(query);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("http://example.com", "param1=value1&param2=value2&", "http://example.com?param1=value1&param2=value2")]
    [InlineData("http://example.com?existing=param", "param1=value1&param2=value2&", "http://example.com?existing=param&param1=value1&param2=value2")]
    public void AppendQuery_Dictionary_ShouldAppendQueryString(string url, string query, string expected)
    {
        // Arrange
        var data = new SortedDictionary<string, string>();
        IEnumerable<string>? list = query.Split('&').Where(p => p.IsNotNullAndEmpty());
        foreach (var param in list)
        {
            var keyValue = param.Split('=');
            data[keyValue[0]] = keyValue[1];
        }

        // Act
        var result = url.AppendQuery(data);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("http://example.com", "param1=value1&param2=value2&", "http://example.com?param1=value1&param2=value2")]
    [InlineData("http://example.com?existing=param", "param1=value1&param2=value2&", "http://example.com?existing=param&param1=value1&param2=value2")]
    public void AppendQuery_List_ShouldAppendQueryString(string url, string query, string expected)
    {
        // Arrange
        var data = new List<KeyValuePair<string, string>>();
        IEnumerable<string>? list = query.Split('&').Where(p => p.IsNotNullAndEmpty());
        foreach (var param in list)
        {
            var keyValue = param.Split('=');
            data.Add(new KeyValuePair<string, string>(keyValue[0], keyValue[1]));
        }

        // Act
        var result = url.AppendQuery(data);

        // Assert
        Assert.Equal(expected, result);
    }
}