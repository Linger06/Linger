using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Linger.Extensions.Core;
using Linger.Extensions.IO;

namespace Linger.UnitTests.Extensions.IO;

public class FileInfoExtensionsTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly List<string> _createdFiles = new();

    public FileInfoExtensionsTests()
    {
        // 创建一个临时目录进行测试
        _testDirectory = Path.Combine(Path.GetTempPath(), "FileInfoExtensionsTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        // 清理创建的所有文件
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
                // 忽略清理过程中的错误
            }
        }

        // 尝试删除测试目录
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // 忽略清理过程中的错误
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
        // 创建测试文件
        string filePath = CreateTestFile("test.txt");
        var fileInfo = new FileInfo(filePath);

        // 执行测试
        var renamedFile = fileInfo.Rename("renamed.txt");

        // 验证
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
        // 创建测试文件
        string filePath = CreateTestFile("test.txt");
        var fileInfo = new FileInfo(filePath);

        // 执行测试
        var renamedFile = fileInfo.RenameFileWithoutExtension("renamed");

        // 验证
        Assert.Equal("renamed.txt", renamedFile.Name);
        Assert.False(File.Exists(filePath));
        Assert.True(File.Exists(Path.Combine(_testDirectory, "renamed.txt")));
    }

    [Fact]
    public void ChangeExtension_ShouldChangeFileExtension()
    {
        // 创建测试文件
        string filePath = CreateTestFile("test.txt");
        var fileInfo = new FileInfo(filePath);

        // 执行测试
        var renamedFile = fileInfo.ChangeExtension(".md");

        // 验证
        Assert.Equal("test.md", renamedFile.Name);
        Assert.False(File.Exists(filePath));
        Assert.True(File.Exists(Path.Combine(_testDirectory, "test.md")));
    }

    [Fact]
    public void ChangeExtension_ShouldHandleExtensionWithoutDot()
    {
        // 创建测试文件
        string filePath = CreateTestFile("test.txt");
        var fileInfo = new FileInfo(filePath);

        // 执行测试
        var renamedFile = fileInfo.ChangeExtension("md");

        // 验证
        Assert.Equal("test.md", renamedFile.Name);
        Assert.False(File.Exists(filePath));
        Assert.True(File.Exists(Path.Combine(_testDirectory, "test.md")));
    }

    [Fact]
    public void ChangeExtensions_ShouldChangeMultipleFileExtensions()
    {
        // 创建测试文件
        var filePaths = new[]
        {
            CreateTestFile("test1.txt"),
            CreateTestFile("test2.txt"),
            CreateTestFile("test3.txt")
        };
        var fileInfos = filePaths.Select(p => new FileInfo(p)).ToArray();

        // 执行测试
        var renamedFiles = fileInfos.ChangeExtensions(".md");

        // 验证
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
        // 创建测试文件
        var filePaths = new[]
        {
            CreateTestFile("test1.txt"),
            CreateTestFile("test2.txt"),
            CreateTestFile("test3.txt")
        };
        var fileInfos = filePaths.Select(p => new FileInfo(p)).ToArray();

        // 执行测试
        fileInfos.Delete(true);

        // 验证
        foreach (var path in filePaths)
        {
            Assert.False(File.Exists(path));
        }
    }

    [Fact]
    public void Delete_ShouldThrowWithoutConsolidation_WhenErrorOccurs()
    {
        // 创建一个存在的文件和一个临时但无效的 FileInfo
        string validPath = CreateTestFile("valid.txt");
        var validFile = new FileInfo(validPath);

        // 创建一个文件，然后添加访问权限以模拟删除错误
        string lockedPath = CreateTestFile("locked.txt");

        // 创建临时文件并尝试锁定它
        var lockedFile = new FileInfo(lockedPath);

        var files = new[] { validFile, lockedFile };

        // 删除之前先打开并持有一个文件流，导致删除失败
        using var fs = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        // 不合并异常，应立即抛出
        Assert.Throws<IOException>(() => files.Delete(false));

        // 验证第一个文件被删除了
        Assert.False(File.Exists(validPath));
        // 但被锁定的文件应该还在
        Assert.True(File.Exists(lockedPath));
    }

    [Fact]
    public void Delete_ShouldConsolidateExceptions_WhenMultipleErrorsOccur()
    {
        // 创建两个有效文件
        string validPath1 = CreateTestFile("valid1.txt");
        string validPath2 = CreateTestFile("valid2.txt");
        var validFile1 = new FileInfo(validPath1);
        var validFile2 = new FileInfo(validPath2);

        // 创建两个锁定的文件
        string lockedPath1 = CreateTestFile("locked1.txt");
        string lockedPath2 = CreateTestFile("locked2.txt");
        var lockedFile1 = new FileInfo(lockedPath1);
        var lockedFile2 = new FileInfo(lockedPath2);

        // 锁定两个文件，使删除操作失败
        using var fs1 = new FileStream(lockedPath1, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        using var fs2 = new FileStream(lockedPath2, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        // 组合所有文件
        var files = new[] { validFile1, lockedFile1, validFile2, lockedFile2 };

        // 合并异常，不应立即抛出
        var aggregateException = Assert.Throws<AggregateException>(() => files.Delete(true));
        
        // 验证聚合异常包含两个内部异常
        Assert.Equal(2, aggregateException.InnerExceptions.Count);
        
        // 验证有效文件被删除了
        Assert.False(File.Exists(validPath1));
        Assert.False(File.Exists(validPath2));
        
        // 验证锁定的文件仍然存在
        Assert.True(File.Exists(lockedPath1));
        Assert.True(File.Exists(lockedPath2));
    }

    [Fact]
    public void CopyTo_ShouldCopyMultipleFiles()
    {
        // 创建测试文件
        var filePaths = new[]
        {
            CreateTestFile("test1.txt", "Content 1"),
            CreateTestFile("test2.txt", "Content 2")
        };
        var fileInfos = filePaths.Select(p => new FileInfo(p)).ToArray();

        // 创建目标目录
        string targetDir = Path.Combine(_testDirectory, "target");
        Directory.CreateDirectory(targetDir);

        // 执行测试
        var copiedFiles = fileInfos.CopyTo(targetDir);

        // 验证
        Assert.Equal(2, copiedFiles.Length);
        for (int i = 0; i < filePaths.Length; i++)
        {
            // 原文件仍然存在
            Assert.True(File.Exists(filePaths[i]));

            // 复制的文件也存在
            string expectedTargetPath = Path.Combine(targetDir, Path.GetFileName(filePaths[i]));
            Assert.True(File.Exists(expectedTargetPath));

            // 内容相同
            string originalContent = File.ReadAllText(filePaths[i]);
            string copiedContent = File.ReadAllText(expectedTargetPath);
            Assert.Equal(originalContent, copiedContent);
        }
    }

    [Fact]
    public void CopyTo_ShouldThrowAggregateException_WhenErrorsOccur()
    {
        // 创建一个有效文件
        string validPath = CreateTestFile("valid_copy.txt", "Valid content");
        var validFile = new FileInfo(validPath);

        // 创建一个锁定的文件
        string lockedPath = CreateTestFile("locked_copy.txt", "Locked content");
        var lockedFile = new FileInfo(lockedPath);

        // 创建目标目录
        string targetDir = Path.Combine(_testDirectory, "copy_error_target");
        Directory.CreateDirectory(targetDir);

        // 锁定一个文件，使复制操作失败
        using var fs = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        // 组合文件
        var files = new[] { validFile, lockedFile };

        // 合并异常测试
        var aggregateException = Assert.Throws<AggregateException>(() => files.CopyTo(targetDir, true));
        
        // 验证聚合异常包含一个内部异常
        Assert.Single(aggregateException.InnerExceptions);
        
        // 验证有效文件被成功复制
        string validTargetPath = Path.Combine(targetDir, "valid_copy.txt");
        Assert.True(File.Exists(validTargetPath));
        
        // 验证锁定的文件未被复制
        string lockedTargetPath = Path.Combine(targetDir, "locked_copy.txt");
        Assert.False(File.Exists(lockedTargetPath));
    }

    [Fact]
    public void CopyTo_ShouldThrowDirectly_WhenConsolidateExceptionsIsFalse()
    {
        // 创建有效和锁定的文件
        string validPath = CreateTestFile("valid_direct.txt", "Valid content");
        string lockedPath = CreateTestFile("locked_direct.txt", "Locked content");
        var validFile = new FileInfo(validPath);
        var lockedFile = new FileInfo(lockedPath);

        // 创建目标目录
        string targetDir = Path.Combine(_testDirectory, "copy_direct_target");
        Directory.CreateDirectory(targetDir);

        // 锁定一个文件，使复制操作失败
        using var fs = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        // 组合文件，有效文件放在后面，确保先处理锁定文件
        var files = new[] { lockedFile, validFile };

        // 不合并异常，应立即抛出
        Assert.Throws<IOException>(() => files.CopyTo(targetDir, false));
        
        // 验证没有文件被复制，因为第一个文件就失败了
        Assert.False(File.Exists(Path.Combine(targetDir, "valid_direct.txt")));
        Assert.False(File.Exists(Path.Combine(targetDir, "locked_direct.txt")));
    }

    [Fact]
    public void MoveTo_ShouldMoveMultipleFiles()
    {
        // 创建测试文件
        var filePaths = new[]
        {
            CreateTestFile("move1.txt", "Content 1"),
            CreateTestFile("move2.txt", "Content 2")
        };
        var fileInfos = filePaths.Select(p => new FileInfo(p)).ToArray();

        // 创建目标目录
        string targetDir = Path.Combine(_testDirectory, "moveTarget");
        Directory.CreateDirectory(targetDir);

        // 执行测试
        var movedFiles = fileInfos.MoveTo(targetDir);

        // 验证
        Assert.Equal(2, movedFiles.Length);
        for (int i = 0; i < filePaths.Length; i++)
        {
            // 原文件不再存在
            Assert.False(File.Exists(filePaths[i]));

            // 移动的文件存在
            string expectedTargetPath = Path.Combine(targetDir, Path.GetFileName(filePaths[i]));
            Assert.True(File.Exists(expectedTargetPath));
        }
    }

    [Fact]
    public void MoveTo_ShouldThrowAggregateException_WhenErrorsOccur()
    {
        // 创建一个有效文件
        string validPath = CreateTestFile("valid_move.txt", "Valid content");
        var validFile = new FileInfo(validPath);

        // 创建一个锁定的文件
        string lockedPath = CreateTestFile("locked_move.txt", "Locked content");
        var lockedFile = new FileInfo(lockedPath);

        // 创建目标目录
        string targetDir = Path.Combine(_testDirectory, "move_error_target");
        Directory.CreateDirectory(targetDir);

        // 锁定一个文件，使移动操作失败
        using var fs = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        // 组合文件
        var files = new[] { validFile, lockedFile };

        // 合并异常测试
        var aggregateException = Assert.Throws<AggregateException>(() => files.MoveTo(targetDir, true));
        
        // 验证聚合异常包含一个内部异常
        Assert.Single(aggregateException.InnerExceptions);
        
        // 验证有效文件被成功移动
        Assert.False(File.Exists(validPath));
        Assert.True(File.Exists(Path.Combine(targetDir, "valid_move.txt")));
        
        // 验证锁定的文件未被移动
        Assert.True(File.Exists(lockedPath));
        Assert.False(File.Exists(Path.Combine(targetDir, "locked_move.txt")));
    }

    [Fact]
    public void MoveTo_ShouldThrowDirectly_WhenConsolidateExceptionsIsFalse()
    {
        // 创建有效和锁定的文件
        string validPath = CreateTestFile("valid_move_direct.txt", "Valid content");
        string lockedPath = CreateTestFile("locked_move_direct.txt", "Locked content");
        var validFile = new FileInfo(validPath);
        var lockedFile = new FileInfo(lockedPath);

        // 创建目标目录
        string targetDir = Path.Combine(_testDirectory, "move_direct_target");
        Directory.CreateDirectory(targetDir);

        // 锁定一个文件，使移动操作失败
        using var fs = new FileStream(lockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        // 组合文件，锁定文件放在前面，确保先处理
        var files = new[] { lockedFile, validFile };

        // 不合并异常，应立即抛出
        Assert.Throws<IOException>(() => files.MoveTo(targetDir, false));
        
        // 验证所有文件仍在原位置
        Assert.True(File.Exists(validPath));
        Assert.True(File.Exists(lockedPath));
        
        // 验证没有文件被移动到目标目录
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
        // 创建一个指定大小的文件
        string content = new string('A', 1000); // 1000 个字符
        string filePath = CreateTestFile("size.txt", content);

        // 执行测试
        long size = filePath.GetFileSize();

        // 验证
        Assert.Equal(1000, size);
    }

    [Fact]
    public void GetFileSizeFormatted_ShouldReturnFormattedSize()
    {
        // 创建一个指定大小的文件
        string content = new string('A', 1024); // 1KB 的内容
        string filePath = CreateTestFile("formatted_size.txt", content);

        // 执行测试
        string formattedSize = filePath.GetFileSizeFormatted();

        // 验证
        Assert.Equal("1KB", formattedSize);
    }

    [Fact]
    public void GetFileSizeFormatted_FileInfo_ShouldReturnFormattedSize()
    {
        // 创建一个指定大小的文件
        string content = new string('A', 1024); // 1KB 的内容
        string filePath = CreateTestFile("fileinfo_size.txt", content);
        var fileInfo = new FileInfo(filePath);

        // 执行测试
        string formattedSize = fileInfo.GetFileSizeFormatted();

        // 验证
        Assert.Equal("1KB", formattedSize);
    }

    [Fact]
    public void ToMemoryStream3_ShouldCreateMemoryStreamWithSameContent()
    {
        // 创建测试文件
        string content = "This is a test for memory stream conversion";
        string filePath = CreateTestFile("memory_stream.txt", content);
        var fileInfo = new FileInfo(filePath);

        // 执行测试
        using var memoryStream = fileInfo.ToMemoryStream3();

        // 验证
        memoryStream.Position = 0;
        using var reader = new StreamReader(memoryStream);
        string result = reader.ReadToEnd();
        Assert.Equal(content, result);
    }

    [Fact]
    public async Task GetFileDataAsync_ShouldReturnCorrectByteArray()
    {
        // 创建测试文件
        string content = "This is a test for GetFileDataAsync method";
        string filePath = CreateTestFile("file_data_async.txt", content);

        // 执行测试
        byte[] result = await filePath.GetFileDataAsync();

        // 验证
        byte[] expected = Encoding.UTF8.GetBytes(content);
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetFileDataAsync_ShouldThrowException_ForNonExistentFile()
    {
        string nonExistentPath = Path.Combine(_testDirectory, "non_existent_file.txt");
        
        // 验证抛出异常
        await Assert.ThrowsAsync<FileNotFoundException>(() => nonExistentPath.GetFileDataAsync());
    }

    [Fact]
    public void ComputeHashMd5_ShouldReturnCorrectHash()
    {
        // 创建测试文件
        string content = "Test content for MD5 hash";
        string filePath = CreateTestFile("md5.txt", content);
        var fileInfo = new FileInfo(filePath);

        // 直接计算预期的 MD5 哈希
        byte[] contentBytes = Encoding.UTF8.GetBytes(content);
        using var md5 = MD5.Create();
        byte[] expectedHashBytes = md5.ComputeHash(contentBytes);
        string expectedHash = BitConverter.ToString(expectedHashBytes).Replace("-", "");

        // 执行测试
        string hash = fileInfo.ComputeHashMd5();

        // 验证
        Assert.Equal(expectedHash, hash);
    }

    [Fact]
    public void GetFileVersion_ShouldReturnVersion_ForAssemblyFile()
    {
        // 获取当前正在执行的程序集的位置
        var currentAssembly = typeof(FileInfoExtensionsTests).Assembly.Location;
        var fileInfo = new FileInfo(currentAssembly);

        // 执行测试
        string? version = fileInfo.GetFileVersion();

        // 验证 - 应该返回一个版本号，而不是 null
        Assert.NotNull(version);
    }

    [Fact]
    public void GetFileVersion_ShouldThrowException_ForNonExistentFile()
    {
        var fileInfo = new FileInfo(Path.Combine(_testDirectory, "nonexistent.dll"));
        Assert.Throws<FileNotFoundException>(() => fileInfo.GetFileVersion());
    }
}