# Linger.HttpClient.Standard

基于 `System.Net.Http.HttpClient` 的 `IHttpClient` 标准实现。

## 功能

- 使用 `HttpClientFactory` 管理底层连接
- JSON、表单和自定义 `HttpContent` 请求
- RFC 7807 ProblemDetails 与旧错误数组解析
- 每请求 headers，不保存共享认证状态
- 原始 `HttpResponseMessage` 和长连接流式处理
- 基于 `StreamContent` 的流式文件上传
- 临时文件提交式流式下载
- 用户取消传播和 `HttpClient.Timeout` 超时处理

## 安装和注册

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

WinForms、控制台等直接创建场景可以传入基础地址。此时 `StandardHttpClient` 持有底层客户端；应在应用生命周期内复用该实例，并在应用关闭时释放：

```csharp
using var client = new StandardHttpClient("https://api.example.com/");
```

需要设置超时、固定请求头或 `Accept-Language` 时，可以在创建阶段配置底层客户端：

```csharp
using var client = new StandardHttpClient(
    "https://api.example.com/",
    configureClient: httpClient =>
    {
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN");
    });
```

需要自定义 Handler、认证、证书或代理时，传入由调用方管理的 `HttpClient`。释放 `StandardHttpClient` 不会释放外部客户端：

也可以直接让 URL 模式持有自定义 Handler；此时 `StandardHttpClient` 负责释放 Handler 和底层客户端：

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

## 类型化调用

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

查询参数对象会缓存属性元数据；集合属性会生成多个同名参数，数值和日期使用固定区域性格式。

## 认证和请求头

### 固定认证头

当一个客户端实例只代表一个用户，且所有请求使用同一个令牌时，可以在创建阶段设置默认认证头：

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

应在应用生命周期内复用客户端，不要为每个请求重新创建实例。需要自动刷新或其他动态逻辑时，使用 `DelegatingHandler`，不要在仍有请求发送期间修改共享的 `DefaultRequestHeaders`。

### 使用 DelegatingHandler

`DelegatingHandler` 可以在请求发送前添加认证头、区域性或其他公共请求信息。处理器只修改当前 `HttpRequestMessage`，不修改共享客户端状态：

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

使用 Refresh Token 自动刷新时，客户端通常需要保存服务端返回的完整 `Token`，在并发请求中使用 `SemaphoreSlim` 防止重复刷新，并为刷新端点创建不带认证处理器的独立客户端。完整实现和 WinForms 生命周期示例参见 [Linger.HttpClient.WinForms 示例](../../examples/Linger.HttpClient.WinForms/README.zh-CN.md)；服务端端点参见 [Linger.AspNetCore.Jwt README](../Linger.AspNetCore.Jwt/README.zh-CN.md)。

### 动态或多用户认证

共享客户端可能代表不同用户发送请求时，应把动态认证信息按请求传入，避免修改共享 `DefaultRequestHeaders.Authorization`：

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

固定的服务端凭据也可以在 `AddHttpClient` 注册时配置。选择原则：专属实例的固定令牌使用默认认证头，不同请求可能使用不同令牌时使用 `headers` 参数。

客户端不会自动添加 `culture`。需要该约定时，应由调用方显式添加查询参数，或通过自定义 `DelegatingHandler` 统一处理。

## 文件上传

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

上传使用 `StreamContent`，不会把完整文件复制到内存。请求完成后会释放传入的流。

## 文件下载

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

下载流程：

1. 使用 `ResponseHeadersRead` 获取响应流。
2. 写入目标目录中的临时文件。
3. 下载与刷新成功后原子替换目标文件。
4. 取消或失败时删除临时文件并保留原目标文件。

## 原始响应和长连接

需要读取响应头、处理 SSE 或边接收边处理内容时，使用 `SendAsync`。它会复用 `StandardHttpClient` 的基础地址、默认请求头以及认证刷新等 `DelegatingHandler`：

```csharp
using var response = await client.SendAsync(
    "events",
    HttpMethod.Get,
    cancellationToken: cancellationToken);

response.EnsureSuccessStatusCode();
using var stream = await response.Content.ReadAsStreamAsync();
await ProcessStreamAsync(stream, cancellationToken);
```

返回的 `HttpResponseMessage` 由调用方释放。原始调用不会解析非成功响应，也不会把网络或超时异常转换为 `ApiResult`。

## 错误处理

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

解析顺序：

1. ProblemDetails（`application/problem+json` 或包含标准字段）
2. 旧版 `IEnumerable<Error>` 数组
3. 状态码消息和原始响应文本

任意普通 JSON 对象不会仅因为能反序列化而被误判为 ProblemDetails。异常的完整堆栈只写日志，`ErrorMsg` 不包含 `Exception.ToString()`。

## 取消和超时

用户取消保持标准 .NET 语义：

```csharp
try
{
    await client.GetAsync<User>("users/42", cancellationToken: cancellationToken);
}
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    // 调用方取消
}
```

超时由注册时设置的 `HttpClient.Timeout` 控制，并返回失败的 `ApiResult`。单次 GET 需要不同超时时间时使用 `GetWithTimeoutAsync`；其他请求可以使用带 `CancelAfter` 的调用方 `CancellationTokenSource`。

## 自定义 JSON 或错误格式

```csharp
public sealed class CustomHttpClient : StandardHttpClient
{
    public CustomHttpClient(HttpClient httpClient, ILogger<StandardHttpClient>? logger = null)
        : base(httpClient, logger)
    {
    }

    protected override JsonSerializerOptions GetResponseJsonOptions()
    {
        return customOptions;
    }
}
```

也可以重写 `GetErrorMessageAsync` 适配非 ProblemDetails 的服务端错误格式。
