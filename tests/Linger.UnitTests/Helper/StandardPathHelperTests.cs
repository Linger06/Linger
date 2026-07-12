using System;
using System.IO;
using Xunit;

namespace Linger.UnitTests.Helper;

public class PathExtensionsTests
{
    [Fact]
    public void CleanAndNormalizePureString_ShouldStandardizePaths()
    {
        // Basic path normalization cases.
        string path1 = "path/to/file";
        string path2 = "path\\to\\file";
        string path3 = "path//to\\\\file";

        string result1 = PathHelper.CleanAndNormalizePureString(path1, false);
        string result2 = PathHelper.CleanAndNormalizePureString(path2, false);
        string result3 = PathHelper.CleanAndNormalizePureString(path3, false);

        string expectedSeparator = OSPlatformHelper.IsWindows ? "\\" : "/";
        string expected = $"path{expectedSeparator}to{expectedSeparator}file";

        Assert.Equal(expected, result1);
        Assert.Equal(expected, result2);
        Assert.Equal(expected, result3);

        // Blank inputs should normalize to empty strings.
        Assert.Equal("", PathHelper.CleanAndNormalizePureString(null, false));
        Assert.Equal(string.Empty, PathHelper.CleanAndNormalizePureString(string.Empty, false));
        Assert.Equal(string.Empty, PathHelper.CleanAndNormalizePureString("   ", false));
    }

    [Fact]
    public void CleanAndNormalizePureString_WithEndingSeparator_ShouldPreserveEnding()
    {
        // Trailing separators should be preserved when requested.
        string path1 = "path/to/folder";
        string path2 = "path/to/folder/";

        string result1 = PathHelper.CleanAndNormalizePureString(path1, true);
        string result2 = PathHelper.CleanAndNormalizePureString(path2, true);

        string expectedSeparator = OSPlatformHelper.IsWindows ? "\\" : "/";

        Assert.Equal($"path{expectedSeparator}to{expectedSeparator}folder{expectedSeparator}", result1);
        Assert.Equal($"path{expectedSeparator}to{expectedSeparator}folder{expectedSeparator}", result2);
    }

    [Fact]
    public void GetRelativePath_ShouldCalculateRelativePaths_SchemeA()
    {
        // Arrange
        string baseDir = OSPlatformHelper.IsWindows ? "C:\\base\\path" : "/base/path";
        string targetSameLevel = OSPlatformHelper.IsWindows ? "C:\\base\\other" : "/base/other";
        string targetSubDir = OSPlatformHelper.IsWindows ? "C:\\base\\path\\subdir" : "/base/path/subdir";
        string targetParentDir = OSPlatformHelper.IsWindows ? "C:\\base" : "/base";

        // Expected results
        string expectedSameLevel = OSPlatformHelper.IsWindows ? "..\\other" : "../other";
        string expectedSubDir = "subdir";
        string expectedParent = "..";

        // Act
        var relSameLevel = baseDir.GetRelativePath(targetSameLevel);
        var relSubDir = baseDir.GetRelativePath(targetSubDir);
        var relParent = baseDir.GetRelativePath(targetParentDir);

        // Assert
        Assert.Equal(expectedSameLevel, relSameLevel);
        Assert.Equal(expectedSubDir, relSubDir);
        Assert.Equal(expectedParent, relParent);

        // Same path should resolve to "."
        Assert.Equal(".", baseDir.GetRelativePath(baseDir));

        // Blank base paths are rejected.
        Assert.Throws<ArgumentException>(() => "".GetRelativePath(targetSubDir));
        Assert.Throws<ArgumentException>(() => "   ".GetRelativePath(targetSubDir));

        // Null and empty target paths map back to empty strings.
        Assert.Equal(string.Empty, baseDir.GetRelativePath(""));
        Assert.Equal(string.Empty, baseDir.GetRelativePath(null!));
        Assert.Throws<ArgumentException>(() => baseDir.GetRelativePath("   "));
    }

    [Fact]
    public void GetRelativePath_ShouldRespectUnixCaseSensitivity()
    {
        if (OSPlatformHelper.IsWindows)
        {
            return;
        }

        const string baseDir = "/tmp/LingerCaseBase";
        const string targetDir = "/tmp/lingercasebase";

        string relativePath = baseDir.GetRelativePath(targetDir);

        Assert.Equal("../lingercasebase", relativePath);
        Assert.NotEqual(".", relativePath);
    }

    [Fact]
    public void GetRelativePath_ShouldWrapTargetNormalizationErrors()
    {
        string baseDir = Directory.GetCurrentDirectory();
        var ex = Assert.Throws<ArgumentException>(() => baseDir.GetRelativePath("bad\0path"));

        Assert.Equal("path", ex.ParamName);
        Assert.Contains("Invalid local path for relative calculation.", ex.Message);
    }

    [Fact]
    public void ContainsInvalidPathChars_ShouldDetectInvalidCharacters()
    {
        // Windows-only invalid path characters.
        string invalidWinChars = OSPlatformHelper.IsWindows ? "path*to?file" : null;

        // Invalid character provided by the current runtime.
        string invalidPath = $"path{Path.GetInvalidPathChars()[0]}file";
        string validPath = "path/to/file";

        // Basic validation checks.
        Assert.True(PathExtensions.ContainsInvalidPathChars(invalidPath));
        Assert.False(PathExtensions.ContainsInvalidPathChars(validPath));

        // Windows-specific invalid character checks.
        if (OSPlatformHelper.IsWindows && invalidWinChars != null)
        {
            Assert.True(PathExtensions.ContainsInvalidPathChars(invalidWinChars));
        }

        // Windows reserved device names should be rejected.
        if (OSPlatformHelper.IsWindows)
        {
            Assert.True(PathExtensions.ContainsInvalidPathChars("C:\\CON\\file.txt"));
            Assert.True(PathExtensions.ContainsInvalidPathChars("C:\\path\\NUL"));
            Assert.True(PathExtensions.ContainsInvalidPathChars("LPT1.txt"));
        }

        // Null and empty strings are treated as not-invalid.
        Assert.False(PathExtensions.ContainsInvalidPathChars(null));
        Assert.False(PathExtensions.ContainsInvalidPathChars(string.Empty));
    }

    [Fact]
    public void ContainsInvalidPathChars_ShouldAllowWindowsDevicePathPrefixes()
    {
        if (!OSPlatformHelper.IsWindows)
        {
            return;
        }

        Assert.False(PathExtensions.ContainsInvalidPathChars(@"\\?\C:\temp\file.txt"));
        Assert.False(PathExtensions.ContainsInvalidPathChars(@"\\.\C:\temp\file.txt"));
        Assert.True(PathExtensions.ContainsInvalidPathChars(@"\\?\C:\temp\file?.txt"));
    }

    [Fact]
    public void GetParentDirectory_ShouldReturnCorrectParentPath()
    {
        // Build a nested path for parent directory checks.
        string testPath = Path.Combine("dir1", "dir2", "dir3");
        var fullPath = Path.GetFullPath(testPath);

        // One level up.
        var parentDir = PathExtensions.GetParentDirectory(fullPath, 1);
        var expectedParent = Directory.GetParent(fullPath).FullName;
        Assert.Equal(expectedParent, parentDir);

        // Two levels up.
        var parentOfParent = PathExtensions.GetParentDirectory(fullPath, 2);
        var expectedGrandParent = Directory.GetParent(Directory.GetParent(fullPath).FullName).FullName;
        Assert.Equal(expectedGrandParent, parentOfParent);

        // Zero levels should leave the path unchanged.
        var noChange = PathExtensions.GetParentDirectory(fullPath, 0);
        Assert.Equal(fullPath, noChange);

        // Negative levels are invalid input.
        Assert.Throws<ArgumentOutOfRangeException>(() => PathExtensions.GetParentDirectory(fullPath, -1));
    }

    [Fact]
    public void GetParentDirectory_ZeroLevels_ShouldReturnNormalizedAbsolutePath()
    {
        string relativePath = Path.Combine("dir1", "dir2");

        string result = PathExtensions.GetParentDirectory(relativePath, 0);

        Assert.Equal(Path.GetFullPath(relativePath), result);
    }

    [Fact]
    public void Exists_ShouldDetectFileAndDirectoryExistence()
    {
        // Use the current working directory as a known-good directory path.
        string currentDir = Directory.GetCurrentDirectory();

        // Existing directory should be detected.
        Assert.True(PathExtensions.Exists(currentDir, false));

        // Create a temporary file to verify file existence checks.
        string tempFile = Path.GetTempFileName();
        try
        {
            Assert.True(PathExtensions.Exists(tempFile, true));

            // Random paths under temp should not exist.
            Assert.False(PathExtensions.Exists(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), true));
            Assert.False(PathExtensions.Exists(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), false));

            // Invalid and blank inputs should fail closed.
            Assert.False(PathExtensions.Exists("||invalid||path||"));
            Assert.False(PathExtensions.Exists(null));
            Assert.False(PathExtensions.Exists(""));
            Assert.False(PathExtensions.Exists("   "));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void ToFullPath_ShouldResolveRelativePaths()
    {
        // Relative path resolution should use the provided base directory.
        string baseDir = Directory.GetCurrentDirectory();
        string relativePath = "subdir/file.txt";

        var result = PathExtensions.ToFullPath(relativePath, baseDir);
        var expected = Path.GetFullPath(Path.Combine(baseDir, relativePath));

        Assert.Equal(expected, result);

        // Absolute paths should pass through unchanged.
        string absolutePath = Path.GetFullPath("file.txt");
        var absoluteResult = PathExtensions.ToFullPath(absolutePath, baseDir);
        Assert.Equal(absolutePath, absoluteResult);

        // Null and empty inputs normalize to empty strings.
        Assert.Equal(string.Empty, PathExtensions.ToFullPath("", baseDir));
        Assert.Equal(string.Empty, PathExtensions.ToFullPath(null, baseDir));

        // Whitespace-only input should match Path.GetFullPath and fail eagerly.
        Assert.Throws<ArgumentException>(() => PathExtensions.ToFullPath("   ", baseDir));

        // Requesting a trailing separator should preserve it.
        var withSeparator = PathExtensions.ToFullPath("subdir/", baseDir, true);
        Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), withSeparator);
    }

    [Fact]
    public void ToFullPath_ShouldRejectWindowsReservedNames()
    {
        if (!OSPlatformHelper.IsWindows)
        {
            return;
        }

        string baseDir = Directory.GetCurrentDirectory();

        Assert.Throws<ArgumentException>(() => PathExtensions.ToFullPath(@"con\file.txt", baseDir));
        Assert.Throws<ArgumentException>(() => PathExtensions.ToFullPath("LPT1.txt", baseDir));
    }

    [Fact]
    public void ToFullPath_ShouldAcceptWindowsDevicePaths()
    {
        if (!OSPlatformHelper.IsWindows)
        {
            return;
        }

        const string devicePath = @"\\?\C:\temp\file.txt";

        Assert.Equal(devicePath, PathExtensions.ToFullPath(devicePath));
    }

    [Fact]
    public void ContainsInvalidPathChars_ShouldCheckNonWindowsPlatforms()
    {
        // Embedded null characters should be rejected on every platform.
        string pathWithNull = "path\0with\0null";

        var result = PathExtensions.ContainsInvalidPathChars(pathWithNull);
        Assert.True(result);
    }

    [Fact]
    public void GetParentDirectory_ShouldHandleRootPath()
    {
        // Root paths may not have a parent, but the helper should still return a value.
        string rootPath;
        if (OSPlatformHelper.IsWindows)
        {
            rootPath = "C:\\";
        }
        else
        {
            rootPath = "/";
        }

        var result = PathExtensions.GetParentDirectory(rootPath, 1);
        Assert.NotNull(result);
    }

    [Fact]
    public void GetParentDirectory_WithInvalidPath_ShouldThrowArgumentException()
    {
        const string invalidPath = "bad\0path";

        Assert.Throws<ArgumentException>(() => PathExtensions.GetParentDirectory(invalidPath, 1));
    }

    [Fact]
    public void CleanAndNormalizePureString_ShouldHandleNullAndWhitespace()
    {
        // Additional empty input coverage.
        Assert.Equal(string.Empty, PathHelper.CleanAndNormalizePureString("", false));
        Assert.Equal(string.Empty, PathHelper.CleanAndNormalizePureString("   ", false));
        Assert.Equal(string.Empty, PathHelper.CleanAndNormalizePureString("\t\n", false));
        Assert.Equal(string.Empty, PathHelper.CleanAndNormalizePureString(null, false));
    }

    [Fact]
    public void Exists_ShouldHandleExceptions()
    {
        // Overly long and invalid paths should fail closed instead of throwing.
        var veryLongPath = new string('a', 500);
        var result = PathExtensions.Exists(veryLongPath);
        Assert.False(result);

        var invalidPath = "path\0with\0null";
        var invalidResult = PathExtensions.Exists(invalidPath);
        Assert.False(invalidResult);
    }
}
