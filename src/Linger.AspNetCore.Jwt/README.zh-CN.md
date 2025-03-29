# Linger.AspNetCore.Jwt

一个用于处理JWT令牌认证的C#辅助库，支持灵活的刷新令牌实现。

## 支持的 .NET 版本

本库支持在.NET 8.0+上运行的ASP.NET Core应用程序。

## 设计特点

本库采用扩展方法实现方式处理刷新令牌功能，主要优势如下：

- **接口隔离**：核心`IJwtService`接口保持简洁，只包含基本令牌生成功能
- **功能扩展**：通过`IRefreshableJwtService`接口和扩展方法提供刷新令牌功能
- **灵活使用**：可以根据不同场景选择使用基本JWT认证或带刷新令牌的认证
- **兼容性好**：不破坏现有代码结构，易于集成到已有项目

## 使用指南

### 1. 配置JWT选项

`JwtOption`类提供了JWT令牌生成和验证的配置设置：

```csharp
public class JwtOption
{
    // JWT签名密钥（在生产环境中应存储在安全位置）
    public string SecurityKey { get; set; } = "this is my custom Secret key for authentication";
    
    // 令牌颁发者（通常是您的应用程序或认证服务器域名）
    public string Issuer { get; set; } = "Linger.com";
    
    // 令牌受众（通常是您的API域名）
    public string Audience { get; set; } = "Linger.com";
    
    // 访问令牌过期时间，单位分钟（默认：30分钟）
    public int Expires { get; set; } = 30;
    
    // 刷新令牌过期时间，单位分钟（默认：60分钟）
    public int RefreshTokenExpires { get; set; } = 60;
    
    // 启用/禁用刷新令牌功能的标志
    public bool EnableRefreshToken { get; set; } = true;
}
```

在`appsettings.json`文件中配置JWT选项：

```json
{
  "JwtOptions": {
    "SecurityKey": "您的安全密钥，至少256位",
    "Issuer": "your-app.com",
    "Audience": "your-api.com",
    "Expires": 15,
    "RefreshTokenExpires": 10080, // 7天
    "EnableRefreshToken": true
  }
}
```

### 2. 实现您的自定义JWT服务

由于`JwtServiceWithRefresh`是一个抽象类，您需要继承它并实现抽象方法来处理刷新令牌的存储和获取：

```csharp
// 使用内存缓存的实现示例
public class MemoryCachedJwtService : JwtServiceWithRefresh
{
    private readonly IMemoryCache _cache;

    public MemoryCachedJwtService(JwtOption jwtOptions, IMemoryCache cache, ILogger<MemoryCachedJwtService>? logger = null) 
        : base(jwtOptions, logger)
    {
        _cache = cache;
    }

    protected override Task HandleRefreshToken(string userId, JwtRefreshToken refreshToken)
    {
        // 将刷新令牌存储在内存缓存中
        _cache.Set($"RT_{userId}", refreshToken, TimeSpan.FromMinutes(_jwtOptions.RefreshTokenExpires));
        return Task.CompletedTask;
    }

    protected override Task<JwtRefreshToken> GetExistRefreshTokenAsync(string userId)
    {
        // 从内存缓存中获取刷新令牌
        if (_cache.TryGetValue($"RT_{userId}", out JwtRefreshToken? token) && token != null)
        {
            return Task.FromResult(token);
        }
        
        throw new Exception("刷新令牌未找到或已过期");
    }
}

// 使用数据库的实现示例
public class DbJwtService : JwtServiceWithRefresh
{
    private readonly IUserRepository _userRepository;

    public DbJwtService(JwtOption jwtOptions, IUserRepository userRepository, ILogger<DbJwtService>? logger = null) 
        : base(jwtOptions, logger)
    {
        _userRepository = userRepository;
    }

    protected override async Task HandleRefreshToken(string userId, JwtRefreshToken refreshToken)
    {
        // 将刷新令牌存储在数据库中
        await _userRepository.UpdateRefreshTokenAsync(userId, refreshToken.RefreshToken, refreshToken.ExpiryTime);
    }

    protected override async Task<JwtRefreshToken> GetExistRefreshTokenAsync(string userId)
    {
        // 从数据库中获取刷新令牌
        var user = await _userRepository.GetUserAsync(userId);
        if (user != null && !string.IsNullOrEmpty(user.RefreshToken))
        {
            return new JwtRefreshToken
            {
                RefreshToken = user.RefreshToken,
                ExpiryTime = user.RefreshTokenExpiryTime
            };
        }
        
        throw new Exception("刷新令牌未找到或已过期");
    }
}
```

### 3. 注册服务

在`Program.cs`或`Startup.cs`中注册JWT服务：

```csharp
// 从appsettings.json添加JWT配置
services.Configure<JwtOption>(Configuration.GetSection("JwtOptions"));

// 注册为单例以确保配置一致性
services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtOption>>().Value);

// 注册JWT服务 - 选择以下选项之一：

// 选项1：基本服务（无刷新令牌）
services.AddScoped<IJwtService, JwtService>();

// 选项2：支持刷新令牌的服务（使用内存缓存）
services.AddScoped<IRefreshableJwtService, MemoryCachedJwtService>();
// 同时注册为基础接口，允许通过IJwtService访问
services.AddScoped<IJwtService>(sp => sp.GetRequiredService<IRefreshableJwtService>());

// 选项3：支持刷新令牌的服务（使用数据库）
services.AddScoped<IRefreshableJwtService, DbJwtService>();
services.AddScoped<IJwtService>(sp => sp.GetRequiredService<IRefreshableJwtService>());
```

### 4. 在控制器中使用

在控制器中实现认证：

```csharp
// 方式1：只使用基本功能时
public class BasicAuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    
    public BasicAuthController(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        // 验证用户凭据...
        string userId = await _userService.ValidateUserAsync(model.Username, model.Password);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        
        var token = await _jwtService.CreateTokenAsync(userId);
        return Ok(token);
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] Token token)
    {
        // 利用扩展方法检查是否支持刷新功能
        if (_jwtService.SupportsRefreshToken())
        {
            var (success, newToken) = await _jwtService.TryRefreshTokenAsync(token);
            if (success)
            {
                return Ok(newToken);
            }
        }
        
        return Unauthorized("请重新登录");
    }
}

// 方式2：直接使用具有刷新功能的接口
public class RefreshableAuthController : ControllerBase
{
    private readonly IRefreshableJwtService _jwtService;
    
    public RefreshableAuthController(IRefreshableJwtService jwtService)
    {
        _jwtService = jwtService;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        // 验证用户凭据...
        string userId = await _userService.ValidateUserAsync(model.Username, model.Password);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        
        var token = await _jwtService.CreateTokenAsync(userId);
        return Ok(token);
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] Token token)
    {
        try 
        {
            // 直接调用刷新方法，无需检查支持与否
            var newToken = await _jwtService.RefreshTokenAsync(token);
            return Ok(newToken);
        }
        catch (Exception ex)
        {
            return Unauthorized($"刷新令牌失败: {ex.Message}");
        }
    }
}
```

## 客户端自动令牌刷新

除了服务端实现令牌刷新外，客户端也需要相应的机制来处理令牌过期和刷新。以下介绍如何使用 `TokenRefreshInterceptor` 实现客户端的自动令牌刷新。

### 安装必要的包

要在客户端使用自动令牌刷新功能，需要安装以下NuGet包：

```bash
# 安装HTTP客户端接口和契约
dotnet add package Linger.HttpClient.Contracts

# 安装HTTP客户端实现（选择一个）
dotnet add package Linger.HttpClient
# 或
dotnet add package Linger.HttpClient.Flurl
```

通过Package Manager Console安装：

```powershell
Install-Package Linger.HttpClient.Contracts
Install-Package Linger.HttpClient
```

### 令牌服务接口

首先定义令牌服务接口，用于管理和刷新令牌：

```csharp
public interface ITokenService
{
    /// <summary>
    /// 获取当前令牌
    /// </summary>
    Task<string> GetCurrentTokenAsync();
    
    /// <summary>
    /// 刷新令牌
    /// </summary>
    Task<bool> RefreshTokenAsync();
    
    /// <summary>
    /// 使令牌失效
    /// </summary>
    Task<bool> InvalidateTokenAsync();
    
    /// <summary>
    /// 令牌是否有效
    /// </summary>
    bool IsTokenValid();
}
```

### 令牌服务实现

下面是一个基本的`ITokenService`实现示例，用于管理JWT令牌和与授权服务器交互：

```csharp
public class TokenService : ITokenService
{
    private readonly IHttpClient _httpClient;
    private string _currentToken = string.Empty;
    private string _refreshToken = string.Empty;
    private DateTime _expiresAt = DateTime.MinValue;
    
    public TokenService(IHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <summary>
    /// 获取当前令牌
    /// </summary>
    public async Task<string> GetCurrentTokenAsync()
    {
        if (!IsTokenValid())
        {
            await RefreshTokenAsync();
        }
        
        return _currentToken;
    }
    
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(_refreshToken))
        {
            // 首次登录或刷新令牌为空时，执行完整登录
            return await LoginAsync();
        }
        
        try
        {
            // 调用刷新令牌API
            var content = new StringContent(
                JsonSerializer.Serialize(new { refresh_token = _refreshToken }),
                Encoding.UTF8,
                "application/json");
                
            var response = await _httpClient.CallApi<TokenResponse>(
                "auth/refresh", 
                HttpMethodEnum.Post, 
                content);
                
            if (response.IsSuccess && response.Data != null)
            {
                // 更新存储的令牌和过期时间
                _currentToken = response.Data.AccessToken;
                _refreshToken = response.Data.RefreshToken;
                _expiresAt = DateTime.UtcNow.AddSeconds(response.Data.ExpiresIn);
                return true;
            }
            
            // 刷新失败，尝试重新登录
            return await LoginAsync();
        }
        catch
        {
            // 发生异常时尝试重新登录
            return await LoginAsync();
        }
    }
    
    /// <summary>
    /// 使令牌失效
    /// </summary>
    public async Task<bool> InvalidateTokenAsync()
    {
        try
        {
            // 可选：调用服务器的登出接口使令牌在服务端失效
            if (!string.IsNullOrEmpty(_currentToken))
            {
                await _httpClient.CallApi<object>("auth/logout", HttpMethodEnum.Post);
            }
        }
        catch
        {
            // 即使服务器调用失败，也继续清除本地令牌
        }
        
        // 清除本地存储的令牌
        _currentToken = string.Empty;
        _refreshToken = string.Empty;
        _expiresAt = DateTime.MinValue;
        
        return true;
    }
    
    /// <summary>
    /// 令牌是否有效
    /// </summary>
    public bool IsTokenValid()
    {
        // 检查是否有令牌且未过期（提前5分钟视为过期，避免临界点问题）
        return !string.IsNullOrEmpty(_currentToken) && DateTime.UtcNow < _expiresAt.AddMinutes(-5);
    }
    
    /// <summary>
    /// 登录获取令牌
    /// </summary>
    private async Task<bool> LoginAsync()
    {
        try
        {
            // 这里应该从安全存储中获取凭据或提示用户输入
            var credentials = await GetUserCredentialsAsync();
            
            var content = new StringContent(
                JsonSerializer.Serialize(credentials),
                Encoding.UTF8,
                "application/json");
                
            var response = await _httpClient.CallApi<TokenResponse>(
                "auth/login", 
                HttpMethodEnum.Post, 
                content);
                
            if (response.IsSuccess && response.Data != null)
            {
                _currentToken = response.Data.AccessToken;
                _refreshToken = response.Data.RefreshToken;
                _expiresAt = DateTime.UtcNow.AddSeconds(response.Data.ExpiresIn);
                
                // 保存令牌到持久化存储（视情况而定）
                await SaveTokensToStorageAsync();
                
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 获取用户凭据
    /// </summary>
    private Task<UserCredentials> GetUserCredentialsAsync()
    {
        // 实际应用中，可能从安全存储中获取，或触发用户输入事件
        return Task.FromResult(new UserCredentials 
        { 
            Username = "用户名", 
            Password = "密码" 
        });
    }
    
    /// <summary>
    /// 将令牌保存到持久化存储
    /// </summary>
    private Task SaveTokensToStorageAsync()
    {
        // 在实际应用中，可能使用安全存储保存令牌
        // 例如：移动应用中的钥匙串、桌面应用中的加密存储等
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// 令牌响应模型
    /// </summary>
    private class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
    
    /// <summary>
    /// 用户凭据模型
    /// </summary>
    private class UserCredentials
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
```

在实际应用中，您可能需要添加更多功能，如：

1. 使用安全存储（如ASP.NET Core的数据保护API）来保存刷新令牌
2. 添加事件回调，当令牌刷新失败需要用户重新登录时通知UI
3. 添加重试逻辑，处理网络错误
4. 在分布式应用中使用共享缓存存储令牌

### 实现令牌刷新拦截器

使用 `IHttpClientInterceptor` 接口实现一个令牌刷新拦截器，自动处理401响应：

```csharp
public class TokenRefreshInterceptor : IHttpClientInterceptor
{
    private readonly ITokenService _tokenService;
    private readonly IHttpClient _httpClient;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public TokenRefreshInterceptor(ITokenService tokenService, IHttpClient httpClient)
    {
        _tokenService = tokenService;
        _httpClient = httpClient;
    }
    
    public async Task<HttpRequestMessage> OnRequestAsync(HttpRequestMessage request)
    {
        // 确保请求带有最新的令牌
        var token = await _tokenService.GetCurrentTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        return request;
    }
    
    public async Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response)
    {
        // 如果响应是401 Unauthorized，尝试刷新令牌并重试请求
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // 使用信号量防止多个请求同时尝试刷新令牌
            await _semaphore.WaitAsync();
            try
            {
                // 再次检查令牌，因为可能在等待信号量时已经被其他请求刷新了
                var isRefreshed = await _tokenService.RefreshTokenAsync();
                if (isRefreshed)
                {
                    // 获取原始请求
                    var originalRequest = response.RequestMessage;
                    if (originalRequest != null)
                    {
                        // 克隆请求
                        var newRequest = new HttpRequestMessage(originalRequest.Method, originalRequest.RequestUri)
                        {
                            Content = originalRequest.Content,
                            Version = originalRequest.Version
                        };
                        
                        // 复制请求头
                        foreach (var header in originalRequest.Headers)
                        {
                            newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                        
                        // 添加新令牌
                        var token = await _tokenService.GetCurrentTokenAsync();
                        newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        
                        // 重新发送请求
                        var method = ConvertMethod(newRequest.Method);
                        return await _httpClient.CallApi<HttpResponseMessage>(
                            newRequest.RequestUri.ToString(), 
                            method, 
                            newRequest.Content).GetAwaiter().GetResult().Data;
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        return response;
    }
    
    private static HttpMethodEnum ConvertMethod(HttpMethod method)
    {
        if (method == HttpMethod.Get) return HttpMethodEnum.Get;
        if (method == HttpMethod.Post) return HttpMethodEnum.Post;
        if (method == HttpMethod.Put) return HttpMethodEnum.Put;
        if (method == HttpMethod.Delete) return HttpMethodEnum.Delete;
        
        // 默认返回GET
        return HttpMethodEnum.Get;
    }
}
```

### 注册和使用

在依赖注入容器中注册令牌服务和拦截器：

```csharp
// 注册令牌服务
services.AddScoped<ITokenService, TokenService>();

// 注册HTTP客户端并配置拦截器
services.AddScoped<IHttpClient>(provider => 
{
    var client = new BaseHttpClient("https://api.example.com");
    var tokenService = provider.GetRequiredService<ITokenService>();
    
    // 添加令牌刷新拦截器
    client.AddInterceptor(new TokenRefreshInterceptor(tokenService, client));
    
    return client;
});
```

### 工作流程

1. 每次发出HTTP请求时，拦截器会检查并添加最新的JWT令牌
2. 如果接收到401 Unauthorized响应，拦截器会：
   - 自动尝试刷新令牌
   - 使用新令牌重新发送原始请求
   - 返回新响应，而不是401错误
3. 应用代码无需关心令牌刷新逻辑，可以专注于业务功能

### 优势

- **透明处理**：应用代码无需显式处理令牌过期
- **防止请求风暴**：使用信号量确保多个并发请求只触发一次令牌刷新
- **自动重试**：无缝重试因令牌过期而失败的请求
- **与服务端协同**：与服务端的JWT刷新机制完美配合

通过这种方式，您可以构建一个无缝的认证体验，用户不会因为令牌过期而被中断，同时保持系统的安全性。

## 刷新令牌原理

### 什么是刷新令牌？

刷新令牌是可用于获取新访问令牌的凭据。当访问令牌过期时，我们可以使用刷新令牌从身份验证组件获取新的访问令牌。

特点比较：
- **访问令牌(Access Token)**：过期时间短（通常几分钟），保存在客户端
- **刷新令牌(Refresh Token)**：过期时间长（通常几天），保存在服务器数据库

### 令牌使用流程

![刷新令牌流程](refresh-token-flow.png "刷新令牌流程")

1. 客户端通过提供凭据（如用户名密码）进行身份验证
2. 服务器验证成功后颁发访问令牌和刷新令牌
3. 客户端使用访问令牌请求受保护的资源
4. 服务器验证访问令牌并提供资源
5. 重复步骤3-4直到访问令牌过期
6. 访问令牌过期后，客户端使用刷新令牌请求新的令牌
7. 服务器验证刷新令牌并颁发新的访问令牌和刷新令牌
8. 重复步骤3-7直到刷新令牌过期
9. 刷新令牌过期后，客户端需要重新进行完整的身份验证（步骤1）

### 为什么需要刷新令牌？

那么，为什么我们既需要访问令牌又需要刷新令牌呢？我们为什么不为访问令牌设置一个较长的到期日期，例如一个月或一年？因为，如果我们这样做并且有人设法获得我们的访问令牌，即使我们更改了密码，他们也可以长时间使用它！

刷新令牌的想法是，我们可以使访问令牌的生存期很短，这样，即使它被破坏，攻击者也只能在较短的时间内获得访问权限。 使用基于刷新令牌的流，身份验证服务器会发出一次性使用的刷新令牌以及访问令牌。该应用程序安全地存储刷新令牌。

每次应用向服务器发送请求时，它都会在 Authorization 标头中发送访问令牌，服务器可以识别使用它的应用。一旦访问令牌过期，服务器将发送令牌过期的响应。应用收到令牌过期响应后，会发送过期的访问令牌和刷新令牌，以获取新的访问令牌和刷新令牌。 

如果出现问题，刷新令牌可以被撤销，这意味着当应用尝试使用它来获取新的访问令牌时，该请求将被拒绝，用户必须再次输入凭据并进行身份验证。

因此，刷新令牌有助于顺利进行身份验证工作流，而无需用户频繁提交其凭据，同时又不会影响应用程序的安全性。