# Linger.HttpClient.Flurl

## Introduction
Linger.HttpClient.Flurl is built on the popular Flurl.Http library, providing a fluent chaining API and powerful URL building capabilities. As an implementation of the Linger.HttpClient.Contracts interfaces, it combines Flurl's intuitiveness with Linger's standardized interfaces.

> 🔗 This project is part of the [Linger HTTP Client Ecosystem](../Linger.HttpClient.Contracts/README.md).

## Core Advantages

- **Fluent Chaining API**: Expressive code style
- **Dynamic URL Building**: Built-in methods for URL manipulation
- **Template Support**: Template interpolation in URL path segments
- **Powerful Request Customization**: Rich options and extensions
- **Friendly Error Handling**: Detailed and readable error information

## Installation

```bash
dotnet add package Linger.HttpClient.Flurl
```

## Quick Start

```csharp
// Create client
var client = new FlurlHttpClient("https://api.example.com");

// Send request
var response = await client.GetAsync<UserData>("api/users/1");
```

## Flurl-Specific Features

### 1. Fluent URL Building

```csharp
// Get the underlying Flurl client
var flurlClient = client.GetFlurlClient();

// Use fluent API to build URL
var url = flurlClient.BaseUrl
    .AppendPathSegment("api")
    .AppendPathSegment("users")
    .AppendPathSegment(userId)
    .SetQueryParam("include", "profile,orders")
    .SetQueryParam("fields", new[] {"id", "name", "email"})
    .ToString();

// Output: https://api.example.com/api/users/123?include=profile,orders&fields=id&fields=name&fields=email
```

### 2. URL Templates and Interpolation

```csharp
// Use path templates
var productUrl = "products/{id}/variants/{variantId}"
    .SetQueryParam("lang", "en-US");

// Path replacement
var finalUrl = productUrl
    .SetRouteParameter("id", 42)
    .SetRouteParameter("variantId", 101);
    
// Output: products/42/variants/101?lang=en-US
```

### 3. Advanced HTTP Operations

```csharp
// Access Flurl's advanced features
var flurlClient = client.GetFlurlClient();

// Configure specific request
var response = await flurlClient
    .Request("api/special-endpoint")
    .WithHeader("X-API-Version", "2.0")
    .WithTimeout(TimeSpan.FromSeconds(60))
    .WithAutoRedirect(false)
    .AllowHttpStatus(HttpStatusCode.NotFound)
    .PostJsonAsync(new { data = "value" });
```

## Use Cases

FlurlHttpClient is particularly well-suited for:

- **RESTful API clients**: Especially those requiring dynamic URL construction
- **Projects needing expressive code**: Self-documenting API calls
- **Modern web applications**: Flexible handling of various API responses
- **Rapid prototyping**: Fluent API speeds up development

## Comparison with StandardHttpClient

| Scenario | FlurlHttpClient | StandardHttpClient |
|----------|----------------|------------------|
| URL building capability | ★★★★★ | ★★☆☆☆ |
| API fluency | ★★★★★ | ★★★☆☆ |
| Code conciseness | ★★★★★ | ★★★☆☆ |
| Performance requirements | ★★★☆☆ | ★★★★★ |
| Low resource usage | ★★★☆☆ | ★★★★★ |
| Learning curve | Moderate | Gentle |
| Suitable projects | Modern web apps, complex API integrations | Enterprise apps, resource-constrained environments |

## Real-World Examples

### Building Complex Queries

```csharp
// Define query parameters
var filters = new
{
    category = "electronics",
    priceRange = new[] { "100-500", "500-1000" },
    brand = new[] { "apple", "samsung" },
    inStock = true
};

// Use FlurlHttpClient for querying
var response = await client.GetAsync<List<Product>>(
    "api/products", 
    filters
);
```

### JWT Authentication with Flurl Features

```csharp
// Get the Flurl client
var flurlClient = client.GetFlurlClient();

// Configure authentication interceptor
flurlClient.BeforeCall(call => 
{
    if (_tokenService.IsTokenValid())
    {
        call.Request.WithOAuthBearerToken(_tokenService.GetToken());
    }
});

// Handle 401 responses
flurlClient.OnError(async call => 
{
    if (call.HttpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
    {
        if (await _tokenService.RefreshTokenAsync())
        {
            await call.Request
                .WithOAuthBearerToken(_tokenService.GetToken())
                .SendAsync(call.HttpRequestMessage.Method, call.CancellationToken);
        }
    }
});
```

## Best Practices

1. **Separate URL building from HTTP calls**
   ```csharp
   // Build URL first, then send request
   var url = flurlClient.BaseUrl
       .AppendPathSegments("api", "users")
       .SetQueryParams(new { page = 1, size = 10 });
       
   var response = await client.GetAsync<List<User>>(url.ToString());
   ```

2. **Use factory to create named clients**
   ```csharp
   services.AddSingleton<IHttpClientFactory, FlurlHttpClientFactory>();
   ```

3. **Organize API calls by functional area**
   ```csharp
   // User-related APIs
   var usersApi = factory.GetOrCreateClient("users");
   
   // Product-related APIs
   var productsApi = factory.GetOrCreateClient("products");
   ```
