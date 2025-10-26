# Linger.Ldap.ActiveDirectory

一个全面的 .NET 库，用于 Active Directory LDAP 操作，提供对 AD 用户信息和身份验证的简化访问。

## 功能特点

### 用户管理
- 用户身份验证和验证
- 详细用户信息检索
- 用户搜索功能
- 组成员资格信息

### 用户信息类别
- 基本标识（用户名、显示名称、UPN）
- 个人信息（名字、姓氏、缩写）
- 联系人详细信息（电子邮件、电话号码、地址）
- 组织信息（部门、职位、员工 ID）
- 系统属性（工作站、配置文件路径）
- 安全设置（账户状态、密码信息）


## 支持的框架
- .NET 9.0
- .NET 8.0
- .NET Standard 2.0

## 安装

```shell
dotnet add package Linger.Ldap.ActiveDirectory
```

## 使用方法

### 配置 LDAP 连接

```csharp
// 在 Program.cs 或 Startup.cs 中
services.AddLdapActiveDirectory(options => 
{
    options.Url = "example.com";
    options.SearchBase = "DC=example,DC=com";
    options.SearchFilter = "(&(objectClass=user)(sAMAccountName={0}))";
    options.BindDn = "CN=ServiceAccount,OU=ServiceAccounts,DC=example,DC=com";
    options.BindPassword = "SecurePassword123!";
});
```

### 用户身份验证

```csharp
public class AuthService
{
    private readonly ILdap _ldap;

    public AuthService(ILdap ldap)
    {
        _ldap = ldap;
    }

    public async Task<bool> AuthenticateUserAsync(string username, string password)
    {
        var (isValid, user) = await _ldap.ValidateUserAsync(username, password);
        return isValid;
    }

    public async Task<bool> AuthenticateAndGetUserInfoAsync(string username, string password)
    {
        var (isValid, user) = await _ldap.ValidateUserAsync(username, password);
        
        if (isValid && user != null)
        {
            Console.WriteLine($"用户名: {user.Username}");
            Console.WriteLine($"显示名称: {user.DisplayName}");
            Console.WriteLine($"电子邮件: {user.Email}");
            Console.WriteLine($"部门: {user.Department}");
            Console.WriteLine($"职位: {user.Title}");
            
            return true;
        }
        
        return false;
    }
}
```

### 检索用户信息

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
        // 查找特定用户
        var adUser = await _ldap.FindUserAsync(username);
        
        if (adUser is null)
            return null;
            
        return new UserViewModel
        {
            Username = adUser.Username,
            DisplayName = adUser.DisplayName,
            Email = adUser.Email,
            Department = adUser.Department,
            IsEnabled = !adUser.AccountDisabled,
            Groups = adUser.MemberOf
        };
    }
    
    public async Task<List<UserViewModel>> SearchUsersAsync(string searchTerm)
    {
        // 按名称或其他属性搜索用户
        var adUsers = await _ldap.GetUsersAsync(searchTerm);
        
        return adUsers.Select(u => new UserViewModel
        {
            Username = u.Username,
            DisplayName = u.DisplayName,
            Email = u.Email,
            Department = u.Department
        }).ToList();
    }
}
```

### 检查组成员资格

```csharp
public class PermissionService
{
    private readonly ILdap _ldap;
    
    public PermissionService(ILdap ldap)
    {
        _ldap = ldap;
    }
    
    public async Task<bool> IsUserInGroupAsync(string username, string groupName)
    {
        var user = await _ldap.FindUserAsync(username);
        
        if (user?.MemberOf == null)
            return false;
            
        foreach (var group in user.MemberOf)
        {
            if (group.Contains($"CN={groupName},"))
                return true;
        }
        
        return false;
    }
}
```

## 特定于 Active Directory 的功能

此库利用 `System.DirectoryServices` 和 `System.DirectoryServices.AccountManagement` 提供对 Active Directory 特定功能的访问：

```csharp
// 例如：访问底层 DirectoryEntry 对象
var ldapService = new Ldap(ldapConfig);
var directoryEntry = ldapService.GetEntryByUsername("username");

// 可以访问 DirectoryEntry 的所有功能
var nativeObject = directoryEntry.NativeObject;
var properties = directoryEntry.Properties;
```

## 注意事项

1. 此库需要在 Windows 上运行，或者在支持 `System.DirectoryServices` 的环境中运行
2. .NET 5+ 版本具有 `[SupportedOSPlatform("windows")]` 属性
3. 对于跨平台 LDAP 解决方案，请查看 [Linger.Ldap.Novell](../Linger.Ldap.Novell/)

## 依赖项

- System.DirectoryServices
- System.DirectoryServices.AccountManagement 
- Microsoft.Extensions.Options
- Linger.Ldap.Contracts

## 相关包

- [Linger.Ldap.Contracts](../Linger.Ldap.Contracts/) - LDAP 操作的核心接口和数据模型
- [Linger.Ldap.Novell](../Linger.Ldap.Novell/) - 基于 Novell 库的跨平台 LDAP 实现
