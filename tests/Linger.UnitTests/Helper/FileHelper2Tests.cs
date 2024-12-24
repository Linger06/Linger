namespace Linger.UnitTests.Helper;

public class FileHelper2Tests
{
    private readonly string _testDirPath = "testDir2";
    private readonly string _testFilePath = "testDir2/testFile.txt";
    private readonly string _testContent = "Hello, World!";
    private readonly Encoding _encoding = new UTF8Encoding(false);

    public FileHelper2Tests()
    {
        if (!Directory.Exists(_testDirPath))
        {
            Directory.CreateDirectory(_testDirPath);
        }
        File.WriteAllText(_testFilePath, _testContent, _encoding);
    }

    [Fact]
    public void IsExistDirectory_DirectoryExists_ReturnsTrue()
    {
        Assert.True(FileHelper.IsExistDirectory(_testDirPath));
    }

    [Fact]
    public void GetDirectories_ReturnsCorrectDirectories()
    {
        var directories = FileHelper.GetDirectories(_testDirPath);
        Assert.Empty(directories);
    }

    [Fact]
    public void GetDirectories_WithSearchPattern_ReturnsCorrectDirectories()
    {
        var directories = FileHelper.GetDirectories(_testDirPath, "*", false);
        Assert.Empty(directories);
    }

    [Fact]
    public void IsEmptyDirectory_DirectoryIsNotEmpty_ReturnsFalse()
    {
        Assert.False(FileHelper.IsEmptyDirectory(_testDirPath));
    }

    [Fact]
    public void CreateDirectoryIfNotExists_CreatesDirectory()
    {
        var newDirPath = "newTestDir";
        FileHelper.CreateDirectoryIfNotExists(newDirPath);
        Assert.True(Directory.Exists(newDirPath));
        Directory.Delete(newDirPath);
    }

    [Fact]
    public void CopyFolder_CopiesFolderContent()
    {
        var destDirPath = "destDir";
        FileHelper.CopyDir(_testDirPath, destDirPath);
        Assert.True(Directory.Exists(destDirPath));
        Assert.True(File.Exists(Path.Combine(destDirPath, "testFile.txt")));
        Directory.Delete(destDirPath, true);
    }

    [Fact]
    public void IsExistDirectory_ReturnsTrue_WhenDirectoryExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        var result = FileHelper.IsExistDirectory(tempDir);
        Assert.True(result);

        Directory.Delete(tempDir);
    }

    [Fact]
    public void IsExistDirectory_ReturnsFalse_WhenDirectoryDoesNotExist()
    {
        var result = FileHelper.IsExistDirectory("nonexistent_directory");
        Assert.False(result);
    }

    [Fact]
    public void GetFileNames_ReturnsFileNames_WhenDirectoryExists()
    {
        var tempDir = $"testDir-{Guid.NewGuid()}";
        Directory.CreateDirectory(tempDir);
        var tempFile1 = Path.Combine(tempDir, "tempFile1.txt");
        File.WriteAllText(tempFile1, "test content");

        var result = FileHelper.GetFileNames(tempDir);

        Assert.Single(result);
        Assert.Contains(tempFile1, result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void GetFileNames_WithSearchPattern_ReturnsFileNames()
    {
        var tempDir = $"testDir-{Guid.NewGuid()}";
        Directory.CreateDirectory(tempDir);
        var tempFile1 = Path.Combine(tempDir, "tempFile1.txt");
        File.WriteAllText(tempFile1, "test content");

        var result = FileHelper.GetFileNames(tempDir, "*.txt", false);

        Assert.Single(result);
        Assert.Contains(tempFile1, result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void GetFileNames_WithSearchPatternAndSubdirectories_ReturnsFileNames()
    {
        var tempDir = $"testDir-{Guid.NewGuid()}";
        Directory.CreateDirectory(tempDir);
        var subDir = Path.Combine(tempDir, "subDir");
        Directory.CreateDirectory(subDir);
        var tempFile1 = Path.Combine(subDir, "tempFile1.txt");
        File.WriteAllText(tempFile1, "test content");

        var result = FileHelper.GetFileNames(tempDir, "*.txt", true);

        Assert.Single(result);
        Assert.Contains(tempFile1, result);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void ClearDirectory_ClearsAllFilesAndDirectories()
    {
        var tempDir = $"testDir-{Guid.NewGuid()}";
        Directory.CreateDirectory(tempDir);
        var tempFile1 = Path.Combine(tempDir, "tempFile1.txt");
        File.WriteAllText(tempFile1, "test content");
        var subDir = Path.Combine(tempDir, "subDir");
        Directory.CreateDirectory(subDir);
        var tempFile2 = Path.Combine(subDir, "tempFile2.txt");
        File.WriteAllText(tempFile2, "test content");

        FileHelper.ClearDirectory(tempDir);

        Assert.Empty(Directory.GetFiles(tempDir));
        Assert.Empty(Directory.GetDirectories(tempDir));

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void DeleteDirectory_DeletesDirectory()
    {
        var tempDir = $"testDir-{Guid.NewGuid()}";
        Directory.CreateDirectory(tempDir);

        FileHelper.DeleteDirectory(tempDir);

        Assert.False(Directory.Exists(tempDir));
    }
}
