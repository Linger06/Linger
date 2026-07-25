namespace Linger.Json.UnitTests.Extensions.Core;

public class StringJsonExtensionsTests
{
    [Fact]
    public void ToDataTable_ShouldReturnCorrectDataTable()
    {
        const string json = """
            [
                { "Name": "John", "Age": 30 },
                { "Name": "Jane", "Age": 25 }
            ]
            """;

        var result = json.ToDataTable();

        Assert.NotNull(result);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal("John", result.Rows[0]["Name"]);
        Assert.Equal(30L, result.Rows[0]["Age"]);
        Assert.Equal("Jane", result.Rows[1]["Name"]);
        Assert.Equal(25L, result.Rows[1]["Age"]);
    }

    [Fact]
    public void ToDataTable_ThrowsJsonException_WhenJsonIsInvalid()
    {
        Assert.Throws<JsonException>(() => "Invalid JSON".ToDataTable());
    }

    [Fact]
    public void ToDataTable_ShouldReturnNull_WhenJsonIsEmpty()
    {
        Assert.Null(string.Empty.ToDataTable());
    }
}
