# Linger.HttpClient.Contracts

## 目录
- [概述](#概述)
- [特性](#特性)
- [安装](#安装)
- [ApiResult与Linger.Results的关联](#apiresult与lingerresults的关联)
- [依赖注入集成](#依赖注入集成)
  - [使用HttpClientFactory](#使用httpclientfactory)
  - [在服务类中使用](#在服务类中使用)
- [基本用法](#基本用法)
  - [GET请求](#get请求)
  - [POST请求](#post请求)
  - [文件上传](#文件上传)
- [错误处理](#错误处理)
- [自动令牌刷新](#自动令牌刷新)
- [最佳实践](#最佳实践)

## 概述

Linger.HttpClient.Contracts 定义了HTTP客户端操作的标准接口和契约，是Linger HTTP客户端实现的基础。通过使用统一的契约，您可以轻松切换不同的HTTP客户端实现，而无需修改业务代码。

### 主要组件

- **Linger.HttpClient.Contracts**: 核心接口和契约（本项目）
- **Linger.HttpClient.Standard**: 基于.NET标准HttpClient的实现

## 特性

- 强类型HTTP客户端接口
- 支持各种HTTP方法（GET, POST, PUT, DELETE）
- 文件上传功能
- 请求/响应处理
- 友好的错误处理
- 超时管理

## 安装

```bash
# 安装接口和契约
dotnet add package Linger.HttpClient.Contracts

# 安装基于标准HttpClient的实现
dotnet add package Linger.HttpClient.Standard

# 弹性功能包（自动重试、断路器等）
dotnet add package Microsoft.Extensions.Http.Resilience
```

## ApiResult与Linger.Results的关联

`ApiResult`类型专门设计为与`Linger.Results`项目无缝协作，特别是与其ASP.NET Core集成部分。`ApiResult`中的`Errors`属性专门用于承接`Linger.Results.AspNetCore`项目中`ToActionResult()`方法转换后的错误信息。

### 错误信息格式对应关系

当API服务端使用`Linger.Results`返回结果并通过`ToActionResult()`方法转换时：

```csharp
// 服务端代码
public Result<UserDto> GetUser(int id)
{
    if (userNotFound)
        return Result<UserDto>.NotFound("用户不存在");
        
    return Result.Success(userDto);
}

// 控制器
[HttpGet("{id}")]
public ActionResult<UserDto> GetUser(int id)
{
    var result = _userService.GetUser(id);
    return result.ToActionResult(); // 失败时转换为包含errors的JSON响应
}
```

客户端通过`ApiResult`可以直接接收并处理这些错误信息：

```csharp
// 客户端代码
var apiResult = await _httpClient.CallApi<UserDto>($"api/users/{id}");

if (!apiResult.IsSuccess)
{
    // apiResult.Errors将包含服务端Result对象通过ToActionResult()转换后的错误信息
    foreach (var error in apiResult.Errors)
    {
        Console.WriteLine($"错误代码：{error.Code}, 消息：{error.Message}");
    }
}
```

### 错误结构映射

`Linger.Results`中的`Error`记录类型会被映射到`ApiResult.Errors`集合中：

| Linger.Results (服务端) | ApiResult (客户端) |
|---------------------|-------------------|
| Error.Code          | ApiError.Code     |
| Error.Message       | ApiError.Message  |

这种设计确保了从服务端到客户端的错误信息传递一致且完整，使客户端能够精确理解和处理服务端返回的业务错误。

你也可以重写GetErrorMessageAsync方法,接收从WebApi回传的其他类型的Error。

## 依赖注入集成

### 使用HttpClientFactory

使用Linger HTTP客户端的推荐方式是结合Microsoft的HttpClientFactory和HTTP弹性扩展：

```csharp
// 在启动配置中
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddResilienceHandler("Default", builder =>
{
    // 配置重试行为
    builder.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2),
        ShouldHandle = args =>
        {
            // 在服务器错误和速率限制时重试
            return ValueTask.FromResult(args.Outcome.Result?.StatusCode is
                HttpStatusCode.RequestTimeout or      // 408
                HttpStatusCode.TooManyRequests or     // 429
                HttpStatusCode.BadGateway or          // 502
                HttpStatusCode.ServiceUnavailable or  // 503
                HttpStatusCode.GatewayTimeout);       // 504
        }
    });
});
```

### 在服务类中使用

注册后，您可以在服务中注入和使用IHttpClient：

```csharp
public class UserService
{
    private readonly IHttpClient _httpClient;

    public UserService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResult<UserInfo>> GetUserInfoAsync(string userId)
    {
        return await _httpClient.CallApi<UserInfo>($"api/users/{userId}");
    }

    public async Task<ApiResult<UserInfo>> CreateUserAsync(UserCreateModel model)
    {
        return await _httpClient.CallApi<UserInfo>("api/users", HttpMethodEnum.Post, model);
    }
}
```

## 基本用法

### GET请求

```csharp
// 简单GET请求
var result = await _httpClient.CallApi<UserData>("api/users/1");

// 带查询参数
var users = await _httpClient.CallApi<List<UserData>>("api/users", 
    new { page = 1, pageSize = 10 });
```

### POST请求

```csharp
// 带JSON正文的POST请求
var newUser = await _httpClient.CallApi<UserData>("api/users", 
    HttpMethodEnum.Post, 
    new { Name = "张三", Email = "zhangsan@example.com" });
```

### 文件上传

```csharp
// 带表单数据的文件上传
byte[] fileData = File.ReadAllBytes("document.pdf");
var formData = new Dictionary<string, string>
{
    { "description", "示例文档" }
};

var uploadResult = await _httpClient.CallApi<UploadResponse>(
    "api/files/upload", 
    HttpMethodEnum.Post, 
    formData, 
    fileData, 
    "document.pdf"
);
```

## 错误处理

```csharp
var result = await _httpClient.CallApi<UserData>($"api/users/{userId}");

if (result.IsSuccess)
{
    // 处理成功响应
    var user = result.Data;
    Console.WriteLine($"用户: {user.Name}");
}
else
{
    // 处理错误
    Console.WriteLine($"错误: {result.ErrorMsg}");
    
    // 检查特定状态码
    if (result.StatusCode == HttpStatusCode.Unauthorized)
    {
        // 处理认证错误
    }
}
```

## 自动令牌刷新

您可以使用Microsoft.Extensions.Http.Resilience实现自动令牌刷新：

```csharp
// 创建令牌刷新处理器
public class TokenRefreshHandler
{
    private readonly AppState _appState;
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public TokenRefreshHandler(AppState appState, IServiceProvider serviceProvider)
    {
        _appState = appState;
        _serviceProvider = serviceProvider;
    }

    public void ConfigureTokenRefreshResiliencePipeline(ResiliencePipelineBuilder<HttpResponseMessage> builder)
    {
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 1, // 只尝试刷新一次
            ShouldHandle = args => 
            {
                bool shouldRetry = args.Outcome.Result?.StatusCode == HttpStatusCode.Unauthorized;
                return ValueTask.FromResult(shouldRetry);
            },
            OnRetry = async context =>
            {
                // 线程安全的令牌刷新
                await _semaphore.WaitAsync();
                try
                {
                    await RefreshTokenAsync();
                }
                finally
                {
                    _semaphore.Release();
                }
            },
            BackoffType = DelayBackoffType.Constant,
            Delay = TimeSpan.Zero // 令牌刷新后立即重试
        });
    }

    private async Task RefreshTokenAsync()
    {
        // 实现令牌刷新逻辑
        // ...
    }
}

// 在应用程序中注册和使用
services.AddSingleton<TokenRefreshHandler>();
services.AddHttpClient<IHttpClient, StandardHttpClient>(/* ... */)
    .AddResilienceHandler("TokenRefresh", (builder, context) =>
    {
        var tokenRefreshHandler = context.ServiceProvider.GetRequiredService<TokenRefreshHandler>();
        tokenRefreshHandler.ConfigureTokenRefreshResiliencePipeline(builder);
    });
```

## 最佳实践

1. **使用HttpClientFactory管理生命周期**
2. **设置合理的超时值**
3. **实现适当的错误处理**
4. **为API响应使用强类型模型**
5. **启用自动令牌刷新以提供更好的用户体验**
6. **使用Microsoft.Extensions.Http.Resilience进行重试逻辑**

更多信息，请参阅[Linger.HttpClient.Standard文档](../Linger.HttpClient.Standard/README.zh-CN.md)。
