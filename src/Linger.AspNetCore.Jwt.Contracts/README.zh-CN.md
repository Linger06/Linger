# Linger.AspNetCore.Jwt.Contracts

ASP.NET Core 应用中 JWT（JSON Web Token）认证和授权的核心接口和抽象。

## 功能特点

- 标准化的 JWT 令牌生成和验证接口
- 支持刷新令牌功能
- 契约与实现的清晰分离
- JWT 配置选项模型
- 服务注册扩展方法

## 支持的框架

- .NET 8.0+
- .NET 9.0+

## 安装

```shell
dotnet add package Linger.AspNetCore.Jwt.Contracts
```

## 核心接口

### IJwtService

```csharp
public interface IJwtService
{
    /// <summary>
    /// 创建 JWT 令牌
    /// </summary>
    /// <param name="userId">用户标识</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>包含访问令牌的 Token 对象</returns>
    Task<Token> CreateTokenAsync(string userId, CancellationToken cancellationToken = default);
}
```

### IRefreshableJwtService

```csharp
public interface IRefreshableJwtService : IJwtService
{
    /// <summary>
    /// 刷新 JWT 令牌
    /// </summary>
    /// <param name="token">包含访问令牌和刷新令牌的 Token 对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新的 Token 对象</returns>
    Task<Token> RefreshTokenAsync(Token token, CancellationToken cancellationToken = default);
}
```

刷新实现由 `Linger.AspNetCore.Jwt.JwtServiceWithRefresh` 提供。派生类需要实现两种不同的存储操作：首次签发使用 `StoreRefreshTokenAsync`；刷新使用 `TryRotateRefreshTokenAsync`，后者必须在存储层原子比较旧令牌并替换新令牌。不要把轮换实现为“查询后更新”，否则并发刷新可能重复成功。

## 相关包

- [Linger.AspNetCore.Jwt](../Linger.AspNetCore.Jwt/) - 这些契约的实现
