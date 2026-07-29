# Linger.Ldap.Contracts

用于 .NET 应用的 LDAP 核心契约与共享模型包。

## 介绍

Linger.Ldap.Contracts 定义了与具体 LDAP 提供程序无关的抽象层。业务代码只依赖统一接口，即可在不同实现之间切换。

## 支持的框架

- .NET 10.0
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0

## 包含内容

- `ILdapClient`：LDAP 核心操作接口
- `LdapConfig`、`LdapCredentials`：连接配置模型
- `LdapUserInfo`：统一用户信息模型

## ASP.NET Core 集成

### 配置服务

在 DI 中注册配置和一个具体实现：

```csharp
using Linger.Ldap.Contracts;

public void ConfigureServices(IServiceCollection services)
{
    services.Configure<LdapConfig>(Configuration.GetSection("LdapConfig"));

    // 二选一：只注册一个具体实现
    services.AddScoped<ILdapClient, Linger.Ldap.ActiveDirectory.AdLdapClient>();
    // services.AddScoped<ILdapClient, Linger.Ldap.Novell.NovellLdapClient>();
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

    public async Task<IEnumerable<LdapUserInfo>> SearchUsersAsync(
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

所有异步 LDAP 操作都接受 `CancellationToken`。Novell 提供程序会将取消传递到原生异步 LDAP 调用；Active Directory 提供程序依赖同步的 `System.DirectoryServices` API，只能在调用前后响应取消，无法中断已经开始的 LDAP 查询。

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

    Task<IEnumerable<LdapUserInfo>> GetUsersAsync(
        string userName,
        LdapCredentials? ldapCredentials = null,
        string? searchBase = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<LdapUserInfo>> SearchUsersByFilterAsync(
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
}
```

### LdapUserInfo 常用字段

- `DisplayName`
- `SamAccountName`
- `Upn`
- `Dn`
- `Email`
- `Department`
- `Title`
- `MemberOf`
- `Status`

## 实现包

- [Linger.Ldap.ActiveDirectory](../Linger.Ldap.ActiveDirectory/)：面向 Microsoft Active Directory 的实现
- [Linger.Ldap.Novell](../Linger.Ldap.Novell/)：基于 Novell.Directory.Ldap 的跨平台实现
