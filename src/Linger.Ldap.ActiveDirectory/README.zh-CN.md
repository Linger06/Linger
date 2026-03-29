# Linger.Ldap.ActiveDirectory

基于 System.DirectoryServices 的 Active Directory LDAP 客户端实现。

## 功能特点

- Active Directory 用户身份验证与验证
- 异步用户查询与搜索 API
- 通过 `searchBase` 支持 OU 范围查询
- 支持 `Attributes` 属性投影，按需返回字段
- 通过 `LdapConfig.Security` 启用 LDAPS
- `FindUserAsync` 与 `GetUsersAsync` 使用可配置 `SearchFilter`
- 通过 `ILdap.SearchUsersByFilterAsync` 提供跨提供者统一的高级过滤查询
- 当 `LdapConfig.Url` 为空时可自动发现域控制器
- 提供构造函数重载便捷入口（`new Ldap()` / `new Ldap(logger)`），可自动补齐默认值（尤其 `SearchBase`）

## 支持的框架

- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0

## 安装

```shell
dotnet add package Linger.Ldap.ActiveDirectory
```

## 快速开始

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

### 便捷创建（自动补齐默认值）

```csharp
var ldap = new Ldap();

// 使用自定义 logger
var ldapWithLogger = new Ldap(logger);
```

当 `SearchBase` 为空时，便捷构造函数重载会优先根据 `Domain` 推断（例如 `example.com` -> `DC=example,DC=com`），再回退到当前域的 `distinguishedName`。

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

### 搜索用户

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

### Active Directory 专有：获取 DirectoryEntry

```csharp
using var entry = ldap.GetEntryByUsername("alice");
var nativeObject = entry.NativeObject;
var properties = entry.Properties;
```

## 注意事项

- 该实现面向 Windows 环境，依赖 `System.DirectoryServices`。
- 在 .NET 5+ 下实现带有 `[SupportedOSPlatform("windows")]` 标注。
- 当 `Security = true` 时，客户端使用 LDAPS（`LDAPS://`）和安全绑定选项。
- `SearchFilter` 会用于 `FindUserAsync` 与 `GetUsersAsync`，建议使用 `{0}` 占位符。
- `SearchUsersByFilterAsync` 可用于跨提供者统一的原始过滤器高级查询。
- 若 `SearchFilter` 格式错误，内部会回退到默认用户过滤模板。
- 查询输入值会在构建 LDAP 过滤器前转义，降低格式破坏和注入风险。
- 绑定用户名会先规范化：已是 `domain\\user`、UPN（`user@domain`）或完整 DN 时不会重复拼接域前缀。
- 当 `LdapConfig.Url` 为空时，ActiveDirectory 实现会尝试自动发现域控制器。
- 便捷构造函数重载可自动补齐缺失的 `Domain` 与 `SearchBase` 默认值。

## 常用用户属性（AdUserInfo）

- `DisplayName`、`SamAccountName`、`Upn`、`Dn`
- `Email`、`TelephoneNumber`、`Mobile`、`Department`、`Title`
- `Company`、`Manager`、`WhenCreated`、`Status`、`PwdLastSet`
- `MemberOf`、`ProfilePath`、`HomeDirectory`、`ExtensionAttribute1`

## 依赖项

- System.DirectoryServices
- System.DirectoryServices.AccountManagement
- Linger.Ldap.Contracts

## 相关包

- [Linger.Ldap.Contracts](../Linger.Ldap.Contracts/)：核心 LDAP 接口与数据模型
- [Linger.Ldap.Novell](../Linger.Ldap.Novell/)：基于 Novell 库的跨平台 LDAP 实现
