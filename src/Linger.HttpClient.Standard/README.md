# Linger.HttpClient.Standard

[中文](README_zh-CN.md) | English

Production-ready HTTP client implementation based on System.Net.Http.HttpClient.

## Features

- **Zero Dependencies**: Built on standard .NET libraries
- **HttpClientFactory Integration**: Proper socket management
- **Comprehensive Logging**: Built-in performance monitoring
- **Linger.Results Integration**: Seamless error mapping from server to client
- **ProblemDetails Support**: Native RFC 7807 support

## Installation

```bash
dotnet add package Linger.HttpClient.Standard
```

## Basic Usage

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
