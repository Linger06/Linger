# Linger.HttpClient.Standard

## Table of Contents
- [Overview](#overview)
- [Linger.Results Integration](#lingerresults-integration)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Usage Examples](#usage-examples)
- [Error Handling](#error-handling)
- [Performance & Monitoring](#performance--monitoring)
- [Troubleshooting](#troubleshooting)

## Overview

**Linger.HttpClient.Standard** is the production-ready implementation of `Linger.HttpClient.Contracts`, built on `System.Net.Http.HttpClient` for real-world applications.

### 🎯 Key Features

- **Zero Dependencies** - Built on standard .NET libraries
- **HttpClientFactory Integration** - Proper socket management
- **Comprehensive Logging** - Built-in performance monitoring
- **Resource Management** - Implements IDisposable
- **Culture Support** - Automatic internationalization handling
- **Linger.Results Integration** - Seamless error mapping from server to client

## Linger.Results Integration

StandardHttpClient's `ApiResult<T>` seamlessly integrates with **Linger.Results** for unified error handling.

### � Error Mapping

| Server (Linger.Results) | Client (ApiResult) | HTTP Status |
|------------------------|-------------------|-------------|
| `Result<T>.NotFound("User not found")` | `ApiResult<T>` with `Errors[0].Code = "NotFound"` | 404 |
| `Result<T>.Failure("Invalid email")` | `ApiResult<T>` with `Errors[0].Code = "Error"` | 400/500 |

### 🚀 Usage Example

```csharp
// Server: API Controller
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetUserAsync(id);
    return result.ToActionResult(); // Automatic HTTP status mapping
}

// Client: Automatically receives structured errors
var apiResult = await _httpClient.CallApi<User>($"api/users/{id}");
if (!apiResult.IsSuccess)
{
    foreach (var error in apiResult.Errors)
        Console.WriteLine($"Error: {error.Code} - {error.Message}");
}
```

### 🔧 Integration with Other APIs

If the server **does not use Linger.Results**, StandardHttpClient still works perfectly:

```csharp
// Standard REST API response
// HTTP 404: { "message": "User not found", "code": "USER_NOT_FOUND" }
var result = await _httpClient.CallApi<User>("api/users/999");
if (!result.IsSuccess)
{
    Console.WriteLine($"Status Code: {result.StatusCode}");
    Console.WriteLine($"Error Message: {result.ErrorMsg}"); // "User not found"
    // result.Errors will be automatically populated from response body
}

// Custom error format
// HTTP 400: { "errors": [{"field": "email", "message": "Invalid format"}] }
var createResult = await _httpClient.CallApi<User>("api/users", HttpMethodEnum.Post, invalidUser);
if (!createResult.IsSuccess)
{
    foreach (var error in createResult.Errors)
    {
        Console.WriteLine($"Field: {error.Code}, Message: {error.Message}");
    }
}

// Simple text error
// HTTP 500: "Internal server error"
var serverErrorResult = await _httpClient.CallApi<User>("api/users/error");
if (!serverErrorResult.IsSuccess)
{
    Console.WriteLine($"Server Error: {serverErrorResult.ErrorMsg}");
    // Even plain text errors are handled correctly
}
```

### 🎛️ Custom Error Parsing

For special API error formats, you can inherit from StandardHttpClient and override the `GetErrorMessageAsync` method:

```csharp
public class CustomApiHttpClient : StandardHttpClient
{
    public CustomApiHttpClient(string baseUrl, ILogger<StandardHttpClient>? logger = null) 
        : base(baseUrl, logger)
    {
    }

    protected override async Task<(string ErrorMessage, Error[] Errors)> GetErrorMessageAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        
        try
        {
            // Custom API error format: { "error": { "message": "xxx", "details": [...] } }
            var errorResponse = JsonSerializer.Deserialize<CustomErrorResponse>(content);
            if (errorResponse?.Error != null)
            {
                var errors = errorResponse.Error.Details?.Select(d => new Error(d.Code, d.Message)).ToArray() 
                           ?? new[] { new Error("API_ERROR", errorResponse.Error.Message) };
                           
                return (errorResponse.Error.Message, errors);
            }
        }
        catch (JsonException)
        {
            // JSON parsing failed, use default handling
        }
        
        // Fallback to default error parsing
        return await base.GetErrorMessageAsync(response);
    }
    
    private class CustomErrorResponse
    {
        public CustomError? Error { get; set; }
    }
    
    private class CustomError
    {
        public string Message { get; set; } = "";
        public CustomErrorDetail[]? Details { get; set; }
    }
    
    private class CustomErrorDetail
    {
        public string Code { get; set; } = "";
        public string Message { get; set; } = "";
    }
}

// Use custom client
services.AddHttpClient<IHttpClient, CustomApiHttpClient>();
```

## Installation

```bash
dotnet add package Linger.HttpClient.Standard
```

## Quick Start

### Basic Usage

```csharp
// Register in DI container
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// Use in your service
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

    public async Task<User?> CreateUserAsync(CreateUserRequest request)
    {
        var result = await _httpClient.CallApi<User>("api/users", HttpMethodEnum.Post, request);
        return result.IsSuccess ? result.Data : null;
    }
}
```

### With Logging

```csharp
services.AddLogging(builder => builder.AddConsole());
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

## Configuration

### HttpClient Options

```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
});
```

### StandardHttpClient Options

```csharp
var client = new StandardHttpClient("https://api.example.com");
client.Options.DefaultTimeout = 30;
client.AddHeader("Authorization", "Bearer token");
```

## Usage Examples

### GET Request

```csharp
var result = await _httpClient.CallApi<UserData>("api/users/123");
if (result.IsSuccess)
{
    Console.WriteLine($"User: {result.Data.Name}");
}
```

### POST with JSON

```csharp
var createRequest = new CreateUserRequest { Name = "John", Email = "john@example.com" };
var result = await _httpClient.CallApi<User>("api/users", HttpMethodEnum.Post, createRequest);
```

### File Upload

```csharp
var fileData = File.ReadAllBytes("document.pdf");
var result = await _httpClient.CallApi<UploadResult>(
    "api/upload", 
    HttpMethodEnum.Post, 
    fileData, 
    headers: new Dictionary<string, string> { ["Content-Type"] = "application/pdf" }
);
```

### With Query Parameters

```csharp
var queryParams = new Dictionary<string, object>
{
    ["page"] = 1,
    ["size"] = 10,
    ["active"] = true
};
var result = await _httpClient.CallApi<PagedResult<User>>("api/users", queryParams: queryParams);
```

## Error Handling

### Linger.Results Compatible Error Handling

Convert `ApiResult<T>` to `Result<T>` for consistent error handling patterns:

```csharp
public async Task<Result<User>> GetUserAsync(int id)
{
    var apiResult = await _httpClient.CallApi<User>($"api/users/{id}");
    
    if (apiResult.IsSuccess)
        return Result<User>.Success(apiResult.Data);
        
    return apiResult.StatusCode switch
    {
        HttpStatusCode.NotFound => Result<User>.NotFound("User not found"),
        HttpStatusCode.BadRequest => Result<User>.Failure(apiResult.ErrorMsg),
        HttpStatusCode.Unauthorized => Result<User>.Failure($"Access denied: {apiResult.ErrorMsg}"),
        _ => Result<User>.Failure($"Server error: {apiResult.ErrorMsg}")
    };
}
```

### ApiResult Pattern

```csharp
var result = await _httpClient.CallApi<UserData>("api/users/123");

if (result.IsSuccess)
{
    // Success case
    var user = result.Data;
    Console.WriteLine($"User: {user.Name}");
}
else
{
    // Error case
    Console.WriteLine($"Error: {result.ErrorMsg}");
    
    // Handle specific status codes
    switch (result.StatusCode)
    {
        case HttpStatusCode.NotFound:
            Console.WriteLine("User not found");
            break;
        case HttpStatusCode.Unauthorized:
            Console.WriteLine("Authentication required");
            break;
        default:
            Console.WriteLine($"HTTP {(int)result.StatusCode}: {result.ErrorMsg}");
            break;
    }
    
    // Access detailed errors
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error Code: {error.Code}, Message: {error.Message}");
    }
}
```

### Exception Handling

```csharp
try
{
    var result = await _httpClient.CallApi<UserData>("api/users/123");
    // Process result...
}
catch (HttpRequestException ex)
{
    // Network-level errors
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (TaskCanceledException ex)
{
    // Timeout errors
    Console.WriteLine($"Request timeout: {ex.Message}");
}
```

## Performance & Monitoring

### Built-in Logging

StandardHttpClient automatically logs:
- **Request/Response details** (Debug level)
- **Performance metrics** (Information level)
- **Errors and warnings** (Warning/Error levels)

```csharp
// Example log output
[INF] HTTP GET https://api.example.com/api/users/123 completed in 245ms (Status: 200)
[DBG] Request Headers: Accept: application/json, User-Agent: MyApp/1.0
[DBG] Response Headers: Content-Type: application/json; charset=utf-8
```

### Performance Monitoring

```csharp
public class MonitoredUserService
{
    private readonly IHttpClient _httpClient;
    private readonly ILogger<MonitoredUserService> _logger;

    public MonitoredUserService(IHttpClient httpClient, ILogger<MonitoredUserService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<User?> GetUserAsync(int id)
    {
        using var activity = Activity.StartActivity("GetUser");
        activity?.SetTag("user.id", id);

        var stopwatch = Stopwatch.StartActivity();
        var result = await _httpClient.CallApi<User>($"api/users/{id}");
        stopwatch.Stop();

        _logger.LogInformation("GetUser completed in {ElapsedMs}ms, Success: {Success}", 
            stopwatch.ElapsedMilliseconds, result.IsSuccess);

        return result.IsSuccess ? result.Data : null;
    }
}
```

## Troubleshooting

### Common Issues

**1. Connection Timeout**
```csharp
// Increase timeout
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});
```

**2. SSL Certificate Issues**
```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
```

**3. Port Exhaustion**
- Always use HttpClientFactory (automatic in DI)
- Never create StandardHttpClient instances manually in loops

**4. Memory Leaks**
```csharp
// ✅ Good: Use DI
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// ❌ Bad: Manual creation without disposal
var client = new StandardHttpClient("https://api.example.com");

// ✅ Good: Manual creation with disposal
using var client = new StandardHttpClient("https://api.example.com");
```

### Debugging Tips

**Enable Detailed Logging**
```json
{
  "Logging": {
    "LogLevel": {
      "Linger.HttpClient.Standard": "Debug"
    }
  }
}
```

**Inspect Network Traffic**
- Use Fiddler, Wireshark, or browser dev tools
- Check request/response headers in logs
- Verify JSON serialization/deserialization

---

## 📖 Related Documentation

- **[Linger.HttpClient.Contracts](../Linger.HttpClient.Contracts/README.md)** - Interface definitions and architecture guidance
- **[Linger.Results](../Linger.Results/README.md)** - Server-side result patterns that integrate seamlessly with ApiResult
- **[Microsoft HttpClientFactory](https://docs.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)** - Official .NET documentation
