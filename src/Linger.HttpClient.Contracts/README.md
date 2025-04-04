# Linger.HttpClient.Contracts

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Dependency Injection Integration](#dependency-injection-integration)
  - [Using HttpClientFactory](#using-httpclientfactory)
  - [In Service Classes](#in-service-classes)
- [Basic Usage](#basic-usage)
  - [GET Requests](#get-requests)
  - [POST Requests](#post-requests)
  - [File Upload](#file-upload)
- [Error Handling](#error-handling)
- [Automatic Token Refresh](#automatic-token-refresh)
- [Best Practices](#best-practices)

## Overview

Linger.HttpClient.Contracts defines standard interfaces and contracts for HTTP client operations, serving as the foundation for Linger HTTP client implementations. By using unified contracts, you can easily switch between different HTTP client implementations without modifying your business code.

### Key Components

- **Linger.HttpClient.Contracts**: Core interfaces and contracts (this project)
- **Linger.HttpClient.Standard**: Implementation based on .NET standard HttpClient

## Features

- Strongly typed HTTP client interfaces
- Support for various HTTP methods (GET, POST, PUT, DELETE)
- File upload capabilities
- Request/response handling
- User-friendly error handling
- Timeout management

## Installation

```bash
# Install interfaces and contracts
dotnet add package Linger.HttpClient.Contracts

# Install implementation based on standard HttpClient
dotnet add package Linger.HttpClient.Standard

# For resilience features (automatic retries, circuit breaker, etc.)
dotnet add package Microsoft.Extensions.Http.Resilience
```

## Dependency Injection Integration

### Using HttpClientFactory

The recommended way to use Linger HTTP clients is with Microsoft's HttpClientFactory and HTTP Resilience:

```csharp
// In your startup configuration
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddResilienceHandler("Default", builder =>
{
    // Configure retry behavior
    builder.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2),
        ShouldHandle = args =>
        {
            // Retry on server errors and rate limiting
            return ValueTask.FromResult(args.Outcome.Result?.StatusCode is
                HttpStatusCode.RequestTimeout or      // 408
                HttpStatusCode.TooManyRequests or     // 429
                HttpStatusCode.BadGateway or          // 502
                HttpStatusCode.ServiceUnavailable or  // 503
                HttpStatusCode.GatewayTimeout);       // 504
        }
    });
});
```

### In Service Classes

Once registered, you can inject and use IHttpClient in your services:

```csharp
public class UserService
{
    private readonly IHttpClient _httpClient;

    public UserService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResult<UserInfo>> GetUserInfoAsync(string userId)
    {
        return await _httpClient.CallApi<UserInfo>($"api/users/{userId}");
    }

    public async Task<ApiResult<UserInfo>> CreateUserAsync(UserCreateModel model)
    {
        return await _httpClient.CallApi<UserInfo>("api/users", HttpMethodEnum.Post, model);
    }
}
```

## Basic Usage

### GET Requests

```csharp
// Simple GET request
var result = await _httpClient.CallApi<UserData>("api/users/1");

// With query parameters
var users = await _httpClient.CallApi<List<UserData>>("api/users", 
    new { page = 1, pageSize = 10 });
```

### POST Requests

```csharp
// POST with JSON body
var newUser = await _httpClient.CallApi<UserData>("api/users", 
    HttpMethodEnum.Post, 
    new { Name = "John", Email = "john@example.com" });
```

### File Upload

```csharp
// File upload with form data
byte[] fileData = File.ReadAllBytes("document.pdf");
var formData = new Dictionary<string, string>
{
    { "description", "Sample document" }
};

var uploadResult = await _httpClient.CallApi<UploadResponse>(
    "api/files/upload", 
    HttpMethodEnum.Post, 
    formData, 
    fileData, 
    "document.pdf"
);
```

## Error Handling

```csharp
var result = await _httpClient.CallApi<UserData>($"api/users/{userId}");

if (result.IsSuccess)
{
    // Process successful response
    var user = result.Data;
    Console.WriteLine($"User: {user.Name}");
}
else
{
    // Handle error
    Console.WriteLine($"Error: {result.ErrorMsg}");
    
    // Check for specific status codes
    if (result.StatusCode == HttpStatusCode.Unauthorized)
    {
        // Handle authentication error
    }
}
```

## Automatic Token Refresh

You can implement automatic token refresh using Microsoft.Extensions.Http.Resilience:

```csharp
// Create a token refresh handler
public class TokenRefreshHandler
{
    private readonly AppState _appState;
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public TokenRefreshHandler(AppState appState, IServiceProvider serviceProvider)
    {
        _appState = appState;
        _serviceProvider = serviceProvider;
    }

    public void ConfigureTokenRefreshResiliencePipeline(ResiliencePipelineBuilder<HttpResponseMessage> builder)
    {
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 1, // Only try refreshing once
            ShouldHandle = args => 
            {
                bool shouldRetry = args.Outcome.Result?.StatusCode == HttpStatusCode.Unauthorized;
                return ValueTask.FromResult(shouldRetry);
            },
            OnRetry = async context =>
            {
                // Thread-safe token refresh
                await _semaphore.WaitAsync();
                try
                {
                    await RefreshTokenAsync();
                }
                finally
                {
                    _semaphore.Release();
                }
            },
            BackoffType = DelayBackoffType.Constant,
            Delay = TimeSpan.Zero // Retry immediately after token refresh
        });
    }

    private async Task RefreshTokenAsync()
    {
        // Implementation of token refresh logic
        // ...
    }
}

// Register and use in your application
services.AddSingleton<TokenRefreshHandler>();
services.AddHttpClient<IHttpClient, StandardHttpClient>(/* ... */)
    .AddResilienceHandler("TokenRefresh", (builder, context) =>
    {
        var tokenRefreshHandler = context.ServiceProvider.GetRequiredService<TokenRefreshHandler>();
        tokenRefreshHandler.ConfigureTokenRefreshResiliencePipeline(builder);
    });
```

## Best Practices

1. **Use HttpClientFactory for lifecycle management**
2. **Set reasonable timeout values**
3. **Implement proper error handling**
4. **Use strongly typed models for API responses**
5. **Enable automatic token refresh for better user experience**
6. **Use Microsoft.Extensions.Http.Resilience for retry logic**

For more information, see the [Linger.HttpClient.Standard documentation](../Linger.HttpClient.Standard/README.md).