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
- Request/response interception
- Automatic retry mechanism
- User-friendly error handling

## Installation

```bash
# Install interfaces and contracts
dotnet add package Linger.HttpClient.Contracts

# Install implementation based on standard HttpClient
dotnet add package Linger.HttpClient.Standard
```

## Dependency Injection Integration

### Using HttpClientFactory

The recommended way to use Linger HTTP clients is with Microsoft's HttpClientFactory:

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
    standardClient.Options.EnableRetry = true;
    standardClient.Options.MaxRetryCount = 3;
    
    // Access other services
    var appState = serviceProvider.GetRequiredService<AppState>();
    if (!string.IsNullOrEmpty(appState.Token))
    {
        standardClient.SetToken(appState.Token);
    }
    
    return standardClient;
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

## Best Practices

1. **Use HttpClientFactory for lifecycle management**
2. **Set reasonable timeout values**
3. **Implement proper error handling**
4. **Use strongly typed models for API responses**

For more information, see the [Linger.HttpClient.Standard documentation](../Linger.HttpClient.Standard/README.md).