using System.Runtime.InteropServices;

namespace Linger.UnitTests.Helper;

public class PathHelperTests : IDisposable
{
    private readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private readonly bool _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    private readonly bool _isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private readonly string _testDir;
    private readonly string _testFile;
    private readonly ITestOutputHelper _output;

    public PathHelperTests(ITestOutputHelper output)
    {
        _output = output;
        _testDir = Path.Combine(Path.GetTempPath(), "PathHelperTests");
        _testFile = Path.Combine(_testDir, "test.txt");

        Directory.CreateDirectory(_testDir);
        if (!File.Exists(_testFile))
            File.WriteAllText(_testFile, "test content");

        _output.WriteLine($"运行平台: {GetPlatformName()}");
    }

    private string GetPlatformName() =>
        _isWindows ? "Windows" : _isLinux ? "Linux" : _isMacOS ? "macOS" : "Unknown";

    [SkippableTheory]
    [InlineData("C:\\example\\path", "C:\\example\\path", true)]
    [InlineData("file:///C:/folder/file.txt", "C:\\folder\\file.txt", true)]
    [InlineData("/home/user/path", "/home/user/path", false)]
    public void NormalizePath_PlatformSpecific(string input, string expected, bool windowsOnly)
    {
        Skip.If(windowsOnly && !_isWindows, "仅在 Windows 平台运行");
        Skip.If(!windowsOnly && _isWindows, "仅在类 Unix 平台运行");

        var result = PathHelper.NormalizePath(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("//server/share/path", "//server/share/path")]
    [InlineData("//server/share/path/", "//server/share/path")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void NormalizePath_CrossPlatform(string input, string expected)
    {
        var result = PathHelper.NormalizePath(input);
        Assert.Equal(expected, result);
    }

    [SkippableFact]
    public void IsFile_Windows()
    {
        Skip.IfNot(_isWindows, "仅在 Windows 平台运行");

        var winFile = Path.Combine(_testDir, "windows.txt");

        Assert.True(PathHelper.IsFile(winFile));
        Assert.True(PathHelper.IsFile(winFile.Replace('\\', '/')));

        File.WriteAllText(winFile, "windows");

        Assert.True(PathHelper.IsFile(winFile));
        Assert.True(PathHelper.IsFile(winFile.Replace('\\', '/')));
    }

    [SkippableFact]
    public void IsFile_Unix()
    {
        Skip.If(_isWindows, "仅在类 Unix 平台运行");

        var unixFile = Path.Combine(_testDir, "unix.txt");

        Assert.True(PathHelper.IsFile(unixFile));

        File.WriteAllText(unixFile, "unix");

        Assert.True(PathHelper.IsFile(unixFile));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsFile_CommonCases(string path)
    {
        Assert.False(PathHelper.IsFile(path));
    }

    [SkippableFact]
    public void IsDirectory_Windows()
    {
        Skip.IfNot(_isWindows, "仅在 Windows 平台运行");

        var winDir = Path.Combine(_testDir, "windows_dir");

        Assert.True(PathHelper.IsDirectory(winDir));
        Assert.True(PathHelper.IsDirectory(winDir + "\\"));
        Assert.True(PathHelper.IsDirectory(winDir.Replace('\\', '/')));

        Directory.CreateDirectory(winDir);

        Assert.True(PathHelper.IsDirectory(winDir));
        Assert.True(PathHelper.IsDirectory(winDir + "\\"));
        Assert.True(PathHelper.IsDirectory(winDir.Replace('\\', '/')));
    }

    [SkippableFact]
    public void IsDirectory_Unix()
    {
        Skip.If(_isWindows, "仅在类 Unix 平台运行");

        var unixDir = Path.Combine(_testDir, "unix_dir");

        Assert.True(PathHelper.IsDirectory(unixDir));
        Assert.True(PathHelper.IsDirectory(unixDir + "/"));

        Directory.CreateDirectory(unixDir);

        Assert.True(PathHelper.IsDirectory(unixDir));
        Assert.True(PathHelper.IsDirectory(unixDir + "/"));
    }

    [SkippableTheory]
    [InlineData(@"C:\test\PATH", @"C:\test\path", true, true)]  // Windows 路径，忽略大小写
    [InlineData("/home/User/test", "/home/user/test", false, false)]  // Unix 路径，区分大小写
    public void PathEquals_PlatformSpecific(string path1, string path2, bool windowsOnly, bool expected)
    {
        Skip.If(windowsOnly && !_isWindows, "仅在 Windows 平台运行");
        Skip.If(!windowsOnly && _isWindows, "仅在类 Unix 平台运行");

        var result = PathHelper.PathEquals(path1, path2);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData(null, "path", false)]
    [InlineData("path", null, false)]
    public void PathEquals_CommonCases(string path1, string path2, bool expected)
    {
        var result = PathHelper.PathEquals(path1, path2);
        Assert.Equal(expected, result);
    }

    [SkippableTheory]
    [InlineData(@"C:\folder1\folder2", 1, @"C:\folder1", true)]
    [InlineData("/home/user/folder", 1, "/home/user", false)]
    public void GetParentDirectory_PlatformSpecific(string path, int levels, string expected, bool windowsOnly)
    {
        Skip.If(windowsOnly && !_isWindows, "仅在 Windows 平台运行");
        Skip.If(!windowsOnly && _isWindows, "仅在类 Unix 平台运行");

        var result = PathHelper.GetParentDirectory(path, levels);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("folder1/folder2", 1, "folder1")]
    public void GetParentDirectory_CommonCases(string path, int levels, string expected)
    {
        var normalizedExpected = expected.Replace('/', Path.DirectorySeparatorChar);
        var result = PathHelper.GetParentDirectory(path, levels);
        Assert.True(PathHelper.PathEquals(normalizedExpected, result));
    }

    // 在现有的 PathHelperTests 类中添加以下测试方法

    [SkippableTheory]
    [InlineData(@"C:\test\path", @"C:\test\path\", true)]   // Windows路径
    [InlineData("/home/user/path", "/home/user/path/", false)] // Unix路径
    public void NormalizePathEndingDirectorySeparator_PlatformSpecific(string input, string expected, bool windowsOnly)
    {
        Skip.If(windowsOnly && !_isWindows, "仅在 Windows 平台运行");
        Skip.If(!windowsOnly && _isWindows, "仅在类 Unix 平台运行");

        var result = PathHelper.NormalizePathEndingDirectorySeparator(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData("  ", "  ")]
    public void NormalizePathEndingDirectorySeparator_EdgeCases(string input, string expected)
    {
        var result = PathHelper.NormalizePathEndingDirectorySeparator(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("path/")]
    [InlineData("path\\")]
    public void NormalizePathEndingDirectorySeparator_AlreadyHasSeparator(string input)
    {
        var result = PathHelper.NormalizePathEndingDirectorySeparator(input);
        Assert.EndsWith(Path.DirectorySeparatorChar.ToString(), result);
    }

    [SkippableTheory]
    [InlineData(@"C:\folder1\folder2", 2, @"C:\", true)]  // Windows 双层目录
    [InlineData("/home/user/folder", 2, "/home", false)]   // Unix 双层目录
    [InlineData(@"C:\folder1\folder2\folder3", 3, @"C:\", true)] // Windows 三层目录
    public void GetParentDirectory_MultiLevelPlatformSpecific(string path, int levels, string expected, bool windowsOnly)
    {
        Skip.If(windowsOnly && !_isWindows, "仅在 Windows 平台运行");
        Skip.If(!windowsOnly && _isWindows, "仅在类 Unix 平台运行");

        var result = PathHelper.GetParentDirectory(path, levels);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("folder1/folder2/folder3", 0, "folder1/folder2/folder3")] // 0级返回原路径
    [InlineData("folder1", 1, ".")] // 到达根目录
    [InlineData("folder1/folder2", -2, ".")] // 负数级别测试
    public void GetParentDirectory_EdgeCases(string path, int levels, string expected)
    {
        var normalizedPath = path.Replace('/', Path.DirectorySeparatorChar);
        var normalizedExpected = expected.Replace('/', Path.DirectorySeparatorChar);
        var result = PathHelper.GetParentDirectory(normalizedPath, levels);
        Assert.True(PathHelper.PathEquals(normalizedExpected, result));
    }

    [Fact]
    public void IsFile_InvalidPath()
    {
        Assert.Throws<ArgumentNullException>(() => PathHelper.IsFile(null));
        Assert.False(PathHelper.IsFile("非法路径*:|"));
    }

    [Fact]
    public void IsDirectory_InvalidPath()
    {
        Assert.Throws<ArgumentNullException>(() => PathHelper.IsDirectory(null));
        Assert.False(PathHelper.IsDirectory("非法路径*:|"));
    }

    [SkippableFact]
    public void PathEquals_FileAndDirectory()
    {
        var fileInDir = Path.Combine(_testDir, "test.txt");
        File.WriteAllText(fileInDir, "test");

        // 同一路径但一个有目录分隔符一个没有
        var pathWithSeparator = _testDir + Path.DirectorySeparatorChar;
        var pathWithoutSeparator = _testDir;

        Assert.True(PathHelper.PathEquals(pathWithSeparator, pathWithoutSeparator));
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(_testFile))
                File.Delete(_testFile);
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"清理时出错: {ex.Message}");
        }
    }
}