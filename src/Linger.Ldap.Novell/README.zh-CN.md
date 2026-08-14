# Linger.Ldap.Novell

> 破坏性变更与 2.0 迁移说明见 [Linger 迁移指南](../Linger/MIGRATION.zh-CN.md)。

基于 Novell.Directory.Ldap 的跨平台 LDAP 客户端实现。

## 功能特点

- 跨平台 LDAP 访问（Windows/Linux/macOS）
- 异步用户认证与查询 API
- 通过 `LdapConfig.Security` 启用 LDAPS
- `FindUserAsync` 与 `GetUsersAsync` 使用可配置 `SearchFilter`
- 通过 `ILdapClient.SearchUsersByFilterAsync` 提供跨提供者统一的高级过滤查询
- 支持 `Attributes` 属性投影，按需返回字段
- 支持通过 `MaxResults` 限制每次查询的结果数量
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

使用依赖注入时，通过 `services.Configure<LdapConfig>(...)` 注册配置，并将 `NovellLdapClient` 注册为 `ILdapClient` 实现。客户端可直接接收 `IOptions<LdapConfig>`。

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

认证结果只由用户绑定决定。用户信息会使用配置的查询凭据单独获取；如果查询不可用或没有权限，方法会返回 `IsValid = true` 和 `userInfo = null`。

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
ILdapClient ldapContract = ldap;

var users = await ldapContract.SearchUsersByFilterAsync(
    "(&(objectClass=person)(department=IT)(mail=*))",
    searchBase: "DC=example,DC=com");
```

## 注意事项

- Novell 实现中 `Url` 为必填项，不支持自动发现域控。
- 当 `Security = true` 时，客户端启用 SSL 并使用默认 LDAPS 端口（`636`）。
- `SearchFilter` 会用于 `FindUserAsync` 与 `GetUsersAsync`，建议使用 `{0}` 作为查询值占位符。
- `SearchUsersByFilterAsync` 可用于跨提供者统一的原始过滤器高级查询。
- 空白用户名和原始过滤器会被拒绝，避免意外执行全目录查询。
- 若 `SearchFilter` 格式错误，内部会回退到默认用户过滤模板。
- 查询输入值会在构建 LDAP 过滤器前进行转义，降低格式破坏和注入风险。
- 绑定用户名会先规范化：已是 `domain\\user`、UPN（`user@domain`）或完整 DN 时不会重复拼接域前缀。
- `Attributes` 可用于减少返回字段，提升查询性能。
- `MaxResults` 必须大于零，用于限制每次查询的结果数量，默认值为 `1000`。
- Client 构造时会生成配置快照。修改原始 `LdapConfig`、凭据或属性列表后，需要创建新的 Client 才会生效。
- 搜索操作中的连接、TLS、绑定和服务器故障会抛出异常。空列表只表示没有匹配用户。

## 常用用户属性（LdapUserInfo）

- `DisplayName`、`SamAccountName`、`Upn`、`Dn`
- `Email`、`TelephoneNumber`、`Mobile`、`Department`、`Title`
- `Company`、`Manager`、`WhenCreated`、`Status`、`PwdLastSet`
- `MemberOf`、`ProxyAddresses`、`OtherTelephone`、`ProfilePath`、`HomeDirectory`、`ExtensionAttribute1`

`MemberOf`、`ProxyAddresses` 和 `OtherTelephone` 会以字符串数组保留 LDAP 多值属性。

## 依赖项

- Novell.Directory.Ldap.NETStandard
- Linger.Ldap.Contracts

## 相关包

- [Linger.Ldap.Contracts](../Linger.Ldap.Contracts/)：核心 LDAP 接口与数据模型
- [Linger.Ldap.ActiveDirectory](../Linger.Ldap.ActiveDirectory/)：面向 Active Directory 的实现
