# Linger.HttpClient.Contracts

## 目录
- [简介](#简介)
- [特性](#特性)
- [支持的.NET版本](#支持的net版本)
- [安装](#安装)
- [基本使用](#基本使用)
- [依赖注入使用](#依赖注入使用)
- [使用HttpClientFactory](#使用HttpClientFactory)
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

## 安装

### 通过NuGet安装

要使用IHttpClient及其实现，需要安装以下NuGet包之一：

#### 选项1：安装基础HTTP客户端

```bash
# 安装接口和契约
dotnet add package Linger.HttpClient.Contracts

# 安装基于标准HttpClient的实现
dotnet add package Linger.HttpClient.Standard
```

#### 选项2：安装基于Flurl的HTTP客户端

```bash
# 安装接口和契约
dotnet add package Linger.HttpClient.Contracts

# 安装基于Flurl的实现
dotnet add package Linger.HttpClient.Flurl
```

#### 选项3：同时安装两种实现（可按需使用）

```bash
dotnet add package Linger.HttpClient.Contracts
dotnet add package Linger.HttpClient
dotnet add package Linger.HttpClient.Flurl
```

### 使用Package Manager Console安装

```powershell
# 安装接口和契约
Install-Package Linger.HttpClient.Contracts

# 安装实现
Install-Package Linger.HttpClient
# 或
Install-Package Linger.HttpClient.Flurl
```

## 基本使用
这是一个契约库，定义了接口和抽象类。对于具体实现，请使用`Linger.HttpClient`或`Linger.HttpClient.Flurl`。

### 简单调用示例

```csharp
// 创建HTTP客户端
var client = new Linger.HttpClient.Standard.StandardHttpClient("https://api.example.com");

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
// 使用 Linger.HttpClient.Standard 实现
services.AddScoped<IHttpClient>(provider => 
    new StandardHttpClient("https://api.example.com"));

// 或使用 Linger.HttpClient.Flurl 实现
services.AddScoped<IHttpClient>(provider => 
    new FlurlHttpClient("https://api.example.com"));
```

### 配置选项

```csharp
services.AddScoped<IHttpClient>(provider => 
{
    var client = new StandardHttpClient("https://api.example.com");
    
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
    new StandardHttpClient("https://api1.example.com"));
    
services.AddKeyedScoped<IHttpClient>("api2", (provider, key) => 
    new StandardHttpClient("https://api2.example.com"));

// 使用时通过IServiceProvider获取
var api2Client = serviceProvider.GetKeyedService<IHttpClient>("api2");
```

## 使用HttpClientFactory

`IHttpClientFactory`提供了一种统一管理HTTP客户端的方式，相比直接创建客户端实例有以下优势：

- 集中配置和管理HTTP客户端
- 支持命名客户端，方便在不同场景使用不同配置
- 自动管理客户端生命周期，避免资源泄漏
- 简化客户端配置和拦截器添加过程

### 注册HttpClientFactory

```csharp
// 在Startup.cs或Program.cs中注册HttpClientFactory

// 注册默认HTTP客户端工厂
services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();

// 或注册Flurl HTTP客户端工厂
services.AddSingleton<IHttpClientFactory, FlurlHttpClientFactory>();

// 预注册一些常用的命名客户端
var serviceProvider = services.BuildServiceProvider();
var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

// 注册不同API的客户端
factory.RegisterClient("api1", "https://api1.example.com", options => {
    options.DefaultTimeout = 30;
    options.EnableRetry = true;
    options.MaxRetryCount = 3;
});

factory.RegisterClient("api2", "https://api2.example.com", options => {
    options.DefaultTimeout = 60;
    options.EnableRetry = false;
});
```

### 使用工厂创建客户端

```csharp
// 方式1：直接使用工厂创建临时客户端
public class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public ApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<UserData> GetUserDataAsync(int userId)
    {
        // 创建一个基本客户端
        var client = _httpClientFactory.CreateClient("https://api.example.com");
        
        // 或创建一个带配置的客户端
        var configuredClient = _httpClientFactory.CreateClient("https://api.example.com", options => {
            options.DefaultTimeout = 15;
            options.EnableRetry = true;
        });
        
        var result = await client.CallApi<UserData>($"users/{userId}");
        return result.Data;
    }
}

// 方式2：使用预注册的命名客户端
public class NamedApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public NamedApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<UserData> GetUserFromApi1Async(int userId)
    {
        // 获取预注册的命名客户端
        var client = _httpClientFactory.GetOrCreateClient("api1");
        var result = await client.CallApi<UserData>($"users/{userId}");
        return result.Data;
    }
    
    public async Task<UserData> GetUserFromApi2Async(int userId)
    {
        var client = _httpClientFactory.GetOrCreateClient("api2");
        var result = await client.CallApi<UserData>($"users/{userId}");
        return result.Data;
    }
}
```

### 在依赖注入容器中注册命名客户端

```csharp
// 在Startup.cs或Program.cs中

// 注册工厂
services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();

// 注册命名客户端服务
services.AddScoped(provider => {
    var factory = provider.GetRequiredService<IHttpClientFactory>();
    return factory.GetOrCreateClient("api1");
});

// 使用命名注入
services.AddScoped<IHttpClient, IHttpClient>(serviceProvider => {
    var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    return factory.GetOrCreateClient("api2");
}, "api2");

// 在服务中使用
public class UserService
{
    private readonly IHttpClient _defaultClient; // api1
    private readonly IHttpClient _api2Client;
    
    public UserService(
        IHttpClient defaultClient, 
        [FromKeyedServices("api2")] IHttpClient api2Client)
    {
        _defaultClient = defaultClient;
        _api2Client = api2Client;
    }
    
    // 使用不同客户端的方法...
}
```

### 使用工厂添加拦截器

```csharp
// 在应用启动时配置
var factory = app.Services.GetRequiredService<IHttpClientFactory>();

// 注册带拦截器的客户端
factory.RegisterClient("api-with-logging", "https://api.example.com", options => {
    options.DefaultTimeout = 30;
});

// 获取客户端并添加拦截器
var client = factory.GetOrCreateClient("api-with-logging");
client.AddInterceptor(new LoggingInterceptor(logger));
client.AddInterceptor(new TokenRefreshInterceptor(tokenService, client));

// 在依赖注入容器中注册这个配置好的客户端
services.AddSingleton("api-with-logging", client);
```

### 动态添加客户端

```csharp
public class DynamicApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConcurrentDictionary<string, IHttpClient> _clientCache = new();
    
    public DynamicApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<T> CallExternalApiAsync<T>(string apiEndpoint, string baseUrl)
    {
        // 使用基础URL作为缓存键
        var client = _clientCache.GetOrAdd(baseUrl, url => {
            // 如果缓存中没有，则创建新客户端
            return _httpClientFactory.CreateClient(url, options => {
                options.DefaultTimeout = 30;
                options.EnableRetry = true;
            });
        });
        
        var result = await client.CallApi<T>(apiEndpoint);
        return result.Data;
    }
}
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

## 内置拦截器

Linger.HttpClient.Contracts提供了一系列内置拦截器，用于增强HTTP客户端的功能:

### 重试拦截器

自动重试因瞬时故障而失败的请求(例如：503 Service Unavailable, 504 Gateway Timeout, 429 Too Many Requests)：

```csharp
// 创建并配置重试拦截器
var retryInterceptor = new RetryInterceptor(
    maxRetries: 3, // 最大重试次数
    shouldRetry: response => response.StatusCode == HttpStatusCode.ServiceUnavailable, // 自定义重试条件
    delayFunc: async retryCount => await Task.Delay((int)Math.Pow(2, retryCount) * 100) // 指数退避策略
);

// 添加到HTTP客户端
client.AddInterceptor(retryInterceptor);
```

### 缓存拦截器

缓存GET请求的响应，减少对服务器的请求：

```csharp
// 创建并配置缓存拦截器
var cachingInterceptor = new CachingInterceptor(
    defaultCacheDuration: TimeSpan.FromMinutes(10) // 默认缓存10分钟
);

// 添加到HTTP客户端
client.AddInterceptor(cachingInterceptor);
```

### 日志拦截器

记录请求和响应的详细信息：

```csharp
// 创建日志拦截器
var loggingInterceptor = new LoggingInterceptor(
    log => _logger.LogInformation(log) // 使用您的日志系统
);

// 添加到HTTP客户端
client.AddInterceptor(loggingInterceptor);
```

## HTTP性能监控

Linger.HttpClient现在支持性能监控，帮助识别和解决性能问题：

```csharp
// 在发送请求前记录开始
var requestId = HttpClientMetrics.Instance.StartRequest(url);

try
{
    // 执行HTTP请求
    var result = await _httpClient.CallApi<UserData>(url);
    
    // 记录请求成功完成
    HttpClientMetrics.Instance.EndRequest(url, requestId, result.IsSuccess);
    
    return result.Data;
}
catch
{
    // 记录请求失败
    HttpClientMetrics.Instance.EndRequest(url, requestId, false);
    throw;
}

// 获取特定端点的性能统计数据
var stats = HttpClientMetrics.Instance.GetEndpointStats("api/users");
Console.WriteLine($"平均响应时间: {stats.AverageResponseTime}ms");
Console.WriteLine($"成功率: {stats.SuccessRate * 100}%");

// 获取所有端点的性能统计
var allStats = HttpClientMetrics.Instance.GetAllStats();
foreach (var entry in allStats)
{
    Console.WriteLine($"端点: {entry.Key}, 请求数: {entry.Value.TotalRequests}");
}
```

性能指标包括：
- 总请求数
- 成功/失败请求数
- 成功率
- 平均/最小/最大响应时间
- 当前活跃请求数

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
