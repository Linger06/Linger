using System.Net;
using System.Net.Sockets;
using FluentFTP;
using FluentFTP.Exceptions;
using Linger.FileSystem.Exceptions;
using Moq;
using Xunit;

namespace Linger.FileSystem.Ftp.Tests;

public class FtpFileSystemTests
{
    private const string TestHost = "127.0.0.1";

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new FtpFileSystem(null!));
    }

    [Fact]
    public void Constructor_WithEmptyHost_ThrowsArgumentException()
    {
        var options = CreateOptions();
        options.Host = string.Empty;

        Assert.Throws<ArgumentException>(() => new FtpFileSystem(options));
    }

    [Fact]
    public void ServerDetails_ContainsProtocolUserHostAndPort()
    {
        using var fileSystem = new FtpFileSystem(CreateOptions(2121));

        Assert.Equal("FTP://user@127.0.0.1:2121", fileSystem.ServerDetails);
    }

    [Fact]
    public void IsRemoteFileSystem_ReturnsTrue()
    {
        using var fileSystem = new FtpFileSystem(CreateOptions());

        Assert.True(fileSystem.IsRemoteFileSystem);
    }

    [Theory]
    [InlineData("421")]
    [InlineData("450")]
    [InlineData("451")]
    [InlineData("452")]
    public void IsRetryableException_WithTransientFtpReply_ReturnsTrue(string completionCode)
    {
        using var fileSystem = new TestableFtpFileSystem(CreateOptions());

        Assert.True(fileSystem.InvokeIsRetryableException(new FtpCommandException(completionCode, "transient failure")));
    }

    [Theory]
    [InlineData("500")]
    [InlineData("530")]
    [InlineData("550")]
    [InlineData("553")]
    public void IsRetryableException_WithPermanentFtpReply_ReturnsFalse(string completionCode)
    {
        using var fileSystem = new TestableFtpFileSystem(CreateOptions());

        Assert.False(fileSystem.InvokeIsRetryableException(new FtpCommandException(completionCode, "permanent failure")));
    }

    [Theory]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(UnauthorizedAccessException))]
    [InlineData(typeof(FileNotFoundException))]
    [InlineData(typeof(NotSupportedException))]
    public void IsRetryableException_WithNonRetryableBaseException_ReturnsFalse(Type exceptionType)
    {
        using var fileSystem = new TestableFtpFileSystem(CreateOptions());
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        Assert.False(fileSystem.InvokeIsRetryableException(exception));
    }

    [Fact]
    public void IsRetryableException_WithIOException_ReturnsTrue()
    {
        using var fileSystem = new TestableFtpFileSystem(CreateOptions());

        Assert.True(fileSystem.InvokeIsRetryableException(new IOException("connection reset")));
    }

    [Fact]
    public void HandleException_WithGenericException_WrapsWithOperationPathAndServer()
    {
        using var fileSystem = new TestableFtpFileSystem(CreateOptions(2121));
        var original = new InvalidOperationException("boom");

        var exception = Assert.Throws<FileSystemException>(
            () => fileSystem.InvokeHandleException("Upload file", original, "data/a.txt"));

        Assert.Equal("Upload file", exception.Operation);
        Assert.Equal("data/a.txt", exception.FilePath);
        Assert.Equal("FTP://user@127.0.0.1:2121", exception.ServerInfo);
        Assert.Same(original, exception.InnerException);
    }

    [Fact]
    public void HandleException_WithGenericException_MessageNamesProtocolAndEndpoint()
    {
        using var fileSystem = new TestableFtpFileSystem(CreateOptions(2121));

        var exception = Assert.Throws<FileSystemException>(
            () => fileSystem.InvokeHandleException("Upload file", new InvalidOperationException(), "data/a.txt"));

        Assert.Contains("Type: FTP", exception.Message, StringComparison.Ordinal);
        Assert.Contains("127.0.0.1:2121", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void HandleException_WithCancellation_RethrowsOriginalException()
    {
        using var fileSystem = new TestableFtpFileSystem(CreateOptions());
        var original = new OperationCanceledException();

        var exception = Assert.Throws<OperationCanceledException>(
            () => fileSystem.InvokeHandleException("Download", original, "data/a.txt"));

        Assert.Same(original, exception);
    }

    [Fact]
    public void HandleException_WithFileSystemException_RethrowsSameInstance()
    {
        using var fileSystem = new TestableFtpFileSystem(CreateOptions());
        var original = new FileSystemException("Upload", "data/a.txt", "already wrapped");

        var exception = Assert.Throws<FileSystemException>(
            () => fileSystem.InvokeHandleException("Outer", original, "data/b.txt"));

        Assert.Same(original, exception);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var fileSystem = new FtpFileSystem(CreateOptions());
        fileSystem.Dispose();

        var exception = Record.Exception(fileSystem.Dispose);

        Assert.Null(exception);
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        var fileSystem = new FtpFileSystem(CreateOptions());
        await fileSystem.DisposeAsync();

        var exception = await Record.ExceptionAsync(() => fileSystem.DisposeAsync().AsTask());

        Assert.Null(exception);
    }

    [Fact]
    public async Task DisposeAsync_ThenDispose_DoesNotThrow()
    {
        var fileSystem = new FtpFileSystem(CreateOptions());
        await fileSystem.DisposeAsync();

        var exception = Record.Exception(fileSystem.Dispose);

        Assert.Null(exception);
    }

    [Fact]
    public async Task FileExistsAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var fileSystem = new FtpFileSystem(CreateOptions());
        fileSystem.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => fileSystem.FileExistsAsync("data/a.txt", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FileExistsAsync_WhenServerUnreachable_ThrowsFileSystemExceptionForConnect()
    {
        using var fileSystem = new FtpFileSystem(CreateOptions(GetClosedLocalPort()));
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));

        var exception = await Assert.ThrowsAsync<FileSystemException>(
            () => fileSystem.FileExistsAsync("data/a.txt", timeout.Token));

        Assert.Equal("Connect", exception.Operation);
    }

    [Fact]
    public async Task UploadAsync_WithOverwrite_CommitsStagedFileByRenameAsync()
    {
        using var fileSystem = new FtpFileSystem(CreateOptions());
        using var stream = new MemoryStream();
        var token = TestContext.Current.CancellationToken;
        const string destination = "data/a.txt";
        var client = MockStagedUpload(stream, token, out var temporaryPath);
        client.Setup(c => c.Rename(It.Is<string>(p => p == temporaryPath()), destination, token))
            .Returns(Task.CompletedTask);
        client.Setup(c => c.FileExists(It.Is<string>(p => p == temporaryPath()), CancellationToken.None))
            .ReturnsAsync(false);

        Assert.True(await fileSystem.UploadStreamAtomicallyAsync(client.Object, stream, destination, true, token));

        client.Verify(c => c.Rename(temporaryPath(), destination, token), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UploadAsync_WhenOverwriteCommitFails_DoesNotRepeatOrRestoreDestinationAsync(bool replyLost)
    {
        using var fileSystem = new FtpFileSystem(CreateOptions());
        using var stream = new MemoryStream();
        var token = TestContext.Current.CancellationToken;
        const string destination = "data/a.txt";
        var client = MockStagedUpload(stream, token, out var temporaryPath);
        Exception failure = replyLost ? new IOException("reply lost") : new FtpCommandException("550", "destination exists");
        client.Setup(c => c.Rename(It.Is<string>(p => p == temporaryPath()), destination, token))
            .ThrowsAsync(failure);
        client.Setup(c => c.FileExists(It.Is<string>(p => p == temporaryPath()), CancellationToken.None))
            .ReturnsAsync(!replyLost);
        client.Setup(c => c.DeleteFile(It.Is<string>(p => p == temporaryPath()), CancellationToken.None))
            .Returns(Task.CompletedTask);

        var exception = await Record.ExceptionAsync(() => fileSystem.UploadStreamAtomicallyAsync(client.Object, stream, destination, true, token));

        Assert.Same(failure, exception);
        client.Verify(c => c.Rename(temporaryPath(), destination, token), Times.Once);
        client.Verify(c => c.DeleteFile(temporaryPath(), CancellationToken.None), replyLost ? Times.Never() : Times.Once());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteAsync_DoesNotReplayDeleteWhenReplyIsLost(bool exists)
    {
        using var fileSystem = new DeleteTestFtpFileSystem(exists);
        var token = TestContext.Current.CancellationToken;

        if (exists)
        {
            var exception = await Assert.ThrowsAsync<FileSystemException>(() => fileSystem.DeleteAsync("data/a.txt", token));
            Assert.IsType<IOException>(exception.InnerException);
        }
        else
        {
            Assert.True((await fileSystem.DeleteAsync("data/a.txt", token)).Success);
        }

        Assert.Equal(exists ? 1 : 0, fileSystem.DeleteAttempts);
    }

    private sealed class DeleteTestFtpFileSystem(bool exists) : FtpFileSystem(CreateOptions())
    {
        public int DeleteAttempts { get; private set; }

        protected override bool IsConnected() => true;

        public override Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default) =>
            Task.FromResult(exists);

        internal override Task DeleteRemoteFileAsync(string path, CancellationToken cancellationToken)
        {
            DeleteAttempts++;
            return Task.FromException(new IOException("delete reply lost"));
        }
    }

    private static Mock<IAsyncFtpClient> MockStagedUpload(Stream stream, CancellationToken token, out Func<string> temporaryPath)
    {
        var path = string.Empty;
        var client = new Mock<IAsyncFtpClient>(MockBehavior.Strict);
        client.Setup(c => c.UploadStream(stream, It.IsRegex(@"^data/\.upload-[0-9a-f]{32}\.tmp$"), FtpRemoteExists.Overwrite, true, null, token))
            .Returns((Stream _, string remotePath, FtpRemoteExists _, bool _, IProgress<FtpProgress>? _, CancellationToken _) =>
            {
                path = remotePath;
                return Task.FromResult(FtpStatus.Success);
            });
        temporaryPath = () => path;

        return client;
    }

    private static FtpFileSystemOptions CreateOptions(int port = 21)
    {
        return new FtpFileSystemOptions
        {
            Host = TestHost,
            Port = port,
            UserName = "user",
            Password = "password",
            ConnectionTimeout = 2000,
            OperationTimeout = 2000
        };
    }

    // 先占用再释放一个本机端口，得到一个确定没有服务监听的端口。
    private static int GetClosedLocalPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
    }

    // 通过子类暴露 protected 方法，避免为了测试而放宽源码的可见性。
    private sealed class TestableFtpFileSystem : FtpFileSystem
    {
        public TestableFtpFileSystem(FtpFileSystemOptions options)
            : base(options)
        {
        }

        public bool InvokeIsRetryableException(Exception exception) => IsRetryableException(exception);

        public void InvokeHandleException(string operation, Exception exception, string? path = null)
            => HandleException(operation, exception, path);
    }
}
