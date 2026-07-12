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
        _testDirectory = Path.Combine(Path.GetTempPath(), "StreamExtensionsTests_" + Guid.NewGuid().ToString("N"));
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

    private string GetTestFilePath(string fileName)
    {
        string filePath = Path.Combine(_testDirectory, fileName);
        _createdFiles.Add(filePath);
        return filePath;
    }

    [Fact]
    public void ToMd5HashByte_ShouldCalculateCorrectHash()
    {
        string testData = "Test data for MD5 calculation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        using var md5 = MD5.Create();
        byte[] expectedHash = md5.ComputeHash(testBytes);

        memoryStream.Position = 0;
        byte[] actualHash = memoryStream.ToMd5HashByte();

        Assert.Equal(expectedHash, actualHash);
    }

    [Fact]
    public void ComputeHashMd5_ShouldCalculateCorrectHash()
    {
        string testData = "Test data for ComputeHashMd5";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(testBytes);
        string expectedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        memoryStream.Position = 0;
        string actualHash = memoryStream.ComputeHashMd5();

        Assert.Equal(expectedHash, actualHash);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public async Task ToMd5HashByteAsync_ShouldCalculateCorrectHash()
    {
        string testData = "Test data for MD5 async calculation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        using var md5 = MD5.Create();
        byte[] expectedHash = md5.ComputeHash(testBytes);

        memoryStream.Position = 0;
        byte[] actualHash = await memoryStream.ToMd5HashByteAsync();

        Assert.Equal(expectedHash, actualHash);
    }
#endif

    [Fact]
    public void ToFile_ShouldWriteStreamContentToFile()
    {
        string testData = "This is test content for file writing";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);
        string outputFilePath = GetTestFilePath("stream_output.txt");

        memoryStream.ToFile(outputFilePath);

        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public void ToFile_MemoryStream_ShouldWriteToFile()
    {
        string testData = "This is a test for MemoryStream.ToFile method";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);
        string outputFilePath = GetTestFilePath("memory_stream_output.txt");

        memoryStream.Position = 0;
        memoryStream.ToFile(outputFilePath);

        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public void ToFile_ShouldCreateDirectoriesIfNeeded()
    {
        string testData = "Test content for directory creation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        string nestedDir = Path.Combine(_testDirectory, "nested", "dirs");
        string outputFilePath = Path.Combine(nestedDir, "output.txt");
        _createdFiles.Add(outputFilePath);

        memoryStream.ToFile(outputFilePath);

        Assert.True(Directory.Exists(nestedDir));
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public async Task ToFileAsync_ShouldWriteMemoryStreamToFile()
    {
        string testData = "This is async test content";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);
        string outputFilePath = GetTestFilePath("async_output.txt");

        await memoryStream.ToFileAsync(outputFilePath);

        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public async Task ToFileAsync_ShouldCreateDirectoriesIfNeeded()
    {
        string testData = "Test content for async directory creation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        string nestedDir = Path.Combine(_testDirectory, "nested", "async", "dirs");
        string outputFilePath = Path.Combine(nestedDir, "async_output.txt");
        _createdFiles.Add(outputFilePath);

        await memoryStream.ToFileAsync(outputFilePath);

        Assert.True(Directory.Exists(nestedDir));
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public void ToFile_WithEmptyStream_ShouldCreateEmptyFile()
    {
        using var emptyStream = new MemoryStream();
        string outputFilePath = GetTestFilePath("empty_stream_output.txt");

        emptyStream.ToFile(outputFilePath);

        Assert.True(File.Exists(outputFilePath));
        Assert.Equal(0, new FileInfo(outputFilePath).Length);
    }

    [Fact]
    public void ToFile_WithLargeStream_ShouldWriteCorrectly()
    {
        int sizeMB = 1;
        int size = sizeMB * 1024 * 1024;
        byte[] largeData = new byte[size];

        var random = new Random(42);
        random.NextBytes(largeData);

        using var largeStream = new MemoryStream(largeData);
        string outputFilePath = GetTestFilePath("large_stream_output.bin");

        largeStream.ToFile(outputFilePath);

        Assert.True(File.Exists(outputFilePath));
        Assert.Equal(size, new FileInfo(outputFilePath).Length);

        byte[] fileContent = File.ReadAllBytes(outputFilePath);
        Assert.Equal(largeData, fileContent);
    }

    [Fact]
    public void ToFile_WithExistingFile_ShouldOverwrite()
    {
        string outputFilePath = GetTestFilePath("existing_file.txt");
        File.WriteAllText(outputFilePath, "Original content");

        string newContent = "New content that should overwrite the original";
        byte[] newContentBytes = Encoding.UTF8.GetBytes(newContent);
        using var stream = new MemoryStream(newContentBytes);

        stream.ToFile(outputFilePath);

        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(newContent, fileContent);
    }

    [Fact]
    public void ToFile_WithNonSeekableStream_ShouldWorkCorrectly()
    {
        string testData = "This is test content for non-seekable stream";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);

        using var sourceStream = new MemoryStream(testBytes);
        using var nonSeekableStream = new NonSeekableStreamWrapper(sourceStream);

        string outputFilePath = GetTestFilePath("non_seekable_output.txt");

        nonSeekableStream.ToFile(outputFilePath);

        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public void ToFile_WithInvalidPath_ShouldThrowException()
    {
        string testData = "Test content for invalid path test";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        Assert.Throws<ArgumentException>(() => memoryStream.ToFile(Path.Combine(_testDirectory, "invalid_chars", "?*:|<>\0", "file.txt")));
    }

    [Fact]
    public void ToFile_WithReadOnlyFile_ShouldThrowException()
    {
        string filePath = GetTestFilePath("readonly_file.txt");
        File.WriteAllText(filePath, "Initial content");
        File.SetAttributes(filePath, FileAttributes.ReadOnly);

        try
        {
            Assert.True(
                (File.GetAttributes(filePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly,
                "File should be marked as read-only before the write test runs");

            string testData = "New content for read-only file";
            byte[] testBytes = Encoding.UTF8.GetBytes(testData);
            using var memoryStream = new MemoryStream(testBytes);

            Assert.Throws<UnauthorizedAccessException>(() => memoryStream.ToFile(filePath));
        }
        finally
        {
            File.SetAttributes(filePath, FileAttributes.Normal);
        }
    }

    [Fact]
    public void ToFile_WithStreamPositionAtEnd_ShouldSeekToBeginningAndWriteCorrectly()
    {
        string testData = "This is test content for stream position test";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        memoryStream.Position = memoryStream.Length;

        string outputFilePath = GetTestFilePath("stream_position_test.txt");

        memoryStream.ToFile(outputFilePath);

        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    #region Helper methods

    private bool IsRunningAsAdmin()
    {
        try
        {
            string adminTestPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
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
/// Wraps a stream and disables seeking for test coverage.
/// </summary>
internal class NonSeekableStreamWrapper : Stream
{
    private readonly Stream _innerStream;

    public NonSeekableStreamWrapper(Stream innerStream)
    {
        _innerStream = innerStream;
    }

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => false;
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
