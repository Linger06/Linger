using System;
using System.IO;
using Linger.Helper.PathHelpers;
using Xunit;

namespace Linger.UnitTests.Helper;

public class StandardPathHelperTests
{
    [Fact]
    public void NormalizePath_ShouldStandardizePaths()
    {
        // 基本路径测试
        string path1 = "path/to/file";
        string path2 = "path\\to\\file";
        string path3 = "path//to\\\\file";

        string result1 = StandardPathHelper.NormalizePath(path1);
        string result2 = StandardPathHelper.NormalizePath(path2);
        string result3 = StandardPathHelper.NormalizePath(path3);

        string expectedSeparator = OSPlatformHelper.IsWindows ? "\\" : "/";
        string expected = $"path{expectedSeparator}to{expectedSeparator}file";

        Assert.Equal(expected, result1);
        Assert.Equal(expected, result2);
        Assert.Equal(expected, result3);

        // 边界条件测试
        Assert.Equal(null, StandardPathHelper.NormalizePath(null));
        Assert.Equal(string.Empty, StandardPathHelper.NormalizePath(string.Empty));
        Assert.Equal(string.Empty, StandardPathHelper.NormalizePath("   "));
    }

    [Fact]
    public void NormalizePath_WithEndingSeparator_ShouldPreserveEnding()
    {
        // 测试保留末尾分隔符
        string path1 = "path/to/folder";
        string path2 = "path/to/folder/";

        string result1 = StandardPathHelper.NormalizePath(path1, true);
        string result2 = StandardPathHelper.NormalizePath(path2, true);

        string expectedSeparator = OSPlatformHelper.IsWindows ? "\\" : "/";

        Assert.Equal($"path{expectedSeparator}to{expectedSeparator}folder{expectedSeparator}", result1);
        Assert.Equal($"path{expectedSeparator}to{expectedSeparator}folder{expectedSeparator}", result2);
    }

    [Fact]
    public void PathEquals_ShouldComparePathsCorrectly()
    {
        // 基本相等测试
        Assert.True(StandardPathHelper.PathEquals("path/to/file", "path\\to\\file"));
        Assert.True(StandardPathHelper.PathEquals("path/to/file/", "path\\to\\file"));

        // 大小写测试
        Assert.True(StandardPathHelper.PathEquals("Path/To/File", "path\\to\\file"));
        Assert.False(StandardPathHelper.PathEquals("Path/To/File", "path\\to\\file", false));

        // null和空字符串测试
        Assert.True(StandardPathHelper.PathEquals(null, null));
        Assert.False(StandardPathHelper.PathEquals(null, ""));
        Assert.False(StandardPathHelper.PathEquals("path", null));

        // 路径不同测试
        Assert.False(StandardPathHelper.PathEquals("path1", "path2"));
    }

    [Fact]
    public void IsWindowsDriveLetter_ShouldIdentifyDriveLettersCorrectly()
    {
        // 有效的Windows盘符
        Assert.True(StandardPathHelper.IsWindowsDriveLetter("C:"));
        Assert.True(StandardPathHelper.IsWindowsDriveLetter("Z:"));
        Assert.True(StandardPathHelper.IsWindowsDriveLetter("C:\\"));
        Assert.True(StandardPathHelper.IsWindowsDriveLetter("D:/"));
        Assert.True(StandardPathHelper.IsWindowsDriveLetter("E:\\path"));

        // 无效的Windows盘符
        Assert.False(StandardPathHelper.IsWindowsDriveLetter(null));
        Assert.False(StandardPathHelper.IsWindowsDriveLetter(""));
        Assert.False(StandardPathHelper.IsWindowsDriveLetter("C"));
        Assert.False(StandardPathHelper.IsWindowsDriveLetter(":C"));
        Assert.False(StandardPathHelper.IsWindowsDriveLetter("1:"));
        Assert.False(StandardPathHelper.IsWindowsDriveLetter("C:*")); // 非法字符
    }

    [Fact]
    public void GetRelativePath_ShouldCalculateRelativePaths()
    {
        // 获取基准路径和目标路径
        string baseDir = OSPlatformHelper.IsWindows ? "C:\\base\\path" : "/base/path";
        string targetSameLevel = OSPlatformHelper.IsWindows ? "C:\\base\\other" : "/base/other";
        string targetSubDir = OSPlatformHelper.IsWindows ? "C:\\base\\path\\subdir" : "/base/path/subdir";
        string targetParentDir = OSPlatformHelper.IsWindows ? "C:\\base" : "/base";
        string targetDifferentRoot = OSPlatformHelper.IsWindows ? "D:\\other\\path" : "/other/path";

        // 测试从basePath到targets的相对路径
#if NETCOREAPP
        // .NET Core环境中使用Path.GetRelativePath直接测试
        var relSameLevel = StandardPathHelper.GetRelativePath(baseDir, targetSameLevel);
        var relSubDir = StandardPathHelper.GetRelativePath(baseDir, targetSubDir);
        var relParent = StandardPathHelper.GetRelativePath(baseDir, targetParentDir);

        // 根据平台期望的结果
        string expectedSameLevel = OSPlatformHelper.IsWindows ? "..\\other" : "../other";
        string expectedSubDir = "subdir";
        string expectedParent = "..";

        Assert.Equal(expectedSameLevel, relSameLevel);
        Assert.Equal(expectedSubDir, relSubDir);
        Assert.Equal(expectedParent, relParent);
#else
        // 如果不在.NET Core环境，还是验证函数不抛异常且返回非空结果
        Assert.NotNull(StandardPathHelper.GetRelativePath(baseDir, targetSameLevel));
        Assert.NotNull(StandardPathHelper.GetRelativePath(baseDir, targetSubDir));
        Assert.NotNull(StandardPathHelper.GetRelativePath(baseDir, targetParentDir));
#endif

        // 边缘情况测试
        Assert.Equal(".", StandardPathHelper.GetRelativePath(baseDir, baseDir));
        Assert.Throws<ArgumentException>(() => StandardPathHelper.GetRelativePath("", targetSubDir));
        Assert.Equal(string.Empty, StandardPathHelper.GetRelativePath(baseDir, ""));
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
        Assert.True(StandardPathHelper.ContainsInvalidPathChars(invalidPath));
        Assert.False(StandardPathHelper.ContainsInvalidPathChars(validPath));

        // Windows特定测试
        if (OSPlatformHelper.IsWindows && invalidWinChars != null)
        {
            Assert.True(StandardPathHelper.ContainsInvalidPathChars(invalidWinChars));
        }

        // 检查Windows保留名
        if (OSPlatformHelper.IsWindows)
        {
            Assert.True(StandardPathHelper.ContainsInvalidPathChars("C:\\CON\\file.txt"));
            Assert.True(StandardPathHelper.ContainsInvalidPathChars("C:\\path\\NUL"));
            Assert.True(StandardPathHelper.ContainsInvalidPathChars("LPT1.txt"));
        }

        // null和空字符串测试
        Assert.False(StandardPathHelper.ContainsInvalidPathChars(null));
        Assert.False(StandardPathHelper.ContainsInvalidPathChars(string.Empty));
    }

    [Fact]
    public void GetParentDirectory_ShouldReturnCorrectParentPath()
    {
        // 创建测试路径
        string testPath = Path.Combine("dir1", "dir2", "dir3");
        var fullPath = Path.GetFullPath(testPath);

        // 获取一级父目录
        var parentDir = StandardPathHelper.GetParentDirectory(fullPath, 1);
        var expectedParent = Directory.GetParent(fullPath).FullName;
        Assert.Equal(expectedParent, parentDir);

        // 获取两级父目录
        var parentOfParent = StandardPathHelper.GetParentDirectory(fullPath, 2);
        var expectedGrandParent = Directory.GetParent(Directory.GetParent(fullPath).FullName).FullName;
        Assert.Equal(expectedGrandParent, parentOfParent);

        // 边缘情况测试
        var noChange = StandardPathHelper.GetParentDirectory(fullPath, 0);
        Assert.Equal(fullPath, noChange);

        // 测试负值级别（应该取绝对值）
        var negativeLevel = StandardPathHelper.GetParentDirectory(fullPath, -1);
        Assert.Equal(expectedParent, negativeLevel);
    }

    [Fact]
    public void Exists_ShouldDetectFileAndDirectoryExistence()
    {
        // 获取当前目录（肯定存在的目录）
        string currentDir = Directory.GetCurrentDirectory();

        // 测试目录存在检查
        Assert.True(StandardPathHelper.Exists(currentDir, false));

        // 创建一个临时文件用于测试
        string tempFile = Path.GetTempFileName();
        try
        {
            // 测试文件存在检查
            Assert.True(StandardPathHelper.Exists(tempFile, true));

            // 测试不存在的路径
            Assert.False(StandardPathHelper.Exists(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), true));
            Assert.False(StandardPathHelper.Exists(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), false));

            // 测试无效路径
            Assert.False(StandardPathHelper.Exists("||invalid||path||"));

            // 测试空路径
            Assert.False(StandardPathHelper.Exists(null));
            Assert.False(StandardPathHelper.Exists(""));
            Assert.False(StandardPathHelper.Exists("   "));
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
    public void ResolveToAbsolutePath_ShouldResolveRelativePaths()
    {
        // 测试基础路径和相对路径
        string baseDir = Directory.GetCurrentDirectory();
        string relativePath = "subdir/file.txt";

        var result = StandardPathHelper.ResolveToAbsolutePath(baseDir, relativePath);
        var expected = Path.GetFullPath(Path.Combine(baseDir, relativePath));

        Assert.Equal(expected, result);

        // 测试绝对路径
        string absolutePath = Path.GetFullPath("file.txt");
        var absoluteResult = StandardPathHelper.ResolveToAbsolutePath(baseDir, absolutePath);
        Assert.Equal(absolutePath, absoluteResult);

        // 测试边缘情况
        Assert.Equal(string.Empty, StandardPathHelper.ResolveToAbsolutePath(baseDir, ""));
        Assert.Equal(string.Empty, StandardPathHelper.ResolveToAbsolutePath(baseDir, null));

        // 测试保留末尾分隔符
        var withSeparator = StandardPathHelper.ResolveToAbsolutePath(baseDir, "subdir/", true);
        Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), withSeparator);
    }

    [Fact]
    public void PathEquals_ShouldHandlePathExceptions()
    {
        // 测试包含非法字符的路径，这可能导致Path.GetFullPath异常
        string invalidPath1 = "C:\\" + new string('\0', 1) + "invalid"; // 包含空字符
        string invalidPath2 = "normal/path";

        // 应该能够处理异常情况，返回false而不是抛出异常
        var result = StandardPathHelper.PathEquals(invalidPath1, invalidPath2);
        Assert.False(result);

        // 测试极长路径
        var longPath = new string('a', 300);
        var normalPath = "short/path";
        var longPathResult = StandardPathHelper.PathEquals(longPath, normalPath);
        Assert.False(longPathResult);
    }

    [Fact]
    public void GetRelativePath_ShouldHandlePathExceptions()
    {
        string validPath = Directory.GetCurrentDirectory();
        
        // 测试null path参数返回空字符串情况
        var nullPathResult = StandardPathHelper.GetRelativePath(validPath, null);
        Assert.Equal(string.Empty, nullPathResult);

        // 测试包含非法字符的路径
        string invalidPath = "invalid\0path";
        
        // 应该抛出ArgumentException而不是其他异常
        Assert.Throws<ArgumentException>(() => StandardPathHelper.GetRelativePath(validPath, invalidPath));
    }

    [Fact]
    public void IsWindowsDriveLetter_ShouldHandleInvalidPathChars()
    {
        // 测试包含非法字符但长度大于3的情况
        string pathWithInvalidChars = "C:\\inv*lid";
        
        // 应该检测到非法字符并返回false
        var result = StandardPathHelper.IsWindowsDriveLetter(pathWithInvalidChars);
        Assert.False(result);

        // 测试长度为3且第三个字符不是分隔符的情况  
        var invalidDrive = "C:x";
        Assert.False(StandardPathHelper.IsWindowsDriveLetter(invalidDrive));

        // 测试有效的较长路径
        var validLongPath = "D:\\ValidPath\\SubDir";
        Assert.True(StandardPathHelper.IsWindowsDriveLetter(validLongPath));
    }

    [Fact] 
    public void ContainsInvalidPathChars_ShouldCheckNonWindowsPlatforms()
    {
        // 这个测试主要是为了覆盖非Windows平台的代码路径
        // 在Windows上这个测试可能不会执行到Unix分支，但仍然有助于理解代码逻辑
        
        // 测试包含null字符的路径（Unix系统中的非法字符）
        string pathWithNull = "path\0with\0null";
        
        // 在所有平台上，包含null字符的路径都应该被认为是非法的
        var result = StandardPathHelper.ContainsInvalidPathChars(pathWithNull);
        
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
        var result = StandardPathHelper.GetParentDirectory(rootPath, 1);
        
        // 如果无法获取父目录，应该返回原路径
        Assert.NotNull(result);
    }

    [Fact] 
    public void GetRelativePathCore_ShouldHandleEmptyResult()
    {
        // 这个测试针对GetRelativePathCore方法中result.Count为0的情况
        string basePath = Directory.GetCurrentDirectory();
        string samePath = basePath;
        
        var result = StandardPathHelper.GetRelativePath(basePath, samePath);
        
        // 相同路径应该返回"."
        Assert.Equal(".", result);
    }

    [Fact]
    public void NormalizePath_ShouldHandleNullAndWhitespace()
    {
        // 详细测试各种空值情况
        Assert.Equal(string.Empty, StandardPathHelper.NormalizePath(""));
        Assert.Equal(string.Empty, StandardPathHelper.NormalizePath("   "));
        Assert.Equal(string.Empty, StandardPathHelper.NormalizePath("\t\n"));
        Assert.Null(StandardPathHelper.NormalizePath(null));
    }

    [Fact]
    public void Exists_ShouldHandleExceptions()
    {
        // 测试非常长的路径，可能导致PathTooLongException
        var veryLongPath = new string('a', 500);
        
        // 应该优雅处理异常，返回false而不是抛出异常
        var result = StandardPathHelper.Exists(veryLongPath);
        Assert.False(result);

        // 测试包含非法字符的路径
        var invalidPath = "path\0with\0null";
        var invalidResult = StandardPathHelper.Exists(invalidPath);
        Assert.False(invalidResult);
    }
}