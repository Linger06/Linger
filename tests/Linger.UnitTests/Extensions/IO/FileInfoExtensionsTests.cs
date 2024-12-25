namespace Linger.UnitTests.Extensions.IO;

public class FileInfoExtensionsTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ITestOutputHelper _outputHelper;

    public FileInfoExtensionsTests(ITestOutputHelper outputHelper)
    {
        _testDirectory = $"testDir-{Guid.NewGuid().ToString()}";
        Directory.CreateDirectory(_testDirectory);
        _outputHelper = outputHelper;
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
                    _outputHelper.WriteLine($"To Delete {_testDirectory}");
                    Directory.Delete(_testDirectory, true);
                    break;
                }
                catch
                {
                    _outputHelper.WriteLine($"Deleted Fail {_testDirectory}");
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
        File.WriteAllText(filePath, content);
        return filePath;
    }

    [Fact]
    public void ToMemoryStream_ShouldConvertFileToMemoryStream()
    {
        var filePath = CreateTestFile("Test content");

        var fileInfo = new FileInfo(filePath);
        using var memoryStream = fileInfo.ToMemoryStream();

        Assert.Equal("Test content", new StreamReader(memoryStream).ReadToEnd());
    }

    [Fact]
    public void ToMemoryStream_ShouldDeleteFileIfSpecified()
    {
        var filePath = CreateTestFile("Test content");

        var fileInfo = new FileInfo(filePath);
        using var memoryStream = fileInfo.ToMemoryStream(deleteFile: true);

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void ToMemoryStream_ShouldThrowFileNotFoundException()
    {
        var fileInfo = new FileInfo("nonexistent.txt");

        Assert.Throws<FileNotFoundException>(() => fileInfo.ToMemoryStream());
    }

    [Fact]
    public void ToMemoryStream2_ShouldConvertFileToMemoryStream()
    {
        var filePath = CreateTestFile("Test content");

        var fileInfo = new FileInfo(filePath);
        using MemoryStream? memoryStream = fileInfo.ToMemoryStream2();

        Assert.Equal("Test content", new StreamReader(memoryStream).ReadToEnd());
    }

    [Fact]
    public void ToMemoryStream2_ShouldThrowFileNotFoundException()
    {
        var fileInfo = new FileInfo("nonexistent.txt");

        Assert.Throws<FileNotFoundException>(() => fileInfo.ToMemoryStream2());
    }

    [Fact]
    public void ToMemoryStream3_ShouldConvertFileToMemoryStream()
    {
        var filePath = CreateTestFile("Test content");

        var fileInfo = new FileInfo(filePath);
        using MemoryStream? memoryStream = fileInfo.ToMemoryStream3();

        Assert.Equal("Test content", new StreamReader(memoryStream).ReadToEnd());
    }

    [Fact]
    public void ToMemoryStream3_ShouldThrowFileNotFoundException()
    {
        var fileInfo = new FileInfo("nonexistent.txt");

        Assert.Throws<FileNotFoundException>(() => fileInfo.ToMemoryStream3());
    }

    [Fact]
    public void ComputeHashMd5_ShouldReturnCorrectMd5Hash()
    {
        var filePath = CreateTestFile("Test content");
        _outputHelper.WriteLine($"CreateTestFile {filePath}");

        var fileInfo = new FileInfo(filePath);
        var hash = fileInfo.ComputeHashMd5();
        _outputHelper.WriteLine($"ComputeHashMd5 {hash}");
        Assert.Equal("8BFA8E0684108F419933A5995264D150", hash);
    }

    [Fact]
    public void ComputeHashMd5_ShouldThrowFileNotFoundException()
    {
        var fileInfo = new FileInfo("nonexistent.txt");

        Assert.Throws<FileNotFoundException>(() => fileInfo.ComputeHashMd5());
    }

    [Fact]
    public void Rename_ShouldRenameFile()
    {
        var filePath = CreateTestFile("Test content");
        var fileInfo = new FileInfo(filePath);
        var newFileName = "renamed.txt";

        fileInfo.Rename(newFileName);

        var newFilePath = Path.Combine(_testDirectory, newFileName);
        Assert.True(File.Exists(newFilePath));
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void Rename_ShouldThrowFileNotFoundException()
    {
        var fileInfo = new FileInfo("nonexistent.txt");

        Assert.Throws<FileNotFoundException>(() => fileInfo.Rename("newname.txt"));
    }

    [Fact]
    public void RenameFileWithoutExtension_ShouldRenameFileWithoutChangingExtension()
    {
        var filePath = CreateTestFile("Test content");
        var fileInfo = new FileInfo(filePath);
        var newFileName = "renamed";

        fileInfo.RenameFileWithoutExtension(newFileName);

        var newFilePath = Path.Combine(_testDirectory, newFileName + ".txt");
        Assert.True(File.Exists(newFilePath));
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void RenameFileWithoutExtension_ShouldThrowFileNotFoundException()
    {
        var fileInfo = new FileInfo("nonexistent.txt");

        Assert.Throws<FileNotFoundException>(() => fileInfo.RenameFileWithoutExtension("newname"));
    }

    [Fact]
    public void ChangeExtension_ShouldChangeFileExtension()
    {
        var filePath = CreateTestFile("Test content");
        var fileInfo = new FileInfo(filePath);
        var newExtension = ".xml";

        fileInfo.ChangeExtension(newExtension);

        var newFilePath = Path.Combine(_testDirectory, Path.GetFileNameWithoutExtension(filePath) + newExtension);
        Assert.True(File.Exists(newFilePath));
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void ChangeExtension_ShouldThrowFileNotFoundException()
    {
        var fileInfo = new FileInfo("nonexistent.txt");

        Assert.Throws<FileNotFoundException>(() => fileInfo.ChangeExtension(".xml"));
    }

    [Fact]
    public void ChangeExtensions_ShouldChangeExtensionsOfMultipleFiles()
    {
        var file1Path = CreateTestFile("Test content 1");
        var file2Path = CreateTestFile("Test content 2");
        FileInfo[]? files = new[] { new FileInfo(file1Path), new FileInfo(file2Path) };
        var newExtension = ".tmp";

        files.ChangeExtensions(newExtension);

        Assert.All(files, file =>
        {
            var newFilePath = Path.Combine(_testDirectory, Path.GetFileNameWithoutExtension(file.Name) + newExtension);
            Assert.True(File.Exists(newFilePath));
        });

        Assert.False(File.Exists(file1Path));
        Assert.False(File.Exists(file2Path));
    }

    [Fact]
    public void Delete_ShouldDeleteMultipleFiles()
    {
        var file1Path = CreateTestFile("Test content 1");
        var file2Path = CreateTestFile("Test content 2");
        FileInfo[]? files = new[] { new FileInfo(file1Path), new FileInfo(file2Path) };

        files.Delete(false);

        Assert.All(files, file => Assert.False(File.Exists(file.FullName)));
    }

    [Fact]
    public void Delete_ShouldHandleExceptions()
    {
        var file1Path = CreateTestFile("Test content 1");
        var file2Path = CreateTestFile("Test content 2");
        var file3Path = CreateTestFile("Test content 3");
        FileInfo[]? files = new[] { new FileInfo(file1Path), new FileInfo(file2Path), new FileInfo(file3Path) };
        using var steam = new FileInfo(file3Path).Open(FileMode.Open, FileAccess.Read, FileShare.None);
        var exception = Assert.Throws<AggregateException>(() => files.Delete(true));
        Assert.Single(exception.InnerExceptions);
        var exception2 = Assert.Throws<IOException>(() => files.Delete(false));
        steam.Close();
    }

    [Fact]
    public void CopyTo_ShouldCopyMultipleFiles()
    {
        var file1Path = CreateTestFile("Test content 1");
        var file2Path = CreateTestFile("Test content 2");
        FileInfo[]? files = new[] { new FileInfo(file1Path), new FileInfo(file2Path) };
        var targetPath = Path.Combine(_testDirectory, "copy");
        Directory.CreateDirectory(targetPath);

        FileInfo[]? copiedFiles = files.CopyTo(targetPath);

        Assert.All(copiedFiles, file =>
        {
            var copiedFilePath = Path.Combine(targetPath, file.Name);
            Assert.True(File.Exists(copiedFilePath));
        });
    }

    [Fact]
    public void CopyTo_ShouldHandleExceptions()
    {
        var file1Path = CreateTestFile("Test content 1");
        var file2Path = CreateTestFile("Test content 2");
        var file3Path = "nonexistent.txt";
        FileInfo[]? files = new[] { new FileInfo(file1Path), new FileInfo(file2Path), new FileInfo(file3Path) };
        var targetPath = Path.Combine(_testDirectory, "copy");
        Directory.CreateDirectory(targetPath);

        var exception = Assert.Throws<AggregateException>(() => files.CopyTo(targetPath, true));
        Assert.Single(exception.InnerExceptions);

        var exception2 = Assert.Throws<IOException>(() => files.CopyTo(targetPath, false));
    }

    [Fact]
    public void MoveTo_ShouldMoveMultipleFiles()
    {
        var file1Path = CreateTestFile("Test content 1");
        var file2Path = CreateTestFile("Test content 2");
        FileInfo[]? files = new[] { new FileInfo(file1Path), new FileInfo(file2Path) };
        var targetPath = Path.Combine(_testDirectory, "move");
        Directory.CreateDirectory(targetPath);

        FileInfo[]? movedFiles = files.MoveTo(targetPath);

        Assert.All(movedFiles, file =>
        {
            var movedFilePath = Path.Combine(targetPath, file.Name);
            Assert.True(File.Exists(movedFilePath));
        });

        Assert.False(File.Exists(file1Path));
        Assert.False(File.Exists(file2Path));
    }

    [Fact]
    public void MoveTo_ShouldHandleExceptions()
    {
        var file1Path = CreateTestFile("Test content 1");
        var file2Path = CreateTestFile("Test content 2");
        var file3Path = "nonexistent.txt";
        FileInfo[]? files = new[] { new FileInfo(file1Path), new FileInfo(file2Path), new FileInfo(file3Path) };
        var targetPath = Path.Combine(_testDirectory, "move");
        Directory.CreateDirectory(targetPath);

        var exception = Assert.Throws<AggregateException>(() => files.MoveTo(targetPath, true));
        Assert.Single(exception.InnerExceptions);

        var exception2 = Assert.Throws<FileNotFoundException>(() => files.MoveTo(targetPath, false));
    }
}
