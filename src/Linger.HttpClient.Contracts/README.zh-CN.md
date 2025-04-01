# Linger.HttpClient.Contracts

## 目录
- [概述](#概述)
  - [Linger HTTP客户端生态系统](#linger-http客户端生态系统)
  - [特性](#特性)
  - [支持的.NET版本](#支持的net版本)
- [安装指南](#安装指南)
  - [通过NuGet安装](#通过nuget安装)
  - [使用Package Manager Console](#使用package-manager-console安装)
- [核心组件](#核心组件)
  - [核心接口](#核心接口)
  - [核心模型](#核心模型)
  - [设计理念](#设计理念)
- [使用指南](#使用指南)
  - [基本用法](#基本用法)
  - [扩展功能](#扩展功能)
  - [拦截器系统](#拦截器系统)
- [依赖注入集成](#依赖注入集成)
  - [基本注册方式](#基本注册方式)
  - [使用Microsoft的HttpClientFactory](#使用microsofts-httpclientfactory)
  - [多实例配置](#多实例配置)
- [Polly策略集成](#polly策略集成)
  - [常用策略类型](#常用策略类型)
  - [配置示例](#配置示例)
  - [与Linger拦截器结合](#与linger拦截器结合)
- [高级功能](#高级功能)
  - [自定义拦截器](#自定义拦截器)
  - [错误处理机制](#错误处理机制)
  - [性能监控](#性能监控)
- [最佳实践](#最佳实践)
  - [实例管理](#实例管理)
  - [请求优化](#请求优化)
  - [异常处理](#异常处理)
- [实现项目](#实现项目)

## 概述

### Linger HTTP客户端生态系统

Linger HTTP客户端生态系统由以下三个主要组件组成：

- **Linger.HttpClient.Contracts**：接口和抽象类，定义HTTP操作的标准契约（本项目）
- **[Linger.HttpClient.Standard](../Linger.HttpClient.Standard/README.zh-CN.md)**：基于.NET标准HttpClient的实现
- **[Linger.HttpClient.Flurl](../Linger.HttpClient.Flurl/README.zh-CN.md)**：基于Flurl.Http的流畅API实现

Linger.HttpClient.Contracts 定义了HTTP客户端操作的标准接口和契约，是Linger HTTP客户端实现的基础。通过使用统一的契约，您可以轻松切换不同的HTTP客户端实现，而无需修改业务代码。

### 特性
- 强类型HTTP客户端接口
- 支持各种HTTP方法（GET, POST, PUT, DELETE）
- 文件上传功能（通过统一的MultipartHelper简化处理）
- 请求/响应拦截器系统
- 可自定义HTTP选项
- 分页结果支持
- 自动重试机制
- 友好的错误处理
- 内置压缩支持
- 性能监控与统计

### 支持的.NET版本
- .NET Standard 2.0+
- .NET Framework 4.6.2+
- .NET 6.0+
- .NET 8.0/9.0

## 安装指南

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
dotnet add package Linger.HttpClient.Standard
dotnet add package Linger.HttpClient.Flurl
```

### 使用Package Manager Console安装

```powershell
# 安装接口和契约
Install-Package Linger.HttpClient.Contracts

# 安装实现
Install-Package Linger.HttpClient.Standard
# 或
Install-Package Linger.HttpClient.Flurl
```

## 核心组件

### 核心接口

#### IHttpClient

核心HTTP客户端接口，定义所有HTTP操作的标准方法：

```csharp
public interface IHttpClient
{
    // 基础HTTP方法
    Task<ApiResult<T>> CallApi<T>(string url, object? queryParams = null, int? timeout = null, 
        CancellationToken cancellationToken = default);
    
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, object? postData = null, 
        object? queryParams = null, int? timeout = null, CancellationToken cancellationToken = default);
    
    // 文件上传和表单提交
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method, IDictionary<string, string>? postData, 
        byte[] fileData, string filename, int? timeout = null, CancellationToken cancellationToken = default);
    
    // 配置和扩展
    void SetToken(string token);
    void AddHeader(string name, string value);
    void AddInterceptor(IHttpClientInterceptor interceptor);
    HttpClientOptions Options { get; }
}
```

#### IHttpClientInterceptor

请求/响应拦截器接口，允许在请求前后添加自定义逻辑：

```csharp
public interface IHttpClientInterceptor
{
    Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request);
    Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response);
}
```

#### IHttpClientFactory

HTTP客户端工厂接口，用于创建和管理HTTP客户端实例：

```csharp
public interface IHttpClientFactory
{
    IHttpClient CreateClient(string baseUrl);
    IHttpClient CreateClient(string baseUrl, Action<HttpClientOptions> configureOptions);
    IHttpClient GetOrCreateClient(string name);
    void RegisterClient(string name, string baseUrl, Action<HttpClientOptions>? configureOptions = null);
}
```

### 核心模型

#### ApiResult&lt;T&gt;

API调用的统一结果封装：

```csharp
public class ApiResult<T>
{
    // 响应数据
    public T Data { get; set; } = default!;
    
    // HTTP状态码
    public HttpStatusCode? StatusCode { get; set; }
    
    // 错误信息
    public ErrorObj? Errors { get; set; }
    public string? ErrorMsg { get; set; }
    
    // 辅助属性
    public bool IsSuccess => StatusCode.HasValue && (int)StatusCode.Value >= 200 && (int)StatusCode.Value < 300;
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
}
```

#### ApiPagedResult&lt;T&gt;

分页API结果封装：

```csharp
public class ApiPagedResult<T>
{
    // 当前页数据
    public List<T> Data { get; set; } = default!;
    
    // 分页信息
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageCount { get; set; }
    
    // 辅助属性
    public bool HasData => Data != null && Data.Count > 0;
    public bool HasNextPage => Page < PageCount;
}
```

#### HttpClientOptions

HTTP客户端配置选项：

```csharp
public class HttpClientOptions
{
    // 超时设置
    public int DefaultTimeout { get; set; } = 30;
    
    // 重试设置
    public bool EnableRetry { get; set; } = false;
    public int MaxRetryCount { get; set; } = 3;
    public int RetryInterval { get; set; } = 1000;
    
    // 请求头设置
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
}
```

### 设计理念

#### 接口隔离原则

Linger.HttpClient.Contracts 遵循接口隔离原则，将不同责任的接口分开：

- **IHttpClient**：定义基本的HTTP请求操作
- **IHttpClientInterceptor**：专注于请求/响应的拦截和修改
- **IHttpClientFactory**：负责客户端实例的创建和管理

#### 可扩展性

拦截器机制是核心的可扩展点，允许添加如下功能：

- 请求/响应日志记录
- 认证令牌自动刷新
- 请求重试和错误处理
- 响应缓存
- 性能监控

#### 统一响应处理

所有HTTP响应都被封装为 `ApiResult<T>`，提供一致的处理模式：

- 统一的成功/失败判断
- 类型安全的数据访问
- 结构化的错误信息

## 使用指南

### 基本用法

这是一个契约库，定义了接口和抽象类。对于具体实现，请使用`Linger.HttpClient.Standard`或`Linger.HttpClient.Flurl`。

#### 创建客户端

```csharp
// 创建HTTP客户端
var client = new Linger.HttpClient.Standard.StandardHttpClient("https://api.example.com");
```

#### 发送GET请求

```csharp
// GET请求
var result = await client.CallApi<UserData>("users/1");

// 处理响应
if (result.IsSuccess)
{
    var user = result.Data;
    Console.WriteLine($"用户: {user.Name}");
}
else
{
    Console.WriteLine($"错误: {result.ErrorMsg}");
}
```

#### 发送POST请求

```csharp
// POST请求
var postResult = await client.CallApi<UserData>("users", HttpMethodEnum.Post, 
    new { Name = "John", Email = "john@example.com" });
```

#### 带查询参数的请求

```csharp
// 带查询参数的GET请求
var queryResult = await client.CallApi<List<UserData>>("users", 
    new { page = 1, pageSize = 10 });
```

### 扩展功能

本库还提供了一些扩展方法以提供更便捷的API使用体验：

```csharp
// GET请求简化
var user = await client.GetAsync<UserData>("api/users/1");

// POST请求简化
var newUser = await client.PostAsync<UserData>("api/users", new { Name = "张三" });

// 分页请求简化
var pagedUsers = await client.GetPagedAsync<UserData>("api/users", new { page = 1, pageSize = 20 });
```

#### 文件上传

```csharp
// 文件上传
byte[] fileData = File.ReadAllBytes("document.pdf");
var formData = new Dictionary<string, string>
{
    { "description", "Sample document" },
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

### 拦截器系统

Linger.HttpClient.Contracts提供了一组内置拦截器，增强HTTP客户端功能：

#### 拦截器使用模式

```csharp
// 定义拦截器
public class LoggingInterceptor : IHttpClientInterceptor
{
    public Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request)
    {
        Console.WriteLine($"发送请求: {request.Method} {request.RequestUri}");
        return Task.FromResult(request);
    }
    
    public Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response)
    {
        Console.WriteLine($"收到响应: {(int)response.StatusCode}");
        return Task.FromResult(response);
    }
}

// 添加拦截器
client.AddInterceptor(new LoggingInterceptor());
```

#### 内置拦截器

##### 重试拦截器

自动重试因暂时性错误而失败的请求：

```csharp
// 创建并配置重试拦截器
var retryInterceptor = new RetryInterceptor(
    options,  // HttpClientOptions实例
    response => response.StatusCode == HttpStatusCode.ServiceUnavailable // 自定义重试条件
);

// 添加到HTTP客户端
client.AddInterceptor(retryInterceptor);
```

##### 缓存拦截器

缓存GET响应以减少服务器请求：

```csharp
// 创建并配置缓存拦截器
var cachingInterceptor = new CachingInterceptor(
    defaultCacheDuration: TimeSpan.FromMinutes(10) // 默认缓存10分钟
);

// 添加到HTTP客户端
client.AddInterceptor(cachingInterceptor);
```

##### 日志拦截器

记录请求和响应的详细信息：

```csharp
// 创建日志拦截器
var loggingInterceptor = new LoggingInterceptor(
    log => _logger.LogInformation(log) // 使用您的日志系统
);

// 添加到HTTP客户端
client.AddInterceptor(loggingInterceptor);
```

#### 工厂使用模式

```csharp
// 创建工厂
var factory = new DefaultHttpClientFactory();

// 注册命名客户端
factory.RegisterClient("users-api", "https://users.example.com", options => {
    options.DefaultTimeout = 20;
    options.EnableRetry = true;
});

// 获取命名客户端
var client = factory.GetOrCreateClient("users-api");
```

## 依赖注入集成

IHttpClient 接口设计支持依赖注入，可以在应用程序中轻松集成。

### 基本注册方式

```csharp
// 使用 Linger.HttpClient.Standard 实现
services.AddScoped<IHttpClient>(provider => 
    new StandardHttpClient("https://api.example.com"));

// 或使用 Linger.HttpClient.Flurl 实现
services.AddScoped<IHttpClient>(provider => 
    new FlurlHttpClient("https://api.example.com"));
```

#### 配置选项

```csharp
services.AddScoped<IHttpClient>(provider => 
{
    var client = new StandardHttpClient("https://api.example.com");
    
    // 配置选项
    client.Options.DefaultTimeout = 30; // 设置默认超时为30秒
    client.Options.EnableRetry = true;  // 启用重试
    client.Options.MaxRetryCount = 3;   // 最大重试次数
    
    // 添加默认请求头
    client.AddHeader("User-Agent", "Linger HttpClient");
    client.AddHeader("Accept", "application/json");
    
    // 添加请求/响应拦截器
    client.AddInterceptor(new LoggingInterceptor());
    
    return client;
});
```

#### 在服务中使用

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

### 使用Microsoft的HttpClientFactory

除了上述方法外，您还可以使用Microsoft的HttpClientFactory来管理HTTP客户端生命周期，这样可以避免DNS变更问题和socket耗尽等常见陷阱：

```csharp
// 1. 基本注册方式
services.AddHttpClient<IHttpClient, StandardHttpClient>(client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "Linger HttpClient");
});

// 2. 使用命名客户端并配置选项和拦截器
services.AddHttpClient("MyApi", client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddTypedClient<IHttpClient>((httpClient, serviceProvider) => 
{
    // 创建客户端实例
    var client = new StandardHttpClient(httpClient);
    
    // 配置选项
    client.Options.DefaultTimeout = 30;
    client.Options.EnableRetry = true;
    client.Options.MaxRetryCount = 3;
    
    // 添加拦截器
    var logger = serviceProvider.GetRequiredService<ILogger<IHttpClient>>();
    client.AddInterceptor(new LoggingInterceptor(log => logger.LogInformation(log)));
    client.AddInterceptor(new RetryInterceptor(client.Options));
    
    return client;
});

// 3. 添加消息处理器和策略
services.AddHttpClient<IHttpClient, StandardHttpClient>(client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.AddPolicyHandler(GetRetryPolicy()) // 添加Polly重试策略
.AddHttpMessageHandler(() => new CustomMessageHandler()); // 添加自定义处理程序

// 获取Polly策略的辅助方法
private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

#### 在服务中使用HttpClientFactory

```csharp
public class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpClient _client;
    
    public ApiService(
        IHttpClientFactory httpClientFactory,
        IHttpClient client) // 默认注入
    {
        _httpClientFactory = httpClientFactory;
        _client = client;
    }
    
    public async Task<UserData> GetNamedClientUserAsync(int userId)
    {
        // 获取类型化的命名客户端
        var apiClient = _httpClientFactory.CreateClient("MyApi")
            .GetTypedClient<IHttpClient>();
            
        var result = await apiClient.CallApi<UserData>($"users/{userId}");
        return result.Data;
    }
}
```

### 多实例配置

如果需要使用多个不同配置的HTTP客户端，可以使用命名注入：

```csharp
services.AddScoped<IHttpClient>(provider => 
    new StandardHttpClient("https://api1.example.com"));
    
services.AddKeyedScoped<IHttpClient>("api2", (provider, key) => 
    new StandardHttpClient("https://api2.example.com"));

// 使用时通过IServiceProvider获取
var api2Client = serviceProvider.GetKeyedService<IHttpClient>("api2");
```

## Polly策略集成

[Polly](https://github.com/App-vNext/Polly)是一个强大的.NET弹性和瞬态故障处理库，可以与Linger.HttpClient和Microsoft的HttpClientFactory无缝集成。

### 常用策略类型

1. **重试策略** - 自动重试失败的请求
2. **断路器策略** - 当系统检测到多次失败时临时停止尝试，防止级联故障
3. **超时策略** - 为请求设置超时限制
4. **回退策略** - 请求失败时提供备选响应
5. **策略组合** - 将多种策略组合使用

### 配置示例

#### 添加Polly支持

```bash
# 安装Polly与HttpClientFactory集成包
dotnet add package Microsoft.Extensions.Http.Polly
```

#### 重试策略

```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>(client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.AddPolicyHandler(GetRetryPolicy());

// 定义重试策略
private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // 处理网络错误和5xx、408响应
        .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests) // 也处理429响应
        .WaitAndRetryAsync(
            retryCount: 3, // 重试3次
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 指数退避
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                // 记录重试信息
                Console.WriteLine($"正在进行第{retryCount}次重试，延迟{timespan.TotalSeconds}秒");
            });
}
```

#### 断路器策略

```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>(client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.AddPolicyHandler(GetCircuitBreakerPolicy());

// 定义断路器策略
private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5, // 5次失败后断开电路
            durationOfBreak: TimeSpan.FromSeconds(30), // 断开30秒
            onBreak: (ex, breakDelay) => 
            {
                // 断路器打开时触发
                Console.WriteLine($"断路器已打开，将在{breakDelay.TotalSeconds}秒后尝试恢复");
            },
            onReset: () => 
            {
                // 断路器重置时触发
                Console.WriteLine("断路器已重置，服务恢复正常");
            });
}
```

#### 组合策略

```csharp
services.AddHttpClient<IHttpClient, StandardHttpClient>(client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.AddPolicyHandler(GetRetryPolicy()) // 首先应用重试策略
.AddPolicyHandler(GetCircuitBreakerPolicy()); // 然后应用断路器策略

// 也可以使用PolicyWrap显式组合策略
private static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
{
    return Policy.WrapAsync(GetRetryPolicy(), GetCircuitBreakerPolicy());
}
```

### 与Linger拦截器结合

Linger的拦截器系统和Polly策略可以结合使用，提供更强大的功能：

```csharp
// 1. 先配置Polly策略
services.AddHttpClient("resilient-api", client => 
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.AddPolicyHandler(GetRetryPolicy())
.AddTypedClient<IHttpClient>((httpClient, serviceProvider) => 
{
    // 2. 创建客户端实例
    var client = new StandardHttpClient(httpClient);
    
    // 3. 添加Linger拦截器处理Polly无法处理的场景
    client.AddInterceptor(new LoggingInterceptor(
        log => serviceProvider.GetRequiredService<ILogger<IHttpClient>>().LogInformation(log)
    ));
    client.AddInterceptor(new TokenRefreshInterceptor(
        serviceProvider.GetRequiredService<ITokenService>()
    ));
    
    return client;
});
```

#### Polly策略与Linger内置重试的区别

1. **作用层次不同**：
   - Polly策略作用于底层HttpClient实例，在网络请求级别处理重试
   - Linger的RetryInterceptor作用于应用层，可以访问完整的响应内容

2. **功能范围不同**：
   - Polly提供了更全面的弹性策略组合（重试、断路器、超时等）
   - Linger的拦截器更专注于业务逻辑处理（如令牌刷新）

3. **使用场景建议**：
   - 使用Polly处理网络级别和基础HTTP错误（超时、5xx错误等）
   - 使用Linger拦截器处理业务级错误和应用特定逻辑

## 高级功能

### 自定义拦截器

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

### 错误处理机制

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

### 性能监控

Linger.HttpClient支持性能监控，帮助识别和解决性能问题：

```csharp
// 记录请求开始
var requestId = HttpClientMetrics.Instance.StartRequest(url);

try
{
    // 执行HTTP请求
    var result = await _httpClient.CallApi<UserData>(url);
    
    // 记录成功完成
    HttpClientMetrics.Instance.EndRequest(url, requestId, result.IsSuccess);
    
    return result.Data;
}
catch
{
    // 记录失败
    HttpClientMetrics.Instance.EndRequest(url, requestId, false);
    throw;
}

// 获取特定端点的性能统计
var stats = HttpClientMetrics.Instance.GetEndpointStats("api/users");
Console.WriteLine($"平均响应时间: {stats.AverageResponseTime}ms");
Console.WriteLine($"成功率: {stats.SuccessRate * 100}%");

// 获取所有端点的统计
var allStats = HttpClientMetrics.Instance.GetAllStats();
foreach (var entry in allStats)
{
    Console.WriteLine($"端点: {entry.Key}, 请求数量: {entry.Value.TotalRequests}");
}
```

性能指标包括：
- 总请求数
- 成功/失败请求数
- 成功率
- 平均/最小/最大响应时间
- 当前活跃请求数

## 最佳实践

### 实例管理
- **推荐**：使用依赖注入容器管理HttpClient生命周期
- 避免为每个请求创建新实例，这可能导致端口耗尽
- 使用`HttpClientFactory`或依赖注入框架

### 请求优化
- 设置合理的超时值，避免请求无限期挂起
- 对大型响应使用流式处理而非完全加载到内存
- 使用压缩减少网络负载

### 异常处理
- 始终捕获并处理HTTP请求异常
- 实现退避策略处理服务器过载情况
- 使用断路器模式防止级联故障

## 实现项目

详细用法和示例请参考具体实现项目的文档：

- [StandardHttpClient 文档](../Linger.HttpClient.Standard/README.zh-CN.md)
- [FlurlHttpClient 文档](../Linger.HttpClient.Flurl/README.zh-CN.md)
