namespace Linger.AspNetCore.Jwt.Contracts;

public class JwtRefreshToken
{
    public DateTime ExpiryTime { get; init; }
    public required string RefreshToken { get; init; }
}