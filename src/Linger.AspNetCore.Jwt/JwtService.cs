using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Linger.AspNetCore.Jwt.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Linger.AspNetCore.Jwt;

/// <summary>
/// Provides JWT token generation and validation services
/// </summary>
public class JwtService : IJwtService
{
    protected readonly JwtOption JwtOptions;
    protected readonly ILogger<JwtService>? Logger;
    protected readonly TokenValidationParameters ValidationParameters;

    public JwtService(JwtOption jwtOptions, ILogger<JwtService>? logger = null)
    {
        JwtOptions = jwtOptions;
        Logger = logger;
        var issuer = JwtOptions.Issuer;
        var audience = JwtOptions.Audience;
        // In production, it's recommended to retrieve the secret key from environment variables
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
            Logger?.LogDebug("Generating JWT token for user: {UserId}", userId);
            SigningCredentials signingCredentials = GetSigningCredentials();
            List<Claim> claims = await GetClaimsAsync(userId).ConfigureAwait(false);

            // Add JTI and IAT claims for security tracking
            var tokenId = Guid.NewGuid().ToString();
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, tokenId));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)));

            JwtSecurityToken tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            Logger?.LogDebug("JWT token generated successfully for user: {UserId}", userId);

            // Returns access token only
            return new Token(token);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Token generation failed for user: {UserId}", userId);
            throw;
        }
    }

    private SigningCredentials GetSigningCredentials()
    {
        return new SigningCredentials(ValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256);
    }

    protected virtual Task<List<Claim>> GetClaimsAsync(string userId)
    {
        // Example: returns username claim
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,userId )
        };

        // Example for adding role claims:
        // IList<string> roles = await _userManager.GetRolesAsync(User);
        // claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return Task.FromResult(claims);
    }

    private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
    {
        var tokenOptions = new JwtSecurityToken
        (
            ValidationParameters.ValidIssuer,
            ValidationParameters.ValidAudience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(JwtOptions.ExpiresInMinutes),
            signingCredentials: signingCredentials
        );
        return tokenOptions;
    }

    protected ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        // Extracts user claims from expired token for refresh scenarios

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = ValidationParameters.IssuerSigningKey,
            ValidateLifetime = false // Skip expiration check
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken? securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                Logger?.LogWarning("Token validation rejected: invalid signature algorithm");
                throw new SecurityTokenException("Invalid token");
            }

            Logger?.LogDebug("Principal extracted from expired token");
            return principal;
        }
        catch (Exception ex) when (ex is not SecurityTokenException)
        {
            Logger?.LogError(ex, "Token validation failed");
            throw;
        }
    }
}
