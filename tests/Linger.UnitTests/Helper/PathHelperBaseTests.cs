using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace Linger.UnitTests.Helper;

public class PathHelperBaseTests
{
    // PathHelper exposes these helpers internally, so reflection is used here to keep the tests focused.
    private static readonly Type s_pathHelperBaseType = typeof(PathHelper);

    [Fact]
    public void RemoveConsecutiveSeparators_ShouldRemoveDuplicateSeparators()
    {
        var method = s_pathHelperBaseType.GetMethod(
            "RemoveConsecutiveSeparators",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var separator = Path.DirectorySeparatorChar.ToString();
        var result1 = method.Invoke(null, new[] { "path//to//file" }) as string;
        var result2 = method.Invoke(null, new[] { "path/to/file" }) as string;
        var result3 = method.Invoke(null, new[] { "//path//to//file//" }) as string;
        var result4 = method.Invoke(null, new[] { string.Empty }) as string;
        var result5 = method.Invoke(null, new[] { (string)null }) as string;

        Assert.Equal($"path{separator}to{separator}file", result1);
        Assert.Equal("path/to/file", result2);
        Assert.Equal($"{separator}{separator}path{separator}to{separator}file{separator}", result3);
        Assert.Equal(string.Empty, result4);
        Assert.Equal(string.Empty, result5);
    }

    [Fact]
    public void HandleEndingSeparator_ShouldCorrectlyHandleEndingSeparator()
    {
        var method = s_pathHelperBaseType.GetMethod(
            "HandleEndingSeparator",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var resultPreserve1 = method.Invoke(null, new object[] { "path/to/file", true, 0 }) as string;
        var resultPreserve2 = method.Invoke(null, new object[] { "path/to/file/", true, 0 }) as string;
        var resultPreserve3 = method.Invoke(null, new object[] { string.Empty, true, 0 }) as string;

        var resultNoPreserve1 = method.Invoke(null, new object[] { "path/to/file", false, 0 }) as string;
        var resultNoPreserve2 = method.Invoke(null, new object[] { "path/to/file/", false, 0 }) as string;
        var resultNoPreserve3 = method.Invoke(null, new object[] { string.Empty, false, 0 }) as string;

        var separator = Path.DirectorySeparatorChar.ToString();
        Assert.Equal($"path/to/file{separator}", resultPreserve1);
        Assert.Equal($"path/to/file{separator}", resultPreserve2);
        Assert.Equal(string.Empty, resultPreserve3);

        Assert.Equal("path/to/file", resultNoPreserve1);
        Assert.Equal("path/to/file", resultNoPreserve2);
        Assert.Equal(string.Empty, resultNoPreserve3);
    }

    [Fact]
    public void HandleEndingSeparator_ShouldPreserveWindowsRootSemantics()
    {
        if (!OSPlatformHelper.IsWindows)
        {
            return;
        }

        var method = s_pathHelperBaseType.GetMethod(
            "HandleEndingSeparator",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var uncRoot = method.Invoke(null, new object[] { @"\\server\share\", false, 0 }) as string;
        var deviceRoot = method.Invoke(null, new object[] { @"\\?\C:\", false, 0 }) as string;

        Assert.Equal(Path.GetPathRoot(@"\\server\share\"), uncRoot);
        Assert.Equal(@"\\?\C:\", deviceRoot);
    }

    [Fact]
    public void StandardizePathSeparators_ShouldUseCorrectSeparator()
    {
        var method = s_pathHelperBaseType.GetMethod(
            "StandardizePathSeparators",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var path = "path/to\\file/example\\test";
        var result = method.Invoke(null, new[] { path }) as string;
        var expected = OSPlatformHelper.IsWindows
            ? "path\\to\\file\\example\\test"
            : "path/to/file/example/test";

        Assert.Equal(expected, result);

        var emptyResult = method.Invoke(null, new[] { string.Empty }) as string;
        Assert.Equal(string.Empty, emptyResult);

        var nullResult = method.Invoke(null, new[] { (string)null }) as string;
        Assert.Equal(string.Empty, nullResult);
    }

}
