# Linger.AspNetCore.Jwt

> View this document in: [English](./README.md) | [中文](./README.zh-CN.md)  \
> Full legacy guide: [legacy-full-guide.md](./docs/legacy-full-guide.md)

Lightweight helpers for issuing and refreshing JWT access tokens in ASP.NET Core.

## Features
- Clean core `IJwtService` + opt-in refresh via `IRefreshableJwtService`
- Pluggable refresh token persistence (memory / DB / custom)
- Environment variable `SECRET` overrides config key
- Short-lived access tokens, long-lived refresh tokens
- Adds `jti` & `iat` claims for replay protection & auditing

## Platform
.NET 8+.  
Install from NuGet (package name placeholder):
```bash
dotnet add package Linger.AspNetCore.Jwt
```

## Installation
In `Program.cs`:
```csharp
// Read JwtOptions & configure authentication
var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOption>()!;
builder.Services.AddSingleton(jwtOptions);

builder.Services.AddAuthentication().AddJwtBearer(jwtOptions);

// Choose ONE registration style
// 1. Basic (no refresh)
builder.Services.AddScoped<IJwtService, JwtService>();

// 2. Custom claims (no refresh)
builder.Services.AddScoped<IJwtService, CustomJwtService>();

// 3. Refresh (memory cache)
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IRefreshableJwtService, MemoryCachedJwtService>();
builder.Services.AddScoped<IJwtService>(sp => sp.GetRequiredService<IRefreshableJwtService>());
```

`appsettings.json` (minimal):
```json
{
  "JwtOptions": {
    "SecurityKey": "your-32+chars-secret",
    "Issuer": "your-app",
    "Audience": "your-api",
    "Expires": 15,
    "RefreshTokenExpires": 10080,
    "EnableRefreshToken": true
  }
}
```
Environment precedence: `SECRET` env var > `JwtOptions:SecurityKey`.

## JwtOption
```csharp
public sealed class JwtOption
{
    public string SecurityKey { get; set; } = "dev-secret-change";
    public string Issuer { get; set; } = "example";
    public string Audience { get; set; } = "example";
    public int Expires { get; set; } = 15;               // minutes
    public int RefreshTokenExpires { get; set; } = 10080; // minutes (7d)
    public bool EnableRefreshToken { get; set; } = true;
}
```

## Custom Claims
Extend `JwtService` or override `GetClaimsAsync`:
```csharp
sealed class CustomJwtService(AppDbContext db, JwtOption opt, ILogger<CustomJwtService> logger)
    : JwtService(opt, logger)
{
    protected override async Task<List<Claim>> GetClaimsAsync(string userId)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, userId) };
        var user = await db.Users.FindAsync(userId);
        if (user is not null)
        {
            foreach (var role in user.Roles.Split(','))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }
        return claims;
    }
}
```

## Refresh Token Implementations
Memory cache example:
```csharp
sealed class MemoryCachedJwtService(JwtOption opt, IMemoryCache cache, ILogger<MemoryCachedJwtService> logger)
    : JwtServiceWithRefresh(opt, logger)
{
    protected override Task HandleRefreshToken(string userId, JwtRefreshToken token)
    {
        cache.Set($"RT_{userId}", token, TimeSpan.FromMinutes(_jwtOptions.RefreshTokenExpires));
        return Task.CompletedTask;
    }
    protected override Task<JwtRefreshToken> GetExistRefreshTokenAsync(string userId)
    {
        if (cache.TryGetValue($"RT_{userId}", out JwtRefreshToken? t) && t is not null)
        {
            return Task.FromResult(t);
        }
        throw new InvalidOperationException("Refresh token missing or expired");
    }
}
```
Database example sketch (pseudo):
```csharp
sealed class DbJwtService(JwtOption opt, IUserRepository repo, ILogger<DbJwtService> logger)
    : JwtServiceWithRefresh(opt, logger)
{
    protected override Task HandleRefreshToken(string userId, JwtRefreshToken token)
        => repo.UpdateRefreshTokenAsync(userId, token.RefreshToken, token.ExpiryTime);

    protected override async Task<JwtRefreshToken> GetExistRefreshTokenAsync(string userId)
    {
        var user = await repo.GetUserAsync(userId) ?? throw new InvalidOperationException("User not found");
        if (string.IsNullOrWhiteSpace(user.RefreshToken)) throw new InvalidOperationException("No stored refresh token");
        return new JwtRefreshToken { RefreshToken = user.RefreshToken, ExpiryTime = user.RefreshTokenExpiryTime };
    }
}
```

## Controller Example
```csharp
[ApiController]
[Route("api/auth")]
sealed class AuthController(IJwtService jwt) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        // validate credentials -> userId
        var userId = dto.UserName; // placeholder
        var token = await jwt.CreateTokenAsync(userId);
        return Ok(token);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(TokenEnvelope dto)
    {
        if (jwt.SupportsRefreshToken())
        {
            var (ok, newToken) = await jwt.TryRefreshTokenAsync(dto);
            if (ok) return Ok(newToken);
        }
        return Unauthorized();
    }
}
```

## Client Refresh (Concept)
Use retry/resilience pipeline to intercept 401, refresh once (semaphore guarded), retry original request. See legacy guide for full client examples (Blazor, WinForms, simple HttpClient wrapper).

Workflow:
1. Login -> access + refresh
2. Use access until 401
3. Refresh endpoint with pair
4. Issue new pair (rotate refresh)
5. Deny if refresh invalid/expired -> re-login

## Security
- Store production secret in environment: `SECRET`
- Rotate secrets & refresh tokens on each refresh
- Include `jti` (unique id) & `iat` (issued at)
- Keep access tokens short-lived (minutes)
- Persist refresh tokens server-side; treat as confidential credentials

## Advanced
- Add custom claim sources (roles, permissions)
- Central revoke list keyed by `jti`
- Multi-tenant: include `tenant_id` claim
- Pairwise rotation: invalidate previous refresh token on use

## Troubleshooting
| Symptom | Cause | Fix |
|---------|-------|-----|
| 401 immediately | Clock skew | Sync server time / allow small skew |
| Refresh succeeds but old token still works | Not rotating | Ensure refresh rotates and revoke old jti |
| Memory implementation loses tokens | App recycled | Use persistent store |
| Env SECRET ignored | Name mismatch | Ensure variable EXACT name SECRET |

## FAQ
Q: Why extension + interface for refresh?  
A: Keeps core minimal; refresh optional.

Q: Can I disable refresh?  
A: Set EnableRefreshToken = false and only register IJwtService.

Q: How to revoke a single token?  
A: Track `jti` in a deny list until natural expiry.
