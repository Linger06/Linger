# Linger.HttpClient.Contracts

## Table of Contents
- [Overview](#overview)
  - [Linger HTTP Client Ecosystem](#linger-http-client-ecosystem)
  - [Features](#features)
  - [Supported .NET Versions](#supported-net-versions)
  - [Recent Improvements](#recent-improvements)
- [Installation](#installation)
  - [Via NuGet](#via-nuget)
  - [Using Package Manager Console](#using-package-manager-console)
- [Core Components](#core-components)
  - [Core Interfaces](#core-interfaces)
  - [Core Models](#core-models)
  - [Design Philosophy](#design-philosophy)
- [Usage Guide](#usage-guide)
  - [Basic Usage](#basic-usage)
  - [Extension Features](#extension-features)
  - [Interceptor System](#interceptor-system)
- [Dependency Injection Integration](#dependency-injection-integration)
  - [Basic Registration](#basic-registration)
  - [Using Microsoft's HttpClientFactory](#using-microsofts-httpclientfactory)
  - [Multiple Instance Configuration](#multiple-instance-configuration)
- [Polly Policies Integration](#polly-policies-integration)
  - [Common Policy Types](#common-policy-types)
  - [Configuration Examples](#configuration-examples)
  - [Combining with Linger Interceptors](#combining-with-linger-interceptors)
- [Advanced Features](#advanced-features)
  - [Custom Interceptors](#custom-interceptors)
  - [Error Handling](#error-handling)
  - [Performance Monitoring](#performance-monitoring)
- [Best Practices](#best-practices)
  - [Instance Management](#instance-management)
  - [Request Optimization](#request-optimization)
  - [Exception Handling](#exception-handling)
- [Implementation Projects](#implementation-projects)

## Overview

### Linger HTTP Client Ecosystem

The Linger HTTP client ecosystem consists of three main components:

- **Linger.HttpClient.Contracts**: Interfaces and abstract classes defining standard contracts for HTTP operations (this project)
- **[Linger.HttpClient.Standard](../Linger.HttpClient.Standard/README.md)**: Implementation based on .NET standard HttpClient
- **[Linger.HttpClient.Flurl](../Linger.HttpClient.Flurl/README.md)**: Fluent API implementation based on Flurl.Http

Linger.HttpClient.Contracts defines standard interfaces and contracts for HTTP client operations and serves as the foundation for Linger HTTP client implementations. By using unified contracts, you can easily switch between different HTTP client implementations without modifying your business code.

### Features

- Strongly typed HTTP client interfaces
- Support for various HTTP methods (GET, POST, PUT, DELETE)
- File upload capabilities (simplified with unified MultipartHelper)
- Request/response interception system
- Customizable HTTP options
- Paged result support
- Automatic retry mechanism
- User-friendly error handling
- Built-in compression support
- Performance monitoring and statistics

### Supported .NET Versions

- .NET Standard 2.0+
- .NET Framework 4.6.2+
- .NET 6.0+
- .NET 8.0/9.0

### Recent Improvements

#### 1. Enhanced Interceptor System

The newly designed interceptor system is consistently applied across all client implementations:

```csharp
// Define an interceptor
public class LoggingInterceptor : IHttpClientInterceptor
{
    public async Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request)
    {
        Console.WriteLine($"Sending request: {request.Method} {request.RequestUri}");
        return request;
    }
    
    public async Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response)
    {
        Console.WriteLine($"Received response: {(int)response.StatusCode}");
        return response;
    }
}

// Add the interceptor
client.AddInterceptor(new LoggingInterceptor());
```

#### 2. Default Interceptor Factory

The new `DefaultInterceptorFactory` automatically creates appropriate interceptors based on configuration:

```csharp
// Factory creates interceptors based on HttpClientOptions configuration
var interceptors = DefaultInterceptorFactory.CreateStandardInterceptors(options);
foreach (var interceptor in the interceptors)
{
    client.AddInterceptor(interceptor);
}
```

#### 3. Built-in Compression Support

HTTP compression is supported automatically without additional configuration:

```csharp
// CompressionInterceptor is automatically applied to all clients
var client = factory.CreateClient("https://api.example.com");

// Requests automatically include Accept-Encoding header with gzip and deflate
var response = await client.GetAsync<UserData>("api/users/1");
```

#### 4. Unified File Upload Handling

File uploads are simplified using MultipartHelper:

```csharp
// File upload example
byte[] fileData = File.ReadAllBytes("document.pdf");
var formData = new Dictionary<string, string> { { "description", "Test document" } };

// All client implementations handle file uploads in a unified way
var response = await client.CallApi<UploadResult>(
    "api/files/upload",
    HttpMethodEnum.Post,
    formData,
    fileData,
    "document.pdf"
);
```

#### 5. Advanced Performance Monitoring

Built-in performance monitoring provides detailed HTTP request statistics:

```csharp
// Get performance stats for a specific endpoint
var stats = HttpClientMetrics.Instance.GetEndpointStats("api/users");
Console.WriteLine($"Average response time: {stats.AverageResponseTime}ms");
Console.WriteLine($"Success rate: {stats.SuccessRate * 100}%");
Console.WriteLine($"Active requests: {stats.ActiveRequests}");
```

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

## Core Components

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

### Design Philosophy

#### Interface Segregation Principle

Linger.HttpClient.Contracts follows the interface segregation principle, separating interfaces with different responsibilities:

- **IHttpClient**: Defines basic HTTP request operations
- **IHttpClientInterceptor**: Focuses on intercepting and modifying requests/responses
- **IHttpClientFactory**: Responsible for client instance creation and management

#### Extensibility

The interceptor mechanism is a core extension point, allowing for features such as:

- Request/response logging
- Authentication token automatic refresh
- Request retry and error handling
- Response caching
- Performance monitoring

#### Unified Response Handling

All HTTP responses are wrapped in an `ApiResult<T>`, providing a consistent handling pattern:

- Unified success/failure determination
- Type-safe data access
- Structured error information

## Usage Guide

### Basic Usage

This is a contracts library that defines interfaces and abstract classes. For implementation, use `Linger.HttpClient.Standard` or `Linger.HttpClient.Flurl`.

#### Creating a Client

```csharp
// Create HTTP client
var client = new Linger.HttpClient.Standard.StandardHttpClient("https://api.example.com");
```

#### Sending GET Requests

```csharp
// GET request
var result = await client.CallApi<UserData>("users/1");

// Handle response
if (result.IsSuccess)
{
    var user = result.Data;
    Console.WriteLine($"User: {user.Name}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMsg}");
}
```

#### Sending POST Requests

```csharp
// POST request
var postResult = await client.CallApi<UserData>("users", HttpMethodEnum.Post, 
    new { Name = "John", Email = "john@example.com" });
```

#### Requests with Query Parameters

```csharp
// GET request with query parameters
var queryResult = await client.CallApi<List<UserData>>("users", 
    new { page = 1, pageSize = 10 });
```

### Extension Features

The library also provides some extension methods for a more convenient API experience:

```csharp
// GET request simplification
var user = await client.GetAsync<UserData>("api/users/1");

// POST request simplification
var newUser = await client.PostAsync<UserData>("api/users", new { Name = "John Doe" });

// Paged request simplification
var pagedUsers = await client.GetPagedAsync<UserData>("api/users", new { page = 1, pageSize = 20 });
```

#### File Upload

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

### Interceptor System

Linger.HttpClient.Contracts provides several built-in interceptors to enhance HTTP client functionality:

#### Interceptor Usage Pattern

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

#### Built-in Interceptors

##### Retry Interceptor

```csharp
// Enable retry with simple configuration
client.Options.EnableRetry = true;
client.Options.MaxRetryCount = 3;
client.Options.RetryInterval = 1000; // milliseconds

// Or manually add a custom retry interceptor
var retryInterceptor = new RetryInterceptor(
    options,
    response => response.StatusCode == HttpStatusCode.ServiceUnavailable
);
client.AddInterceptor(retryInterceptor);
```

##### Compression Interceptor

```csharp
// Compression interceptor is automatically added by DefaultInterceptorFactory
// Or add it manually
client.AddInterceptor(new CompressionInterceptor());
```

##### Caching Interceptor

```csharp
// Create and configure caching interceptor
var cachingInterceptor = new CachingInterceptor(
    defaultCacheDuration: TimeSpan.FromMinutes(10)
);
client.AddInterceptor(cachingInterceptor);
```

#### Factory Usage Pattern

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

## Dependency Injection Integration

IHttpClient can be easily integrated with dependency injection containers.

### Basic Registration

```csharp
// Using Linger.HttpClient.Standard implementation
services.AddScoped<IHttpClient>(provider => 
    new StandardHttpClient("https://api.example.com"));

// Or using Linger.HttpClient.Flurl implementation
services.AddScoped<IHttpClient>(provider => 
    new FlurlHttpClient("https://api.example.com"));
```

#### Configuring Options

```csharp
services.AddScoped<IHttpClient>(provider => 
{
    var client = new StandardHttpClient("https://api.example.com");
    
    // Configure options
    client.Options.DefaultTimeout = 30; // Set default timeout to 30 seconds
    client.Options.EnableRetry = true;  // Enable retry
    client.Options.MaxRetryCount = 3;   // Maximum retry count
    
    // Add default headers
    client.AddHeader("User-Agent", "Linger HttpClient");
    client.AddHeader("Accept", "application/json");
    
    // Add request/response interceptors
    client.AddInterceptor(new LoggingInterceptor());
    
    return client;
});
```

#### Using in Services

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

### Using Microsoft's HttpClientFactory

In addition to the methods above, you can also use Microsoft's HttpClientFactory to manage HTTP client lifecycle, avoiding common pitfalls such as DNS changes issues and socket exhaustion:

```csharp
// 1. Basic registration
services.AddHttpClient<IHttpClient, StandardHttpClient>(client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "Linger HttpClient");
});

// 2. Using named clients with options and interceptors
services.AddHttpClient("MyApi", client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddTypedClient<IHttpClient>((httpClient, serviceProvider) => 
{
    // Create client instance
    var client = new StandardHttpClient(httpClient);
    
    // Configure options
    client.Options.DefaultTimeout = 30;
    client.Options.EnableRetry = true;
    client.Options.MaxRetryCount = 3;
    
    // Add interceptors
    var logger = serviceProvider.GetRequiredService<ILogger<IHttpClient>>();
    client.AddInterceptor(new LoggingInterceptor(log => logger.LogInformation(log)));
    client.AddInterceptor(new RetryInterceptor(client.Options));
    
    return client;
});

// 3. Adding message handlers and policies
services.AddHttpClient<IHttpClient, StandardHttpClient>(client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.AddPolicyHandler(GetRetryPolicy()) // Add Polly retry policy
.AddHttpMessageHandler(() => new CustomMessageHandler()); // Add custom handler

// Helper method to get Polly policy
private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

### Using HttpClientFactory in Services

```csharp
public class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpClient _client;
    
    public ApiService(
        IHttpClientFactory httpClientFactory,
        IHttpClient client) // Default injected client
    {
        _httpClientFactory = httpClientFactory;
        _client = client;
    }
    
    public async Task<UserData> GetNamedClientUserAsync(int userId)
    {
        // Get typed named client
        var apiClient = _httpClientFactory.CreateClient("MyApi")
            .GetTypedClient<IHttpClient>();
            
        var result = await apiClient.CallApi<UserData>($"users/{userId}");
        return result.Data;
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

## Polly Policies Integration

[Polly](https://github.com/App-vNext/Polly) is a powerful .NET resilience and transient-fault-handling library that can be seamlessly integrated with both Linger.HttpClient and Microsoft's HttpClientFactory.

### Common Policy Types

1. **Retry Policies** - Automatically retry failed requests
2. **Circuit Breaker Policies** - Temporarily stop attempts when the system detects multiple failures
3. **Timeout Policies** - Set timeout limits for requests
4. **Fallback Policies** - Provide alternative responses when requests fail
5. **Policy Wraps** - Combine multiple policies together

### Configuration Examples

#### Adding Polly Support

```bash
# Install Polly integration package for HttpClientFactory
dotnet add package Microsoft.Extensions.Http.Polly
```

#### Retry Policy Example

```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>(client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.AddPolicyHandler(GetRetryPolicy());

// Define retry policy
private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // Handles network errors and 5xx, 408 responses
        .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests) // Also handle 429 responses
        .WaitAndRetryAsync(
            retryCount: 3, // Retry 3 times
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff: 2, 4, 8 seconds
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                // Log retry information
                Console.WriteLine($"Attempting retry {retryCount}, waiting {timespan.TotalSeconds} seconds");
            });
}
```

#### Circuit Breaker Policy Example

```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>(client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.AddPolicyHandler(GetCircuitBreakerPolicy());

// Define circuit breaker policy
private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5, // Break after 5 failures
            durationOfBreak: TimeSpan.FromSeconds(30), // Stay open for 30 seconds
            onBreak: (ex, breakDelay) => 
            {
                // Circuit is now open
                Console.WriteLine($"Circuit breaker opened, will attempt to reset after {breakDelay.TotalSeconds} seconds");
            },
            onReset: () => 
            {
                // Circuit is now closed again
                Console.WriteLine("Circuit breaker reset, service recovered");
            });
}
```

#### Combining Multiple Policies

```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>(client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.AddPolicyHandler(GetRetryPolicy()) // Apply retry policy first
.AddPolicyHandler(GetCircuitBreakerPolicy()); // Then circuit breaker policy

// Alternatively, explicitly combine using PolicyWrap
private static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
{
    return Policy.WrapAsync(GetRetryPolicy(), GetCircuitBreakerPolicy());
}
```

### Combining with Linger Interceptors

Linger's interceptor system and Polly policies can be combined for greater functionality:

```csharp
// 1. First configure Polly policies
services.AddHttpClient("resilient-api", client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.AddPolicyHandler(GetRetryPolicy())
.AddTypedClient<IHttpClient>((httpClient, serviceProvider) => 
{
    // 2. Create client instance
    var client = new StandardHttpClient(httpClient);
    
    // 3. Add Linger interceptors for scenarios Polly can't handle
    client.AddInterceptor(new LoggingInterceptor(
        log => serviceProvider.GetRequiredService<ILogger<IHttpClient>>().LogInformation(log)
    ));
    client.AddInterceptor(new TokenRefreshInterceptor(
        serviceProvider.GetRequiredService<ITokenService>()
    ));
    
    return client;
});
```

#### Differences Between Polly Policies and Linger Retry Mechanism

1. **Different Levels of Operation**:
   - Polly policies operate at the underlying HttpClient level, handling retries at the network request level
   - Linger's RetryInterceptor operates at the application layer, with access to the complete response content

2. **Feature Range**:
   - Polly provides a more comprehensive set of resilience policies (retry, circuit breaker, timeout, etc.)
   - Linger interceptors focus more on business logic handling (e.g., token refreshing)

3. **Usage Recommendations**:
   - Use Polly for handling network-level and basic HTTP errors (timeouts, 5xx errors, etc.)
   - Use Linger interceptors for handling business-level errors and application-specific logic
   
Using both in combination can build more robust HTTP client applications.

## Advanced Features

### Custom Interceptors

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

### Performance Monitoring

Built-in performance monitoring helps identify and resolve performance issues:

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

## Best Practices

### Instance Management
- **Recommended**: Use dependency injection container to manage HttpClient lifecycle
- Avoid creating new instances for each request which can lead to port exhaustion
- Use `HttpClientFactory` or dependency injection framework

### Request Optimization
- Set reasonable timeout values to prevent requests from hanging indefinitely
- Use streaming for large responses instead of loading them entirely into memory
- Use compression to reduce network load

### Exception Handling
- Always catch and handle HTTP request exceptions
- Implement backoff strategies for server overload situations
- Use circuit breaker pattern to prevent cascading failures

## Implementation Projects

For detailed usage and examples, refer to the documentation of specific implementation projects:

- [StandardHttpClient Documentation](../Linger.HttpClient.Standard/README.md)
- [FlurlHttpClient Documentation](../Linger.HttpClient.Flurl/README.md)