# Linger.HttpClient

### 简介
Linger.HttpClient 是标准.NET HttpClient的轻量级封装，提供了额外的功能和更友好的API。

### 特性
- 简单、流畅的HTTP操作API
- 自动重试支持
- 请求/响应拦截
- 便捷的身份验证处理
- 可自定义超时设置
- 支持文化感知请求

### 支持的.NET版本
该库支持使用.NET Framework 4.6.2+或.NET Standard 2.0+的.NET应用程序。

### 使用示例

```csharp
// 创建客户端
var client = new BaseHttpClient("https://api.example.com");

// 添加授权令牌
client.SetToken("your-auth-token");

// 配置选项
client.Options.EnableRetry = true;
client.Options.MaxRetryCount = 3;

// 添加请求头
client.AddHeader("User-Agent", "Linger HttpClient");

// 发送GET请求
var response = await client.GetAsync<YourResponseType>("api/resources");

// 发送POST请求
var postResponse = await client.PostAsync<YourResponseType>(
    "api/resources", 
    new { Name = "新资源", Description = "一些描述" }
);

// 处理响应
if (response.StatusCode == System.Net.HttpStatusCode.OK)
{
    var data = response.Data;
    // 处理数据
}
```
