using System;
using System.IO;
using System.Reflection;
using Linger.Helper.PathHelpers;
using Xunit;

namespace Linger.UnitTests.Helper;

public class PathHelperBaseTests
{
    // 由于PathHelperBase是抽象类，我们将通过反射来测试其保护方法
    private static readonly Type s_pathHelperBaseType = typeof(PathHelperBase);
    
    [Fact]
    public void RemoveConsecutiveSeparators_ShouldRemoveDuplicateSeparators()
    {
        // 获取RemoveConsecutiveSeparators方法
        var method = s_pathHelperBaseType.GetMethod("RemoveConsecutiveSeparators", 
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        // 测试常见的输入
        var result1 = method.Invoke(null, new[] { "path//to//file" }) as string;
        var result2 = method.Invoke(null, new[] { "path/to/file" }) as string;
        var result3 = method.Invoke(null, new[] { "//path//to//file//" }) as string;
        var result4 = method.Invoke(null, new[] { string.Empty }) as string;
        var result5 = method.Invoke(null, new[] { (string)null }) as string;
        
        // 确认结果符合预期
        Assert.Equal("path/to/file", result1);
        Assert.Equal("path/to/file", result2);
        Assert.Equal("/path/to/file/", result3);
        Assert.Equal(string.Empty, result4);
        Assert.Equal(string.Empty, result5);
    }
    
    [Fact]
    public void HandleEndingSeparator_ShouldCorrectlyHandleEndingSeparator()
    {
        // 获取HandleEndingSeparator方法
        var method = s_pathHelperBaseType.GetMethod("HandleEndingSeparator", 
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        
        // 测试保留末尾分隔符
        var resultPreserve1 = method.Invoke(null, new object[] { "path/to/file", true, 0 }) as string;
        var resultPreserve2 = method.Invoke(null, new object[] { "path/to/file/", true, 0 }) as string;
        var resultPreserve3 = method.Invoke(null, new object[] { string.Empty, true, 0 }) as string;
        
        // 测试不保留末尾分隔符
        var resultNoPreserve1 = method.Invoke(null, new object[] { "path/to/file", false, 0 }) as string;
        var resultNoPreserve2 = method.Invoke(null, new object[] { "path/to/file/", false, 0 }) as string;
        var resultNoPreserve3 = method.Invoke(null, new object[] { string.Empty, false, 0 }) as string;
        
        // 确认结果符合预期
        var separator = Path.DirectorySeparatorChar.ToString();
        Assert.Equal($"path/to/file{separator}", resultPreserve1);
        Assert.Equal($"path/to/file{separator}", resultPreserve2);
        Assert.Equal(string.Empty, resultPreserve3);
        
        Assert.Equal("path/to/file", resultNoPreserve1);
        Assert.Equal("path/to/file", resultNoPreserve2);
        Assert.Equal(string.Empty, resultNoPreserve3);
    }
    
    [Fact]
    public void SplitPath_ShouldSplitPathIntoSegments()
    {
        // 获取SplitPath方法
        var method = s_pathHelperBaseType.GetMethod("SplitPath", 
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        
        // 测试不同的路径
        var result1 = method.Invoke(null, new[] { "path/to/file" }) as string[];
        var result2 = method.Invoke(null, new[] { "/path/to/file" }) as string[];
        var result3 = method.Invoke(null, new[] { "path//to///file" }) as string[];
        var result4 = method.Invoke(null, new[] { string.Empty }) as string[];
        
        // 确认结果符合预期
        Assert.Equal(new[] { "path", "to", "file" }, result1);
        Assert.Equal(new[] { "path", "to", "file" }, result2);
        Assert.Equal(new[] { "path", "to", "file" }, result3);
        Assert.Empty(result4);
    }
    
    [Fact]
    public void StandardizePathSeparators_ShouldUseCorrectSeparator()
    {
        // 获取StandardizePathSeparators方法
        var method = s_pathHelperBaseType.GetMethod("StandardizePathSeparators", 
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        
        // 测试混合分隔符的路径
        var path = "path/to\\file/example\\test";
        var result = method.Invoke(null, new[] { path }) as string;
        var expected = OSPlatformHelper.IsWindows 
            ? "path\\to\\file\\example\\test" 
            : "path/to/file/example/test";
        
        // 确认结果符合预期
        Assert.Equal(expected, result);
        
        // 测试空字符串
        var emptyResult = method.Invoke(null, new[] { string.Empty }) as string;
        Assert.Equal(string.Empty, emptyResult);
        
        // 测试null
        var nullResult = method.Invoke(null, new[] { (string)null }) as string;
        Assert.Equal(string.Empty, nullResult);
    }
    
    [Fact]
    public void NormalizeBasicPath_ShouldStandardizePath()
    {
        // 获取NormalizeBasicPath方法
        var method = s_pathHelperBaseType.GetMethod("NormalizeBasicPath", 
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        
        // 测试基础路径标准化
        var path1 = "path//to\\\\file";
        var result1 = method.Invoke(null, new object[] { path1, false }) as string;
        var result2 = method.Invoke(null, new object[] { path1, true }) as string;
        
        // 确认结果符合预期，标准化后应该没有连续分隔符
        Assert.DoesNotContain("//", result1);
        Assert.DoesNotContain("\\\\", result1);
        Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), result2);
        
        // 测试无效路径输入
        var invalidPath = new string('a', 300) + new string('\\', 10) + new string('b', 300);
        var resultInvalid = method.Invoke(null, new object[] { invalidPath, false }) as string;
        Assert.NotNull(resultInvalid); // 应该返回某个值，而不是抛出异常
    }
    
    [Fact]
    public void ContainsInvalidPathChars_ShouldDetectInvalidChars()
    {
        // 获取ContainsInvalidPathChars方法
        var method = s_pathHelperBaseType.GetMethod("ContainsInvalidPathChars", 
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        
        // 测试包含系统定义的无效字符的路径
        var invalidPath = $"path{Path.GetInvalidPathChars()[0]}file";
        var validPath = "path/to/file";
        
        var resultInvalid = (bool)method.Invoke(null, new[] { invalidPath });
        var resultValid = (bool)method.Invoke(null, new[] { validPath });
        var resultEmpty = (bool)method.Invoke(null, new[] { string.Empty });
        var resultNull = (bool)method.Invoke(null, new[] { (string)null });
        
        // 确认结果符合预期
        Assert.True(resultInvalid);
        Assert.False(resultValid);
        Assert.False(resultEmpty);
        Assert.False(resultNull);
    }
}