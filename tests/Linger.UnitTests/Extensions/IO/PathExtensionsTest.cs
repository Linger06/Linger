namespace Linger.UnitTests.Extensions.IO;

public class PathExtensionsTests
{
    private readonly string _testPath;
    private readonly string _testBasePath;

    public PathExtensionsTests()
    {
        _testPath = OSPlatformHelper.IsWindows ? @"C:\Projects\Test" : "/Projects/Test";
        _testBasePath = OSPlatformHelper.IsWindows ? @"C:\Projects" : "/Projects";
    }

    [Theory]
    [InlineData("Test", "Projects", "Projects/Test")]
    [InlineData("../Test", "Projects", "Test")]
    [InlineData("./Test", "Projects", "Projects/Test")]
    public void GetAbsolutePath_ReturnsCorrectPath(string path, string basePath, string expectedRelative)
    {
        // Arrange
        var fullBasePath = Path.GetFullPath(basePath);
        var expected = Path.GetFullPath(Path.Combine(fullBasePath, path));

        // Act
        var result = path.GetAbsolutePath(fullBasePath);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetRelativePath_CommonTest()
    {
        // Arrange
        var path = Path.Combine(_testBasePath, "SubDir");
        var relativeTo = _testBasePath;

        // Act
        var result = path.GetRelativePath(relativeTo);

        // Assert
        Assert.Equal("SubDir", result);
    }

    private static string GetRelativePathCompat(string relativeTo, string path)
    {
#if NETFRAMEWORK
        var uri1 = new Uri(Path.GetFullPath(relativeTo));
        var uri2 = new Uri(Path.GetFullPath(path));
        return Uri.UnescapeDataString(uri1.MakeRelativeUri(uri2).ToString()).Replace('/', Path.DirectorySeparatorChar);
#else
        return Path.GetRelativePath(relativeTo, path);
#endif
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetRelativePath_EmptyRelativeTo_UsesCurrentDirectory(string? relativeTo)
    {
        // Arrange
        var path = Path.Combine(_testBasePath, "Test");
        var expected = GetRelativePathCompat(Environment.CurrentDirectory, path);

        // Act
        var result = path.GetRelativePath(relativeTo);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("C:/Test", true)]
    [InlineData("/Test", true)]
    [InlineData("Test", false)]
    [InlineData("./Test", false)]
    [InlineData("../Test", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void IsAbsolutePath_ReturnsExpectedResult(string? path, bool expected)
    {
        // Act
        var result = path.IsAbsolutePath();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RelativeTo_WithValidPaths()
    {
        // Arrange
        var sourcePath = Path.Combine(_testBasePath, "SubDir", "File.txt");
        var folder = _testBasePath;
        var expected = Path.Combine("SubDir", "File.txt");

        // Act
        var result = sourcePath.RelativeTo(folder);

        // Assert
        Assert.Equal(expected.Replace('\\', '/'), result.Replace('\\', '/'));
    }

    [Fact]
    public void NormalizeWithSeparator_StandardizesPathSeparators()
    {
        // Arrange
        var path = OSPlatformHelper.IsWindows ? @"C:\Test\Path" : "/Test/Path";
        var expected = path.Replace('\\', '/');

        // Act
        var result = path.NormalizeWithSeparator();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToAbsolutePath_WithRelativePath()
    {
        // Arrange
        var path = "Test";
        var basePath = _testBasePath;
        var expected = Path.GetFullPath(Path.Combine(basePath, path));

        // Act
        var result = path.ToAbsolutePath(basePath);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void GetParentPath_ReturnsCorrectParentDirectory(int levels)
    {
        // Arrange
        var path = Path.Combine(_testBasePath, "Level1", "Level2", "Level3");
        var expected = levels == 1
            ? Path.GetFullPath(Path.Combine(_testBasePath, "Level1", "Level2"))
            : Path.GetFullPath(Path.Combine(_testBasePath, "Level1"));

        // Act
        var result = path.GetParentPath(levels);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PathEqualsTo_WithSamePaths_ReturnsTrue()
    {
        // Arrange
        var path1 = Path.Combine(_testBasePath, "Test");
        var path2 = Path.Combine(_testBasePath, "Test");

        // Act
        var result = path1.PathEqualsTo(path2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PathEqualsTo_WithDifferentPaths_ReturnsFalse()
    {
        // Arrange
        var path1 = Path.Combine(_testBasePath, "Test1");
        var path2 = Path.Combine(_testBasePath, "Test2");

        // Act
        var result = path1.PathEqualsTo(path2);

        // Assert
        Assert.False(result);
    }
}
