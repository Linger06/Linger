# Linger.HttpClient.Flurl

### 简介
Linger.HttpClient.Flurl 是使用流行的 Flurl.Http 库实现的 Linger.HttpClient.Contracts 接口。它结合了 Flurl 的强大功能和 Linger 的标准化接口。

### 特性
- 包含 Linger.HttpClient.Contracts 的所有功能
- 利用 Flurl.Http 简化 URL 构建和操作
- 现代 C# 开发的流畅 API
- 增强的错误处理
- 自动处理查询参数

### 支持的.NET版本
该库支持使用.NET Framework 4.6.2+或.NET Standard 2.0+的.NET应用程序。

### 使用示例

```csharp
// 创建客户端
var client = new FlurlHttpClient("https://api.example.com");

// 添加授权令牌
client.SetToken("your-auth-token");

// 配置选项
client.Options.EnableRetry = true;
client.Options.MaxRetryCount = 3;

// 发送GET请求
var response = await client.GetAsync<YourResponseType>("api/resources");

// 发送带查询参数的POST请求
var postResponse = await client.PostAsync<YourResponseType>(
    "api/resources", 
    new { Name = "新资源" }, 
    new { category = "重要" }
);

// 上传文件
var fileBytes = File.ReadAllBytes("example.pdf");
var uploadResponse = await client.CallApi<UploadResult>(
    "api/upload",
    HttpMethodEnum.Post,
    new Dictionary<string, string> { { "description", "示例文件" } },
    fileBytes,
    "example.pdf"
);
```
