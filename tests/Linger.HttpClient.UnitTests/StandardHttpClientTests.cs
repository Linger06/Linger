using System.Net;
using System.Net.Http;
using System.Text;
using Linger.HttpClient.Contracts.Core;
using Linger.HttpClient.Contracts.Extensions;
using Linger.HttpClient.Contracts.Models;
using Linger.HttpClient.Standard;
using Xunit;

namespace Linger.HttpClient.UnitTests;

public class StandardHttpClientTests
{
    [Fact]
    public void Constructor_WithBaseUrl_CanBeDisposedMoreThanOnce()
    {
        var client = new StandardHttpClient("https://example.test/");

        client.Dispose();
        client.Dispose();
    }

    [Fact]
    public async Task Dispose_WithExternalHttpClient_DoesNotDisposeExternalClient()
    {
        using var httpClient = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = new StandardHttpClient(httpClient);

        client.Dispose();
        using var response = await httpClient.GetAsync("https://example.test/health");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Constructor_WithHandlerAndConfiguration_UsesBaseUrlAndConfiguredHeaders()
    {
        Uri? capturedUri = null;
        string? capturedLanguage = null;
        using var client = new StandardHttpClient(
            "https://example.test/api/",
            new DelegateHandler(request =>
            {
                capturedUri = request.RequestUri;
                capturedLanguage = request.Headers.GetValues("Accept-Language").Single();

                return CreateJsonResponse(HttpStatusCode.OK, "{\"name\":\"Ada\",\"age\":37}");
            }),
            configureClient: httpClient => httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN"));

        var result = await client.GetAsync<ResponseModel>("users/1");

        Assert.True(result.IsSuccess);
        Assert.Equal("https://example.test/api/users/1", capturedUri?.AbsoluteUri);
        Assert.Equal("zh-CN", capturedLanguage);
    }

    [Fact]
    public async Task CallApi_WithTypedJsonResponse_DeserializesResponse()
    {
        using var httpClient = CreateHttpClient(_ => CreateJsonResponse(HttpStatusCode.OK, "{\"name\":\"Ada\",\"age\":37}"));
        var client = new StandardHttpClient(httpClient);

        var result = await client.GetAsync<ResponseModel>("https://example.test/users/1");

        Assert.True(result.IsSuccess);
        Assert.Equal("Ada", result.Data.Name);
        Assert.Equal(37, result.Data.Age);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \r\n\t")]
    public async Task CallApi_WithEmptyJsonResponse_ReturnsDefault(string responseBody)
    {
        using var httpClient = CreateHttpClient(_ => CreateJsonResponse(HttpStatusCode.OK, responseBody));
        var client = new StandardHttpClient(httpClient);

        var result = await client.GetAsync<ResponseModel?>("https://example.test/users/1");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Data);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \r\n\t")]
    public async Task CallApi_WithEmptyNonSeekableJsonResponse_ReturnsDefault(string responseBody)
    {
        var responseContent = new StreamContent(
            new NonSeekableMemoryStream(Encoding.UTF8.GetBytes(responseBody)));
        using var httpClient = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        });
        var client = new StandardHttpClient(httpClient);
        Assert.Null(responseContent.Headers.ContentLength);

        var result = await client.GetAsync<ResponseModel?>("https://example.test/users/1");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task CallApi_WithInvalidJsonResponse_ReturnsError()
    {
        using var httpClient = CreateHttpClient(_ => CreateJsonResponse(HttpStatusCode.OK, "not-json"));
        var client = new StandardHttpClient(httpClient);

        var result = await client.GetAsync<ResponseModel>("https://example.test/users/1");

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMsg));
    }

    [Fact]
    public async Task CallApi_WithProblemDetails_PreservesAllErrors()
    {
        const string responseBody = """
            {
              "title": "One or more validation errors occurred",
              "status": 400,
              "detail": "Please refer to the errors property for additional details.",
              "errors": {
                "": ["First global error", "Second global error"],
                "Name": ["Name is required"]
              }
            }
            """;
        using var httpClient = CreateHttpClient(_ => CreateJsonResponse(
            HttpStatusCode.BadRequest,
            responseBody,
            "application/problem+json"));
        var client = new StandardHttpClient(httpClient);

        var result = await client.GetAsync<ResponseModel>("https://example.test/users/1");
        var errors = result.Errors.ToList();

        Assert.Equal("Please refer to the errors property for additional details.", result.ErrorMsg);
        Assert.Equal(3, errors.Count);
        Assert.Equal(2, errors.Count(error => error.Code == string.Empty));
        Assert.Contains(errors, error => error.Code == "Name" && error.Message == "Name is required");
    }

    [Fact]
    public async Task CallApi_WithLegacyErrorArray_PreservesCompatibility()
    {
        const string responseBody = "[{\"code\":\"Legacy.Error\",\"message\":\"Legacy error message\"}]";
        using var httpClient = CreateHttpClient(_ => CreateJsonResponse(HttpStatusCode.BadRequest, responseBody));
        var client = new StandardHttpClient(httpClient);

        var result = await client.GetAsync<ResponseModel>("https://example.test/users/1");
        var error = Assert.Single(result.Errors);

        Assert.Equal("Legacy error message", result.ErrorMsg);
        Assert.Equal("Legacy.Error", error.Code);
        Assert.Equal("Legacy error message", error.Message);
    }

    [Fact]
    public async Task CallApi_WithUnknownJsonObject_PreservesResponseBody()
    {
        const string responseBody = "{\"message\":\"Custom API error\"}";
        using var httpClient = CreateHttpClient(_ => CreateJsonResponse((HttpStatusCode)418, responseBody));
        var client = new StandardHttpClient(httpClient);

        var result = await client.GetAsync<ResponseModel>("https://example.test/users/1");

        Assert.Equal(responseBody, result.ErrorMsg);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task CallApi_WithLargeErrorResponse_ReadsOnlyBoundedPrefix()
    {
        var responseBody = new string('x', 100_000);
        var responseStream = new CountingReadStream(Encoding.UTF8.GetBytes(responseBody));
        using var httpClient = CreateHttpClient(_ => new HttpResponseMessage((HttpStatusCode)418)
        {
            Content = new StreamingContent(responseStream)
        });
        var client = new StandardHttpClient(httpClient);

        var result = await client.GetAsync<ResponseModel>("https://example.test/users/1");

        Assert.False(result.IsSuccess);
        Assert.True(responseStream.BytesRead < responseBody.Length);
        Assert.Equal(new string('x', 200) + "...", result.ErrorMsg);
    }

    [Fact]
    public async Task CallApi_WhenUserCancels_ThrowsOperationCanceledException()
    {
        using var httpClient = new System.Net.Http.HttpClient(new CancellableHandler());
        var client = new StandardHttpClient(httpClient);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetAsync<ResponseModel>(
            "https://example.test/users/1",
            cancellationToken: cancellationTokenSource.Token));
    }

    [Fact]
    public async Task CallApi_WhenHttpClientTimesOut_ReturnsFailure()
    {
        using var httpClient = new System.Net.Http.HttpClient(new CancellableHandler())
        {
            Timeout = TimeSpan.FromMilliseconds(100)
        };
        var client = new StandardHttpClient(httpClient);

        var result = await client.GetAsync<ResponseModel>("https://example.test/users/1");

        Assert.False(result.IsSuccess);
        Assert.Contains("timed out", result.ErrorMsg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetWithTimeoutAsync_WhenDeadlineExpires_ReturnsFailure()
    {
        using var httpClient = new System.Net.Http.HttpClient(new CancellableHandler())
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
        var client = new StandardHttpClient(httpClient);

        var result = await client.GetWithTimeoutAsync<ResponseModel>(
            "https://example.test/users/1",
            timeout: TimeSpan.FromMilliseconds(100));

        Assert.False(result.IsSuccess);
        Assert.Contains("timed out", result.ErrorMsg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetWithTimeoutAsync_WhenCallerCancels_ThrowsOperationCanceledException()
    {
        using var httpClient = new System.Net.Http.HttpClient(new CancellableHandler())
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
        var client = new StandardHttpClient(httpClient);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GetWithTimeoutAsync<ResponseModel>(
                "https://example.test/users/1",
                timeout: TimeSpan.FromSeconds(5),
                cancellationToken: cancellationSource.Token));
    }

    [Fact]
    public async Task CallApi_DisposesRequestAndResponse()
    {
        HttpRequestMessage? capturedRequest = null;
        var responseContent = new TrackingContent("{\"name\":\"Ada\",\"age\":37}");
        using var httpClient = CreateHttpClient(request =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = responseContent };
        });
        var client = new StandardHttpClient(httpClient);

        var result = await client.CallApi<ResponseModel>(
            "https://example.test/users",
            HttpMethod.Post,
            new { Name = "Ada" });

        Assert.True(result.IsSuccess);
        Assert.True(responseContent.IsDisposed);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => capturedRequest!.Content!.ReadAsStringAsync());
    }

    [Fact]
    public async Task SendAsync_ReturnsCallerOwnedResponse()
    {
        var responseContent = new TrackingContent("raw-response");
        using var httpClient = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = responseContent
        });
        using var standardClient = new StandardHttpClient(httpClient);
        IHttpClient client = standardClient;

        using (var response = await client.SendAsync("https://example.test/events", HttpMethod.Get))
        {
            Assert.False(responseContent.IsDisposed);
            Assert.Equal("raw-response", await response.Content.ReadAsStringAsync());
        }

        Assert.True(responseContent.IsDisposed);
    }

    [Fact]
    public async Task SendAsync_UsesConfiguredClientAndPerRequestValues()
    {
        Uri? capturedUri = null;
        string? capturedDefaultHeader = null;
        string? capturedRequestHeader = null;
        using var client = new StandardHttpClient(
            "https://example.test/api/",
            new DelegateHandler(request =>
            {
                capturedUri = request.RequestUri;
                capturedDefaultHeader = request.Headers.GetValues("X-Client").Single();
                capturedRequestHeader = request.Headers.GetValues("X-Request").Single();

                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }),
            configureClient: httpClient => httpClient.DefaultRequestHeaders.Add("X-Client", "configured"));

        using var response = await client.SendAsync(
            "events",
            HttpMethod.Get,
            queryParams: new { Page = 2 },
            headers: new Dictionary<string, string> { ["X-Request"] = "current" });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.Equal("https://example.test/api/events?Page=2", capturedUri?.AbsoluteUri);
        Assert.Equal("configured", capturedDefaultHeader);
        Assert.Equal("current", capturedRequestHeader);
    }

    [Fact]
    public async Task SendAsync_WithFailureStatus_ReturnsUnparsedResponse()
    {
        const string responseBody = "{\"message\":\"Custom API error\"}";
        using var httpClient = CreateHttpClient(_ => CreateJsonResponse(
            HttpStatusCode.BadRequest,
            responseBody));
        var client = new StandardHttpClient(httpClient);

        using var response = await client.SendAsync("https://example.test/users/1", HttpMethod.Get);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(responseBody, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SendAsync_WhenNetworkFails_ThrowsHttpRequestException()
    {
        using var httpClient = new System.Net.Http.HttpClient(new ThrowingHandler());
        var client = new StandardHttpClient(httpClient);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.SendAsync("https://example.test/users/1", HttpMethod.Get));

        Assert.Equal("Network failed.", exception.Message);
    }

    [Fact]
    public async Task CallApi_AppliesHeadersPerRequestAndDoesNotAppendCulture()
    {
        var requests = new List<(Uri? Uri, string? Authorization)>();
        using var httpClient = CreateHttpClient(request =>
        {
            var authorization = request.Headers.TryGetValues("Authorization", out var values)
                ? values.Single()
                : null;
            requests.Add((request.RequestUri, authorization));

            return CreateJsonResponse(HttpStatusCode.OK, "{}");
        });
        var client = new StandardHttpClient(httpClient);

        await client.GetAsync<ResponseModel>(
            "https://example.test/users",
            new { Page = 2 },
            new Dictionary<string, string> { ["Authorization"] = "Bearer token-a" });
        await client.GetAsync<ResponseModel>("https://example.test/users");

        Assert.Equal("Bearer token-a", requests[0].Authorization);
        Assert.Null(requests[1].Authorization);
        Assert.Contains("Page=2", requests[0].Uri!.Query, StringComparison.Ordinal);
        Assert.DoesNotContain("culture", requests[0].Uri.Query, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UploadFileAsync_StreamsContentAndDisposesInputStream()
    {
        var sawStreamContent = false;
        using var httpClient = CreateHttpClient(request =>
        {
            var multipart = Assert.IsType<MultipartFormDataContent>(request.Content);
            sawStreamContent = multipart.Any(content => content is StreamContent);

            return CreateJsonResponse(HttpStatusCode.OK, "{\"name\":\"uploaded\",\"age\":1}");
        });
        var client = new StandardHttpClient(httpClient);
        var fileStream = new TrackingMemoryStream(Encoding.UTF8.GetBytes("file-content"));

        var result = await client.UploadFileAsync<ResponseModel>(
            "https://example.test/files",
            HttpMethod.Post,
            fileStream,
            "sample.txt");

        Assert.True(result.IsSuccess);
        Assert.True(sawStreamContent);
        Assert.True(fileStream.IsDisposed);
    }

    [Theory]
    [InlineData(HttpStatusCode.Created, "created")]
    [InlineData(HttpStatusCode.Accepted, "queued")]
    [InlineData(HttpStatusCode.NoContent, "")]
    [InlineData(HttpStatusCode.ResetContent, "")]
    [InlineData(HttpStatusCode.PartialContent, "part")]
    [InlineData((HttpStatusCode)299, "extension response")]
    public async Task DownloadToFileAsync_WithoutFullFile_PreservesDestination(HttpStatusCode status, string body)
    {
        using var httpClient = CreateHttpClient(_ => CreateJsonResponse(status, body));
        using var client = new StandardHttpClient(httpClient);
        var directory = CreateTestDirectory();
        var destination = Path.Combine(directory, "file.bin");
        try
        {
            File.WriteAllText(destination, "original");
            var result = await client.DownloadToFileAsync("https://example.test/file", destination, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal("original", File.ReadAllText(destination));
            Assert.False(result.IsSuccess);
            Assert.Equal(status, result.StatusCode);
            Assert.Single(Directory.GetFiles(directory));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task WriteWithoutResult_WithTextResponse_ReturnsSuccess(string method)
    {
        const string url = "https://example.test/items/1";
        var stream = new CountingReadStream(Encoding.UTF8.GetBytes("queued"));
        using var httpClient = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.Accepted)
        {
            Content = new StreamingContent(stream)
        });
        using var client = new StandardHttpClient(httpClient);

        var result = method switch
        {
            "POST" => await client.PostAsync(url, new { }, cancellationToken: TestContext.Current.CancellationToken),
            "PUT" => await client.PutAsync(url, new { }, cancellationToken: TestContext.Current.CancellationToken),
            _ => await client.DeleteAsync(url, cancellationToken: TestContext.Current.CancellationToken)
        };

        Assert.True(result.IsSuccess, result.ErrorMsg);
        Assert.Equal(HttpStatusCode.Accepted, result.StatusCode);
        Assert.Equal(0, stream.BytesRead);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task WriteWithoutResult_WithLegacyClient_PreservesObjectCallAndResult(string method)
    {
        const string url = "https://example.test/items/1";
        var client = new LegacyWriteClient();
        var token = TestContext.Current.CancellationToken;

        var result = method switch
        {
            "POST" => await client.PostAsync(url, new { }, cancellationToken: token),
            "PUT" => await client.PutAsync(url, new { }, cancellationToken: token),
            _ => await client.DeleteAsync(url, cancellationToken: token)
        };

        Assert.IsType<ApiResult<object>>(client.LastResult);
        Assert.Same(client.LastResult, result);
    }

    [Fact]
    public async Task WriteWithoutResult_WithProblemDetails_PreservesErrors()
    {
        using var httpClient = CreateHttpClient(_ => CreateJsonResponse(HttpStatusCode.BadRequest,
            """{"detail":"Invalid item","errors":{"name":["Required"]}}""", "application/problem+json"));
        using var client = new StandardHttpClient(httpClient);

        var result = await client.DeleteAsync("https://example.test/items/1", cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("Invalid item", result.ErrorMsg);
        Assert.Equal("Required", Assert.Single(result.Errors).Message);
    }

#if NETFRAMEWORK
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CallApi_WithPendingLegacyBody_ReturnsTimeout(bool asText)
    {
        var body = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var httpClient = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new PendingContent(body.Task)
        });
        using var client = new StandardHttpClient(httpClient);
        const string url = "https://example.test/file";
        httpClient.Timeout = TimeSpan.FromMilliseconds(250);
        try
        {
            Task<ApiResult> read = ReadAsync();
            Assert.Same(read, await Task.WhenAny(read, Task.Delay(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken)));
            var result = await read;
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Contains("timed out", result.ErrorMsg, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            body.TrySetResult(true);
        }

        async Task<ApiResult> ReadAsync() => asText
            ? await client.GetAsync<string>(url, cancellationToken: TestContext.Current.CancellationToken)
            : await client.GetAsync<byte[]>(url, cancellationToken: TestContext.Current.CancellationToken);
    }

    private sealed class PendingContent(Task body) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => body;

        protected override bool TryComputeLength(out long length)
        {
            length = 0;

            return false;
        }
    }
#endif

    [Theory]
    [InlineData(HttpStatusCode.OK, "new-content")]
    [InlineData(HttpStatusCode.NonAuthoritativeInformation, "new-content")]
    [InlineData(HttpStatusCode.OK, "")]
    [InlineData(HttpStatusCode.NonAuthoritativeInformation, "")]
    public async Task DownloadToFileAsync_WithSuccessfulDownload_ReplacesDestination(HttpStatusCode status, string body)
    {
        var directoryPath = CreateTestDirectory();
        var destinationPath = Path.Combine(directoryPath, "download.bin");
        File.WriteAllText(destinationPath, "old");
        using var httpClient = CreateHttpClient(_ => new HttpResponseMessage(status)
        {
            Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(body)))
        });
        var client = new StandardHttpClient(httpClient);

        try
        {
            var result = await client.DownloadToFileAsync("https://example.test/file", destinationPath, cancellationToken: TestContext.Current.CancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(status, result.StatusCode);
            Assert.Equal(body, File.ReadAllText(destinationPath));
            Assert.Empty(Directory.GetFiles(directoryPath, "*.tmp"));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
        }
    }

    [Fact]
    public async Task DownloadToFileAsync_WithProblemDetails_PreservesErrorsAndDestination()
    {
        using var httpClient = CreateHttpClient(_ => CreateJsonResponse(HttpStatusCode.BadRequest,
            """{"detail":"Invalid download","errors":{"id":["Unknown file"]}}""", "application/problem+json"));
        using var client = new StandardHttpClient(httpClient);
        var directory = CreateTestDirectory();
        var destination = Path.Combine(directory, "file.bin");
        try
        {
            File.WriteAllText(destination, "original");
            var result = await client.DownloadToFileAsync("https://example.test/file", destination, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Equal("Invalid download", result.ErrorMsg);
            Assert.Equal("Unknown file", Assert.Single(result.Errors).Message);
            Assert.Equal("original", File.ReadAllText(destination));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task DownloadToFileAsync_WhenCancelled_PreservesDestinationAndRemovesTemporaryFile()
    {
        var directoryPath = CreateTestDirectory();
        var destinationPath = Path.Combine(directoryPath, "download.bin");
        File.WriteAllText(destinationPath, "original");
        using var httpClient = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("new-content")))
        });
        var client = new StandardHttpClient(httpClient);
        using var cancellationTokenSource = new CancellationTokenSource();
        var progress = new InlineProgress<(long downloaded, long? total)>(_ => cancellationTokenSource.Cancel());

        try
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.DownloadToFileAsync(
                "https://example.test/file",
                destinationPath,
                2,
                progress,
                cancellationToken: cancellationTokenSource.Token));

            Assert.Equal("original", File.ReadAllText(destinationPath));
            Assert.Empty(Directory.GetFiles(directoryPath, "*.tmp"));
        }
        finally
        {
            Directory.Delete(directoryPath, true);
        }
    }

    [Fact]
    public async Task CallApi_WhenNetworkFails_DoesNotExposeStackTrace()
    {
        using var httpClient = new System.Net.Http.HttpClient(new ThrowingHandler());
        var client = new StandardHttpClient(httpClient);

        var result = await client.GetAsync<ResponseModel>("https://example.test/users/1");

        Assert.Equal("Network failed.", result.ErrorMsg);
        Assert.DoesNotContain(nameof(ThrowingHandler), result.ErrorMsg, StringComparison.Ordinal);
    }

    private static System.Net.Http.HttpClient CreateHttpClient(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        return new System.Net.Http.HttpClient(new DelegateHandler(responseFactory));
    }

    private static HttpResponseMessage CreateJsonResponse(
        HttpStatusCode statusCode,
        string responseBody,
        string mediaType = "application/json")
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseBody, Encoding.UTF8, mediaType)
        };
    }

    private static string CreateTestDirectory()
    {
        var directoryPath = Path.Combine(
            Path.GetTempPath(),
            nameof(StandardHttpClientTests),
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directoryPath);

        return directoryPath;
    }

    // 模拟仅实现原有契约的第三方写客户端，不依赖标准实现或增强接口。
    private sealed class LegacyWriteClient : IHttpClient
    {
        public ApiResult? LastResult { get; private set; }

        public Task<ApiResult<T>> CallApi<T>(string url, HttpMethod method, object? requestBody, object? queryParams,
            IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken)
        {
            var result = new ApiResult<T> { StatusCode = HttpStatusCode.Accepted };
            LastResult = result;

            return Task.FromResult(result);
        }

        public Task<HttpResponseMessage> SendAsync(string url, HttpMethod method, object? requestBody, object? queryParams,
            IReadOnlyDictionary<string, string>? headers, HttpCompletionOption completionOption, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ApiResult<T>> UploadFileAsync<T>(string url, HttpMethod method, Stream fileStream, string fileName,
            IReadOnlyDictionary<string, string>? formData, string fileFieldName, string? contentType,
            IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ApiResult> DownloadToFileAsync(string url, string destinationPath, int bufferSize,
            IProgress<(long downloaded, long? total)>? progress, IReadOnlyDictionary<string, string>? headers,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class ResponseModel
    {
        public string? Name { get; init; }
        public int Age { get; init; }
    }

    [Theory]
    [InlineData(8)]
    [InlineData(1)]
    [InlineData(null)]
    public async Task DownloadToFileAsync_ChecksDeclaredLengthBeforeReplacingDestination(int? declaredLength)
    {
        using var httpClient = CreateHttpClient(_ =>
        {
            var content = new StreamingContent(new MemoryStream(Encoding.UTF8.GetBytes("ab")));
            content.Headers.ContentLength = declaredLength;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        });
        using var client = new StandardHttpClient(httpClient);
        var directory = CreateTestDirectory();
        var destination = Path.Combine(directory, "download.bin");
        try
        {
            File.WriteAllText(destination, "original");
            var result = await client.DownloadToFileAsync("https://example.test/file", destination, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(declaredLength is null, result.IsSuccess);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(declaredLength is null ? "ab" : "original", File.ReadAllText(destination));
            Assert.Single(Directory.GetFiles(directory));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private sealed class DelegateHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(responseFactory(request));
        }
    }

    private sealed class CancellableHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(System.Threading.Timeout.Infinite, cancellationToken).ConfigureAwait(false);

            throw new InvalidOperationException("The cancellation token was not observed.");
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Network failed.");
        }
    }

    private sealed class TrackingContent(string content) : HttpContent
    {
        private readonly byte[] _content = Encoding.UTF8.GetBytes(content);
        public bool IsDisposed { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return stream.WriteAsync(_content, 0, _content.Length);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _content.Length;

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class TrackingMemoryStream(byte[] buffer) : MemoryStream(buffer)
    {
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class NonSeekableMemoryStream(byte[] buffer) : MemoryStream(buffer)
    {
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin loc)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class StreamingContent(Stream contentStream) : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return contentStream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;

            return false;
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return Task.FromResult(contentStream);
        }

#if NET5_0_OR_GREATER
        protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(contentStream);
        }
#endif

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                contentStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    private sealed class CountingReadStream(byte[] buffer) : MemoryStream(buffer)
    {
        public int BytesRead { get; private set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = base.Read(buffer, offset, count);
            BytesRead += bytesRead;

            return bytesRead;
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytesRead = base.Read(buffer, offset, count);
            BytesRead += bytesRead;

            return Task.FromResult(bytesRead);
        }

#if NET6_0_OR_GREATER
        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytesRead = base.Read(buffer.Span);
            BytesRead += bytesRead;

            return ValueTask.FromResult(bytesRead);
        }
#endif
    }

    private sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value)
        {
            report(value);
        }
    }
}
