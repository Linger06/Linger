# Linger.Ldap.ActiveDirectory

基于 System.DirectoryServices 的 Active Directory LDAP 客户端实现。

## 功能特点

- Active Directory 用户身份验证与验证
- 异步用户查询与搜索 API
- 通过 `searchBase` 支持 OU 范围查询
- 支持 `Attributes` 属性投影，按需返回字段
- 通过 `LdapConfig.Security` 启用 LDAPS
- `FindUserAsync` 与 `GetUsersAsync` 使用可配置 `SearchFilter`
- 通过 `ILdapClient.SearchUsersByFilterAsync` 提供跨提供者统一的高级过滤查询
- 当 `LdapConfig.Url` 为空时可自动发现域控制器
- 支持通过无参构造使用当前 Windows 域
- 支持通过 `MaxResults` 限制每次查询的结果数量

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

使用当前 Windows 域时，可以省略配置：

```csharp
var ldap = new AdLdapClient();
```

无参构造本身不会执行网络 I/O。客户端使用当前 Windows 身份，并在首次 LDAP 操作开始时发现域控制器。

使用依赖注入时，通过 `services.Configure<LdapConfig>(...)` 注册配置，并将 `AdLdapClient` 注册为 `ILdapClient` 实现。客户端可直接接收 `IOptions<LdapConfig>`。

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

认证结果只由 `ValidateCredentials` 决定。用户信息会使用配置的查询凭据单独获取；如果查询不可用或没有权限，方法会返回 `IsValid = true` 和 `userInfo = null`。

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
ILdapClient ldapContract = ldap;

var users = await ldapContract.SearchUsersByFilterAsync(
    "(&(objectClass=person)(department=IT)(mail=*))",
    searchBase: "DC=example,DC=com");
```

## 注意事项

- 该实现面向 Windows 环境，依赖 `System.DirectoryServices`。
- 在 .NET 5+ 下实现带有 `[SupportedOSPlatform("windows")]` 标注。
- 当 `Security = true` 时，客户端在 ADSI 连接上启用 `AuthenticationTypes.SecureSocketsLayer`。
- `SearchFilter` 会用于 `FindUserAsync` 与 `GetUsersAsync`，建议使用 `{0}` 占位符。
- `SearchUsersByFilterAsync` 可用于跨提供者统一的原始过滤器高级查询。
- 空白用户名和原始过滤器会被拒绝，避免意外执行全目录查询。
- 取消会在同步 `System.DirectoryServices` 调用前后生效，但无法中断已经开始的目录查询。
- 若 `SearchFilter` 格式错误，内部会回退到默认用户过滤模板。
- 查询输入值会在构建 LDAP 过滤器前转义，降低格式破坏和注入风险。
- 绑定用户名会先规范化：已是 `domain\\user`、UPN（`user@domain`）或完整 DN 时不会重复拼接域前缀。
- 当 `LdapConfig.Url` 为空时，ActiveDirectory 实现会尝试自动发现域控制器。
- 无参构造会保持 `SearchBase` 为空，并从发现的服务器根节点开始查询。客户端不会推断 `SearchBase`；如需限制查询范围，应使用配置构造。
- `MaxResults` 必须大于零，用于限制每次查询的结果数量，默认值为 `1000`。
- Client 构造时会生成配置快照。修改原始 `LdapConfig`、凭据或属性列表后，需要创建新的 Client 才会生效。
- 搜索操作中的连接、绑定和目录服务器故障会抛出异常。空列表只表示没有匹配用户。

## 常用用户属性（LdapUserInfo）

- `DisplayName`、`SamAccountName`、`Upn`、`Dn`
- `Email`、`TelephoneNumber`、`Mobile`、`Department`、`Title`
- `Company`、`Manager`、`WhenCreated`、`Status`、`PwdLastSet`
- `MemberOf`、`ProxyAddresses`、`OtherTelephone`、`ProfilePath`、`HomeDirectory`、`ExtensionAttribute1`

`MemberOf`、`ProxyAddresses` 和 `OtherTelephone` 会以字符串数组保留 LDAP 多值属性。

## 依赖项

- System.DirectoryServices
- System.DirectoryServices.AccountManagement
- Linger.Ldap.Contracts

## 相关包

- [Linger.Ldap.Contracts](../Linger.Ldap.Contracts/)：核心 LDAP 接口与数据模型
- [Linger.Ldap.Novell](../Linger.Ldap.Novell/)：基于 Novell 库的跨平台 LDAP 实现
