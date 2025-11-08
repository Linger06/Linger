# Linger.Ldap.Contracts

A C# LDAP contract library that provides standardized interfaces and models for integrating LDAP directory services across multiple .NET platforms.

## Introduction

Linger.Ldap.Contracts provides a set of standardized LDAP operation interfaces and models, making it easier to implement consistent LDAP functionality across different .NET applications.

## Features

### Core Contracts
- Standardized LDAP operation interfaces
- Common LDAP attribute definitions
- Cross-platform compatible models
- Type-safe LDAP operations

### Model Support
- Comprehensive user attribute mappings
- Groups and organizational unit models
- Search filter definitions
- Connection parameter contracts

## ASP.NET Core Integration

### Configuring Services

In ASP.NET Core projects, you can utilize LDAP services through dependency injection:

```csharp
// 在Program.cs或Startup.cs中配置服务
public void ConfigureServices(IServiceCollection services)
{
    // 添加LDAP配置
    services.Configure<LdapConfig>(Configuration.GetSection("LdapConfig"));
    
    // 根据具体实现注册LDAP服务
    // 针对Active Directory
    services.AddScoped<ILdap, Linger.Ldap.ActiveDirectory.Ldap>();
    
    // 或者针对Novell LDAP
    // services.AddScoped<ILdap, Linger.Ldap.Novell.Ldap>();
}
```

### appsettings.json配置示例

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
      "displayName", "mail", "sAMAccountName", "userPrincipalName", 
      "telephoneNumber", "department", "title", "givenName", "sn"
    ]
  }
}
```

### 使用示例

#### 用户身份验证

```csharp
public class AuthenticationService
{
    private readonly ILdap _ldap;
    
    public AuthenticationService(ILdap ldap)
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
            cancellationToken);
        
        if (isValid && userInfo != null)
        {
            // 用户认证成功，可以使用userInfo中的信息
            Console.WriteLine($"用户 {userInfo.DisplayName} 认证成功");
            return true;
        }
        
        return false;
    }
}
```

#### 查找用户信息

```csharp
public class UserService
{
    private readonly ILdap _ldap;
    
    public UserService(ILdap ldap)
    {
        _ldap = ldap;
    }
    
    public async Task<AdUserInfo?> GetUserInfoAsync(
        string username, 
        CancellationToken cancellationToken = default)
    {
        return await _ldap.FindUserAsync(username, cancellationToken);
    }
    
    public async Task<IEnumerable<AdUserInfo>> SearchUsersAsync(
        string searchTerm, 
        CancellationToken cancellationToken = default)
    {
        return await _ldap.GetUsersAsync(searchTerm, cancellationToken);
    }
    
    public async Task<bool> CheckUserExistsAsync(
        string username, 
        CancellationToken cancellationToken = default)
    {
        return await _ldap.UserExistsAsync(username, cancellationToken);
    }
}
```

### 取消操作支持

所有异步 LDAP 操作都支持 `CancellationToken`，适用于超时控制和请求取消：

```csharp
public class LdapService
{
    private readonly ILdap _ldap;
    
    public LdapService(ILdap ldap)
    {
        _ldap = ldap;
    }
    
    // 带超时的用户验证
    public async Task<bool> ValidateUserWithTimeoutAsync(
        string username, 
        string password, 
        int timeoutSeconds = 5)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        
        try
        {
            var (isValid, _) = await _ldap.ValidateUserAsync(
                username, 
                password, 
                cts.Token);
            return isValid;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("LDAP 验证超时");
            return false;
        }
    }
    
    // 在 ASP.NET Core 中使用请求取消令牌
    public async Task<AdUserInfo?> GetUserForRequestAsync(
        string username, 
        CancellationToken requestCancellationToken)
    {
        // 如果客户端断开连接，操作会自动取消
        return await _ldap.FindUserAsync(username, requestCancellationToken);
    }
}
```

## 支持的实现

库提供了以下LDAP目录服务的实现：

- **Linger.Ldap.ActiveDirectory** - 针对Microsoft Active Directory的实现
- **Linger.Ldap.Novell** - 使用Novell LDAP客户端库的跨平台实现