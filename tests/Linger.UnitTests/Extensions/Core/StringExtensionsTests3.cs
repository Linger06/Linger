namespace Linger.UnitTests.Extensions.Core;

public partial class StringExtensionsTests
{
    [Fact]
    public void ToSplitList_WithRegexPattern_ReturnsExpectedResult()
    {
        var result = "part1part2part3".ToSplitList(@"\d");

        Assert.Equal(["part", "part", "part", string.Empty], result);
    }

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
        IEnumerable<string>? list = query.Split('&').Where(p => p.IsNotNullOrEmpty());
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

    [Fact]
    public void AppendQuery_Dictionary_ShouldEncodeSpecialCharacters()
    {
        var url = "http://example.com";
        var data = new SortedDictionary<string, string>
        {
            ["user name"] = "hello world",
            ["filter"] = "a&b"
        };

        var result = url.AppendQuery(data);

        Assert.Equal("http://example.com?filter=a%26b&user%20name=hello%20world", result);
    }

    [Fact]
    public void AppendQuery_Dictionary_ShouldInsertQueryBeforeFragment()
    {
        var url = "http://example.com/path#section";
        var data = new Dictionary<string, string>
        {
            ["id"] = "123"
        };

        var result = url.AppendQuery(data);

        Assert.Equal("http://example.com/path?id=123#section", result);
    }

    [Theory]
    [InlineData("http://example.com", "param1=value1&param2=value2&", "http://example.com?param1=value1&param2=value2")]
    [InlineData("http://example.com?existing=param", "param1=value1&param2=value2&", "http://example.com?existing=param&param1=value1&param2=value2")]
    public void AppendQuery_List_ShouldAppendQueryString(string url, string query, string expected)
    {
        // Arrange
        var data = new List<KeyValuePair<string, string>>();
        IEnumerable<string>? list = query.Split('&').Where(p => p.IsNotNullOrEmpty());
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

    [Fact]
    public void AppendQuery_List_ShouldEncodeSpecialCharacters()
    {
        var url = "http://example.com";
        var data = new List<KeyValuePair<string, string>>
        {
            new("user name", "hello world"),
            new("filter", "a&b")
        };

        var result = url.AppendQuery(data);

        Assert.Equal("http://example.com?user%20name=hello%20world&filter=a%26b", result);
    }
}
