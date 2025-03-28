namespace Linger.AspNetCore.Jwt.Contracts;

public class Token
{
    public Token(string accessToken, string? refreshToken = null)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public string AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    
    // 检查是否包含刷新令牌
    public bool HasRefreshToken => !string.IsNullOrEmpty(RefreshToken);
}