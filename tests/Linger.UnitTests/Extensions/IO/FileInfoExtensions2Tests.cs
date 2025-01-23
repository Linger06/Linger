namespace Linger.UnitTests.Extensions.IO;

public class FileInfoExtensions2Tests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ITestOutputHelper _outputHelper;

    public FileInfoExtensions2Tests(ITestOutputHelper outputHelper)
    {
        _testDirectory = Path.Combine("TestTempDir", "FileInfoExtensions2Tests", $"testDir-{Guid.NewGuid().ToString()}");
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

    private List<string> CreateTempFiles(int count)
    {
        var tempFiles = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var tempFile = CreateTestFile("");
            tempFiles.Add(tempFile);
        }
        return tempFiles;
    }

    private string CreateTempFile()
    {
        return CreateTestFile("");
    }

    [Fact]
    public void SetAttributes_ShouldSetAttributes()
    {
        List<string>? tempFiles = CreateTempFiles(2);
        FileInfo[]? files = tempFiles.Select(f => new FileInfo(f)).ToArray();

        files.SetAttributes(FileAttributes.Hidden);

        foreach (FileInfo? file in files)
        {
            Assert.True(file.Attributes.HasFlag(FileAttributes.Hidden));
        }
    }

    [Fact]
    public void SetAttributesAdditive_ShouldAddAttributes()
    {
        List<string>? tempFiles = CreateTempFiles(2);
        FileInfo[]? files = tempFiles.Select(f => new FileInfo(f)).ToArray();

        files.SetAttributesAdditive(FileAttributes.Hidden);

        foreach (FileInfo? file in files)
        {
            Assert.True(file.Attributes.HasFlag(FileAttributes.Hidden));
        }
    }

    [Fact]
    public void Delete_ShouldDeleteFiles()
    {
        List<string>? tempFiles = CreateTempFiles(2);
        var files = tempFiles.Select(f => new FileInfo(f)).ToList();

        files.Delete();

        foreach (FileInfo? file in files)
        {
            Assert.False(file.Exists);
        }
    }

    [Fact]
    public void ToFileSizeBytesString_ShouldFormatBytes()
    {
        Assert.Equal("1G", 1073741824.ToFileSizeBytesString());
        Assert.Equal("1M", 1048576.ToFileSizeBytesString());
        Assert.Equal("1K", 1024.ToFileSizeBytesString());
        Assert.Equal("512Bytes", 512.ToFileSizeBytesString());
    }

    [Fact]
    public void GetFilePath_ShouldReturnFilePath()
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        var expectedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testDirectory);
        Assert.Equal(expectedPath, filePath.GetFilePath());
    }

    [Fact]
    public void GetFilePathString_ShouldReturnFilePath()
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        var expectedPath = _testDirectory;

        Assert.Equal(expectedPath, filePath.GetFilePathString());
    }

    [Fact]
    public void GetFilePath_FileInfo_ShouldReturnFilePath()
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        var expectedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testDirectory);

        var fileInfo = new FileInfo(filePath);

        Assert.Equal(expectedPath, fileInfo.GetFilePath());
    }


    [Fact]
    public void GetFileNameNoExtension_ShouldReturnFileNameWithoutExtension()
    {
        var expectedFileName = Guid.NewGuid().ToString();
        var filePath = Path.Combine(_testDirectory, expectedFileName + ".txt");

        Assert.Equal(expectedFileName, filePath.GetFileNameWithoutExtension());
    }

    [Fact]
    public void GetFileNameNoExtensionString_ShouldReturnFileNameWithoutExtension()
    {
        var expectedFileName = Guid.NewGuid().ToString();
        var filePath = Path.Combine(_testDirectory, expectedFileName + ".txt");

        Assert.Equal(expectedFileName, filePath.GetFileNameWithoutExtensionString());
    }

    [Fact]
    public void GetFileNameString_ShouldReturnFileName()
    {
        var expectedFileName = Guid.NewGuid().ToString() + ".txt";
        var filePath = Path.Combine(_testDirectory, expectedFileName);

        Assert.Equal(expectedFileName, filePath.GetFileNameString());
    }


    [Fact]
    public void GetExtension_ShouldReturnExtension()
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        var expectedExtension = ".txt";

        Assert.Equal(expectedExtension, filePath.GetExtension());
    }

    [Fact]
    public void GetExtensionString_ShouldReturnExtension()
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        var expectedExtension = ".txt";

        Assert.Equal(expectedExtension, filePath.GetExtensionString());
    }

    [Fact]
    public void GetExtension_FileInfo_ShouldReturnExtension()
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        var fileInfo = new FileInfo(filePath);
        var expectedExtension = ".txt";

        Assert.Equal(expectedExtension, fileInfo.GetExtension());
    }

    [Fact]
    public void GetExtensionNotDotString_ShouldReturnExtensionWithoutDot()
    {
        var filePath = Path.Combine(_testDirectory, Guid.NewGuid().ToString() + ".txt");
        var expectedExtension = "txt";

        Assert.Equal(expectedExtension, filePath.GetExtensionWithoutDotString());
    }

    [Fact]
    public void GetFileNameNoExtension_FileInfo_ShouldReturnFileNameWithoutExtension()
    {
        var expectedFileName = Guid.NewGuid().ToString();
        var filePath = Path.Combine(_testDirectory, expectedFileName + ".txt");
        var fileInfo = new FileInfo(filePath);

        Assert.Equal(expectedFileName, fileInfo.GetFileNameWithoutExtension());
    }

    [Fact]
    public async Task GetFileDataAsync_ShouldReturnFileData()
    {
        var tempFile = CreateTempFile();
        var fileData = await tempFile.GetFileDataAsync();

        Assert.NotNull(fileData);
    }

    [Fact]
    public void GetFileVersion_String_ShouldReturnFileVersion()
    {
        var filePath = typeof(object).Assembly.Location;
        var fileVersion = filePath.GetFileVersion();

        Assert.NotNull(fileVersion);
    }

    [Fact]
    public void GetFileVersion_ShouldReturnFileVersion()
    {
        var fileInfo = new FileInfo(typeof(object).Assembly.Location);
        var fileVersion = fileInfo.GetFileVersion();

        Assert.NotNull(fileVersion);
    }

    [Fact]
    public void FileSize_ShouldReturnFileSize_String()
    {
        var tempFile = CreateTempFile();
        var fileSize = tempFile.FileSize();

        Assert.Equal("0Bytes", fileSize);
    }

    [Fact]
    public void FileSize_FileInfo_ShouldReturnFileSize()
    {
        var tempFile = CreateTempFile();
        var fileInfo = new FileInfo(tempFile);
        var fileSize = fileInfo.FileSize();

        Assert.Equal("0Bytes", fileSize);
    }

    [Fact]
    public void ToMd5Hash_ShouldReturnMd5Hash()
    {
        var tempFile = CreateTempFile();
        var fileInfo = new FileInfo(tempFile);
        var md5Hash = fileInfo.ToMd5Hash();

        Assert.NotNull(md5Hash);
    }

    [Fact]
    public void ToMd5HashByte_ShouldReturnMd5HashByte()
    {
        var tempFile = CreateTempFile();
        var fileInfo = new FileInfo(tempFile);
        var md5Hash = fileInfo.ToMd5HashByte();

        Assert.NotNull(md5Hash);
    }
}