# Linger.HttpClient

## English

### Introduction
Linger.HttpClient is a lightweight wrapper around the standard .NET HttpClient that provides additional features and a more developer-friendly API.

### Features
- Simple, fluent API for HTTP operations
- Automatic retry support
- Request/response interception
- Easy authentication handling
- Customizable timeout settings
- Culture-aware requests

### Supported .NET versions
This library supports .NET applications that utilize .NET Framework 4.6.2+ or .NET Standard 2.0+.

### Usage Example

```csharp
// Create a client
var client = new BaseHttpClient("https://api.example.com");

// Add an authorization token
client.SetToken("your-auth-token");

// Configure options
client.Options.EnableRetry = true;
client.Options.MaxRetryCount = 3;

// Add headers
client.AddHeader("User-Agent", "Linger HttpClient");

// Make a GET request
var response = await client.GetAsync<YourResponseType>("api/resources");

// Make a POST request
var postResponse = await client.PostAsync<YourResponseType>(
    "api/resources", 
    new { Name = "New Resource", Description = "Some description" }
);

// Work with the response
if (response.StatusCode == System.Net.HttpStatusCode.OK)
{
    var data = response.Data;
    // Process your data
}
```
