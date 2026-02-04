using Linger.Extensions.IO;

namespace Linger.UnitTests.Extensions.IO;

public class PathExtensionsTests : IDisposable
{
    private readonly string _testDirectory;

    public PathExtensionsTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"PathExtensionsTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // 忽略清理错误
        }
    }

#pragma warning disable CS0618 // 测试废弃的方法
    
    [Fact]
    public void GetRelativePath_WithAbsolutePath_ShouldReturnRelativePath()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "subdir");
        Directory.CreateDirectory(subDir);
        var filePath = Path.Combine(subDir, "file.txt");

        // Act
        var relativePath = filePath.GetRelativePath(_testDirectory);

        // Assert
        Assert.Equal(Path.Combine("subdir", "file.txt"), relativePath);
    }

    [Fact]
    public void GetRelativePath_WithNullRelativeTo_ShouldUseCurrentDirectory()
    {
        // Arrange
        var originalDir = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = _testDirectory;
            var subDir = Path.Combine(_testDirectory, "subdir");
            Directory.CreateDirectory(subDir);
            var filePath = Path.Combine(subDir, "file.txt");

            // Act
            var relativePath = filePath.GetRelativePath(null);

            // Assert
            Assert.Equal(Path.Combine("subdir", "file.txt"), relativePath);
        }
        finally
        {
            Environment.CurrentDirectory = originalDir;
        }
    }

    [Fact]
    public void GetAbsolutePath_WithAbsolutePath_ShouldReturnSamePath()
    {
        // Arrange
        var absolutePath = Path.Combine(_testDirectory, "file.txt");

        // Act
        var result = absolutePath.GetAbsolutePath();

        // Assert
        Assert.Equal(Path.GetFullPath(absolutePath), result);
    }

    [Fact]
    public void GetAbsolutePath_WithRelativePath_ShouldCombineWithBasePath()
    {
        // Arrange
        var relativePath = "subdir\\file.txt";

        // Act
        var result = relativePath.GetAbsolutePath(_testDirectory);

        // Assert
        Assert.Equal(Path.Combine(_testDirectory, "subdir", "file.txt"), result);
    }

    [Fact]
    public void GetAbsolutePath_WithNullBasePath_ShouldUseCurrentDirectory()
    {
        // Arrange
        var originalDir = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = _testDirectory;
            var relativePath = "subdir\\file.txt";

            // Act
            var result = relativePath.GetAbsolutePath(null);

            // Assert
            Assert.Equal(Path.Combine(_testDirectory, "subdir", "file.txt"), result);
        }
        finally
        {
            Environment.CurrentDirectory = originalDir;
        }
    }

    [Fact]
    public void IsAbsolutePath_WithAbsolutePath_ShouldReturnTrue()
    {
        // Arrange
        var absolutePath = @"C:\Users\Test\file.txt";

        // Act
        var result = absolutePath.IsAbsolutePath();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAbsolutePath_WithRelativePath_ShouldReturnFalse()
    {
        // Arrange
        var relativePath = @"subdir\file.txt";

        // Act
        var result = relativePath.IsAbsolutePath();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAbsolutePath_WithNullPath_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((string)null!).IsAbsolutePath());
    }

    [Fact]
    public void IsAbsolutePath_WithEmptyPath_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => string.Empty.IsAbsolutePath());
    }

    [Fact]
    public void RelativeTo_WithValidPaths_ShouldReturnRelativePath()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "subdir");
        Directory.CreateDirectory(subDir);
        var filePath = Path.Combine(subDir, "file.txt");

        // Act
        var relativePath = filePath.RelativeTo(_testDirectory);

        // Assert
        Assert.Equal(Path.Combine("subdir", "file.txt"), relativePath);
    }

    [Fact]
    public void RelativeTo_WithNullSourcePath_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((string)null!).RelativeTo(_testDirectory));
    }

    [Fact]
    public void RelativeTo_WithNullFolder_ShouldThrowArgumentException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "file.txt");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => filePath.RelativeTo(null!));
    }

    [Fact]
    public void RelativeTo_WithEmptyFolder_ShouldThrowArgumentException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "file.txt");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => filePath.RelativeTo(string.Empty));
    }

    [Fact]
    public void RelativeTo_WithRelativeSourcePath_ShouldConvertToAbsolute()
    {
        // Arrange
        var originalDir = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = _testDirectory;
            var relativePath = "file.txt";

            // Act
            var result = relativePath.RelativeTo(_testDirectory);

            // Assert
            Assert.Equal("file.txt", result);
        }
        finally
        {
            Environment.CurrentDirectory = originalDir;
        }
    }

    [Fact]
    public void RelativeTo_WithFolderWithoutTrailingSeparator_ShouldWork()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "subdir");
        Directory.CreateDirectory(subDir);
        var filePath = Path.Combine(subDir, "file.txt");
        var folderWithoutSeparator = _testDirectory.TrimEnd(Path.DirectorySeparatorChar);

        // Act
        var relativePath = filePath.RelativeTo(folderWithoutSeparator);

        // Assert
        Assert.Equal(Path.Combine("subdir", "file.txt"), relativePath);
    }

#pragma warning restore CS0618
}
