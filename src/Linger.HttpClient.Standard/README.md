# Linger.HttpClient.Standard

## Introduction
Linger.HttpClient.Standard is an implementation based on the standard .NET HttpClient, providing a lightweight wrapper that conforms to the Linger.HttpClient.Contracts interfaces. This project focuses on delivering a stable, efficient, and .NET-style HTTP communication solution.

> 🔗 This project is part of the [Linger HTTP Client Ecosystem](../Linger.HttpClient.Contracts/README.md).

## Core Advantages

- **Lightweight Design**: Minimal dependencies, low runtime overhead
- **.NET Conventions**: Naturally integrates with .NET projects, follows platform design principles
- **High Performance**: Optimized for performance, suitable for high-concurrency scenarios
- **Easy Troubleshooting**: Transparent implementation, clear error information
- **Low Memory Footprint**: Optimized memory management, suitable for resource-constrained environments

## Installation

```bash
dotnet add package Linger.HttpClient.Standard
```

## Quick Start

```csharp
// Create client
var client = new StandardHttpClient("https://api.example.com");

// Send request
var response = await client.GetAsync<UserData>("api/users/1");
```

## Advanced Features

### 1. Custom HttpMessageHandler

Full control over the underlying HttpClient behavior:

```csharp
// Custom handler
var handler = new HttpClientHandler
{
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
    UseCookies = false,
    MaxConnectionsPerServer = 20
};

// Create client with custom handler
var client = new StandardHttpClient(new System.Net.Http.HttpClient(handler));
```

### 2. Integrated HTTP Compression

Use the built-in compression helper class to reduce bandwidth consumption:

```csharp
// Create compression-enabled handler
var handler = CompressionHelper.CreateCompressionHandler();

// Create client
var client = new StandardHttpClient(new System.Net.Http.HttpClient(handler));
```

### 3. Efficient Parallel Requests

```csharp
// Send multiple requests in parallel
var task1 = client.GetAsync<Data1>("api/endpoint1");
var task2 = client.GetAsync<Data2>("api/endpoint2");
var task3 = client.GetAsync<Data3>("api/endpoint3");

// Wait for all requests to complete
await Task.WhenAll(task1, task2, task3);

// Process all results
var result1 = task1.Result.Data;
var result2 = task2.Result.Data;
var result3 = task3.Result.Data;
```

## Use Cases

StandardHttpClient is particularly well-suited for:

- **Performance and resource-sensitive applications**: Mobile apps, applications running on low-spec devices
- **Projects requiring fine-grained HTTP communication control**: Security-sensitive enterprise systems
- **Projects migrating from existing .NET HttpClient**: Smooth transition, low learning curve
- **Applications requiring .NET-specific features**: WinForms, WPF, or systems that need integration with .NET-specific APIs

## Comparison with FlurlHttpClient

| Scenario | StandardHttpClient | FlurlHttpClient |
|----------|-------------------|-----------------|
| Performance requirements | ★★★★★ | ★★★☆☆ |
| Low resource usage | ★★★★★ | ★★★☆☆ |
| URL building capability | ★★☆☆☆ | ★★★★★ |
| API fluency | ★★★☆☆ | ★★★★★ |
| Learning curve | Gentle | Moderate |
| Suitable projects | Enterprise applications, resource-constrained environments | Modern web applications, complex API integrations |

## Best Practices

1. **Use HttpClientFactory to manage instances**
   ```csharp
   services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();
   ```

2. **Create named clients grouped by API**
   ```csharp
   factory.RegisterClient("users-api", "https://users.example.com");
   factory.RegisterClient("products-api", "https://products.example.com");
   ```

3. **Performance-oriented configuration**
   ```csharp
   client.Options.DefaultTimeout = 15; // Shorter timeout
   ```

4. **Use with CancellationToken**
   ```csharp
   using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
   await client.GetAsync<Data>("api/data", cancellationToken: cts.Token);
   ```
