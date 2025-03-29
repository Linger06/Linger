# Linger.AspNetCore.Jwt

A C# helper library for handling JWT token authentication with flexible refresh token implementation.

## Supported .NET versions

This library supports ASP.NET Core applications running on .NET 8.0+.

## Design Features

This library uses extension methods to implement Refresh Token functionality, with the following key advantages:

- **Interface Segregation**: The core `IJwtService` interface remains concise, containing only basic token generation functionality
- **Functional Extensions**: Refresh token functionality is provided through the `IRefreshableJwtService` interface and extension methods
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
services.AddScoped<IRefreshableJwtService, MemoryCachedJwtService>();
// Also register as base interface to allow access via IJwtService
services.AddScoped<IJwtService>(sp => sp.GetRequiredService<IRefreshableJwtService>());

// Option 3: Service with refresh token support using database
services.AddScoped<IRefreshableJwtService, DbJwtService>();
services.AddScoped<IJwtService>(sp => sp.GetRequiredService<IRefreshableJwtService>());
```

### 4. Use in Controllers

Implement authentication in your controllers:

```csharp
// Approach 1: Using only the basic functionality
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

// Approach 2: Directly using the interface with refresh capability
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
        try 
        {
            // Directly call the refresh method, no need to check for support
            var newToken = await _jwtService.RefreshTokenAsync(token);
            return Ok(newToken);
        }
        catch (Exception ex)
        {
            return Unauthorized($"Token refresh failed: {ex.Message}");
        }
    }
}
```

## Client-Side Automatic Token Refresh

In addition to server-side token refresh implementation, clients also need mechanisms to handle token expiration and refreshing. Below is how to implement automatic token refresh using `TokenRefreshInterceptor`.

### Install Required Packages

To use automatic token refresh functionality on the client side, you need to install the following NuGet packages:

```bash
# Install HTTP client interfaces and contracts
dotnet add package Linger.HttpClient.Contracts

# Install HTTP client implementation (choose one)
dotnet add package Linger.HttpClient
# or
dotnet add package Linger.HttpClient.Flurl
```

Using Package Manager Console:

```powershell
Install-Package Linger.HttpClient.Contracts
Install-Package Linger.HttpClient
```

### Token Service Interface

First, define a token service interface for managing and refreshing tokens:

```csharp
public interface ITokenService
{
    /// <summary>
    /// Get the current token
    /// </summary>
    Task<string> GetCurrentTokenAsync();
    
    /// <summary>
    /// Refresh the token
    /// </summary>
    Task<bool> RefreshTokenAsync();
    
    /// <summary>
    /// Invalidate the token
    /// </summary>
    Task<bool> InvalidateTokenAsync();
    
    /// <summary>
    /// Check if the token is valid
    /// </summary>
    bool IsTokenValid();
}
```

### Token Service Implementation

Here's a basic implementation of the `ITokenService` interface for managing JWT tokens and interacting with the authorization server:

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
    /// Get the current token
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
    /// Refresh the token
    /// </summary>
    public async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrEmpty(_refreshToken))
        {
            // First login or refresh token is empty, perform full login
            return await LoginAsync();
        }
        
        try
        {
            // Call the refresh token API
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
                // Update stored tokens and expiration
                _currentToken = response.Data.AccessToken;
                _refreshToken = response.Data.RefreshToken;
                _expiresAt = DateTime.UtcNow.AddSeconds(response.Data.ExpiresIn);
                return true;
            }
            
            // Refresh failed, try to login again
            return await LoginAsync();
        }
        catch
        {
            // If any exception occurs, try to login again
            return await LoginAsync();
        }
    }
    
    /// <summary>
    /// Invalidate the token
    /// </summary>
    public async Task<bool> InvalidateTokenAsync()
    {
        try
        {
            // Optional: Call server logout endpoint to invalidate token on server side
            if (!string.IsNullOrEmpty(_currentToken))
            {
                await _httpClient.CallApi<object>("auth/logout", HttpMethodEnum.Post);
            }
        }
        catch
        {
            // Continue clearing local tokens even if server call fails
        }
        
        // Clear locally stored tokens
        _currentToken = string.Empty;
        _refreshToken = string.Empty;
        _expiresAt = DateTime.MinValue;
        
        return true;
    }
    
    /// <summary>
    /// Check if the token is valid
    /// </summary>
    public bool IsTokenValid()
    {
        // Check if token exists and is not expired (consider as expired 5 minutes before actual expiry)
        return !string.IsNullOrEmpty(_currentToken) && DateTime.UtcNow < _expiresAt.AddMinutes(-5);
    }
    
    /// <summary>
    /// Login to get new tokens
    /// </summary>
    private async Task<bool> LoginAsync()
    {
        try
        {
            // Get credentials from secure storage or prompt user
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
                
                // Save tokens to persistent storage depending on your scenario
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
    /// Get user credentials
    /// </summary>
    private Task<UserCredentials> GetUserCredentialsAsync()
    {
        // In a real app, this might get from secure storage or trigger UI for user input
        return Task.FromResult(new UserCredentials 
        { 
            Username = "username", 
            Password = "password" 
        });
    }
    
    /// <summary>
    /// Save tokens to persistent storage
    /// </summary>
    private Task SaveTokensToStorageAsync()
    {
        // In a real app, you might use secure storage to save tokens
        // Examples: Keychain on mobile, encrypted storage on desktop
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Token response model
    /// </summary>
    private class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
    
    /// <summary>
    /// User credentials model
    /// </summary>
    private class UserCredentials
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
```

In a real-world application, you might want to add more features such as:

1. Using secure storage (like ASP.NET Core's Data Protection API) to persist refresh tokens
2. Adding event callbacks to notify the UI when token refresh fails and user needs to re-login
3. Adding retry logic to handle network errors
4. Using shared cache storage for tokens in distributed applications

### Implementing the Token Refresh Interceptor

Implement a token refresh interceptor using the `IHttpClientInterceptor` interface to automatically handle 401 responses:

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
        // Ensure the request has the latest token
        var token = await _tokenService.GetCurrentTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        return request;
    }
    
    public async Task<HttpResponseMessage> OnResponseAsync(HttpResponseMessage response)
    {
        // If the response is 401 Unauthorized, try to refresh the token and retry the request
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Use a semaphore to prevent multiple requests from trying to refresh the token simultaneously
            await _semaphore.WaitAsync();
            try
            {
                // Check the token again, as it might have been refreshed while waiting for the semaphore
                var isRefreshed = await _tokenService.RefreshTokenAsync();
                if (isRefreshed)
                {
                    // Get the original request
                    var originalRequest = response.RequestMessage;
                    if (originalRequest != null)
                    {
                        // Clone the request
                        var newRequest = new HttpRequestMessage(originalRequest.Method, originalRequest.RequestUri)
                        {
                            Content = originalRequest.Content,
                            Version = originalRequest.Version
                        };
                        
                        // Copy headers
                        foreach (var header in originalRequest.Headers)
                        {
                            newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                        
                        // Add the new token
                        var token = await _tokenService.GetCurrentTokenAsync();
                        newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        
                        // Resend the request
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
        
        // Default to GET
        return HttpMethodEnum.Get;
    }
}
```

### Registration and Usage

Register the token service and interceptor in the dependency injection container:

```csharp
// Register token service
services.AddScoped<ITokenService, TokenService>();

// Register HTTP client and configure interceptor
services.AddScoped<IHttpClient>(provider => 
{
    var client = new BaseHttpClient("https://api.example.com");
    var tokenService = provider.GetRequiredService<ITokenService>();
    
    // Add token refresh interceptor
    client.AddInterceptor(new TokenRefreshInterceptor(tokenService, client));
    
    return client;
});
```

### Workflow

1. For each HTTP request, the interceptor checks and adds the latest JWT token
2. If a 401 Unauthorized response is received, the interceptor will:
   - Automatically attempt to refresh the token
   - Resend the original request with the new token
   - Return the new response instead of the 401 error
3. The application code doesn't need to be concerned with token refresh logic and can focus on business functionality

### Advantages

- **Transparent Handling**: Application code doesn't need to explicitly handle token expiration
- **Prevents Request Storms**: Uses a semaphore to ensure multiple concurrent requests only trigger one token refresh
- **Automatic Retry**: Seamlessly retries requests that failed due to token expiration
- **Coordinated with Server**: Works perfectly with the server-side JWT refresh mechanism

With this approach, you can build a seamless authentication experience where users are not interrupted due to token expiration while maintaining system security.

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

## Advanced Features

### Token Blacklist and Revocation

This library supports revoking issued but not yet expired tokens through a blacklist mechanism, providing additional security:

```csharp
// Register token blacklist service (already added automatically in the ConfigureJwt method)
services.AddSingleton<JwtTokenBlacklist>();

// Implement revocation in a JWT service
public class CustomJwtService : JwtService 
{
    public CustomJwtService(JwtOption jwtOptions, JwtTokenBlacklist tokenBlacklist, ILogger<CustomJwtService>? logger = null)
        : base(jwtOptions, logger, tokenBlacklist)
    {
    }
    
    // Revoke a specific token by calling this method
    public async Task RevokeUserTokenAsync(string userId) 
    {
        // Find the user's token ID and revoke it
        var tokenId = GetUserTokenId(userId);
        if (!string.IsNullOrEmpty(tokenId))
        {
            // Revoke the token until its original expiration time
            await RevokeTokenAsync(tokenId, DateTime.UtcNow.AddMinutes(_jwtOptions.Expires));
        }
    }
}
```

The blacklist service periodically cleans up expired token entries, no manual maintenance required.

### Enhanced Token Security

The following claims have been added to tokens to enhance security:

1. **Unique Identifier (jti)**: Each token has a unique ID for tracking and revocation
2. **Issued At Time (iat)**: Records when the token was issued, for validation and auditing

These enhancements can be used without modifying existing code and provide the following benefits:

- Prevention of replay attacks
- Support for precise token revocation
- Improved logging and auditing capabilities
- Compliance with security best practices

### Using Token Revocation

```csharp
[Authorize]
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    // Get the current user's token ID
    var tokenId = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
    
    if (!string.IsNullOrEmpty(tokenId))
    {
        // Calculate the token's original expiration time
        var issuedAt = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value;
        var expiryTime = DateTime.UtcNow.AddMinutes(_jwtOptions.Expires);
        
        if (long.TryParse(issuedAt, out var issuedAtTimestamp))
        {
            var issuedAtDateTime = DateTimeOffset.FromUnixTimeSeconds(issuedAtTimestamp).UtcDateTime;
            expiryTime = issuedAtDateTime.AddMinutes(_jwtOptions.Expires);
        }
        
        // Revoke the token
        await _jwtService.RevokeTokenAsync(tokenId, expiryTime);
        return Ok(new { message = "Logout successful" });
    }
    
    return BadRequest(new { message = "Invalid token ID" });
}
```