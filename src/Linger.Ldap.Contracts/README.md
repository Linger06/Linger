# Linger.Ldap.Contracts

一个C#的LDAP契约库，提供了跨多个.NET平台的LDAP目录服务集成的标准化接口和模型。

## 介绍

Linger.Ldap.Contracts提供了一组标准化的LDAP操作接口和模型，使得在不同的.NET应用程序中实现一致的LDAP功能变得更加容易。

## 特性

### 核心契约
- 标准化的LDAP操作接口
- 通用的LDAP属性定义
- 跨平台兼容的模型
- 类型安全的LDAP操作

### 模型支持
- 全面的用户属性映射
- 组和组织单位模型
- 搜索过滤器定义
- 连接参数契约

## ASP.NET Core集成

### 配置服务

在ASP.NET Core项目中，您可以通过依赖注入来使用LDAP服务：

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