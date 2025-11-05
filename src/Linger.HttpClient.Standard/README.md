# Linger.HttpClient.Standard

Production-ready HTTP client implementation based on System.Net.Http.HttpClient.

## Features

- **Zero Dependencies**: Built on standard .NET libraries
- **HttpClientFactory Integration**: Proper socket management and connection pooling
- **Proper Resource Management**: Automatic disposal tracking with ownership pattern to prevent resource leaks
- **Comprehensive Logging**: Built-in performance monitoring
- **Linger.Results Integration**: Seamless error mapping from server to client
- **ProblemDetails Support**: Native RFC 7807 support

## Recent Updates

### v1.0.0+ - Streaming Download Optimization
- ✅ **Streaming Download Support**: New `DownloadStreamAsync` and `DownloadToFileAsync` methods
- ✅ **Memory Optimization**: Large file download memory usage reduced from 100% file size to ~8KB buffer (99.99% reduction)
- ✅ **Progress Reporting**: `DownloadToFileAsync` supports real-time progress callbacks
- ✅ **HttpResponseMode**: New `Buffered`/`Streamed` modes for different scenarios

### v0.9.8 - Resource Management Improvements
- ✅ **Fixed Resource Leak**: Added ownership tracking to prevent disposing externally-provided `HttpClient` instances
- ✅ **Safe Disposal**: Only disposes `HttpClient` instances that it created internally
- ✅ **HttpClientFactory Compatible**: Properly handles `HttpClient` instances from factory without disposal issues

## Installation

```bash
dotnet add package Linger.HttpClient.Standard
```

## Basic Usage

### ✅ Recommended: Using HttpClientFactory (Best Practice)

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

### ⚠️ Using Existing HttpClient Instance

If you already have an `HttpClient` instance (e.g., from HttpClientFactory), you can wrap it:

```csharp
// The StandardHttpClient will NOT dispose the external HttpClient
var httpClient = httpClientFactory.CreateClient("MyClient");
using var standardClient = new StandardHttpClient(httpClient, logger);

var result = await standardClient.CallApi<User>("api/users/123");
```

### ⚠️ Direct Instantiation (Not Recommended for Production)

Only use this approach for testing or simple scenarios:

```csharp
// ⚠️ Creates new HttpClient instance
// StandardHttpClient will dispose it when disposed
using var client = new StandardHttpClient("https://api.example.com", logger);
var result = await client.CallApi<User>("api/users/123");
// HttpClient is automatically disposed here
```

**Why HttpClientFactory is Recommended:**
- ✅ Proper connection pooling
- ✅ Automatic DNS refresh handling
- ✅ Prevents socket exhaustion
- ✅ Built-in lifetime management

## Linger.Results Integration

Seamless integration with Linger.Results framework for unified error handling:

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

Native support for RFC 7807 ProblemDetails format:

```csharp
// Automatically parse ProblemDetails responses
var result = await _httpClient.CallApi<User>("api/users", HttpMethodEnum.Post, invalidUser);
if (!result.IsSuccess)
{
    Console.WriteLine($"Error: {result.ErrorMsg}");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Field: {error.Code}, Error: {error.Message}");
    }
}
```

## Core Methods

### CallApi<T>
```csharp
public async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method = HttpMethodEnum.Get, 
    object? data = null, Dictionary<string, string>? headers = null)
```

### Streaming Download (New in v0.9.8+)

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

### Streaming Download (New in v0.9.8+)

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

**Performance Comparison (Downloading 500MB file):**

| Method | Memory Usage | Notes |
|--------|-------------|-------|
| `CallApi<byte[]>` | ~500MB | Loads entire file into memory |
| `DownloadStreamAsync` | ~8KB | Only buffer memory usage |
| `DownloadToFileAsync` | ~8KB | Customizable buffer size |

Supported HTTP methods:
- GET: Retrieve data
- POST: Create resource
- PUT: Update resource
- DELETE: Delete resource
- PATCH: Partial update

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

## Best Practices

- Use HttpClientFactory for dependency injection
- Use `using` statements to ensure proper resource disposal
- Enable detailed logging for debugging
- Set reasonable timeout values
- Handle network exceptions and timeouts
- **Use streaming methods for large file downloads** (`DownloadStreamAsync` or `DownloadToFileAsync`) to save memory

## More Examples

For complete streaming download examples and performance comparisons, see [STREAMING_DOWNLOAD_EXAMPLE.md](STREAMING_DOWNLOAD_EXAMPLE.md)
