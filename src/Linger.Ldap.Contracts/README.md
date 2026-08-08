# Linger.Ldap.Contracts

Core contracts and shared models for LDAP operations in .NET applications.

## Introduction

Linger.Ldap.Contracts defines provider-agnostic abstractions so application code can work with a single LDAP API while switching between concrete implementations.

## Supported Frameworks

- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0

## What This Package Contains

- `ILdapClient` interface for core LDAP operations
- `LdapConfig` and `LdapCredentials` configuration models
- `LdapUserInfo` unified user profile model

## ASP.NET Core Integration

### Configure Services

Register your LDAP configuration and one concrete provider implementation.

```csharp
using Linger.Ldap.Contracts;

public void ConfigureServices(IServiceCollection services)
{
    services.Configure<LdapConfig>(Configuration.GetSection("LdapConfig"));

    // Choose exactly one provider implementation
    services.AddScoped<ILdapClient, Linger.Ldap.ActiveDirectory.AdLdapClient>();
    // services.AddScoped<ILdapClient, Linger.Ldap.Novell.NovellLdapClient>();
}
```

### appsettings.json Example

```json
{
  "LdapConfig": {
    "Url": "ldap.example.com",
    "Domain": "example",
    "SearchBase": "DC=example,DC=com",
    "SearchFilter": "(&(objectClass=user)(|(sAMAccountName={0})(userPrincipalName={0})(mail={0})))",
    "Security": true,
    "MaxResults": 1000,
    "Credentials": {
      "BindDn": "serviceaccount",
      "BindCredentials": "password"
    },
    "Attributes": [
      "displayName",
      "mail",
      "sAMAccountName",
      "userPrincipalName",
      "telephoneNumber",
      "department"
    ]
  }
}
```

## Usage Examples

### Validate User Credentials

```csharp
public class AuthenticationService
{
    private readonly ILdapClient _ldap;

    public AuthenticationService(ILdapClient ldap)
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
            cancellationToken: cancellationToken);

        if (isValid && userInfo is not null)
        {
            Console.WriteLine($"User {userInfo.DisplayName} authenticated successfully.");
            return true;
        }

        return false;
    }
}
```

### Find and Search Users

```csharp
public class UserService
{
    private readonly ILdapClient _ldap;

    public UserService(ILdapClient ldap)
    {
        _ldap = ldap;
    }

    public async Task<LdapUserInfo?> GetUserInfoAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return await _ldap.FindUserAsync(
            username,
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<LdapUserInfo>> SearchUsersAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        return await _ldap.GetUsersAsync(
            searchTerm,
            cancellationToken: cancellationToken);
    }

    public async Task<bool> CheckUserExistsAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return await _ldap.UserExistsAsync(
            username,
            cancellationToken: cancellationToken);
    }
}
```

### Search in a Specific OU with Custom Bind Credentials

```csharp
var ldapCredentials = new LdapCredentials
{
    BindDn = "readonly.user",
    BindCredentials = "ReadonlyPassword123!"
};

var users = await _ldap.GetUsersAsync(
    "alice",
    ldapCredentials: ldapCredentials,
    searchBase: "OU=Sales,DC=example,DC=com",
    cancellationToken: cancellationToken);
```

### Advanced Filter Search (Cross-Provider)

```csharp
var advancedFilter = "(&(objectClass=person)(department=IT)(mail=*))";

var users = await _ldap.SearchUsersByFilterAsync(
    advancedFilter,
    searchBase: "DC=example,DC=com",
    cancellationToken: cancellationToken);
```

## Cancellation Support

All asynchronous LDAP operations accept `CancellationToken`. The Novell provider forwards cancellation to its native asynchronous LDAP calls. The Active Directory provider is backed by synchronous `System.DirectoryServices` APIs: it observes cancellation before and after those calls, but cannot interrupt an LDAP query already in progress.

`FindUserAsync` and `GetUsersAsync` reject blank usernames, and `SearchUsersByFilterAsync` rejects blank filters. Use an explicit raw LDAP filter when a broad query is intentional.

```csharp
public async Task<bool> ValidateUserWithTimeoutAsync(
    ILdapClient ldap,
    string username,
    string password,
    int timeoutSeconds = 5)
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

    try
    {
        var (isValid, _) = await ldap.ValidateUserAsync(
            username,
            password,
            cancellationToken: cts.Token);
        return isValid;
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("LDAP validation timed out.");
        return false;
    }
}
```

## Core Interface

```csharp
public interface ILdapClient
{
    Task<(bool IsValid, LdapUserInfo? LdapUserInfo)> ValidateUserAsync(
        string userName,
        string password,
        string? searchBase = null,
        CancellationToken cancellationToken = default);

    Task<LdapUserInfo?> FindUserAsync(
        string userName,
        LdapCredentials? ldapCredentials = null,
        string? searchBase = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LdapUserInfo>> GetUsersAsync(
        string userName,
        LdapCredentials? ldapCredentials = null,
        string? searchBase = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LdapUserInfo>> SearchUsersByFilterAsync(
        string filter,
        LdapCredentials? ldapCredentials = null,
        string? searchBase = null,
        CancellationToken cancellationToken = default);

    Task<bool> UserExistsAsync(
        string userName,
        string? searchBase = null,
        CancellationToken cancellationToken = default);
}
```

## Core Models

### LdapConfig

```csharp
public class LdapConfig
{
    public string Url { get; set; } = null!;
    public bool Security { get; set; }
    public string Domain { get; set; } = null!;
    public LdapCredentials? Credentials { get; set; }
    public string SearchBase { get; set; } = null!;
    public string SearchFilter { get; set; } = null!;
    public string[]? Attributes { get; set; }
    public int MaxResults { get; set; } = 1000;
}
```

Providers take an independent snapshot of `LdapConfig`, including `Credentials` and `Attributes`, during client construction. Later changes to the source configuration do not affect an existing client; create a new client to apply new configuration.

### LdapUserInfo (Commonly Used Fields)

- `DisplayName`
- `SamAccountName`
- `Upn`
- `Dn`
- `Email`
- `Department`
- `Title`
- `MemberOf`
- `ProxyAddresses`
- `OtherTelephone`
- `Status`

`MemberOf`, `ProxyAddresses`, and `OtherTelephone` are string arrays because LDAP can return multiple values for these attributes.

## Error Behavior

Invalid arguments and configuration are rejected before use. Search-operation connection, TLS, bind, and directory-server failures are surfaced as exceptions; an empty result means that the query completed successfully without matching users. Credential validation returns `IsValid = false` for invalid credentials. After successful authentication, a provider can return `IsValid = true` with no profile when the separate profile lookup fails.

## Supported Implementations

- [Linger.Ldap.ActiveDirectory](../Linger.Ldap.ActiveDirectory/): implementation optimized for Microsoft Active Directory
- [Linger.Ldap.Novell](../Linger.Ldap.Novell/): cross-platform implementation based on Novell.Directory.Ldap
