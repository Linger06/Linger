namespace Linger.UnitTests.Helper;

public class FileHelper3Tests
{
    private readonly string _testDirPath = "testDir3";
    private readonly string _testFilePath = "testDir3/testFile.txt";
    private readonly string _testContent = "Hello, World!";
    private readonly Encoding _encoding = Encoding.UTF8;

    public FileHelper3Tests()
    {
        if (!Directory.Exists(_testDirPath))
        {
            Directory.CreateDirectory(_testDirPath);
        }
        File.WriteAllText(_testFilePath, _testContent, _encoding);
    }

    [Fact]
    public void GetFileName_ReturnsCorrectFileName()
    {
        var fileName = FileHelper.GetFileName(_testFilePath);
        Assert.Equal("testFile.txt", fileName);
    }

    [Fact]
    public void ClearDirectory_ClearsAllFilesAndDirectories()
    {
        var subDirPath = Path.Combine(_testDirPath, "subDir");
        Directory.CreateDirectory(subDirPath);
        File.WriteAllText(Path.Combine(subDirPath, "subFile.txt"), _testContent, _encoding);

        FileHelper.ClearDirectory(_testDirPath);
        Assert.True(Directory.Exists(_testDirPath));
        Assert.Empty(Directory.GetFiles(_testDirPath));
        Assert.Empty(Directory.GetDirectories(_testDirPath));
    }

    [Fact]
    public void ClearFile_ClearsFileContent()
    {
        FileHelper.ClearFile(_testFilePath);
        Assert.Empty(File.ReadAllText(_testFilePath));
    }

    [Fact]
    public void DeleteDirectory_DeletesDirectory()
    {
        var subDirPath = Path.Combine(_testDirPath, "subDir");
        Directory.CreateDirectory(subDirPath);

        FileHelper.DeleteDirectory(subDirPath);
        Assert.False(Directory.Exists(subDirPath));
    }

    [Fact]
    public void CreateFile_CreatesFileWithContent()
    {
        var newFilePath = "newTestFile.txt";
        FileHelper.CreateFile(newFilePath, _testContent, _encoding);
        Assert.True(File.Exists(newFilePath));
        Assert.Equal(_testContent, File.ReadAllText(newFilePath, _encoding));
        File.Delete(newFilePath);
    }

    [Fact]
    public void CreateFile_CreatesEmptyFile()
    {
        var newFilePath = "newTestFile.txt";
        FileHelper.CreateFile(newFilePath);
        Assert.True(File.Exists(newFilePath));
        Assert.Empty(File.ReadAllText(newFilePath));
        File.Delete(newFilePath);
    }

    [Fact]
    public void CreateFile_WithBuffer_CreatesFileWithContent()
    {
        var newFilePath = "newTestFile.txt";
        var buffer = _encoding.GetBytes(_testContent);
        FileHelper.CreateFile(newFilePath, buffer);
        Assert.True(File.Exists(newFilePath));
        Assert.Equal(_testContent, File.ReadAllText(newFilePath, _encoding));
        File.Delete(newFilePath);
    }



    [Fact]
    public void GetFileName_ReturnsFileNameWithExtension()
    {
        var filePath = Path.Combine(Path.GetTempPath(), "example.txt");
        var result = FileHelper.GetFileName(filePath);

        Assert.Equal("example.txt", result);
    }

    [Fact]
    public void ClearDirectory_RemovesAllFilesAndSubdirectories()
    {
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        var subDir = Path.Combine(dir, "subDir");
        Directory.CreateDirectory(subDir);
        var file = Path.Combine(dir, "file.txt");
        File.WriteAllText(file, "content");

        FileHelper.ClearDirectory(dir);

        Assert.Empty(Directory.GetFiles(dir));
        Assert.Empty(Directory.GetDirectories(dir));

        Directory.Delete(dir, true);
    }

    [Fact]
    public void ClearFile_EmptiesFileContent()
    {
        var file = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        File.WriteAllText(file, "content");

        FileHelper.ClearFile(file);

        Assert.Empty(File.ReadAllText(file));

        File.Delete(file);
    }

    [Fact]
    public void DeleteDirectory_RemovesDirectoryAndContents()
    {
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "file.txt");
        File.WriteAllText(file, "content");

        FileHelper.DeleteDirectory(dir);

        Assert.False(Directory.Exists(dir));
    }

    [Fact]
    public void CreateFile_CreatesFileWithContent2()
    {
        var file = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var content = "test content";

        FileHelper.CreateFile(file, content);

        Assert.True(File.Exists(file));
        Assert.Equal(content, File.ReadAllText(file));

        File.Delete(file);
    }

    [Fact]
    public void CreateFile_CreatesEmptyFile2()
    {
        var file = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        FileHelper.CreateFile(file);

        Assert.True(File.Exists(file));
        Assert.Empty(File.ReadAllText(file));

        File.Delete(file);
    }

    [Fact]
    public void CreateFile_WithBuffer_CreatesFileWithContent2()
    {
        var file = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var content = "test content";
        var buffer = Encoding.UTF8.GetBytes(content);

        FileHelper.CreateFile(file, buffer);

        Assert.True(File.Exists(file));
        Assert.Equal(content, File.ReadAllText(file));

        File.Delete(file);
    }
}