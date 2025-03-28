# Linger.HttpClient.Contracts

## 目录
- [简介](#简介)
- [特性](#特性)
- [支持的.NET版本](#支持的net版本)
- [基本使用](#基本使用)
- [依赖注入使用](#依赖注入使用)
- [高级用法](#高级用法)
- [性能优化与最佳实践](#性能优化与最佳实践)

## 简介
Linger.HttpClient.Contracts 是一个.NET库，提供了HTTP客户端操作的契约接口和基础实现。它是Linger框架中HTTP客户端实现的基础。

## 特性
- 强类型HTTP客户端接口
- 支持各种HTTP方法（GET, POST, PUT, DELETE）
- 文件上传功能
- 请求/响应拦截
- 可自定义HTTP选项
- 分页结果支持
- 自动重试机制
- 友好的错误处理

## 支持的.NET版本
该库支持使用.NET Framework 4.6.2+或.NET Standard 2.0+的.NET应用程序。

## 基本使用
这是一个契约库，定义了接口和抽象类。对于具体实现，请使用`Linger.HttpClient`或`Linger.HttpClient.Flurl`。

### 简单调用示例

```csharp
// 创建HTTP客户端
var client = new BaseHttpClient("https://api.example.com");

// GET请求
var result = await client.CallApi<UserData>("users/1");

// POST请求
var postResult = await client.CallApi<UserData>("users", HttpMethodEnum.Post, 
    new { Name = "John", Email = "john@example.com" });

// 带查询参数的GET请求
var queryResult = await client.CallApi<List<UserData>>("users", 
    new { page = 1, pageSize = 10 });
```

## 依赖注入使用
IHttpClient 接口设计支持依赖注入，可以在应用程序中轻松集成。以下是使用示例：

### 注册服务

```csharp
// 使用 Linger.HttpClient 实现
services.AddScoped<IHttpClient>(provider => 
    new BaseHttpClient("https://api.example.com"));

// 或使用 Linger.HttpClient.Flurl 实现
services.AddScoped<IHttpClient>(provider => 
    new FlurlHttpClient("https://api.example.com"));
```

### 配置选项

```csharp
services.AddScoped<IHttpClient>(provider => 
{
    var client = new BaseHttpClient("https://api.example.com");
    
    // 配置选项
    client.Options.DefaultTimeout = 30; // 设置默认超时时间为30秒
    client.Options.EnableRetry = true;  // 启用重试
    client.Options.MaxRetryCount = 3;   // 最大重试次数
    client.Options.RetryInterval = 1000; // 重试间隔（毫秒）
    
    // 添加默认请求头
    client.AddHeader("User-Agent", "Linger HttpClient");
    client.AddHeader("Accept", "application/json");
    
    // 添加请求/响应拦截器
    client.AddInterceptor(new LoggingInterceptor());
    
    return client;
});
```

### 在服务中使用

```csharp
public class MyService
{
    private readonly IHttpClient _httpClient;
    
    public MyService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<UserData> GetUserDataAsync(int userId)
    {
        var result = await _httpClient.CallApi<UserData>($"users/{userId}");
        
        if (result.IsSuccess)
        {
            return result.Data;
        }
        
        throw new Exception($"获取用户数据失败: {result.ErrorMsg}");
    }
}
```

### 多实例配置

如果需要同时使用多个不同配置的HTTP客户端，可以使用命名注入：

```csharp
services.AddScoped<IHttpClient>(provider => 
    new BaseHttpClient("https://api1.example.com"));
    
services.AddKeyedScoped<IHttpClient>("api2", (provider, key) => 
    new BaseHttpClient("https://api2.example.com"));

// 使用时通过IServiceProvider获取
var api2Client = serviceProvider.GetKeyedService<IHttpClient>("api2");
```

## 高级用法

### 实现自定义拦截器

```csharp
public class LoggingInterceptor : IHttpClientInterceptor
{
    private readonly ILogger _logger;
    
    public LoggingInterceptor(ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request)
    {
        _logger.LogInformation($"请求: {request.Method} {request.RequestUri}");
        return request;
    }
    
    public async Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response)
    {
        _logger.LogInformation($"响应: {(int)response.StatusCode} {response.ReasonPhrase}");
        return response;
    }
}
```

### 文件上传示例

```csharp
// 文件上传
byte[] fileData = File.ReadAllBytes("document.pdf");
var formData = new Dictionary<string, string>
{
    { "description", "示例文档" },
    { "category", "reports" }
};

var uploadResult = await client.CallApi<UploadResponse>(
    "files/upload", 
    HttpMethodEnum.Post, 
    formData, 
    fileData, 
    "document.pdf"
);
```

### 错误处理

```csharp
public async Task<T> ExecuteApiCall<T>(string endpoint)
{
    try
    {
        var result = await _httpClient.CallApi<T>(endpoint);
        
        if (!result.IsSuccess)
        {
            if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await RefreshTokenAsync();
                // 重试请求
                return await ExecuteApiCall<T>(endpoint);
            }
            
            if (result.Errors != null)
            {
                // 处理详细错误信息
                throw new ApiException(result.Errors);
            }
            
            throw new ApiException(result.ErrorMsg);
        }
        
        return result.Data;
    }
    catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
    {
        // 处理网络错误
        throw new NetworkException("网络请求失败", ex);
    }
}
```

## 性能优化与最佳实践

### HttpClient实例管理
- **推荐**: 使用依赖注入容器管理HttpClient实例生命周期
- 避免在每次请求时创建新的实例，这可能导致端口耗尽问题
- 使用`HttpClientFactory`或依赖注入框架

### 请求优化
- 设置合理的超时时间，避免请求长时间挂起
- 对大型响应使用流处理而不是一次性加载
- 使用压缩传输数据，减少网络负载

### 连接管理
- 对频繁访问的API，保持连接复用
- 对重要请求使用重试策略，但避免无限重试
- 合理设置重试间隔，避免DOS攻击目标服务器

### 异常处理
- 始终捕获并处理HTTP请求异常
- 实现退避策略处理服务器过载情况
- 使用断路器模式防止级联故障
