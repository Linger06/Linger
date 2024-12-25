namespace Linger.UnitTests.Extensions.IO;

public class PathExtensionsTests
{
    [Fact]
    public void GetRelativePath_ShouldReturnRelativePath_NETCOREAPP()
    {
#if NETCOREAPP
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
        var path = @"..\Test";
        var basePath = @"C:\Projects";
        var expected = Path.GetFullPath(Path.Combine(basePath, path));

        var result = path.GetAbsolutePath(basePath);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAbsolutePath_ShouldReturnAbsolutePath_WhenPathIsAbsolute()
    {
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
        Assert.Throws<ArgumentNullException>(() => path.GetAbsolutePath(basePath));
    }

    [Fact]
    public void IsAbsolutePath_ShouldReturnTrue_WhenPathIsAbsolute()
    {
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
        var expected = @"Test\File.txt";

        var result = sourcePath.RelativeTo(folder);

        Assert.Equal(expected, result);
    }
}