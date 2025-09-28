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
        // ����һ����ʱĿ¼���в���
        _testDirectory = Path.Combine(Path.GetTempPath(), "StreamExtensionsTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        // ���������������ļ�
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
                // �������������еĴ���
            }
        }

        // ����ɾ������Ŀ¼
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch
        {
            // �������������еĴ���
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
        // ׼����������
        string testData = "Test data for MD5 calculation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // ����Ԥ�ڵ� MD5 ��ϣ
        using var md5 = MD5.Create();
        byte[] expectedHash = md5.ComputeHash(testBytes);

        // ִ�в���
        memoryStream.Position = 0;
        byte[] actualHash = memoryStream.ToMd5HashByte();

        // ��֤
        Assert.Equal(expectedHash, actualHash);
    }

    [Fact]
    public void ToMd5Hash_ShouldCalculateCorrectHashString()
    {
        // ׼����������
        string testData = "Test data for MD5 string calculation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // ����Ԥ�ڵ� MD5 ��ϣ
        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(testBytes);
        var sb = new StringBuilder();
        foreach (byte b in hashBytes)
        {
            sb.Append(b.ToString("X2"));
        }
        string expectedHash = sb.ToString();

        // ִ�в���
        memoryStream.Position = 0;
#pragma warning disable CS0618 // ���ͻ��Ա�ѹ�ʱ
        string actualHash = memoryStream.ToMd5Hash();
#pragma warning restore CS0618 // ���ͻ��Ա�ѹ�ʱ

        // ��֤
        Assert.Equal(expectedHash, actualHash);
    }

    [Fact]
    public void ComputeHashMd5_ShouldCalculateCorrectHash()
    {
        // ׼����������
        string testData = "Test data for ComputeHashMd5";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // ����Ԥ�ڵ� MD5 ��ϣ
        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(testBytes);
    string expectedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        // ִ�в���
        memoryStream.Position = 0;
        string actualHash = memoryStream.ComputeHashMd5();

        // ��֤
        Assert.Equal(expectedHash, actualHash);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public async Task ToMd5HashByteAsync_ShouldCalculateCorrectHash()
    {
        // ׼����������
        string testData = "Test data for MD5 async calculation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // ����Ԥ�ڵ� MD5 ��ϣ
        using var md5 = MD5.Create();
        byte[] expectedHash = md5.ComputeHash(testBytes);

        // ִ�в���
        memoryStream.Position = 0;
        byte[] actualHash = await memoryStream.ToMd5HashByteAsync();

        // ��֤
        Assert.Equal(expectedHash, actualHash);
    }
#endif

    [Fact]
    public void ToFile_ShouldWriteStreamContentToFile()
    {
        // ׼����������
        string testData = "This is test content for file writing";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);
        string outputFilePath = GetTestFilePath("stream_output.txt");

        // ִ�в���
        memoryStream.ToFile(outputFilePath);

        // ��֤
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public void ToFile_MemoryStream_ShouldWriteToFile()
    {
        // ׼����������
        string testData = "This is a test for MemoryStream.ToFile method";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);
        string outputFilePath = GetTestFilePath("memory_stream_output.txt");

        // ִ�в���
        memoryStream.Position = 0;
        memoryStream.ToFile(outputFilePath);

        // ��֤
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public void ToFile_ShouldCreateDirectoriesIfNeeded()
    {
        // ׼����������
        string testData = "Test content for directory creation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // ����һ��Ƕ�׵�·��
        string nestedDir = Path.Combine(_testDirectory, "nested", "dirs");
        string outputFilePath = Path.Combine(nestedDir, "output.txt");
        _createdFiles.Add(outputFilePath);

        // ִ�в��� - Ӧ�ô�����Ҫ��Ŀ¼
        memoryStream.ToFile(outputFilePath);

        // ��֤
        Assert.True(Directory.Exists(nestedDir));
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public async Task ToFileAsync_ShouldWriteMemoryStreamToFile()
    {
        // ׼����������
        string testData = "This is async test content";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);
        string outputFilePath = GetTestFilePath("async_output.txt");

        // ִ�в���
        await memoryStream.ToFileAsync(outputFilePath);

        // ��֤
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public async Task ToFileAsync_ShouldCreateDirectoriesIfNeeded()
    {
        // ׼����������
        string testData = "Test content for async directory creation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // ����һ��Ƕ�׵�·��
        string nestedDir = Path.Combine(_testDirectory, "nested", "async", "dirs");
        string outputFilePath = Path.Combine(nestedDir, "async_output.txt");
        _createdFiles.Add(outputFilePath);

        // ִ�в��� - Ӧ�ô�����Ҫ��Ŀ¼
        await memoryStream.ToFileAsync(outputFilePath);

        // ��֤
        Assert.True(Directory.Exists(nestedDir));
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public void ToFile_WithEmptyStream_ShouldCreateEmptyFile()
    {
        // ��������
        using var emptyStream = new MemoryStream();
        string outputFilePath = GetTestFilePath("empty_stream_output.txt");

        // ִ�в���
        emptyStream.ToFile(outputFilePath);

        // ��֤
        Assert.True(File.Exists(outputFilePath));
        Assert.Equal(0, new FileInfo(outputFilePath).Length);
    }

    [Fact]
    public void ToFile_WithLargeStream_ShouldWriteCorrectly()
    {
        // ����һ���ϴ���� (1MB)
        int sizeMB = 1;
        int size = sizeMB * 1024 * 1024;
        byte[] largeData = new byte[size];

        // ���һЩ�������
        var random = new Random(42); // ʹ�ù̶������Ա���Կ��ظ�
        random.NextBytes(largeData);

        using var largeStream = new MemoryStream(largeData);
        string outputFilePath = GetTestFilePath("large_stream_output.bin");

        // ִ�в���
        largeStream.ToFile(outputFilePath);

        // ��֤
        Assert.True(File.Exists(outputFilePath));
        Assert.Equal(size, new FileInfo(outputFilePath).Length);

        // ��֤�ļ�������ԭʼ����ƥ��
        byte[] fileContent = File.ReadAllBytes(outputFilePath);
        Assert.Equal(largeData, fileContent);
    }

    [Fact]
    public void ToFile_WithExistingFile_ShouldOverwrite()
    {
        // �ȴ���һ���Ѵ��ڵ��ļ�
        string outputFilePath = GetTestFilePath("existing_file.txt");
        File.WriteAllText(outputFilePath, "Original content");

        // ׼��������
        string newContent = "New content that should overwrite the original";
        byte[] newContentBytes = Encoding.UTF8.GetBytes(newContent);
        using var stream = new MemoryStream(newContentBytes);

        // ִ�в���
        stream.ToFile(outputFilePath);

        // ��֤�ļ�������
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(newContent, fileContent);
    }

    [Fact]
    public void ToFile_WithNonSeekableStream_ShouldWorkCorrectly()
    {
        // ����һ�����ɲ��ҵ���
        string testData = "This is test content for non-seekable stream";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);

        // ʹ��NetworkStreamģ�ⲻ�ɲ����� (����һ�����ӵ��ڴ���)
        using var sourceStream = new MemoryStream(testBytes);
        using var nonSeekableStream = new NonSeekableStreamWrapper(sourceStream);

        string outputFilePath = GetTestFilePath("non_seekable_output.txt");

        // ִ�в���
        nonSeekableStream.ToFile(outputFilePath);

        // ��֤
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    [Fact]
    public void ToFile_WithInvalidPath_ShouldThrowException()
    {
        // ׼����������
        string testData = "Test content for invalid path test";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // ��֤�쳣���׳�
        Assert.Throws<System.ArgumentException>(() => memoryStream.ToFile(Path.Combine(_testDirectory, "invalid_chars", "?*:|<>\0", "file.txt")));
    }

    [Fact]
    public void ToFile_WithReadOnlyFile_ShouldThrowException()
    {
        // ����һ���ļ�������Ϊֻ��
        string filePath = GetTestFilePath("readonly_file.txt");
        File.WriteAllText(filePath, "Initial content");
        File.SetAttributes(filePath, FileAttributes.ReadOnly);

        try
        {
            // ��֤�ļ���ֻ����
            Assert.True((File.GetAttributes(filePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly,
                "�ļ�δ�ɹ�����Ϊֻ��ģʽ");

            // ׼����������
            string testData = "New content for read-only file";
            byte[] testBytes = Encoding.UTF8.GetBytes(testData);
            using var memoryStream = new MemoryStream(testBytes);

            // ����д��ֻ���ļ���Ӧ���׳��쳣
            Assert.Throws<UnauthorizedAccessException>(() => memoryStream.ToFile(filePath));
        }
        finally
        {
            // �ָ��ļ������Ա�����
            File.SetAttributes(filePath, FileAttributes.Normal);
        }
    }

    [Fact]
    public void ToFile_WithStreamPositionAtEnd_ShouldSeekToBeginningAndWriteCorrectly()
    {
        // ׼����������
        string testData = "This is test content for stream position test";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        // ����λ���ƶ���ĩβ
        memoryStream.Position = memoryStream.Length;

        string outputFilePath = GetTestFilePath("stream_position_test.txt");

        // ִ�в���
        memoryStream.ToFile(outputFilePath);

        // ��֤
        Assert.True(File.Exists(outputFilePath));
        string fileContent = File.ReadAllText(outputFilePath);
        Assert.Equal(testData, fileContent);
    }

    #region Helper methods

    private bool IsRunningAsAdmin()
    {
        try
        {
            // ���Դ���һ��ϵͳĿ¼�µ��ļ�
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
/// ���ɲ������İ�װ���࣬���ڲ���
/// </summary>
internal class NonSeekableStreamWrapper : Stream
{
    private readonly Stream _innerStream;

    public NonSeekableStreamWrapper(Stream innerStream)
    {
        _innerStream = innerStream;
    }

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => false; // ǿ����Ϊ���ɲ���
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
