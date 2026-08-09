# Linger.HttpClient.Contracts

`Linger.HttpClient` 的接口和传输模型包，不包含 HTTP 请求实现。

## 包含内容

- `IHttpClient`：类型化调用、原始响应、流式文件上传和文件下载契约
- `ApiResult` / `ApiResult<T>`：包含 HTTP 状态码和结构化错误的传输结果
- `ProblemDetailsWithErrors`：RFC 7807 ProblemDetails 字段错误模型
- `HttpClientExtensions`：GET、POST、PUT、DELETE 便捷方法

具体实现由 `Linger.HttpClient.Standard` 提供。

## 安装

```bash
dotnet add package Linger.HttpClient.Contracts
dotnet add package Linger.HttpClient.Standard
```

## 核心接口

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

## 基本用法

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

需要为单次 GET 请求设置不同于客户端默认值的超时时间时，使用 `GetWithTimeoutAsync`：

```csharp
var result = await httpClient.GetWithTimeoutAsync<User>(
    "users/42",
    timeout: TimeSpan.FromSeconds(5),
    cancellationToken: cancellationToken);
```

单次超时会返回失败的 `ApiResult<T>`；调用方通过 `cancellationToken` 主动取消时仍抛出 `OperationCanceledException`。传入 `null` 或 `Timeout.InfiniteTimeSpan` 不会增加单次超时限制。

动态请求头只应用于当前请求，客户端不会维护可变的共享令牌或请求头状态，也不会自动添加 `culture` 查询参数。固定请求头可以通过 `AddHttpClient` 配置；WinForms 等由调用方持有专属 `HttpClient` 的场景，也可以在创建实例时设置 `DefaultRequestHeaders.Authorization`。同一个客户端可能代表不同用户时，应继续使用每请求 `headers`。契约包不负责保存或刷新令牌；`DelegatingHandler` 接入方式以及与 `Linger.AspNetCore.Jwt` Refresh Token 的完整示例参见 [Linger.HttpClient.Standard README](../Linger.HttpClient.Standard/README.zh-CN.md)。

## 请求体

`CallApi<T>` 根据 `requestBody` 类型创建内容：

- `HttpContent`：直接发送，并在请求完成后释放
- `IDictionary<string, string>`：发送 `application/x-www-form-urlencoded`
- 其他对象：序列化为 JSON

```csharp
var result = await httpClient.CallApi<User>(
    "users",
    HttpMethod.Post,
    new CreateUserRequest("Ada"),
    cancellationToken: cancellationToken);
```

## 原始响应和流式处理

需要读取响应头、处理 SSE 或边接收边处理内容时，使用 `SendAsync`。它会继续使用同一个客户端的基础地址、默认请求头和 `DelegatingHandler` 链：

```csharp
using var response = await httpClient.SendAsync(
    "events",
    HttpMethod.Get,
    cancellationToken: cancellationToken);

response.EnsureSuccessStatusCode();
using var stream = await response.Content.ReadAsStreamAsync();
await ProcessStreamAsync(stream, cancellationToken);
```

调用方必须释放返回的 `HttpResponseMessage`。`SendAsync` 不解析非成功响应，也不把传输异常转换为 `ApiResult`；需要统一错误模型时使用 `CallApi<T>`。

## 文件传输

上传使用 `StreamContent`，不会把整个文件复制到 `byte[]`。上传请求完成后，传入的流会被释放。

```csharp
var stream = File.OpenRead("report.pdf");
var upload = await httpClient.UploadFileAsync<FileInfoDto>(
    "files",
    HttpMethod.Post,
    stream,
    "report.pdf",
    cancellationToken: cancellationToken);
```

下载直接流式写入目标目录中的临时文件，刷新成功后才替换目标文件。取消或传输失败会删除临时文件并保留已有目标文件。

```csharp
var download = await httpClient.DownloadToFileAsync(
    "files/report.pdf",
    "report.pdf",
    progress: progress,
    cancellationToken: cancellationToken);
```

## 错误和取消

- 2xx 且没有解析错误时，`IsSuccess` 为 `true`
- ProblemDetails 的 `errors` 会展开为 `ApiResult.Errors`
- 旧版 `IEnumerable<Error>` JSON 数组仍作为兼容回退解析
- 类型化调用中的网络、超时和 JSON 解析错误通过失败的 `ApiResult` 返回
- 用户主动取消始终抛出 `OperationCanceledException`

原始 `HttpResponseMessage` 和响应流不通过泛型 `T` 返回，而是通过所有权明确的 `SendAsync` 返回。
