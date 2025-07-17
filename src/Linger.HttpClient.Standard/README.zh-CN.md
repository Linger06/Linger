# Linger.HttpClient.Standard

## 简介

Linger.HttpClient.Standard 是基于标准 .NET HttpClient 的实现，提供了符合 Linger.HttpClient.Contracts 接口的轻量级封装。本项目专注于提供稳定、高效、符合.NET风格的 HTTP 通信解决方案。

## 核心优势

- **轻量级设计**：最小依赖，运行时开销低
- **.NET集成**：与HttpClientFactory和依赖注入无缝协作
- **高性能**：针对.NET环境性能优化
- **简易配置**：使用熟悉的.NET模式简单设置
- **内置日志记录**：使用Microsoft.Extensions.Logging提供全面的日志支持
- **结构化日志**：性能指标、请求/响应跟踪和错误监控

## 安装

```bash
dotnet add package Linger.HttpClient.Standard
```

## 快速入门

### 基础创建

```csharp
// 直接创建客户端
var client = new StandardHttpClient("https://api.example.com");

// 配置选项
client.Options.DefaultTimeout = 30;
client.AddHeader("User-Agent", "Linger.Client");

// 创建带日志记录的客户端
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<StandardHttpClient>();
var clientWithLogging = new StandardHttpClient("https://api.example.com", logger);
```

### 使用日志记录和HttpClientFactory

```csharp
// 在启动配置中
services.AddLogging(builder => builder.AddConsole());
services.AddHttpClient<StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddTypedClient<IHttpClient>((httpClient, serviceProvider) => 
{
    var logger = serviceProvider.GetService<ILogger<StandardHttpClient>>();
    var standardClient = new StandardHttpClient(httpClient, logger);
    
    // 配置选项
    standardClient.Options.DefaultTimeout = 30;
    standardClient.AddHeader("User-Agent", "MyApp/1.0");
    
    return standardClient;
});
```

## 日志记录功能

### 日志级别

`StandardHttpClient` 在不同级别提供全面的日志记录：

- **Debug**：请求开始/结束、计时信息、配置详情
- **Trace**：详细的请求头、查询参数
- **Information**：成功操作
- **Warning**：失败的API调用、空URL警告
- **Error**：异常、超时、协议违规

### 示例日志输出

```
[Debug] StandardHttpClient initialized with base URL: https://api.example.com
[Debug] Starting API call: Post /api/users with timeout: 30s
[Trace] Request headers: Authorization: Bearer xxx, User-Agent: MyApp/1.0
[Debug] Query parameters appended to URL: culture=zh-CN
[Debug] API call completed in 245ms with status: Created
[Debug] API call successful for Post /api/users
```

### 配置日志记录

```csharp
// 开发环境 - 详细日志
services.AddLogging(builder =>
{
    builder.AddConsole()
           .SetMinimumLevel(LogLevel.Debug);
});

// 生产环境 - 仅关键信息
services.AddLogging(builder =>
{
    builder.AddFile("logs/httpclient-{Date}.txt")
           .SetMinimumLevel(LogLevel.Information);
});
```

## 使用示例

### 简单GET请求

```csharp
// 发送GET请求
var response = await client.CallApi<UserData>("api/users/1");

// 处理响应
if (response.IsSuccess)
{
    Console.WriteLine($"用户: {response.Data.Name}");
}
```

### 带JSON的POST请求

```csharp
// 创建用户数据
var userData = new UserCreateModel { Name = "张三", Email = "zhangsan@example.com" };

// 发送POST请求
var response = await client.CallApi<UserData>(
    "api/users",
    HttpMethodEnum.Post,
    userData
);

if (response.IsSuccess)
{
    Console.WriteLine($"创建用户成功: {response.Data.Id}");
}
```

### 文件上传

```csharp
// 读取文件
byte[] fileData = File.ReadAllBytes("document.pdf");

// 创建表单数据
var formData = new Dictionary<string, string>
{
    { "description", "示例文档" }
};

// 上传文件
var response = await client.CallApi<FileResponse>(
    "api/files",
    HttpMethodEnum.Post,
    formData,
    fileData,
    "document.pdf"
);

if (response.IsSuccess)
{
    Console.WriteLine($"文件上传成功: {response.Data.FileId}");
}
```

### 带查询参数的请求

```csharp
// 查询参数
var queryParams = new { page = 1, size = 10, keyword = "test" };

// 发送请求
var response = await client.CallApi<PagedResult<UserData>>(
    "api/users",
    queryParams
);

if (response.IsSuccess)
{
    Console.WriteLine($"获取到 {response.Data.Items.Count} 个用户");
}
```



## 最佳实践

### 配置

```csharp
// 生产环境推荐设置
client.Options.DefaultTimeout = 15; // 15秒超时
client.AddHeader("User-Agent", "MyApp/1.0");
client.AddHeader("Accept", "application/json");

// 或者通过Options设置
client.Options.DefaultHeaders["Authorization"] = "Bearer your-token";
client.Options.DefaultHeaders["Custom-Header"] = "custom-value";
```

### 带日志记录的错误处理

```csharp
var logger = serviceProvider.GetService<ILogger<StandardHttpClient>>();
var client = new StandardHttpClient("https://api.example.com", logger);

try
{
    var response = await client.CallApi<UserData>("api/users/1");
    
    if (response.IsSuccess)
    {
        // 处理数据
    }
    else
    {
        // 处理API错误 - 会自动记录日志
        Console.WriteLine($"API错误: {response.ErrorMsg}");
    }
}
catch (Exception ex)
{
    // 处理网络或其他异常 - 会自动记录日志
    Console.WriteLine($"请求失败: {ex.Message}");
}
```

### 资源管理

**使用HttpClientFactory（推荐）**：
```csharp
// 在启动配置中注册
services.AddHttpClient<IHttpClient, StandardHttpClient>();

// 在服务中使用 - 自动管理生命周期
public class UserService
{
    private readonly IHttpClient _httpClient;
    
    public UserService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<UserData> GetUserAsync(int id)
    {
        return await _httpClient.CallApi<UserData>($"users/{id}");
    }
}
```

**直接创建时的资源管理**：
```csharp
// 方式1：使用using语句
using var httpClient = new System.Net.Http.HttpClient();
var client = new StandardHttpClient(httpClient, logger);
// 使用客户端...
// using语句确保资源被正确释放

// 方式2：手动管理
var httpClient = new System.Net.Http.HttpClient();
try
{
    var client = new StandardHttpClient(httpClient, logger);
    // 使用客户端...
}
finally
{
    httpClient?.Dispose();
}
```

## 性能考虑

1. **日志级别优化**：生产环境建议使用 `Information` 级别，避免过于详细的日志影响性能
2. **HttpClientFactory使用**：推荐使用HttpClientFactory来避免端口耗尽问题
3. **超时设置**：根据API响应时间合理设置超时，避免长时间等待
4. **并发控制**：HttpClient线程安全，可以安全地在多线程环境中使用

## 注意事项

⚠️ **重要提醒**：
- 不要为每个请求创建新的HttpClient实例，应该重用
- 在使用日志记录时，注意敏感信息（如Authorization头）的处理
- 超时时间应该根据实际API响应时间来设置
- 在高并发场景下，建议使用HttpClientFactory