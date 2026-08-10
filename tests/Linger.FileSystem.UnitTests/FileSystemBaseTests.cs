using System.Collections.Concurrent;
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
    public async Task ExecuteWithBatchRetryAsync_FalseResult_RetriesUntilSuccess()
    {
        var fileSystem = new TestRemoteFileSystem();
        var attempts = 0;

        var result = await fileSystem.ExecuteBooleanBatchAsync(() =>
        {
            attempts++;
            return Task.FromResult(attempts == 3);
        });

        Assert.True(result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task ExecuteWithBatchRetryAsync_WhenAllResultsFail_ReturnsLastResult()
    {
        var fileSystem = new TestRemoteFileSystem();
        var attempts = 0;

        var result = await fileSystem.ExecuteBooleanBatchAsync(() =>
        {
            attempts++;
            return Task.FromResult(false);
        });

        Assert.False(result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task ExecuteWithBatchRetryAsync_PermanentFailure_DoesNotRetry()
    {
        var fileSystem = new TestRemoteFileSystem();
        var attempts = 0;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            fileSystem.ExecuteBooleanBatchAsync(() =>
            {
                attempts++;
                throw new UnauthorizedAccessException("Access denied.");
            }));

        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task ExecuteParallelBatchAsync_RecordsItemFailuresAndProcessesRemainingItems()
    {
        var fileSystem = new TestRemoteFileSystem();
        var processed = new ConcurrentBag<string>();
        var failures = new ConcurrentBag<string>();
        var activeWorkers = 0;
        var maximumWorkers = 0;

        await fileSystem.ExecuteParallelAsync(
            ["one", "two", "three", "four"],
            degree: 2,
            async (_, filePath, cancellationToken) =>
            {
                var active = Interlocked.Increment(ref activeWorkers);
                InterlockedExtensions.Max(ref maximumWorkers, active);
                await Task.Delay(1, cancellationToken);
                Interlocked.Decrement(ref activeWorkers);

                if (filePath == "two")
                {
                    throw new IOException("simulated failure");
                }

                processed.Add(filePath);
            },
            _ => Task.CompletedTask,
            (filePath, _) => failures.Add(filePath));

        Assert.Equal(3, processed.Count);
        Assert.Contains("two", failures);
        Assert.InRange(maximumWorkers, 1, 2);
    }

    [Fact]
    public async Task ExecuteParallelBatchAsync_CancellationIsPropagated()
    {
        var fileSystem = new TestRemoteFileSystem();
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            fileSystem.ExecuteParallelAsync(
                ["one", "two"],
                degree: 2,
                static (_, _, _) => Task.CompletedTask,
                static _ => Task.CompletedTask,
                static (_, _) => { },
                cancellationSource.Token));
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

    private static class InterlockedExtensions
    {
        public static void Max(ref int location, int value)
        {
            while (true)
            {
                var current = Volatile.Read(ref location);
                if (current >= value || Interlocked.CompareExchange(ref location, value, current) == current)
                {
                    return;
                }
            }
        }
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
                    UserName = "test",
                    BatchRetryOptions = new RetryOptions
                    {
                        MaxRetryAttempts = 3,
                        DelayMilliseconds = 1,
                        MaxDelayMilliseconds = 1,
                        UseExponentialBackoff = false,
                        Jitter = 0
                    }
                },
                "TEST")
        {
        }

        public Task<bool> ExecuteBooleanBatchAsync(Func<Task<bool>> operation)
        {
            return ExecuteWithBatchRetryAsync(operation, CancellationToken.None);
        }

        public Task ExecuteParallelAsync(
            IReadOnlyCollection<string> filePaths,
            int degree,
            Func<object, string, CancellationToken, Task> operation,
            Func<object, Task> disposeClient,
            Action<string, Exception> onError,
            CancellationToken cancellationToken = default)
        {
            return ExecuteParallelBatchAsync(
                filePaths,
                degree,
                static () => new object(),
                operation,
                disposeClient,
                onError,
                cancellationToken);
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
                useBatchRetry: false,
                CancellationToken.None);
        }

        public override bool IsConnected() => false;
        public override Task ConnectAsync() => Task.CompletedTask;
        public override Task ConnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public override Task DisconnectAsync() => Task.CompletedTask;
        public override Task<DateTime> GetLastModifiedTimeAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override void Dispose() { }
        public override Task<BatchOperationResult> UploadFilesAsync(IEnumerable<string> localFilePaths, string remoteDirectory, bool overwrite = false, IProgress<BatchProgress>? progress = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<BatchOperationResult> DownloadFilesAsync(IEnumerable<string> remoteFilePaths, string localDirectory, bool overwrite = false, IProgress<BatchProgress>? progress = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<BatchOperationResult> DeleteFilesAsync(IEnumerable<string> filePaths, IProgress<BatchProgress>? progress = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
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
