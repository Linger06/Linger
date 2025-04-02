# Linger.HttpClient.Standard

## Introduction

Linger.HttpClient.Standard is an implementation based on the standard .NET HttpClient, providing a lightweight wrapper that conforms to the Linger.HttpClient.Contracts interfaces. This project focuses on delivering a stable, efficient, and .NET-style HTTP communication solution.

## Core Advantages

- **Lightweight Design**: Minimal dependencies, low runtime overhead
- **.NET Integration**: Seamlessly works with HttpClientFactory and DI
- **High Performance**: Optimized for performance in .NET environments
- **Easy Configuration**: Simple setup with familiar .NET patterns

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
client.Options.EnableRetry = true;
client.AddHeader("User-Agent", "Linger.Client");
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
    standardClient.Options.EnableRetry = true;
    standardClient.Options.MaxRetryCount = 3;
    
    return standardClient;
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
```

## Best Practices

### Configuration

```csharp
// Recommended settings for production
client.Options.DefaultTimeout = 15; // 15 seconds timeout
client.Options.EnableRetry = true;
client.Options.MaxRetryCount = 3;
client.Options.RetryInterval = 1000; // 1 second between retries
```

### Error Handling

```csharp
try
{
    var response = await client.CallApi<UserData>("api/users/1");
    
    if (response.IsSuccess)
    {
        // Process data
    }
    else
    {
        // Handle API error
        Console.WriteLine($"API Error: {response.ErrorMsg}");
    }
}
catch (Exception ex)
{
    // Handle network or other exceptions
    Console.WriteLine($"Request failed: {ex.Message}");
}
```

### Resource Management

When using StandardHttpClient directly (not via HttpClientFactory), dispose it properly when finished:

```csharp
using (var httpClient = new System.Net.Http.HttpClient())
{
    var client = new StandardHttpClient(httpClient);
    // Use client...
}
```

When using HttpClientFactory, disposal is handled automatically.
