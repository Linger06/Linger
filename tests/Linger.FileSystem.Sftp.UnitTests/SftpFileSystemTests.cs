using System.Net;
using System.Net.Sockets;
using Linger.FileSystem.Exceptions;
using Renci.SshNet.Common;
using Xunit;

namespace Linger.FileSystem.Sftp.Tests;

public class SftpFileSystemTests
{
    private const string TestHost = "127.0.0.1";

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SftpFileSystem(null!));
    }

    [Fact]
    public void Constructor_WithEmptyHost_ThrowsArgumentException()
    {
        var options = CreateOptions();
        options.Host = string.Empty;

        Assert.Throws<ArgumentException>(() => new SftpFileSystem(options));
    }

    [Fact]
    public void ServerDetails_ContainsProtocolUserHostAndPort()
    {
        using var fileSystem = new SftpFileSystem(CreateOptions(2222));

        Assert.Equal("SFTP://user@127.0.0.1:2222", fileSystem.ServerDetails);
    }

    [Fact]
    public void IsRemoteFileSystem_ReturnsTrue()
    {
        using var fileSystem = new SftpFileSystem(CreateOptions());

        Assert.True(fileSystem.IsRemoteFileSystem);
    }

    [Fact]
    public void IsRetryableException_WithPermissionDenied_ReturnsFalse()
    {
        using var fileSystem = new TestableSftpFileSystem(CreateOptions());

        Assert.False(fileSystem.InvokeIsRetryableException(new SftpPermissionDeniedException("denied")));
    }

    [Fact]
    public void IsRetryableException_WithPathNotFound_ReturnsFalse()
    {
        using var fileSystem = new TestableSftpFileSystem(CreateOptions());

        Assert.False(fileSystem.InvokeIsRetryableException(new SftpPathNotFoundException("missing")));
    }

    [Fact]
    public void IsRetryableException_WithAuthenticationFailure_ReturnsFalse()
    {
        using var fileSystem = new TestableSftpFileSystem(CreateOptions());

        Assert.False(fileSystem.InvokeIsRetryableException(new SshAuthenticationException("bad credentials")));
    }

    [Fact]
    public void IsRetryableException_WithIOException_ReturnsTrue()
    {
        using var fileSystem = new TestableSftpFileSystem(CreateOptions());

        Assert.True(fileSystem.InvokeIsRetryableException(new IOException("connection reset")));
    }

    [Theory]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(UnauthorizedAccessException))]
    [InlineData(typeof(FileNotFoundException))]
    [InlineData(typeof(NotSupportedException))]
    public void IsRetryableException_WithNonRetryableBaseException_ReturnsFalse(Type exceptionType)
    {
        using var fileSystem = new TestableSftpFileSystem(CreateOptions());
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        Assert.False(fileSystem.InvokeIsRetryableException(exception));
    }

    [Fact]
    public void HandleException_WithGenericException_WrapsWithOperationPathAndServer()
    {
        using var fileSystem = new TestableSftpFileSystem(CreateOptions(2222));
        var original = new InvalidOperationException("boom");

        var exception = Assert.Throws<FileSystemException>(
            () => fileSystem.InvokeHandleException("Upload file", original, "data/a.txt"));

        Assert.Equal("Upload file", exception.Operation);
        Assert.Equal("data/a.txt", exception.FilePath);
        Assert.Equal("SFTP://user@127.0.0.1:2222", exception.ServerInfo);
        Assert.Same(original, exception.InnerException);
    }

    [Fact]
    public void HandleException_WithCancellation_RethrowsOriginalException()
    {
        using var fileSystem = new TestableSftpFileSystem(CreateOptions());
        var original = new OperationCanceledException();

        var exception = Assert.Throws<OperationCanceledException>(
            () => fileSystem.InvokeHandleException("Download", original, "data/a.txt"));

        Assert.Same(original, exception);
    }

    [Fact]
    public void HandleException_WithFileSystemException_RethrowsSameInstance()
    {
        using var fileSystem = new TestableSftpFileSystem(CreateOptions());
        var original = new FileSystemException("Upload", "data/a.txt", "already wrapped");

        var exception = Assert.Throws<FileSystemException>(
            () => fileSystem.InvokeHandleException("Outer", original, "data/b.txt"));

        Assert.Same(original, exception);
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var fileSystem = new SftpFileSystem(CreateOptions());
        fileSystem.Dispose();

        var exception = Record.Exception(fileSystem.Dispose);

        Assert.Null(exception);
    }

    [Fact]
    public async Task FileExistsAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var fileSystem = new SftpFileSystem(CreateOptions());
        fileSystem.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => fileSystem.FileExistsAsync("data/a.txt", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FileExistsAsync_WhenServerUnreachable_ThrowsFileSystemExceptionForConnect()
    {
        using var fileSystem = new SftpFileSystem(CreateOptions(GetClosedLocalPort()));
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));

        var exception = await Assert.ThrowsAsync<FileSystemException>(
            () => fileSystem.FileExistsAsync("data/a.txt", timeout.Token));

        Assert.Equal("Connect", exception.Operation);
    }

    private static SftpFileSystemOptions CreateOptions(int port = 22)
    {
        return new SftpFileSystemOptions
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
    private sealed class TestableSftpFileSystem : SftpFileSystem
    {
        public TestableSftpFileSystem(SftpFileSystemOptions options)
            : base(options)
        {
        }

        public bool InvokeIsRetryableException(Exception exception) => IsRetryableException(exception);

        public void InvokeHandleException(string operation, Exception exception, string? path = null)
            => HandleException(operation, exception, path);
    }
}
