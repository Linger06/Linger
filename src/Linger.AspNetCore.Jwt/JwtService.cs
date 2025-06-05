using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Linger.AspNetCore.Jwt.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Linger.AspNetCore.Jwt;

/// <summary>
/// JWT服务基础实现，仅提供创建访问令牌的功能
/// </summary>
public class JwtService : IJwtService
{
    protected readonly JwtOption JwtOptions;
    protected readonly ILogger? Logger;
    protected readonly TokenValidationParameters ValidationParameters;
    private readonly JwtTokenBlacklist? _tokenBlacklist;

    public JwtService(JwtOption jwtOptions, ILogger? logger = null, JwtTokenBlacklist? tokenBlacklist = null)
    {
        JwtOptions = jwtOptions;
        Logger = logger;
        _tokenBlacklist = tokenBlacklist;
        var issuer = JwtOptions.Issuer;
        var audience = JwtOptions.Audience;
        //实际环境中，最好是需要从环境变量中进行获取，而不应该写在代码中
        var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET") ?? JwtOptions.SecurityKey);
        var securityKey = new SymmetricSecurityKey(key);

        ValidationParameters = new TokenValidationParameters
        {
            ValidAudience = audience,
            ValidateAudience = false,
            ValidIssuer = issuer,
            ValidateIssuer = false,
            IssuerSigningKey = securityKey,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    }

    public virtual async Task<Token> CreateTokenAsync(string userId)
    {
        try
        {
            Logger?.LogDebug("正在为用户 {userId} 生成令牌，密钥长度: {Length}", userId, ValidationParameters.IssuerSigningKey.KeySize);
            SigningCredentials signingCredentials = GetSigningCredentials();
            List<Claim> claims = await GetClaimsAsync(userId).ConfigureAwait(false);

            // 添加唯一标识符和颁发时间，增强安全性
            var tokenId = Guid.NewGuid().ToString();
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, tokenId));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));

            JwtSecurityToken tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            Logger?.LogDebug("令牌生成成功: {token}...", token[..10]);

            // 基础实现只返回访问令牌
            return await Task.FromResult(new Token(token)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger?.LogError("生成令牌时出错: {Message}", ex.Message);
            throw;
        }
    }

    // 添加撤销令牌的功能
    public virtual Task RevokeTokenAsync(string tokenId, DateTime expiryTime)
    {
        if (_tokenBlacklist == null)
        {
            Logger?.LogWarning("无法撤销令牌，黑名单服务未注册");
            return Task.CompletedTask;
        }

        _tokenBlacklist.Add(tokenId, expiryTime);
        return Task.CompletedTask;
    }

    // 验证令牌是否已被撤销
    protected bool IsTokenRevoked(string tokenId)
    {
        return _tokenBlacklist?.Contains(tokenId) ?? false;
    }

    private SigningCredentials GetSigningCredentials()
    {
        return new SigningCredentials(ValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256);
    }

    protected virtual Task<List<Claim>> GetClaimsAsync(string userId)
    {
        // 演示了返回用户名和Role两类Claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,userId )
        };

        //IList<string> roles = await _userManager.GetRolesAsync(User);
        //claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return Task.FromResult(claims);
    }

    private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
    {
        // 配置JWT选项
        var tokenOptions = new JwtSecurityToken
        (
            ValidationParameters.ValidIssuer,
            ValidationParameters.ValidAudience,
            claims,
            expires: DateTime.Now.AddMinutes(JwtOptions.Expires),
            signingCredentials: signingCredentials
        );
        return tokenOptions;
    }

    protected ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        // 根据已过期的Token获取用户相关的Principal数据，用来生成新的Token

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = ValidationParameters.IssuerSigningKey,
            ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken? securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}
