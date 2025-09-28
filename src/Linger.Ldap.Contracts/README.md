# Linger.Ldap.Contracts

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

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
    
    public async Task<bool> AuthenticateUserAsync(string username, string password)
    {
        var (isValid, userInfo) = await _ldap.ValidateUserAsync(username, password);
        
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
    
    public async Task<AdUserInfo?> GetUserInfoAsync(string username)
    {
        return await _ldap.FindUserAsync(username);
    }
    
    public async Task<IEnumerable<AdUserInfo>> SearchUsersAsync(string searchTerm)
    {
        return await _ldap.GetUsersAsync(searchTerm);
    }
    
    public async Task<bool> CheckUserExistsAsync(string username)
    {
        return await _ldap.UserExistsAsync(username);
    }
}
```

## 支持的实现

库提供了以下LDAP目录服务的实现：

- **Linger.Ldap.ActiveDirectory** - 针对Microsoft Active Directory的实现
- **Linger.Ldap.Novell** - 使用Novell LDAP客户端库的跨平台实现