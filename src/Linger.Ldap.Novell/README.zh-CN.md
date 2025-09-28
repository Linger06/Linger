# Linger.Ldap.Novell

> 📝 *查看此文档: [English](./README.md) | [中文](./README.zh-CN.md)*

一个综合性的 .NET 库，使用 Novell.Directory.Ldap 提供程序实现与 LDAP 目录的无缝集成，支持跨平台。

## 功能特点

### 核心功能
- 平台无关的 LDAP 操作
- SSL/TLS 安全连接
- 连接池管理
- 全面的错误处理

### 用户管理
- 用户身份验证和验证
- 详细用户信息检索
- 高级搜索功能
- 组成员资格查询

### 信息类别
- 基本标识（用户名、显示名称、UPN）
- 个人信息（名字、姓氏、缩写）
- 联系人详细信息（电子邮件、电话号码、地址）
- 组织信息（部门、职位、员工 ID）
- 系统属性（工作站、配置文件路径）
- 安全设置（账户状态、密码信息）

## 支持的框架

- .NET 9.0
- .NET 8.0

## 安装

```shell
dotnet add package Linger.Ldap.Novell
```

## 基本用法

### 配置

```csharp
// 在 Startup.cs 或 Program.cs 中
services.AddLdapNovell(options => 
{
    options.Server = "ldap.example.com";
    options.Port = 389;
    options.UseSsl = false;
    options.BindDn = "cn=admin,dc=example,dc=com";
    options.BindPassword = "password";
    options.SearchBase = "dc=example,dc=com";
    options.SearchFilter = "(&(objectClass=person)(uid={0}))";
});
```

### 身份验证

```csharp
public class AuthService
{
    private readonly ILdap _ldap;
    
    public AuthService(ILdap ldap)
    {
        _ldap = ldap;
    }
    
    public async Task<bool> AuthenticateAsync(string username, string password)
    {
        var (isValid, userInfo) = await _ldap.ValidateUserAsync(username, password);
        return isValid;
    }
}
```

### 用户信息检索

```csharp
public class UserService
{
    private readonly ILdap _ldap;
    
    public UserService(ILdap ldap)
    {
        _ldap = ldap;
    }
    
    public async Task<UserViewModel?> GetUserInfoAsync(string username)
    {
        var ldapUser = await _ldap.FindUserAsync(username);
        
        if (ldapUser is null)
            return null;
            
        return new UserViewModel
        {
            Username = ldapUser.Username,
            DisplayName = ldapUser.DisplayName,
            Email = ldapUser.Email,
            Department = ldapUser.Department,
            IsActive = !ldapUser.AccountDisabled
        };
    }
    
    public async Task<IEnumerable<UserViewModel>> SearchUsersAsync(string searchTerm)
    {
        var ldapUsers = await _ldap.GetUsersAsync(searchTerm);
        
        return ldapUsers.Select(u => new UserViewModel
        {
            Username = u.Username,
            DisplayName = u.DisplayName,
            Email = u.Email,
            Department = u.Department
        });
    }
}
```

### 自定义 LDAP 操作

```csharp
public class AdvancedLdapService
{
    private readonly LdapConfig _config;
    
    public AdvancedLdapService(IOptions<LdapConfig> options)
    {
        _config = options.Value;
    }
    
    public async Task<bool> ChangeUserPasswordAsync(string username, string oldPassword, string newPassword)
    {
        // 使用 Novell.Directory.Ldap 库实现自定义 LDAP 操作
        using var connection = new LdapConnection();
        connection.SecureSocketLayer = _config.Security;
        
        try
        {
            await connection.ConnectAsync(_config.Server, _config.Port);
            
            // 先验证用户当前密码
            var userDn = $"uid={username},{_config.SearchBase}";
            await connection.BindAsync(userDn, oldPassword);
            
            // 修改密码
            var modification = new LdapModification(
                LdapModification.Replace,
                new LdapAttribute("userPassword", newPassword)
            );
            
            await connection.ModifyAsync(userDn, new[] { modification });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

## 高级特性

### 连接池管理

该库内部使用连接池来优化性能。您可以配置连接池参数：

```csharp
services.AddLdapNovell(options => 
{
    // 基本配置
    options.Server = "ldap.example.com";
    
    // 连接池配置
    options.ConnectionPooling = true;
    options.ConnectionPoolSize = 10;
    options.ConnectionTimeout = TimeSpan.FromSeconds(30);
});
```

### SSL/TLS 支持

```csharp
services.AddLdapNovell(options => 
{
    options.Server = "ldaps.example.com";
    options.Port = 636;
    options.UseSsl = true;
    options.IgnoreSslCertificateErrors = false; // 生产环境应设为 false
});
```

## 依赖项

- Novell.Directory.Ldap.NETStandard (4.0.0+)
- Linger.Ldap.Contracts
- Microsoft.Extensions.Options

## 相关包

- [Linger.Ldap.Contracts](../Linger.Ldap.Contracts/)：核心 LDAP 接口和数据模型
- [Linger.Ldap.ActiveDirectory](../Linger.Ldap.ActiveDirectory/)：专为 Active Directory 优化的实现
