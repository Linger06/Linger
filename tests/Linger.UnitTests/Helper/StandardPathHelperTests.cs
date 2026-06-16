using System;
using System.IO;
using Xunit;

namespace Linger.UnitTests.Helper;

public class PathExtensionsTests
{
    [Fact]
    public void CleanAndNormalizePureString_ShouldStandardizePaths()
    {
        // 基本路径测试
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

        // 边界条件测试
        Assert.Equal("", PathHelper.CleanAndNormalizePureString(null, false));
        Assert.Equal(string.Empty, PathHelper.CleanAndNormalizePureString(string.Empty, false));
        Assert.Equal(string.Empty, PathHelper.CleanAndNormalizePureString("   ", false));
    }

    [Fact]
    public void CleanAndNormalizePureString_WithEndingSeparator_ShouldPreserveEnding()
    {
        // 测试保留末尾分隔符
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
        // 1. 准备数据
        string baseDir = OSPlatformHelper.IsWindows ? "C:\\base\\path" : "/base/path";
        string targetSameLevel = OSPlatformHelper.IsWindows ? "C:\\base\\other" : "/base/other";
        string targetSubDir = OSPlatformHelper.IsWindows ? "C:\\base\\path\\subdir" : "/base/path/subdir";
        string targetParentDir = OSPlatformHelper.IsWindows ? "C:\\base" : "/base";

        // 2. 期望结果
        string expectedSameLevel = OSPlatformHelper.IsWindows ? "..\\other" : "../other";
        string expectedSubDir = "subdir";
        string expectedParent = "..";

        // 3. 执行核心逻辑测试：扩展方法写法是 base.GetRelativePath(target)
        var relSameLevel = baseDir.GetRelativePath(targetSameLevel);
        var relSubDir = baseDir.GetRelativePath(targetSubDir);
        var relParent = baseDir.GetRelativePath(targetParentDir);

        // 4. 断言结果
        Assert.Equal(expectedSameLevel, relSameLevel);
        Assert.Equal(expectedSubDir, relSubDir);
        Assert.Equal(expectedParent, relParent);

        // 5. 相同路径边界
        Assert.Equal(".", baseDir.GetRelativePath(baseDir));

        // 6. 异常边界测试：当基准路径（主体）为空或空格时，应该抛出 ArgumentException
        Assert.Throws<ArgumentException>(() => "".GetRelativePath(targetSubDir));
        Assert.Throws<ArgumentException>(() => "   ".GetRelativePath(targetSubDir));

        // 7. 空路径返回值测试：当目标路径（参数）为空时，应该返回 string.Empty
        Assert.Equal(string.Empty, baseDir.GetRelativePath(""));
        Assert.Equal(string.Empty, baseDir.GetRelativePath(null!));
    }

    [Fact]
    public void ContainsInvalidPathChars_ShouldDetectInvalidCharacters()
    {
        // 创建包含Windows非法字符的路径
        string invalidWinChars = OSPlatformHelper.IsWindows ? "path*to?file" : null;

        // 创建包含系统非法字符的路径
        string invalidPath = $"path{Path.GetInvalidPathChars()[0]}file";
        string validPath = "path/to/file";

        // 测试非法字符检测
        Assert.True(PathExtensions.ContainsInvalidPathChars(invalidPath));
        Assert.False(PathExtensions.ContainsInvalidPathChars(validPath));

        // Windows特定测试
        if (OSPlatformHelper.IsWindows && invalidWinChars != null)
        {
            Assert.True(PathExtensions.ContainsInvalidPathChars(invalidWinChars));
        }

        // 检查Windows保留名
        if (OSPlatformHelper.IsWindows)
        {
            Assert.True(PathExtensions.ContainsInvalidPathChars("C:\\CON\\file.txt"));
            Assert.True(PathExtensions.ContainsInvalidPathChars("C:\\path\\NUL"));
            Assert.True(PathExtensions.ContainsInvalidPathChars("LPT1.txt"));
        }

        // null和空字符串测试
        Assert.False(PathExtensions.ContainsInvalidPathChars(null));
        Assert.False(PathExtensions.ContainsInvalidPathChars(string.Empty));
    }

    [Fact]
    public void GetParentDirectory_ShouldReturnCorrectParentPath()
    {
        // 创建测试路径
        string testPath = Path.Combine("dir1", "dir2", "dir3");
        var fullPath = Path.GetFullPath(testPath);

        // 获取一级父目录
        var parentDir = PathExtensions.GetParentDirectory(fullPath, 1);
        var expectedParent = Directory.GetParent(fullPath).FullName;
        Assert.Equal(expectedParent, parentDir);

        // 获取两级父目录
        var parentOfParent = PathExtensions.GetParentDirectory(fullPath, 2);
        var expectedGrandParent = Directory.GetParent(Directory.GetParent(fullPath).FullName).FullName;
        Assert.Equal(expectedGrandParent, parentOfParent);

        // 边缘情况测试
        var noChange = PathExtensions.GetParentDirectory(fullPath, 0);
        Assert.Equal(fullPath, noChange);

        // 测试负值级别（应该取绝对值）
        var negativeLevel = PathExtensions.GetParentDirectory(fullPath, -1);
        Assert.Equal(expectedParent, negativeLevel);
    }

    [Fact]
    public void Exists_ShouldDetectFileAndDirectoryExistence()
    {
        // 获取当前目录（肯定存在的目录）
        string currentDir = Directory.GetCurrentDirectory();

        // 测试目录存在检查
        Assert.True(PathExtensions.Exists(currentDir, false));

        // 创建一个临时文件用于测试
        string tempFile = Path.GetTempFileName();
        try
        {
            // 测试文件存在检查
            Assert.True(PathExtensions.Exists(tempFile, true));

            // 测试不存在的路径
            Assert.False(PathExtensions.Exists(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), true));
            Assert.False(PathExtensions.Exists(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), false));

            // 测试无效路径
            Assert.False(PathExtensions.Exists("||invalid||path||"));

            // 测试空路径
            Assert.False(PathExtensions.Exists(null));
            Assert.False(PathExtensions.Exists(""));
            Assert.False(PathExtensions.Exists("   "));
        }
        finally
        {
            // 清理临时文件
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void ToFullPath_ShouldResolveRelativePaths()
    {
        // 测试基础路径和相对路径
        string baseDir = Directory.GetCurrentDirectory();
        string relativePath = "subdir/file.txt";

        var result = PathExtensions.ToFullPath(relativePath,baseDir);
        var expected = Path.GetFullPath(Path.Combine(baseDir, relativePath));

        Assert.Equal(expected, result);

        // 测试绝对路径
        string absolutePath = Path.GetFullPath("file.txt");
        var absoluteResult = PathExtensions.ToFullPath(absolutePath,baseDir);
        Assert.Equal(absolutePath, absoluteResult);

        // 测试边缘情况
        Assert.Equal(string.Empty, PathExtensions.ToFullPath("",baseDir));
        Assert.Equal(string.Empty, PathExtensions.ToFullPath(null,baseDir));

        // 测试保留末尾分隔符
        var withSeparator = PathExtensions.ToFullPath("subdir/", baseDir, true);
        Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), withSeparator);
    }

    [Fact]
    public void ContainsInvalidPathChars_ShouldCheckNonWindowsPlatforms()
    {
        // 这个测试主要是为了覆盖非Windows平台的代码路径
        // 在Windows上这个测试可能不会执行到Unix分支，但仍然有助于理解代码逻辑

        // 测试包含null字符的路径（Unix系统中的非法字符）
        string pathWithNull = "path\0with\0null";

        // 在所有平台上，包含null字符的路径都应该被认为是非法的
        var result = PathExtensions.ContainsInvalidPathChars(pathWithNull);

        // 注意：在Windows上这会通过系统非法字符检查捕获，在Unix上通过null字符检查捕获
        Assert.True(result);
    }

    [Fact]
    public void GetParentDirectory_ShouldHandleRootPath()
    {
        // 测试根目录情况，Directory.GetParent可能返回null
        string rootPath;
        if (OSPlatformHelper.IsWindows)
        {
            rootPath = "C:\\";
        }
        else
        {
            rootPath = "/";
        }

        // 尝试获取根目录的父目录
        var result = PathExtensions.GetParentDirectory(rootPath, 1);

        // 如果无法获取父目录，应该返回原路径
        Assert.NotNull(result);
    }

    [Fact]
    public void CleanAndNormalizePureString_ShouldHandleNullAndWhitespace()
    {
        // 详细测试各种空值情况
        Assert.Equal(string.Empty, PathHelper.CleanAndNormalizePureString("", false));
        Assert.Equal(string.Empty, PathHelper.CleanAndNormalizePureString("   ", false));
        Assert.Equal(string.Empty, PathHelper.CleanAndNormalizePureString("\t\n", false));
        Assert.Equal(string.Empty, PathHelper.CleanAndNormalizePureString(null, false));
    }

    [Fact]
    public void Exists_ShouldHandleExceptions()
    {
        // 测试非常长的路径，可能导致PathTooLongException
        var veryLongPath = new string('a', 500);

        // 应该优雅处理异常，返回false而不是抛出异常
        var result = PathExtensions.Exists(veryLongPath);
        Assert.False(result);

        // 测试包含非法字符的路径
        var invalidPath = "path\0with\0null";
        var invalidResult = PathExtensions.Exists(invalidPath);
        Assert.False(invalidResult);
    }
}
