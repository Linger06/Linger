using System.Text;
using Linger.FileSystem.Exceptions;
using Linger.FileSystem.Remote;
using Linger.Helper;
using Xunit;

namespace Linger.FileSystem.Tests;

public class FileSystemBaseTests
{
    [Fact]
    public async Task ExecuteStreamOperationAsync_OutputRetry_RestoresPositionAndLength()
    {
        var fileSystem = new TestFileSystem();
        using var outputStream = new MemoryStream();
        var prefix = Encoding.UTF8.GetBytes("prefix");
        await outputStream.WriteAsync(prefix, 0, prefix.Length);
        var attempt = 0;

        await fileSystem.ExecuteOutputAsync(outputStream, async cancellationToken =>
        {
            attempt++;
            var content = Encoding.UTF8.GetBytes(attempt == 1 ? "partial" : "complete");
            await outputStream.WriteAsync(content, 0, content.Length, cancellationToken);
            if (attempt == 1)
            {
                throw new IOException("Simulated partial download failure.");
            }

            return true;
        });

        Assert.Equal("prefixcomplete", Encoding.UTF8.GetString(outputStream.ToArray()));
    }

    [Fact]
    public async Task DownloadFileAtomicallyAsync_DuplicateTargetPreservesExistingFile()
    {
        var fileSystem = new TestRemoteFileSystem();
        var testDirectory = Path.Combine(Path.GetTempPath(), $"linger-download-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDirectory);
        var destinationPath = Path.Combine(testDirectory, "destination.txt");
        File.WriteAllText(destinationPath, "original");

        try
        {
            await Assert.ThrowsAsync<DuplicateFileException>(() =>
                fileSystem.DownloadAtomicAsync(
                    destinationPath,
                    overwrite: false,
                    (temporaryPath, _) =>
                    {
                        File.WriteAllText(temporaryPath, "replacement");
                        return Task.FromResult(true);
                    }));

            Assert.Equal("original", File.ReadAllText(destinationPath));
            Assert.Empty(Directory.GetFiles(testDirectory, ".download-*.tmp"));
        }
        finally
        {
            Directory.Delete(testDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task DownloadFileAtomicallyAsync_OverwriteCommitsAndCleansTemporaryFile()
    {
        var fileSystem = new TestRemoteFileSystem();
        var testDirectory = Path.Combine(Path.GetTempPath(), $"linger-download-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDirectory);
        var destinationPath = Path.Combine(testDirectory, "destination.txt");
        File.WriteAllText(destinationPath, "original");

        try
        {
            var result = await fileSystem.DownloadAtomicAsync(
                destinationPath,
                overwrite: true,
                (temporaryPath, _) =>
                {
                    File.WriteAllText(temporaryPath, "replacement");
                    return Task.FromResult(true);
                });

            Assert.True(result);
            Assert.Equal("replacement", File.ReadAllText(destinationPath));
            Assert.Empty(Directory.GetFiles(testDirectory, ".download-*.tmp"));
        }
        finally
        {
            Directory.Delete(testDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task GetReaderAsync_DisposeClosesUnderlyingStream()
    {
        var fileSystem = new TestFileSystem();

        using (var reader = await fileSystem.GetReaderAsync("input.txt", Encoding.UTF8))
        {
            Assert.Equal("content", await reader.ReadToEndAsync());
        }

        Assert.True(fileSystem.LastStreamDisposed);
    }

    [Fact]
    public async Task GetWriterAsync_DisposeClosesUnderlyingStream()
    {
        var fileSystem = new TestFileSystem();

        using (var writer = await fileSystem.GetWriterAsync("output.txt", encoding: Encoding.UTF8))
        {
            await writer.WriteAsync("content");
        }

        Assert.True(fileSystem.LastStreamDisposed);
    }

    private sealed class TestFileSystem : FileSystemBase
    {
        private TrackingMemoryStream? _lastStream;

        public TestFileSystem()
            : base(new RetryOptions
            {
                MaxRetryAttempts = 2,
                DelayMilliseconds = 1,
                MaxDelayMilliseconds = 1,
                UseExponentialBackoff = false,
                Jitter = 0
            })
        {
        }

        public Task<bool> ExecuteOutputAsync(Stream stream, Func<CancellationToken, Task<bool>> operation)
        {
            return ExecuteStreamOperationAsync(
                stream,
                operation,
                "Test output operation",
                restoreLength: true,
                cancellationToken: CancellationToken.None);
        }

        public bool LastStreamDisposed => _lastStream?.WasDisposed == true;

        public override Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            _lastStream = new TrackingMemoryStream(Encoding.UTF8.GetBytes("content"));
            return Task.FromResult<Stream>(_lastStream);
        }

        public override Task<Stream> OpenWriteAsync(string filePath, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            _lastStream = new TrackingMemoryStream();
            return Task.FromResult<Stream>(_lastStream);
        }
        public override Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> DownloadFileAsync(string remoteFilePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class TestRemoteFileSystem : RemoteFileSystemBase
    {
        public TestRemoteFileSystem()
            : base(
                new RemoteFileSystemOptions
                {
                    Host = "localhost",
                    Port = 1,
                    UserName = "test"
                },
                "TEST")
        {
        }

        public Task<bool> DownloadAtomicAsync(
            string localDestinationPath,
            bool overwrite,
            Func<string, CancellationToken, Task<bool>> downloadAttempt)
        {
            return DownloadFileAtomicallyAsync(
                localDestinationPath,
                overwrite,
                downloadAttempt,
                CancellationToken.None);
        }

        protected override bool IsConnected() => false;
        protected override Task ConnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        protected override Task DisconnectAsync() => Task.CompletedTask;
        public override Task<DateTime> GetLastModifiedTimeAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override void Dispose() { }
        public override Task<IReadOnlyList<string>> ListFilesAsync(string directoryPath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<IReadOnlyList<string>> ListDirectoriesAsync(string directoryPath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<Stream> OpenWriteAsync(string filePath, bool overwrite = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> DownloadFileAsync(string remoteFilePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class TrackingMemoryStream : MemoryStream
    {
        public TrackingMemoryStream()
        {
        }

        public TrackingMemoryStream(byte[] buffer)
            : base(buffer)
        {
        }

        public bool WasDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }
}
