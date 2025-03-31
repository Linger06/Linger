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

## 最新改进

### 1. 拦截器全面集成

FlurlHttpClient现在与StandardHttpClient一样，完全支持请求和响应拦截器：

```csharp
// 创建HttpRequestMessage用于应用拦截器
var request = new HttpRequestMessage(httpMethod, requestUri);

// 应用请求拦截器
request = await ApplyInterceptorsToRequestAsync(request);

// 从拦截器处理过的请求更新Flurl请求
if (request.RequestUri != requestUri)
{
    flurlRequest = new FlurlRequest(request.RequestUri);
}

// 执行Flurl请求
var flurlResponse = await ExecuteFlurlRequest(flurlRequest, method, content, token);

// 应用响应拦截器
var res = flurlResponse.ResponseMessage;
if (res != null)
{
    res = await ApplyInterceptorsToResponseAsync(res);
}
```

### 2. 令牌处理改进

SetToken方法已增强，确保令牌正确应用：

```csharp
public override void SetToken(string token)
{
    // 修正：不使用忽略结果的语法，确保令牌正确应用
    if (string.IsNullOrEmpty(token))
    {
        _flurlClient.Headers.Remove("Authorization");
    }
    else
    {
        _flurlClient.WithOAuthBearerToken(token);
    }
}
```

### 3. 与StandardHttpClient行为一致性

文化信息处理位置与StandardHttpClient相同：

```csharp
// 统一添加文化信息 - 将位置调整为与StandardHttpClient相同
url = url.AppendQuery("culture=" + Thread.CurrentThread.CurrentUICulture.Name);
```

### 4. 底层客户端访问支持

添加了GetFlurlClient方法，便于访问底层Flurl功能：

```csharp
/// <summary>
/// 获取底层Flurl客户端用于高级操作
/// </summary>
public IFlurlClient GetFlurlClient()
{
    return _flurlClient;
}
```

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
   // 使用工厂获取所有标准功能（包括拦截器）
   services.AddSingleton<IHttpClientFactory, FlurlHttpClientFactory>();
   var client = factory.CreateClient("https://api.example.com", options => {
       options.EnableRetry = true;
   });
   ```

3. **按功能区域组织API调用**
   ```csharp
   // 用户相关API
   var usersApi = factory.GetOrCreateClient("users");
   
   // 产品相关API
   var productsApi = factory.GetOrCreateClient("products");
   ```

4. **确保令牌正确应用**
   ```csharp
   // 设置token（现在实现已修复）
   client.SetToken(jwtToken);
   
   // 清除token
   client.SetToken(string.Empty);
   ```

5. **结合拦截器和Flurl特性**
   ```csharp
   // 添加标准拦截器
   client.AddInterceptor(new LoggingInterceptor());
   
   // 同时利用Flurl特性
   var flurlClient = client.GetFlurlClient();
   flurlClient.BeforeCall(call => {
       // Flurl特定的前置处理
   });
   ```
