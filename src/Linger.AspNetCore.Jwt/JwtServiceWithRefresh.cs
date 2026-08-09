using System.Security.Claims;
using System.Security.Cryptography;
using Linger.AspNetCore.Jwt.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Linger.AspNetCore.Jwt;

/// <summary>
/// JWT service implementation with refresh token support
/// </summary>
public abstract class JwtServiceWithRefresh(
    JwtOption jwtOptions,
    ILogger<JwtServiceWithRefresh>? logger = null) : JwtService(jwtOptions, logger), IRefreshableJwtService
{
    /// <inheritdoc />
    public override async Task<Token> CreateTokenAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        Logger?.LogDebug("Generating token with refresh capability for user: {UserId}", userId);

        Token accessToken = await base
            .CreateTokenAsync(userId, cancellationToken)
            .ConfigureAwait(false);
        JwtRefreshToken refreshToken = GenerateRefreshToken();
        await StoreRefreshTokenAsync(userId, refreshToken, cancellationToken).ConfigureAwait(false);

        Logger?.LogDebug("Token with refresh token generated successfully for user: {UserId}", userId);

        return new Token(accessToken.AccessToken, refreshToken.RefreshToken);
    }

    /// <inheritdoc />
    public async Task<Token> RefreshTokenAsync(
        Token token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);
        if (string.IsNullOrWhiteSpace(token.AccessToken) ||
            string.IsNullOrWhiteSpace(token.RefreshToken))
        {
            Logger?.LogWarning("Token refresh attempt with missing refresh token");
            throw new ArgumentException("Token must contain an access token and a refresh token.", nameof(token));
        }

        cancellationToken.ThrowIfCancellationRequested();
        Logger?.LogDebug("Refreshing token");

        ClaimsPrincipal principal = GetPrincipalFromExpiredToken(token.AccessToken);
        var userId = principal.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userId))
        {
            Logger?.LogWarning("Token refresh rejected because the access token has no user identifier");
            throw new SecurityTokenException("The access token has no user identifier.");
        }

        Token accessToken = await base
            .CreateTokenAsync(userId, cancellationToken)
            .ConfigureAwait(false);
        JwtRefreshToken replacement = GenerateRefreshToken();
        var rotated = await TryRotateRefreshTokenAsync(
            userId,
            token.RefreshToken,
            replacement,
            cancellationToken).ConfigureAwait(false);

        if (!rotated)
        {
            Logger?.LogWarning(
                "Token refresh rejected for user: {UserId} - invalid, expired, or already used refresh token",
                userId);
            throw new SecurityTokenException("The refresh token is invalid, expired, or already used.");
        }

        Logger?.LogDebug("Token refreshed successfully for user: {UserId}", userId);

        return new Token(accessToken.AccessToken, replacement.RefreshToken);
    }

    private JwtRefreshToken GenerateRefreshToken()
    {
        var randomNumber = RandomNumberGenerator.GetBytes(32);
        var refreshToken = Convert.ToBase64String(randomNumber);
        DateTime refreshTokenExpires = DateTime.UtcNow.AddMinutes(JwtOptions.RefreshTokenExpiresInMinutes);

        return new JwtRefreshToken { RefreshToken = refreshToken, ExpiryTime = refreshTokenExpires };
    }

    /// <summary>
    /// Stores the first refresh token issued for a new token session.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="refreshToken">Refresh token to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected abstract Task StoreRefreshTokenAsync(
        string userId,
        JwtRefreshToken refreshToken,
        CancellationToken cancellationToken);

    /// <summary>
    /// Atomically replaces a valid stored refresh token.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="presentedRefreshToken">Refresh token presented by the client.</param>
    /// <param name="replacement">Replacement refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see langword="true"/> only when the stored token matched, was not expired, and was replaced atomically.</returns>
    protected abstract Task<bool> TryRotateRefreshTokenAsync(
        string userId,
        string presentedRefreshToken,
        JwtRefreshToken replacement,
        CancellationToken cancellationToken);
}
