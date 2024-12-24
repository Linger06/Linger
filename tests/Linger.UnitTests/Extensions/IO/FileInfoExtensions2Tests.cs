using System.Diagnostics;

namespace Linger.UnitTests.Extensions.IO;

public class FileInfoExtensions2Tests
{
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

        CleanupTempFiles(tempFiles);
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

        CleanupTempFiles(tempFiles);
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
    public void GetVersionInfo_ShouldReturnVersionInfo()
    {
        var fileInfo = new FileInfo(typeof(object).Assembly.Location);
        FileVersionInfo? versionInfo = fileInfo.GetVersionInfo();

        Assert.NotNull(versionInfo);
    }

    [Fact]
    public void GetFileVersion_ShouldReturnFileVersion()
    {
        var fileInfo = new FileInfo(typeof(object).Assembly.Location);
        var fileVersion = fileInfo.GetFileVersion();

        Assert.NotNull(fileVersion);
    }

    [Fact]
    public void GetFilePath_ShouldReturnFilePath()
    {
        var filePath = @"C:\Temp\test.txt";
        var expectedPath = @"C:\Temp\";

        Assert.Equal(expectedPath, filePath.GetFilePath());
    }

    [Fact]
    public void FileSize_ShouldReturnFileSize()
    {
        var tempFile = CreateTempFile();
        var fileInfo = new FileInfo(tempFile);
        var fileSize = fileInfo.FileSize();

        Assert.Equal("0Bytes", fileSize);

        CleanupTempFile(tempFile);
    }

    [Fact]
    public void GetFileNameNoExtension_ShouldReturnFileNameWithoutExtension()
    {
        var filePath = @"C:\Temp\test.txt";
        var expectedFileName = "test";

        Assert.Equal(expectedFileName, filePath.GetFileNameNoExtension());
    }

    [Fact]
    public void GetFileNameNoExtensionString_ShouldReturnFileNameWithoutExtension()
    {
        var filePath = @"C:\Temp\test.txt";
        var expectedFileName = "test";

        Assert.Equal(expectedFileName, filePath.GetFileNameNoExtensionString());
    }

    [Fact]
    public void GetFileNameString_ShouldReturnFileName()
    {
        var filePath = @"C:\Temp\test.txt";
        var expectedFileName = "test.txt";

        Assert.Equal(expectedFileName, filePath.GetFileNameString());
    }

    [Fact]
    public void GetFilePathString_ShouldReturnFilePath()
    {
        var filePath = @"C:\Temp\test.txt";
        var expectedPath = @"C:\Temp";

        Assert.Equal(expectedPath, filePath.GetFilePathString());
    }

    [Fact]
    public void ToMd5Hash_ShouldReturnMd5Hash()
    {
        var tempFile = CreateTempFile();
        var fileInfo = new FileInfo(tempFile);
        var md5Hash = fileInfo.ToMd5Hash();

        Assert.NotNull(md5Hash);

        CleanupTempFile(tempFile);
    }

    [Fact]
    public void ToMd5HashByte_ShouldReturnMd5HashByte()
    {
        var tempFile = CreateTempFile();
        var fileInfo = new FileInfo(tempFile);
        var md5Hash = fileInfo.ToMd5HashByte();

        Assert.NotNull(md5Hash);

        CleanupTempFile(tempFile);
    }

    [Fact]
    public void GetExtension_ShouldReturnExtension()
    {
        var filePath = @"C:\Temp\test.txt";
        var expectedExtension = ".txt";

        Assert.Equal(expectedExtension, filePath.GetExtension());
    }

    [Fact]
    public void GetExtensionString_ShouldReturnExtension()
    {
        var filePath = @"C:\Temp\test.txt";
        var expectedExtension = ".txt";

        Assert.Equal(expectedExtension, filePath.GetExtensionString());
    }

    [Fact]
    public void GetExtensionNotDotString_ShouldReturnExtensionWithoutDot()
    {
        var filePath = @"C:\Temp\test.txt";
        var expectedExtension = "txt";

        Assert.Equal(expectedExtension, filePath.GetExtensionNotDotString());
    }

    [Fact]
    public async Task GetFileDataAsync_ShouldReturnFileData()
    {
        var tempFile = CreateTempFile();
        var fileData = await tempFile.GetFileDataAsync();

        Assert.NotNull(fileData);

        CleanupTempFile(tempFile);
    }

    [Fact]
    public void GetFilePath_FileInfo_ShouldReturnFilePath()
    {
        var fileInfo = new FileInfo(@"C:\Temp\test.txt");
        var expectedPath = @"C:\Temp\";

        Assert.Equal(expectedPath, fileInfo.GetFilePath());
    }

    [Fact]
    public void GetFileNameNoExtension_FileInfo_ShouldReturnFileNameWithoutExtension()
    {
        var fileInfo = new FileInfo(@"C:\Temp\test.txt");
        var expectedFileName = "test";

        Assert.Equal(expectedFileName, fileInfo.GetFileNameNoExtension());
    }

    [Fact]
    public void FileSize_FileInfo_ShouldReturnFileSize()
    {
        var tempFile = CreateTempFile();
        var fileInfo = new FileInfo(tempFile);
        var fileSize = fileInfo.FileSize();

        Assert.Equal("0Bytes", fileSize);

        CleanupTempFile(tempFile);
    }

    [Fact]
    public void GetExtension_FileInfo_ShouldReturnExtension()
    {
        var fileInfo = new FileInfo(@"C:\Temp\test.txt");
        var expectedExtension = ".txt";

        Assert.Equal(expectedExtension, fileInfo.GetExtension());
    }

    private List<string> CreateTempFiles(int count)
    {
        var tempFiles = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var tempFile = Path.GetTempFileName();
            tempFiles.Add(tempFile);
        }
        return tempFiles;
    }

    private string CreateTempFile()
    {
        return Path.GetTempFileName();
    }

    private void CleanupTempFiles(List<string> tempFiles)
    {
        foreach (var tempFile in tempFiles)
        {
            File.Delete(tempFile);
        }
    }

    private void CleanupTempFile(string tempFile)
    {
        File.Delete(tempFile);
    }
    [Fact]
    public void GetFileVersion_String_ShouldReturnFileVersion()
    {
        var filePath = typeof(object).Assembly.Location;
        var fileVersion = filePath.GetFileVersion();

        Assert.NotNull(fileVersion);
    }
    [Fact]
    public void FileSize_ShouldReturnFileSize_String()
    {
        var tempFile = CreateTempFile();
        var fileSize = tempFile.FileSize();

        Assert.Equal("0Bytes", fileSize);

        CleanupTempFile(tempFile);
    }
}
