# Linger.Ldap.Contracts

A C# LDAP contract library that provides standardized interfaces and models for integrating LDAP directory services across multiple .NET platforms.

## Introduction

Linger.Ldap.Contracts provides a set of standardized LDAP operation interfaces and models, making it easier to implement consistent LDAP functionality across different .NET applications.

## Features

### Core Contracts
- Standardized LDAP operation interfaces
- Common LDAP attribute definitions
- Cross-platform compatible models
- Type-safe LDAP operations

### Model Support
- Comprehensive user attribute mappings
- Groups and organizational unit models
- Search filter definitions
- Connection parameter contracts

## ASP.NET Core Integration

### Configuring Services

In ASP.NET Core projects, you can utilize LDAP services through dependency injection:

```csharp
// Configure services in Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Add LDAP configuration
    services.Configure<LdapConfig>(Configuration.GetSection("LdapConfig"));
    
    // Register LDAP service depending on the implementation
    // For Active Directory
    services.AddScoped<ILdap, Linger.Ldap.ActiveDirectory.Ldap>();
    
    // Or for Novell LDAP
    // services.AddScoped<ILdap, Linger.Ldap.Novell.Ldap>();
}
```

### appsettings.json example

```json
{
  "LdapConfig": {
    "Url": "ldap.example.com",
    "Domain": "example",
    "SearchBase": "DC=example,DC=com",
    "SearchFilter": "(&(objectClass=user)(|(sAMAccountName={0})(userPrincipalName={0})(mail={0})))",
    "Security": true,
    "Credentials": {
      "BindDn": "serviceaccount",
      "BindCredentials": "password"
    },
    "Attributes": [
      "displayName", "mail", "sAMAccountName", "userPrincipalName", 
      "telephoneNumber", "department", "title", "givenName", "sn"
    ]
  }
}
```

### Usage examples

#### User authentication

```csharp
public class AuthenticationService
{
    private readonly ILdap _ldap;
    
    public AuthenticationService(ILdap ldap)
    {
        _ldap = ldap;
    }
    
    public async Task<bool> AuthenticateUserAsync(
        string username, 
        string password, 
        CancellationToken cancellationToken = default)
    {
        var (isValid, userInfo) = await _ldap.ValidateUserAsync(
            username, 
            password, 
            cancellationToken);
        
        if (isValid && userInfo != null)
        {
            // User authenticated successfully; you can use information from userInfo
            Console.WriteLine($"User {userInfo.DisplayName} authenticated successfully");
            return true;
        }
        
        return false;
    }
}
```

#### Find user information

```csharp
public class UserService
{
    private readonly ILdap _ldap;
    
    public UserService(ILdap ldap)
    {
        _ldap = ldap;
    }
    
    public async Task<AdUserInfo?> GetUserInfoAsync(
        string username, 
        CancellationToken cancellationToken = default)
    {
        return await _ldap.FindUserAsync(username, cancellationToken);
    }
    
    public async Task<IEnumerable<AdUserInfo>> SearchUsersAsync(
        string searchTerm, 
        CancellationToken cancellationToken = default)
    {
        return await _ldap.GetUsersAsync(searchTerm, cancellationToken);
    }
    
    public async Task<bool> CheckUserExistsAsync(
        string username, 
        CancellationToken cancellationToken = default)
    {
        return await _ldap.UserExistsAsync(username, cancellationToken);
    }
}
```

### Cancellation support

All asynchronous LDAP operations support `CancellationToken` for timeout control and request cancellation:

```csharp
public class LdapService
{
    private readonly ILdap _ldap;
    
    public LdapService(ILdap ldap)
    {
        _ldap = ldap;
    }
    
    // User validation with timeout
    public async Task<bool> ValidateUserWithTimeoutAsync(
        string username, 
        string password, 
        int timeoutSeconds = 5)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        
        try
        {
            var (isValid, _) = await _ldap.ValidateUserAsync(
                username, 
                password, 
                cts.Token);
            return isValid;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("LDAP validation timed out");
            return false;
        }
    }
    
    // Using request cancellation token in ASP.NET Core
    public async Task<AdUserInfo?> GetUserForRequestAsync(
        string username, 
        CancellationToken requestCancellationToken)
    {
        // If the client disconnects, the operation will be automatically canceled
        return await _ldap.FindUserAsync(username, requestCancellationToken);
    }
}
```

## Supported implementations

The library provides the following LDAP directory service implementations:

- **Linger.Ldap.ActiveDirectory** - Implementation for Microsoft Active Directory
- **Linger.Ldap.Novell** - Cross-platform implementation using the Novell LDAP client library