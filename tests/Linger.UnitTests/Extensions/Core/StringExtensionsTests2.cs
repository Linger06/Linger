namespace Linger.UnitTests.Extensions.Core;

public partial class StringExtensionsTests
{

    [Theory]
    [InlineData("test", 2, "te")]
    [InlineData("test", 10, "test")]
    public void Take_ShouldReturnExpectedResult(string value, int length, string expected)
    {
        var result = value.Take(length);
        Assert.Equal(expected, result);
    }

}
