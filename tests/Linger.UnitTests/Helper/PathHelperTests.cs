using System.Runtime.InteropServices;

namespace Linger.Tests.Helper;

public class PathHelperTests
{
    private readonly ITestOutputHelper _output;
    private readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private readonly string _rootPath;
    private readonly string _baseDir;

    public PathHelperTests(ITestOutputHelper output)
    {
        _output = output;
        _rootPath = _isWindows ? "C:\\" : "/";
        _baseDir = _isWindows ? @"C:\test\dir" : "/test/dir";
    }

    public static TheoryData<string, string, string> ValidPathCombinations => new()
    {
        { @"folder1", "file1.txt", "folder1/file1.txt" },
        { @"folder1/", "file1.txt", "folder1/file1.txt" },
        { @"folder1\", "file1.txt", "folder1/file1.txt" },
        { @"folder1/subfolder", "../file1.txt", "folder1/file1.txt" },
    };

    [Theory]
    [MemberData(nameof(ValidPathCombinations))]
    public void NormalizePath_WithValidPaths_ReturnsNormalizedPath(string path1, string path2, string expected)
    {
        // Arrange
        var input = Path.Combine(path1, path2);
        expected = expected.Replace('/', Path.DirectorySeparatorChar);

        // Act
        var result = PathHelper.NormalizePath(input, returnType: PathHelper.PathReturnType.KeepOriginal);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizePath_WithNull_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, PathHelper.NormalizePath(null));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void NormalizePath_WithEmptyOrWhitespace_ReturnsSameString(string input)
    {
        Assert.Equal(input, PathHelper.NormalizePath(input));
    }

    [Theory]
    [InlineData(@"C:\test\path", @"C:\test\path")]
    [InlineData(@"C:\TEST\PATH", @"C:\test\path")]
    public void NormalizePath_WindowsAbsolutePath_ReturnsNormalizedPath(string input, string expected)
    {
        Assert.SkipUnless(_isWindows, "Windows-specific test");
        var result = PathHelper.NormalizePath(input);
        Assert.Equal(expected, result, ignoreCase: true);
    }

    [Theory]
    [InlineData("/test/path", "/test/path")]
    [InlineData("/TEST/PATH", "/TEST/PATH")]
    public void NormalizePath_UnixAbsolutePath_ReturnsNormalizedPath(string input, string expected)
    {
        Assert.SkipWhen(_isWindows, "Unix-specific test");
        var result = PathHelper.NormalizePath(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"\\server\share\file.txt")]
    [InlineData(@"//server/share/file.txt")]
    public void NormalizePath_UNCPath_PreservesFormat(string input)
    {
        var result = PathHelper.NormalizePath(input);
        Assert.Equal(input.Replace('/', Path.DirectorySeparatorChar), result);
    }

    [Fact]
    public void ResolveToAbsolutePath_WithRelativePath_ResolvesCorrectly()
    {
        var basePath = _baseDir;
        var relativePath = "subfolder/file.txt";
        var expected = Path.Combine(basePath, relativePath)
            .Replace('/', Path.DirectorySeparatorChar);

        var result = PathHelper.ResolveToAbsolutePath(basePath, relativePath);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetRelativePath_WithValidPaths_ReturnsCorrectRelativePath()
    {
        var basePath = _baseDir;
        var fullPath = Path.Combine(basePath, "subfolder", "file.txt");
        var expected = Path.Combine("subfolder", "file.txt")
            .Replace('/', Path.DirectorySeparatorChar);

        var result = PathHelper.GetRelativePath(basePath, fullPath);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData(@"C:\test\file.txt", @"c:\TEST\file.txt", true)]
    [InlineData(@"/test/file.txt", @"/test/FILE.txt", false)]
    public void PathEquals_WithVariousPaths_ReturnsExpectedResult(
        string path1, string path2, bool expectedResult)
    {
        var result = PathHelper.PathEquals(path1, path2);
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(@"C:\test\path", @"CON")]
    [InlineData(@"C:\test\path", @"PRN")]
    [InlineData(@"C:\test\path", @"AUX")]
    [InlineData(@"C:\test\path", @"NUL")]
    public void ContainsInvalidPathChars_WindowsReservedNames_ReturnsTrue(string basePath, string fileName)
    {
        Assert.SkipUnless(_isWindows, "Windows-specific test");
        var path = Path.Combine(basePath, fileName);
        Assert.False(PathHelper.Exists(path));
    }

    [Theory]
    [InlineData("test/file.txt", 1, "test")]
    [InlineData("test/sub/file.txt", 2, "test")]
    public void GetParentDirectory_WithValidLevels_ReturnsCorrectParent(
        string path, int levels, string expected)
    {
        var fullPath = Path.Combine(_baseDir, path);
        var expectedPath = Path.Combine(_baseDir, expected);
        var result = PathHelper.GetParentDirectory(fullPath, levels);
        Assert.Equal(expectedPath, result);
    }
}
