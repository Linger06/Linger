# Linger.HttpClient.Contracts

[中文](README_zh-CN.md) | [English](README.md)

HTTP 客户端操作的标准接口和契约定义。

## 功能特性

- **接口解耦**: 业务逻辑与具体 HTTP 实现分离
- **实现灵活**: 支持多种 HTTP 客户端实现
- **测试友好**: 易于进行单元测试和模拟
- **强类型**: 泛型 `ApiResult<T>` 提供类型安全
- **异步支持**: 完整的 async/await 模式

## 安装

```bash
# 核心契约
dotnet add package Linger.HttpClient.Contracts

# 生产实现  
dotnet add package Linger.HttpClient.Standard
```

## 核心接口

### IHttpClient
```csharp
public interface IHttpClient : IDisposable
{
    Task<ApiResult<T>> CallApi<T>(string url, HttpMethodEnum method = HttpMethodEnum.Get, 
        object? data = null, Dictionary<string, string>? headers = null, 
        Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default);
}
```

### ApiResult<T>
```csharp
public class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public string ErrorMsg { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public Error[] Errors { get; set; }
}
```

## 基本用法

```csharp
// 依赖注入注册
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

## 与 Linger.Results 集成

`ApiResult` 与 `Linger.Results` 无缝集成：

```csharp
// 服务端使用 Linger.Results
public async Task<Result<User>> GetUserAsync(int id)
{
    var user = await _userRepository.GetUserAsync(id);
    return user is not null ? Result<User>.Success(user) : Result<User>.NotFound("用户未找到");
}

// 客户端接收结构化错误
var apiResult = await _httpClient.CallApi<User>($"api/users/{id}");
if (!apiResult.IsSuccess)
{
    // 自动映射的错误信息
    foreach (var error in apiResult.Errors)
        Console.WriteLine($"错误: {error.Code} - {error.Message}");
}
```

## 错误处理

```csharp
var result = await _httpClient.CallApi<User>("api/users/123");

if (result.IsSuccess)
{
    var user = result.Data;
    // 处理成功情况
}
else
{
    // 处理错误情况
    Console.WriteLine($"HTTP 状态: {result.StatusCode}");
    Console.WriteLine($"错误消息: {result.ErrorMsg}");
    
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"详细错误: {error.Code} - {error.Message}");
    }
}
```

## 最佳实践

- 使用依赖注入管理 HTTP 客户端生命周期
- 利用 `ApiResult` 的结构化错误处理
- 实现自定义错误处理逻辑时继承现有实现
- 使用 `CancellationToken` 支持请求取消
- 在单元测试中使用模拟实现