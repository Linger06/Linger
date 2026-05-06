# Linger.HttpClient.Contracts

Standard interfaces and contracts for HTTP client operations.

## Features

- **Interface Decoupling**: Separate business logic from HTTP implementations
- **Implementation Flexibility**: Support multiple HTTP client implementations
- **Testing Friendly**: Easy unit testing and mocking
- **Strongly Typed**: Generic `ApiResult<T>` for type safety
- **Async Support**: Full async/await pattern

## Installation

```bash
# Core contracts
dotnet add package Linger.HttpClient.Contracts

# Production implementation
dotnet add package Linger.HttpClient.Standard
```

## Core Interfaces

### IHttpClient
```csharp
public interface IHttpClient
{
    Task<ApiResult<T>> CallApi<T>(
        string url,
        object? queryParams = null,
        int? timeout = null,
        CancellationToken cancellationToken = default);

    Task<ApiResult<T>> CallApi<T>(
        string url,
        HttpMethodEnum method,
        object? requestBody = null,
        object? queryParams = null,
        int? timeout = null,
        CancellationToken cancellationToken = default);

    Task<ApiResult<Stream>> DownloadStreamAsync(
        string url,
        int? timeout = null,
        CancellationToken cancellationToken = default);

    Task<ApiResult> DownloadToFileAsync(
        string url,
        string destinationPath,
        int? timeout = null,
        int bufferSize = 8192,
        IProgress<(long downloaded, long? total)>? progress = null,
        CancellationToken cancellationToken = default);
}
```

### ApiResult<T>
```csharp
public class ApiResult<T>
{
    public bool IsSuccess { get; }
    public T Data { get; set; }
    public string? ErrorMsg { get; set; }
    public HttpStatusCode? StatusCode { get; set; }
    public IEnumerable<Error> Errors { get; set; }
}
```

## Basic Usage

```csharp
// Register in DI
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// Use in service
public class UserService
{
    private readonly IHttpClient _httpClient;

    public UserService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<User?> GetUserAsync(int id)
    {
        var result = await _httpClient.CallApi<User>($"api/users/{id}");
        return result.IsSuccess ? result.Data : null;
    }
}
```

## Linger.Results Integration

`ApiResult` seamlessly integrates with `Linger.Results`:

```csharp
// Server using Linger.Results (generic Result: returns data)
public async Task<Result<User>> GetUserAsync(int id)
{
    var user = await _userRepository.GetUserAsync(id);
    return user is not null ? Result<User>.Success(user) : Result<User>.NotFound("User not found");
}

// Server using Linger.Results (non-generic Result: only indicates success/failure)
public async Task<Result> UpdateUserAsync(UpdateUserRequest request)
{
    await _userRepository.UpdateUserAsync(request);
    return Result.Success();
}

// Client receives structured errors
var apiResult = await _httpClient.CallApi<User>($"api/users/{id}");
if (!apiResult.IsSuccess)
{
    // Automatically mapped error information
    foreach (var error in apiResult.Errors)
        Console.WriteLine($"Error: {error.Code} - {error.Message}");
}

// For non-generic Result, the response usually has no Data; check IsSuccess / StatusCode / Errors
ApiResult commandResult = await _httpClient.DownloadToFileAsync("api/files/export", "export.zip");
if (commandResult.IsSuccess)
{
    Console.WriteLine("Operation succeeded");
}
else
{
    Console.WriteLine($"HTTP Status: {commandResult.StatusCode}");
    Console.WriteLine($"Error Message: {commandResult.ErrorMsg}");
}
```

## Error Handling

```csharp
var result = await _httpClient.CallApi<User>("api/users/123");

if (result.IsSuccess)
{
    var user = result.Data;
    // Handle success
}
else
{
    // Handle error
    Console.WriteLine($"HTTP Status: {result.StatusCode}");
    Console.WriteLine($"Error Message: {result.ErrorMsg}");
    
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Detailed Error: {error.Code} - {error.Message}");
    }
}
```

## JSON Serialization Configuration

`HttpClientBase` provides default JSON serialization configuration with a "secure-by-default" approach:

### Response Deserialization Configuration

`HttpClientBase.DefaultResponseOptions` is used for deserializing HTTP responses:

- **Encoder**: `JavaScriptEncoder.Default` (safer escaping strategy)
- **Number handling**: Lenient (allows reading numbers from strings, `AllowReadingFromString`)
- **Other settings**: Case-insensitive properties, CamelCase naming, ignore nulls, disallow trailing commas and comments, ignore cycles
- **Built-in converters**: `JsonObjectConverter`, `DateTimeConverter`, `DateTimeNullConverter`, `DataTableJsonConverter`

### Request Serialization Configuration

`HttpClientBase.DefaultRequestOptions` is used for serializing HTTP requests:

- **Encoder**: `JavaScriptEncoder.Default`
- **Based on standard Web defaults**
- **Converters**: Only includes `DateTimeConverter`

### Unified JSON Configuration Management

It's recommended to use `Linger.Json.JsonDefaults` for unified JSON configuration:

```csharp
using Linger.Json;

// Use factory methods to get pre-configured options
var responseOptions = JsonDefaults.CreateResponseOptions();  // HTTP responses
var requestOptions = JsonDefaults.CreateRequestOptions();    // HTTP requests

// Apply configuration in WebAPI
builder.Services.AddControllers()
    .AddJsonOptions(options => 
        JsonDefaults.ApplyDefaultConfiguration(options.JsonSerializerOptions));
```

For detailed configuration documentation, see `Linger/Json/JsonDefaults.README.md`

### Custom Configuration

Prefer overriding `GetRequestJsonOptions()` / `GetResponseJsonOptions()` to provide custom JSON options rather than replacing the entire serialization implementation. Example:

```csharp
using Linger.Json;
using Linger.Json.JsonConverter;

public class CustomHttpClient : HttpClientBase
{
    protected override JsonSerializerOptions GetRequestJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };
        options.Converters.Add(new DateTimeConverter());
        return options;
    }

    protected override JsonSerializerOptions GetResponseJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new DateTimeConverter());
        options.Converters.Add(new JsonObjectConverter());
        return options;
    }
}
```

If you need full control over serialization you can still override `CreateHttpContent`, but prefer the two methods above to keep behavior consistent.

## Best Practices

- Use dependency injection to manage HTTP client lifecycle
- Leverage `ApiResult`'s structured error handling
- Inherit from existing implementations when implementing custom error handling
- Use `CancellationToken` to support request cancellation
- Use mock implementations in unit tests