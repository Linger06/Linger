# Linger.HttpClient.Standard

The standard `IHttpClient` implementation based on `System.Net.Http.HttpClient`.

## Features

- `HttpClientFactory` connection management
- JSON, form, and custom `HttpContent` requests
- RFC 7807 ProblemDetails and legacy error-array parsing
- Per-request headers without shared mutable authentication state
- Raw `HttpResponseMessage` access and long-lived streaming
- Streaming uploads based on `StreamContent`
- Temporary-file commit semantics for streaming downloads
- User-cancellation propagation and `HttpClient.Timeout` handling

## Installation and registration

```bash
dotnet add package Linger.HttpClient.Standard
```

```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});
```

WinForms and console applications can construct the client directly from a base URL. In this mode, `StandardHttpClient` owns the underlying client; reuse the instance for the application lifetime and dispose it during shutdown:

```csharp
using var client = new StandardHttpClient("https://api.example.com/");
```

Configure the underlying client during construction when you need a timeout, fixed headers, or `Accept-Language`:

```csharp
using var client = new StandardHttpClient(
    "https://api.example.com/",
    configureClient: httpClient =>
    {
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN");
    });
```

Supply a caller-owned `HttpClient` when custom handlers, authentication, certificates, or proxies are required. Disposing `StandardHttpClient` does not dispose the external client. You can also let URL mode own a custom handler:

```csharp
using var client = new StandardHttpClient(
    "https://api.example.com/",
    accessTokenHandler);
```

```csharp
using var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.example.com/")
};
using var client = new StandardHttpClient(httpClient, logger);
```

## Typed calls

```csharp
var getResult = await client.GetAsync<User>(
    "users/42",
    queryParams: new { IncludeRoles = true },
    cancellationToken: cancellationToken);

var postResult = await client.PostAsync<User>(
    "users",
    new CreateUserRequest("Ada"),
    cancellationToken: cancellationToken);

var patchResult = await client.CallApi<User>(
    "users/42",
    HttpMethod.Patch,
    new UpdateUserRequest("Grace"),
    cancellationToken: cancellationToken);
```

Query-object property metadata is cached. Collection properties produce repeated keys, while numbers and dates use invariant formatting.

## Authentication and headers

### Fixed authorization header

When one client instance represents one user and every request uses the same token, configure the default authorization header when creating the client:

```csharp
var accessToken = "eyJ...";
using var client = new StandardHttpClient(
    "https://api.example.com/",
    configureClient: httpClient =>
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    });
```

Reuse the client for the application lifetime instead of creating one per request. Use a `DelegatingHandler` for token refresh or other dynamic behavior rather than mutating shared `DefaultRequestHeaders` while requests are in flight.

### Using DelegatingHandler

A `DelegatingHandler` can attach authentication, culture, or other common request information before sending. The handler modifies only the current `HttpRequestMessage`:

```csharp
public sealed class AccessTokenHandler(string accessToken) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
        return base.SendAsync(request, cancellationToken);
    }
}
```

Automatic refresh normally retains the complete server `Token`, uses `SemaphoreSlim` to prevent duplicate refreshes under concurrency, and calls the refresh endpoint through a separate client without the authentication handler. See the [Linger.HttpClient.WinForms example](../../examples/Linger.HttpClient.WinForms/README.md) for the complete client implementation and lifecycle; see the [Linger.AspNetCore.Jwt README](../Linger.AspNetCore.Jwt/README.md) for the server endpoints.

### Dynamic or multi-user authentication

When a shared client can send requests for different users, pass dynamic authentication per request instead of mutating shared `DefaultRequestHeaders.Authorization`:

```csharp
var headers = new Dictionary<string, string>
{
    ["Authorization"] = $"Bearer {accessToken}",
    ["X-Correlation-Id"] = correlationId
};

var result = await client.GetAsync<User>(
    "users/me",
    headers: headers,
    cancellationToken: cancellationToken);
```

Fixed service credentials can also be configured during `AddHttpClient` registration. Use a default authorization header for a fixed token on a dedicated instance, and use the `headers` parameter when tokens can differ between requests.

The client does not append `culture`. Add it explicitly as a query parameter or through a custom `DelegatingHandler` when required.

## File uploads

```csharp
var fileStream = File.OpenRead("report.pdf");
var result = await client.UploadFileAsync<UploadResponse>(
    "files",
    HttpMethod.Post,
    fileStream,
    "report.pdf",
    formData: new Dictionary<string, string>
    {
        ["category"] = "report"
    },
    cancellationToken: cancellationToken);
```

Uploads use `StreamContent` and do not copy the complete file into memory. The input stream is disposed when the request completes.

## File downloads

```csharp
var progress = new Progress<(long downloaded, long? total)>(value =>
{
    Console.WriteLine($"{value.downloaded}/{value.total}");
});

var result = await client.DownloadToFileAsync(
    "files/report.pdf",
    "report.pdf",
    progress: progress,
    cancellationToken: cancellationToken);
```

The download flow uses `ResponseHeadersRead`, writes a same-directory temporary file, and replaces the destination only after download and flush succeed. Cancellation or failure removes the temporary file and preserves an existing destination.

## Raw responses and long-lived streams

Use `SendAsync` when you need response headers, SSE, or incremental content processing. It reuses the `StandardHttpClient` base address, default headers, and `DelegatingHandler` pipeline, including authentication refresh:

```csharp
using var response = await client.SendAsync(
    "events",
    HttpMethod.Get,
    cancellationToken: cancellationToken);

response.EnsureSuccessStatusCode();
using var stream = await response.Content.ReadAsStreamAsync();
await ProcessStreamAsync(stream, cancellationToken);
```

The caller owns the returned `HttpResponseMessage`. Raw calls do not parse unsuccessful responses or convert network and timeout exceptions into `ApiResult`.

## Error handling

```csharp
var result = await client.GetAsync<User>("users/42", cancellationToken: cancellationToken);

if (!result.IsSuccess)
{
    Console.WriteLine($"HTTP: {result.StatusCode}");
    Console.WriteLine(result.ErrorMsg);

    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.Code}: {error.Message}");
    }
}
```

Parsing order:

1. ProblemDetails (`application/problem+json` or standard problem fields)
2. Legacy `IEnumerable<Error>` arrays
3. Status-code message and raw response text

An arbitrary JSON object is not treated as ProblemDetails merely because it can be deserialized. Full exception details go to logs; `ErrorMsg` never contains `Exception.ToString()`.

## Cancellation and timeout

User cancellation preserves standard .NET semantics and throws `OperationCanceledException`. Timeout is configured through `HttpClient.Timeout` and returns a failed `ApiResult`. Use `GetWithTimeoutAsync` for a different one-off GET timeout; for other requests, pass a token from a caller-owned `CancellationTokenSource` configured with `CancelAfter`.

## Custom JSON and errors

Derive from `StandardHttpClient` and override `GetRequestJsonOptions`, `GetResponseJsonOptions`, or `GetErrorMessageAsync` when a server requires a custom format.
