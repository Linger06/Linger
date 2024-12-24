namespace Linger.UnitTests.Helper;

public class PathNormalizationTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("C:\\Users\\", "C:\\")]
    [InlineData("/home/user/", "\\")]
    public void GetPathRoot_ShouldReturnCorrectRoot(string? input, string expected)
    {
        var result = PathNormalization.GetPathRoot(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("C:\\Users\\", "C:\\Users")]
    [InlineData("/home/user/", "/home/user")]
    public void TrimPath_ShouldTrimTrailingSeparators(string? input, string? expected)
    {
        var result = input.TrimPath();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("C:\\Users\\", "C:\\Users")]
    [InlineData("/home/user/", "/home/user")]
    [InlineData("C:\\Users\\Documents", "C:\\Users")]
    [InlineData("/home/user/docs", "/home/user")]
    public void GetParentDir_ShouldReturnCorrectParentDirectory(string? input, string expected)
    {
        var result = PathNormalization.GetParentDir(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, "file.txt", "file.txt")]
    [InlineData("", "file.txt", "file.txt")]
    [InlineData("C:\\Users", "file.txt", "C:\\Users\\file.txt")]
    [InlineData("/home/user", "file.txt", "/home/user/file.txt")]
    public void Combine_ShouldReturnCorrectCombinedPath(string? folder, string name, string expected)
    {
        var result = PathNormalization.Combine(folder, name);
        Assert.Equal(expected, result);
    }
}