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

    [Theory]
    [InlineData(@"C:\temp\CON.txt", true)]
    [InlineData(@"C:\temp\CON .txt", true)]
    [InlineData(@"C:\temp\COM9.log", true)]
    [InlineData(@"C:\temp\COM10.log", false)]
    [InlineData(@"C:\temp\CONSOLE.txt", false)]
    public void ContainsInvalidPathChars_WindowsReservedNames_ReturnsExpectedResult(string path, bool expected)
    {
        if (!OSPlatformHelper.IsWindows)
        {
            return;
        }

        Assert.Equal(expected, PathExtensions.ContainsInvalidPathChars(path));
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
    public void ContainsInvalidPathChars_ShouldCheckNonWindowsPlatforms()
    {
        // Embedded null characters should be rejected on every platform.
        string pathWithNull = "path\0with\0null";

        var result = PathExtensions.ContainsInvalidPathChars(pathWithNull);
        Assert.True(result);
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
