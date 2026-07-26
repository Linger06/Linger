using System.Text;
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

    private sealed class TestFileSystem : FileSystemBase
    {
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

        public override Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<Stream> OpenWriteAsync(string filePath, bool overwrite = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<StreamReader> GetReaderAsync(string filePath, Encoding? encoding = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<StreamWriter> GetWriterAsync(string filePath, bool overwrite = false, Encoding? encoding = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> DownloadFileAsync(string remoteFilePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public override Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
