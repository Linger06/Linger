namespace Linger.UnitTests.Helper;

public class FileHelperTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _testContent = "Hello, World!";
    private readonly Encoding _encoding = Encoding.UTF8;

    public FileHelperTests()
    {
        _testDirectory = Path.Combine("TestTempDir", "FileHelperTests", $"testDir-{Guid.NewGuid().ToString()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            const int maxRetries = 3;
            const int delay = 1000; // 1 second

            for (var i = 0; i < maxRetries; i++)
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                    break;
                }
                catch (IOException)
                {
                    if (i == maxRetries - 1)
                        throw;
                    Thread.Sleep(delay);
                }
            }
        }
    }

    private string CreateTestFile(string content)
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        File.WriteAllText(filePath, content, _encoding);
        return filePath;
    }

    [Fact]
    public void IsExistFile_FileExists_ReturnsTrue()
    {
        var filePath = CreateTestFile(_testContent);
        Assert.True(FileHelper.IsExistFile(filePath));
    }

    [Fact]
    public void WriteText_WritesContentToFile()
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        FileHelper.WriteText(filePath, _testContent, _encoding);
        Assert.Equal(_testContent, File.ReadAllText(filePath, _encoding));
        File.Delete(filePath);
    }

    [Fact]
    public void AppendText_AppendsContentToFile()
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        FileHelper.WriteText(filePath, _testContent, _encoding);
        FileHelper.AppendText(filePath, _testContent);
        Assert.Equal(_testContent + _testContent, File.ReadAllText(filePath, _encoding));
        File.Delete(filePath);
    }

    [Fact]
    public void GetLineCount_ReturnsCorrectLineCount()
    {
        var filePath = CreateTestFile(_testContent);
        Assert.Equal(1, FileHelper.GetLineCount(filePath));
        File.Delete(filePath);
    }

    [Fact]
    public void GetFileSize_ReturnsCorrectFileSize()
    {
        var filePath = CreateTestFile(_testContent);
        Assert.True(_testContent.Length < FileHelper.GetFileSize(filePath));
        File.Delete(filePath);
    }

    [Fact]
    public void GetFileSize_ReturnsCorrectFileSizeWithoutBOM()
    {
        var encodingWithoutBOM = new UTF8Encoding(false);
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        FileHelper.WriteText(filePath, _testContent, encodingWithoutBOM);
        Assert.Equal(_testContent.Length, FileHelper.GetFileSize(filePath));
        File.Delete(filePath);
    }

    [Fact]
    public void ReadTxt_ReadsContentFromFile()
    {
        var filePath = CreateTestFile(_testContent);
        Assert.Equal(_testContent, FileHelper.ReadText(filePath, _encoding));
        File.Delete(filePath);
    }

    [Fact]
    public void GetPostfixStr_ReturnsCorrectPostfix()
    {
        var filename = "testFile.txt";
        Assert.Equal(".txt", FileHelper.GetPostfixStr(filename));
    }

    [Fact]
    public void MoveFile_MovesFileToNewDirectory()
    {
        Directory.CreateDirectory(_testDirectory);
        var filePath = CreateTestFile(_testContent);
        FileHelper.MoveFile(filePath, _testDirectory);
        Assert.True(File.Exists(Path.Combine(_testDirectory, Path.GetFileName(filePath))));
        Directory.Delete(_testDirectory, true);
    }

    [Fact]
    public void CopyFile_CopiesFileContent()
    {
        var newFilePath = "newFile.txt";
        var filePath = CreateTestFile(_testContent);
        FileHelper.CopyFile(filePath, newFilePath);
        Assert.Equal(_testContent, File.ReadAllText(newFilePath, _encoding));
        File.Delete(filePath);
        File.Delete(newFilePath);
    }

    [Fact]
    public void DeleteFileIfExists_DeletesFileIfExists()
    {
        var filePath = CreateTestFile(_testContent);
        FileHelper.DeleteFileIfExists(filePath);
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void CopyDir_CopiesDirectoryContent()
    {
        var destDirPath = $"testDir-{Guid.NewGuid().ToString()}";
        var fileName = Guid.NewGuid().ToString() + ".txt";
        var filePath = Path.Combine(_testDirectory, fileName);
        FileHelper.WriteText(filePath, _testContent, _encoding);
        FileHelper.CopyDir(_testDirectory, destDirPath);
        Assert.True(File.Exists(Path.Combine(destDirPath, fileName)));
        Directory.Delete(_testDirectory, true);
        Directory.Delete(destDirPath, true);
    }

    [Fact]
    public void IsExistFile_ReturnsTrue_WhenFileExists()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        File.WriteAllText(tempFile, "test content");

        var result = FileHelper.IsExistFile(tempFile);
        Assert.True(result);

        File.Delete(tempFile);
    }

    [Fact]
    public void IsExistFile_ReturnsFalse_WhenFileDoesNotExist()
    {
        var result = FileHelper.IsExistFile("nonexistent_file.txt");
        Assert.False(result);
    }

    [Fact]
    public void WriteText_CreatesFileWithContent()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var content = "test content";
        Encoding? encoding = Encoding.UTF8;

        FileHelper.WriteText(tempFile, content, encoding);
        var result = File.ReadAllText(tempFile, encoding);

        Assert.Equal(content, result);

        File.Delete(tempFile);
    }

    [Fact]
    public void AppendText_AppendsContentToFile2()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var initialContent = "initial content";
        var appendedContent = " appended content";

        File.WriteAllText(tempFile, initialContent);
        FileHelper.AppendText(tempFile, appendedContent);
        var result = File.ReadAllText(tempFile);

        Assert.Equal(initialContent + appendedContent, result);

        File.Delete(tempFile);
    }

    [Fact]
    public void GetLineCount_ReturnsCorrectLineCount2()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var content = "line1\nline2\nline3";

        File.WriteAllText(tempFile, content);
        var result = FileHelper.GetLineCount(tempFile);

        Assert.Equal(3, result);

        File.Delete(tempFile);
    }

    [Fact]
    public void GetFileSize_ReturnsCorrectFileSize2()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var content = "test content";

        File.WriteAllText(tempFile, content);
        var result = FileHelper.GetFileSize(tempFile);

        Assert.Equal(content.Length, result);

        File.Delete(tempFile);
    }

    [Fact]
    public void ReadTxt_ReturnsFileContent()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var content = "test content";
        Encoding? encoding = Encoding.UTF8;

        File.WriteAllText(tempFile, content, encoding);
        var result = FileHelper.ReadText(tempFile, encoding);

        Assert.Equal(content, result);

        File.Delete(tempFile);
    }

    [Fact]
    public void GetPostfixStr_ReturnsCorrectPostfix2()
    {
        var filename = "example.txt";
        var result = FileHelper.GetPostfixStr(filename);

        Assert.Equal(".txt", result);
    }

    [Fact]
    public void MoveFile_MovesFileToNewDirectory2()
    {
        var sourceFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var destDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var content = "test content";

        File.WriteAllText(sourceFile, content);
        Directory.CreateDirectory(destDir);
        FileHelper.MoveFile(sourceFile, destDir);

        var destFile = Path.Combine(destDir, Path.GetFileName(sourceFile));
        Assert.True(File.Exists(destFile));
        Assert.Equal(content, File.ReadAllText(destFile));

        File.Delete(destFile);
        Directory.Delete(destDir);
    }

    [Fact]
    public void CopyFile_CopiesFileContent2()
    {
        var originFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var newFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var content = "test content";

        File.WriteAllText(originFile, content);
        FileHelper.CopyFile(originFile, newFile);
        var result = File.ReadAllText(newFile);

        Assert.Equal(content, result);

        File.Delete(originFile);
        File.Delete(newFile);
    }

    [Fact]
    public void DeleteFileIfExists_DeletesFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        File.WriteAllText(tempFile, "test content");

        FileHelper.DeleteFileIfExists(tempFile);
        Assert.False(File.Exists(tempFile));
    }

    [Fact]
    public void CopyDir_CopiesDirectoryContent2()
    {
        var srcDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var destDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var tempFile = Path.Combine(srcDir, "tempFile.txt");
        var content = "test content";

        Directory.CreateDirectory(srcDir);
        File.WriteAllText(tempFile, content);
        FileHelper.CopyDir(srcDir, destDir);

        var copiedFile = Path.Combine(destDir, "tempFile.txt");
        Assert.True(File.Exists(copiedFile));
        Assert.Equal(content, File.ReadAllText(copiedFile));

        Directory.Delete(srcDir, true);
        Directory.Delete(destDir, true);
    }

    [Fact]
    public void ClearFile_ClearsFileContent()
    {
        var filePath = CreateTestFile(_testContent);
        FileHelper.ClearFile(filePath);
        Assert.Equal(0, new FileInfo(filePath).Length);
        File.Delete(filePath);
    }

    [Fact]
    public void CreateFile_CreatesFileWithContent()
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        FileHelper.CreateFile(filePath, _testContent, _encoding);
        Assert.Equal(_testContent, File.ReadAllText(filePath, _encoding));
        File.Delete(filePath);
    }

    [Fact]
    public void CreateFile_CreatesEmptyFile()
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        FileHelper.CreateFile(filePath);
        Assert.True(File.Exists(filePath));
        Assert.Equal(0, new FileInfo(filePath).Length);
        File.Delete(filePath);
    }

    [Fact]
    public void CreateFile_CreatesFileWithBuffer()
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        var buffer = _encoding.GetBytes(_testContent);
        FileHelper.CreateFile(filePath, buffer);
        Assert.Equal(_testContent, File.ReadAllText(filePath, _encoding));
        File.Delete(filePath);
    }
}
