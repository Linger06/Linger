using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Linger.FileSystem.Exceptions;
using Moq;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Xunit;

namespace Linger.FileSystem.Sftp.Tests;

public class SftpFileSystemTests
{
    private const string PublicHostKey = "AAAAC3NzaC1lZDI1NTE5AAAAINdamAGCsQq31Uv+08lkBzoO4XLz2qYjJa8CGmj3B1Ea";
    private const string MatchingPin = "SHA256:bbXpuKG6zhzdmnxq256TlqzFBzRl2f6OOg722cYNbU8";
    private const string DifferentPin = "SHA256:BbXpuKG6zhzdmnxq256TlqzFBzRl2f6OOg722cYNbU8";
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

    [Theory]
    [InlineData(null)]
    [InlineData("unused-client-private-key.pem")]
    public void Constructor_WithoutHostKeyFingerprint_RejectsBothAuthenticationModes(string? certificatePath)
    {
        var options = new SftpFileSystemOptions { Host = TestHost, Port = 22, UserName = "user", CertificatePath = certificatePath };

        var exception = Assert.Throws<ArgumentException>(() => new SftpFileSystem(options));

        Assert.Contains(nameof(options.HostKeyFingerprintSha256), exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("MD5:00:11")]
    [InlineData("SHA256:AAAA")]
    [InlineData("bbXpuKG6zhzdmnxq256TlqzFBzRl2f6OOg722cYNbU8")]
    [InlineData("SHA256:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    [InlineData(MatchingPin + "=")]
    [InlineData("SHA256:!bXpuKG6zhzdmnxq256TlqzFBzRl2f6OOg722cYNbU8")]
    [InlineData("SHA256:bbXpuKG6zhzdmnxq256TlqzFBzRl2f6OOg722cYNbU9")]
    public void Constructor_WithMalformedHostKeyFingerprint_ThrowsArgumentException(string fingerprint)
    {
        var options = CreateOptions();
        options.HostKeyFingerprintSha256 = fingerprint;

        var exception = Assert.Throws<ArgumentException>(() => new SftpFileSystem(options));

        Assert.Contains(nameof(options.HostKeyFingerprintSha256), exception.Message);
    }

    [Theory]
    [InlineData(MatchingPin, true)]
    [InlineData(DifferentPin, false)]
    public void HostKeyReceived_TrustsOnlyFingerprintConfiguredAtConstruction(string pin, bool expectedTrust)
    {
        var options = CreateOptions();
        options.HostKeyFingerprintSha256 = pin;
        using var fileSystem = new TestableSftpFileSystem(options);
        options.HostKeyFingerprintSha256 = expectedTrust ? DifferentPin : MatchingPin;
        using var key = new ED25519Key(new SshKeyData(Convert.FromBase64String(PublicHostKey)));
        var args = new HostKeyEventArgs(new KeyHostAlgorithm("ssh-ed25519", key)) { CanTrust = !expectedTrust };

        fileSystem.RaiseHostKeyReceived(args);

        Assert.Equal(expectedTrust, args.CanTrust);
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Upload_RetriesStagingButNeverRetriesCommitAsync(bool replyLost)
    {
        const string destination = "data/a.txt";
        var token = TestContext.Current.CancellationToken;
        using var fileSystem = new SftpFileSystem(CreateOptions(), new Linger.Helper.RetryOptions { DelayMilliseconds = 1 });
        using var stream = new MemoryStream([1, 2, 3]);
        stream.Position = 1;
        var client = new Mock<ISftpClient>(MockBehavior.Strict);
        string? staged = null;
        var attempts = 0;
        client.Setup(c => c.UploadFileAsync(stream, It.IsAny<string>(), token)).Returns((Stream input, string path, CancellationToken _) =>
        {
            Assert.Equal(2, input.ReadByte());
            staged ??= path;
            Assert.Equal(staged, path);
            return ++attempts == 1 ? Task.FromException(new IOException("staging interrupted")) : Task.CompletedTask;
        });
        client.Setup(c => c.GetAttributesAsync(destination, token)).ThrowsAsync(new SftpPathNotFoundException());
        client.Setup(c => c.RenameFileAsync(It.Is<string>(p => p == staged), destination, token))
            .Returns(replyLost ? Task.FromException(new IOException("commit reply lost")) : Task.CompletedTask);
        client.Setup(c => c.ExistsAsync(It.Is<string>(p => p == staged), CancellationToken.None)).ReturnsAsync(false);

        if (replyLost)
        {
            await Assert.ThrowsAsync<IOException>(() => fileSystem.UploadStreamAtomicallyAsync(client.Object, stream, destination, true, token));
        }
        else
        {
            await fileSystem.UploadStreamAtomicallyAsync(client.Object, stream, destination, true, token);
        }

        Assert.Equal(2, attempts);
        client.Verify(c => c.RenameFileAsync(staged!, destination, token), Times.Once());
    }

    [Fact]
    public async Task Upload_WhenOverwriteReplyIsLost_DoesNotReplaceDestinationAgainAsync()
    {
        const string destination = "data/a.txt";
        var token = TestContext.Current.CancellationToken;
        using var fileSystem = new SftpFileSystem(CreateOptions());
        using var stream = new MemoryStream([1]);
        var client = new Mock<ISftpClient>(MockBehavior.Strict);
        string? staged = null;
        client.Setup(c => c.UploadFileAsync(stream, It.IsRegex(@"^data/\.upload-[0-9a-f]{32}\.tmp$"), token))
            .Callback<Stream, string, CancellationToken>((_, path, _) => staged = path)
            .Returns(Task.CompletedTask);
        // SSH.NET 不公开属性构造函数；0x8000 是 SFTP 普通文件类型位。
        var attributes = (Renci.SshNet.Sftp.SftpFileAttributes)Activator.CreateInstance(
            typeof(Renci.SshNet.Sftp.SftpFileAttributes), BindingFlags.NonPublic | BindingFlags.Instance,
            null, [DateTime.UtcNow, DateTime.UtcNow, 0L, 0, 0, 0x8000u, null], null)!;
        client.Setup(c => c.GetAttributesAsync(destination, token)).ReturnsAsync(attributes);
        client.Setup(c => c.RenameFile(It.Is<string>(p => p == staged), destination, true))
            .Throws(new IOException("commit reply lost"));
        client.Setup(c => c.ExistsAsync(It.Is<string>(p => p == staged), CancellationToken.None)).ReturnsAsync(false);

        await Assert.ThrowsAsync<IOException>(() => fileSystem.UploadStreamAtomicallyAsync(client.Object, stream, destination, true, token));

        client.Verify(c => c.RenameFile(staged!, destination, true), Times.Once());
        client.Verify(c => c.UploadFileAsync(stream, staged!, token), Times.Once());
        client.Verify(c => c.DeleteFileAsync(destination, It.IsAny<CancellationToken>()), Times.Never());
    }

    private static SftpFileSystemOptions CreateOptions(int port = 22)
    {
        return new SftpFileSystemOptions
        {
            HostKeyFingerprintSha256 = MatchingPin,
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

        public void RaiseHostKeyReceived(HostKeyEventArgs args)
        {
            // 离线触发 SSH.NET 的事件分发入口，同时验证实际客户端已经订阅，而不只测试比较函数。
            var dispatch = typeof(BaseClient).GetMethod("Session_HostKeyReceived", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(dispatch);
            dispatch.Invoke(Client, [Client, args]);
        }

        public bool InvokeIsRetryableException(Exception exception) => IsRetryableException(exception);

        public void InvokeHandleException(string operation, Exception exception, string? path = null)
            => HandleException(operation, exception, path);
    }
}
