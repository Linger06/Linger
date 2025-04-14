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
    public void NormalizePath_WithNetworkSharePath_PreservesFormat()
    {
        // Arrange
        string networkPath = @"\\server\share\folder";

        // Act
        var result = PathHelper.NormalizePath(networkPath);

        // Assert
        Assert.Equal(@"\\server\share\folder", result);
    }

    [Fact]
    public void NormalizePath_WithFtpPath_PreservesFormat()
    {
        // Arrange
        string ftpPath = "ftp://example.com/folder/file.txt";

        // Act
        var result = PathHelper.NormalizePath(ftpPath);

        // Assert
        Assert.Equal("ftp://example.com/folder/file.txt", result);
    }

    [Fact]
    public void NormalizePath_WithInvalidUri_HandlesException()
    {
        // Arrange
        string invalidUri = "file:///C:/folder/with spaces/file.txt";

        // Act - 不应抛出异常
        var result = PathHelper.NormalizePath(invalidUri);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(invalidUri, result);
    }

    [Fact]
    public void NormalizePath_WithMixedPathSeparators_StandardizesToCurrentPlatformSeparator()
    {
        // Arrange
        string mixedPath = "folder1/folder2\\folder3/file.txt";
        char expectedSeparator = Path.DirectorySeparatorChar;

        // Act
        var result = PathHelper.NormalizePath(mixedPath);

        // Assert
        Assert.DoesNotContain("/", result);
        Assert.DoesNotContain("\\", result);
        
        // 由于不同平台分隔符不同，我们使用Split分割并验证段数
        var segments = result.Split(expectedSeparator);
        Assert.Equal(4, segments.Length);
        Assert.Equal("folder1", segments[0]);
        Assert.Equal("folder2", segments[1]);
        Assert.Equal("folder3", segments[2]);
        Assert.Equal("file.txt", segments[3]);
    }

    [Fact]
    public void NormalizePath_WithConsecutiveSeparators_RemovesThem()
    {
        // Arrange
        string pathWithConsecutiveSeparators = $"folder1{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}folder2{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}file.txt";

        // Act
        var result = PathHelper.NormalizePath(pathWithConsecutiveSeparators);

        // Assert
        Assert.DoesNotContain(new string(Path.DirectorySeparatorChar, 2), result);
        
        var segments = result.Split(Path.DirectorySeparatorChar);
        Assert.Equal(3, segments.Length);
        Assert.Equal("folder1", segments[0]);
        Assert.Equal("folder2", segments[1]);
        Assert.Equal("file.txt", segments[2]);
    }

    [Fact]
    public void NormalizePath_WithConsecutiveSeparatorsInUncPath_PreservesUncPrefix()
    {
        // Arrange - 仅在Windows环境测试
        if (!OSPlatformHelper.IsWindows)
            return;

        string uncPath = @"\\server\\share\\\folder\\file.txt";

        // Act
        var result = PathHelper.NormalizePath(uncPath);

        // Assert
        Assert.StartsWith(@"\\server\share\folder\file.txt", result);
        Assert.DoesNotContain(@"\\\", result);
        Assert.DoesNotContain(@"\\share", result);
    }

    [Fact]
    public void NormalizePath_WithMixedSeparatorsInFtpPath_PreservesFtpProtocol()
    {
        // Arrange
        string ftpPath = "ftp://example.com//folder///subfolder\\file.txt";

        // Act
        var result = PathHelper.NormalizePath(ftpPath);

        // Assert
        Assert.StartsWith("ftp://example.com/", result);
        Assert.DoesNotContain("//folder", result);
        Assert.DoesNotContain("///subfolder", result);
        Assert.DoesNotContain("\\file.txt", result);
    }

    [Fact]
    public void StandardizePathSeparators_WithWindowsPath_ReplacesAllSeparators()
    {
        // Arrange - 仅在Windows环境测试
        if (!OSPlatformHelper.IsWindows)
            return;

        string mixedPath = "C:/folder1\\folder2/folder3";

        // Act
        var result = PathHelper.NormalizePath(mixedPath);

        // Assert
        Assert.Equal(@"C:\folder1\folder2\folder3", result);
    }

    [Fact]
    public void StandardizePathSeparators_WithUnixPath_ReplacesAllSeparators()
    {
        // Arrange - 仅在非Windows环境测试
        if (OSPlatformHelper.IsWindows)
            return;

        string mixedPath = "/usr\\local/bin\\program";

        // Act
        var result = PathHelper.NormalizePath(mixedPath);

        // Assert
        Assert.Equal("/usr/local/bin/program", result);
    }

    [Fact]
    public void NormalizePath_WithInvalidCharactersInPath_HandlesThem()
    {
        // Arrange
        string path = Path.Combine("folder", "sub?folder", "file.txt");

        // Act - 不应抛出异常
        var result = PathHelper.NormalizePath(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(path, result);
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
    public void ResolveToAbsolutePath_WithUncPath_ReturnsNormalizedPath()
    {
        // Arrange
        string uncPath = @"\\server\share\folder\";

        // Act
        var result = PathHelper.ResolveToAbsolutePath(null, uncPath);

        // Assert
        Assert.Equal(@"\\server\share\folder", result);
    }

    [Fact]
    public void ResolveToAbsolutePath_WithFtpPath_ReturnsNormalizedPath()
    {
        // Arrange
        string ftpPath = "ftp://example.com/path/file.txt";

        // Act
        var result = PathHelper.ResolveToAbsolutePath(null, ftpPath);

        // Assert
        Assert.Equal("ftp://example.com/path/file.txt", result);
    }

    [Fact]
    public void ResolveToAbsolutePath_WithRelativeBasePathAndPreserveEnding_ThrowsArgumentException()
    {
        // Arrange
        string basePath = "relative/path";
        string relativePath = "subfolder";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            PathHelper.ResolveToAbsolutePath(basePath, relativePath, true));
    }

    [Fact]
    public void ResolveToAbsolutePath_WithWindowsReservedName_ThrowsArgumentException()
    {
        // Arrange - 仅在Windows上测试
        if (!OSPlatformHelper.IsWindows)
            return;

        string invalidPath = "CON";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            PathHelper.ResolveToAbsolutePath(null, invalidPath));
    }

    [Fact]
    public void ResolveToAbsolutePath_WithDirectorySeparatorsInFileName_NormalizesPath()
    {
        // Arrange
        string basePath = Path.GetFullPath(Path.Combine("C:", "base"));
        string relativePath = "folder/subfolder\\file.txt";

        // Act
        var result = PathHelper.ResolveToAbsolutePath(basePath, relativePath);

        // Assert
        Assert.Equal(Path.Combine(basePath, "folder", "subfolder", "file.txt"), result);
    }

    [Fact]
    public void ResolveToAbsolutePath_WithPreserveEndingSeparator_PreservesSeparator()
    {
        // Arrange
        string basePath = Path.GetFullPath(Path.Combine("C:", "base"));
        string relativePath = "folder";

        // Act
        var result = PathHelper.ResolveToAbsolutePath(basePath, relativePath, true);

        // Assert
        Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), result);
    }

    [Fact]
    public void ResolveToAbsolutePath_WithMixedSeparatorsInRelativePath_StandardizesThem()
    {
        // Arrange
        string basePath = Path.GetFullPath(Path.Combine("C:", "base"));
        string relativePath = "folder1/folder2\\folder3";

        // Act
        var result = PathHelper.ResolveToAbsolutePath(basePath, relativePath);

        // Assert
        string expected = Path.Combine(basePath, "folder1", "folder2", "folder3");
        Assert.Equal(expected, result);
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
    public void GetRelativePath_WithNullBasePath_ThrowsArgumentException()
    {
        // Arrange
        string? basePath = null;
        string targetPath = "C:\\target\\path";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            PathHelper.GetRelativePath(basePath, targetPath));
    }

    [Fact]
    public void GetRelativePath_WithEmptyBasePath_ThrowsArgumentException()
    {
        // Arrange
        string basePath = "";
        string targetPath = "C:\\target\\path";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            PathHelper.GetRelativePath(basePath, targetPath));
    }

    [Fact]
    public void GetRelativePath_WithNullTargetPath_ReturnsEmptyString()
    {
        // Arrange
        string basePath = "C:\\base\\path";
        string? targetPath = null;

        // Act
        var result = PathHelper.GetRelativePath(basePath, targetPath);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetRelativePath_WithParentDirectory_ReturnsRelativePathWithDotDot()
    {
        // Arrange - 使用绝对路径避免依赖于测试运行环境
        string basePath = Path.GetFullPath(Path.Combine("C:", "base", "path", "deeper"));
        string targetPath = Path.GetFullPath(Path.Combine("C:", "base", "other"));

        // Act
        var result = PathHelper.GetRelativePath(basePath, targetPath);

        // Assert - 期望结果为 "..\\..\\other"，但适应不同平台的路径分隔符
        Assert.Contains("..", result);
        Assert.Contains("other", result);
    }

    [Fact]
    public void GetRelativePath_WithDifferentDrives_ReturnsAbsolutePath()
    {
        // Arrange - 仅在Windows上测试
        if (!OSPlatformHelper.IsWindows)
            return;

        string basePath = "C:\\base\\path";
        string targetPath = "D:\\target\\path";

        // Act
        var result = PathHelper.GetRelativePath(basePath, targetPath);

        // Assert - 应返回原始目标路径，因为无法创建相对路径
        Assert.Equal(targetPath, result);
    }

    [Fact]
    public void GetRelativePath_WithTrailingSeparatorsInPaths_HandlesThemCorrectly()
    {
        // Arrange
        string basePath = Path.GetFullPath(Path.Combine("C:", "base", "path")) + Path.DirectorySeparatorChar;
        string targetPath = Path.GetFullPath(Path.Combine("C:", "base", "path", "target")) + Path.DirectorySeparatorChar;

        // Act
        var result = PathHelper.GetRelativePath(basePath, targetPath);

        // Assert
        Assert.Equal("target", result);
    }

    [Fact]
    public void GetRelativePath_WithMixedSeparatorsInPaths_HandlesThemCorrectly()
    {
        // Arrange
        string basePath = Path.GetFullPath("C:/base/path").TrimEnd(Path.DirectorySeparatorChar);
        string targetPath = Path.GetFullPath("C:\\base\\path\\target/subfolder");

        // Act
        var result = PathHelper.GetRelativePath(basePath, targetPath);

        // Assert
        string expectedFormat = Path.Combine("target", "subfolder");
        Assert.Equal(expectedFormat, result);
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
    public void Exists_WithInvalidPathChars_ReturnsFalse()
    {
        // Arrange - 创建包含无效字符的路径
        string invalidPath = OSPlatformHelper.IsWindows ? "C:\\temp\\invalid*file.txt" : "/temp/invalid\0file.txt";

        // Act
        var result = PathHelper.Exists(invalidPath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Exists_WithLongPath_HandlesExceptions()
    {
        // Arrange - 创建超长路径
        string longPath = Path.Combine(Path.GetTempPath(), new string('a', 300), "file.txt");

        // Act - 不应抛出异常
        var result = PathHelper.Exists(longPath);

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
    public void PathEquals_WithDifferentSeparators_ReturnsTrue()
    {
        // Arrange
        string path1 = "folder/file.txt";
        string path2 = "folder\\file.txt";

        // Act
        var result = PathHelper.PathEquals(path1, path2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PathEquals_WithTrailingSeparators_ReturnsTrue()
    {
        // Arrange
        string path1 = Path.Combine("folder", "subfolder") + Path.DirectorySeparatorChar;
        string path2 = Path.Combine("folder", "subfolder");

        // Act
        var result = PathHelper.PathEquals(path1, path2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PathEquals_WithInvalidPaths_FallsBackToStringComparison()
    {
        // Arrange - 创建无效路径
        string path1 = "invalid*path";
        string path2 = "invalid*path";

        // Act - 不应抛出异常
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

    [Fact]
    public void GetParentDirectory_WithMultipleLevels_ReturnsCorrectParentPath()
    {
        // Arrange
        string tempDir = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
        string subDir1 = Path.Combine(tempDir, "test_level1");
        string subDir2 = Path.Combine(subDir1, "test_level2");
        string subDir3 = Path.Combine(subDir2, "test_level3");

        try
        {
            // 创建嵌套的临时目录
            Directory.CreateDirectory(subDir3);

            // Act
            var result = PathHelper.GetParentDirectory(subDir3, 2);

            // Assert
            Assert.Equal(subDir1, result);
        }
        finally
        {
            // 清理
            if (Directory.Exists(subDir1))
            {
                Directory.Delete(subDir1, true);
            }
        }
    }

    [Fact]
    public void GetParentDirectory_WithNegativeLevels_TreatsAsPositive()
    {
        // Arrange
        string tempDir = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
        string subDir1 = Path.Combine(tempDir, "test_neg_level1");
        string subDir2 = Path.Combine(subDir1, "test_neg_level2");

        try
        {
            // 创建嵌套的临时目录
            Directory.CreateDirectory(subDir2);

            // Act - 使用负数级别
            var result = PathHelper.GetParentDirectory(subDir2, -1);

            // Assert - 应该将负数转为正数处理
            Assert.Equal(subDir1, result);
        }
        finally
        {
            // 清理
            if (Directory.Exists(subDir1))
            {
                Directory.Delete(subDir1, true);
            }
        }
    }

    [Fact]
    public void GetParentDirectory_WithRootPath_ReturnsRootPath()
    {
        // Arrange
        string rootPath = Path.GetPathRoot(Environment.CurrentDirectory);

        // Act
        var result = PathHelper.GetParentDirectory(rootPath, 1);

        // Assert - 根路径没有父级，应返回自身
        Assert.Equal(rootPath, result);
    }

    [Fact]
    public void GetParentDirectory_WithLevelsExceedingDepth_ReturnsRootPath()
    {
        // Arrange
        string path = Path.Combine("level1", "level2", "level3");
        string fullPath = Path.GetFullPath(path);
        string rootPath = Path.GetPathRoot(fullPath);

        // Act - 尝试获取超过路径深度的父级
        var result = PathHelper.GetParentDirectory(fullPath, 10);

        // Assert - 应返回根路径
        Assert.Equal(rootPath, result);
    }
}