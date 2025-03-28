# Linger.HttpClient.Flurl

### Introduction
Linger.HttpClient.Flurl is an implementation of the Linger.HttpClient.Contracts interfaces using the popular Flurl.Http library. It combines the power of Flurl with the standardized interfaces of Linger.

### Features
- All features of Linger.HttpClient.Contracts
- Leverages Flurl.Http for simplified URL building and manipulation
- Fluent API for modern C# development
- Enhanced error handling
- Automatic handling of query parameters

### Supported .NET versions
This library supports .NET applications that utilize .NET Framework 4.6.2+ or .NET Standard 2.0+.

### Usage Example

```csharp
// Create a client
var client = new FlurlHttpClient("https://api.example.com");

// Add an authorization token
client.SetToken("your-auth-token");

// Configure options
client.Options.EnableRetry = true;
client.Options.MaxRetryCount = 3;

// Make a GET request
var response = await client.GetAsync<YourResponseType>("api/resources");

// Make a POST request with query parameters
var postResponse = await client.PostAsync<YourResponseType>(
    "api/resources", 
    new { Name = "New Resource" }, 
    new { category = "important" }
);

// Upload a file
var fileBytes = File.ReadAllBytes("example.pdf");
var uploadResponse = await client.CallApi<UploadResult>(
    "api/upload",
    HttpMethodEnum.Post,
    new Dictionary<string, string> { { "description", "Example file" } },
    fileBytes,
    "example.pdf"
);
```
