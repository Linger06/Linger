# Linger.HttpClient.Contracts

## 目录
- [概述](#概述)
- [特性](#特性)
- [安装](#安装)
- [依赖注入集成](#依赖注入集成)
  - [使用HttpClientFactory](#使用httpclientfactory)
  - [在服务类中使用](#在服务类中使用)
- [基本用法](#基本用法)
  - [GET请求](#get请求)
  - [POST请求](#post请求)
  - [文件上传](#文件上传)
- [错误处理](#错误处理)
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
- 请求/响应拦截
- 自动重试机制
- 友好的错误处理

## 安装

```bash
# 安装接口和契约
dotnet add package Linger.HttpClient.Contracts

# 安装基于标准HttpClient的实现
dotnet add package Linger.HttpClient.Standard
```

## 依赖注入集成

### 使用HttpClientFactory

使用Linger HTTP客户端的推荐方式是结合Microsoft的HttpClientFactory：

```csharp
// 在启动配置中
services.AddHttpClient<StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddTypedClient<IHttpClient>((httpClient, serviceProvider) => 
{
    var standardClient = new StandardHttpClient(httpClient);
    
    // 配置选项
    standardClient.Options.EnableRetry = true;
    standardClient.Options.MaxRetryCount = 3;
    
    // 获取其他服务
    var appState = serviceProvider.GetRequiredService<AppState>();
    if (!string.IsNullOrEmpty(appState.Token))
    {
        standardClient.SetToken(appState.Token);
    }
    
    return standardClient;
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

## 最佳实践

1. **使用HttpClientFactory管理生命周期**
2. **设置合理的超时值**
3. **实现适当的错误处理**
4. **为API响应使用强类型模型**

更多信息，请参阅[Linger.HttpClient.Standard文档](../Linger.HttpClient.Standard/README.zh-CN.md)。
