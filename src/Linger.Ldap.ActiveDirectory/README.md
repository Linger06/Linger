# Linger.Ldap.ActiveDirectory

An Active Directory focused LDAP client implementation based on System.DirectoryServices.

## Features

- Active Directory user authentication and validation
- Async user lookup and search APIs
- OU-scoped search support via `searchBase`
- Optional `Attributes` projection to limit returned fields
- LDAPS support via `LdapConfig.Security`
- Configurable `SearchFilter` for `FindUserAsync` and `GetUsersAsync`
- Cross-provider advanced query via `ILdapClient.SearchUsersByFilterAsync`
- Optional domain controller auto-discovery when `LdapConfig.Url` is empty
- Parameterless construction for the current Windows domain
- Configurable `MaxResults` limit for each query

## Supported Frameworks

- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0

## Installation

```shell
dotnet add package Linger.Ldap.ActiveDirectory
```

## Quick Start

```csharp
using Linger.Ldap.ActiveDirectory;
using Linger.Ldap.Contracts;

var config = new LdapConfig
{
    Url = "example.com",
    Domain = "example",
    SearchBase = "DC=example,DC=com",
    SearchFilter = "(&(objectClass=user)(sAMAccountName={0}))",
    Security = true,
    MaxResults = 1000,
    Credentials = new LdapCredentials
    {
        BindDn = "serviceAccount",
        BindCredentials = "SecurePassword123!"
    },
    Attributes =
    [
        "displayName",
        "sAMAccountName",
        "mail",
        "department",
        "memberOf"
    ]
};

var ldap = new AdLdapClient(config);
```

For the current Windows domain, configuration is optional:

```csharp
var ldap = new AdLdapClient();
```

The parameterless constructor performs no network I/O. It uses the current Windows identity and discovers a domain controller when the first LDAP operation starts.

For dependency injection, register `LdapConfig` with `services.Configure<LdapConfig>(...)` and `AdLdapClient` as the `ILdapClient` implementation. The client consumes `IOptions<LdapConfig>` directly.

## Usage

### Validate User Credentials

```csharp
var (isValid, userInfo) = await ldap.ValidateUserAsync("alice", "Password123!");

if (isValid && userInfo is not null)
{
    Console.WriteLine($"DisplayName: {userInfo.DisplayName}");
    Console.WriteLine($"Email: {userInfo.Email}");
}
```

Authentication is determined by `ValidateCredentials`. User information is queried separately with the configured search credentials. If that query is unavailable or unauthorized, the method returns `IsValid = true` and `userInfo = null`.

### Find a Single User

```csharp
var user = await ldap.FindUserAsync("alice");

if (user is not null)
{
    Console.WriteLine($"SamAccountName: {user.SamAccountName}");
    Console.WriteLine($"DN: {user.Dn}");
}
```

### Search Users

```csharp
var users = await ldap.GetUsersAsync("alice");

foreach (var item in users)
{
    Console.WriteLine($"{item.DisplayName} ({item.Email})");
}
```

### Search in a Specific OU with Custom Bind Credentials

```csharp
var customCreds = new LdapCredentials
{
    BindDn = "readonly.user",
    BindCredentials = "ReadonlyPassword123!"
};

var usersInOu = await ldap.GetUsersAsync(
    "alice",
    ldapCredentials: customCreds,
    searchBase: "OU=Sales,DC=example,DC=com");
```

### Advanced Filter Search (Cross-Provider)

```csharp
ILdapClient ldapContract = ldap;

var users = await ldapContract.SearchUsersByFilterAsync(
    "(&(objectClass=person)(department=IT)(mail=*))",
    searchBase: "DC=example,DC=com");
```

## Notes

- This implementation is intended for Windows environments using `System.DirectoryServices`.
- In .NET 5+, the implementation is marked with `[SupportedOSPlatform("windows")]`.
- When `Security = true`, the client enables `AuthenticationTypes.SecureSocketsLayer` on the ADSI connection.
- `SearchFilter` is used by both `FindUserAsync` and `GetUsersAsync`; using `{0}` placeholder is recommended.
- `SearchUsersByFilterAsync` provides provider-agnostic advanced raw-filter queries.
- Blank usernames and raw filters are rejected to prevent accidental full-directory searches.
- Cancellation is observed before and after synchronous `System.DirectoryServices` calls, but an in-flight directory query cannot be interrupted.
- If `SearchFilter` format is invalid, the implementation falls back to a default user filter.
- Input value in user search is escaped before building LDAP filter to reduce malformed/injection risk.
- Bind username normalization supports existing `domain\\user`, UPN (`user@domain`), and full DN forms.
- If `LdapConfig.Url` is empty, Active Directory provider attempts domain controller auto-discovery.
- The parameterless constructor leaves `SearchBase` empty and searches from the discovered server root. `SearchBase` is not inferred; use configured construction to restrict the search scope.
- `MaxResults` must be greater than zero and limits each query; the default is `1000`.
- Configuration is snapshotted during client construction. Changes to the original `LdapConfig`, credentials, or attributes require a new client.
- Search-operation connection, bind, and directory-server failures throw exceptions. An empty list only means that no users matched.

## Key User Properties (LdapUserInfo)

- `DisplayName`, `SamAccountName`, `Upn`, `Dn`
- `Email`, `TelephoneNumber`, `Mobile`, `Department`, `Title`
- `Company`, `Manager`, `WhenCreated`, `Status`, `PwdLastSet`
- `MemberOf`, `ProxyAddresses`, `OtherTelephone`, `ProfilePath`, `HomeDirectory`, `ExtensionAttribute1`

`MemberOf`, `ProxyAddresses`, and `OtherTelephone` preserve LDAP multi-values as string arrays.

## Dependencies

- System.DirectoryServices
- System.DirectoryServices.AccountManagement
- Linger.Ldap.Contracts

## Related Packages

- [Linger.Ldap.Contracts](../Linger.Ldap.Contracts/): core LDAP interfaces and data models
- [Linger.Ldap.Novell](../Linger.Ldap.Novell/): cross-platform LDAP implementation based on Novell library

