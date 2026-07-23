# Linger.HttpClient.Standard

Production-ready HTTP client implementation based on System.Net.Http.HttpClient.

## Features

- **Zero Dependencies**: Built on standard .NET libraries
- **HttpClientFactory Integration**: Proper socket management and connection pooling
- **Proper Resource Management**: Automatic disposal tracking with ownership pattern to prevent resource leaks
- **Streaming Download Support**: `DownloadStreamAsync` and `DownloadToFileAsync` for large-file scenarios
- **Optional Response Mode**: `Buffered` / `Streamed` response reading modes
- **Comprehensive Logging**: Built-in performance monitoring
- **Linger.Results Integration**: Seamless error mapping from server to client
- **ProblemDetails Support**: Native RFC 7807 support

## Installation

```bash
dotnet add package Linger.HttpClient.Standard
```

## Quick Start

```csharp
// Program.cs / Startup.cs
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// In any business service
public sealed class UserQueryService
{
    private readonly IHttpClient _httpClient;

    public UserQueryService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<User?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.CallApi<User>($"api/users/{id}", cancellationToken: cancellationToken);

        if (result.IsSuccess && result.Data is not null)
        {
            return result.Data;
        }

        Console.WriteLine($"Request failed: {(int)result.StatusCode} - {result.ErrorMsg}");

        foreach (var error in result.Errors)
        {
            Console.WriteLine($"Error item: {error.Code} - {error.Message}");
        }

        return null;
    }
}

// Called from a controller or page
var user = await userQueryService.GetAsync(123);

if (user is not null)
{
    Console.WriteLine($"User: {user.Name}");
}
else
{
    Console.WriteLine("No user returned. Check the error output above.");
}
```

Key points:
- Prefer HttpClientFactory in production.
- Check `ErrorMsg` and `Errors` first when a call fails; do not rely on the status code alone.
- Prefer `DownloadStreamAsync` / `DownloadToFileAsync` for large files.

## Basic Usage

### Recommended: Using HttpClientFactory

```csharp
// Register in DI container
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// Use in service
public class UserService
{
    private readonly IHttpClient _httpClient;

    public UserService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<User?> GetUserAsync(int id)
    {
        var result = await _httpClient.CallApi<User>($"api/users/{id}");
        return result.IsSuccess ? result.Data : null;
    }
}
```

### Using Existing HttpClient Instance

If you already have an `HttpClient` instance (e.g., from HttpClientFactory), you can wrap it:

```csharp
// The StandardHttpClient will NOT dispose the external HttpClient
var httpClient = httpClientFactory.CreateClient("MyClient");
using var standardClient = new StandardHttpClient(httpClient, logger);

var result = await standardClient.CallApi<User>("api/users/123");
```

### Direct Instantiation (Not Recommended for Production)

Only use this approach for testing or simple scenarios:

```csharp
// ⚠️ Creates new HttpClient instance
// StandardHttpClient will dispose it when disposed
using var client = new StandardHttpClient("https://api.example.com", logger);
var result = await client.CallApi<User>("api/users/123");
// HttpClient is automatically disposed here
```

**Why HttpClientFactory is Recommended:**
- Proper connection pooling
- Automatic DNS refresh handling
- Prevents socket exhaustion
- Built-in lifetime management

## Linger.Results Integration

Integrates with Linger.Results for unified error handling:

```csharp
// Server using Linger.Results
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetUserAsync(id);
    return result.ToActionResult(); // Automatic HTTP status mapping
}

// Client automatically receives structured errors
var apiResult = await _httpClient.CallApi<User>($"api/users/{id}");
if (!apiResult.IsSuccess)
{
    foreach (var error in apiResult.Errors)
        Console.WriteLine($"Error: {error.Code} - {error.Message}");
}
```

## ProblemDetails Support
See the full request/response mapping and error contract in
[REQUEST_RESPONSE_MAPPING.zh-CN.md](REQUEST_RESPONSE_MAPPING.zh-CN.md).

Short summary: the client prefers `ProblemDetails.detail` as the global message;
if absent it uses the first message from `errors` (each `errors` value is an array).
The `Errors` list preserves all individual error items for fine-grained handling.

## Call Flow and Response Mapping

See [REQUEST_RESPONSE_MAPPING.zh-CN.md](REQUEST_RESPONSE_MAPPING.zh-CN.md) for controller / minimal API examples and status-code mapping.

## Custom Error Handling

`StandardHttpClient` can be inherited. If a server returns neither Linger.Results nor RFC 7807 ProblemDetails, override the error parsing logic to adapt custom formats.

The most common extension point is `GetErrorMessageAsync` in `HttpClientBase`, which converts the custom error body into `ErrorMsg` and `Errors`.

```csharp
public class CustomHttpClient : StandardHttpClient
{
    public CustomHttpClient(HttpClient httpClient, ILogger<StandardHttpClient>? logger = null)
        : base(httpClient, logger)
    {
    }

    protected override async Task<(string ErrorMsg, IEnumerable<Error> Errors)> GetErrorMessageAsync(HttpResponseMessage response)
    {
        var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        // Parse your custom error format here
        // Example: {"code":"BusinessRule","message":"Out of stock"}

        return await base.GetErrorMessageAsync(response).ConfigureAwait(false);
    }
}
```

## Server Conventions

Follow these conventions for more stable error mapping:

1. Response content type
- Use `application/problem+json` for validation errors (RFC 7807).
- Use an error array (`IEnumerable<Error>`) for business errors.

2. Error payload structure
- ProblemDetails: include `title`, `status`, and `errors`.
- Error array: each item should include `code` and `message`.

3. Status code conventions
- Parameter or validation failure: 400 / 422
- Unauthorized or authentication failure: 401 / 403
- Resource not found: 404
- Business conflict: 409

## Core Methods

### CallApi<T>
```csharp
public async Task<ApiResult<T>> CallApi<T>(
    string url,
    HttpMethodEnum method,
    object? requestBody = null,
    object? queryParams = null,
    int? timeout = null,
    CancellationToken cancellationToken = default)
```

Supported HTTP methods:
- GET: Retrieve data
- POST: Create resource
- PUT: Update resource
- DELETE: Delete resource

### Streaming Download

For large file downloads, use streaming methods to minimize memory consumption:

#### DownloadStreamAsync
```csharp
// Download large file as stream (minimal memory usage)
var result = await _httpClient.DownloadStreamAsync("https://example.com/large-file.zip");
if (result.IsSuccess && result.Data is not null)
{
    using var stream = result.Data;
    // Process stream directly without loading entire file into memory
    // Remember to dispose the stream when done
}
```

#### DownloadToFileAsync (Recommended)
```csharp
// Download directly to file with progress reporting
var progress = new Progress<(long downloaded, long? total)>(p =>
{
    var percent = p.total.HasValue ? (double)p.downloaded / p.total.Value * 100 : 0;
    Console.WriteLine($"Downloaded: {p.downloaded} bytes ({percent:F1}%)");
});

var result = await _httpClient.DownloadToFileAsync(
    url: "https://example.com/large-file.zip",
    destinationPath: "output.zip",
    progress: progress
);

if (result.IsSuccess)
{
    Console.WriteLine("Download completed successfully!");
}
```

**Benefits of Streaming Download:**
- ✅ Minimal memory usage (~8KB buffer vs full file size)
- ✅ Supports files of any size
- ✅ Built-in progress reporting
- ✅ Cancellation token support

`DownloadToFileAsync` does not write directly to the final path. It commits a same-directory temporary file only after a successful flush. Cancellation or transfer failure removes the temporary file and leaves an existing destination unchanged. Cancellation is reported by throwing `OperationCanceledException`.

To access the raw HTTP response, request `HttpResponseMessage` and dispose the returned instance:

```csharp
var result = await _httpClient.CallApi<HttpResponseMessage>(url);
if (result.IsSuccess)
{
    using var response = result.Data;
    // Inspect headers or content directly.
}
```

**Performance Comparison (Downloading 500MB file):**

| Method | Memory Usage | Notes |
|--------|-------------|-------|
| `CallApi<byte[]>` | ~500MB | Loads entire file into memory |
| `DownloadStreamAsync` | ~8KB | Only buffer memory usage |
| `DownloadToFileAsync` | ~8KB | Customizable buffer size |

#### HttpResponseMode (`Buffered` / `Streamed`)

Choose the response reading mode based on the scenario:

- `Buffered`: Suitable for small responses or cases where the full content must be read at once
- `Streamed`: Suitable for large responses or download scenarios, processing data incrementally with lower memory usage

| Scenario | Recommended Mode | Reason |
|------|----------|------|
| Regular JSON APIs (small to medium responses) | `Buffered` | Simple and easy to deserialize directly |
| File download / export | `Streamed` | Avoids loading the whole payload into memory and reduces peak memory usage |
| Potentially huge responses (logs, reports, binary data) | `Streamed` | More stable and reduces OOM risk |
| Need full content before unified processing | `Buffered` | Business logic is simpler |

**Performance comparison (downloading a 500 MB file):**

| Method | Memory Usage | Notes |
|------|---------|------|
| `CallApi<byte[]>` | ~500 MB | Loads the entire file into memory |
| `DownloadStreamAsync` | ~8 KB | Buffer-only memory usage |
| `DownloadToFileAsync` | ~8 KB | Customizable buffer size |

## Error Handling

```csharp
var result = await _httpClient.CallApi<User>("api/users/123");

if (result.IsSuccess)
{
    var user = result.Data;
}
else
{
    // Check HTTP status code
    switch (result.StatusCode)
    {
        case HttpStatusCode.NotFound:
            Console.WriteLine("User not found");
            break;
        case HttpStatusCode.Unauthorized:
            Console.WriteLine("Authentication required");
            break;
    }

    // Access detailed errors
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error.Code} - {error.Message}");
    }
}
```

## Common Pitfalls

- Do not use `CallApi<byte[]>` to download large files: it loads the entire response into memory.
- Dispose the stream promptly after `DownloadStreamAsync`; `using` is recommended.
- Pass a cancellation token to download tasks so timeouts or user cancellation can stop quickly.
- Catch `OperationCanceledException` when cancellation is an expected application flow; `DownloadToFileAsync` does not convert cancellation into a failed `ApiResult`.
- Do not manage the lifecycle of an external `HttpClient` twice when wrapping an instance created by a factory.
- Handle structured errors consistently and prefer the `Errors` list over status-code-only checks.

## Best Practices

- Use HttpClientFactory for dependency injection
- Use `using` statements to ensure proper resource disposal
- Enable detailed logging for debugging
- Set reasonable timeout values
- Handle network exceptions and timeouts
- **Use streaming methods for large file downloads** (`DownloadStreamAsync` or `DownloadToFileAsync`) to save memory

## More Examples

For complete streaming download examples and performance comparisons, see [STREAMING_DOWNLOAD_EXAMPLE.md](STREAMING_DOWNLOAD_EXAMPLE.md)
