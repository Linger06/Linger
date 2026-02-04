namespace Linger.AspNetCore.Jwt.Contracts;

/// <summary>
/// Represents the result of a token refresh operation
/// </summary>
/// <param name="Success">Indicates whether the refresh operation was successful</param>
/// <param name="Token">The new token if the operation was successful; otherwise, null</param>
/// <param name="ErrorMessage">The error message if the operation failed; otherwise, null</param>
public record RefreshResult(bool Success, Token? Token, string? ErrorMessage);

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
    /// <remarks>
    /// Consider using <see cref="TryRefreshTokenAsync"/> for better performance and cleaner error handling.
    /// This method is suitable when you have a global exception handling middleware.
    /// </remarks>
    public static Task<Token> RefreshTokenAsync(this IJwtService jwtService, Token token)
    {
        if (jwtService is not IRefreshableJwtService refreshableService)
        {
            throw new NotSupportedException("This JWT service does not support token refresh functionality");
        }

        return refreshableService.RefreshTokenAsync(token);
    }

    /// <summary>
    /// Attempts to refresh the token and returns a result indicating success or failure
    /// </summary>
    /// <param name="jwtService">The JWT service instance</param>
    /// <param name="token">The token to refresh</param>
    /// <returns>A <see cref="RefreshResult"/> containing the operation result</returns>
    public static async Task<RefreshResult> RefreshTokenResultAsync(this IJwtService jwtService, Token token)
    {
        if (jwtService is not IRefreshableJwtService refreshableService)
        {
            return new RefreshResult(false, null, "Token refresh is not supported by this service");
        }

        try
        {
            var newToken = await refreshableService.RefreshTokenAsync(token).ConfigureAwait(false);
            return new RefreshResult(true, newToken, null);
        }
        catch (Exception ex)
        {
            return new RefreshResult(false, null, ex.Message);
        }
    }

    [Obsolete("Use RefreshTokenResultAsync instead. This method will be removed in a future version.")]
    public static async Task<(bool Success, Token? NewToken)> TryRefreshTokenAsync(this IJwtService jwtService, Token token)
    {
        if (jwtService is IRefreshableJwtService refreshableService)
        {
            try
            {
                var newToken = await refreshableService.RefreshTokenAsync(token).ConfigureAwait(false);
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
