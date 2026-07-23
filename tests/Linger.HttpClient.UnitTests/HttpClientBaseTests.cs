using System.Net.Http;
using System.Text;
using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Models;
using Linger.HttpClient.Standard;
using Xunit;

namespace Linger.HttpClient.UnitTests;

public class HttpClientBaseTests
{
    [Fact]
    public async Task DeserializeResponseContent_WithTypedJson_DeserializesFromResponseStream()
    {
        using var response = new HttpResponseMessage
        {
            Content = new StringContent("{\"name\":\"Ada\",\"age\":37}")
        };

        var result = await new TestHttpClient().DeserializeAsync<ResponseModel>(response);

        Assert.Equal("Ada", result.Name);
        Assert.Equal(37, result.Age);
    }

    [Fact]
    public async Task DeserializeResponseContent_WithEmptyTypedJson_ReturnsDefault()
    {
        using var response = new HttpResponseMessage
        {
            Content = new StringContent(string.Empty)
        };

        var result = await new TestHttpClient().DeserializeAsync<ResponseModel?>(response);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeserializeResponseContent_WithEmptyUnknownLengthContent_ReturnsDefault()
    {
        using var response = new HttpResponseMessage
        {
            Content = new StreamContent(new EmptyNonSeekableStream())
        };
        Assert.Null(response.Content.Headers.ContentLength);

        var result = await new TestHttpClient().DeserializeAsync<ResponseModel?>(response);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeserializeResponseContent_WithWhitespaceTypedJson_ReturnsDefault()
    {
        using var response = new HttpResponseMessage
        {
            Content = new StringContent(" \r\n\t")
        };

        var result = await new TestHttpClient().DeserializeAsync<ResponseModel?>(response);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeserializeResponseContent_WithCancelledToken_ThrowsOperationCancelledException()
    {
        using var response = new HttpResponseMessage
        {
            Content = new StringContent("{\"name\":\"Ada\",\"age\":37}")
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => new TestHttpClient().DeserializeAsync<ResponseModel>(response, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task DeserializeResponseContent_WithHttpResponseMessage_ReturnsOriginalResponse()
    {
        using var response = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);

        var result = await new TestHttpClient().DeserializeAsync<HttpResponseMessage>(response);

        Assert.Same(response, result);
    }

    [Fact]
    public async Task CallApi_WithTypedJsonResponse_DeserializesResponse()
    {
        using var httpClient = new System.Net.Http.HttpClient(new StaticResponseHandler("{\"name\":\"Ada\",\"age\":37}"));
        using var client = new StandardHttpClient(httpClient, new HttpClientOptions());

        var result = await client.CallApi<ResponseModel>("https://example.test", HttpMethodEnum.Get, content: null);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ada", result.Data.Name);
        Assert.Equal(37, result.Data.Age);
    }

    [Fact]
    public async Task CallApi_WithInvalidJsonResponse_ReturnsError()
    {
        using var httpClient = new System.Net.Http.HttpClient(new StaticResponseHandler("not-json"));
        using var client = new StandardHttpClient(httpClient, new HttpClientOptions());

        var result = await client.CallApi<ResponseModel>("https://example.test", HttpMethodEnum.Get, content: null);

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMsg));
    }

    [Fact]
    public async Task CallApi_WithBlockedTypedResponseBody_UsesHttpClientTimeout()
    {
        using var httpClient = new System.Net.Http.HttpClient(new BlockingResponseHandler())
        {
            Timeout = TimeSpan.FromMilliseconds(100)
        };
        using var client = new StandardHttpClient(httpClient, new HttpClientOptions());
        using var testTimeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var result = await client.CallApi<ResponseModel>(
            "https://example.test",
            HttpMethodEnum.Get,
            content: null,
            cancellationToken: testTimeoutSource.Token);

        Assert.False(result.IsSuccess);
        Assert.Contains("timed out", result.ErrorMsg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DownloadToFileAsync_WithSuccessfulDownload_ReplacesDestination()
    {
        var directoryPath = CreateTestDirectory();
        var destinationPath = Path.Combine(directoryPath, "download.bin");
        File.WriteAllText(destinationPath, "old");
        var client = new TestHttpClient
        {
            DownloadResult = CreateStreamResult("new-content")
        };

        try
        {
            var result = await client.DownloadToFileAsync("https://example.test/file", destinationPath);

            Assert.True(result.IsSuccess);
            Assert.Equal("new-content", File.ReadAllText(destinationPath));
            Assert.Empty(Directory.GetFiles(directoryPath, "*.tmp"));
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Fact]
    public async Task DownloadToFileAsync_WhenCancelled_PreservesDestinationAndRemovesTemporaryFile()
    {
        var directoryPath = CreateTestDirectory();
        var destinationPath = Path.Combine(directoryPath, "download.bin");
        File.WriteAllText(destinationPath, "original");
        using var cancellationTokenSource = new CancellationTokenSource();
        var client = new TestHttpClient
        {
            DownloadResult = CreateStreamResult("new-content")
        };
        var progress = new InlineProgress<(long downloaded, long? total)>(_ => cancellationTokenSource.Cancel());

        try
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.DownloadToFileAsync(
                "https://example.test/file",
                destinationPath,
                bufferSize: 2,
                progress: progress,
                cancellationToken: cancellationTokenSource.Token));

            Assert.Equal("original", File.ReadAllText(destinationPath));
            Assert.Empty(Directory.GetFiles(directoryPath, "*.tmp"));
        }
        finally
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    [Fact]
    public async Task DownloadToFileAsync_WhenRequestCancellationWasConvertedToResult_ThrowsOperationCanceledException()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var client = new TestHttpClient
        {
            DownloadResult = new ApiResult<Stream>
            {
                ErrorMsg = "Request was cancelled"
            }
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.DownloadToFileAsync(
            "https://example.test/file",
            "unused.bin",
            cancellationToken: cancellationTokenSource.Token));
    }

    private static string CreateTestDirectory()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), nameof(HttpClientBaseTests), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);

        return directoryPath;
    }

    private static ApiResult<Stream> CreateStreamResult(string content)
    {
        return new ApiResult<Stream>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Data = new MemoryStream(Encoding.UTF8.GetBytes(content))
        };
    }

    private sealed class ResponseModel
    {
        public string? Name { get; init; }

        public int Age { get; init; }
    }

    private sealed class TestHttpClient : HttpClientBase
    {
        public ApiResult<Stream>? DownloadResult { get; init; }

        public override void SetToken(string token)
        {
        }

        public override Task<ApiResult<T>> CallApi<T>(
            string url,
            HttpMethodEnum method,
            HttpContent? content = null,
            object? queryParams = null,
            int? timeout = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public override Task<ApiResult<T>> CallApiWithMode<T>(
            string url,
            HttpMethodEnum method,
            HttpContent? content = null,
            object? queryParams = null,
            int? timeout = null,
            HttpResponseMode responseMode = HttpResponseMode.Buffered,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<T> DeserializeAsync<T>(HttpResponseMessage response)
        {
            return DeserializeResponseContent<T>(response);
        }

        public Task<T> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return DeserializeResponseContent<T>(response, cancellationToken);
        }

        public override Task<ApiResult<Stream>> DownloadStreamAsync(
            string url,
            int? timeout = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(DownloadResult ?? throw new InvalidOperationException("Download result is not configured."));
        }
    }

    private sealed class StaticResponseHandler(string responseBody) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody)
            };

            return Task.FromResult(response);
        }
    }

    private sealed class BlockingResponseHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StreamContent(new CancellationBlockingStream())
            };

            return Task.FromResult(response);
        }
    }

    private sealed class CancellationBlockingStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override async Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            await Task.Delay(System.Threading.Timeout.Infinite, cancellationToken).ConfigureAwait(false);

            return 0;
        }

#if NET8_0_OR_GREATER
        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(System.Threading.Timeout.Infinite, cancellationToken).ConfigureAwait(false);

            return 0;
        }
#endif

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class EmptyNonSeekableStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

#if NET8_0_OR_GREATER
        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(0);
        }
#endif

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value)
        {
            report(value);
        }
    }
}
