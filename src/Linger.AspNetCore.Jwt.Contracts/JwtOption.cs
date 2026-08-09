namespace Linger.AspNetCore.Jwt.Contracts;

/// <summary>
/// Defines JWT signing and lifetime settings.
/// </summary>
public class JwtOption
{
    /// <summary>
    /// Gets the expected token issuer.
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// Gets the expected token audience.
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Token expiration time in minutes (default: 30 minutes)
    /// </summary>
    public int ExpiresInMinutes { get; init; } = 30;

    /// <summary>
    /// Security key for signing tokens. MUST be changed in production!
    /// Recommended minimum length: 32 characters.
    /// </summary>
    public string SecurityKey { get; init; } = string.Empty;

    /// <summary>
    /// Refresh token expiration time in minutes (default: 7 days = 10080 minutes)
    /// </summary>
    public int RefreshTokenExpiresInMinutes { get; init; } = 10080;
}
