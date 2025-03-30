# Linger.HttpClient.Flurl

## 简介
Linger.HttpClient.Flurl 基于流行的 Flurl.Http 库实现，提供了流畅的链式API和强大的URL构建功能。作为 Linger.HttpClient.Contracts 接口的实现，它将 Flurl 的直观性与 Linger 的标准化接口相结合。

> 🔗 此项目是 [Linger HTTP客户端生态系统](../Linger.HttpClient.Contracts/README.zh-CN.md) 的一部分。

## 核心优势

- **流畅的链式API**：富有表现力的代码风格
- **动态URL构建**：内置多种URL操作方法
- **模板支持**：URL路径段中的模板插值
- **强大的请求定制**：丰富的选项和扩展
- **友好的异常处理**：详细且可读性强的错误信息

## 安装

```bash
dotnet add package Linger.HttpClient.Flurl
```

## 快速入门

```csharp
// 创建客户端
var client = new FlurlHttpClient("https://api.example.com");

// 发送请求
var response = await client.GetAsync<UserData>("api/users/1");
```

## Flurl特有功能

### 1. 流畅的URL构建

```csharp
// 获取底层Flurl客户端
var flurlClient = client.GetFlurlClient();

// 使用流畅API构建URL
var url = flurlClient.BaseUrl
    .AppendPathSegment("api")
    .AppendPathSegment("users")
    .AppendPathSegment(userId)
    .SetQueryParam("include", "profile,orders")
    .SetQueryParam("fields", new[] {"id", "name", "email"})
    .ToString();

// 输出: https://api.example.com/api/users/123?include=profile,orders&fields=id&fields=name&fields=email
```

### 2. URL模板与插值

```csharp
// 使用路径模板
var productUrl = "products/{id}/variants/{variantId}"
    .SetQueryParam("lang", "zh-CN");

// 路径替换
var finalUrl = productUrl
    .SetRouteParameter("id", 42)
    .SetRouteParameter("variantId", 101);
    
// 输出: products/42/variants/101?lang=zh-CN
```

### 3. 高级HTTP操作

```csharp
// 访问Flurl的高级功能
var flurlClient = client.GetFlurlClient();

// 配置特定请求
var response = await flurlClient
    .Request("api/special-endpoint")
    .WithHeader("X-API-Version", "2.0")
    .WithTimeout(TimeSpan.FromSeconds(60))
    .WithAutoRedirect(false)
    .AllowHttpStatus(HttpStatusCode.NotFound)
    .PostJsonAsync(new { data = "value" });
```

## 应用场景

FlurlHttpClient 特别适合以下场景：

- **RESTful API 客户端**：特别是那些需要动态构建 URL 的场景
- **需要表达性代码的项目**：代码即文档，易于理解的API调用
- **现代Web应用**：需要灵活处理各种API响应
- **快速原型开发**：流畅API加快开发速度

## 与 StandardHttpClient 对比

| 场景 | FlurlHttpClient | StandardHttpClient |
|------|----------------|------------------|
| URL构建能力 | ★★★★★ | ★★☆☆☆ |
| API流畅度 | ★★★★★ | ★★★☆☆ |
| 代码简洁性 | ★★★★★ | ★★★☆☆ |
| 性能要求高 | ★★★☆☆ | ★★★★★ |
| 资源占用少 | ★★★☆☆ | ★★★★★ |
| 学习曲线 | 中等 | 平缓 |
| 适合项目 | 现代Web应用、复杂API集成 | 企业级应用、资源受限环境 |

## 实际示例

### 构建复杂查询

```csharp
// 定义查询参数
var filters = new
{
    category = "electronics",
    priceRange = new[] { "100-500", "500-1000" },
    brand = new[] { "apple", "samsung" },
    inStock = true
};

// 使用FlurlHttpClient进行查询
var response = await client.GetAsync<List<Product>>(
    "api/products", 
    filters
);
```

### 使用Flurl特性的JWT认证

```csharp
// 获取Flurl客户端
var flurlClient = client.GetFlurlClient();

// 配置认证拦截器
flurlClient.BeforeCall(call => 
{
    if (_tokenService.IsTokenValid())
    {
        call.Request.WithOAuthBearerToken(_tokenService.GetToken());
    }
});

// 处理401响应
flurlClient.OnError(async call => 
{
    if (call.HttpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
    {
        if (await _tokenService.RefreshTokenAsync())
        {
            await call.Request
                .WithOAuthBearerToken(_tokenService.GetToken())
                .SendAsync(call.HttpRequestMessage.Method, call.CancellationToken);
        }
    }
});
```

## 最佳实践

1. **分离URL构建和HTTP调用**
   ```csharp
   // 先构建URL，后发送请求
   var url = flurlClient.BaseUrl
       .AppendPathSegments("api", "users")
       .SetQueryParams(new { page = 1, size = 10 });
       
   var response = await client.GetAsync<List<User>>(url.ToString());
   ```

2. **使用工厂创建命名客户端**
   ```csharp
   services.AddSingleton<IHttpClientFactory, FlurlHttpClientFactory>();
   ```

3. **按功能区域组织API调用**
   ```csharp
   // 用户相关API
   var usersApi = factory.GetOrCreateClient("users");
   
   // 产品相关API
   var productsApi = factory.GetOrCreateClient("products");
   ```
