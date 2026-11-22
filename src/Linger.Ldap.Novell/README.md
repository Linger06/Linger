# Linger.Ldap.Novell

> Breaking changes and 2.0 migration notes are documented in the [Linger migration guide](../Linger/MIGRATION.md).

A cross-platform LDAP client implementation based on Novell.Directory.Ldap.

## Features

- Cross-platform LDAP access (Windows/Linux/macOS)
- Async user authentication and lookup APIs
- LDAPS support via `LdapConfig.Security`
- Configurable `SearchFilter` for `FindUserAsync` and `GetUsersAsync`
- Cross-provider advanced query via `ILdapClient.SearchUsersByFilterAsync`
- Optional `Attributes` projection to limit returned fields
- Configurable `MaxResults` limit for each query
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

var ldap = new NovellLdapClient(config);
```

For dependency injection, register `LdapConfig` with `services.Configure<LdapConfig>(...)` and `NovellLdapClient` as the `ILdapClient` implementation. The client consumes `IOptions<LdapConfig>` directly.

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

Only the `ValidateUserAsync` profile check requires RFC 4532 Who Am I. On the user-bound connection and within the requested search base, the client verifies one entry against the returned `dn:` (requires searchable `entryDN` or `distinguishedName`) or `u:DOMAIN\account` (requires readable `msDS-PrincipalName`). It does not follow referrals or aliases. Grant users permission to read their own required attributes; no service account is substituted.

For profile validation a correct password alone is insufficient. An unresolved, ambiguous, unreadable or out-of-scope identity does not authenticate: `(false, null)`. Do not retry via a business lookup to obtain an authenticated profile. Business queries retain their configured filters, wildcard searches and result limits.

For password-only checks use AD `IActiveDirectoryCredentialValidator.ValidateCredentials` or Novell `ILdapCredentialValidator.ValidateCredentialsAsync`. Neither requires a pinned controller, Who Am I, or profile read access, and neither checks OU or group membership. For OU restrictions or a verified profile, keep `ValidateUser` / `ValidateUserAsync`; never fall back to password-only validation on failure.

`LdapConfig.Security` defaults to `false`. AD validates with signed and sealed Negotiate by default. Novell requires explicit `Security = true` and a trusted LDAPS certificate for all user and service-account password binds. With `false` it only allows passwordless anonymous queries.

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

- `Url` is required for Novell provider (no automatic domain controller discovery).
- When `Security = true`, the client uses SSL and connects with default LDAPS port (`636`).
- `SearchFilter` is used by both `FindUserAsync` and `GetUsersAsync`; using `{0}` placeholder is recommended.
- `SearchUsersByFilterAsync` provides provider-agnostic advanced raw-filter queries.
- Blank usernames and raw filters are rejected to prevent accidental full-directory searches.
- If `SearchFilter` format is invalid, the implementation falls back to a default user filter.
- Input value in user search is escaped before building LDAP filter to reduce malformed/injection risk.
- Bind username normalization supports existing `domain\\user`, UPN (`user@domain`), and full DN forms.
- `Attributes` can be used to reduce payload and improve query performance.
- `MaxResults` must be greater than zero and limits each query; the default is `1000`.
- Configuration is snapshotted during client construction. Changes to the original `LdapConfig`, credentials, or attributes require a new client.
- Search-operation connection, TLS, bind, and server failures throw exceptions. An empty list only means that no users matched.

## Key User Properties (LdapUserInfo)

- `DisplayName`, `SamAccountName`, `Upn`, `Dn`
- `Email`, `TelephoneNumber`, `Mobile`, `Department`, `Title`
- `Company`, `Manager`, `WhenCreated`, `Status`, `PwdLastSet`
- `MemberOf`, `ProxyAddresses`, `OtherTelephone`, `ProfilePath`, `HomeDirectory`, `ExtensionAttribute1`

`MemberOf`, `ProxyAddresses`, and `OtherTelephone` preserve LDAP multi-values as string arrays.

## Dependencies

- Novell.Directory.Ldap.NETStandard
- Linger.Ldap.Contracts

## Related Packages

- [Linger.Ldap.Contracts](../Linger.Ldap.Contracts/): core LDAP interfaces and data models
- [Linger.Ldap.ActiveDirectory](../Linger.Ldap.ActiveDirectory/): Active Directory optimized implementation
