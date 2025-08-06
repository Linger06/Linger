# Linger.HttpClient.Standard

[中文](README_zh-CN.md) | [English](README.md)

基于 System.Net.Http.HttpClient 的生产级 HTTP 客户端实现。

## 功能特性

- **零依赖**: 基于标准 .NET 库构建
- **HttpClientFactory 集成**: 正确的套接字管理
- **全面日志记录**: 内置性能监控
- **Linger.Results 集成**: 服务端到客户端的无缝错误映射
- **ProblemDetails 支持**: 原生支持 RFC 7807 标准

## 安装

```bash
dotnet add package Linger.HttpClient.Standard
```

## 基本用法

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

## Linger.Results 集成

与 Linger.Results 框架无缝集成，提供统一的错误处理：

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

## ProblemDetails 支持

原生支持 RFC 7807 ProblemDetails 格式：

```csharp
// 自动解析 ProblemDetails 响应
var result = await _httpClient.CallApi<User>("api/users", HttpMethodEnum.Post, invalidUser);
if (!result.IsSuccess)
{
    Console.WriteLine($"错误: {result.ErrorMsg}");
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
