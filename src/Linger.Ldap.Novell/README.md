# Linger.Ldap.Novell

A cross-platform LDAP client implementation based on Novell.Directory.Ldap.

## Features

- Cross-platform LDAP access (Windows/Linux/macOS)
- Async user authentication and lookup APIs
- LDAPS support via `LdapConfig.Security`
- Configurable `SearchFilter` for `FindUserAsync` and `GetUsersAsync`
- Cross-provider advanced query via `ILdap.SearchUsersByFilterAsync`
- Optional `Attributes` projection to limit returned fields
- Built-in LDAP filter value escaping for safer search input

## Supported Frameworks

- .NET 10.0
- .NET 9.0
- .NET 8.0

## Installation

```shell
dotnet add package Linger.Ldap.Novell
```

## Quick Start

```csharp
using Linger.Ldap.Contracts;
using Linger.Ldap.Novell;

var config = new LdapConfig
{
    Url = "ldap.example.com",
    Domain = "example",
    SearchBase = "DC=example,DC=com",
    SearchFilter = "(&(objectClass=person)(|(uid={0})(sAMAccountName={0})(mail={0})))",
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

using var ldap = new Ldap(config);
```

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

## Notes

- `Url` is required for Novell provider (no automatic domain controller discovery).
- When `Security = true`, the client uses SSL and connects with default LDAPS port (`636`).
- `SearchFilter` is used by both `FindUserAsync` and `GetUsersAsync`; using `{0}` placeholder is recommended.
- `SearchUsersByFilterAsync` provides provider-agnostic advanced raw-filter queries.
- If `SearchFilter` format is invalid, the implementation falls back to a default user filter.
- Input value in user search is escaped before building LDAP filter to reduce malformed/injection risk.
- Bind username normalization supports existing `domain\\user`, UPN (`user@domain`), and full DN forms.
- `Attributes` can be used to reduce payload and improve query performance.

## Key User Properties (AdUserInfo)

- `DisplayName`, `SamAccountName`, `Upn`, `Dn`
- `Email`, `TelephoneNumber`, `Mobile`, `Department`, `Title`
- `Company`, `Manager`, `WhenCreated`, `Status`, `PwdLastSet`
- `MemberOf`, `ProfilePath`, `HomeDirectory`, `ExtensionAttribute1`

## Dependencies

- Novell.Directory.Ldap.NETStandard
- Linger.Ldap.Contracts

## Related Packages

- [Linger.Ldap.Contracts](../Linger.Ldap.Contracts/): core LDAP interfaces and data models
- [Linger.Ldap.ActiveDirectory](../Linger.Ldap.ActiveDirectory/): Active Directory optimized implementation
