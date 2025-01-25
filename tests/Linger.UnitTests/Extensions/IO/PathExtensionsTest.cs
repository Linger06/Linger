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

    public static IEnumerable<object[]> PathData = new List<object[]>
    {
        new object[] { "./Test", "", "Test" },
        new object[] { "../Test/Test2", "../Test", "Test2" }
    };

    [Theory]
    [MemberData(nameof(PathData))]
    public void GetRelativePathTest(string path1, string path2, string expectedPath)
    {
        if (path2.IsNotNullAndEmpty())
        {
            var result = path1.GetRelativePath(path2);
            Assert.Equal(expectedPath, result);
        }
        else
        {
            var result = path1.GetRelativePath();
            Assert.Equal(expectedPath, result);
        }
    }


    public static IEnumerable<object[]> PathDataWindows = new List<object[]>
    {
        new object[] { @".\Test", "", "Test" },
        new object[] { @"..\Test", "", @"..\Test" },
        new object[] { @"C:\Path\Test", @"C:\Path", "Test" },
        new object[] { @"D:\Path\Test", @"D:\Path2\Test\Test", @"..\..\..\Path\Test" },
        new object[] { @"D:\Path\Test\a.txt", @"D:\Path", @"Test\a.txt" }
    };

    [Theory]
    [MemberData(nameof(PathDataWindows))]
    public void GetRelativePathTest_Windows(string path1, string path2, string expectedPath)
    {
        Assert.SkipUnless(OSPlatformHelper.IsWindows, "仅在 Windows 平台运行");
        if (path2.IsNotNullAndEmpty())
        {
            var result = path1.GetRelativePath(path2);
            Assert.Equal(expectedPath, result);
        }
        else
        {
            var result = path1.GetRelativePath();
            Assert.Equal(expectedPath, result);
        }
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
        Assert.True(PathHelper.PathEquals(expected, result));
        Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), result);
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
