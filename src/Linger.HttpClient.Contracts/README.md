# Linger.HttpClient.Contracts

## Table of Contents
- [Introduction](#introduction)
- [Features](#features)
- [Supported .NET Versions](#supported-net-versions)
- [Basic Usage](#basic-usage)
- [Dependency Injection Usage](#dependency-injection-usage)
- [Advanced Usage](#advanced-usage)
- [Performance Tips & Best Practices](#performance-tips--best-practices)

## Introduction
Linger.HttpClient.Contracts is a .NET library that provides contract interfaces and base implementations for HTTP client operations. It serves as the foundation for HTTP client implementations in the Linger framework.

## Features
- Strongly typed HTTP client interfaces
- Support for various HTTP methods (GET, POST, PUT, DELETE)
- File upload capabilities
- Request/response interception
- Customizable HTTP options
- Paged result support
- Automatic retry mechanism
- User-friendly error handling

## Supported .NET Versions
This library supports .NET applications that utilize .NET Framework 4.6.2+ or .NET Standard 2.0+.

## Basic Usage
This is a contracts library that defines interfaces and abstract classes. For implementation, use `Linger.HttpClient` or `Linger.HttpClient.Flurl`.

### Simple Call Examples

```csharp
// Create HTTP client
var client = new BaseHttpClient("https://api.example.com");

// GET request
var result = await client.CallApi<UserData>("users/1");

// POST request
var postResult = await client.CallApi<UserData>("users", HttpMethodEnum.Post, 
    new { Name = "John", Email = "john@example.com" });

// GET request with query parameters
var queryResult = await client.CallApi<List<UserData>>("users", 
    new { page = 1, pageSize = 10 });
```

## Dependency Injection Usage
The IHttpClient interface is designed to support dependency injection and can be easily integrated into your applications. Here's how to use it:

### Registering the Service

```csharp
// Using Linger.HttpClient implementation
services.AddScoped<IHttpClient>(provider => 
    new BaseHttpClient("https://api.example.com"));

// Or using Linger.HttpClient.Flurl implementation
services.AddScoped<IHttpClient>(provider => 
    new FlurlHttpClient("https://api.example.com"));
```

### Configuring Options

```csharp
services.AddScoped<IHttpClient>(provider => 
{
    var client = new BaseHttpClient("https://api.example.com");
    
    // Configure options
    client.Options.DefaultTimeout = 30; // Set default timeout to 30 seconds
    client.Options.EnableRetry = true;  // Enable retry
    client.Options.MaxRetryCount = 3;   // Maximum retry count
    client.Options.RetryInterval = 1000; // Retry interval in milliseconds
    
    // Add default headers
    client.AddHeader("User-Agent", "Linger HttpClient");
    client.AddHeader("Accept", "application/json");
    
    // Add request/response interceptors
    client.AddInterceptor(new LoggingInterceptor());
    
    return client;
});
```

### Using in Services

```csharp
public class MyService
{
    private readonly IHttpClient _httpClient;
    
    public MyService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<UserData> GetUserDataAsync(int userId)
    {
        var result = await _httpClient.CallApi<UserData>($"users/{userId}");
        
        if (result.IsSuccess)
        {
            return result.Data;
        }
        
        throw new Exception($"Failed to get user data: {result.ErrorMsg}");
    }
}
```

### Multiple Instance Configuration

If you need to use multiple HTTP clients with different configurations, you can use named injection:

```csharp
services.AddScoped<IHttpClient>(provider => 
    new BaseHttpClient("https://api1.example.com"));
    
services.AddKeyedScoped<IHttpClient>("api2", (provider, key) => 
    new BaseHttpClient("https://api2.example.com"));

// Access through IServiceProvider when using
var api2Client = serviceProvider.GetKeyedService<IHttpClient>("api2");
```

## Advanced Usage

### Implementing Custom Interceptors

```csharp
public class LoggingInterceptor : IHttpClientInterceptor
{
    private readonly ILogger _logger;
    
    public LoggingInterceptor(ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request)
    {
        _logger.LogInformation($"Request: {request.Method} {request.RequestUri}");
        return request;
    }
    
    public async Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response)
    {
        _logger.LogInformation($"Response: {(int)response.StatusCode} {response.ReasonPhrase}");
        return response;
    }
}
```

### File Upload Example

```csharp
// File upload
byte[] fileData = File.ReadAllBytes("document.pdf");
var formData = new Dictionary<string, string>
{
    { "description", "Sample document" },
    { "category", "reports" }
};

var uploadResult = await client.CallApi<UploadResponse>(
    "files/upload", 
    HttpMethodEnum.Post, 
    formData, 
    fileData, 
    "document.pdf"
);
```

### Error Handling

```csharp
public async Task<T> ExecuteApiCall<T>(string endpoint)
{
    try
    {
        var result = await _httpClient.CallApi<T>(endpoint);
        
        if (!result.IsSuccess)
        {
            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await RefreshTokenAsync();
                // Retry the request
                return await ExecuteApiCall<T>(endpoint);
            }
            
            if (result.Errors != null)
            {
                // Handle detailed error information
                throw new ApiException(result.Errors);
            }
            
            throw new ApiException(result.ErrorMsg);
        }
        
        return result.Data;
    }
    catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
    {
        // Handle network errors
        throw new NetworkException("Network request failed", ex);
    }
}
```

## Performance Tips & Best Practices

### HttpClient Instance Management
- **Recommended**: Use dependency injection container to manage HttpClient lifecycle
- Avoid creating new instances for each request which can lead to port exhaustion
- Use `HttpClientFactory` or dependency injection framework

### Request Optimization
- Set reasonable timeout values to prevent requests from hanging indefinitely
- Use streaming for large responses instead of loading them entirely into memory
- Use compression to reduce network load

### Connection Management
- Keep connections alive for frequently accessed APIs
- Use retry strategies for important requests, but avoid infinite retries
- Set appropriate retry intervals to avoid DOS-ing your target server

### Exception Handling
- Always catch and handle HTTP request exceptions
- Implement backoff strategies for server overload situations
- Use circuit breaker pattern to prevent cascading failures