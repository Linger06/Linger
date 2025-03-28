using Linger.AspNetCore.Jwt.Contracts;

namespace Linger.AspNetCore.Jwt;

/// <summary>
/// 为JWT服务提供刷新令牌相关的扩展方法
/// </summary>
public static class RefreshTokenExtensions
{
    /// <summary>
    /// 检查JWT服务是否支持刷新令牌功能
    /// </summary>
    /// <param name="service">JWT服务</param>
    /// <returns>如果支持刷新令牌功能则返回true，否则返回false</returns>
    public static bool SupportsRefreshToken(this IJwtService service)
    {
        return service is ISupportsRefreshToken;
    }
    
    /// <summary>
    /// 尝试刷新令牌，如果服务不支持刷新令牌则返回失败
    /// </summary>
    /// <param name="service">JWT服务</param>
    /// <param name="token">当前Token</param>
    /// <returns>包含操作结果和新Token的元组</returns>
    public static async Task<(bool success, Token? newToken)> TryRefreshTokenAsync(
        this IJwtService service, Token token)
    {
        if (service is ISupportsRefreshToken refreshService)
        {
            try
            {
                var newToken = await refreshService.RefreshTokenAsync(token);
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
