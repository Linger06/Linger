namespace Linger.AspNetCore.Jwt.Contracts;

/// <summary>
/// Defines basic JWT service operations for token creation
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Creates a new JWT token for the specified user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A token object containing the access token</returns>
    Task<Token> CreateTokenAsync(string userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Extends JWT service with refresh token capabilities
/// </summary>
public interface IRefreshableJwtService : IJwtService
{
    /// <summary>
    /// Refreshes an expired access token using a valid refresh token
    /// </summary>
    /// <param name="token">Token object containing both access and refresh tokens</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A new token object with updated access and refresh tokens</returns>
    Task<Token> RefreshTokenAsync(Token token, CancellationToken cancellationToken = default);
}
