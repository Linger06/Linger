namespace Linger.AspNetCore.Jwt.Contracts;

/// <summary>
/// JWT服务基础接口，仅包含基本的创建令牌功能
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// 创建令牌
    /// </summary>
    /// <param name="userId">用户标识</param>
    /// <returns>包含访问令牌的Token对象</returns>
    Task<Token> CreateTokenAsync(string userId);
}

/// <summary>
/// 标记接口，表示JWT服务支持刷新令牌功能
/// </summary>
public interface ISupportsRefreshToken
{
    /// <summary>
    /// 刷新令牌
    /// </summary>
    /// <param name="token">包含访问令牌和刷新令牌的Token对象</param>
    /// <returns>新的Token对象</returns>
    Task<Token> RefreshTokenAsync(Token token);
}