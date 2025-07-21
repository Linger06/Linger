# Linger.HttpClient.Standard

## 目录
- [概述](#概述)
- [Linger.Results集成](#lingerresults集成)
- [安装](#安装)
- [快速开始](#快速开始)
- [配置](#配置)
- [使用示例](#使用示例)
- [错误处理](#错误处理)
- [性能与监控](#性能与监控)
- [故障排除](#故障排除)

## 概述

**Linger.HttpClient.Standard** 是 `Linger.HttpClient.Contracts` 的生产级实现，基于 `System.Net.Http.HttpClient` 构建，专为实际应用场景设计。

### 🎯 核心特性

- **零依赖** - 基于标准.NET库构建
- **HttpClientFactory集成** - 正确的套接字管理
- **全面日志记录** - 内置性能监控
- **资源管理** - 实现IDisposable
- **文化支持** - 自动国际化处理
- **Linger.Results集成** - 服务端到客户端的无缝错误映射

## Linger.Results集成

StandardHttpClient的 `ApiResult<T>` 与 **Linger.Results** 无缝集成，提供统一的错误处理体验。

### 🔗 错误映射

| 服务端 (Linger.Results) | 客户端 (ApiResult) | HTTP状态 |
|------------------------|-------------------|-------------|
| `Result<T>.NotFound("用户未找到")` | `ApiResult<T>` 其中 `Errors[0].Code = "NotFound"` | 404 |
| `Result<T>.Failure("邮箱无效")` | `ApiResult<T>` 其中 `Errors[0].Code = "Error"` | 400/500 |

### 🚀 使用示例

```csharp
// 服务端: API控制器
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetUserAsync(id);
    return result.ToActionResult(); // 自动HTTP状态映射
}

// 客户端: 自动接收结构化错误
var apiResult = await _httpClient.CallApi<User>($"api/users/{id}");
if (!apiResult.IsSuccess)
{
    foreach (var error in apiResult.Errors)
        Console.WriteLine($"错误: {error.Code} - {error.Message}");
}
```

### 🔧 与其他API集成

如果服务端**没有使用Linger.Results**，StandardHttpClient同样能很好地工作：

```csharp
// 标准REST API响应
// HTTP 404: { "message": "User not found", "code": "USER_NOT_FOUND" }
var result = await _httpClient.CallApi<User>("api/users/999");
if (!result.IsSuccess)
{
    Console.WriteLine($"状态码: {result.StatusCode}");
    Console.WriteLine($"错误消息: {result.ErrorMsg}"); // "User not found"
    // result.Errors 将根据响应体自动填充
}

// 自定义错误格式
// HTTP 400: { "errors": [{"field": "email", "message": "Invalid format"}] }
var createResult = await _httpClient.CallApi<User>("api/users", HttpMethodEnum.Post, invalidUser);
if (!createResult.IsSuccess)
{
    foreach (var error in createResult.Errors)
    {
        Console.WriteLine($"字段: {error.Code}, 消息: {error.Message}");
    }
}

// 简单文本错误
// HTTP 500: "Internal server error"
var serverErrorResult = await _httpClient.CallApi<User>("api/users/error");
if (!serverErrorResult.IsSuccess)
{
    Console.WriteLine($"服务器错误: {serverErrorResult.ErrorMsg}");
    // 即使是纯文本也会被正确处理
}
```

### 🎛️ 自定义错误解析

对于特殊的API错误格式，可以通过继承StandardHttpClient并重写`GetErrorMessageAsync`方法：

```csharp
public class CustomApiHttpClient : StandardHttpClient
{
    public CustomApiHttpClient(string baseUrl, ILogger<StandardHttpClient>? logger = null) 
        : base(baseUrl, logger)
    {
    }

    protected override async Task<(string ErrorMessage, Error[] Errors)> GetErrorMessageAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        
        try
        {
            // 自定义API错误格式: { "error": { "message": "xxx", "details": [...] } }
            var errorResponse = JsonSerializer.Deserialize<CustomErrorResponse>(content);
            if (errorResponse?.Error != null)
            {
                var errors = errorResponse.Error.Details?.Select(d => new Error(d.Code, d.Message)).ToArray() 
                           ?? new[] { new Error("API_ERROR", errorResponse.Error.Message) };
                           
                return (errorResponse.Error.Message, errors);
            }
        }
        catch (JsonException)
        {
            // JSON解析失败，使用默认处理
        }
        
        // 回退到默认错误解析
        return await base.GetErrorMessageAsync(response);
    }
    
    private class CustomErrorResponse
    {
        public CustomError? Error { get; set; }
    }
    
    private class CustomError
    {
        public string Message { get; set; } = "";
        public CustomErrorDetail[]? Details { get; set; }
    }
    
    private class CustomErrorDetail
    {
        public string Code { get; set; } = "";
        public string Message { get; set; } = "";
    }
}

// 使用自定义客户端
services.AddHttpClient<IHttpClient, CustomApiHttpClient>();
```

## 安装

```bash
dotnet add package Linger.HttpClient.Standard
```

## 快速开始

### 基本用法

```csharp
// 在DI容器中注册
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

    public async Task<User?> CreateUserAsync(CreateUserRequest request)
    {
        var result = await _httpClient.CallApi<User>("api/users", HttpMethodEnum.Post, request);
        return result.IsSuccess ? result.Data : null;
    }
}
```

### 带日志记录

```csharp
services.AddLogging(builder => builder.AddConsole());
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

## 配置

### HttpClient选项

```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
});
```

### StandardHttpClient选项

```csharp
var client = new StandardHttpClient("https://api.example.com");
client.Options.DefaultTimeout = 30;
client.AddHeader("Authorization", "Bearer token");
```

## 使用示例

### GET请求

```csharp
var result = await _httpClient.CallApi<UserData>("api/users/123");
if (result.IsSuccess)
{
    Console.WriteLine($"用户: {result.Data.Name}");
}
```

### POST JSON数据

```csharp
var createRequest = new CreateUserRequest { Name = "张三", Email = "zhangsan@example.com" };
var result = await _httpClient.CallApi<User>("api/users", HttpMethodEnum.Post, createRequest);
```

### 文件上传

```csharp
var fileData = File.ReadAllBytes("document.pdf");
var result = await _httpClient.CallApi<UploadResult>(
    "api/upload", 
    HttpMethodEnum.Post, 
    fileData, 
    headers: new Dictionary<string, string> { ["Content-Type"] = "application/pdf" }
);
```

### 带查询参数

```csharp
var queryParams = new Dictionary<string, object>
{
    ["page"] = 1,
    ["size"] = 10,
    ["active"] = true
};
var result = await _httpClient.CallApi<PagedResult<User>>("api/users", queryParams: queryParams);
```

## 错误处理

### Linger.Results兼容的错误处理

将 `ApiResult<T>` 转换为 `Result<T>` 以保持一致的错误处理模式：

```csharp
public async Task<Result<User>> GetUserAsync(int id)
{
    var apiResult = await _httpClient.CallApi<User>($"api/users/{id}");
    
    if (apiResult.IsSuccess)
        return Result<User>.Success(apiResult.Data);
        
    return apiResult.StatusCode switch
    {
        HttpStatusCode.NotFound => Result<User>.NotFound("用户未找到"),
        HttpStatusCode.BadRequest => Result<User>.Failure(apiResult.ErrorMsg),
        HttpStatusCode.Unauthorized => Result<User>.Failure($"访问被拒绝: {apiResult.ErrorMsg}"),
        _ => Result<User>.Failure($"服务器错误: {apiResult.ErrorMsg}")
    };
}
```

### ApiResult模式

```csharp
var result = await _httpClient.CallApi<UserData>("api/users/123");

if (result.IsSuccess)
{
    // 成功情况
    var user = result.Data;
    Console.WriteLine($"用户: {user.Name}");
}
else
{
    // 错误情况
    Console.WriteLine($"错误: {result.ErrorMsg}");
    
    // 处理特定状态码
    switch (result.StatusCode)
    {
        case HttpStatusCode.NotFound:
            Console.WriteLine("用户未找到");
            break;
        case HttpStatusCode.Unauthorized:
            Console.WriteLine("需要身份验证");
            break;
        default:
            Console.WriteLine($"HTTP {(int)result.StatusCode}: {result.ErrorMsg}");
            break;
    }
    
    // 访问详细错误
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"错误代码: {error.Code}, 消息: {error.Message}");
    }
}
```

### 异常处理

```csharp
try
{
    var result = await _httpClient.CallApi<UserData>("api/users/123");
    // 处理结果...
}
catch (HttpRequestException ex)
{
    // 网络级错误
    Console.WriteLine($"网络错误: {ex.Message}");
}
catch (TaskCanceledException ex)
{
    // 超时错误
    Console.WriteLine($"请求超时: {ex.Message}");
}
```

## 性能与监控

### 内置日志记录

StandardHttpClient自动记录：
- **请求/响应详情** (Debug级别)
- **性能指标** (Information级别)
- **错误和警告** (Warning/Error级别)

```csharp
// 示例日志输出
[INF] HTTP GET https://api.example.com/api/users/123 completed in 245ms (Status: 200)
[DBG] Request Headers: Accept: application/json, User-Agent: MyApp/1.0
[DBG] Response Headers: Content-Type: application/json; charset=utf-8
```

### 性能监控

```csharp
public class MonitoredUserService
{
    private readonly IHttpClient _httpClient;
    private readonly ILogger<MonitoredUserService> _logger;

    public MonitoredUserService(IHttpClient httpClient, ILogger<MonitoredUserService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<User?> GetUserAsync(int id)
    {
        using var activity = Activity.StartActivity("GetUser");
        activity?.SetTag("user.id", id);

        var stopwatch = Stopwatch.StartActivity();
        var result = await _httpClient.CallApi<User>($"api/users/{id}");
        stopwatch.Stop();

        _logger.LogInformation("GetUser completed in {ElapsedMs}ms, Success: {Success}", 
            stopwatch.ElapsedMilliseconds, result.IsSuccess);

        return result.IsSuccess ? result.Data : null;
    }
}
```

## 故障排除

### 常见问题

**1. 连接超时**
```csharp
// 增加超时时间
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});
```

**2. SSL证书问题**
```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
```

**3. 端口耗尽**
- 始终使用HttpClientFactory（DI中自动）
- 不要在循环中手动创建StandardHttpClient实例

**4. 内存泄漏**
```csharp
// ✅ 正确：使用DI
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// ❌ 错误：手动创建不释放
var client = new StandardHttpClient("https://api.example.com");

// ✅ 正确：手动创建要释放
using var client = new StandardHttpClient("https://api.example.com");
```

### 调试技巧

**启用详细日志**
```json
{
  "Logging": {
    "LogLevel": {
      "Linger.HttpClient.Standard": "Debug"
    }
  }
}
```

**检查网络流量**
- 使用Fiddler、Wireshark或浏览器开发工具
- 检查日志中的请求/响应头
- 验证JSON序列化/反序列化

---

## 📖 相关文档

- **[Linger.HttpClient.Contracts](../Linger.HttpClient.Contracts/README.zh-CN.md)** - 接口定义和架构指导
- **[Linger.Results](../Linger.Results/README.zh-CN.md)** - 与ApiResult无缝集成的服务端结果模式
- **[Microsoft HttpClientFactory](https://docs.microsoft.com/zh-cn/dotnet/core/extensions/httpclient-factory)** - 官方.NET文档
