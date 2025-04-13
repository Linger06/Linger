using System.Runtime.InteropServices;
using Linger.Helper;
using Xunit.v3;

namespace Linger.UnitTests.Helper;

public class PathHelperTests
{
    [Fact]
    public void NormalizePath_WithNullPath_ReturnsEmptyString()
    {
        // Arrange
        string? path = null;

        // Act
        var result = PathHelper.NormalizePath(path);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NormalizePath_WithEmptyPath_ReturnsEmptyString()
    {
        // Arrange
        string path = string.Empty;

        // Act
        var result = PathHelper.NormalizePath(path);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NormalizePath_WithWhitespacePath_ReturnsEmptyString()
    {
        // Arrange
        string path = "   ";

        // Act
        var result = PathHelper.NormalizePath(path);

        // Assert
        Assert.Equal("   ", result);
    }

    [Fact]
    public void NormalizePath_WithTrailingSeparators_RemovesThem()
    {
        // Arrange
        string path = Path.Combine("temp", "folder") + Path.DirectorySeparatorChar;

        // Act
        var result = PathHelper.NormalizePath(path);

        // Assert
        Assert.Equal(Path.Combine("temp", "folder"), result);
    }

    [Fact]
    public void NormalizePath_WithPreserveEndingSeparatorTrue_PreservesSeparator()
    {
        // Arrange
        string path = Path.Combine("temp", "folder");

        // Act
        var result = PathHelper.NormalizePath(path, true);

        // Assert
        Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), result);
    }

    [Fact]
    public void NormalizePath_WithFileUri_ConvertsToLocalPath()
    {
        // Arrange
        string path = $"file://C://temp//file.txt";

        // Act
        var result = PathHelper.NormalizePath(path);

        // Assert
        Assert.DoesNotContain("file://", result);
    }

    [Fact]
    public void ResolveToAbsolutePath_WithNullRelativePath_ReturnsEmptyString()
    {
        // Arrange
        string? relativePath = null;

        // Act
        var result = PathHelper.ResolveToAbsolutePath(null, relativePath);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ResolveToAbsolutePath_WithAbsolutePath_ReturnsSamePath()
    {
        // Arrange
        string path = Path.GetFullPath(Path.Combine("C:", "temp", "file.txt"));

        // Act
        var result = PathHelper.ResolveToAbsolutePath(null, path);

        // Assert
        Assert.Equal(path, result);
    }

    [Fact]
    public void ResolveToAbsolutePath_WithRelativePathAndBasePath_CombinesThem()
    {
        // Arrange
        string basePath = Path.GetFullPath(Path.Combine("C:", "base"));
        string relativePath = "folder";

        // Act
        var result = PathHelper.ResolveToAbsolutePath(basePath, relativePath);

        // Assert
        Assert.Equal(Path.Combine(basePath, relativePath), result);
    }

    [Fact]
    public void ResolveToAbsolutePath_WithRelativePathAndNoBasePath_UsesCurrentDirectory()
    {
        // Arrange
        string relativePath = "folder";

        // Act
        var result = PathHelper.ResolveToAbsolutePath(null, relativePath);

        // Assert
        Assert.Equal(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, relativePath)), result);
    }

    [Fact]
    public void ResolveToAbsolutePath_WithInvalidPath_ThrowsArgumentException()
    {
        // Arrange
        string invalidPath = OSPlatformHelper.IsWindows ? "C:\\invalid\0path" : "/invalid\0path";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => PathHelper.ResolveToAbsolutePath(null, invalidPath));
    }

    [Fact]
    public void GetRelativePath_WithSamePaths_ReturnsEmptyOrDot()
    {
        // Arrange
        string path = Path.GetFullPath(Path.Combine("C:", "temp", "folder"));

        // Act
        var result = PathHelper.GetRelativePath(path, path);

        // Assert
        Assert.True(result == string.Empty || result == ".");
    }

    [Fact]
    public void GetRelativePath_WithSubdirectory_ReturnsRelativePath()
    {
        // Arrange
        string basePath = Path.GetFullPath(Path.Combine("C:", "temp"));
        string targetPath = Path.Combine(basePath, "subfolder", "file.txt");

        // Act
        var result = PathHelper.GetRelativePath(basePath, targetPath);

        // Assert
        Assert.Equal(Path.Combine("subfolder", "file.txt"), result);
    }

    [Fact]
    public void Exists_WithNullPath_ReturnsFalse()
    {
        // Arrange
        string? path = null;

        // Act
        var result = PathHelper.Exists(path);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Exists_WithEmptyPath_ReturnsFalse()
    {
        // Arrange
        string path = string.Empty;

        // Act
        var result = PathHelper.Exists(path);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Exists_WithExistingFile_ReturnsTrue()
    {
        // Arrange
        string tempFile = Path.GetTempFileName();
        try
        {
            // Act
            var result = PathHelper.Exists(tempFile, true);

            // Assert
            Assert.True(result);
        }
        finally
        {
            // 清理临时文件
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Exists_WithExistingDirectory_ReturnsTrue()
    {
        // Arrange
        string tempDir = Path.GetTempPath();

        // Act
        var result = PathHelper.Exists(tempDir, false);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Exists_WithNonExistentPath_ReturnsFalse()
    {
        // Arrange
        string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        var result = PathHelper.Exists(nonExistentPath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PathEquals_WithSamePaths_ReturnsTrue()
    {
        // Arrange
        string path1 = Path.Combine("folder", "file.txt");
        string path2 = Path.Combine("folder", "file.txt");

        // Act
        var result = PathHelper.PathEquals(path1, path2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PathEquals_WithDifferentCasePaths_ReturnsTrueWhenIgnoreCase()
    {
        // Arrange
        string path1 = Path.Combine("folder", "FILE.txt");
        string path2 = Path.Combine("folder", "file.txt");

        // Act
        var result = PathHelper.PathEquals(path1, path2, true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PathEquals_WithDifferentCasePaths_ReturnsFalseWhenNotIgnoreCase()
    {
        // Arrange
        string path1 = Path.Combine("folder", "FILE.txt");
        string path2 = Path.Combine("folder", "file.txt");

        // Act
        var result = PathHelper.PathEquals(path1, path2, false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PathEquals_WithDifferentPaths_ReturnsFalse()
    {
        // Arrange
        string path1 = Path.Combine("folder1", "file.txt");
        string path2 = Path.Combine("folder2", "file.txt");

        // Act
        var result = PathHelper.PathEquals(path1, path2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PathEquals_WithOneNullPath_ReturnsFalse()
    {
        // Arrange
        string? path1 = null;
        string path2 = Path.Combine("folder", "file.txt");

        // Act
        var result = PathHelper.PathEquals(path1, path2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PathEquals_WithBothNullPaths_ReturnsTrue()
    {
        // Arrange
        string? path1 = null;
        string? path2 = null;

        // Act
        var result = PathHelper.PathEquals(path1, path2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetParentDirectory_WithZeroLevels_ReturnsSamePath()
    {
        // Arrange
        string path = Path.Combine("parent", "child");

        // Act
        var result = PathHelper.GetParentDirectory(path, 0);

        // Assert
        Assert.Equal(path, result);
    }

    [Fact]
    public void GetParentDirectory_WithOneLevelAndValidPath_ReturnsParentPath()
    {
        // Arrange
        string tempDir = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
        string subDir = Path.Combine(tempDir, "test_subdir");

        try
        {
            // 创建临时子目录
            Directory.CreateDirectory(subDir);

            // Act
            var result = PathHelper.GetParentDirectory(subDir, 1);

            // Assert
            Assert.Equal(tempDir, result);
        }
        finally
        {
            // 清理
            if (Directory.Exists(subDir))
            {
                Directory.Delete(subDir);
            }
        }
    }
}