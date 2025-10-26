using System.Security.Claims;
using System.Security.Cryptography;
using Linger.AspNetCore.Jwt.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Linger.AspNetCore.Jwt;

/// <summary>
/// JWT service implementation with refresh token support
/// </summary>
public abstract class JwtServiceWithRefresh(JwtOption jwtOptions, ILogger<JwtServiceWithRefresh>? logger = null) : JwtService(jwtOptions, logger), IRefreshableJwtService
{
    public override async Task<Token> CreateTokenAsync(string userId)
    {
        try
        {
            Logger?.LogDebug("Generating token with refresh capability for user: {UserId}", userId);

            // Generate base access token
            Token baseToken = await base.CreateTokenAsync(userId).ConfigureAwait(false);

            // Generate and store refresh token
            JwtRefreshToken refreshToken = GenerateRefreshToken();
            await HandleRefreshToken(userId, refreshToken).ConfigureAwait(false);

            Logger?.LogDebug("Token with refresh token generated successfully for user: {UserId}", userId);

            // Return complete token with refresh token
            return new Token(baseToken.AccessToken, refreshToken.RefreshToken);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to generate token with refresh token for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<Token> RefreshTokenAsync(Token token)
    {
        if (string.IsNullOrEmpty(token.RefreshToken))
        {
            Logger?.LogWarning("Token refresh attempt with missing refresh token");
            throw new ArgumentException("Token does not contain a valid refresh token");
        }

        try
        {
            Logger?.LogDebug("Refreshing token");

            ClaimsPrincipal principal = GetPrincipalFromExpiredToken(token.AccessToken);
            var userId = principal.Identity?.Name!;
            JwtRefreshToken refreshToken = await GetExistRefreshTokenAsync(userId).ConfigureAwait(false);

            if (refreshToken.RefreshToken != token.RefreshToken || refreshToken.ExpiryTime <= DateTime.Now)
            {
                Logger?.LogWarning("Token refresh rejected for user: {UserId} - invalid or expired refresh token", userId);
                throw new SecurityTokenException("The provided refresh token is invalid or has expired");
            }

            Logger?.LogDebug("Token refreshed successfully for user: {UserId}", userId);
            return await CreateTokenAsync(userId).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not SecurityTokenException and not ArgumentException)
        {
            Logger?.LogError(ex, "Token refresh failed");
            throw;
        }
    }

    private JwtRefreshToken GenerateRefreshToken()
    {
        var randomNumber = new byte[32];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        var refreshToken = Convert.ToBase64String(randomNumber);
        DateTime refreshTokenExpires = DateTime.Now.AddMinutes(JwtOptions.RefreshTokenExpires);
        return new JwtRefreshToken { RefreshToken = refreshToken, ExpiryTime = refreshTokenExpires };
    }

    /// <summary>
    /// Handles refresh token storage
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="refreshToken">Refresh token information</param>
    protected abstract Task HandleRefreshToken(string userId, JwtRefreshToken refreshToken);

    /// <summary>
    /// Retrieves existing refresh token
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>Refresh token information</returns>
    protected abstract Task<JwtRefreshToken> GetExistRefreshTokenAsync(string userId);
}
