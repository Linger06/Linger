namespace Linger.UnitTests.Extensions.IO;

public class PathExtensionsTests
{
    [Fact]
    public void GetRelativePath_ShouldReturnRelativePath_NETCOREAPP()
    {
#if NETCOREAPP
        Assert.SkipUnless(OSPlatformHelper.IsWindows, "仅在 Windows 平台运行");
        var path = @"C:\Projects\Test";
        var relativeTo = @"C:\Projects";
        var expected = "Test";

        var result = path.GetRelativePath(relativeTo);

        Assert.Equal(expected, result);
#endif
    }

    [Fact]
    public void GetRelativePath_ShouldReturnRelativePath_NETFRAMEWORK_NETSTANDARD()
    {
#if NETFRAMEWORK || NETSTANDARD
        Assert.SkipUnless(OSPlatformHelper.IsWindows, "仅在 Windows 平台运行");
        var path = @"C:\Projects\Test";
        var relativeTo = @"C:\Projects";
        var expected = "Test";

        var result = path.GetRelativePath(relativeTo);

        Assert.Equal(expected, result);
#endif
    }

    [Fact]
    public void GetAbsolutePath_ShouldReturnAbsolutePath_WhenPathIsRelative()
    {
        Assert.SkipUnless(OSPlatformHelper.IsWindows, "仅在 Windows 平台运行");
        var path = @"..\Test";
        var basePath = @"C:\Projects";
        var expected = Path.GetFullPath(Path.Combine(basePath, path));

        var result = path.GetAbsolutePath(basePath);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAbsolutePath_ShouldReturnAbsolutePath_WhenPathIsAbsolute()
    {
        Assert.SkipUnless(OSPlatformHelper.IsWindows, "仅在 Windows 平台运行");
        var path = @"C:\Test";
        var expected = Path.GetFullPath(path);

        var result = path.GetAbsolutePath();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAbsolutePath_ShouldReturnExpected_WhenBasePahtIsNotAbsolute()
    {
        var path = @"..\Test";
        var basePath = @"..\Projects";
        Assert.Throws<ArgumentException>(() => path.GetAbsolutePath(basePath));
    }

    [Fact]
    public void GetAbsolutePath_ShouldReturnExpected_WhenPahtIsNull()
    {
        string? path = null;
        var basePath = @"..\Projects";
        Assert.Throws<System.ArgumentNullException>(() => path.GetAbsolutePath(basePath));
    }

    [Fact]
    public void IsAbsolutePath_ShouldReturnTrue_WhenPathIsAbsolute()
    {
        Assert.SkipUnless(OSPlatformHelper.IsWindows, "仅在 Windows 平台运行");
        var path = @"C:\Test";

        var result = path.IsAbsolutePath();

        Assert.True(result);
    }

    [Fact]
    public void IsAbsolutePath_ShouldReturnFalse_WhenPathIsRelative()
    {
        var path = @"..\Test";

        var result = path.IsAbsolutePath();

        Assert.False(result);
    }

    [Fact]
    public void RelativeTo_ShouldReturnRelativePath()
    {
        Assert.SkipUnless(OSPlatformHelper.IsWindows, "仅在 Windows 平台运行");
        var sourcePath = @"C:\Projects\Test\File.txt";
        var folder = @"C:\Projects";
        var expected = @"Test\File.txt";

        var result = sourcePath.RelativeTo(folder);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void RelativeTo_ShouldReturnRelativePath_WhenFolderIsNotAbsolute()
    {
        var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Test", "File.txt");
        var folder = "/";
        var expected = Path.Combine("Test", "File.txt");

        var result = sourcePath.RelativeTo(folder);

        Assert.Equal(expected, result);
    }
}