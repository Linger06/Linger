using Linger.Extensions.IO;

namespace Linger.UnitTests.Extensions.IO;

public class PathExtensionsTest
{
    private readonly ITestOutputHelper _outputHelper;

    public PathExtensionsTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void GetAbsolutePath_WithRelativePath_ReturnsFullPath()
    {
        var relativePath = "test/path";
        var result = relativePath.GetAbsolutePath();
        Assert.True(Path.IsPathRooted(result));
        Assert.Contains("test/path", result.Replace("\\", "/"));
    }

    [Fact]
    public void GetRelativePath_FromCurrentDirectory_ReturnsRelativePath()
    {
        var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "test", "path");
        var result = absolutePath.GetRelativePath();
        Assert.False(Path.IsPathRooted(result));
        Assert.Contains("test", result);
    }

    [Theory]
    [InlineData(@"C:\temp\test.txt", true)]
    [InlineData(@"relative\path.txt", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsAbsolutePath_ChecksPathType(string path, bool expected)
    {
        var result = path.IsAbsolutePath();
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RelativeTo_CalculatesRelativePath()
    {
        var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "test", "file.txt");
        var folderPath = Directory.GetCurrentDirectory();
        var result = sourcePath.RelativeTo(folderPath);
        Assert.Equal(Path.Combine("test", "file.txt").Replace("\\", "/"), 
                    result.Replace("\\", "/"));
    }

    [Theory]
    [InlineData(@"path\to\file", "path/to/file")]
    [InlineData(@"path\\to\\file", "path/to/file")]
    public void NormalizeWithSeparator_NormalizesPath(string input, string expected)
    {
        var result = input.NormalizeWithSeparator();
        Assert.Equal(expected, result.Replace("\\", "/"));
    }

    [Fact]
    public void ToAbsolutePath_ConvertToAbsolutePath()
    {
        var relativePath = "test/file.txt";
        var basePath = Directory.GetCurrentDirectory();
        var result = relativePath.ToAbsolutePath(basePath);
        Assert.True(Path.IsPathRooted(result));
        Assert.Contains("test/file.txt", result.Replace("\\", "/"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetParentPath_ReturnsParentDirectory(int levels)
    {
        var path = Path.Combine("root", "level1", "level2", "file.txt");
        var result = path.GetParentPath(levels);
        var expected = levels == 1 
            ? Path.Combine("root", "level1", "level2")
            : Path.Combine("root", "level1");
        Assert.Equal(expected.Replace("\\", "/"), result.Replace("\\", "/"));
    }

    [Theory]
    [InlineData(@"C:\temp\test.txt", @"C:\temp\test.txt", true)]
    [InlineData(@"C:\temp\test.txt", @"C:\temp\other.txt", false)]
    [InlineData(@"path\test.txt", @"path/test.txt", true)]
    public void PathEqualsTo_ComparesPathsCorrectly(string path1, string path2, bool expected)
    {
        var result = path1.PathEqualsTo(path2);
        Assert.Equal(expected, result);
    }
}