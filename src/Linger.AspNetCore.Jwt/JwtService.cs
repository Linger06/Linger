using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Linger.AspNetCore.Jwt.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Linger.AspNetCore.Jwt;

public abstract class JwtService : IJwtService
{
    private readonly JwtOption _jwtOptions;
    private readonly ILogger? _logger;
    private readonly TokenValidationParameters _validationParameters;

    protected JwtService(JwtOption jwtOptions, ILogger? logger = null)
    {
        _jwtOptions = jwtOptions;
        _logger = logger;
        var issuer = _jwtOptions.Issuer;
        var audience = _jwtOptions.Audience;
        //实际环境中，最好是需要从环境变量中进行获取，而不应该写在代码中
        var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET") ?? _jwtOptions.SecurityKey);
        var securityKey = new SymmetricSecurityKey(key);

        _validationParameters = new TokenValidationParameters
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

    public async Task<Token> CreateTokenAsync(string userId)
    {
        try
        {
            _logger?.LogDebug($"正在为用户 {userId} 生成令牌，密钥长度: {_validationParameters.IssuerSigningKey.KeySize}");
            SigningCredentials signingCredentials = GetSigningCredentials();
            List<Claim> claims = await GetClaimsAsync(userId);
            JwtSecurityToken tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            _logger?.LogDebug($"令牌生成成功: {token[..10]}...");
            JwtRefreshToken refreshToken = GenerateRefreshToken();
            await HandleRefreshToken(userId, refreshToken);
            return await Task.FromResult(new Token(token, refreshToken.RefreshToken));
        }
        catch (Exception ex)
        {
            _logger?.LogError($"生成令牌时出错: {ex.Message}");
            throw;
        }
    }

    protected abstract Task HandleRefreshToken(string userId, JwtRefreshToken refreshToken);

    private SigningCredentials GetSigningCredentials()
    {
        return new SigningCredentials(_validationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256);
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
            _validationParameters.ValidIssuer,
            _validationParameters.ValidAudience,
            claims,
            expires: DateTime.Now.AddMinutes(_jwtOptions.Expires),
            signingCredentials: signingCredentials
        );
        return tokenOptions;
    }

    private JwtRefreshToken GenerateRefreshToken()
    {
        // 创建一个随机的Token用做Refresh Token
        var randomNumber = new byte[32];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        var refreshToken = Convert.ToBase64String(randomNumber);
        DateTime refreshTokenExpires = DateTime.Now.AddMinutes(_jwtOptions.RefreshTokenExpires);
        return new JwtRefreshToken { RefreshToken = refreshToken, ExpiryTime = refreshTokenExpires };
    }

    public async Task<Token?> RefreshTokenAsync(Token token)
    {
        ClaimsPrincipal principal = GetPrincipalFromExpiredToken(token.AccessToken);
        var userId = principal.Identity?.Name!;
        JwtRefreshToken refreshToken = await GetExistRefreshTokenAsync(userId);
        //ApplicationUser? user = await _userManager.FindByNameAsync(principal.Identity?.Name!);
        //if (user == null || user.RefreshToken != token.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
        //{
        //    throw new BadHttpRequestException("provided token has some invalid value");
        //}
        if (refreshToken.RefreshToken != token.RefreshToken || refreshToken.ExpiryTime <= DateTime.Now)
        {
            throw new Exception("provided token has some invalid value");
        }

        return await CreateTokenAsync(userId);
    }

    protected abstract Task<JwtRefreshToken> GetExistRefreshTokenAsync(string userId);

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        // 根据已过期的Token获取用户相关的Principal数据，用来生成新的Token

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _validationParameters.IssuerSigningKey,
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