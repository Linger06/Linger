namespace Linger.UnitTests.Extensions.Core;

public partial class StringExtensionsTests
{

    [Theory]
    [InlineData("test", 2, "te")]
    [InlineData("test", 10, "test")]
    public void Substring2_ShouldReturnExpectedResult(string value, int length, string expected)
    {
        var result = value.Substring2(length);
        Assert.Equal(expected, result);
    }


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