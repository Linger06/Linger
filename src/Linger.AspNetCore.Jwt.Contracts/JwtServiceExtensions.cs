using System;
using System.Threading.Tasks;

namespace Linger.AspNetCore.Jwt.Contracts;

/// <summary>
/// 为IJwtService提供扩展方法，支持刷新令牌功能
/// </summary>
public static class JwtServiceExtensions
{
    /// <summary>
    /// 检查JWT服务是否支持刷新令牌
    /// </summary>
    /// <param name="jwtService">JWT服务</param>
    /// <returns>是否支持刷新令牌</returns>
    public static bool SupportsRefreshToken(this IJwtService jwtService)
    {
        return jwtService is IRefreshableJwtService;
    }
    
    /// <summary>
    /// 尝试刷新令牌
    /// </summary>
    /// <param name="jwtService">JWT服务</param>
    /// <param name="token">当前令牌</param>
    /// <returns>是否成功刷新令牌及新令牌</returns>
    public static async Task<(bool Success, Token? NewToken)> TryRefreshTokenAsync(this IJwtService jwtService, Token token)
    {
        if (jwtService is IRefreshableJwtService refreshableService)
        {
            try
            {
                var newToken = await refreshableService.RefreshTokenAsync(token);
                return (true, newToken);
            }
            catch (Exception)
            {
                return (false, null);
            }
        }
        
        return (false, null);
    }
}
