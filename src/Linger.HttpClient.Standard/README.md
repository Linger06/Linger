# Linger.HttpClient.Standard

## Introduction

Linger.HttpClient.Standard is an implementation based on the standard .NET HttpClient, providing a lightweight wrapper that conforms to the Linger.HttpClient.Contracts interfaces. This project focuses on delivering a stable, efficient, and .NET-style HTTP communication solution.

## Core Advantages

- **Lightweight Design**: Minimal dependencies, low runtime overhead
- **.NET Integration**: Seamlessly works with HttpClientFactory and DI
- **High Performance**: Optimized for performance in .NET environments
- **Easy Configuration**: Simple setup with familiar .NET patterns
- **Built-in Logging**: Comprehensive logging support using Microsoft.Extensions.Logging
- **Structured Logging**: Performance metrics, request/response tracking, and error monitoring

## Installation

```bash
dotnet add package Linger.HttpClient.Standard
```

## Quick Start

### Basic Creation

```csharp
// Create client directly
var client = new StandardHttpClient("https://api.example.com");

// Configure options
client.Options.DefaultTimeout = 30;
client.AddHeader("User-Agent", "Linger.Client");

// Create with logging support
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<StandardHttpClient>();
var clientWithLogging = new StandardHttpClient("https://api.example.com", logger);
```

### With HttpClientFactory

```csharp
// In your startup configuration
services.AddHttpClient<StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddTypedClient<IHttpClient>((httpClient, serviceProvider) => 
{
    var standardClient = new StandardHttpClient(httpClient);
    
    // Configure options
    standardClient.Options.DefaultTimeout = 30;
    standardClient.AddHeader("User-Agent", "MyApp/1.0");
    
    return standardClient;
});
```

### With Logging and HttpClientFactory

```csharp
// In your startup configuration
services.AddLogging(builder => builder.AddConsole());
services.AddHttpClient<StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddTypedClient<IHttpClient>((httpClient, serviceProvider) => 
{
    var logger = serviceProvider.GetService<ILogger<StandardHttpClient>>();
    var standardClient = new StandardHttpClient(httpClient, logger);
    
    // Configure options
    standardClient.Options.DefaultTimeout = 30;
    standardClient.AddHeader("User-Agent", "MyApp/1.0");
    
    return standardClient;
});
```

## Logging Features

### Log Levels

The `StandardHttpClient` provides comprehensive logging at different levels:

- **Debug**: Request start/end, timing information, configuration details
- **Trace**: Detailed request headers, query parameters  
- **Information**: Successful operations
- **Warning**: Failed API calls, empty URLs
- **Error**: Exceptions, timeouts, protocol violations

### Example Log Output

```
[Debug] StandardHttpClient initialized with base URL: https://api.example.com
[Debug] Starting API call: Post /api/users with timeout: 30s
[Trace] Request headers: Authorization: Bearer xxx, User-Agent: MyApp/1.0
[Debug] Query parameters appended to URL: culture=en-US
[Debug] API call completed in 245ms with status: Created
[Debug] API call successful for Post /api/users
```

### Configuring Logging

```csharp
// Development - verbose logging
services.AddLogging(builder =>
{
    builder.AddConsole()
           .SetMinimumLevel(LogLevel.Debug);
});

// Production - essential logging only
services.AddLogging(builder =>
{
    builder.AddFile("logs/httpclient-{Date}.txt")
           .SetMinimumLevel(LogLevel.Information);
});
```

## Usage Examples

### Simple GET Request

```csharp
// Send GET request
var response = await client.CallApi<UserData>("api/users/1");

// Process response
if (response.IsSuccess)
{
    Console.WriteLine($"User: {response.Data.Name}");
}
```

### POST Request with JSON

```csharp
// Create user data
var userData = new UserCreateModel { Name = "John", Email = "john@example.com" };

// Send POST request
var response = await client.CallApi<UserData>(
    "api/users",
    HttpMethodEnum.Post,
    userData
);

if (response.IsSuccess)
{
    Console.WriteLine($"User created successfully: {response.Data.Id}");
}
```

### File Upload

```csharp
// Read file
byte[] fileData = File.ReadAllBytes("document.pdf");

// Create form data
var formData = new Dictionary<string, string>
{
    { "description", "Sample document" }
};

// Upload file
var response = await client.CallApi<FileResponse>(
    "api/files",
    HttpMethodEnum.Post,
    formData,
    fileData,
    "document.pdf"
);

if (response.IsSuccess)
{
    Console.WriteLine($"File uploaded successfully: {response.Data.FileId}");
}
```

### Request with Query Parameters

```csharp
// Query parameters
var queryParams = new { page = 1, size = 10, keyword = "test" };

// Send request
var response = await client.CallApi<PagedResult<UserData>>(
    "api/users",
    queryParams
);

if (response.IsSuccess)
{
    Console.WriteLine($"Retrieved {response.Data.Items.Count} users");
}
```

## Best Practices

### Configuration

```csharp
// Recommended settings for production
client.Options.DefaultTimeout = 15; // 15 seconds timeout
client.AddHeader("User-Agent", "MyApp/1.0");
client.AddHeader("Accept", "application/json");

// Or set via Options
client.Options.DefaultHeaders["Authorization"] = "Bearer your-token";
client.Options.DefaultHeaders["Custom-Header"] = "custom-value";
```

### Error Handling with Logging

```csharp
var logger = serviceProvider.GetService<ILogger<StandardHttpClient>>();
var client = new StandardHttpClient("https://api.example.com", logger);

try
{
    var response = await client.CallApi<UserData>("api/users/1");
    
    if (response.IsSuccess)
    {
        // Process data
    }
    else
    {
        // Handle API error - automatically logged
        Console.WriteLine($"API Error: {response.ErrorMsg}");
    }
}
catch (Exception ex)
{
    // Handle network or other exceptions - automatically logged
    Console.WriteLine($"Request failed: {ex.Message}");
}
```

### Resource Management

**Using HttpClientFactory (Recommended)**:
```csharp
// Register in startup configuration
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// Use in service - lifecycle managed automatically
public class UserService
{
    private readonly IHttpClient _httpClient;
    
    public UserService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<UserData> GetUserAsync(int id)
    {
        return await _httpClient.CallApi<UserData>($"users/{id}");
    }
}
```

**Direct Creation Resource Management**:
```csharp
// Method 1: Using using statement
using var httpClient = new System.Net.Http.HttpClient();
var client = new StandardHttpClient(httpClient, logger);
// Use client...
// using statement ensures proper disposal

// Method 2: Manual management
var httpClient = new System.Net.Http.HttpClient();
try
{
    var client = new StandardHttpClient(httpClient, logger);
    // Use client...
}
finally
{
    httpClient?.Dispose();
}
```

## Performance Considerations

1. **Log Level Optimization**: Use `Information` level in production to avoid performance impact from verbose logging
2. **HttpClientFactory Usage**: Recommended to use HttpClientFactory to avoid port exhaustion issues
3. **Timeout Settings**: Set appropriate timeouts based on API response times to avoid long waits
4. **Concurrency Control**: HttpClient is thread-safe and can be safely used in multi-threaded environments

## Important Notes

⚠️ **Important Reminders**:
- Don't create new HttpClient instances for each request; reuse them
- Be careful with sensitive information (like Authorization headers) when using logging
- Set timeout values based on actual API response times
- Use HttpClientFactory in high-concurrency scenarios
