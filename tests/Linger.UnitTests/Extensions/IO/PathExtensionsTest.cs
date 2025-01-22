namespace Linger.UnitTests.Extensions.IO;

public class PathExtensionsTest()
{
    public static IEnumerable<object[]> PathData = new List<object[]>
    {
        new object[] { "./Test", "", "Test" },
        new object[] { @".\Test", "", "Test" },
        new object[] { "../Test", "", @"..\Test" },
        new object[] { @"..\Test", "", @"..\Test" }
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


    public static IEnumerable<object[]> PathDataWindows = new List<object[]>
    {
        new object[] { @"C:\Path\Test", @"C:\Path", "Test" },
        new object[] { @"D:\Path\Test", @"D:\Path2\Test\Test", @"..\..\..\Path\Test" },
        new object[] { @"D:\Path\Test\a.txt", @"D:\Path", @"Test\a.txt" }
    };

    [Theory]
    [MemberData(nameof(PathDataWindows))]
    public void GetRelativePathTest_Windows(string path1, string path2, string path3)
    {
        Assert.SkipUnless(OSPlatformHelper.IsWindows, "仅在 Windows 平台运行");
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
        new object[] { "Test", null, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test") },
        new object[] { "Test", "", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test") },
        new object[] { @".\Test", "", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test") },
        new object[] { "./Test", "", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test") },
        new object[] { "../Test", "", Path.Combine(Path.GetFullPath(".."), "Test") },
        new object[] { @"..\Test", "", Path.Combine(Path.GetFullPath(".."), "Test") }
    };

    [Theory]
    [MemberData(nameof(PathData2))]
    public void GetAbsolutePathTest(string value, string? basePath, string expectedPath)
    {
        if (basePath.IsNotNullAndEmpty())
        {
            var result = value.GetAbsolutePath(basePath);
            Assert.True(PathHelper.PathEquals(result, expectedPath));
        }
        else
        {
            var result = value.GetAbsolutePath();
            Assert.True(PathHelper.PathEquals(result, expectedPath));
        }
    }

    public static IEnumerable<object[]> PathData2_Windows = new List<object[]>
    {
        new object[] { @"C:\Path\Test", "", @"C:\Path\Test" },
        new object[] { "../Test", @"C:\Path\Test", @"C:\Path\Test" },
        new object[] { "../Test", @"C:\Path\Test\XXX", @"C:\Path\Test\Test" },
        new object[] { "../", @"C:\Path\Test\XXX", @"C:\Path\Test" },
        new object[] { "/", @"XXXX", @"C:\" },
        new object[] { "/", "", @"C:\" },
        new object[] { "", @"C:\Path", @"C:\Path" },
    };

    [Theory]
    [MemberData(nameof(PathData2_Windows))]
    public void GetAbsolutePathTest_Windows(string value, string? basePath, string expectedPath)
    {
        Assert.SkipUnless(OSPlatformHelper.IsWindows, "仅在 Windows 平台运行");
        if (basePath.IsNotNullAndEmpty())
        {
            var result = value.GetAbsolutePath(basePath);
            Assert.True(PathHelper.PathEquals(result, expectedPath));
        }
        else
        {
            var result = value.GetAbsolutePath();
            Assert.True(PathHelper.PathEquals(result, expectedPath));
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
}