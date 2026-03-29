# Linger.Ldap.ActiveDirectory

An Active Directory focused LDAP client implementation based on System.DirectoryServices.

## Features

- Active Directory user authentication and validation
- Async user lookup and search APIs
- OU-scoped search support via `searchBase`
- Optional `Attributes` projection to limit returned fields
- LDAPS support via `LdapConfig.Security`
- Configurable `SearchFilter` for `FindUserAsync` and `GetUsersAsync`
- Cross-provider advanced query via `ILdap.SearchUsersByFilterAsync`
- Optional domain controller auto-discovery when `LdapConfig.Url` is empty
- Convenience constructor overloads (`new Ldap()` / `new Ldap(logger)`) for auto-filling defaults (especially `SearchBase`)

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

var ldap = new Ldap(config);
```

### Convenience Creation with Auto Defaults

```csharp
var ldap = new Ldap();

// With custom logger
var ldapWithLogger = new Ldap(logger);
```

When `SearchBase` is empty, the convenience constructor overloads infer it from `Domain` (for example, `example.com` -> `DC=example,DC=com`) and then fall back to the current domain distinguished name.

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
ILdap ldapContract = ldap;

var users = await ldapContract.SearchUsersByFilterAsync(
    "(&(objectClass=person)(department=IT)(mail=*))",
    searchBase: "DC=example,DC=com");
```

### Active Directory Specific: Get DirectoryEntry

```csharp
using var entry = ldap.GetEntryByUsername("alice");
var nativeObject = entry.NativeObject;
var properties = entry.Properties;
```

## Notes

- This implementation is intended for Windows environments using `System.DirectoryServices`.
- In .NET 5+, the implementation is marked with `[SupportedOSPlatform("windows")]`.
- When `Security = true`, the client uses LDAPS (`LDAPS://`) and secure bind options.
- `SearchFilter` is used by both `FindUserAsync` and `GetUsersAsync`; using `{0}` placeholder is recommended.
- `SearchUsersByFilterAsync` provides provider-agnostic advanced raw-filter queries.
- If `SearchFilter` format is invalid, the implementation falls back to a default user filter.
- Input value in user search is escaped before building LDAP filter to reduce malformed/injection risk.
- Bind username normalization supports existing `domain\\user`, UPN (`user@domain`), and full DN forms.
- If `LdapConfig.Url` is empty, Active Directory provider attempts domain controller auto-discovery.
- Convenience constructor overloads can auto-fill missing `Domain` and `SearchBase` defaults.

## Key User Properties (AdUserInfo)

- `DisplayName`, `SamAccountName`, `Upn`, `Dn`
- `Email`, `TelephoneNumber`, `Mobile`, `Department`, `Title`
- `Company`, `Manager`, `WhenCreated`, `Status`, `PwdLastSet`
- `MemberOf`, `ProfilePath`, `HomeDirectory`, `ExtensionAttribute1`

## Dependencies

- System.DirectoryServices
- System.DirectoryServices.AccountManagement
- Linger.Ldap.Contracts

## Related Packages

- [Linger.Ldap.Contracts](../Linger.Ldap.Contracts/): core LDAP interfaces and data models
- [Linger.Ldap.Novell](../Linger.Ldap.Novell/): cross-platform LDAP implementation based on Novell library

