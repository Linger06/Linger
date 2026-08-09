# Linger.AspNetCore.Jwt

一个用于处理 JWT 认证并可选支持刷新令牌的 C# 辅助库，聚焦“简单集成 + 可扩展 + 安全实践”。

> 从 1.x 升级？请参阅 [2.0 迁移指南](https://github.com/Linger06/Linger/blob/v2.0.0-preview.1/src/Linger/MIGRATION.zh-CN.md)。

## 目录
- [核心特性](#核心特性)
- [支持平台](#支持平台)
- [安装](#安装)
- [快速开始（最少代码）](#快速开始最少代码)
- [配置 JwtOption](#配置-jwtoption)
- [注册与集成方式](#注册与集成方式)
- [扩展 Claims](#扩展-claims)
- [启用刷新令牌](#启用刷新令牌)
- [控制器示例](#控制器示例)
- [客户端自动刷新](#客户端自动刷新)
- [刷新令牌工作流程说明](#刷新令牌工作流程说明)
- [安全最佳实践](#安全最佳实践)
- [高级功能](#高级功能)
- [故障排查](#故障排查)
- [FAQ](#faq)

## 核心特性
- ✅ 接口分离：`IJwtService` 仅颁发访问令牌；刷新逻辑通过扩展接口解耦
- ✅ 渐进式增强：按需启用刷新令牌
- ✅ 可插拔存储：内存、数据库或自定义实现
- ✅ 安全强化：支持 jti / iat、外部化密钥、最小权限原则
- ✅ 易扩展：重写 `GetClaimsAsync` 即可添加角色 / 权限 / 租户

## 支持平台
.NET 8.0+ ASP.NET Core

## 安装
```bash
dotnet add package Linger.AspNetCore.Jwt
```
> 使用客户端自动刷新示例需要额外引用：`Linger.HttpClient.Standard`、`Linger.AspNetCore.Jwt.Contracts`

## 快速开始（最少代码）
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
// 1. 绑定配置 + 注册验证方案
builder.Services.ConfigureJwt(builder.Configuration);
// 2. 基础服务
builder.Services.AddScoped<IJwtService, JwtService>();
// 3. 中间件
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
// 4. 登录端点
app.MapPost("/login", async (IJwtService jwt, LoginModel m) =>
{
    if (!UserValidator.Validate(m.Username, m.Password)) return Results.Unauthorized();
    return Results.Ok(await jwt.CreateTokenAsync(m.Username));
});
app.Run();
```
> 至此：已具备基础 JWT 能力；如需刷新支持 → 参见下文“启用刷新令牌”。

## 配置 JwtOption
```csharp
public class JwtOption
{
    public string SecurityKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiresInMinutes { get; init; } = 30;
    public int RefreshTokenExpiresInMinutes { get; init; } = 10080;
}
```
`appsettings.json`：
```json
{
  "JwtOptions": {
    "SecurityKey": "至少32字符生产密钥(用SECRET环境变量覆盖)",
    "Issuer": "your-app.com",
    "Audience": "your-api.com",
    "ExpiresInMinutes": 15,
    "RefreshTokenExpiresInMinutes": 10080
  }
}
```
环境变量示例：
```bash
# Linux / macOS
export SECRET="Prod_YourLongSecret_AtLeast32Chars"
# Windows PowerShell
$Env:SECRET = "Prod_YourLongSecret_AtLeast32Chars"
```

## 注册与集成方式
```csharp
// 简洁方式
builder.Services.ConfigureJwt(builder.Configuration);

// 自行绑定 + 多认证共存
var opt = builder.Configuration.GetGeneric<JwtOption>("JwtOptions");
ArgumentNullException.ThrowIfNull(opt);
builder.Services.AddSingleton(opt);
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddJwtBearer(opt); // 扩展

// 可选实现注入
builder.Services.AddScoped<IJwtService, JwtService>();

// 使用刷新令牌时，改为注册一个支持原子轮换的实现
builder.Services.AddScoped<IRefreshableJwtService, DbJwtService>();
builder.Services.AddScoped<IJwtService>(provider =>
    provider.GetRequiredService<IRefreshableJwtService>());
```

上述基础服务和刷新服务是二选一注册方式，不应同时注册多个 `IJwtService` 实现。`AddJwtBearer` 会为常见令牌错误保留诊断响应头，并使用 ASP.NET Core 默认的 `401` / `403` 响应，不会把内部异常信息写入响应。

## 扩展 Claims
默认：
```csharp
protected virtual Task<List<Claim>> GetClaimsAsync(
    string userId,
    CancellationToken cancellationToken) =>
    Task.FromResult(new List<Claim>{ new(ClaimTypes.Name, userId) });
```
自定义：
```csharp
public class CustomJwtService(AppDbContext db, JwtOption opt, ILogger? logger = null) : JwtService(opt, logger)
{
    protected override async Task<List<Claim>> GetClaimsAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, userId) };
        var user = await db.Users.FindAsync([userId], cancellationToken);
        foreach (var role in user.Roles.Split(','))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }
}
```

## 启用刷新令牌
继承 `JwtServiceWithRefresh`，分别实现首次保存和原子轮换。内存示例使用进程内锁保证同一个旧令牌只成功一次：
```csharp
public class MemoryCachedJwtService : JwtServiceWithRefresh
{
    private readonly IMemoryCache _cache;
    private readonly object _rotationLock = new();

    public MemoryCachedJwtService(
        JwtOption options,
        IMemoryCache cache,
        ILogger<MemoryCachedJwtService>? logger = null) : base(options, logger)
    {
        _cache = cache;
    }

    protected override Task StoreRefreshTokenAsync(
        string userId,
        JwtRefreshToken refreshToken,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _cache.Set($"RT_{userId}", refreshToken, refreshToken.ExpiryTime);

        return Task.CompletedTask;
    }

    protected override Task<bool> TryRotateRefreshTokenAsync(
        string userId,
        string presentedRefreshToken,
        JwtRefreshToken replacement,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_rotationLock)
        {
            if (!_cache.TryGetValue($"RT_{userId}", out JwtRefreshToken? current) ||
                current is null ||
                current.ExpiryTime <= DateTime.UtcNow ||
                !string.Equals(current.RefreshToken, presentedRefreshToken, StringComparison.Ordinal))
            {
                return Task.FromResult(false);
            }

            _cache.Set($"RT_{userId}", replacement, replacement.ExpiryTime);

            return Task.FromResult(true);
        }
    }
}
```

内存示例的锁只在当前进程内有效；多实例部署必须使用数据库事务或分布式缓存提供的原子比较与替换能力。

数据库实现必须使用条件更新或事务完成比较与替换，不能在 `TryRotateRefreshTokenAsync` 内重新拆成“查询后更新”：

```csharp
public class DbJwtService : JwtServiceWithRefresh
{
    private readonly IUserRepository _repo;

    public DbJwtService(
        JwtOption options,
        IUserRepository repo,
        ILogger<DbJwtService>? logger = null) : base(options, logger)
    {
        _repo = repo;
    }

    protected override Task StoreRefreshTokenAsync(
        string userId,
        JwtRefreshToken refreshToken,
        CancellationToken cancellationToken) =>
        _repo.StoreRefreshTokenAsync(userId, refreshToken, cancellationToken);

    protected override Task<bool> TryRotateRefreshTokenAsync(
        string userId,
        string presentedRefreshToken,
        JwtRefreshToken replacement,
        CancellationToken cancellationToken) =>
        _repo.TryRotateRefreshTokenAsync(
            userId,
            presentedRefreshToken,
            replacement,
            cancellationToken);
}
```

`TryRotateRefreshTokenAsync` 只有在旧令牌匹配、尚未过期且已成功替换时才返回 `true`。例如数据库可以执行带 `UserId`、旧令牌和过期时间条件的 `UPDATE`，并根据受影响行数判断是否成功。

## 控制器示例

### 推荐方式 (使用 RefreshTokenResultAsync)
```csharp
public class AuthController(IJwtService jwt, IUserService users) : ControllerBase
{
    [HttpPost("login")] 
    public async Task<IActionResult> Login(LoginModel m, CancellationToken cancellationToken)
    { 
        var id = await users.ValidateUserAsync(m.Username, m.Password); 
        if (string.IsNullOrEmpty(id)) return Unauthorized(); 
        return Ok(await jwt.CreateTokenAsync(id, cancellationToken));
    }
    
    [HttpPost("refresh")] 
    public async Task<IActionResult> Refresh(Token token, CancellationToken cancellationToken)
    { 
        var result = await jwt.RefreshTokenResultAsync(token, cancellationToken);
        if (result.Success) 
            return Ok(result.Token);
        
        return Unauthorized(result.ErrorMessage);
    }
}
```

### 备选方式 (使用异常处理)
```csharp
[HttpPost("refresh")] 
public async Task<IActionResult> Refresh(Token token, CancellationToken cancellationToken)
{ 
    if (!jwt.SupportsRefreshToken()) 
        return Unauthorized("不支持刷新令牌");
    
    try 
    {
        return Ok(await jwt.RefreshTokenAsync(token, cancellationToken));
    }
    catch (NotSupportedException)
    {
        return Unauthorized("不支持刷新令牌");
    }
    catch (SecurityTokenException ex)
    {
        _logger.LogWarning(ex, "刷新令牌验证失败");
        return Unauthorized("刷新令牌无效或已过期,请重新登录");
    }
}
```

`RefreshTokenResultAsync` 只把预期的无效令牌异常转换为失败结果。调用取消和未知的存储异常会继续向上传播。

### 客户端自动刷新

`Linger.AspNetCore.Jwt` 在服务端校验并轮换 Refresh Token，但不会直接操作桌面端或其他客户端的 `HttpClient`。客户端应保存登录或刷新端点返回的完整 `Token`，在 Access Token 即将过期时调用上面的 `refresh` 端点，并使用返回的新 Access Token 和 Refresh Token 整体替换旧值。

WinForms 直接创建 `StandardHttpClient` 时，可以通过 `DelegatingHandler` 在请求发送前完成检查和刷新。完整示例以及并发刷新、独立刷新客户端和失败后重新登录的处理方式参见 [Linger.HttpClient.Standard 中文 README](../Linger.HttpClient.Standard/README.zh-CN.md)。


---

## 刷新令牌工作流程说明
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

## 安全最佳实践
- 使用环境变量 SECRET 覆盖配置密钥（长度 ≥ 32）
- 访问令牌短期 + 刷新令牌较长期，及时撤销
- 刷新令牌持久化可哈希（防泄露滥用）
- 记录 jti / iat 便于吊销与审计
- 失败刷新立即清理本地状态

## 高级功能
- jti / iat 声明 → 审计与防重放
- 自定义 Claims（角色 / 权限 / 租户 / 策略标签）
- 多存储后端：内存 / 数据库 / 分布式缓存
- 组合 Resilience：重试 + 刷新 + 断路器 + 超时

## 故障排查
| 症状 | 可能原因 | 解决建议 |
|------|----------|----------|
| 登录成功后立即 401 | 时间不同步 / 签名失败 | 校准时间；统一 SECRET |
| 刷新未触发 | 未注册刷新实现 | 检查 `IRefreshableJwtService` 与 `IJwtService` 的 DI 映射 |
| 刷新风暴 | 并发 401 竞态 | 使用信号量/单次刷新管控 |
| 刷新成功但仍旧老令牌 | 客户端未更新头部 | 确认事件订阅与 SetToken 调用 |
| Invalid signature | 多实例密钥不一致 | 配置中心或环境变量统一 |

## FAQ
**Q:** 必须启用刷新令牌吗？  **A:** 否，可仅短期令牌。

**Q:** 如何吊销用户所有令牌？  **A:** 记录 jti，加入黑名单；删除刷新令牌记录。

**Q:** 如何支持多租户？  **A:** 添加租户 Claim，并在授权策略中校验。

**Q:** 可以扩展返回模型吗？  **A:** 可在自定义实现中封装 DTO。

**Q:** 如何防止刷新令牌被窃取？  **A:** 服务端存储哈希，客户端仅持有随机值，启用 HTTPS 与最小持久化。
