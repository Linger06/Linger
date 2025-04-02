# Linger.HttpClient.Standard

## 简介

Linger.HttpClient.Standard 是基于标准 .NET HttpClient 的实现，提供了符合 Linger.HttpClient.Contracts 接口的轻量级封装。本项目专注于提供稳定、高效、符合.NET风格的 HTTP 通信解决方案。

## 核心优势

- **轻量级设计**：最小依赖，运行时开销低
- **.NET集成**：与HttpClientFactory和依赖注入无缝协作
- **高性能**：针对.NET环境性能优化
- **简易配置**：使用熟悉的.NET模式简单设置

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
client.Options.EnableRetry = true;
client.AddHeader("User-Agent", "Linger.Client");
```

### 使用HttpClientFactory

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
    
    return standardClient;
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
```

## 最佳实践

### 配置

```csharp
// 生产环境推荐设置
client.Options.DefaultTimeout = 15; // 15秒超时
client.Options.EnableRetry = true;
client.Options.MaxRetryCount = 3;
client.Options.RetryInterval = 1000; // 重试间隔1秒
```

### 错误处理

```csharp
try
{
    var response = await client.CallApi<UserData>("api/users/1");
    
    if (response.IsSuccess)
    {
        // 处理数据
    }
    else
    {
        // 处理API错误
        Console.WriteLine($"API错误: {response.ErrorMsg}");
    }
}
catch (Exception ex)
{
    // 处理网络或其他异常
    Console.WriteLine($"请求失败: {ex.Message}");
}
```

### 资源管理

当直接使用StandardHttpClient（不通过HttpClientFactory）时，完成后正确处理它：

```csharp
using (var httpClient = new System.Net.Http.HttpClient())
{
    var client = new StandardHttpClient(httpClient);
    // 使用客户端...
}
```

当使用HttpClientFactory时，处理会自动完成。
