# Linger.Ldap.Contracts

用于 .NET 应用的 LDAP 核心契约与共享模型包。

## 介绍

Linger.Ldap.Contracts 按提供程序的真实能力分别定义契约：`ILdapClient` 表示原生异步 LDAP 操作，`IActiveDirectoryClient` 表示同步 Windows Active Directory 操作。

## 支持的框架

- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0

## 包含内容

- `ILdapClient`：原生异步 LDAP 操作接口
- `IActiveDirectoryClient`：同步 Windows Active Directory 操作接口
- `LdapConfig`、`LdapCredentials`：连接配置模型
- `LdapUserInfo`：统一用户信息模型

## ASP.NET Core 集成

### 配置服务

在 DI 中注册与所选提供程序匹配的契约；应用同时使用两个提供程序时可以同时注册：

```csharp
using Linger.Ldap.Contracts;

public void ConfigureServices(IServiceCollection services)
{
    services.Configure<LdapConfig>(Configuration.GetSection("LdapConfig"));

    services.AddScoped<ILdapClient, Linger.Ldap.Novell.NovellLdapClient>();
    services.AddScoped<IActiveDirectoryClient, Linger.Ldap.ActiveDirectory.AdLdapClient>();
}
```

### appsettings.json 示例

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

## 使用示例

### 验证用户账号密码

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
            Console.WriteLine($"用户 {userInfo.DisplayName} 验证成功。");
            return true;
        }

        return false;
    }
}
```

### 查询用户

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

### 指定 OU 与自定义凭据查询

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

### 高级过滤查询（跨提供者统一）

```csharp
var advancedFilter = "(&(objectClass=person)(department=IT)(mail=*))";

var users = await _ldap.SearchUsersByFilterAsync(
    advancedFilter,
    searchBase: "DC=example,DC=com",
    cancellationToken: cancellationToken);
```

## 取消操作支持

所有 `ILdapClient` 操作都接受 `CancellationToken`，Novell 提供程序会将取消传递到原生异步 LDAP 调用。`IActiveDirectoryClient` 刻意保持同步，因为 `System.DirectoryServices` 不提供异步搜索操作。

`FindUserAsync` 和 `GetUsersAsync` 会拒绝空白用户名，`SearchUsersByFilterAsync` 会拒绝空白过滤器。如果确实需要宽泛查询，请显式传入原始 LDAP 过滤器。

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
        Console.WriteLine("LDAP 验证超时。");
        return false;
    }
}
```

## 核心接口

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

## 核心模型

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

`IActiveDirectoryClient` 提供对应的同步方法：`ValidateUser`、`FindUser`、`GetUsers`、`SearchUsersByFilter` 和 `UserExists`，不返回 `Task`，也不接收 `CancellationToken`。

提供者会在 Client 构造时生成独立的 `LdapConfig` 快照，包括 `Credentials` 和 `Attributes`。之后修改原始配置不会影响已有 Client；需要应用新配置时，应创建新的 Client。

### LdapUserInfo 常用字段

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

`MemberOf`、`ProxyAddresses` 和 `OtherTelephone` 均为字符串数组，因为 LDAP 可能为这些属性返回多个值。

## 错误语义

无效参数和配置会在使用前被拒绝。搜索操作中的连接、TLS、绑定和目录服务器故障会以异常形式向上传递；空结果仅表示查询已成功完成但没有匹配用户。凭据无效时，认证返回 `IsValid = false`。认证成功后，如果独立的用户信息查询失败，提供者可以返回 `IsValid = true` 且用户信息为空。

## 实现包

- [Linger.Ldap.ActiveDirectory](../Linger.Ldap.ActiveDirectory/)：面向 Microsoft Active Directory 的实现
- [Linger.Ldap.Novell](../Linger.Ldap.Novell/)：基于 Novell.Directory.Ldap 的跨平台实现
