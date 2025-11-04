# Linger.HttpClient.Standard

Production-ready HTTP client implementation based on System.Net.Http.HttpClient.

## Features

- **Zero Dependencies**: Built on standard .NET libraries
- **HttpClientFactory Integration**: Proper socket management and connection pooling
- **Proper Resource Management**: Automatic disposal tracking with ownership pattern to prevent resource leaks
- **Comprehensive Logging**: Built-in performance monitoring
- **Linger.Results Integration**: Seamless error mapping from server to client
- **ProblemDetails Support**: Native RFC 7807 support

## Recent Updates (v0.9.7+)

### Resource Management Improvements
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
