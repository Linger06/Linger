# Linger.HttpClient.Contracts

[中文](README_zh-CN.md) | English

Standard interfaces and contracts for HTTP client operations.

## Features

- **Interface Decoupling**: Separate business logic from HTTP implementations
- **Implementation Flexibility**: Support multiple HTTP client implementations
- **Testing Friendly**: Easy unit testing and mocking
- **Strongly Typed**: Generic `ApiResult<T>` for type safety
- **Async Support**: Full async/await pattern

## Installation

```bash
# Core contracts
dotnet add package Linger.HttpClient.Contracts

# Production implementation
dotnet add package Linger.HttpClient.Standard
```

## Core Interfaces

### IHttpClient
```csharp
public interface IHttpClient : IDisposable
{
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method = HttpMethodEnum.Get, 
        object? data = null, Dictionary<string, string>? headers = null, 
        Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default);
}
```

### ApiResult<T>
```csharp
public class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public string ErrorMsg { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public Error[] Errors { get; set; }
}
```

## Basic Usage

```csharp
// Register in DI
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

`ApiResult` seamlessly integrates with `Linger.Results`:

```csharp
// Server using Linger.Results
public async Task<Result<User>> GetUserAsync(int id)
{
    var user = await _userRepository.GetUserAsync(id);
    return user is not null ? Result<User>.Success(user) : Result<User>.NotFound("User not found");
}

// Client receives structured errors
var apiResult = await _httpClient.CallApi<User>($"api/users/{id}");
if (!apiResult.IsSuccess)
{
    // Automatically mapped error information
    foreach (var error in apiResult.Errors)
        Console.WriteLine($"Error: {error.Code} - {error.Message}");
}
```

## Error Handling

```csharp
var result = await _httpClient.CallApi<User>("api/users/123");

if (result.IsSuccess)
{
    var user = result.Data;
    // Handle success
}
else
{
    // Handle error
    Console.WriteLine($"HTTP Status: {result.StatusCode}");
    Console.WriteLine($"Error Message: {result.ErrorMsg}");
    
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Detailed Error: {error.Code} - {error.Message}");
    }
}
```

## Best Practices

- Use dependency injection to manage HTTP client lifecycle
- Leverage `ApiResult`'s structured error handling
- Inherit from existing implementations when implementing custom error handling
- Use `CancellationToken` to support request cancellation
- Use mock implementations in unit tests