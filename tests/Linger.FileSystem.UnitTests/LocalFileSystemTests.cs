using System.Text;
using Linger.Exceptions;
using Linger.FileSystem.Exceptions;
using Linger.FileSystem.Local;
using Xunit;

namespace Linger.FileSystem.Tests.Local
{
    public class LocalFileSystemTests : IDisposable
    {
        private readonly string _testRootPath;
        private readonly LocalFileSystem _fileSystem;

        public LocalFileSystemTests()
        {
            // 设置测试根目录
            _testRootPath = Path.Combine("TestTempDir", $"testDir-{Guid.NewGuid().ToString()}");
            _fileSystem = new LocalFileSystem(_testRootPath);

            // 确保测试目录存在且为空
            if (Directory.Exists(_testRootPath))
                Directory.Delete(_testRootPath, true);
            Directory.CreateDirectory(_testRootPath);
        }

        public void Dispose()
        {
            // 清理测试目录
            if (Directory.Exists(_testRootPath))
                Directory.Delete(_testRootPath, true);
        }

        [Fact]
        public void Exists_WhenDirectoryExists_ReturnsTrue()
        {
            // Act
            var result = _fileSystem.Exists();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CreateIfNotExists_CreatesDirectory()
        {
            // Arrange
            Directory.Delete(_testRootPath, true);

            // Act
            _fileSystem.CreateIfNotExists();

            // Assert
            Assert.True(Directory.Exists(_testRootPath));
        }

        [Fact]
        public async Task UploadAsync_WithValidStream_UploadsFile()
        {
            // Arrange
            var content = "Test Content";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            var result = await _fileSystem.UploadAsync(
                stream,
                "test.txt",
                "container1",
              namingRule: NamingRule.Normal);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result.RelativeFilePath));
            Assert.Equal("test.txt", result.FileName);
        }

        [Fact]
        public void DeleteAsync_WhenFileExists_DeletesFile()
        {
            // Arrange
            var filePath = Path.Combine(_testRootPath, "test.txt");
            File.WriteAllText(filePath, "test");

            // Act
            _fileSystem.DeleteAsync("test.txt");

            // Assert
            Assert.False(File.Exists(filePath));
        }

        [Fact]
        public async Task DownloadAsync_WhenFileExists_DownloadsFile()
        {
            // Arrange
            var sourceContent = "Test Content";
            var sourcePath = Path.Combine(_testRootPath, "source.txt");
            File.WriteAllText(sourcePath, sourceContent);

            // Act
            var destPath = await _fileSystem.DownloadAsync(
                "source.txt",
                "dest.txt");

            // Assert
            Assert.True(File.Exists(destPath));
            Assert.Equal(sourceContent, File.ReadAllText(destPath));
        }

        [Fact]
        public async Task DownloadToStreamAsync_WhenFileExists_CopiesContent()
        {
            // Arrange
            var sourceContent = "Test Content";
            var sourcePath = Path.Combine(_testRootPath, "source.txt");
            File.WriteAllText(sourcePath, sourceContent);
            using var destStream = new MemoryStream();

            // Act
            await _fileSystem.DownloadToStreamAsync("source.txt", destStream);

            // Assert
            destStream.Position = 0;
            using var reader = new StreamReader(destStream);
            var downloadedContent = await reader.ReadToEndAsync();
            Assert.Equal(sourceContent, downloadedContent);
        }

        [Theory]
        [InlineData(NamingRule.Uuid)]  // 使用UUID命名
        [InlineData(NamingRule.Normal)] // 使用常规命名
        public async Task UploadAsync_WithDifferentNamingSchemes_CreatesCorrectFiles(NamingRule namingRule)
        {
            // Arrange
            var content = "Test Content";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            var result = await _fileSystem.UploadAsync(
                stream,
                "test.txt",
                "container1",
              namingRule: namingRule);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result.RelativeFilePath));
            if (namingRule == NamingRule.Uuid)
            {
                Assert.NotEqual("test.txt", result.NewFileName);
                Assert.EndsWith(".txt", result.NewFileName);
            }
            else
            {
                Assert.Equal("test.txt", result.NewFileName);
            }
        }

        [Fact]
        public async Task DirectoryExistsAsync_WhenDirectoryDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var nonExistentPath = "nonexistent";

            // Act
            var result = await _fileSystem.DirectoryExistsAsync(nonExistentPath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task FileExistsAsync_WhenFileDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var nonExistentFile = "nonexistent.txt";

            // Act
            var result = await _fileSystem.FileExistsAsync(nonExistentFile);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UploadAsync_WithDuplicateFile_ThrowsDuplicateFileException()
        {
            // Arrange
            var content = "Test Content";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act & Assert
            await _fileSystem.UploadAsync(
                stream,
                "test.txt",
                "container1",
                namingRule: NamingRule.Normal);

            stream.Position = 0;
            await Assert.ThrowsAsync<DuplicateFileException>(() =>
                _fileSystem.UploadAsync(
                    stream,
                    "test.txt",
                    "container1",
                    namingRule: NamingRule.Normal,
                    overwrite: false,
                    useSequencedName: false));
        }

        [Fact]
        public async Task UploadAsync_WithSequencedName_CreatesNumberedFile()
        {
            // Arrange
            var content = "Test Content";
            using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(content));
            using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            await _fileSystem.UploadAsync(
                stream1,
                "test.txt",
                "container1",
                namingRule: NamingRule.Normal);

            var result = await _fileSystem.UploadAsync(
                stream2,
                "test.txt",
                "container1",
                namingRule: NamingRule.Normal);

            // Assert
            Assert.Contains("[1]", result.NewFileName);
        }

        [Fact]
        public async Task DownloadAsync_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _fileSystem.DownloadAsync("nonexistent.txt", "dest.txt"));
        }

        [Fact]
        public async Task DownloadToStreamAsync_WithNullStream_ReturnsFailureResult()
        {
            // Act
            var result = await _fileSystem.DownloadToStreamAsync("test.txt", null!);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async Task UploadAsync_WithHashMd5Name_CreatesHashNamedFile()
        {
            // Arrange
            var content = "Test Content";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            var result = await _fileSystem.UploadAsync(
                stream,
                "test.txt",
                "container1",
                namingRule: NamingRule.Md5);

            // Assert
            Assert.Contains("-", result.NewFileName);
            Assert.EndsWith(".txt", result.NewFileName);
        }


        [Fact]
        public async Task UploadAsync_WithInvalidFileName_ThrowsArgumentException()
        {
            // Arrange
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

            // Act & Assert
            await Assert.ThrowsAsync<System.ArgumentException>(() =>
                _fileSystem.UploadAsync(stream, "", "container1"));
        }

        [Fact]
        public async Task UploadAsync_WithOverwrite_ReplacesExistingFile()
        {
            // Arrange
            var content1 = "Content 1";
            var content2 = "Content 2";
            using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(content1));
            using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(content2));

            // Act
            await _fileSystem.UploadAsync(
                stream1,
                "test.txt",
                "container1",
                namingRule: NamingRule.Normal);

            var result = await _fileSystem.UploadAsync(
                stream2,
                "test.txt",
                "container1",
                namingRule: NamingRule.Normal, overwrite: true);

            // Assert
            Assert.Equal("test.txt", result.NewFileName);
            Assert.Equal(content2, File.ReadAllText(result.RelativeFilePath));
        }

        [Fact]
        public async Task GetRealPath_WithRelativePath_ReturnsFullPath()
        {
            // Arrange
            var relativePath = "test/file.txt";

            // Act
            var filePath = Path.Combine(_testRootPath, "test/file.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, "test");

            // Assert
            Assert.True(await _fileSystem.FileExistsAsync(relativePath));
        }

        [Fact]
        public async Task UploadAsync_WithSourceFilePathName_UploadsFile()
        {
            // Arrange
            var sourceContent = "Test Content";
            var sourceFilePath = Path.Combine(_testRootPath, "source.txt");
            File.WriteAllText(sourceFilePath, sourceContent);

            // Act
            var result = await _fileSystem.UploadAsync(
                sourceFilePath,
                "container1",
                namingRule: NamingRule.Md5);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result.RelativeFilePath));
            Assert.Equal("source.txt", result.FileName);
            Assert.Equal(sourceContent, File.ReadAllText(result.RelativeFilePath));
        }

        [Fact]
        public async Task UploadAsync_WithNonExistentSourceFile_ThrowsFileNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _fileSystem.UploadAsync(
                    "nonexistent.txt",
                    "container1"));
        }

        [Fact]
        public async Task UploadAsync_WithOneFailure_RetrySucceeds()
        {
            // Arrange
            var content = Encoding.UTF8.GetBytes("Test Content");
            using var stream = new FailingStream(content, 1); // 第一次失败，第二次成功

            // Act
            var result = await _fileSystem.UploadAsync(
                stream,
                "test.txt",
                "container1",
                namingRule: NamingRule.Md5);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result.RelativeFilePath));
            Assert.Equal(content, File.ReadAllBytes(result.RelativeFilePath));
        }

        [Fact]
        public async Task UploadAsync_ExceedingRetryCount_ThrowsOutOfRetryCountException()
        {
            // Arrange
            var content = Encoding.UTF8.GetBytes("Test Content");
            using var stream = new FailingStream(content, 3); // 会连续失败3次

            // Act & Assert
            await Assert.ThrowsAsync<OutOfRetryCountException>(() =>
                _fileSystem.UploadAsync(
                    stream,
                    "test.txt",
                    "container1",
                    namingRule: NamingRule.Md5));
        }


        [Fact]
        public async Task UploadAsync_WithLargeFile_TransfersCorrectly()
        {
            // Arrange
            var largeContent = new byte[5 * 1024 * 1024]; // 5MB
            new Random().NextBytes(largeContent);
            using var stream = new MemoryStream(largeContent);

            // Act
            var result = await _fileSystem.UploadAsync(
                stream,
                "large.bin",
                "container1",
                namingRule: NamingRule.Md5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(largeContent.Length, result.Length);
            Assert.True(File.Exists(result.RelativeFilePath));
            Assert.Equal(largeContent, File.ReadAllBytes(result.RelativeFilePath));
        }

        [Fact]
        public async Task UploadAsync_WithEmptyStream_UploadsSuccessfully()
        {
            // Arrange
            using var stream = new MemoryStream();

            // Act
            var result = await _fileSystem.UploadAsync(
                stream,
                "empty.txt",
                "container1",
                namingRule: NamingRule.Md5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Length);
            Assert.True(File.Exists(result.RelativeFilePath));
        }

        [Theory]
        [InlineData("test.txt", "container1", "", NamingRule.Md5, false, true)]
        [InlineData("test.txt", "container1", "custom/path", NamingRule.Uuid, false, false)]
        [InlineData("test.txt", "container1", "", NamingRule.Md5, true, false)]
        public async Task UploadAsync_WithVariousParameters_WorksCorrectly(
            string fileName,
            string containerName,
            string destPath,
            NamingRule namingRule,
            bool overwrite,
            bool useSequencedName)
        {
            // Arrange
            var content = "Test Content";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            var result = await _fileSystem.UploadAsync(
                stream,
                fileName,
                containerName,
                destPath,
                namingRule,
                overwrite,
                useSequencedName);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result.RelativeFilePath));

            if (namingRule == NamingRule.Uuid)
            {
                Assert.NotEqual(fileName, result.NewFileName);
                Assert.EndsWith(Path.GetExtension(fileName), result.NewFileName);
            }
            else if (namingRule == NamingRule.Md5)
            {
                Assert.Contains("-", result.NewFileName);
            }

            if (!string.IsNullOrEmpty(destPath))
            {
                Assert.Contains(destPath, result.FilePath);
            }
        }

        #region 流工厂与元数据方法测试

        [Fact]
        public async Task OpenReadAsync_WhenFileExists_ReturnsReadableStream()
        {
            // Arrange
            var content = "Test Content for OpenReadAsync";
            var filePath = Path.Combine(_testRootPath, "openread.txt");
            File.WriteAllText(filePath, content);

            // Act
            using var stream = await _fileSystem.OpenReadAsync("openread.txt");

            // Assert
            Assert.NotNull(stream);
            Assert.True(stream.CanRead);
            using var reader = new StreamReader(stream);
            var readContent = await reader.ReadToEndAsync();
            Assert.Equal(content, readContent);
        }

        [Fact]
        public async Task OpenReadAsync_WhenFileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _fileSystem.OpenReadAsync("nonexistent.txt"));
        }

        [Fact]
        public async Task OpenWriteAsync_CreatesNewFile()
        {
            // Arrange
            var filePath = "openwrite.txt";
            var content = "Test Content for OpenWriteAsync";

            // Act
            using (var stream = await _fileSystem.OpenWriteAsync(filePath))
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }

            // Assert
            var fullPath = Path.Combine(_testRootPath, filePath);
            Assert.True(File.Exists(fullPath));
            Assert.Equal(content, File.ReadAllText(fullPath));
        }

        [Fact]
        public async Task OpenWriteAsync_WithOverwriteFalse_ThrowsWhenFileExists()
        {
            // Arrange
            var filePath = "existing.txt";
            var fullPath = Path.Combine(_testRootPath, filePath);
            File.WriteAllText(fullPath, "existing content");

            // Act & Assert
            await Assert.ThrowsAsync<DuplicateFileException>(() =>
                _fileSystem.OpenWriteAsync(filePath, overwrite: false));
        }

        [Fact]
        public async Task OpenWriteAsync_WithOverwriteTrue_OverwritesExistingFile()
        {
            // Arrange
            var filePath = "overwrite.txt";
            var fullPath = Path.Combine(_testRootPath, filePath);
            File.WriteAllText(fullPath, "old content");
            var newContent = "new content";

            // Act
            using (var stream = await _fileSystem.OpenWriteAsync(filePath, overwrite: true))
            {
                var bytes = Encoding.UTF8.GetBytes(newContent);
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }

            // Assert
            Assert.Equal(newContent, File.ReadAllText(fullPath));
        }

        [Fact]
        public async Task GetReaderAsync_WhenFileExists_ReturnsStreamReader()
        {
            // Arrange
            var content = "Test Content for GetReaderAsync";
            var filePath = Path.Combine(_testRootPath, "getreader.txt");
            File.WriteAllText(filePath, content);

            // Act
            using var reader = await _fileSystem.GetReaderAsync("getreader.txt");

            // Assert
            Assert.NotNull(reader);
            var readContent = await reader.ReadToEndAsync();
            Assert.Equal(content, readContent);
        }

        [Fact]
        public async Task GetReaderAsync_WithEncoding_UsesSpecifiedEncoding()
        {
            // Arrange
            var content = "中文内容测试";
            var filePath = Path.Combine(_testRootPath, "encoding.txt");
            File.WriteAllText(filePath, content, Encoding.UTF8);

            // Act
            using var reader = await _fileSystem.GetReaderAsync("encoding.txt", Encoding.UTF8);

            // Assert
            var readContent = await reader.ReadToEndAsync();
            Assert.Equal(content, readContent);
        }

        [Fact]
        public async Task GetWriterAsync_CreatesNewFileWithContent()
        {
            // Arrange
            var filePath = "getwriter.txt";
            var content = "Test Content for GetWriterAsync";

            // Act
            using (var writer = await _fileSystem.GetWriterAsync(filePath))
            {
                await writer.WriteAsync(content);
            }

            // Assert
            var fullPath = Path.Combine(_testRootPath, filePath);
            Assert.True(File.Exists(fullPath));
            Assert.Equal(content, File.ReadAllText(fullPath));
        }

        [Fact]
        public async Task GetWriterAsync_WithEncoding_UsesSpecifiedEncoding()
        {
            // Arrange
            var filePath = "writer_encoding.txt";
            var content = "中文内容写入测试";

            // Act
            using (var writer = await _fileSystem.GetWriterAsync(filePath, encoding: Encoding.UTF8))
            {
                await writer.WriteAsync(content);
            }

            // Assert
            var fullPath = Path.Combine(_testRootPath, filePath);
            var readContent = File.ReadAllText(fullPath, Encoding.UTF8);
            Assert.Equal(content, readContent);
        }

        [Fact]
        public async Task IsDirectoryAsync_WhenDirectoryExists_ReturnsTrue()
        {
            // Arrange
            var dirPath = Path.Combine(_testRootPath, "testdir");
            Directory.CreateDirectory(dirPath);

            // Act
            var result = await _fileSystem.IsDirectoryAsync("testdir");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsDirectoryAsync_WhenDirectoryDoesNotExist_ReturnsFalse()
        {
            // Act
            var result = await _fileSystem.IsDirectoryAsync("nonexistentdir");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsDirectoryAsync_WhenPathIsFile_ReturnsFalse()
        {
            // Arrange
            var filePath = Path.Combine(_testRootPath, "isfile.txt");
            File.WriteAllText(filePath, "content");

            // Act
            var result = await _fileSystem.IsDirectoryAsync("isfile.txt");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetFileSizeAsync_WhenFileExists_ReturnsCorrectSize()
        {
            // Arrange
            var content = "Test Content for GetFileSizeAsync";
            var filePath = Path.Combine(_testRootPath, "filesize.txt");
            File.WriteAllText(filePath, content);
            var expectedSize = new FileInfo(filePath).Length;

            // Act
            var result = await _fileSystem.GetFileSizeAsync("filesize.txt");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSize, result.Value);
        }

        [Fact]
        public async Task GetFileSizeAsync_WhenFileDoesNotExist_ReturnsNull()
        {
            // Act
            var result = await _fileSystem.GetFileSizeAsync("nonexistent.txt");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetFileSizeAsync_WhenPathIsDirectory_ReturnsNull()
        {
            // Arrange
            var dirPath = Path.Combine(_testRootPath, "sizedir");
            Directory.CreateDirectory(dirPath);

            // Act
            var result = await _fileSystem.GetFileSizeAsync("sizedir");

            // Assert
            Assert.Null(result);
        }

        #endregion
    }

    public class FailingStream : Stream
    {
        private readonly MemoryStream _internalStream;
        private readonly int _failCount;
        private int _currentCopyAttempt;

        public FailingStream(byte[] buffer, int failCount)
        {
            _internalStream = new MemoryStream(buffer);
            _failCount = failCount;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _internalStream.Length;

        public override long Position
        {
            get => _internalStream.Position;
            set => _internalStream.Position = value;
        }

        public override void Flush() => _internalStream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            _currentCopyAttempt++;
            if (_currentCopyAttempt <= _failCount)
            {
                throw new IOException($"Simulated failure on attempt {_currentCopyAttempt}");
            }
            return _internalStream.Read(buffer, offset, count);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _currentCopyAttempt++;
            if (_currentCopyAttempt <= _failCount)
            {
                throw new IOException($"Simulated failure on attempt {_currentCopyAttempt}");
            }
            return await _internalStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _internalStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            _currentCopyAttempt++;
            if (_currentCopyAttempt <= _failCount)
            {
                throw new IOException($"Simulated failure on attempt {_currentCopyAttempt}");
            }

            _internalStream.Position = 0;
            return _internalStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _internalStream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
