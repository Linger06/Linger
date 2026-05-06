# Linger.HttpClient.Standard

基于 System.Net.Http.HttpClient 的生产可用 HTTP 客户端实现。

## 功能特性

- **零依赖**: 基于标准 .NET 库构建
- **HttpClientFactory 集成**: 正确的套接字管理和连接池
- **正确的资源管理**: 使用所有权模式自动跟踪释放，防止资源泄漏
- **流式下载支持**: 提供 `DownloadStreamAsync` 和 `DownloadToFileAsync`，适合大文件场景
- **响应模式可选**: 支持 `Buffered` / `Streamed` 响应读取模式
- **全面日志记录**: 内置性能监控
- **Linger.Results 集成**: 服务端到客户端的无缝错误映射
- **ProblemDetails 支持**: 原生支持 RFC 7807 标准

## 安装

```bash
dotnet add package Linger.HttpClient.Standard
```

## 快速上手

```csharp
// Program.cs / Startup.cs
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// 任意业务服务中
public sealed class UserQueryService
{
    private readonly IHttpClient _httpClient;

    public UserQueryService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<User?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.CallApi<User>($"api/users/{id}", cancellationToken: cancellationToken);

        if (result.IsSuccess && result.Data is not null)
        {
            return result.Data;
        }

        Console.WriteLine($"请求失败: {(int)result.StatusCode} - {result.ErrorMsg}");

        foreach (var error in result.Errors)
        {
            Console.WriteLine($"错误项: {error.Code} - {error.Message}");
        }

        return null;
    }
}

// 控制器或页面中调用
var user = await userQueryService.GetAsync(123);

if (user is not null)
{
    Console.WriteLine($"用户: {user.Name}");
}
else
{
    Console.WriteLine("未获取到用户，请查看上方错误输出。");
}
```

要点：
- 生产环境优先使用 HttpClientFactory。
- 失败时先看 `ErrorMsg` 和 `Errors`，不要只看状态码。
- 大文件优先使用 `DownloadStreamAsync` / `DownloadToFileAsync`。

## 基本用法

### 推荐：使用 HttpClientFactory（最佳实践）

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

### 使用现有 HttpClient 实例

如果已有 `HttpClient` 实例（例如来自 HttpClientFactory），可以包装它：

```csharp
// StandardHttpClient 不会释放外部的 HttpClient
var httpClient = httpClientFactory.CreateClient("MyClient");
using var standardClient = new StandardHttpClient(httpClient, logger);

var result = await standardClient.CallApi<User>("api/users/123");
```

### 直接实例化（不推荐用于生产环境）

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

与 Linger.Results 无缝集成，客户端可自动识别两类标准化错误输出：

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

客户端解析示例：

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

客户端解析示例：

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
> - 客户端基于 RFC 7807 解析 ProblemDetails，不强依赖 Linger.Results。
> - 服务端采用 Linger.Results 时，常见输出为 ProblemDetails（验证错误）或 `IEnumerable<Error>`（业务错误）。
> - 客户端会自动汇总结构化错误到全局消息，并保留 `Errors` 列表用于细粒度展示。

## ProblemDetails 支持

原生支持 RFC 7807 ProblemDetails（只要返回 `application/problem+json` 即可解析）。

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

客户端解析示例：

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

## 自定义错误处理

`StandardHttpClient` 是可继承的。对于既不返回 Linger.Results，也不返回 RFC 7807 ProblemDetails 的服务端响应，可以通过继承 `StandardHttpClient` 并重写错误解析逻辑来适配自定义错误格式。

最常见的扩展点是重写 `HttpClientBase` 中的 `GetErrorMessageAsync`，让客户端把自定义错误体转换为 `ErrorMsg` 和 `Errors`。

```csharp
public class CustomHttpClient : StandardHttpClient
{
    public CustomHttpClient(HttpClient httpClient, ILogger<StandardHttpClient>? logger = null)
        : base(httpClient, logger)
    {
    }

    protected override async Task<(string ErrorMsg, IEnumerable<Error> Errors)> GetErrorMessageAsync(HttpResponseMessage response)
    {
        var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        // 在这里解析你的自定义错误格式
        // 例如：{"code":"BusinessRule","message":"库存不足"}

        return await base.GetErrorMessageAsync(response).ConfigureAwait(false);
    }
}
```

## 调用流程与返回映射

本章详细说明：WebAPI 返回什么 → HttpClient.CallApi 如何调用 → 返回 ApiResult<T> 的各字段如何填充。

### 场景 1：成功情况

**WebAPI 返回 200 OK 且包含有效的 JSON 数据**

```
WebAPI 响应:
HTTP/1.1 200 OK
Content-Type: application/json
{
    "id": 123,
    "name": "张三",
    "email": "zhangsan@example.com"
}
```

**客户端调用与返回值：**

```csharp
// 客户端调用
var result = await _httpClient.CallApi<User>("api/users/123", cancellationToken: ct);

// 返回值字段映射:
// result.IsSuccess       = true
// result.Data            = User { Id = 123, Name = "张三", Email = "zhangsan@example.com" }
// result.StatusCode      = 200
// result.ErrorMsg        = null
// result.Errors          = 空数组

if (result.IsSuccess && result.Data is not null)
{
    var user = result.Data; // 直接使用反序列化的对象
    Console.WriteLine($"用户: {user.Name}");
}
```

### 场景 2：参数或验证错误（ProblemDetails）

**WebAPI 返回 422 且包含验证错误**

```
WebAPI 响应:
HTTP/1.1 422 Unprocessable Entity
Content-Type: application/problem+json
{
    "title": "One or more validation errors occurred.",
    "status": 422,
    "errors": {
        "Email": "邮箱格式不正确",
        "Age": "年龄必须大于 18"
    }
}
```

**客户端调用与返回值：**

```csharp
// 客户端调用 POST 请求，传入无效数据
var invalidUser = new User { Email = "invalid-email", Age = 10 };
var result = await _httpClient.CallApi<User>(
    "api/users",
    HttpMethodEnum.Post,
    requestBody: invalidUser
);

// 返回值字段映射:
// result.IsSuccess       = false
// result.Data            = null（因为 IsSuccess=false）
// result.StatusCode      = 422
// result.ErrorMsg        = "Email: 邮箱格式不正确\nAge: 年龄必须大于 18" （自动合并）
// result.Errors          = [
//     Error { Code = "Email", Message = "邮箱格式不正确" },
//     Error { Code = "Age", Message = "年龄必须大于 18" }
// ]

if (!result.IsSuccess)
{
    // 直接显示全局错误
    Console.WriteLine($"验证失败: {result.ErrorMsg}");
    
    // 或逐项显示用于表单内联提示
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"字段 {error.Code}: {error.Message}");
    }
}
```

### 场景 3：业务/全局错误（Linger.Results 格式）

**WebAPI 返回 409 Conflict 且包含业务错误数组**

```
WebAPI 响应:
HTTP/1.1 409 Conflict
Content-Type: application/json
[
    {
        "code": "InsufficientStock",
        "message": "库存不足，需求 10 件但仅剩 5 件"
    },
    {
        "code": "PaymentGatewayDown",
        "message": "支付网关暂时不可用，请稍后重试"
    }
]
```

**客户端调用与返回值：**

```csharp
// 客户端调用 POST 请求，提交订单
var order = new OrderSubmitRequest { /* ... */ };
var result = await _httpClient.CallApi<Order>(
    "api/orders/submit",
    HttpMethodEnum.Post,
    requestBody: order
);

// 返回值字段映射:
// result.IsSuccess       = false
// result.Data            = null（因为 IsSuccess=false）
// result.StatusCode      = 409
// result.ErrorMsg        = "InsufficientStock: 库存不足，需求 10 件但仅剩 5 件\n
//                           PaymentGatewayDown: 支付网关暂时不可用，请稍后重试" （自动合并）
// result.Errors          = [
//     Error { Code = "InsufficientStock", Message = "库存不足，需求 10 件但仅剩 5 件" },
//     Error { Code = "PaymentGatewayDown", Message = "支付网关暂时不可用，请稍后重试" }
// ]

if (!result.IsSuccess)
{
    // 全局错误消息会自动合并所有业务错误
    Console.WriteLine($"订单提交失败: {result.ErrorMsg}");
    
    // 逐项访问具体错误编码以进行不同处理
    foreach (var error in result.Errors)
    {
        switch (error.Code)
        {
            case "InsufficientStock":
                Console.WriteLine("请重新调整购物车数量");
                break;
            case "PaymentGatewayDown":
                Console.WriteLine("请稍后重试或更换支付方式");
                break;
        }
    }
}
```

### 场景 4：HTTP 错误（4xx / 5xx 无结构化错误体）

**WebAPI 返回 500 且无法解析错误体（或纯文本）**

```
WebAPI 响应:
HTTP/1.1 500 Internal Server Error
Content-Type: text/plain
Internal server error occurred
```

**客户端调用与返回值：**

```csharp
// 客户端调用
var result = await _httpClient.CallApi<ReportData>("api/reports/generate");

// 返回值字段映射:
// result.IsSuccess       = false
// result.Data            = null
// result.StatusCode      = 500
// result.ErrorMsg        = "Internal server error occurred" （直接使用响应体文本）
// result.Errors          = 空数组（没有结构化的错误信息）

if (!result.IsSuccess)
{
    if (result.StatusCode == HttpStatusCode.InternalServerError)
    {
        Console.WriteLine($"服务器错误: {result.ErrorMsg}");
        Console.WriteLine("请稍后重试或联系管理员");
    }
}
```

### 场景 5：自定义错误格式

**WebAPI 返回自定义格式的错误（既不是 ProblemDetails 也不是 Linger.Results 数组）**

```
WebAPI 响应:
HTTP/1.1 400 Bad Request
Content-Type: application/json
{
    "error_code": "CUSTOM_ERROR",
    "error_message": "自定义错误信息",
    "details": "这是一个自定义格式的错误"
}
```

**客户端需要继承 StandardHttpClient 来处理自定义格式：**

```csharp
public class CustomHttpClient : StandardHttpClient
{
    public CustomHttpClient(HttpClient httpClient, ILogger<StandardHttpClient>? logger = null)
        : base(httpClient, logger)
    {
    }

    protected override async Task<(string ErrorMsg, IEnumerable<Error> Errors)> GetErrorMessageAsync(HttpResponseMessage response)
    {
        var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        
        try
        {
            // 尝试解析自定义格式
            var customError = JsonSerializer.Deserialize<CustomErrorFormat>(responseText);
            if (customError is not null)
            {
                var errorMsg = $"{customError.ErrorCode}: {customError.ErrorMessage}";
                var errors = new[] { new Error(customError.ErrorCode, customError.Details) };
                return (errorMsg, errors);
            }
        }
        catch
        {
            // 解析失败，回退到默认处理
        }
        
        // 回退到默认的 ProblemDetails / Linger.Results 处理
        return await base.GetErrorMessageAsync(response).ConfigureAwait(false);
    }

    private record CustomErrorFormat(string ErrorCode, string ErrorMessage, string Details);
}

// 使用自定义客户端
services.AddHttpClient<IHttpClient, CustomHttpClient>();

// 返回值字段映射完全相同:
// result.IsSuccess       = false
// result.StatusCode      = 400
// result.ErrorMsg        = "CUSTOM_ERROR: 自定义错误信息"
// result.Errors          = [
//     Error { Code = "CUSTOM_ERROR", Message = "这是一个自定义格式的错误" }
// ]
```

### ApiResult<T> 字段对照表

| 字段 | 类型 | 成功时 | 失败时 | 说明 |
|------|------|--------|--------|------|
| `IsSuccess` | `bool` | `true` | `false` | 标识本次调用是否成功 |
| `Data` | `T` | 反序列化后的对象 | `null` | 只在 IsSuccess=true 时有意义 |
| `StatusCode` | `HttpStatusCode?` | `200` 等 2xx | `400` / `401` / `404` / `422` / `500` 等 | HTTP 状态码 |
| `ErrorMsg` | `string?` | `null` | 合并后的错误信息 | 自动合并 Errors 列表；若无法结构化解析则为原始响应文本 |
| `Errors` | `IEnumerable<Error>` | 空集合 | 错误详情列表 | Code 和 Message 字段的含义取决于错误类型（字段/业务错误/自定义） |

### 调用方法快速参考

```csharp
// 1. GET 请求（最简形式）
var result = await _httpClient.CallApi<User>("api/users/123");

// 2. GET 请求（带查询参数）
var result = await _httpClient.CallApi<IEnumerable<User>>(
    "api/users",
    queryParams: new { page = 1, pageSize = 10 }
);

// 3. POST 请求（带请求体）
var result = await _httpClient.CallApi<User>(
    "api/users",
    HttpMethodEnum.Post,
    requestBody: new { name = "张三", email = "zhangsan@example.com" }
);

// 4. PUT 请求（带请求体）
var result = await _httpClient.CallApi<User>(
    "api/users/123",
    HttpMethodEnum.Put,
    requestBody: new { name = "李四", email = "lisi@example.com" }
);

// 5. DELETE 请求
var result = await _httpClient.CallApi<object>(
    "api/users/123",
    HttpMethodEnum.Delete
);

// 6. 带超时和取消令牌
var result = await _httpClient.CallApi<User>(
    "api/users/123",
    timeout: 5000,
    cancellationToken: ct
);

// 7. 完整参数
var result = await _httpClient.CallApi<User>(
    "api/users/123",
    HttpMethodEnum.Get,
    requestBody: null,
    queryParams: new { includeDetails = true },
    timeout: 5000,
    cancellationToken: ct
);
```

## 与服务端约定

建议遵循以下约定，获得更稳定的错误映射体验：

1. 响应内容类型
- 验证错误使用 `application/problem+json`（RFC 7807）。
- 业务错误可使用错误数组（`IEnumerable<Error>` 结构）。

2. 错误数据结构
- ProblemDetails 场景：建议包含 `title`、`status`、`errors`。
- 错误数组场景：每项建议包含 `code` 和 `message`。

3. 状态码习惯
- 参数或验证失败：400 / 422
- 未授权或鉴权失败：401 / 403
- 资源不存在：404
- 业务冲突：409

## 核心方法

### CallApi<T>
```csharp
public async Task<ApiResult<T>> CallApi<T>(
    string url,
    HttpMethodEnum method,
    object? requestBody = null,
    object? queryParams = null,
    int? timeout = null,
    CancellationToken cancellationToken = default)
```

支持的 HTTP 方法：
- GET: 获取数据
- POST: 创建资源
- PUT: 更新资源
- DELETE: 删除资源

### 流式下载

大文件下载建议使用流式方法，可显著降低内存占用：

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

#### HttpResponseMode（Buffered / Streamed）

可按场景选择响应读取模式：

- `Buffered`: 适合小响应或需要一次性读取完整内容的场景
- `Streamed`: 适合大响应或下载场景，以更低内存占用逐步处理数据

| 场景 | 推荐模式 | 原因 |
|------|----------|------|
| 常规 JSON 接口（小到中等响应） | `Buffered` | 使用简单，便于直接反序列化 |
| 文件下载/导出 | `Streamed` | 避免整包进内存，降低峰值内存占用 |
| 可能超大响应（日志、报表、二进制） | `Streamed` | 更稳定，降低 OOM 风险 |
| 需要完整内容后统一处理 | `Buffered` | 业务处理逻辑更直接 |

**性能对比（下载 500MB 文件）：**

| 方法 | 内存占用 | 说明 |
|------|---------|------|
| `CallApi<byte[]>` | ~500MB | 将整个文件加载到内存 |
| `DownloadStreamAsync` | ~8KB | 仅缓冲区内存占用 |
| `DownloadToFileAsync` | ~8KB | 可自定义缓冲区大小 |

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

## 常见坑

- 不要用 `CallApi<byte[]>` 下载大文件：会将整个响应载入内存。
- 使用 `DownloadStreamAsync` 后要及时释放流：推荐配合 `using`。
- 下载任务建议传入取消令牌，便于超时或用户取消时快速中断。
- 包装外部 `HttpClient`（如工厂创建实例）时，不要重复管理其生命周期。
- 统一按结构化错误处理，优先读取 `Errors` 列表，而不只是打印状态码。

## 最佳实践

- 使用 HttpClientFactory 进行依赖注入
- 使用 `using` 语句确保资源正确释放
- 启用详细日志以便调试
- 合理设置超时时间
- 处理网络异常和超时情况
- **大文件下载使用流式方法**（`DownloadStreamAsync` 或 `DownloadToFileAsync`）以节省内存

## 更多示例

更多流式下载示例与性能对比见 [STREAMING_DOWNLOAD_EXAMPLE.md](STREAMING_DOWNLOAD_EXAMPLE.md)
