namespace Linger.AspNetCore.Jwt.Contracts;

/// <summary>
/// Provides extension methods for JWT services
/// </summary>
public static class JwtServiceExtensions
{
    /// <summary>
    /// Determines whether the JWT service supports token refresh functionality
    /// </summary>
    /// <param name="jwtService">The JWT service instance</param>
    /// <returns><c>true</c> if the service supports token refresh; otherwise, <c>false</c></returns>
    public static bool SupportsRefreshToken(this IJwtService jwtService)
    {
        return jwtService is IRefreshableJwtService;
    }

    /// <summary>
    /// Attempts to refresh the token if the service supports it
    /// </summary>
    /// <param name="jwtService">The JWT service instance</param>
    /// <param name="token">The token to refresh</param>
    /// <returns>The refreshed token</returns>
    /// <exception cref="NotSupportedException">Thrown when the service does not support token refresh</exception>
    /// <exception cref="SecurityTokenException">Thrown when the refresh token is invalid or expired</exception>
    public static Task<Token> RefreshTokenAsync(this IJwtService jwtService, Token token)
    {
        if (jwtService is not IRefreshableJwtService refreshableService)
        {
            throw new NotSupportedException("This JWT service does not support token refresh functionality");
        }

        return refreshableService.RefreshTokenAsync(token);
    }
}
