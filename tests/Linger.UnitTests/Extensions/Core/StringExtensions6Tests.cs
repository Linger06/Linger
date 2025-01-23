namespace Linger.UnitTests.Extensions.Core;

public class StringExtensions6Tests
{
    [Theory]
    [InlineData("test@example.com;test2@example.com", true)]
    [InlineData("invalid-email", false)]
    public void IsMultipleEmail_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsMultipleEmail();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("abc", false)]
    public void IsPositiveInteger_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsPositiveInteger();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("-123", true)]
    [InlineData("abc", false)]
    public void IsInteger_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsInteger();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", 0, 0, false)]
    [InlineData("123", 3, 0, true)]
    [InlineData("123.45", 3, 2, true)]
    [InlineData("123.456", 3, 2, false)]
    [InlineData("abc", 3, 0, false)]
    public void IsNumber_ShouldReturnExpectedResult(string value, int precision, int scale, bool expected)
    {
        var result = value.IsNumber(precision, scale);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("line1\r\nline2", "\r\n", new[] { "line1", "line2" })]
    [InlineData("line1,line2", ",", new[] { "line1", "line2" })]
    [InlineData("", ",", new string[] { })]
    [InlineData(null, ",", new string[] { })]
    public void ToSplitList_String_ShouldReturnExpectedResult(string? value, string symbol, string[] expected)
    {
        var result = value.ToSplitList(symbol).ToArray();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("line1,line2", ',', new[] { "line1", "line2" })]
    public void ToSplitList_Char_ShouldReturnExpectedResult(string value, char symbol, string[] expected)
    {
        var result = value.ToSplitList(symbol).ToArray();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("line1,line2", ',', new[] { "line1", "line2" })]
    [InlineData(null, ',', new string[] { })]
    public void ToSplitArray_ShouldReturnExpectedResult(string? value, char symbol, string[] expected)
    {
        var result = value.ToSplitArray(symbol);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("http://example.com", "param1=value1", "http://example.com?param1=value1")]
    [InlineData("http://example.com?existing=param", "param1=value1", "http://example.com?existing=param&param1=value1")]
    [InlineData("http://example.com", "", "http://example.com?")]
    [InlineData("http://example.com?existing=param", "", "http://example.com?existing=param&")]
    public void AppendQuery_String_ShouldAppendQueryString(string url, string query, string expected)
    {
        // Act
        var result = url.AppendQuery(query);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("http://example.com", "param1=value1&param2=value2&", "http://example.com?param1=value1&param2=value2&")]
    [InlineData("http://example.com?existing=param", "param1=value1&param2=value2&", "http://example.com?existing=param&param1=value1&param2=value2&")]
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
    [InlineData("http://example.com", "param1=value1&param2=value2&", "http://example.com?param1=value1&param2=value2&")]
    [InlineData("http://example.com?existing=param", "param1=value1&param2=value2&", "http://example.com?existing=param&param1=value1&param2=value2&")]
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