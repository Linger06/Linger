namespace Linger.AspNetCore.Jwt.Contracts;

public class JwtOption
{
    public string Issuer { get; init; } = "http://localhost";
    public string Audience { get; init; } = "http://localhost";
    public int Expires { get; init; } = 30;
    public string SecurityKey { get; init; } = "Linger_JWT";
    public int RefreshTokenExpires { get; init; }
}