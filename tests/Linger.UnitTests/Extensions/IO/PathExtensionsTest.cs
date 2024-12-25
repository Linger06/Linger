using System.Runtime.InteropServices;

namespace Linger.UnitTests.Extensions.IO;

public class PathExtensionsTest()
{
    public static IEnumerable<object[]> PathData = new List<object[]>
    {
        new object[] { "./Test", "", "Test" },
        new object[] { @".\Test", "", "Test" },
        new object[] { "../Test", "", @"..\Test" },
        new object[] { @"..\Test", "", @"..\Test" },
        new object[] { @"C:\Path\Test", @"C:\Path", "Test" },
        new object[] { @"D:\Path\Test", @"D:\Path2\Test\Test", @"..\..\..\Path\Test" },
        new object[] { @"D:\Path\Test\a.txt", @"D:\Path", @"Test\a.txt" }
    };

    [Theory]
    [MemberData(nameof(PathData))]
    public void GetRelativePathTest(string path1, string path2, string path3)
    {
        if (path2.IsNotNullAndEmpty())
        {
            var result = path1.GetRelativePath(path2);
            Assert.Equal(result, path3);
        }
        else
        {
            var result = path1.GetRelativePath();
            Assert.Equal(result, path3);
        }
    }

    public static IEnumerable<object[]> PathData2 = new List<object[]>
    {
        new object[] { "Test", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test"),null },
        new object[] { "Test", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test"),"" },
        new object[] { @".\Test", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test") ,""},
        new object[] { "./Test", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test") ,""},
        new object[] { "../Test", Path.Combine(Path.GetFullPath(".."), "Test") ,""},
        new object[] { @"..\Test", Path.Combine(Path.GetFullPath(".."), "Test") ,""},
        new object[] { @"C:\Path\Test", @"C:\Path\Test" ,""},
        new object[] { "../Test", @"C:\Path\Test",@"C:\Path\Test" },
        new object[] { "../Test", @"C:\Path\Test\Test",@"C:\Path\Test\XXX" },
        new object[] { "../", @"C:\Path\Test",@"C:\Path\Test\XXX" },
        new object[] { "/", @"C:\Path\Test\Test",@"C:\Path\Test\XXX" },
        new object[] { "", @"C:\Path",@"C:\Path" },
    };

    [Theory]
    [MemberData(nameof(PathData2))]
    public void GetAbsolutePathTest(string value, string expectedPath, string? basePath)
    {
        if (basePath.IsNotNullAndEmpty())
        {
            var result = value.GetAbsolutePath(basePath);
            Assert.Equal(result.ToLower(), expectedPath.ToLower());
        }
        else
        {
            var result = value.GetAbsolutePath();
            Assert.Equal(result.ToLower(), expectedPath.ToLower());
        }
    }

#if NETSTANDARD2_1_OR_GREATER
        [Theory]
        [InlineData("Test")]
        [InlineData(@".\Test")]
        [InlineData(@"./Test")]
        [InlineData(@"../Test")]
        [InlineData(@"..\Test")]
        public void TestRelativePath(string path)
        {
            var result = path.GetAbsolutePath();
            var fakeRoot = Environment.CurrentDirectory;
            var relativePath = Path.GetRelativePath(fakeRoot, result);
            Assert.Equal(path.GetRelativePath(fakeRoot), relativePath);
        }
#endif    

    private static string GetFullPath(string path)
    {
        string rv;
        if (path.StartsWith("."))
        {
            rv = Path.Combine(string.Empty, path);
        }
        else
        {
            rv = path;
        }

        rv = Path.GetFullPath(rv);
        return rv;
    }

    public static string GetRuntimeDirectory(string path)
    {
        if (IsLinuxRunTime())
        {
            return GetLinuxDirectory(path);
        }

        if (IsWindowRunTime())
        {
            return GetWindowDirectory(path);
        }

        return path;
    }

    public static bool IsWindowRunTime()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    public static bool IsLinuxRunTime()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    public static string GetLinuxDirectory(string path)
    {
        var pathTemp = Path.Combine(path);
        return pathTemp.Replace("\\", "/");
    }

    public static string GetWindowDirectory(string path)
    {
        var pathTemp = Path.Combine(path);
        return pathTemp.Replace("/", "\\");
    }
}