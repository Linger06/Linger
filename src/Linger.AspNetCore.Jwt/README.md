# Linger.AspNetCore.Jwt

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

A C# helper library for handling JWT token authentication with flexible refresh token implementation.

## Supported .NET Versions

This library supports ASP.NET Core applications running on .NET 8.0+.

## Design Features

The library implements refresh token functionality through extension methods, with the following advantages:

- **Interface Isolation**: Core `IJwtService` interface remains clean, containing only basic token generation
- **Feature Extensions**: Refresh token functionality provided via `IRefreshableJwtService` and extensions
- **Flexible Usage**: Choose between basic JWT authentication or authentication with refresh tokens
- **Backward Compatibility**: Does not break existing code structure, easy to integrate

## Usage Guide

### 1. Configure JWT Options

`JwtOption` class provides configuration settings for JWT token generation and validation:

```csharp
public class JwtOption
{
    // JWT signing key (in production, should be stored in a secure location)
    public string SecurityKey { get; set; } = "this is my custom Secret key for authentication";
    
    // Token issuer (typically your application or auth server domain)
    public string Issuer { get; set; } = "Linger.com";
    
    // Token audience (typically your API domain)
    public string Audience { get; set; } = "Linger.com";
    
    // Access token expiry time in minutes (default: 30 minutes)
    public int Expires { get; set; } = 30;
    
    // Refresh token expiry time in minutes (default: 60 minutes)
    public int RefreshTokenExpires { get; set; } = 60;
    
    // Flag to enable/disable refresh token functionality
    public bool EnableRefreshToken { get; set; } = true;
}
```

Configure JWT options in your `appsettings.json` file:

```json
{
  "JwtOptions": {
    "SecurityKey": "your-secure-key-at-least-256-bits",
    "Issuer": "your-app.com",
    "Audience": "your-api.com",
    "Expires": 15,
    "RefreshTokenExpires": 10080, // 7 days
    "EnableRefreshToken": true
  }
}
```

### 2. Using the Default JWT Service

`JwtService` implements `IJwtService` and only includes `ClaimTypes.Name` in the token:

```csharp
    protected virtual Task<List<Claim>> GetClaimsAsync(string userId)
    {
        // Example of returning username and role claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userId)
        };

        //IList<string> roles = await _userManager.GetRolesAsync(User);
        //claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return Task.FromResult(claims);
    }
```

### 3. Implement Your Custom JWT Service

If `JwtService` doesn't meet your needs, you can create a custom JWT service and override the `GetClaimsAsync` method:

```csharp
public class CustomJwtServices(CrudAppContext dbContext, JwtOption jwtOptions, ILogger? logger = null) : JwtService(jwtOptions, logger)
{
    protected override async Task<List<Claim>> GetClaimsAsync(string userId)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userId)
            };

        var user = await dbContext.Users.FindAsync(userId);

        // Add current user's roles to claims for frontend permission control
        foreach (var role in user.Roles.Split(','))
            claims.Add(new Claim(ClaimTypes.Role, role));

        return claims;
    }
}
```

If you need refresh token functionality, you need to implement `JwtServiceWithRefresh`.
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
        // Get refresh token from memory cache
        if (_cache.TryGetValue($"RT_{userId}", out JwtRefreshToken? token) && token != null)
        {
            return Task.FromResult(token);
        }
        
        throw new Exception("Refresh token not found or expired");
    }
}

// Example implementation using a database
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
        // Get refresh token from database
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

### 4. Register Services

Register the JWT service in your `Program.cs` or `Startup.cs`:

```csharp
// Configure JwtBearer - choose one of the following options:

// Option 1: Use built-in method
builder.Services.ConfigureJwt(builder.Configuration);

// Option 2: If there are multiple authentication methods like Cookie, Jwt, etc.

    // Get JWT configuration from appsettings.json and register as singleton
    JwtOption? config = builder.Configuration.GetGeneric<JwtOption>("JwtOptions");
    ArgumentNullException.ThrowIfNull(config);
    builder.Services.AddSingleton(config);

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .AddJwtBearer(config); // This method is an extension method from the library

// Register JWT service - choose one of these options:

// Option 1: Basic service (no refresh token)
builder.Services.AddScoped<IJwtService, JwtService>();

// Option 2: Custom service (no refresh token)
builder.Services.AddScoped<IJwtService, CustomJwtServices>();

// Option 3: Service with refresh token support (using memory cache)
builder.Services.AddScoped<IRefreshableJwtService, MemoryCachedJwtService>();
// Also register as base interface, allowing access via IJwtService
builder.Services.AddScoped<IJwtService>(sp => sp.GetRequiredService<IRefreshableJwtService>());

// Option 4: Service with refresh token support (using database)
builder.Services.AddScoped<IRefreshableJwtService, DbJwtService>();
builder.Services.AddScoped<IJwtService>(sp => sp.GetRequiredService<IRefreshableJwtService>());
```

### 5. Use in Controllers

Implement authentication in your controllers:

```csharp
// Approach 1: Using only basic functionality
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
        // Use extension method to check if refresh is supported
        if (_jwtService.SupportsRefreshToken())
        {
            var (success, newToken) = await _jwtService.TryRefreshTokenAsync(token);
            if (success)
            {
                return Ok(newToken);
            }
        }
        
        return Unauthorized("Please login again");
    }
}

// Approach 2: Using interface with refresh functionality
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
            // Directly call refresh method, no need to check support
            var newToken = await _jwtService.RefreshTokenAsync(token);
            return Ok(newToken);
        }
        catch (Exception ex)
        {
            return Unauthorized($"Refresh token failed: {ex.Message}");
        }
    }
}
```

## Client-Side Automatic Token Refresh

In addition to server-side token refresh implementation, clients also need mechanisms to handle token expiration and refreshing. The recommended approach is using Microsoft.Extensions.Http.Resilience, which provides a more integrated and robust solution compared to traditional interceptors.

### Install Required Packages

To use automatic token refresh functionality on the client side, you need to install the following NuGet packages:

```bash
# Install HTTP client interfaces and contracts
dotnet add package Linger.HttpClient.Contracts

# Install HTTP client implementation
dotnet add package Linger.HttpClient.Standard

# Install Microsoft.Extensions.Http.Resilience for handling retries and token refresh
dotnet add package Microsoft.Extensions.Http.Resilience
```

### Implementing Token Refresh with Resilience

The modern approach uses Microsoft.Extensions.Http.Resilience to handle token refresh in a thread-safe and resilient manner:

1. First, create an application state class to maintain token state:

```csharp
/// <summary>
/// Application state management class for storing cross-component state
/// </summary>
public class AppState
{
    private string _token = string.Empty;
    
    /// <summary>
    /// User's JWT authentication token
    /// </summary>
    public string? Token 
    { 
        get => _token;
        set 
        {
            _token = value ?? string.Empty;
            NotifyStateChanged();
        }
    }
    
    /// <summary>
    /// Refresh token for obtaining new access tokens
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Checks if the user is currently authenticated
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
    
    /// <summary>
    /// Event triggered when token changes
    /// </summary>
    public event Action? OnChange;
    
    /// <summary>
    /// Event triggered when re-login is required
    /// </summary>
    public event Action? RequireRelogin;
    
    /// <summary>
    /// Notify listeners about state changes
    /// </summary>
    private void NotifyStateChanged() => OnChange?.Invoke();
    
    /// <summary>
    /// Triggers a re-login request
    /// </summary>
    public void RaiseRequireReloginEvent()
    {
        RequireRelogin?.Invoke();
    }
}
```

2. Then, create a token refresh handler that will manage the token refresh process:

```csharp
/// <summary>
— Token refresh handler using Microsoft.Extensions.Http.Resilience
/// </summary>
public class TokenRefreshHandler
{
    private readonly AppState _appState;
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public TokenRefreshHandler(AppState appState, IServiceProvider serviceProvider)
    {
        _appState = appState;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Configure token refresh resilience pipeline
    /// </summary>
    public void ConfigureTokenRefreshResiliencePipeline(ResiliencePipelineBuilder<HttpResponseMessage> builder)
    {
        // Add retry policy for handling 401 (Unauthorized) responses
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            // Only try refreshing the token once
            MaxRetryAttempts = 1,
            // Only handle 401 Unauthorized responses
            ShouldHandle = args => 
            {
                bool shouldRetry = args.Outcome.Result?.StatusCode == HttpStatusCode.Unauthorized;
                return ValueTask.FromResult(shouldRetry);
            },
            // Refresh the token before retrying
            OnRetry = async context =>
            {
                // Use semaphore to prevent multiple concurrent token refresh attempts
                await _semaphore.WaitAsync();
                try
                {
                    await RefreshTokenAsync();
                }
                finally
                {
                    _semaphore.Release();
                }
            },
            // Retry immediately after token refresh
            BackoffType = DelayBackoffType.Constant,
            Delay = TimeSpan.Zero
        });
    }

    /// <summary>
    /// Refresh the token
    /// </summary>
    private async Task RefreshTokenAsync()
    {
        try
        {
            // Get auth service to refresh token
            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();

            // Try to get a new token using the current token
            var (success, newToken) = await authService.RefreshTokenAsync(
                _appState.Token, 
                _appState.RefreshToken);

            if (success && !string.IsNullOrEmpty(newToken))
            {
                // Update token in app state
                _appState.Token = newToken;
                // Token property setter automatically notifies changes
            }
            else
            {
                // Clear tokens if refresh failed
                _appState.Token = string.Empty;
                _appState.RefreshToken = string.Empty;
                // Trigger re-login event
                _appState.RaiseRequireReloginEvent();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token refresh failed: {ex.Message}");
            
            // Clear invalid tokens
            _appState.Token = string.Empty;
            _appState.RefreshToken = string.Empty;
            
            // Trigger re-login event
            _appState.RaiseRequireReloginEvent();
        }
    }
}
```

3. Implement an authentication service that can handle both login and token refresh:

```csharp
/// <summary>
— Authentication service using IHttpClient for login and logout operations
/// </summary>
public class AuthService
{
    private readonly IHttpClient _httpClient;
    private readonly AppState _appState;
    private readonly ILogger<AuthService>? _logger;

    public AuthService(IHttpClient httpClient, AppState appState, ILogger<AuthService>? logger = null)
    {
        _httpClient = httpClient;
        _appState = appState;
        _logger = logger;
    }

    /// <summary>
    /// Login method
    /// </summary>
    public async Task<bool> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation($"Attempting to login user: {loginRequest.Username}");

            // Use IHttpClient to send POST request
            var result = await _httpClient.CallApi<LoginResponse>(
                "api/auth/login",
                HttpMethodEnum.Post,
                postData: loginRequest,
                cancellationToken: cancellationToken);

            if (!result.IsSuccess)
            {
                _logger?.LogWarning($"Login failed: {result.ErrorMsg}");
                return false;
            }

            // Save token and user info to app state
            _appState.Token = result.Data.Token;
            _appState.Username = loginRequest.Username;
            _appState.IsLoggedIn = true;

            _logger?.LogInformation($"User {loginRequest.Username} logged in successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Exception during login process: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Refresh token method
    /// </summary>
    public async Task<(bool success, string newToken)> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        try
        {
            // Create refresh token request data
            var refreshRequest = new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            
            // Call refresh token API
            var response = await _httpClient.CallApi<TokenResponse>(
                "api/auth/refresh", 
                HttpMethodEnum.Post, 
                refreshRequest);
                
            if (response.IsSuccess && response.Data != null)
            {
                return (true, response.Data.AccessToken);
            }
            
            return (false, string.Empty);
        }
        catch
        {
            return (false, string.Empty);
        }
    }
    
    /// <summary>
    /// Logout method
    /// </summary>
    public Task<bool> Logout()
    {
        _logger?.LogInformation($"User {_appState.Username} logging out");

        // Clear token and user info
        _appState.Token = null;
        _appState.Username = string.Empty;
        _appState.IsLoggedIn = false;

        return Task.FromResult(true);
    }
    
    // Token response model
    private class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
}
```

### Registration and Usage

Register the token refresh handler with HttpClientFactory and configure resilience:

```csharp
// Register AppState
services.AddSingleton<AppState>();

// Register token refresh handler
services.AddSingleton<TokenRefreshHandler>();

// Register HTTP client and services
services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddTypedClient<IHttpClient>((httpClient, serviceProvider) =>
{
    var standardClient = new StandardHttpClient(httpClient);

    // Set token from AppState
    var appState = serviceProvider.GetRequiredService<AppState>();
    if (!string.IsNullOrEmpty(appState.Token))
    {
        standardClient.SetToken(appState.Token);
    }

    // Subscribe to token changes
    appState.OnChange += () =>
    {
        if (!string.IsNullOrEmpty(appState.Token))
        {
            standardClient.SetToken(appState.Token);
        }
    };

    return standardClient;
})
.AddResilienceHandler("Default", (builder, context) =>
{
    // Add standard retry policy for common HTTP errors
    builder.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2),
        ShouldHandle = args =>
        {
            return ValueTask.FromResult(args.Outcome.Result?.StatusCode is
                HttpStatusCode.RequestTimeout or        // 408
                HttpStatusCode.TooManyRequests or       // 429
                HttpStatusCode.BadGateway or            // 502
                HttpStatusCode.ServiceUnavailable or    // 503
                HttpStatusCode.GatewayTimeout);         // 504
        }
    });

    // Add token refresh policy
    var tokenRefreshHandler = context.ServiceProvider.GetRequiredService<TokenRefreshHandler>();
    tokenRefreshHandler.ConfigureTokenRefreshResiliencePipeline(builder);
});

// Register authentication service
services.AddScoped<AuthService>();
```

### Handling Re-Login in Different Client Types

You need to handle the `RequireReLogin` event according to your client type:

#### For Blazor Applications

```csharp
// Inject AppState
@inject AppState AppState
@inject NavigationManager Navigation
@implements IDisposable

@code {
    protected override void OnInitialized()
    {
        // Subscribe to re-login event
        AppState.RequireRelogin += HandleRequireReLogin;
        base.OnInitialized();
    }

    private void HandleRequireReLogin()
    {
        // Redirect to login page
        Navigation.NavigateTo("/login", forceLoad: false);
    }

    public void Dispose()
    {
        // Unsubscribe to prevent memory leaks
        AppState.RequireRelogin -= HandleRequireReLogin;
    }
}
```

#### For WinForms Applications with Blazor WebView

```csharp
public partial class MainForm : Form
{
    // Access AppState directly from services
    public MainForm()
    {
        InitializeComponent();
        
        // Other initialization...
        
        // Get AppState from Blazor services
        var appState = blazorWebView.Services.GetRequiredService<AppState>();
        appState.RequireRelogin += HandleRequireReLogin;
    }
    
    private void HandleRequireReLogin()
    {
        // Need to invoke on UI thread since event might come from background thread
        this.Invoke((MethodInvoker)delegate
        {
            // Show login form
            var loginForm = new LoginForm();
            
            // Option 1: Show as dialog
            if (loginForm.ShowDialog(this) != DialogResult.OK)
            {
                // User cancelled login
                // You might want to close the application or take other actions
            }
        });
    }
    
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // Get AppState from Blazor services
        var appState = blazorWebView.Services.GetRequiredService<AppState>();
        
        // Unsubscribe when form closes
        appState.RequireRelogin -= HandleRequireReLogin;
        base.OnFormClosed(e);
    }
}
```

#### For Pure WinForms Applications (without Blazor WebView)

```csharp
// Program.cs
internal static class Program
{
    // Service provider for the application
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
        
        // Start the main form
        var mainForm = ServiceProvider.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }
    
    private static void ConfigureServices(IServiceCollection services)
    {
        // Register AppState as singleton
        services.AddSingleton<AppState>();
        
        // Register token refresh handler
        services.AddSingleton<TokenRefreshHandler>();
        
        // Register HttpClient with resilience pipeline
        services.AddHttpClient<IHttpClient, StandardHttpClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.example.com/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddTypedClient<IHttpClient>((httpClient, serviceProvider) =>
        {
            var standardClient = new StandardHttpClient(httpClient);
            
            // Set token from AppState
            var appState = serviceProvider.GetRequiredService<AppState>();
            if (!string.IsNullOrEmpty(appState.Token))
            {
                standardClient.SetToken(appState.Token);
            }
            
            // Subscribe to token changes
            appState.OnChange += () =>
            {
                if (!string.IsNullOrEmpty(appState.Token))
                {
                    standardClient.SetToken(appState.Token);
                }
            };
            
            return standardClient;
        })
        .AddResilienceHandler("Default", (builder, context) =>
        {
            // Standard retry policy
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                ShouldHandle = args =>
                {
                    return ValueTask.FromResult(args.Outcome.Result?.StatusCode is
                        HttpStatusCode.RequestTimeout or        // 408
                        HttpStatusCode.TooManyRequests or       // 429
                        HttpStatusCode.BadGateway or            // 502
                        HttpStatusCode.ServiceUnavailable or    // 503
                        HttpStatusCode.GatewayTimeout);         // 504
                }
            });
            
            // Add token refresh policy
            var tokenRefreshHandler = context.ServiceProvider.GetRequiredService<TokenRefreshHandler>();
            tokenRefreshHandler.ConfigureTokenRefreshResiliencePipeline(builder);
        });
        
        // Register services
        services.AddTransient<AuthService>();
        services.AddTransient<LoginForm>();
        services.AddTransient<MainForm>();
    }
}

// MainForm.cs
public partial class MainForm : Form
{
    private readonly AppState _appState;
    
    public MainForm(AppState appState)
    {
        InitializeComponent();
        _appState = appState;
        
        // Subscribe to re-login event
        _appState.RequireRelogin += HandleRequireReLogin;
        
        // Check if user is already authenticated
        if (!_appState.IsAuthenticated)
        {
            ShowLoginForm();
        }
    }
    
    private void HandleRequireReLogin()
    {
        // Need to invoke on UI thread since event might come from background thread
        this.Invoke(() => ShowLoginForm());
    }
    
    private void ShowLoginForm()
    {
        using var loginForm = Program.ServiceProvider.GetRequiredService<LoginForm>();
        
        if (loginForm.ShowDialog() != DialogResult.OK)
        {
            // User cancelled login
            Close();
        }
    }
    
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // Unsubscribe when form closes
        _appState.RequireRelogin -= HandleRequireReLogin;
        base.OnFormClosed(e);
    }
}

// LoginForm.cs
public partial class LoginForm : Form
{
    private readonly AuthService _authService;
    
    public LoginForm(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        
        // Setup UI controls
        btnLogin.Click += BtnLogin_Click;
    }
    
    private async void BtnLogin_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
        {
            MessageBox.Show("Please enter both username and password", "Login Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
        btnLogin.Enabled = false;
        lblStatus.Text = "Logging in...";
        
        try
        {
            var loginRequest = new LoginRequest
            {
                Username = txtUsername.Text,
                Password = txtPassword.Text
            };
            
            bool success = await _authService.LoginAsync(loginRequest);
            
            if (success)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblStatus.Text = "Login failed. Please check your credentials.";
                btnLogin.Enabled = true;
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = "An error occurred during login.";
            MessageBox.Show($"Login failed: {ex.Message}", "Login Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnLogin.Enabled = true;
        }
    }
}
```

This pure WinForms approach uses dependency injection without Blazor, allowing your WinForms application to benefit from the same token refresh mechanism. The key differences are:

1. Service provider is created and managed at the application level
2. Forms are resolved from the service provider and receive dependencies via constructor injection
3. The AppState is still used to track authentication state and trigger re-login
4. The token refresh handler works with Microsoft.Extensions.Http.Resilience in the same way

### Advantages of the Resilience Approach

This approach using Microsoft.Extensions.Http.Resilience offers several advantages over traditional interceptors:

1. **Tight Integration with .NET Ecosystem**: Uses the official Microsoft-supported approach for HTTP client resiliency
2. **Declarative Configuration**: Clear, well-structured configuration of resilience behaviors
3. **Thread Safety**: Built-in protection against token refresh storms with semaphore
4. **Composable Policies**: Easily combine with other resilience policies (retries, circuit breakers, etc.)
5. **Testability**: Easier to unit test than interceptor-based approaches
6. **Performance**: More efficient implementation with less overhead
7. **Maintainability**: Clear separation of concerns between HTTP client and token refresh logic

### Workflow

1. When a request returns 401 Unauthorized, the resilience handler detects it
2. The token refresh policy is triggered and attempts to refresh the token
3. If successful, the request is automatically retried with the new token
4. If unsuccessful, the re-login event is triggered
5. The application then handles the re-login event (e.g., redirect to login page)

All of this happens transparently to the business logic code, which remains focused on its primary responsibilities rather than authentication concerns.

## Refresh Token Principles

### What is a Refresh Token?

A refresh token is a credential that can be used to obtain a new access token. When an access token expires, we can use the refresh token to get a new access token from the authentication component.

Feature comparison:
- **Access Token**: Short expiry time (typically minutes), stored client-side
- **Refresh Token**: Long expiry time (typically days), stored server-side in a database

### Token Usage Flow

![Refresh Token Flow](refresh-token-flow.png "Refresh Token Flow")

1. Client authenticates by providing credentials (e.g., username password)
2. Server validates credentials and issues access token and refresh token
3. Client uses access token to request protected resources
4. Server validates access token and provides resources
5. Repeat steps 3-4 until access token expires
6. After access token expires, client uses refresh token to request new tokens
7. Server validates refresh token and issues new access and refresh tokens
8. Repeat steps 3-7 until refresh token expires
9. After refresh token expires, client needs to authenticate again (step 1)

### Why Use Refresh Tokens?

So why do we need both access and refresh tokens? Why not set a longer expiration date for access tokens, like a month or year? Because if we did that and someone managed to get our access token, they could use it for a long time even if we changed our password!

The idea with refresh tokens is that we can keep the access token's lifetime short, so even if it's compromised, an attacker only gets access for a brief period. With a refresh token flow, the authentication server issues a one-time use refresh token along with the access token. The application securely stores the refresh token.

Each time the application sends a request to the server, it sends the access token in the Authorization header, which the server can identify as being sent by the application. Once the access token expires, the server will send a token expired response. Upon receiving a token expired response, the application sends the expired access token and refresh token to get a new access and refresh token.

If something goes wrong, the refresh token can be revoked, meaning that when the application tries to use it to get a new access token, the request will be denied, and the user must authenticate again by providing their credentials.

Therefore, refresh tokens help for a smooth authentication workflow without requiring users to frequently submit their credentials, while not compromising the security of the application.

## Advanced Features

### Enhanced Token Security

The following claims are added to tokens to enhance security:

1. **Unique Identifier (jti)**: Each token has a unique ID, facilitating tracking and revocation
2. **Issue Time (iat)**: Records when the token was issued, used for validation and auditing

These enhancements can be used without modifying existing code and provide the following benefits:

- Prevention of replay attacks
- Support for precise token revocation
- Improved logging and auditing
- Compliance with security best practices