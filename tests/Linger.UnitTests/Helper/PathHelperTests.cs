using System.Runtime.InteropServices;

namespace Linger.Tests.Helper;

public class PathHelperTests
{
    private readonly string _windowsBasePath = @"C:\test\path";
    private readonly string _unixBasePath = "/test/path";
    private readonly string _basePath;

    public PathHelperTests()
    {
        _basePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? _windowsBasePath
            : _unixBasePath;
    }

    public class ProcessPathTests : PathHelperTests
    {
        [Theory]
        [InlineData(null, null, false, "")]
        [InlineData("", "", false, "")]
        [InlineData(" ", null, false, "")]
        public void ProcessPath_EmptyOrWhitespace_ReturnsExpected(string? basePath, string? relativePath,
            bool preserveEndingSeparator, string expected)
        {
            var result = PathHelper.ProcessPath(basePath, relativePath, preserveEndingSeparator);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("ftp://test.com/path")]
        [InlineData("//network/share")]
        [InlineData("/absolute/unix/path")]
        public void ProcessPath_SpecialPaths_ReturnsNormalizedPath(string path)
        {
            var result = PathHelper.ProcessPath(null, path);
            Assert.Equal(PathHelper.NormalizePath(path), result);
        }

        [Fact]
        public void ProcessPath_RelativePath_CombinesCorrectly()
        {
            var relativePath = "subfolder/file.txt";
            var result = PathHelper.ProcessPath(_basePath, relativePath);
            Assert.True(PathHelper.PathEquals(Path.Combine(_basePath, relativePath), result));
        }
    }

    public class GetRelativePathTests : PathHelperTests
    {
        [Fact]
        public void GetRelativePath_SameDirectory_ReturnsFileName()
        {
            var path = Path.Combine(_basePath, "file.txt");
            var result = PathHelper.GetRelativePath(path, _basePath);
            Assert.Equal("file.txt", result);
        }

        [Fact]
        public void GetRelativePath_SubDirectory_ReturnsRelativePath()
        {
            var path = Path.Combine(_basePath, "sub", "file.txt");
            var result = PathHelper.GetRelativePath(path, _basePath);
            Assert.Equal(Path.Combine("sub", "file.txt"), result);
        }
    }

    public class NormalizePathTests : PathHelperTests
    {
        [Theory]
        [InlineData(null, false, "")]
        [InlineData("", true, "")]
        public void NormalizePath_EmptyOrNull_ReturnsInput(string? input, bool preserveEndingSeparator,
            string expected)
        {
            var result = PathHelper.NormalizePath(input, preserveEndingSeparator);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void NormalizePath_WithPreserveEndingSeparator_AddsDirectorySeparator()
        {
            var result = PathHelper.NormalizePath(_basePath, true);
            Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), result);
        }
    }

    public class ExistsTests : PathHelperTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Exists_EmptyOrWhitespace_ReturnsFalse(string? path)
        {
            Assert.False(PathHelper.Exists(path));
        }


        [Theory]
        [InlineData(@"C:\CON\test.txt")]
        [InlineData(@"C:\test\*.txt")]
        [InlineData(@"C:\test\PRN")]
        public void Exists_WindowsInvalidPaths_ReturnsFalse(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.False(PathHelper.Exists(path));
            }
        }

        [Theory]
        [InlineData("/test/./")]
        [InlineData("/test/../")]
        public void Exists_UnixSpecialPaths_ReturnsFalse(string path)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.False(PathHelper.Exists(path));
            }
        }

        [Fact]
        public void Exists_InvalidCharacters_ReturnsFalse()
        {
            var invalidPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\test\invalid<>path"  // Windows invalid characters
                : "/test/invalid\0path";     // Unix null character

            Assert.False(PathHelper.Exists(invalidPath));
        }
    }

    public class PathEqualsTests : PathHelperTests
    {
        [Theory]
        [InlineData(null, null, true, true)]
        [InlineData(null, "path", true, false)]
        [InlineData("path", null, true, false)]
        public void PathEquals_NullPaths_HandlesCorrectly(string? path1, string? path2,
            bool ignoreCase, bool expected)
        {
            var result = PathHelper.PathEquals(path1, path2, ignoreCase);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void PathEquals_SamePath_DifferentCase_ReturnsExpected()
        {
            var path1 = Path.Combine(_basePath, "File.txt");
            var path2 = Path.Combine(_basePath, "file.txt");

            Assert.True(PathHelper.PathEquals(path1, path2, true));
            Assert.False(PathHelper.PathEquals(path1, path2, false));
        }
    }

    public class GetParentDirectoryTests : PathHelperTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void GetParentDirectory_ValidLevels_ReturnsExpectedPath(int levels)
        {
            var path = Path.Combine(_basePath, "level1", "level2", "level3");
            var result = PathHelper.GetParentDirectory(path, levels);

            var expected = path;
            for (int i = 0; i < levels; i++)
            {
                expected = Path.GetDirectoryName(expected);
            }

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetParentDirectory_RootPath_ReturnsSamePathForExcessiveLevels()
        {
            var rootPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\"
                : "/";
            var result = PathHelper.GetParentDirectory(rootPath, 5);
            Assert.Equal(rootPath, result);
        }
    }
}
