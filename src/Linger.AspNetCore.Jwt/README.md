# Linger.AspNetCore.Jwt

A C# helper library for handling JWT token authentication with flexible refresh token implementation.

## Supported .NET versions

This library supports ASP.NET Core applications running on .NET 8.0+.

## Design Features

This library uses extension methods to implement Refresh Token functionality, with the following key advantages:

- **Interface Segregation**: The core `IJwtService` interface remains concise, containing only basic token generation functionality
- **Functional Extensions**: Refresh token functionality is provided through the marker interface `ISupportsRefreshToken` and extension methods
- **Flexible Usage**: Choose between basic JWT authentication or authentication with refresh tokens based on different scenarios
- **Compatibility**: Does not disrupt existing code structure and easily integrates into existing projects

## Usage Guide

### 1. Configure JWT Options

The `JwtOption` class provides configuration settings for JWT token generation and validation:

```csharp
public class JwtOption
{
    // JWT signing key (in production, should be stored in a secure location)
    public string SecurityKey { get; set; } = "this is my custom Secret key for authentication";
    
    // Token issuer (typically your application or authentication server domain)
    public string Issuer { get; set; } = "Linger.com";
    
    // Token audience (typically your API domain)
    public string Audience { get; set; } = "Linger.com";
    
    // Access token expiration time in minutes (default: 30 minutes)
    public int Expires { get; set; } = 30;
    
    // Refresh token expiration time in minutes (default: 60 minutes)
    public int RefreshTokenExpires { get; set; } = 60;
    
    // Flag to enable/disable refresh token functionality
    public bool EnableRefreshToken { get; set; } = true;
}
```

Configure JWT options in your `appsettings.json` file:

```json
{
  "JwtOptions": {
    "SecurityKey": "your-secure-key-with-at-least-256-bits",
    "Issuer": "your-app.com",
    "Audience": "your-api.com",
    "Expires": 15,
    "RefreshTokenExpires": 10080, // 7 days
    "EnableRefreshToken": true
  }
}
```

### 2. Implement Your Custom JWT Service

Since `JwtServiceWithRefresh` is an abstract class, you need to inherit from it and implement the abstract methods to handle refresh token storage and retrieval:

```csharp
// Example implementation using memory cache
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
        // Store refresh token in memory cache
        _cache.Set($"RT_{userId}", refreshToken, TimeSpan.FromMinutes(_jwtOptions.RefreshTokenExpires));
        return Task.CompletedTask;
    }

    protected override Task<JwtRefreshToken> GetExistRefreshTokenAsync(string userId)
    {
        // Retrieve refresh token from memory cache
        if (_cache.TryGetValue($"RT_{userId}", out JwtRefreshToken? token) && token != null)
        {
            return Task.FromResult(token);
        }
        
        throw new Exception("Refresh token not found or expired");
    }
}

// Example implementation using database
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
        // Store refresh token in database
        await _userRepository.UpdateRefreshTokenAsync(userId, refreshToken.RefreshToken, refreshToken.ExpiryTime);
    }

    protected override async Task<JwtRefreshToken> GetExistRefreshTokenAsync(string userId)
    {
        // Retrieve refresh token from database
        var user = await _userRepository.GetUserAsync(userId);
        if (user != null && !string.IsNullOrEmpty(user.RefreshToken))
        {
            return new JwtRefreshToken
            {
                RefreshToken = user.RefreshToken,
                ExpiryTime = user.RefreshTokenExpiryTime
            };
        }
        
        throw new Exception("Refresh token not found or expired");
    }
}
```

### 3. Register Services

Register the JWT services in your `Program.cs` or `Startup.cs`:

```csharp
// Add JWT configuration from appsettings.json
services.Configure<JwtOption>(Configuration.GetSection("JwtOptions"));

// Register as singleton to ensure consistent configuration
services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtOption>>().Value);

// Register JWT service - choose one of these options:
// Option 1: Basic service (without refresh token)
services.AddScoped<IJwtService, JwtService>();

// Option 2: Service with refresh token support using memory cache
services.AddScoped<IJwtService, MemoryCachedJwtService>();

// Option 3: Service with refresh token support using database
services.AddScoped<IJwtService, DbJwtService>();
```

### 4. Use in Controllers

Implement authentication in your controllers:

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
        // Validate user credentials...
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
        // Use extension method to check if refresh functionality is supported
        if (_jwtService.SupportsRefreshToken())
        {
            var (success, newToken) = await _jwtService.TryRefreshTokenAsync(token);
            if (success)
            {
                return Ok(newToken);
            }
        }
        
        return Unauthorized("Please log in again");
    }
}
```

## Refresh Token Principles

### What are Refresh Tokens?

Refresh tokens are credentials used to obtain new access tokens. When an access token expires, we can use the refresh token to get a new access token from the authentication component.

Comparison of features:
- **Access Token**: Short expiration time (typically minutes), stored on the client side
- **Refresh Token**: Longer expiration time (typically days), stored in the server database

### Token Usage Flow

![Refresh Token Flow](refresh-token-flow.png "Refresh Token Flow")

1. Client authenticates by providing credentials (e.g., username/password)
2. Server validates credentials and issues an access token and refresh token
3. Client uses the access token to request protected resources
4. Server validates the access token and provides the resources
5. Steps 3-4 repeat until the access token expires
6. When the access token expires, client uses the refresh token to request new tokens
7. Server validates the refresh token and issues new access and refresh tokens
8. Steps 3-7 repeat until the refresh token expires
9. When the refresh token expires, client must re-authenticate with full credentials (step 1)

### Why Do We Need Refresh Tokens?

So, why do we need both access tokens and refresh tokens? Why don’t we just set a long expiration date, like a month or a year for the access tokens? Because, if we do that and someone manages to get hold of our access token they can use it for a long period, even if we change our password!

The idea of refresh tokens is that we can make the access token short-lived so that, even if it is compromised, the attacker gets access only for a shorter period. With refresh token-based flow, the authentication server issues a one-time use refresh token along with the access token. The app stores the refresh token safely.

Every time the app sends a request to the server it sends the access token in the Authorization header and the server can identify the app using it. Once the access token expires, the server will send a token expired response. Once the app receives the token expired response, it sends the expired access token and the refresh token to obtain a new access token and refresh token. 

If something goes wrong, the refresh token can be revoked which means that when the app tries to use it to get a new access token, that request will be rejected and the user will have to enter credentials once again and authenticate.

Thus, refresh tokens help in a smooth authentication workflow without the need for users to submit their credentials frequently, and at the same time, without compromising the security of the app.