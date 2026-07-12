using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Linger.Extensions.Core;
using Linger.Extensions.IO;

#pragma warning disable CS0618 // Compatibility overload coverage.

namespace Linger.UnitTests.Extensions.IO;

public class FileInfoExtensionsTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly List<string> _createdFiles = new();

    public FileInfoExtensionsTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "FileInfoExtensionsTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        foreach (var file in _createdFiles)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch
            {
            }
        }

        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
        }
    }

    private string CreateTestFile(string fileName, string content = "Test Content")
    {
        string filePath = Path.Combine(_testDirectory, fileName);
        File.WriteAllText(filePath, content);
        _createdFiles.Add(filePath);
        return filePath;
    }

    [Fact]
    public void Rename_ShouldRenameFile()
    {
        string filePath = CreateTestFile("test.txt");
        var fileInfo = new FileInfo(filePath);

        var renamedFile = fileInfo.Rename("renamed.txt");

        Assert.Equal("renamed.txt", renamedFile.Name);
        Assert.False(File.Exists(filePath));
        Assert.True(File.Exists(Path.Combine(_testDirectory, "renamed.txt")));
    }

    [Fact]
    public void Rename_ShouldThrowExceptionForNonExistentFile()
    {
        var fileInfo = new FileInfo(Path.Combine(_testDirectory, "nonexistent.txt"));
        Assert.Throws<FileNotFoundException>(() => fileInfo.Rename("renamed.txt"));
    }

    [Fact]
    public void RenameFileWithoutExtension_ShouldRenameWithoutChangingExtension()
    {
        string filePath = CreateTestFile("test.txt");
        var fileInfo = new FileInfo(filePath);

        var renamedFile = fileInfo.RenameFileWithoutExtension("renamed");

        Assert.Equal("renamed.txt", renamedFile.Name);
        Assert.False(File.Exists(filePath));
        Assert.True(File.Exists(Path.Combine(_testDirectory, "renamed.txt")));
    }

    [Fact]
    public void ChangeExtension_ShouldChangeFileExtension()
    {
        string filePath = CreateTestFile("test.txt");
        var fileInfo = new FileInfo(filePath);

        var renamedFile = fileInfo.ChangeExtension(".md");

        Assert.Equal("test.md", renamedFile.Name);
        Assert.False(File.Exists(filePath));
        Assert.True(File.Exists(Path.Combine(_testDirectory, "test.md")));
    }

    [Fact]
    public void ChangeExtension_ShouldHandleExtensionWithoutDot()
    {
        string filePath = CreateTestFile("test.txt");
        var fileInfo = new FileInfo(filePath);

        var renamedFile = fileInfo.ChangeExtension("md");

        Assert.Equal("test.md", renamedFile.Name);
        Assert.False(File.Exists(filePath));
        Assert.True(File.Exists(Path.Combine(_testDirectory, "test.md")));
    }

    [Fact]
    public void ChangeExtensions_ShouldChangeMultipleFileExtensions()
    {
        var filePaths = new[]
        {
            CreateTestFile("test1.txt"),
            CreateTestFile("test2.txt"),
            CreateTestFile("test3.txt")
        };
        var fileInfos = filePaths.Select(p => new FileInfo(p)).ToArray();

        var renamedFiles = fileInfos.ChangeExtensions(".md");

        Assert.Equal(3, renamedFiles.Length);
        foreach (var file in renamedFiles)
        {
            Assert.Equal(".md", file.Extension);
            Assert.True(File.Exists(file.FullName));
        }
        foreach (var path in filePaths)
        {
            Assert.False(File.Exists(path));
        }
    }

    [Fact]
    public void Delete_ShouldDeleteMultipleFiles_WithConsolidation()
    {
        var filePaths = new[]
        {
            CreateTestFile("test1.txt"),
            CreateTestFile("test2.txt"),
            CreateTestFile("test3.txt")
        };
        var fileInfos = filePaths.Select(p => new FileInfo(p)).ToArray();

        fileInfos.Delete(true);

        foreach (var path in filePaths)
        {
            Assert.False(File.Exists(path));
        }
    }

    [Fact]
    public void Delete_ShouldThrowWithoutConsolidation_WhenErrorOccurs()
    {
        string validPath = CreateTestFile("valid.txt");
        var validFile = new FileInfo(validPath);

        string lockedPath = CreateTestFile("locked.txt");

        var lockedFile = new FileInfo(lockedPath);

        var files = new[] { validFile, lockedFile };

        using var fs = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        Assert.Throws<IOException>(() => files.Delete(false));

        Assert.False(File.Exists(validPath));
        Assert.True(File.Exists(lockedPath));
    }

    [Fact]
    public void Delete_ShouldConsolidateExceptions_WhenMultipleErrorsOccur()
    {
        string validPath1 = CreateTestFile("valid1.txt");
        string validPath2 = CreateTestFile("valid2.txt");
        var validFile1 = new FileInfo(validPath1);
        var validFile2 = new FileInfo(validPath2);

        string lockedPath1 = CreateTestFile("locked1.txt");
        string lockedPath2 = CreateTestFile("locked2.txt");
        var lockedFile1 = new FileInfo(lockedPath1);
        var lockedFile2 = new FileInfo(lockedPath2);

        using var fs1 = new FileStream(lockedPath1, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        using var fs2 = new FileStream(lockedPath2, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var files = new[] { validFile1, lockedFile1, validFile2, lockedFile2 };

        var aggregateException = Assert.Throws<AggregateException>(() => files.Delete(true));

        Assert.Equal(2, aggregateException.InnerExceptions.Count);

        Assert.False(File.Exists(validPath1));
        Assert.False(File.Exists(validPath2));

        Assert.True(File.Exists(lockedPath1));
        Assert.True(File.Exists(lockedPath2));
    }

    [Fact]
    public void Delete_SingleParameterOverload_ShouldPreserveFailFastBehavior()
    {
        string validPath = CreateTestFile("valid_default.txt");
        string lockedPath = CreateTestFile("locked_default.txt");
        var validFile = new FileInfo(validPath);
        var lockedFile = new FileInfo(lockedPath);
        var files = new[] { validFile, lockedFile };

        using var fs = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        Assert.Throws<IOException>(() => files.Delete());
        Assert.False(File.Exists(validPath));
        Assert.True(File.Exists(lockedPath));
    }

    [Fact]
    public void CopyTo_ShouldCopyMultipleFiles()
    {
        var filePaths = new[]
        {
            CreateTestFile("test1.txt", "Content 1"),
            CreateTestFile("test2.txt", "Content 2")
        };
        var fileInfos = filePaths.Select(p => new FileInfo(p)).ToArray();

        string targetDir = Path.Combine(_testDirectory, "target");
        Directory.CreateDirectory(targetDir);

        var copiedFiles = fileInfos.CopyTo(targetDir);

        Assert.Equal(2, copiedFiles.Length);
        for (int i = 0; i < filePaths.Length; i++)
        {
            Assert.True(File.Exists(filePaths[i]));

            string expectedTargetPath = Path.Combine(targetDir, Path.GetFileName(filePaths[i]));
            Assert.True(File.Exists(expectedTargetPath));

            string originalContent = File.ReadAllText(filePaths[i]);
            string copiedContent = File.ReadAllText(expectedTargetPath);
            Assert.Equal(originalContent, copiedContent);
        }
    }

    [Fact]
    public void CopyTo_ShouldThrowAggregateException_WhenErrorsOccur()
    {
        string validPath = CreateTestFile("valid_copy.txt", "Valid content");
        var validFile = new FileInfo(validPath);

        string lockedPath = CreateTestFile("locked_copy.txt", "Locked content");
        var lockedFile = new FileInfo(lockedPath);

        string targetDir = Path.Combine(_testDirectory, "copy_error_target");
        Directory.CreateDirectory(targetDir);

        using var fs = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var files = new[] { validFile, lockedFile };

        var aggregateException = Assert.Throws<AggregateException>(() => files.CopyTo(targetDir, true));

        Assert.Single(aggregateException.InnerExceptions);

        string validTargetPath = Path.Combine(targetDir, "valid_copy.txt");
        Assert.True(File.Exists(validTargetPath));

        string lockedTargetPath = Path.Combine(targetDir, "locked_copy.txt");
        Assert.False(File.Exists(lockedTargetPath));
    }

    [Fact]
    public void CopyTo_ShouldThrowDirectly_WhenConsolidateExceptionsIsFalse()
    {
        string validPath = CreateTestFile("valid_direct.txt", "Valid content");
        string lockedPath = CreateTestFile("locked_direct.txt", "Locked content");
        var validFile = new FileInfo(validPath);
        var lockedFile = new FileInfo(lockedPath);

        string targetDir = Path.Combine(_testDirectory, "copy_direct_target");
        Directory.CreateDirectory(targetDir);

        using var fs = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var files = new[] { lockedFile, validFile };

        Assert.Throws<IOException>(() => files.CopyTo(targetDir, false));

        Assert.False(File.Exists(Path.Combine(targetDir, "valid_direct.txt")));
        Assert.False(File.Exists(Path.Combine(targetDir, "locked_direct.txt")));
    }

    [Fact]
    public void MoveTo_ShouldMoveMultipleFiles()
    {
        var filePaths = new[]
        {
            CreateTestFile("move1.txt", "Content 1"),
            CreateTestFile("move2.txt", "Content 2")
        };
        var fileInfos = filePaths.Select(p => new FileInfo(p)).ToArray();

        string targetDir = Path.Combine(_testDirectory, "moveTarget");
        Directory.CreateDirectory(targetDir);

        var movedFiles = fileInfos.MoveTo(targetDir);

        Assert.Equal(2, movedFiles.Length);
        for (int i = 0; i < filePaths.Length; i++)
        {
            Assert.False(File.Exists(filePaths[i]));

            string expectedTargetPath = Path.Combine(targetDir, Path.GetFileName(filePaths[i]));
            Assert.True(File.Exists(expectedTargetPath));
        }
    }

    [Fact]
    public void MoveTo_ShouldThrowAggregateException_WhenErrorsOccur()
    {
        string validPath = CreateTestFile("valid_move.txt", "Valid content");
        var validFile = new FileInfo(validPath);

        string lockedPath = CreateTestFile("locked_move.txt", "Locked content");
        var lockedFile = new FileInfo(lockedPath);

        string targetDir = Path.Combine(_testDirectory, "move_error_target");
        Directory.CreateDirectory(targetDir);

        using var fs = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var files = new[] { validFile, lockedFile };

        var aggregateException = Assert.Throws<AggregateException>(() => files.MoveTo(targetDir, true));

        Assert.Single(aggregateException.InnerExceptions);

        Assert.False(File.Exists(validPath));
        Assert.True(File.Exists(Path.Combine(targetDir, "valid_move.txt")));

        Assert.True(File.Exists(lockedPath));
        Assert.False(File.Exists(Path.Combine(targetDir, "locked_move.txt")));
    }

    [Fact]
    public void MoveTo_ShouldThrowDirectly_WhenConsolidateExceptionsIsFalse()
    {
        string validPath = CreateTestFile("valid_move_direct.txt", "Valid content");
        string lockedPath = CreateTestFile("locked_move_direct.txt", "Locked content");
        var validFile = new FileInfo(validPath);
        var lockedFile = new FileInfo(lockedPath);

        string targetDir = Path.Combine(_testDirectory, "move_direct_target");
        Directory.CreateDirectory(targetDir);

        using var fs = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var files = new[] { lockedFile, validFile };

        Assert.Throws<IOException>(() => files.MoveTo(targetDir, false));

        Assert.True(File.Exists(validPath));
        Assert.True(File.Exists(lockedPath));

        Assert.False(File.Exists(Path.Combine(targetDir, "valid_move_direct.txt")));
        Assert.False(File.Exists(Path.Combine(targetDir, "locked_move_direct.txt")));
    }

    [Fact]
    public void ToFileSizeBytesString_ShouldFormatBytesCorrectly()
    {
        Assert.Equal("100Bytes", 100.ToFileSizeBytesString());
        Assert.Equal("1K", 1024.ToFileSizeBytesString());
        Assert.Equal("1M", 1048576.ToFileSizeBytesString());
        Assert.Equal("1G", 1073741824.ToFileSizeBytesString());
    }

    [Fact]
    public void GetFileSize_ShouldReturnCorrectSize()
    {
        string content = new string('A', 1000);
        string filePath = CreateTestFile("size.txt", content);

        long size = filePath.GetFileSize();

        Assert.Equal(1000, size);
    }

    [Fact]
    public void GetFileSizeFormatted_ShouldReturnFormattedSize()
    {
        string content = new string('A', 1024);
        string filePath = CreateTestFile("formatted_size.txt", content);

        string formattedSize = filePath.GetFileSizeFormatted();

        Assert.Equal("1KB", formattedSize);
    }

    [Fact]
    public void GetFileSizeFormatted_FileInfo_ShouldReturnFormattedSize()
    {
        string content = new string('A', 1024);
        string filePath = CreateTestFile("fileinfo_size.txt", content);
        var fileInfo = new FileInfo(filePath);

        string formattedSize = fileInfo.GetFileSizeFormatted();

        Assert.Equal("1KB", formattedSize);
    }

    [Fact]
    public void ToMemoryStream_ShouldCreateMemoryStreamWithSameContent()
    {
        string content = "This is a test for memory stream conversion";
        string filePath = CreateTestFile("memory_stream.txt", content);
        var fileInfo = new FileInfo(filePath);

        using var memoryStream = fileInfo.ToMemoryStream();

        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream);
        string result = reader.ReadToEnd();
        Assert.Equal(content, result);
    }

    [Fact]
    public async Task GetFileDataAsync_ShouldReturnCorrectByteArray()
    {
        string content = "This is a test for GetFileDataAsync method";
        string filePath = CreateTestFile("file_data_async.txt", content);

        byte[] result = await filePath.GetFileDataAsync();

        byte[] expected = Encoding.UTF8.GetBytes(content);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetFileDataAsync_ShouldThrowException_ForNonExistentFile()
    {
        string nonExistentPath = Path.Combine(_testDirectory, "non_existent_file.txt");

        await Assert.ThrowsAsync<FileNotFoundException>(() => nonExistentPath.GetFileDataAsync());
    }

    [Fact]
    public void ComputeHashMd5_ShouldReturnCorrectHash()
    {
        string content = "Test content for MD5 hash";
        string filePath = CreateTestFile("md5.txt", content);
        var fileInfo = new FileInfo(filePath);

        byte[] contentBytes = Encoding.UTF8.GetBytes(content);
        using var md5 = MD5.Create();
        byte[] expectedHashBytes = md5.ComputeHash(contentBytes);
    string expectedHash = BitConverter.ToString(expectedHashBytes).Replace("-", "").ToLowerInvariant();

        string hash = fileInfo.ComputeHashMd5();

        Assert.Equal(expectedHash, hash);
    }

    [Fact]
    public void GetFileVersion_ShouldReturnVersion_ForAssemblyFile()
    {
        var currentAssembly = typeof(FileInfoExtensionsTests).Assembly.Location;
        var fileInfo = new FileInfo(currentAssembly);

        string? version = fileInfo.GetFileVersion();

        Assert.NotNull(version);
    }

    [Fact]
    public void GetFileVersion_StringOverload_ShouldMatchPlatformBehavior()
    {
        string currentAssembly = typeof(FileInfoExtensionsTests).Assembly.Location;

        string? version = currentAssembly.GetFileVersion();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.NotNull(version);
        }
        else
        {
            Assert.Null(version);
        }
    }

    [Fact]
    public void GetFileVersion_ShouldThrowException_ForNonExistentFile()
    {
        var fileInfo = new FileInfo(Path.Combine(_testDirectory, "nonexistent.dll"));
        Assert.Throws<FileNotFoundException>(() => fileInfo.GetFileVersion());
    }
}
