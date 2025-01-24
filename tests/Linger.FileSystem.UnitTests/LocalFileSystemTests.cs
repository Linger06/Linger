using System.Text;
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
                useUuidName: false,
                useHashMd5Name: false);

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
        [InlineData(true)]  // 使用UUID命名
        [InlineData(false)] // 使用常规命名
        public async Task UploadAsync_WithDifferentNamingSchemes_CreatesCorrectFiles(bool useUuidName)
        {
            // Arrange
            var content = "Test Content";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            var result = await _fileSystem.UploadAsync(
                stream,
                "test.txt",
                "container1",
                useUuidName: useUuidName,
                useHashMd5Name: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result.RelativeFilePath));
            if (useUuidName)
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
        public void DirectoryExists_WhenDirectoryDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var nonExistentPath = "nonexistent";

            // Act
            var result = _fileSystem.DirectoryExists(nonExistentPath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void FileExists_WhenFileDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var nonExistentFile = "nonexistent.txt";

            // Act
            var result = _fileSystem.FileExists(nonExistentFile);

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
                useUuidName: false,
                useHashMd5Name: false);

            stream.Position = 0;
            await Assert.ThrowsAsync<DuplicateFileException>(() =>
                _fileSystem.UploadAsync(
                    stream,
                    "test.txt",
                    "container1",
                    useUuidName: false,
                    overwrite: false,
                    useSequencedName: false,
                    useHashMd5Name: false));
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
                useUuidName: false,
                useHashMd5Name: false);

            var result = await _fileSystem.UploadAsync(
                stream2,
                "test.txt",
                "container1",
                useUuidName: false,
                useSequencedName: true,
                useHashMd5Name: false);

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
        public async Task DownloadToStreamAsync_WithNullStream_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<System.ArgumentNullException>(() =>
                _fileSystem.DownloadToStreamAsync("test.txt", null!));
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
                useUuidName: false,
                useHashMd5Name: true);

            // Assert
            Assert.Contains("^_^", result.NewFileName);
            Assert.EndsWith(".txt", result.NewFileName);
        }


        [Fact]
        public async Task UploadAsync_WithInvalidFileName_ThrowsArgumentException()
        {
            // Arrange
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
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
                useUuidName: false,
                useHashMd5Name: false);

            var result = await _fileSystem.UploadAsync(
                stream2,
                "test.txt",
                "container1",
                useUuidName: false,
                overwrite: true,
                useHashMd5Name: false);

            // Assert
            Assert.Equal("test.txt", result.NewFileName);
            Assert.Equal(content2, File.ReadAllText(result.RelativeFilePath));
        }

        [Fact]
        public void GetRealPath_WithRelativePath_ReturnsFullPath()
        {
            // Arrange
            var relativePath = "test/file.txt";

            // Act
            var filePath = Path.Combine(_testRootPath, "test/file.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, "test");

            // Assert
            Assert.True(_fileSystem.FileExists(relativePath));
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
                useUuidName: false);

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
                useUuidName: false);

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
            await Assert.ThrowsAsync<OutOfReTryCountException>(() =>
                _fileSystem.UploadAsync(
                    stream,
                    "test.txt",
                    "container1",
                    useUuidName: false));
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
                useUuidName: false);

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
                useUuidName: false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Length);
            Assert.True(File.Exists(result.RelativeFilePath));
        }

        [Theory]
        [InlineData("test.txt", "container1", "", false, false, true, true)]
        [InlineData("test.txt", "container1", "custom/path", true, false, false, false)]
        [InlineData("test.txt", "container1", "", false, true, false, true)]
        public async Task UploadAsync_WithVariousParameters_WorksCorrectly(
            string fileName,
            string containerName,
            string destPath,
            bool useUuidName,
            bool overwrite,
            bool useSequencedName,
            bool useHashMd5Name)
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
                useUuidName,
                overwrite,
                useSequencedName,
                useHashMd5Name);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result.RelativeFilePath));

            if (useUuidName)
            {
                Assert.NotEqual(fileName, result.NewFileName);
                Assert.EndsWith(Path.GetExtension(fileName), result.NewFileName);
            }
            else if (useHashMd5Name)
            {
                Assert.Contains("^_^", result.NewFileName);
            }

            if (!string.IsNullOrEmpty(destPath))
            {
                Assert.Contains(destPath, result.FilePath);
            }
        }
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
            return _internalStream.Read(buffer, offset, count);
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
