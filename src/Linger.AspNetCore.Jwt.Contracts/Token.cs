namespace Linger.AspNetCore.Jwt.Contracts;

public class Token(string accessToken, string? refreshToken = null)
{
    public string AccessToken { get; set; } = accessToken;
    public string? RefreshToken { get; set; } = refreshToken;

    // 检查是否包含刷新令牌
    public bool HasRefreshToken => !string.IsNullOrEmpty(RefreshToken);
}