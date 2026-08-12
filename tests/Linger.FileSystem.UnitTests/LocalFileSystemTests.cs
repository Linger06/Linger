using System.Text;
using Linger.Exceptions;
using Linger.FileSystem.Exceptions;
using Linger.FileSystem.Local;
using Linger.FileSystem.Remote;
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
        public void LocalFileSystem_UsesLocalContractOnly()
        {
            Assert.IsAssignableFrom<ILocalFileSystem>(_fileSystem);
            Assert.IsNotAssignableFrom<IRemoteFileSystem>(_fileSystem);
        }

        [Fact]
        public async Task UploadAsync_WithValidStream_UploadsFile()
        {
            // Arrange
            var content = "Test Content";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            var result = await _fileSystem.UploadWithNamingAsync(
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
        public async Task UploadAsync_ThroughTransferInterface_PreservesDestinationPath()
        {
            IFileTransfer fileSystem = _fileSystem;
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("content"));

            var result = await fileSystem.UploadAsync(stream, "uploads/exact-name.txt");

            Assert.True(result.Success);
            Assert.Equal(Path.Combine("uploads", "exact-name.txt"), result.FilePath);
            Assert.True(File.Exists(Path.Combine(_testRootPath, "uploads", "exact-name.txt")));
        }

        [Fact]
        public async Task UploadFileAsync_ThroughTransferInterface_UsesSharedFileAdapter()
        {
            IFileTransfer fileSystem = _fileSystem;
            var sourcePath = Path.Combine(_testRootPath, "source.txt");
            File.WriteAllText(sourcePath, "content");

            var result = await fileSystem.UploadFileAsync(sourcePath, "uploads/copied.txt");

            Assert.True(result.Success);
            Assert.Equal("content", File.ReadAllText(Path.Combine(_testRootPath, "uploads", "copied.txt")));
        }

        [Fact]
        public async Task UploadAsync_ThroughTransferInterface_WhenDestinationExists_ReturnsFailureWithoutSequencing()
        {
            var destinationPath = Path.Combine(_testRootPath, "existing.txt");
            File.WriteAllText(destinationPath, "original");
            IFileTransfer fileSystem = _fileSystem;
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("replacement"));

            var result = await fileSystem.UploadAsync(stream, "existing.txt");

            Assert.False(result.Success);
            Assert.Equal("original", File.ReadAllText(destinationPath));
            Assert.Single(Directory.EnumerateFiles(_testRootPath));
        }

        [Fact]
        public void LocalFileSystemOptions_DefaultValidationLevel_IsSizeOnly()
        {
            var options = new LocalFileSystemOptions();

            Assert.Equal(FileValidationLevel.SizeOnly, options.ValidationLevel);
        }

        [Fact]
        public async Task UploadAsync_WhenSourceReadFails_PreservesExistingFile()
        {
            var destinationPath = Path.Combine(_testRootPath, "existing.txt");
            File.WriteAllText(destinationPath, "original");
            IFileTransfer fileSystem = _fileSystem;
            using var stream = new FailingAsyncReadStream();

            await Assert.ThrowsAsync<OutOfRetryCountException>(() =>
                fileSystem.UploadAsync(stream, "existing.txt", overwrite: true));

            Assert.Equal("original", File.ReadAllText(destinationPath));
        }

        [Fact]
        public void Delete_WhenFileExists_DeletesFile()
        {
            // Arrange
            var filePath = Path.Combine(_testRootPath, "test.txt");
            File.WriteAllText(filePath, "test");

            // Act
            _fileSystem.Delete("test.txt");

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
            var requestedDestinationPath = Path.Combine(_testRootPath, "downloads", "dest.txt");
            var destPath = await _fileSystem.DownloadAsync(
                "source.txt",
                requestedDestinationPath);

            // Assert
            Assert.Equal(Path.GetFullPath(requestedDestinationPath), destPath);
            Assert.True(File.Exists(destPath));
            Assert.Equal(sourceContent, File.ReadAllText(destPath));
        }

        [Fact]
        public async Task DownloadAsync_WhenDestinationExistsAndOverwriteIsFalse_DoesNotReplaceFile()
        {
            var sourcePath = Path.Combine(_testRootPath, "source.txt");
            var destinationPath = Path.Combine(_testRootPath, "downloads", "dest.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.WriteAllText(sourcePath, "source content");
            File.WriteAllText(destinationPath, "existing content");

            await Assert.ThrowsAsync<DuplicateFileException>(() =>
                _fileSystem.DownloadAsync(
                    "source.txt",
                    destinationPath,
                    overwrite: false,
                    useSequencedName: false));

            Assert.Equal("existing content", File.ReadAllText(destinationPath));
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
            var result = await _fileSystem.UploadWithNamingAsync(
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
            await _fileSystem.UploadWithNamingAsync(
                stream,
                "test.txt",
                "container1",
                namingRule: NamingRule.Normal);

            stream.Position = 0;
            await Assert.ThrowsAsync<DuplicateFileException>(() =>
                _fileSystem.UploadWithNamingAsync(
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
            await _fileSystem.UploadWithNamingAsync(
                stream1,
                "test.txt",
                "container1",
                namingRule: NamingRule.Normal);

            var result = await _fileSystem.UploadWithNamingAsync(
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
        public async Task DownloadToStreamAsync_WithNullStream_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _fileSystem.DownloadToStreamAsync("test.txt", null!));
        }

        [Fact]
        public async Task UploadAsync_WithHashMd5Name_CreatesHashNamedFile()
        {
            // Arrange
            var content = "Test Content";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            var result = await _fileSystem.UploadWithNamingAsync(
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
                _fileSystem.UploadWithNamingAsync(stream, "", "container1"));
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
            await _fileSystem.UploadWithNamingAsync(
                stream1,
                "test.txt",
                "container1",
                namingRule: NamingRule.Normal);

            var result = await _fileSystem.UploadWithNamingAsync(
                stream2,
                "test.txt",
                "container1",
                namingRule: NamingRule.Normal, overwrite: true);

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
        public void GetRealPath_WithSiblingPrefix_ThrowsArgumentException()
        {
            var siblingPath = Path.GetFullPath(_fileSystem.RootDirectoryPath + "-sibling");

            Assert.Throws<ArgumentException>(() => _fileSystem.GetRealPath(Path.Combine(siblingPath, "file.txt")));
        }

        [Fact]
        public void GetRealPath_WithParentTraversal_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _fileSystem.GetRealPath(Path.Combine("..", "outside.txt")));
        }

#if NET6_0_OR_GREATER
        [Fact]
        public void GetRealPath_WithDirectoryReparsePoint_ThrowsIOException()
        {
            var targetPath = _testRootPath + "-target";
            var linkPath = Path.Combine(_testRootPath, "linkedDirectory");
            Directory.CreateDirectory(targetPath);

            try
            {
                _ = Directory.CreateSymbolicLink(linkPath, targetPath);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or PlatformNotSupportedException)
            {
                Directory.Delete(targetPath, true);
                throw Xunit.Sdk.SkipException.ForSkip("The current environment does not allow creating directory symbolic links.");
            }

            try
            {
                Assert.Throws<IOException>(() => _fileSystem.GetRealPath(Path.Combine("linkedDirectory", "outside.txt")));
            }
            finally
            {
                Directory.Delete(linkPath);
                Directory.Delete(targetPath, true);
            }
        }
#endif

        [Fact]
        public async Task UploadAsync_WithSourceFilePathName_UploadsFile()
        {
            // Arrange
            var sourceContent = "Test Content";
            var sourceFilePath = Path.Combine(_testRootPath, "source.txt");
            File.WriteAllText(sourceFilePath, sourceContent);

            // Act
            var result = await _fileSystem.UploadFileWithNamingAsync(
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
                _fileSystem.UploadFileWithNamingAsync(
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
            var result = await _fileSystem.UploadWithNamingAsync(
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
        public async Task UploadAsync_AfterPartialFailure_RestoresInitialStreamPosition()
        {
            var content = Encoding.UTF8.GetBytes("xxTest Content");
            using var stream = new PartialFailureStream(content, canSeek: true)
            {
                Position = 2
            };

            var result = await _fileSystem.UploadWithNamingAsync(
                stream,
                "test.txt",
                "container1",
                namingRule: NamingRule.Md5);

            Assert.Equal(content.Skip(2).ToArray(), File.ReadAllBytes(result.RelativeFilePath));
        }

        [Fact]
        public async Task UploadAsync_WithNonSeekableStream_DoesNotRetryPartialTransfer()
        {
            var content = Encoding.UTF8.GetBytes("Test Content");
            using var stream = new PartialFailureStream(content, canSeek: false);

            await Assert.ThrowsAsync<IOException>(() => _fileSystem.UploadWithNamingAsync(
                stream,
                "test.txt",
                "container1",
                namingRule: NamingRule.Md5));
        }

        [Fact]
        public async Task UploadAsync_ExceedingRetryCount_ThrowsOutOfRetryCountException()
        {
            // Arrange
            var content = Encoding.UTF8.GetBytes("Test Content");
            using var stream = new FailingStream(content, 3); // 会连续失败3次

            // Act & Assert
            await Assert.ThrowsAsync<OutOfRetryCountException>(() =>
                _fileSystem.UploadWithNamingAsync(
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
            var result = await _fileSystem.UploadWithNamingAsync(
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
            var result = await _fileSystem.UploadWithNamingAsync(
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
            var result = await _fileSystem.UploadWithNamingAsync(
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
        public async Task OpenRead_WhenFileExists_ReturnsReadableStream()
        {
            // Arrange
            var content = "Test Content for OpenReadAsync";
            var filePath = Path.Combine(_testRootPath, "openread.txt");
            File.WriteAllText(filePath, content);

            // Act
            using var stream = _fileSystem.OpenRead("openread.txt");

            // Assert
            Assert.NotNull(stream);
            Assert.True(stream.CanRead);
            using var reader = new StreamReader(stream);
            var readContent = await reader.ReadToEndAsync();
            Assert.Equal(content, readContent);
        }

        [Fact]
        public void OpenRead_WhenFileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Act & Assert
            Assert.Throws<FileNotFoundException>(() =>
                _fileSystem.OpenRead("nonexistent.txt"));
        }

        [Fact]
        public async Task OpenWrite_CreatesNewFile()
        {
            // Arrange
            var filePath = "openwrite.txt";
            var content = "Test Content for OpenWriteAsync";

            // Act
            using (var stream = _fileSystem.OpenWrite(filePath))
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
        public void OpenWrite_WithOverwriteFalse_ThrowsWhenFileExists()
        {
            // Arrange
            var filePath = "existing.txt";
            var fullPath = Path.Combine(_testRootPath, filePath);
            File.WriteAllText(fullPath, "existing content");

            // Act & Assert
            Assert.Throws<DuplicateFileException>(() =>
                _fileSystem.OpenWrite(filePath, overwrite: false));
        }

        [Fact]
        public async Task OpenWrite_WithOverwriteTrue_OverwritesExistingFile()
        {
            // Arrange
            var filePath = "overwrite.txt";
            var fullPath = Path.Combine(_testRootPath, filePath);
            File.WriteAllText(fullPath, "old content");
            var newContent = "new content";

            // Act
            using (var stream = _fileSystem.OpenWrite(filePath, overwrite: true))
            {
                var bytes = Encoding.UTF8.GetBytes(newContent);
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }

            // Assert
            Assert.Equal(newContent, File.ReadAllText(fullPath));
        }

        [Fact]
        public async Task GetReader_WhenFileExists_ReturnsStreamReader()
        {
            // Arrange
            var content = "Test Content for GetReaderAsync";
            var filePath = Path.Combine(_testRootPath, "getreader.txt");
            File.WriteAllText(filePath, content);

            // Act
            using var reader = _fileSystem.GetReader("getreader.txt");

            // Assert
            Assert.NotNull(reader);
            var readContent = await reader.ReadToEndAsync();
            Assert.Equal(content, readContent);
        }

        [Fact]
        public async Task GetReader_WithEncoding_UsesSpecifiedEncoding()
        {
            // Arrange
            var content = "中文内容测试";
            var filePath = Path.Combine(_testRootPath, "encoding.txt");
            File.WriteAllText(filePath, content, Encoding.UTF8);

            // Act
            using var reader = _fileSystem.GetReader("encoding.txt", Encoding.UTF8);

            // Assert
            var readContent = await reader.ReadToEndAsync();
            Assert.Equal(content, readContent);
        }

        [Fact]
        public async Task GetWriter_CreatesNewFileWithContent()
        {
            // Arrange
            var filePath = "getwriter.txt";
            var content = "Test Content for GetWriterAsync";

            // Act
            using (var writer = _fileSystem.GetWriter(filePath))
            {
                await writer.WriteAsync(content);
            }

            // Assert
            var fullPath = Path.Combine(_testRootPath, filePath);
            Assert.True(File.Exists(fullPath));
            Assert.Equal(content, File.ReadAllText(fullPath));
        }

        [Fact]
        public async Task GetWriter_WithEncoding_UsesSpecifiedEncoding()
        {
            // Arrange
            var filePath = "writer_encoding.txt";
            var content = "中文内容写入测试";

            // Act
            using (var writer = _fileSystem.GetWriter(filePath, encoding: Encoding.UTF8))
            {
                await writer.WriteAsync(content);
            }

            // Assert
            var fullPath = Path.Combine(_testRootPath, filePath);
            var readContent = File.ReadAllText(fullPath, Encoding.UTF8);
            Assert.Equal(content, readContent);
        }

        [Fact]
        public void GetFileSize_WhenFileExists_ReturnsCorrectSize()
        {
            // Arrange
            var content = "Test Content for GetFileSizeAsync";
            var filePath = Path.Combine(_testRootPath, "filesize.txt");
            File.WriteAllText(filePath, content);
            var expectedSize = new FileInfo(filePath).Length;

            // Act
            var result = _fileSystem.GetFileSize("filesize.txt");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSize, result.Value);
        }

        [Fact]
        public void GetFileSize_WhenFileDoesNotExist_ReturnsNull()
        {
            // Act
            var result = _fileSystem.GetFileSize("nonexistent.txt");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFileSize_WhenPathIsDirectory_ReturnsNull()
        {
            // Arrange
            var dirPath = Path.Combine(_testRootPath, "sizedir");
            Directory.CreateDirectory(dirPath);

            // Act
            var result = _fileSystem.GetFileSize("sizedir");

            // Assert
            Assert.Null(result);
        }

        #endregion
    }

    internal sealed class FailingAsyncReadStream : MemoryStream
    {
#if NET8_0_OR_GREATER
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromException<int>(new IOException("Simulated read failure."));
        }
#else
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.FromException<int>(new IOException("Simulated read failure."));
        }
#endif
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

    internal sealed class PartialFailureStream : Stream
    {
        private readonly MemoryStream _innerStream;
        private readonly bool _canSeek;
        private bool _hasReturnedPartialData;
        private bool _hasFailed;

        public PartialFailureStream(byte[] buffer, bool canSeek)
        {
            _innerStream = new MemoryStream(buffer);
            _canSeek = canSeek;
        }

        public override bool CanRead => true;
        public override bool CanSeek => _canSeek;
        public override bool CanWrite => false;
        public override long Length => _innerStream.Length;

        public override long Position
        {
            get => _innerStream.Position;
            set
            {
                if (!_canSeek)
                {
                    throw new NotSupportedException();
                }

                _innerStream.Position = value;
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_hasReturnedPartialData && !_hasFailed)
            {
                _hasFailed = true;
                throw new IOException("Simulated failure after a partial read.");
            }

            var bytesRead = _innerStream.Read(buffer, offset, _hasReturnedPartialData ? count : Math.Min(count, 3));
            _hasReturnedPartialData = bytesRead > 0;

            return bytesRead;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.FromResult(Read(buffer, offset, count));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!_canSeek)
            {
                throw new NotSupportedException();
            }

            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _innerStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
