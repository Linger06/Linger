namespace Linger.AspNetCore.Jwt.Contracts;

public interface IJwtService
{
    /// <summary>
    ///     Create token
    /// </summary>
    /// <returns></returns>
    Task<Token> CreateTokenAsync(string userId);

    Task<Token?> RefreshTokenAsync(Token token);
}