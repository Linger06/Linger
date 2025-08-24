using System.Security.Cryptography;
using System.Text;
using Linger.Extensions.IO;
using Linger.Helper;

namespace Linger.UnitTests.Extensions.IO;

public class StreamExtensionsTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly List<string> _createdFiles = new();

    public StreamExtensionsTests()
    {
        // 创建一个临时目录进行测试
        _testDirectory = Path.Combine(Path.GetTempPath(), "StreamExtensionsTests_" + Guid.NewGuid().ToString("N"));
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

    private string GetTestFilePath(string fileName)
    {
        string filePath = Path.Combine(_testDirectory, fileName);
        _createdFiles.Add(filePath);
        return filePath;
    }

    [Fact]
    public void ToMd5HashByte_ShouldCalculateCorrectHash()
    {
        // 准备测试数据
        string testData = "Test data for MD5 calculation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // 计算预期的 MD5 哈希
        using var md5 = MD5.Create();
        byte[] expectedHash = md5.ComputeHash(testBytes);

        // 执行测试
        memoryStream.Position = 0;
        byte[] actualHash = memoryStream.ToMd5HashByte();

        // 验证
        Assert.Equal(expectedHash, actualHash);
    }

    [Fact]
    public void ToMd5Hash_ShouldCalculateCorrectHashString()
    {
        // 准备测试数据
        string testData = "Test data for MD5 string calculation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // 计算预期的 MD5 哈希
        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(testBytes);
        var sb = new StringBuilder();
        foreach (byte b in hashBytes)
        {
            sb.Append(b.ToString("X2"));
        }
        string expectedHash = sb.ToString();

        // 执行测试
        memoryStream.Position = 0;
#pragma warning disable CS0618 // 类型或成员已过时
        string actualHash = memoryStream.ToMd5Hash();
#pragma warning restore CS0618 // 类型或成员已过时

        // 验证
        Assert.Equal(expectedHash, actualHash);
    }

    [Fact]
    public void ComputeHashMd5_ShouldCalculateCorrectHash()
    {
        // 准备测试数据
        string testData = "Test data for ComputeHashMd5";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // 计算预期的 MD5 哈希
        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(testBytes);
        string expectedHash = BitConverter.ToString(hashBytes).Replace("-", "");

        // 执行测试
        memoryStream.Position = 0;
        string actualHash = memoryStream.ComputeHashMd5();

        // 验证
        Assert.Equal(expectedHash, actualHash);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public async Task ToMd5HashByteAsync_ShouldCalculateCorrectHash()
    {
        // 准备测试数据
        string testData = "Test data for MD5 async calculation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // 计算预期的 MD5 哈希
        using var md5 = MD5.Create();
        byte[] expectedHash = md5.ComputeHash(testBytes);

        // 执行测试
        memoryStream.Position = 0;
        byte[] actualHash = await memoryStream.ToMd5HashByteAsync();

        // 验证
        Assert.Equal(expectedHash, actualHash);
    }
#endif

    [Fact]
    public void ToFile_ShouldWriteStreamContentToFile()
    {
        // 准备测试数据
        string testData = "This is test content for file writing";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);
        string outputFilePath = GetTestFilePath("stream_output.txt");

        // 执行测试
        memoryStream.ToFile(outputFilePath);

        // 验证
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public void ToFile_MemoryStream_ShouldWriteToFile()
    {
        // 准备测试数据
        string testData = "This is a test for MemoryStream.ToFile method";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);
        string outputFilePath = GetTestFilePath("memory_stream_output.txt");

        // 执行测试
        memoryStream.Position = 0;
        memoryStream.ToFile(outputFilePath);

        // 验证
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public void ToFile_ShouldCreateDirectoriesIfNeeded()
    {
        // 准备测试数据
        string testData = "Test content for directory creation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // 创建一个嵌套的路径
        string nestedDir = Path.Combine(_testDirectory, "nested", "dirs");
        string outputFilePath = Path.Combine(nestedDir, "output.txt");
        _createdFiles.Add(outputFilePath);

        // 执行测试 - 应该创建必要的目录
        memoryStream.ToFile(outputFilePath);

        // 验证
        Assert.True(Directory.Exists(nestedDir));
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public async Task ToFileAsync_ShouldWriteMemoryStreamToFile()
    {
        // 准备测试数据
        string testData = "This is async test content";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);
        string outputFilePath = GetTestFilePath("async_output.txt");

        // 执行测试
        await memoryStream.ToFileAsync(outputFilePath);

        // 验证
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public async Task ToFileAsync_ShouldCreateDirectoriesIfNeeded()
    {
        // 准备测试数据
        string testData = "Test content for async directory creation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // 创建一个嵌套的路径
        string nestedDir = Path.Combine(_testDirectory, "nested", "async", "dirs");
        string outputFilePath = Path.Combine(nestedDir, "async_output.txt");
        _createdFiles.Add(outputFilePath);

        // 执行测试 - 应该创建必要的目录
        await memoryStream.ToFileAsync(outputFilePath);

        // 验证
        Assert.True(Directory.Exists(nestedDir));
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public void ToFile_WithEmptyStream_ShouldCreateEmptyFile()
    {
        // 创建空流
        using var emptyStream = new MemoryStream();
        string outputFilePath = GetTestFilePath("empty_stream_output.txt");

        // 执行测试
        emptyStream.ToFile(outputFilePath);

        // 验证
        Assert.True(File.Exists(outputFilePath));
        Assert.Equal(0, new FileInfo(outputFilePath).Length);
    }

    [Fact]
    public void ToFile_WithLargeStream_ShouldWriteCorrectly()
    {
        // 创建一个较大的流 (1MB)
        int sizeMB = 1;
        int size = sizeMB * 1024 * 1024;
        byte[] largeData = new byte[size];

        // 填充一些随机数据
        var random = new Random(42); // 使用固定种子以便测试可重复
        random.NextBytes(largeData);

        using var largeStream = new MemoryStream(largeData);
        string outputFilePath = GetTestFilePath("large_stream_output.bin");

        // 执行测试
        largeStream.ToFile(outputFilePath);

        // 验证
        Assert.True(File.Exists(outputFilePath));
        Assert.Equal(size, new FileInfo(outputFilePath).Length);

        // 验证文件内容与原始数据匹配
        byte[] fileContent = File.ReadAllBytes(outputFilePath);
        Assert.Equal(largeData, fileContent);
    }

    [Fact]
    public void ToFile_WithExistingFile_ShouldOverwrite()
    {
        // 先创建一个已存在的文件
        string outputFilePath = GetTestFilePath("existing_file.txt");
        File.WriteAllText(outputFilePath, "Original content");

        // 准备新内容
        string newContent = "New content that should overwrite the original";
        byte[] newContentBytes = Encoding.UTF8.GetBytes(newContent);
        using var stream = new MemoryStream(newContentBytes);

        // 执行测试
        stream.ToFile(outputFilePath);

        // 验证文件被覆盖
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(newContent, fileContent);
    }

    [Fact]
    public void ToFile_WithNonSeekableStream_ShouldWorkCorrectly()
    {
        // 创建一个不可查找的流
        string testData = "This is test content for non-seekable stream";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);

        // 使用NetworkStream模拟不可查找流 (创建一对连接的内存流)
        using var sourceStream = new MemoryStream(testBytes);
        using var nonSeekableStream = new NonSeekableStreamWrapper(sourceStream);

        string outputFilePath = GetTestFilePath("non_seekable_output.txt");

        // 执行测试
        nonSeekableStream.ToFile(outputFilePath);

        // 验证
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public void ToFile_WithInvalidPath_ShouldThrowException()
    {
        // 准备测试数据
        string testData = "Test content for invalid path test";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // 验证异常被抛出
        Assert.Throws<System.ArgumentException>(() => memoryStream.ToFile(Path.Combine(_testDirectory, "invalid_chars", "?*:|<>\0", "file.txt")));
    }

    [Fact]
    public void ToFile_WithReadOnlyFile_ShouldThrowException()
    {
        // 创建一个文件并设置为只读
        string filePath = GetTestFilePath("readonly_file.txt");
        File.WriteAllText(filePath, "Initial content");
        File.SetAttributes(filePath, FileAttributes.ReadOnly);

        try
        {
            // 验证文件是只读的
            Assert.True((File.GetAttributes(filePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly,
                "文件未成功设置为只读模式");

            // 准备测试数据
            string testData = "New content for read-only file";
            byte[] testBytes = Encoding.UTF8.GetBytes(testData);
            using var memoryStream = new MemoryStream(testBytes);

            // 尝试写入只读文件，应该抛出异常
            Assert.Throws<UnauthorizedAccessException>(() => memoryStream.ToFile(filePath));
        }
        finally
        {
            // 恢复文件属性以便清理
            File.SetAttributes(filePath, FileAttributes.Normal);
        }
    }

    [Fact]
    public void ToFile_WithStreamPositionAtEnd_ShouldSeekToBeginningAndWriteCorrectly()
    {
        // 准备测试数据
        string testData = "This is test content for stream position test";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // 将流位置移动到末尾
        memoryStream.Position = memoryStream.Length;

        string outputFilePath = GetTestFilePath("stream_position_test.txt");

        // 执行测试
        memoryStream.ToFile(outputFilePath);

        // 验证
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    #region Helper methods

    private bool IsRunningAsAdmin()
    {
        try
        {
            // 尝试创建一个系统目录下的文件
            string adminTestPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                                               $"test_{Guid.NewGuid()}.tmp");
            using (FileStream fs = File.Create(adminTestPath, 1, FileOptions.DeleteOnClose))
            {
                fs.WriteByte(0);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void SetDirectoryReadOnly(string directoryPath)
    {
        var dirInfo = new DirectoryInfo(directoryPath);
        dirInfo.Attributes |= FileAttributes.ReadOnly;
    }

    private void SetDirectoryWritable(string directoryPath)
    {
        var dirInfo = new DirectoryInfo(directoryPath);
        dirInfo.Attributes &= ~FileAttributes.ReadOnly;
    }

    #endregion
}

/// <summary>
/// 不可查找流的包装器类，用于测试
/// </summary>
internal class NonSeekableStreamWrapper : Stream
{
    private readonly Stream _innerStream;

    public NonSeekableStreamWrapper(Stream innerStream)
    {
        _innerStream = innerStream;
    }

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => false; // 强制设为不可查找
    public override bool CanWrite => _innerStream.CanWrite;
    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set => throw new NotSupportedException("This stream does not support seeking");
    }

    public override void Flush() => _innerStream.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);
    public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException("This stream does not support seeking");
    public override void SetLength(long value) => _innerStream.SetLength(value);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _innerStream.Dispose();
        }
        base.Dispose(disposing);
    }
}
