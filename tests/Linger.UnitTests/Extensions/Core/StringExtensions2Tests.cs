namespace Linger.UnitTests.Extensions.Core;

public class StringExtensions2Tests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("  test  ", "test")]
    [InlineData("test", "test")]
    public void ToNotSpaceString_ShouldReturnExpectedResult(string? value, string expected)
    {
        var result = value.ToNotSpaceString();
        Assert.Equal(expected, result);
    }

}