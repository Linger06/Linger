namespace Linger.UnitTests.Helper;

public class PathHelperTests
{
    [Theory]
    [InlineData("C:\\example\\path", "C:\\example\\path")]
    [InlineData("C:\\example\\path\\", "C:\\example\\path")]
    [InlineData("\\\\server\\share\\path", "\\\\server\\share\\path")]
    [InlineData("//server/share/path", "//server/share/path")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void NormalizePath_ShouldReturnNormalizedPath(string input, string expected)
    {
        var result = PathHelper.NormalizePath(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("C:/example/path", "C:\\example\\path")]
    [InlineData("C:/example/path/", "C:\\example\\path")]
    public void NormalizePath_ShouldHandleDifferentPathFormats(string input, string expected)
    {
        var result = PathHelper.NormalizePath(input);
        Assert.Equal(expected, result);
    }
}