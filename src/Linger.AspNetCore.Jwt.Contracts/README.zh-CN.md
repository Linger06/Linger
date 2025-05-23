# Linger.AspNetCore.Jwt.Contracts

> 📝 *查看此文档: [English](./README.md) | [中文](./README.zh-CN.md)*

ASP.NET Core 应用中 JWT（JSON Web Token）认证和授权的核心接口和抽象。

## 功能特点

- 标准化的 JWT 令牌生成和验证接口
- 支持刷新令牌功能
- 契约与实现的清晰分离
- JWT 配置选项模型
- 服务注册扩展方法

## 支持的框架

- .NET 8.0+
- .NET 9.0+

## 安装

```shell
dotnet add package Linger.AspNetCore.Jwt.Contracts
```

## 使用方法

### 配置

在 `appsettings.json` 中定义 JWT 设置：

```json
{
  "Jwt": {
    "Issuer": "https://api.example.com",
    "Audience": "https://example.com",
    "SecurityKey": "你的长而安全的密钥，至少32字节",
    "Expires": 30,
    "RefreshTokenExpires": 1440
  }
}
```

### 依赖注入

```csharp
// Program.cs 或 Startup.cs
services.Configure<JwtOption>(Configuration.GetSection("Jwt"));

// 注册 JWT 服务实现（来自 Linger.AspNetCore.Jwt 包）
services.AddJwtService();
```

### 基本用法

```csharp
public class AuthService
{
    private readonly IJwtService _jwtService;
    
    public AuthService(IJwtService jwtService)
    {
        _jwtService = jwtService;
    }
    
    public async Task<Token> LoginAsync(string username, string password)
    {
        // 验证凭证
        if (!await ValidateCredentialsAsync(username, password))
        {
            throw new AuthenticationException("无效的凭证");
        }
        
        // 获取用户 ID
        var userId = await GetUserIdAsync(username);
        
        // 生成 JWT 令牌
        return await _jwtService.CreateTokenAsync(userId);
    }
}
```

### 使用刷新令牌

```csharp
public class AuthController : ControllerBase
{
    private readonly IRefreshableJwtService _jwtService;
    
    public AuthController(IRefreshableJwtService jwtService)
    {
        _jwtService = jwtService;
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var token = new Token(request.AccessToken, request.RefreshToken);
            var newToken = await _jwtService.RefreshTokenAsync(token);
            
            return Ok(new
            {
                accessToken = newToken.AccessToken,
                refreshToken = newToken.RefreshToken
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}
```

## 核心接口

### IJwtService

```csharp
public interface IJwtService
{
    /// <summary>
    /// 创建 JWT 令牌
    /// </summary>
    /// <param name="userId">用户标识</param>
    /// <returns>包含访问令牌的 Token 对象</returns>
    Task<Token> CreateTokenAsync(string userId);
}
```

### IRefreshableJwtService

```csharp
public interface IRefreshableJwtService : IJwtService
{
    /// <summary>
    /// 刷新 JWT 令牌
    /// </summary>
    /// <param name="token">包含访问令牌和刷新令牌的 Token 对象</param>
    /// <returns>新的 Token 对象</returns>
    Task<Token> RefreshTokenAsync(Token token);
}
```

## 相关包

- [Linger.AspNetCore.Jwt](../Linger.AspNetCore.Jwt/) - 这些契约的实现
