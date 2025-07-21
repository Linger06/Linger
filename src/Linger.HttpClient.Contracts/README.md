# Linger.HttpClient.Contracts

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [ApiResult and Linger.Results Integration](#apiresult-and-lingerresults-integration)
- [Quick Start](#quick-start)
- [Best Practices](#best-practices)
- [Related Documentation](#related-documentation)

## Overview

**Linger.HttpClient.Contracts** defines standard interfaces and contracts for HTTP client operations, enabling **dependency inversion** and **implementation flexibility**.

### 🎯 Core Benefits

- **Decouple** - Separate business logic from HTTP implementations
- **Switch** - Seamlessly change HTTP client implementations  
- **Test** - Easy unit testing with mock implementations
- **Extend** - Support custom HTTP client implementations

### 🏗️ Architecture Layers

```
Application → IHttpClient Interface → Concrete Implementation
            (Contracts)              (Standard/Custom)
```

## Features

- **Strongly Typed Contracts**: Generic `ApiResult<T>` for type-safe responses
- **Async Support**: Full async/await pattern with `CancellationToken`
- **Error Handling**: Structured `ApiResult` error handling framework
- **Dependency Injection**: Designed for DI containers and HttpClientFactory
- **Extensibility**: Easy to implement custom HTTP clients

## Installation

```bash
# Core contracts
dotnet add package Linger.HttpClient.Contracts

# Production implementation
dotnet add package Linger.HttpClient.Standard
```

## ApiResult and Linger.Results Integration

`ApiResult` is designed to seamlessly integrate with `Linger.Results`, but **also fully compatible with other API designs**:

**When integrated with Linger.Results**:
- **Error Structure Compatibility** - `ApiResult.Errors` matches `Result<T>.Errors` structure
- **Status Code Mapping** - HTTP status automatically corresponds to Result error types  
- **Message Propagation** - Server-side error information fully transmitted to client

**When integrated with other APIs**:
- **Standard HTTP Responses** - Automatically parses HTTP status codes and response bodies
- **Flexible Error Handling** - Supports arbitrary JSON error formats
- **Universal Adaptation** - Works with REST, GraphQL, and various API styles

> 💡 **Detailed Integration Examples**: See [StandardHttpClient Documentation](../Linger.HttpClient.Standard/README.md#lingerresults-integration)

## Quick Start

### 🚀 Basic Usage

```csharp
// 1. Register implementation
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// 2. Inject and use
public class UserService
{
    private readonly IHttpClient _httpClient;
    
    public UserService(IHttpClient httpClient) => _httpClient = httpClient;
    
    public async Task<User?> GetUserAsync(int id)
    {
        var result = await _httpClient.CallApi<User>($"api/users/{id}");
        return result.IsSuccess ? result.Data : null;
    }
}
```

### 🔄 Implementation Switching

```csharp
// Development: Standard implementation
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// Testing: Mock implementation
services.AddSingleton<IHttpClient, MockHttpClient>();

// Production: Resilient implementation
services.AddHttpClient<IHttpClient, ResilientHttpClient>();
```

## Best Practices

### 🏛️ Architecture Principles
1. **Always program against interfaces** - Use `IHttpClient`, never concrete implementations
2. **Register implementations in DI** - Let the container manage lifecycle and dependencies  
3. **Keep business logic implementation-agnostic** - Your services should work with any IHttpClient implementation

### 🧪 Testing Strategy  
4. **Interface mocking** - Use mock implementations for unit tests
5. **Integration testing** - Use real implementations to verify HTTP behavior
6. **Error testing** - Ensure graceful handling of network exceptions

### 📊 Performance Considerations
7. **Resource management** - Properly implement IDisposable
8. **Async patterns** - Use ConfigureAwait(false)
9. **Cancellation support** - Respect CancellationToken

---

## 📖 Related Documentation

- **[StandardHttpClient](../Linger.HttpClient.Standard/README.md)** - Production implementation with detailed usage examples