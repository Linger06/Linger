namespace Linger.UnitTests.Helper;

public class FileHelper4Tests
{
    private readonly string _testDirPath = Path.Combine("TestTempDir", "FileHelper4Tests", $"testDir-{Guid.NewGuid().ToString()}");
    private readonly string _testFileName = "testFile.txt";
    private readonly string _testContent = "Hello, World!";
    private readonly Encoding _encoding = Encoding.UTF8;

    public FileHelper4Tests()
    {
        if (!Directory.Exists(_testDirPath))
        {
            Directory.CreateDirectory(_testDirPath);
        }
        File.WriteAllText(Path.Combine(_testDirPath, _testFileName), _testContent, _encoding);
    }

    [Fact]
    public void Contains_FileExists_ReturnsTrue()
    {
        Assert.True(FileHelper.Contains(_testDirPath, "*.txt"));
    }

    [Fact]
    public void Contains_FileExistsInSubDirectory_ReturnsTrue()
    {
        var subDirPath = Path.Combine(_testDirPath, "subDir");
        Directory.CreateDirectory(subDirPath);
        File.WriteAllText(Path.Combine(subDirPath, "subFile.txt"), _testContent, _encoding);

        Assert.True(FileHelper.Contains(_testDirPath, "*.txt", true));

        Directory.Delete(subDirPath, true);
    }

    [Fact]
    public void Contains_ReturnsTrue_WhenFileExistsInDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var tempFile = Path.Combine(tempDir, "tempFile.txt");
        File.WriteAllText(tempFile, "test content");

        var result = FileHelper.Contains(tempDir, "tempFile.txt");

        Assert.True(result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Contains_ReturnsFalse_WhenFileDoesNotExistInDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        var result = FileHelper.Contains(tempDir, "nonexistentFile.txt");

        Assert.False(result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Contains_ReturnsTrue_WhenFileExistsInSubdirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var subDir = Path.Combine(tempDir, "subDir");
        Directory.CreateDirectory(subDir);
        var tempFile = Path.Combine(subDir, "tempFile.txt");
        File.WriteAllText(tempFile, "test content");

        var result = FileHelper.Contains(tempDir, "tempFile.txt", true);

        Assert.True(result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Contains_ReturnsFalse_WhenFileDoesNotExistInSubdirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var subDir = Path.Combine(tempDir, "subDir");
        Directory.CreateDirectory(subDir);

        var result = FileHelper.Contains(tempDir, "nonexistentFile.txt", true);

        Assert.False(result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void GetCustomFileInfo_ReturnsFileInfo_WhenFileExists()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        File.WriteAllText(tempFile, "test content");

        CustomExistFileInfo? result = FileHelper.GetCustomFileInfo(tempFile);

        Assert.NotNull(result);
        Assert.Equal("test content".Length, result.Length);

        File.Delete(tempFile);
    }

    [Fact]
    public void GetCustomFileInfo_ReturnsNull_WhenFileDoesNotExist()
    {
        CustomExistFileInfo? result = FileHelper.GetCustomFileInfo("nonexistentFile.txt");

        Assert.Null(result);
    }
}
