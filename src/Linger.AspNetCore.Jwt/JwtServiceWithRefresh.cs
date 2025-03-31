using System.Security.Claims;
using System.Security.Cryptography;
using Linger.AspNetCore.Jwt.Contracts;
using Microsoft.Extensions.Logging;

namespace Linger.AspNetCore.Jwt;

/// <summary>
/// 支持刷新令牌功能的JWT服务实现
/// </summary>
public abstract class JwtServiceWithRefresh(JwtOption jwtOptions, ILogger? logger = null) : JwtService(jwtOptions, logger), IRefreshableJwtService
{
    public override async Task<Token> CreateTokenAsync(string userId)
    {
        try
        {
            // 首先获取基本访问令牌
            Token baseToken = await base.CreateTokenAsync(userId);

            // 生成刷新令牌并处理存储
            JwtRefreshToken refreshToken = GenerateRefreshToken();
            await HandleRefreshToken(userId, refreshToken);

            // 返回包含刷新令牌的完整Token
            return new Token(baseToken.AccessToken, refreshToken.RefreshToken);
        }
        catch (Exception ex)
        {
            Logger?.LogError("生成带刷新令牌的Token时出错: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<Token> RefreshTokenAsync(Token token)
    {
        if (string.IsNullOrEmpty(token.RefreshToken))
        {
            throw new ArgumentException("Token不包含有效的刷新令牌");
        }

        ClaimsPrincipal principal = GetPrincipalFromExpiredToken(token.AccessToken);
        var userId = principal.Identity?.Name!;
        JwtRefreshToken refreshToken = await GetExistRefreshTokenAsync(userId);

        if (refreshToken.RefreshToken != token.RefreshToken || refreshToken.ExpiryTime <= DateTime.Now)
        {
            throw new Exception("提供的刷新令牌无效或已过期");
        }

        return await CreateTokenAsync(userId);
    }

    private JwtRefreshToken GenerateRefreshToken()
    {
        // 创建一个随机的Token用做Refresh Token
        var randomNumber = new byte[32];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        var refreshToken = Convert.ToBase64String(randomNumber);
        DateTime refreshTokenExpires = DateTime.Now.AddMinutes(JwtOptions.RefreshTokenExpires);
        return new JwtRefreshToken { RefreshToken = refreshToken, ExpiryTime = refreshTokenExpires };
    }

    /// <summary>
    /// 处理刷新令牌的存储
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="refreshToken">刷新令牌信息</param>
    protected abstract Task HandleRefreshToken(string userId, JwtRefreshToken refreshToken);

    /// <summary>
    /// 获取已存在的刷新令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>刷新令牌信息</returns>
    protected abstract Task<JwtRefreshToken> GetExistRefreshTokenAsync(string userId);
}
