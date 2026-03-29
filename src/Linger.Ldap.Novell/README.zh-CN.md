# Linger.Ldap.Novell

基于 Novell.Directory.Ldap 的跨平台 LDAP 客户端实现。

## 功能特点

- 跨平台 LDAP 访问（Windows/Linux/macOS）
- 异步用户认证与查询 API
- 通过 `LdapConfig.Security` 启用 LDAPS
- `FindUserAsync` 与 `GetUsersAsync` 使用可配置 `SearchFilter`
- 通过 `ILdap.SearchUsersByFilterAsync` 提供跨提供者统一的高级过滤查询
- 支持 `Attributes` 属性投影，按需返回字段
- 内置 LDAP 过滤值转义，降低过滤器拼接风险

## 支持的框架

- .NET 10.0
- .NET 9.0
- .NET 8.0

## 安装

```shell
dotnet add package Linger.Ldap.Novell
```

## 快速开始

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

## 使用示例

### 验证用户账号密码

```csharp
var (isValid, userInfo) = await ldap.ValidateUserAsync("alice", "Password123!");

if (isValid && userInfo is not null)
{
    Console.WriteLine($"DisplayName: {userInfo.DisplayName}");
    Console.WriteLine($"Email: {userInfo.Email}");
}
```

### 查询单个用户

```csharp
var user = await ldap.FindUserAsync("alice");

if (user is not null)
{
    Console.WriteLine($"SamAccountName: {user.SamAccountName}");
    Console.WriteLine($"DN: {user.Dn}");
}
```

### 模糊搜索用户

```csharp
var users = await ldap.GetUsersAsync("alice");

foreach (var item in users)
{
    Console.WriteLine($"{item.DisplayName} ({item.Email})");
}
```

### 指定 OU 与自定义凭据查询

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

### 高级过滤查询（跨提供者统一）

```csharp
ILdap ldapContract = ldap;

var users = await ldapContract.SearchUsersByFilterAsync(
    "(&(objectClass=person)(department=IT)(mail=*))",
    searchBase: "DC=example,DC=com");
```

## 注意事项

- Novell 实现中 `Url` 为必填项，不支持自动发现域控。
- 当 `Security = true` 时，客户端启用 SSL 并使用默认 LDAPS 端口（`636`）。
- `SearchFilter` 会用于 `FindUserAsync` 与 `GetUsersAsync`，建议使用 `{0}` 作为查询值占位符。
- `SearchUsersByFilterAsync` 可用于跨提供者统一的原始过滤器高级查询。
- 若 `SearchFilter` 格式错误，内部会回退到默认用户过滤模板。
- 查询输入值会在构建 LDAP 过滤器前进行转义，降低格式破坏和注入风险。
- 绑定用户名会先规范化：已是 `domain\\user`、UPN（`user@domain`）或完整 DN 时不会重复拼接域前缀。
- `Attributes` 可用于减少返回字段，提升查询性能。

## 常用用户属性（AdUserInfo）

- `DisplayName`、`SamAccountName`、`Upn`、`Dn`
- `Email`、`TelephoneNumber`、`Mobile`、`Department`、`Title`
- `Company`、`Manager`、`WhenCreated`、`Status`、`PwdLastSet`
- `MemberOf`、`ProfilePath`、`HomeDirectory`、`ExtensionAttribute1`

## 依赖项

- Novell.Directory.Ldap.NETStandard
- Linger.Ldap.Contracts

## 相关包

- [Linger.Ldap.Contracts](../Linger.Ldap.Contracts/)：核心 LDAP 接口与数据模型
- [Linger.Ldap.ActiveDirectory](../Linger.Ldap.ActiveDirectory/)：面向 Active Directory 的实现
