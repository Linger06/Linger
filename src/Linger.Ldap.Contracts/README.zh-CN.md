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
    // 添加LDAP服务
    services.AddSingleton<ILdap, YourLdapImplementation>();

    // 或者使用特定实现的扩展方法
    services.AddLdapNovell(options => 
    {
        options.Server = "ldap.example.com";
        options.Port = 389;
        options.BindDn = "cn=admin,dc=example,dc=com";
        options.BindPassword = "password";
        options.SearchBase = "dc=example,dc=com";
    });
}
```

### 在控制器中使用

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILdap _ldap;

    public AuthController(ILdap ldap)
    {
        _ldap = ldap;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        var (isValid, userInfo) = await _ldap.ValidateUserAsync(model.Username, model.Password);

        if (!isValid)
        {
            return Unauthorized();
        }

        // 用户验证成功，创建身份验证令牌或会话
        return Ok(new 
        { 
            success = true,
            user = new 
            {
                username = userInfo.Username,
                displayName = userInfo.DisplayName,
                email = userInfo.Email,
                department = userInfo.Department
            }
        });
    }
}
```

## 核心接口

### ILdap

主要LDAP操作接口：

```csharp
public interface ILdap
{
    Task<(bool IsValid, AdUserInfo? AdUserInfo)> ValidateUserAsync(
        string userName, string password);

    Task<AdUserInfo?> FindUserAsync(
        string userName, LdapCredentials? ldapCredentials = null);

    Task<IEnumerable<AdUserInfo>> GetUsersAsync(
        string filter, LdapCredentials? ldapCredentials = null);
}
```

### 用户模型

用户信息数据模型：

```csharp
public class AdUserInfo
{
    // 识别属性
    public string Username { get; set; }
    public string Dn { get; set; }
    public string DisplayName { get; set; }
    public string UserPrincipalName { get; set; }

    // 个人信息
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Initials { get; set; }
    
    // 联系信息
    public string Email { get; set; }
    public string Telephone { get; set; }
    public string Mobile { get; set; }
    
    // 组织信息
    public string Department { get; set; }
    public string Company { get; set; }
    public string Title { get; set; }
    public string EmployeeId { get; set; }
    
    // 系统和安全信息
    public string[] MemberOf { get; set; }
    public bool AccountDisabled { get; set; }
    public DateTime? PasswordLastSet { get; set; }
    // 其他属性...
}
```

### 配置模型

LDAP连接配置：

```csharp
public class LdapConfig
{
    public string Server { get; set; }
    public int Port { get; set; } = 389;
    public bool Security { get; set; } = false;
    public string BindDn { get; set; }
    public string BindPassword { get; set; }
    public string SearchBase { get; set; }
    public string SearchFilter { get; set; } = "(&(objectClass=person)(sAMAccountName={0}))";
    public int ConnectionTimeout { get; set; } = 30;
}
```

## 实现

此包仅包含接口定义和模型，需要搭配具体实现使用：

- [Linger.Ldap.ActiveDirectory](../Linger.Ldap.ActiveDirectory/) - 针对Active Directory优化的实现
- [Linger.Ldap.Novell](../Linger.Ldap.Novell/) - 基于Novell.Directory.Ldap的跨平台实现

## 支持的框架

- .NET Standard 2.0
- .NET 8.0
- .NET 9.0
