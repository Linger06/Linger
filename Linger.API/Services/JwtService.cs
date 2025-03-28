using Linger.AspNetCore.Jwt.Contracts;

namespace Linger.API.Services;

public class JwtService(JwtOption jwtOptions, ILogger<JwtService>? logger = null) : AspNetCore.Jwt.JwtService(jwtOptions, logger)
{
    protected override Task<JwtRefreshToken> GetExistRefreshTokenAsync(string userId)
    {
        throw new NotImplementedException();
    }

    protected override Task HandleRefreshToken(string userId, JwtRefreshToken refreshToken)
    {
        throw new NotImplementedException();
    }
}
