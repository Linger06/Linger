# Linger.AspNetCore.Jwt

一个用于处理JWT令牌认证的C#辅助库，支持灵活的刷新令牌实现。

## 支持的 .NET 版本

本库支持在.NET 8.0+上运行的ASP.NET Core应用程序。

## 设计特点

本库采用扩展方法实现方式处理刷新令牌功能，主要优势如下：

- **接口隔离**：核心`IJwtService`接口保持简洁，只包含基本令牌生成功能
- **功能扩展**：通过标记接口`ISupportsRefreshToken`和扩展方法提供刷新令牌功能
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

// 选项2：使用内存缓存的刷新令牌服务
services.AddScoped<IJwtService, MemoryCachedJwtService>();

// 选项3：使用数据库的刷新令牌服务
services.AddScoped<IJwtService, DbJwtService>();
```

### 4. 在控制器中使用

在控制器中实现认证：

```csharp
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    
    public AuthController(IJwtService jwtService)
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
```

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