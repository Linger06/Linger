# Linger.HttpClient.Standard

Production-ready HTTP client implementation based on System.Net.Http.HttpClient.

## Features

- **Zero Dependencies**: Built on standard .NET libraries
- **HttpClientFactory Integration**: Proper socket management and connection pooling
- **Proper Resource Management**: Automatic disposal tracking with ownership pattern to prevent resource leaks
- **Comprehensive Logging**: Built-in performance monitoring
- **Linger.Results Integration**: Seamless error mapping from server to client
- **ProblemDetails Support**: Native RFC 7807 support

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

## Request-Response Contract Mapping

This section details: What does the WebAPI return? → How to call HttpClient.CallApi? → What does the result ApiResult<T> contain?

### Scenario 1: Success Case

**WebAPI returns 200 OK with valid JSON data**

```
WebAPI Response:
HTTP/1.1 200 OK
Content-Type: application/json
{
    "id": 123,
    "name": "John Doe",
    "email": "john@example.com"
}
```

**Client call and return value mapping:**

```csharp
// Client call
var result = await _httpClient.CallApi<User>("api/users/123", cancellationToken: ct);

// Return value field mapping:
// result.IsSuccess       = true
// result.Data            = User { Id = 123, Name = "John Doe", Email = "john@example.com" }
// result.StatusCode      = 200
// result.ErrorMsg        = null
// result.Errors          = Empty array

if (result.IsSuccess && result.Data is not null)
{
    var user = result.Data; // Use the deserialized object directly
    Console.WriteLine($"User: {user.Name}");
}
```

### Scenario 2: Validation Error (ProblemDetails)

**WebAPI returns 422 with validation errors**

```
WebAPI Response:
HTTP/1.1 422 Unprocessable Entity
Content-Type: application/problem+json
{
    "title": "One or more validation errors occurred.",
    "status": 422,
    "errors": {
        "Email": "Invalid email format",
        "Age": "Age must be greater than 18"
    }
}
```

**Client call and return value mapping:**

```csharp
// Client sends POST request with invalid data
var invalidUser = new User { Email = "invalid-email", Age = 10 };
var result = await _httpClient.CallApi<User>(
    "api/users",
    HttpMethodEnum.Post,
    requestBody: invalidUser
);

// Return value field mapping:
// result.IsSuccess       = false
// result.Data            = null (because IsSuccess=false)
// result.StatusCode      = 422
// result.ErrorMsg        = "Email: Invalid email format\nAge: Age must be greater than 18" (auto-merged)
// result.Errors          = [
//     Error { Code = "Email", Message = "Invalid email format" },
//     Error { Code = "Age", Message = "Age must be greater than 18" }
// ]

if (!result.IsSuccess)
{
    // Display global error message
    Console.WriteLine($"Validation failed: {result.ErrorMsg}");
    
    // Or iterate for form inline hints
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Field {error.Code}: {error.Message}");
    }
}
```

### Scenario 3: Business Error (Linger.Results Format)

**WebAPI returns 409 Conflict with business error array**

```
WebAPI Response:
HTTP/1.1 409 Conflict
Content-Type: application/json
[
    {
        "code": "InsufficientStock",
        "message": "Insufficient stock: need 10 but only 5 available"
    },
    {
        "code": "PaymentGatewayDown",
        "message": "Payment gateway is temporarily unavailable"
    }
]
```

**Client call and return value mapping:**

```csharp
// Client sends POST request to submit order
var order = new OrderSubmitRequest { /* ... */ };
var result = await _httpClient.CallApi<Order>(
    "api/orders/submit",
    HttpMethodEnum.Post,
    requestBody: order
);

// Return value field mapping:
// result.IsSuccess       = false
// result.Data            = null (because IsSuccess=false)
// result.StatusCode      = 409
// result.ErrorMsg        = "InsufficientStock: Insufficient stock: need 10 but only 5 available\n
//                           PaymentGatewayDown: Payment gateway is temporarily unavailable" (auto-merged)
// result.Errors          = [
//     Error { Code = "InsufficientStock", Message = "Insufficient stock: need 10 but only 5 available" },
//     Error { Code = "PaymentGatewayDown", Message = "Payment gateway is temporarily unavailable" }
// ]

if (!result.IsSuccess)
{
    // Global error message with all business errors auto-merged
    Console.WriteLine($"Order submission failed: {result.ErrorMsg}");
    
    // Iterate specific error codes for different handling
    foreach (var error in result.Errors)
    {
        switch (error.Code)
        {
            case "InsufficientStock":
                Console.WriteLine("Please adjust your cart quantity");
                break;
            case "PaymentGatewayDown":
                Console.WriteLine("Please try again later or use alternative payment method");
                break;
        }
    }
}
```

### Scenario 4: HTTP Error (4xx / 5xx with No Structured Error Body)

**WebAPI returns 500 with unstructured error body (or plain text)**

```
WebAPI Response:
HTTP/1.1 500 Internal Server Error
Content-Type: text/plain
Internal server error occurred
```

**Client call and return value mapping:**

```csharp
// Client call
var result = await _httpClient.CallApi<ReportData>("api/reports/generate");

// Return value field mapping:
// result.IsSuccess       = false
// result.Data            = null
// result.StatusCode      = 500
// result.ErrorMsg        = "Internal server error occurred" (raw response text)
// result.Errors          = Empty array (no structured error info)

if (!result.IsSuccess)
{
    if (result.StatusCode == HttpStatusCode.InternalServerError)
    {
        Console.WriteLine($"Server error: {result.ErrorMsg}");
        Console.WriteLine("Please try again later or contact support");
    }
}
```

### Scenario 5: Custom Error Format

**WebAPI returns custom format error (neither ProblemDetails nor Linger.Results array)**

```
WebAPI Response:
HTTP/1.1 400 Bad Request
Content-Type: application/json
{
    "error_code": "CUSTOM_ERROR",
    "error_message": "Custom error message",
    "details": "This is a custom format error"
}
```

**Client needs to inherit StandardHttpClient to handle custom format:**

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
        
        try
        {
            // Parse custom error format
            var customError = JsonSerializer.Deserialize<CustomErrorFormat>(responseText);
            if (customError is not null)
            {
                var errorMsg = $"{customError.ErrorCode}: {customError.ErrorMessage}";
                var errors = new[] { new Error(customError.ErrorCode, customError.Details) };
                return (errorMsg, errors);
            }
        }
        catch
        {
            // Parse failed, fallback to default handling
        }
        
        // Fallback to default ProblemDetails / Linger.Results handling
        return await base.GetErrorMessageAsync(response).ConfigureAwait(false);
    }

    private record CustomErrorFormat(string ErrorCode, string ErrorMessage, string Details);
}

// Register custom client
services.AddHttpClient<IHttpClient, CustomHttpClient>();

// Return value field mapping remains the same:
// result.IsSuccess       = false
// result.StatusCode      = 400
// result.ErrorMsg        = "CUSTOM_ERROR: Custom error message"
// result.Errors          = [
//     Error { Code = "CUSTOM_ERROR", Message = "This is a custom format error" }
// ]
```

### ApiResult<T> Field Reference Table

| Field | Type | On Success | On Failure | Description |
|-------|------|-----------|-----------|------------|
| `IsSuccess` | `bool` | `true` | `false` | Indicates if the call succeeded |
| `Data` | `T` | Deserialized object | `null` | Only meaningful when IsSuccess=true |
| `StatusCode` | `HttpStatusCode?` | `200` etc (2xx) | `400` / `401` / `404` / `422` / `500` etc | HTTP status code |
| `ErrorMsg` | `string?` | `null` | Merged error message | Auto-merges Errors list; raw response text if unstructured |
| `Errors` | `IEnumerable<Error>` | Empty collection | Error details list | Code and Message meaning depends on error type (field/business/custom) |

### CallApi Method Quick Reference

```csharp
// 1. Simple GET request
var result = await _httpClient.CallApi<User>("api/users/123");

// 2. GET request with query parameters
var result = await _httpClient.CallApi<IEnumerable<User>>(
    "api/users",
    queryParams: new { page = 1, pageSize = 10 }
);

// 3. POST request with request body
var result = await _httpClient.CallApi<User>(
    "api/users",
    HttpMethodEnum.Post,
    requestBody: new { name = "John Doe", email = "john@example.com" }
);

// 4. PUT request with request body
var result = await _httpClient.CallApi<User>(
    "api/users/123",
    HttpMethodEnum.Put,
    requestBody: new { name = "Jane Doe", email = "jane@example.com" }
);

// 5. DELETE request
var result = await _httpClient.CallApi<object>(
    "api/users/123",
    HttpMethodEnum.Delete
);

// 6. With timeout and cancellation token
var result = await _httpClient.CallApi<User>(
    "api/users/123",
    timeout: 5000,
    cancellationToken: ct
);

// 7. Full parameters
var result = await _httpClient.CallApi<User>(
    "api/users/123",
    HttpMethodEnum.Get,
    requestBody: null,
    queryParams: new { includeDetails = true },
    timeout: 5000,
    cancellationToken: ct
);
```

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
