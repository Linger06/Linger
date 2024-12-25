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
    [InlineData("C:\\example\\path\\test.txt", "C:\\example\\path\\test.txt")]
    [InlineData("C:\\example\\path\\../test", "C:\\example\\test")]
    [InlineData("C:/example/path", "C:\\example\\path")]
    [InlineData("C:/example/path/", "C:\\example\\path")]
    [InlineData("/example/path/", "/example/path")]
    [InlineData("/example/path", "/example/path")]
    [InlineData("/", "")]
    [InlineData("file:///C:/folder/file.txt", "C:\folder\file.txt")]
    public void NormalizePath_ShouldReturnNormalizedPath(string input, string expected)
    {
        var result = PathHelper.NormalizePath(input);
        Assert.Equal(expected, result);
    }
}