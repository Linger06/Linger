namespace Linger.AspNetCore.Jwt.Contracts;

public class JwtOption
{
    public string Issuer { get; init; } = "http://localhost";
    public string Audience { get; init; } = "http://localhost";

    /// <summary>
    /// Token expiration time in minutes (default: 30 minutes)
    /// </summary>
    public int ExpiresInMinutes { get; init; } = 30;

    /// <summary>
    /// Security key for signing tokens. MUST be changed in production!
    /// Recommended minimum length: 32 characters.
    /// </summary>
    public string SecurityKey { get; init; } = null!;

    /// <summary>
    /// Refresh token expiration time in minutes (default: 7 days = 10080 minutes)
    /// </summary>
    public int RefreshTokenExpiresInMinutes { get; init; } = 10080;
}