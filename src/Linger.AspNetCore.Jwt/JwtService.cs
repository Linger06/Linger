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
    private const int MinimumSecurityKeySizeInBytes = 32;
    private const string SecurityKeyEnvironmentVariable = "SECRET";
    private static readonly TimeSpan s_defaultClockSkew = TimeSpan.FromMinutes(5);
    private readonly TokenValidationParameters _expiredTokenValidationParameters;
    protected readonly JwtOption JwtOptions;
    protected readonly ILogger<JwtService>? Logger;
    protected readonly TokenValidationParameters ValidationParameters;

    /// <summary>
    /// Initializes a JWT service with validated signing and token-lifetime settings.
    /// </summary>
    /// <param name="jwtOptions">JWT options.</param>
    /// <param name="logger">Optional logger.</param>
    public JwtService(JwtOption jwtOptions, ILogger<JwtService>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(jwtOptions);

        JwtOptions = jwtOptions;
        Logger = logger;
        ValidateOptions(jwtOptions);

        var securityKey = CreateSecurityKey(jwtOptions);
        ValidationParameters = CreateTokenValidationParameters(jwtOptions, securityKey, validateLifetime: true);
        _expiredTokenValidationParameters = CreateTokenValidationParameters(
            jwtOptions,
            securityKey,
            validateLifetime: false);
    }

    /// <inheritdoc />
    public virtual async Task<Token> CreateTokenAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        cancellationToken.ThrowIfCancellationRequested();

        Logger?.LogDebug("Generating JWT token for user: {UserId}", userId);
        SigningCredentials signingCredentials = GetSigningCredentials();
        List<Claim> claims = await GetClaimsAsync(userId, cancellationToken).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
        var tokenId = Guid.NewGuid().ToString();
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, tokenId));
        claims.Add(new Claim(
            JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)));

        JwtSecurityToken tokenOptions = GenerateTokenOptions(signingCredentials, claims);
        var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        Logger?.LogDebug("JWT token generated successfully for user: {UserId}", userId);

        return new Token(token);
    }

    private SigningCredentials GetSigningCredentials()
    {
        return new SigningCredentials(ValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256);
    }

    protected virtual Task<List<Claim>> GetClaimsAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userId)
        };

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
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal = tokenHandler.ValidateToken(
                token,
                _expiredTokenValidationParameters,
                out SecurityToken? securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.Ordinal))
            {
                Logger?.LogWarning("Token validation rejected: invalid signature algorithm");
                throw new SecurityTokenException("Invalid token");
            }

            Logger?.LogDebug("Principal extracted from expired token");
            return principal;
        }
        catch (SecurityTokenException ex)
        {
            Logger?.LogWarning(ex, "Token validation failed");
            throw;
        }
    }

    internal static TokenValidationParameters CreateTokenValidationParameters(
        JwtOption jwtOptions,
        bool validateLifetime)
    {
        ArgumentNullException.ThrowIfNull(jwtOptions);
        ValidateOptions(jwtOptions);

        return CreateTokenValidationParameters(
            jwtOptions,
            CreateSecurityKey(jwtOptions),
            validateLifetime);
    }

    private static TokenValidationParameters CreateTokenValidationParameters(
        JwtOption jwtOptions,
        SymmetricSecurityKey securityKey,
        bool validateLifetime)
    {
        return new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateLifetime = validateLifetime,
            RequireExpirationTime = true,
            ClockSkew = s_defaultClockSkew
        };
    }

    private static SymmetricSecurityKey CreateSecurityKey(JwtOption jwtOptions)
    {
        var securityKey = Environment.GetEnvironmentVariable(SecurityKeyEnvironmentVariable)
            ?? jwtOptions.SecurityKey;
        if (string.IsNullOrWhiteSpace(securityKey))
        {
            throw new InvalidOperationException(
                $"Configure JwtOptions.SecurityKey or the {SecurityKeyEnvironmentVariable} environment variable.");
        }

        var keyBytes = Encoding.UTF8.GetBytes(securityKey);
        if (keyBytes.Length < MinimumSecurityKeySizeInBytes)
        {
            throw new InvalidOperationException(
                $"The JWT security key must be at least {MinimumSecurityKeySizeInBytes} bytes.");
        }

        return new SymmetricSecurityKey(keyBytes);
    }

    private static void ValidateOptions(JwtOption jwtOptions)
    {
        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
        {
            throw new InvalidOperationException("JwtOptions.Issuer must be configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
        {
            throw new InvalidOperationException("JwtOptions.Audience must be configured.");
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(jwtOptions.ExpiresInMinutes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(jwtOptions.RefreshTokenExpiresInMinutes);
    }
}
