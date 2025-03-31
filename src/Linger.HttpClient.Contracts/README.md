# Linger.HttpClient.Contracts

## Linger HTTP Client Ecosystem

The Linger HTTP client ecosystem consists of three main components:

- **Linger.HttpClient.Contracts**: Interfaces and abstract classes defining standard contracts for HTTP operations (this project)
- **[Linger.HttpClient.Standard](../Linger.HttpClient.Standard/README.md)**: Implementation based on .NET standard HttpClient
- **[Linger.HttpClient.Flurl](../Linger.HttpClient.Flurl/README.md)**: Fluent API implementation based on Flurl.Http

## Table of Contents
- [Introduction](#introduction)
- [Features](#features)
- [Supported .NET Versions](#supported-net-versions)
- [Installation](#installation)
- [Basic Usage](#basic-usage)
- [Dependency Injection Usage](#dependency-injection-usage)
- [Advanced Usage](#advanced-usage)
- [Performance Tips & Best Practices](#performance-tips--best-practices)

## Introduction

Linger.HttpClient.Contracts defines standard interfaces and contracts for HTTP client operations and serves as the foundation for Linger HTTP client implementations. By using unified contracts, you can easily switch between different HTTP client implementations without modifying your business code.

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

- .NET Standard 2.0+
- .NET Framework 4.6.2+
- .NET 6.0+

## Installation

### Via NuGet

To use IHttpClient and its implementations, you need to install one of the following NuGet packages:

#### Option 1: Install Basic HTTP Client

```bash
# Install interfaces and contracts
dotnet add package Linger.HttpClient.Contracts

# Install implementation based on standard HttpClient
dotnet add package Linger.HttpClient.Standard
```

#### Option 2: Install Flurl-based HTTP Client

```bash
# Install interfaces and contracts
dotnet add package Linger.HttpClient.Contracts

# Install Flurl-based implementation
dotnet add package Linger.HttpClient.Flurl
```

#### Option 3: Install Both Implementations (use as needed)

```bash
dotnet add package Linger.HttpClient.Contracts
dotnet add package Linger.HttpClient.Standard
dotnet add package Linger.HttpClient.Flurl
```

### Using Package Manager Console

```powershell
# Install interfaces and contracts
Install-Package Linger.HttpClient.Contracts

# Install implementation
Install-Package Linger.HttpClient.Standard
# or
Install-Package Linger.HttpClient.Flurl
```

## Core Interfaces and Models

### Core Interfaces

#### IHttpClient

The core HTTP client interface that defines standard methods for all HTTP operations:

```csharp
public interface IHttpClient
{
    // Basic HTTP methods
    Task<ApiResult<T>> CallApi<T>(string url, object? queryParams = null, int? timeout = null, 
        CancellationToken cancellationToken = default);
    
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, object? postData = null, 
        object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default);
    
    // File upload and form submission
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? postData, 
        byte[] fileData, string filename, int? timeout = null, CancellationToken cancellationToken = default);
    
    // Configuration and extension
    void SetToken(string token);
    void AddHeader(string name, string value);
    void AddInterceptor(IHttpClientInterceptor interceptor);
    HttpClientOptions Options { get; }
}
```

#### IHttpClientInterceptor

Request/response interceptor interface that allows adding custom logic before and after requests:

```csharp
public interface IHttpClientInterceptor
{
    Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request);
    Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response);
}
```

#### IHttpClientFactory

HTTP client factory interface for creating and managing HTTP client instances:

```csharp
public interface IHttpClientFactory
{
    IHttpClient CreateClient(string baseUrl);
    IHttpClient CreateClient(string baseUrl, Action<HttpClientOptions> configureOptions);
    IHttpClient GetOrCreateClient(string name);
    void RegisterClient(string name, string baseUrl, Action<HttpClientOptions>? configureOptions = null);
}
```

### Core Models

#### ApiResult&lt;T&gt;

Unified result wrapper for API calls:

```csharp
public class ApiResult<T>
{
    // Response data
    public T Data { get; set; } = default!;
    
    // HTTP status code
    public HttpStatusCode? StatusCode { get; set; }
    
    // Error information
    public ErrorObj? Errors { get; set; }
    public string? ErrorMsg { get; set; }
    
    // Helper properties
    public bool IsSuccess => StatusCode.HasValue && (int)StatusCode.Value >= 200 && (int)StatusCode.Value < 300;
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
}
```

#### ApiPagedResult&lt;T&gt;

Paged API result wrapper:

```csharp
public class ApiPagedResult<T>
{
    // Current page data
    public List<T> Data { get; set; } = default!;
    
    // Pagination information
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageCount { get; set; }
    
    // Helper properties
    public bool HasData => Data != null && Data.Count > 0;
    public bool HasNextPage => Page < PageCount;
}
```

#### HttpClientOptions

HTTP client configuration options:

```csharp
public class HttpClientOptions
{
    // Timeout settings
    public int DefaultTimeout { get; set; } = 30;
    
    // Retry settings
    public bool EnableRetry { get; set; } = false;
    public int MaxRetryCount { get; set; } = 3;
    public int RetryInterval { get; set; } = 1000;
    
    // Headers
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
}
```

## Design Philosophy

### Interface Segregation Principle

Linger.HttpClient.Contracts follows the interface segregation principle, separating interfaces with different responsibilities:

- **IHttpClient**: Defines basic HTTP request operations
- **IHttpClientInterceptor**: Focuses on intercepting and modifying requests/responses
- **IHttpClientFactory**: Responsible for client instance creation and management

### Extensibility

The interceptor mechanism is a core extension point, allowing for features such as:

- Request/response logging
- Authentication token automatic refresh
- Request retry and error handling
- Response caching
- Performance monitoring

### Unified Response Handling

All HTTP responses are wrapped in an `ApiResult<T>`, providing a consistent handling pattern:

- Unified success/failure determination
- Type-safe data access
- Structured error information

## Basic Usage Patterns

### Basic Request Handling

```csharp
// Create client
var client = new StandardHttpClient("https://api.example.com");

// Send GET request
var response = await client.CallApi<UserData>("api/users/1");

// Handle response
if (response.IsSuccess)
{
    var user = response.Data;
    Console.WriteLine($"User: {user.Name}");
}
else
{
    Console.WriteLine($"Error: {response.ErrorMsg}");
}
```

### Interceptor Usage Pattern

```csharp
// Define interceptor
public class LoggingInterceptor : IHttpClientInterceptor
{
    public Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request)
    {
        Console.WriteLine($"Sending request: {request.Method} {request.RequestUri}");
        return Task.FromResult(request);
    }
    
    public Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response)
    {
        Console.WriteLine($"Received response: {(int)response.StatusCode}");
        return Task.FromResult(response);
    }
}

// Add interceptor
client.AddInterceptor(new LoggingInterceptor());
```

### Factory Usage Pattern

```csharp
// Create factory
var factory = new DefaultHttpClientFactory();

// Register named client
factory.RegisterClient("users-api", "https://users.example.com", options => {
    options.DefaultTimeout = 20;
    options.EnableRetry = true;
});

// Get named client
var client = factory.GetOrCreateClient("users-api");
```

## Extension Features

The library also provides some extension methods for a more convenient API experience:

```csharp
// GET request
var user = await client.GetAsync<UserData>("api/users/1");

// POST request
var newUser = await client.PostAsync<UserData>("api/users", new { Name = "John Doe" });

// Paged request
var pagedUsers = await client.GetPagedAsync<UserData>("api/users", new { page = 1, pageSize = 20 });
```

## Built-in Interceptors

Linger.HttpClient.Contracts provides a set of built-in interceptors to enhance HTTP client functionality:

### Retry Interceptor

Automatically retries requests that fail due to transient errors (e.g., 503 Service Unavailable, 504 Gateway Timeout, 429 Too Many Requests):

```csharp
// Create and configure retry interceptor
var retryInterceptor = new RetryInterceptor(
    maxRetries: 3, // Maximum retry attempts
    shouldRetry: response => response.StatusCode == HttpStatusCode.ServiceUnavailable, // Custom retry condition
    delayFunc: async retryCount => await Task.Delay((int)Math.Pow(2, retryCount) * 100) // Exponential backoff
);

// Add to HTTP client
client.AddInterceptor(retryInterceptor);
```

### Caching Interceptor

Caches GET responses to reduce server requests:

```csharp
// Create and configure caching interceptor
var cachingInterceptor = new CachingInterceptor(
    defaultCacheDuration: TimeSpan.FromMinutes(10) // Default cache for 10 minutes
);

// Add to HTTP client
client.AddInterceptor(cachingInterceptor);
```

## Performance Monitoring

Built-in performance monitoring helps identify and resolve performance issues:

```csharp
// Get API endpoint performance statistics
var stats = HttpClientMetrics.Instance.GetEndpointStats("api/users");
Console.WriteLine($"Average response time: {stats.AverageResponseTime}ms");
Console.WriteLine($"Success rate: {stats.SuccessRate * 100}%");
```

## Dependency Injection Integration

IHttpClient can be easily integrated with dependency injection containers:

```csharp
// Register service
services.AddScoped<IHttpClient>(provider => 
    new StandardHttpClient("https://api.example.com"));

// Configure options
services.AddScoped<IHttpClient>(provider => 
{
    var client = new StandardHttpClient("https://api.example.com");
    
    // Configure options
    client.Options.DefaultTimeout = 30;
    client.Options.EnableRetry = true;
    
    // Add interceptors
    client.AddInterceptor(new LoggingInterceptor());
    
    return client;
});
```

## Implementation Projects

For detailed usage and examples, refer to the documentation of specific implementation projects:

- [StandardHttpClient Documentation](../Linger.HttpClient.Standard/README.md)
- [FlurlHttpClient Documentation](../Linger.HttpClient.Flurl/README.md)

## Basic Usage
This is a contracts library that defines interfaces and abstract classes. For implementation, use `Linger.HttpClient` or `Linger.HttpClient.Flurl`.

### Simple Call Examples

```csharp
// Create HTTP client
var client = new Linger.HttpClient.Standard.StandardHttpClient("https://api.example.com");

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
// Using Linger.HttpClient.Standard implementation
services.AddScoped<IHttpClient>(provider => 
    new StandardHttpClient("https://api.example.com"));

// Or using Linger.HttpClient.Flurl implementation
services.AddScoped<IHttpClient>(provider => 
    new FlurlHttpClient("https://api.example.com"));
```

### Configuring Options

```csharp
services.AddScoped<IHttpClient>(provider => 
{
    var client = new StandardHttpClient("https://api.example.com");
    
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
    new StandardHttpClient("https://api1.example.com"));
    
services.AddKeyedScoped<IHttpClient>("api2", (provider, key) => 
    new StandardHttpClient("https://api2.example.com"));

// Access through IServiceProvider when using
var api2Client = serviceProvider.GetKeyedService<IHttpClient>("api2");
```

## Using HttpClientFactory

`IHttpClientFactory` provides a unified way to manage HTTP clients with several advantages over creating client instances directly:

- Centralized configuration and management of HTTP clients
- Support for named clients for different scenarios
- Automatic client lifecycle management to prevent resource leaks
- Simplified configuration and interceptor setup

### Registering HttpClientFactory

```csharp
// In your Startup.cs or Program.cs

// Register the default HTTP client factory
services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();

// Or register the Flurl HTTP client factory
services.AddSingleton<IHttpClientFactory, FlurlHttpClientFactory>();

// Pre-register some commonly used named clients
var serviceProvider = services.BuildServiceProvider();
var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

// Register clients for different APIs
factory.RegisterClient("api1", "https://api1.example.com", options => {
    options.DefaultTimeout = 30;
    options.EnableRetry = true;
    options.MaxRetryCount = 3;
});

factory.RegisterClient("api2", "https://api2.example.com", options => {
    options.DefaultTimeout = 60;
    options.EnableRetry = false;
});
```

### Creating Clients Using Factory

```csharp
// Method 1: Using the factory to create temporary clients
public class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public ApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<UserData> GetUserDataAsync(int userId)
    {
        // Create a basic client
        var client = _httpClientFactory.CreateClient("https://api.example.com");
        
        // Or create a client with configuration
        var configuredClient = _httpClientFactory.CreateClient("https://api.example.com", options => {
            options.DefaultTimeout = 15;
            options.EnableRetry = true;
        });
        
        var result = await client.CallApi<UserData>($"users/{userId}");
        return result.Data;
    }
}

// Method 2: Using pre-registered named clients
public class NamedApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public NamedApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<UserData> GetUserFromApi1Async(int userId)
    {
        // Get a pre-registered named client
        var client = _httpClientFactory.GetOrCreateClient("api1");
        var result = await client.CallApi<UserData>($"users/{userId}");
        return result.Data;
    }
    
    public async Task<UserData> GetUserFromApi2Async(int userId)
    {
        var client = _httpClientFactory.GetOrCreateClient("api2");
        var result = await client.CallApi<UserData>($"users/{userId}");
        return result.Data;
    }
}
```

### Registering Named Clients in DI Container

```csharp
// In your Startup.cs or Program.cs

// Register the factory
services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();

// Register a named client as a service
services.AddScoped(provider => {
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    return factory.GetOrCreateClient("api1");
});

// Using keyed injections
services.AddScoped<IHttpClient, IHttpClient>(serviceProvider => {
    var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    return factory.GetOrCreateClient("api2");
}, "api2");

// Using in a service
public class UserService
{
    private readonly IHttpClient _defaultClient; // api1
    private readonly IHttpClient _api2Client;
    
    public UserService(
        IHttpClient defaultClient, 
        [FromKeyedServices("api2")] IHttpClient api2Client)
    {
        _defaultClient = defaultClient;
        _api2Client = api2Client;
    }
    
    // Methods using different clients...
}
```

### Adding Interceptors with Factory

```csharp
// During application startup
var factory = app.Services.GetRequiredService<IHttpClientFactory>();

// Register a client with interceptors
factory.RegisterClient("api-with-logging", "https://api.example.com", options => {
    options.DefaultTimeout = 30;
});

// Get the client and add interceptors
var client = factory.GetOrCreateClient("api-with-logging");
client.AddInterceptor(new LoggingInterceptor(logger));
client.AddInterceptor(new TokenRefreshInterceptor(tokenService, client));

// Register the configured client in the DI container
services.AddSingleton("api-with-logging", client);
```

### Adding Clients Dynamically

```csharp
public class DynamicApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConcurrentDictionary<string, IHttpClient> _clientCache = new();
    
    public DynamicApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<T> CallExternalApiAsync<T>(string apiEndpoint, string baseUrl)
    {
        // Use base URL as cache key
        var client = _clientCache.GetOrAdd(baseUrl, url => {
            // Create new client if not in cache
            return _httpClientFactory.CreateClient(url, options => {
                options.DefaultTimeout = 30;
                options.EnableRetry = true;
            });
        });
        
        var result = await client.CallApi<T>(apiEndpoint);
        return result.Data;
    }
}
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

## Built-in Interceptors

Linger.HttpClient.Contracts provides a set of built-in interceptors to enhance HTTP client functionality:

### Retry Interceptor

Automatically retries requests that fail due to transient errors (e.g., 503 Service Unavailable, 504 Gateway Timeout, 429 Too Many Requests):

```csharp
// Create and configure retry interceptor
var retryInterceptor = new RetryInterceptor(
    maxRetries: 3, // Maximum retry attempts
    shouldRetry: response => response.StatusCode == HttpStatusCode.ServiceUnavailable, // Custom retry condition
    delayFunc: async retryCount => await Task.Delay((int)Math.Pow(2, retryCount) * 100) // Exponential backoff
);

// Add to HTTP client
client.AddInterceptor(retryInterceptor);
```

### Caching Interceptor

Caches GET responses to reduce server requests:

```csharp
// Create and configure caching interceptor
var cachingInterceptor = new CachingInterceptor(
    defaultCacheDuration: TimeSpan.FromMinutes(10) // Default cache for 10 minutes
);

// Add to HTTP client
client.AddInterceptor(cachingInterceptor);
```

### Logging Interceptor

Records detailed information about requests and responses:

```csharp
// Create logging interceptor
var loggingInterceptor = new LoggingInterceptor(
    log => _logger.LogInformation(log) // Use your logging system
);

// Add to HTTP client
client.AddInterceptor(loggingInterceptor);
```

## HTTP Performance Monitoring

Linger.HttpClient now supports performance monitoring to help identify and resolve performance issues:

```csharp
// Record start before sending request
var requestId = HttpClientMetrics.Instance.StartRequest(url);

try
{
    // Execute HTTP request
    var result = await _httpClient.CallApi<UserData>(url);
    
    // Record successful completion
    HttpClientMetrics.Instance.EndRequest(url, requestId, result.IsSuccess);
    
    return result.Data;
}
catch
{
    // Record failure
    HttpClientMetrics.Instance.EndRequest(url, requestId, false);
    throw;
}

// Get performance stats for a specific endpoint
var stats = HttpClientMetrics.Instance.GetEndpointStats("api/users");
Console.WriteLine($"Average response time: {stats.AverageResponseTime}ms");
Console.WriteLine($"Success rate: {stats.SuccessRate * 100}%");

// Get stats for all endpoints
var allStats = HttpClientMetrics.Instance.GetAllStats();
foreach (var entry in allStats)
{
    Console.WriteLine($"Endpoint: {entry.Key}, Requests: {entry.Value.TotalRequests}");
}
```

Performance metrics include:
- Total request count
- Successful/failed request count
- Success rate
- Average/min/max response time
- Currently active requests

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