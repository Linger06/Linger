# Linger.HttpClient.Contracts

Interfaces and transport models for `Linger.HttpClient`. This package contains no HTTP request implementation.

## Included APIs

- `IHttpClient`: typed calls, raw responses, streaming file uploads, and file downloads
- `ApiResult` / `ApiResult<T>`: transport results with HTTP status and structured errors
- `ProblemDetailsWithErrors`: RFC 7807 field-error model
- `HttpClientExtensions`: GET, POST, PUT, and DELETE convenience methods

The implementation is provided by `Linger.HttpClient.Standard`.

## Installation

```bash
dotnet add package Linger.HttpClient.Contracts
dotnet add package Linger.HttpClient.Standard
```

## Core interface

```csharp
public interface IHttpClient
{
    Task<HttpResponseMessage> SendAsync(
        string url,
        HttpMethod method,
        object? requestBody = null,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead,
        CancellationToken cancellationToken = default);

    Task<ApiResult<T>> CallApi<T>(
        string url,
        HttpMethod method,
        object? requestBody = null,
        object? queryParams = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    Task<ApiResult<T>> UploadFileAsync<T>(
        string url,
        HttpMethod method,
        Stream fileStream,
        string fileName,
        IReadOnlyDictionary<string, string>? formData = null,
        string fileFieldName = "file",
        string? contentType = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    Task<ApiResult> DownloadToFileAsync(
        string url,
        string destinationPath,
        int bufferSize = 8192,
        IProgress<(long downloaded, long? total)>? progress = null,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);
}
```

## Basic usage

```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var result = await httpClient.GetAsync<User>(
    "users/42",
    headers: new Dictionary<string, string>
    {
        ["Authorization"] = $"Bearer {accessToken}"
    },
    cancellationToken: cancellationToken);
```

Use `GetWithTimeoutAsync` when one GET request needs a timeout different from the client default:

```csharp
var result = await httpClient.GetWithTimeoutAsync<User>(
    "users/42",
    timeout: TimeSpan.FromSeconds(5),
    cancellationToken: cancellationToken);
```

A one-off timeout returns a failed `ApiResult<T>`; caller cancellation through `cancellationToken` still throws `OperationCanceledException`. Passing `null` or `Timeout.InfiniteTimeSpan` adds no per-request timeout.

Dynamic headers apply only to one request. The client stores no mutable shared token/header state and does not append a `culture` query parameter. Configure fixed headers through `AddHttpClient`; a caller-owned dedicated `HttpClient`, such as in WinForms, can instead set `DefaultRequestHeaders.Authorization` when the instance is created. Continue using per-request `headers` when one client can represent different users. The contracts package does not store or refresh tokens; see the [Linger.HttpClient.Standard README](../Linger.HttpClient.Standard/README.md) for complete `DelegatingHandler` integration with `Linger.AspNetCore.Jwt` refresh tokens.

## Request bodies

`CallApi<T>` creates content according to `requestBody`:

- `HttpContent`: sent directly and disposed when the request completes
- `IDictionary<string, string>`: sent as `application/x-www-form-urlencoded`
- Other objects: serialized as JSON

```csharp
var result = await httpClient.CallApi<User>(
    "users",
    HttpMethod.Post,
    new CreateUserRequest("Ada"),
    cancellationToken: cancellationToken);
```

## Raw responses and streaming

Use `SendAsync` when you need response headers, SSE, or incremental content processing. The request still uses the same base address, default headers, and `DelegatingHandler` pipeline:

```csharp
using var response = await httpClient.SendAsync(
    "events",
    HttpMethod.Get,
    cancellationToken: cancellationToken);

response.EnsureSuccessStatusCode();
using var stream = await response.Content.ReadAsStreamAsync();
await ProcessStreamAsync(stream, cancellationToken);
```

The caller must dispose the returned `HttpResponseMessage`. `SendAsync` does not parse unsuccessful responses or convert transport exceptions into `ApiResult`; use `CallApi<T>` when the unified error model is required.

## File transfers

Uploads use `StreamContent` and do not copy the complete file into a `byte[]`. The input stream is disposed when the upload request completes.

```csharp
var stream = File.OpenRead("report.pdf");
var upload = await httpClient.UploadFileAsync<FileInfoDto>(
    "files",
    HttpMethod.Post,
    stream,
    "report.pdf",
    cancellationToken: cancellationToken);
```

Downloads stream into a same-directory temporary file and replace the destination only after a successful flush. Cancellation or transfer failure removes the temporary file and preserves an existing destination.

```csharp
var download = await httpClient.DownloadToFileAsync(
    "files/report.pdf",
    "report.pdf",
    progress: progress,
    cancellationToken: cancellationToken);
```

## Errors and cancellation

- `IsSuccess` is `true` only for a 2xx response without parsing errors
- ProblemDetails `errors` entries are flattened into `ApiResult.Errors`
- Legacy `IEnumerable<Error>` JSON arrays remain supported as a compatibility fallback
- Network, timeout, and JSON errors from typed calls return a failed `ApiResult`
- User cancellation always throws `OperationCanceledException`

Raw `HttpResponseMessage` and response streams are not returned through generic `T`; they are exposed through the ownership-explicit `SendAsync` method.
