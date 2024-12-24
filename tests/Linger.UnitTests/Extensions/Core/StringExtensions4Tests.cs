namespace Linger.UnitTests.Extensions.Core;

public class StringExtensions4Tests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("dGVzdA==", "test")]
    public void FromBase64ToString_ShouldReturnExpectedResult(string? value, string expected)
    {
        var result = value.FromBase64ToString();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("test", "dGVzdA==")]
    public void ToBase64String_ShouldReturnExpectedResult(string? value, string expected)
    {
        var result = value.ToBase64String();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("test@example.com", "test")]
    [InlineData("invalid-email", "")]
    public void EmailPrefix_ShouldReturnExpectedResult(string? value, string expected)
    {
        var result = value.EmailPrefix();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test\n", "test")]
    [InlineData("test\r\n", "test")]
    public void DelLastNewLine_ShouldReturnExpectedResult(string value, string expected)
    {
        var result = value.DelLastNewLine();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test\n", "test")]
    [InlineData("test\r\n", "test")]
    [InlineData(null, "")]
    public void DelAllNewLine_ShouldReturnExpectedResult(string? value, string expected)
    {
        var result = value.DelAllNewLine();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test,", "test")]
    public void DelLastComma_ShouldReturnExpectedResult(string value, string expected)
    {
        var result = value.DelLastComma();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test,", ",", "test")]
    [InlineData("", ",", "")]
    public void DelLastChar_ShouldReturnExpectedResult(string value, string character, string expected)
    {
        var result = value.DelLastChar(character);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test", "prefix", "prefixtest")]
    [InlineData("prefixtest", "prefix", "prefixtest")]
    public void EnsureStartsWith_ShouldReturnExpectedResult(string value, string prefix, string expected)
    {
        var result = value.EnsureStartsWith(prefix);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test", "suffix", "testsuffix")]
    [InlineData("testsuffix", "suffix", "testsuffix")]
    public void EnsureEndsWith_ShouldReturnExpectedResult(string value, string suffix, string expected)
    {
        var result = value.EnsureEndsWith(suffix);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test", 2, "te")]
    [InlineData("test", 10, "test")]
    public void Substring2_ShouldReturnExpectedResult(string value, int length, string expected)
    {
        var result = value.Substring2(length);
        Assert.Equal(expected, result);
    }

    public partial class StringExtensionsTests
    {
        [Fact]
        public void ToDataTable_ShouldReturnCorrectDataTable()
        {
            // Arrange
            var json = @"
            [
                { ""Name"": ""John"", ""Age"": 30 },
                { ""Name"": ""Jane"", ""Age"": 25 }
            ]";

            // Act
            var result = json.ToDataTable();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
            Assert.Equal("John", result.Rows[0]["Name"]);
            Assert.Equal(30, result.Rows[0]["Age"].ToInt());
            Assert.Equal("Jane", result.Rows[1]["Name"]);
            Assert.Equal(25, result.Rows[1]["Age"].ToInt());
        }

        [Fact]
        public void ToDataTable_ThrowsJsonException_WhenJsonIsInvalid()
        {
            var json = "Invalid JSON";
            Assert.Throws<JsonException>(() => json.ToDataTable());
        }

        [Fact]
        public void ToDataTable_ShouldReturnNull_WhenJsonIsEmpty()
        {
            // Arrange
            var json = string.Empty;

            // Act
            var result = json.ToDataTable();

            // Assert
            Assert.Null(result);
        }
    }
}