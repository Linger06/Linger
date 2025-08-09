# Linger.AspNetCore.Jwt

> 📝 *查看此文档：[English](./README.md) | [中文](./README.zh-CN.md)*

一个用于处理 JWT 认证并可选支持刷新令牌的 C# 辅助库，聚焦“简单集成 + 可扩展 + 安全实践”。

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
- [客户端自动刷新（概览）](#客户端自动刷新概览)
- [刷新令牌工作流程说明](#刷新令牌工作流程说明)
- [安全最佳实践](#安全最佳实践)
- [高级功能](#高级功能)
- [故障排查](#故障排查)
- [FAQ](#faq)

## 核心特性
- ✅ 接口分离：`IJwtService` 仅颁发访问令牌；刷新逻辑通过扩展接口解耦
- ✅ 渐进式增强：按需启用刷新令牌 / 自动刷新
- ✅ 可插拔存储：内存、数据库或自定义实现
- ✅ Resilience 支持：基于 `Microsoft.Extensions.Http.Resilience` 的并发安全刷新
- ✅ 安全强化：支持 jti / iat、外部化密钥、最小权限原则
- ✅ 易扩展：重写 `GetClaimsAsync` 即可添加角色 / 权限 / 租户

## 支持平台
- .NET 8.0+ ASP.NET Core

## 安装
```bash
dotnet add package Linger.AspNetCore.Jwt
```
> 客户端自动刷新需要额外：`Linger.HttpClient.Contracts`、`Linger.HttpClient.Standard`、`Microsoft.Extensions.Http.Resilience`

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
    public string SecurityKey { get; set; } = "this is my custom Secret key for authentication"; // 生产使用环境变量覆盖
    public string Issuer { get; set; } = "Linger.com";
    public string Audience { get; set; } = "Linger.com";
    public int Expires { get; set; } = 30;               // 访问令牌有效期(分钟)
    public int RefreshTokenExpires { get; set; } = 60;    // 刷新令牌有效期(分钟)
    public bool EnableRefreshToken { get; set; } = true;  // 是否启用刷新支持
}
```
`appsettings.json`：
```json
{
  "JwtOptions": {
    "SecurityKey": "至少32字符生产密钥(用SECRET环境变量覆盖)",
    "Issuer": "your-app.com",
    "Audience": "your-api.com",
    "Expires": 15,
    "RefreshTokenExpires": 10080,
    "EnableRefreshToken": true
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
// 简洁方式uilder.Services.ConfigureJwt(builder.Configuration);

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
builder.Services.AddScoped<IJwtService, CustomJwtServices>();
builder.Services.AddScoped<IRefreshableJwtService, MemoryCachedJwtService>();
builder.Services.AddScoped<IJwtService>(sp => sp.GetRequiredService<IRefreshableJwtService>());
builder.Services.AddScoped<IRefreshableJwtService, DbJwtService>();
```
## 扩展 Claims
默认：
```csharp
protected virtual Task<List<Claim>> GetClaimsAsync(string userId) =>
    Task.FromResult(new List<Claim>{ new(ClaimTypes.Name, userId) });
```
自定义：
```csharp
public class CustomJwtServices(CrudAppContext db, JwtOption opt, ILogger? logger = null) : JwtService(opt, logger)
{
    protected override async Task<List<Claim>> GetClaimsAsync(string userId)
    {
        var claims = new List<Claim>{ new(ClaimTypes.Name, userId) };
        var user = await db.Users.FindAsync(userId);
        foreach (var role in user.Roles.Split(','))
            claims.Add(new Claim(ClaimTypes.Role, role));
        return claims;
    }
}
```

## 启用刷新令牌
继承抽象 `JwtServiceWithRefresh`并实现存储：
```csharp
public class MemoryCachedJwtService : JwtServiceWithRefresh
{
    private readonly IMemoryCache _cache;
    public MemoryCachedJwtService(JwtOption opt, IMemoryCache cache, ILogger<MemoryCachedJwtService>? logger = null) : base(opt, logger) => _cache = cache;
    protected override Task HandleRefreshToken(string userId, JwtRefreshToken token)
    {
        _cache.Set($"RT_{userId}", token, TimeSpan.FromMinutes(_jwtOptions.RefreshTokenExpires));
        return Task.CompletedTask;
    }
    protected override Task<JwtRefreshToken> GetExistRefreshTokenAsync(string userId)
    {
        if (_cache.TryGetValue($"RT_{userId}", out JwtRefreshToken? token) && token is not null)
            return Task.FromResult(token);
        throw new Exception("刷新令牌未找到或已过期");
    }
}
```
数据库示例（节选）：
```csharp
public class DbJwtService : JwtServiceWithRefresh
{
    private readonly IUserRepository _repo;
    public DbJwtService(JwtOption opt, IUserRepository repo, ILogger<DbJwtService>? logger = null) : base(opt, logger) => _repo = repo;
    protected override Task HandleRefreshToken(string userId, JwtRefreshToken token)
        => _repo.UpdateRefreshTokenAsync(userId, token.RefreshToken, token.ExpiryTime);
    protected override async Task<JwtRefreshToken> GetExistRefreshTokenAsync(string userId)
    {
        var user = await _repo.GetUserAsync(userId);
        if (user is not null && !string.IsNullOrEmpty(user.RefreshToken))
            return new JwtRefreshToken { RefreshToken = user.RefreshToken, ExpiryTime = user.RefreshTokenExpiryTime };
        throw new Exception("刷新令牌未找到或已过期");
    }
}
```

## 控制器示例
```csharp
public class AuthController(IJwtService jwt, IUserService users) : ControllerBase
{
    [HttpPost("login")] public async Task<IActionResult> Login(LoginModel m)
    { var id = await users.ValidateUserAsync(m.Username, m.Password); if (string.IsNullOrEmpty(id)) return Unauthorized(); return Ok(await jwt.CreateTokenAsync(id)); }
    [HttpPost("refresh")] public async Task<IActionResult> Refresh(Token token)
    { if (jwt.SupportsRefreshToken()) { var (ok, tk) = await jwt.TryRefreshTokenAsync(token); if (ok) return Ok(tk);} return Unauthorized("请重新登录"); }
}
```

## 客户端自动刷新（概览）
若只需演示，可参考“快速入门示例”；生产建议使用弹性策略防止并发重复刷新。

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
| 刷新未触发 | 未启用/未注册刷新实现 | 检查 EnableRefreshToken & DI |
| 刷新风暴 | 并发 401 竞态 | 使用信号量/单次刷新管控 |
| 刷新成功但仍旧老令牌 | 客户端未更新头部 | 确认事件订阅与 SetToken 调用 |
| Invalid signature | 多实例密钥不一致 | 配置中心或环境变量统一 |

## FAQ
**Q:** 必须启用刷新令牌吗？  **A:** 否，可仅短期令牌。

**Q:** 如何吊销用户所有令牌？  **A:** 记录 jti，加入黑名单；删除刷新令牌记录。

**Q:** 如何支持多租户？  **A:** 添加租户 Claim，并在授权策略中校验。

**Q:** 可以扩展返回模型吗？  **A:** 可在自定义实现中封装 DTO。

**Q:** 如何防止刷新令牌被窃取？  **A:** 服务端存储哈希，客户端仅持有随机值，启用 HTTPS 与最小持久化。
