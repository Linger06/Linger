# Linger.HttpClient.Standard

基于 System.Net.Http.HttpClient 的生产级 HTTP 客户端实现。

## 功能特性

- **零依赖**: 基于标准 .NET 库构建
- **HttpClientFactory 集成**: 正确的套接字管理和连接池
- **正确的资源管理**: 使用所有权模式自动跟踪释放，防止资源泄漏
- **全面日志记录**: 内置性能监控
- **Linger.Results 集成**: 服务端到客户端的无缝错误映射
- **ProblemDetails 支持**: 原生支持 RFC 7807 标准

## 最近更新

### v1.0.0+ - 流式下载优化
- ✅ **流式下载支持**: 新增 `DownloadStreamAsync` 和 `DownloadToFileAsync` 方法
- ✅ **内存优化**: 大文件下载内存占用从 100% 文件大小降至 ~8KB 缓冲区（99.99% 减少）
- ✅ **进度报告**: `DownloadToFileAsync` 支持实时进度回调
- ✅ **HttpResponseMode**: 新增 `Buffered`/`Streamed` 模式，可根据场景选择

### v0.9.78 - 资源管理改进
- ✅ **修复资源泄漏**: 添加所有权跟踪，防止释放外部提供的 `HttpClient` 实例
- ✅ **安全释放**: 仅释放内部创建的 `HttpClient` 实例
- ✅ **HttpClientFactory 兼容**: 正确处理来自工厂的 `HttpClient` 实例，无释放问题

## 安装

```bash
dotnet add package Linger.HttpClient.Standard
```

## 基本用法

### ✅ 推荐：使用 HttpClientFactory（最佳实践）

```csharp
// 在 DI 容器中注册
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// 在服务中使用
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

### ⚠️ 使用现有 HttpClient 实例

如果已有 `HttpClient` 实例（例如来自 HttpClientFactory），可以包装它：

```csharp
// StandardHttpClient 不会释放外部的 HttpClient
var httpClient = httpClientFactory.CreateClient("MyClient");
using var standardClient = new StandardHttpClient(httpClient, logger);

var result = await standardClient.CallApi<User>("api/users/123");
```

### ⚠️ 直接实例化（不推荐用于生产环境）

仅在测试或简单场景中使用此方式：

```csharp
// ⚠️ 创建新的 HttpClient 实例
// StandardHttpClient 会在释放时一并释放它
using var client = new StandardHttpClient("https://api.example.com", logger);
var result = await client.CallApi<User>("api/users/123");
// HttpClient 在此处自动释放
```

**为什么推荐 HttpClientFactory：**
- ✅ 正确的连接池管理
- ✅ 自动处理 DNS 刷新
- ✅ 防止套接字耗尽
- ✅ 内置生命周期管理

## Linger.Results 集成

与 Linger.Results 框架无缝集成，客户端会自动识别两类标准化输出：

1) 字段/验证错误（ProblemDetailsWithErrors）

服务端常见响应（HTTP 400/422）：

```json
{
    "title": "One or more validation errors occurred.",
    "status": 422,
    "errors": {
        "Name": "不能为空",
        "Age": "必须 >= 18"
    }
}
```

客户端解析与呈现示例：

```csharp
var result = await _httpClient.CallApi<User>("api/users", HttpMethodEnum.Post, invalidUser);
if (!result.IsSuccess)
{
        // 全局消息会合并字段错误：
        // Name: 不能为空\nAge: 必须 >= 18
        Console.WriteLine(result.ErrorMsg);

        // Errors 列表用于表单内联提示（Code=字段名, Message=错误提示）
        foreach (var e in result.Errors)
        {
                Console.WriteLine($"字段: {e.Code}, 错误: {e.Message}");
        }
}
```

2) 业务/全局错误（IEnumerable<Error>）

服务端常见响应（HTTP 400/409/...）：

```json
[
    { "code": "BusinessRule", "message": "库存不足" },
    { "code": "PaymentFailed", "message": "支付网关超时" }
]
```

客户端解析与呈现示例：

```csharp
var apiResult = await _httpClient.CallApi<object>("api/orders/submit", HttpMethodEnum.Post, orderPayload);
if (!apiResult.IsSuccess)
{
        // 全局消息会合并每项错误：
        // BusinessRule: 库存不足\nPaymentFailed: 支付网关超时
        Console.WriteLine(apiResult.ErrorMsg);

        // Errors 列表保留每项（Code/Message）
        foreach (var error in apiResult.Errors)
        {
                Console.WriteLine($"错误: {error.Code} - {error.Message}");
        }
}
```

```csharp
// 服务端使用 Linger.Results
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetUserAsync(id);
    return result.ToActionResult(); // 自动 HTTP 状态映射
}

// 客户端自动接收结构化错误
var apiResult = await _httpClient.CallApi<User>($"api/users/{id}");
if (!apiResult.IsSuccess)
{
    foreach (var error in apiResult.Errors)
        Console.WriteLine($"错误: {error.Code} - {error.Message}");
}
```

> 说明：与 ProblemDetails 的关系
>
> - 客户端对 ProblemDetails 的支持是基于 RFC 7807 的通用能力，并不强依赖 Linger.Results。只要服务端返回 `application/problem+json`，本客户端即可自动解析。
> - 若服务端采用 Linger.Results（通常配合 `Linger.Results.AspNetCore`），常见输出会规范化为两类：
>   1) 字段/验证错误 → ProblemDetails（含 `errors` 字典，状态码常见为 400 或 422）；
>   2) 业务/全局错误 → `IEnumerable<Error>`（每项含 `Code` 与 `Message`）。
> - 本客户端已内建对应解析与合并策略：
>   - 对 ProblemDetails（含 `errors`）会将每个键值以“`Key: Value`”合并到全局错误消息中，同时保留 `Errors` 列表用于前端逐字段提示；
>   - 对 `IEnumerable<Error>` 会将每项以“`Code: Message`”合并为全局错误消息，同时保留 `Errors` 列表。
> - 因此：使用 Linger.Results 可获得“前后端无缝映射”的最佳体验；即便未使用 Linger.Results，只要遵循 RFC 7807 或返回错误数组，也能被本客户端正确解析并呈现。

## ProblemDetails 支持

原生支持 RFC 7807 ProblemDetails 格式（无论是否使用 Linger.Results，只要返回 `application/problem+json` 即可被解析）。

```json
{
    "title": "Invalid input.",
    "status": 400,
    "errors": {
        "Email": "格式不正确",
        "Password": "长度至少 8 位"
    }
}
```

客户端解析与呈现示例：

```csharp
var result = await _httpClient.CallApi<User>("api/users", HttpMethodEnum.Post, invalidUser);
if (!result.IsSuccess)
{
        // 全局错误会合并为：
        // Email: 格式不正确\nPassword: 长度至少 8 位
        Console.WriteLine(result.ErrorMsg);

        // Errors 列表：Code=键(字段), Message=提示
        foreach (var error in result.Errors)
        {
                Console.WriteLine($"字段: {error.Code}, 错误: {error.Message}");
        }
}
```

## 核心方法

### CallApi<T>
```csharp
public async Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method = HttpMethodEnum.Get, 
    object? data = null, Dictionary<string, string>? headers = null)
```

### 流式下载（v0.9.8+ 新增）

针对大文件下载，使用流式方法可以最大限度减少内存占用：

#### DownloadStreamAsync
```csharp
// 下载大文件为流（内存占用最小）
var result = await _httpClient.DownloadStreamAsync("https://example.com/large-file.zip");
if (result.IsSuccess && result.Data is not null)
{
    using var stream = result.Data;
    // 直接处理流，无需将整个文件加载到内存
    // 注意：必须手动释放 Stream 或使用 using 语句
}
```

#### DownloadToFileAsync（推荐）
```csharp
// 直接下载到文件，支持进度报告
var progress = new Progress<(long downloaded, long? total)>(p =>
{
    var percent = p.total.HasValue ? (double)p.downloaded / p.total.Value * 100 : 0;
    Console.WriteLine($"已下载: {p.downloaded} 字节 ({percent:F1}%)");
});

var result = await _httpClient.DownloadToFileAsync(
    url: "https://example.com/large-file.zip",
    destinationPath: "output.zip",
    progress: progress
);

if (result.IsSuccess)
{
    Console.WriteLine("下载完成！");
}
```

**流式下载的优势：**
- ✅ 内存占用极小（~8KB 缓冲区 vs 完整文件大小）
- ✅ 支持任意大小的文件
- ✅ 内置进度报告功能
- ✅ 支持取消令牌

**性能对比（下载 500MB 文件）：**

| 方法 | 内存占用 | 说明 |
|------|---------|------|
| `CallApi<byte[]>` | ~500MB | 将整个文件加载到内存 |
| `DownloadStreamAsync` | ~8KB | 仅缓冲区内存占用 |
| `DownloadToFileAsync` | ~8KB | 可自定义缓冲区大小 |

支持的 HTTP 方法：
- GET: 获取数据
- POST: 创建资源
- PUT: 更新资源
- DELETE: 删除资源
- PATCH: 部分更新

## 错误处理

```csharp
var result = await _httpClient.CallApi<User>("api/users/123");

if (result.IsSuccess)
{
    var user = result.Data;
}
else
{
    // 检查 HTTP 状态码
    switch (result.StatusCode)
    {
        case HttpStatusCode.NotFound:
            Console.WriteLine("用户未找到");
            break;
        case HttpStatusCode.Unauthorized:
            Console.WriteLine("需要身份验证");
            break;
    }
    
    // 访问详细错误
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"错误: {error.Code} - {error.Message}");
    }
}
```

## 最佳实践

- 使用 HttpClientFactory 进行依赖注入
- 使用 `using` 语句确保资源正确释放
- 启用详细日志以便调试
- 合理设置超时时间
- 处理网络异常和超时情况
- **大文件下载使用流式方法**（`DownloadStreamAsync` 或 `DownloadToFileAsync`）以节省内存

## 更多示例

完整的流式下载示例和性能对比，请参考 [STREAMING_DOWNLOAD_EXAMPLE.md](STREAMING_DOWNLOAD_EXAMPLE.md)
