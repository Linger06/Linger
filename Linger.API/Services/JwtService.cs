using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Linger.API.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateToken(string username)
        {
            try
            {
                var jwtKey = _configuration["Jwt:Key"] ?? "YourSecretKeyHereMustBeLongEnoughForSecurity123456789";
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? "LingerApiIssuer";
                var jwtAudience = _configuration["Jwt:Audience"] ?? "LingerApiAudience";
                var jwtExpMinutes = Convert.ToDouble(_configuration["Jwt:ExpireMinutes"] ?? "60");

                _logger.LogInformation($"正在为用户 {username} 生成令牌，密钥长度: {jwtKey.Length}");

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(jwtExpMinutes),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                _logger.LogInformation($"令牌生成成功: {tokenString[..10]}...");
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError($"生成令牌时出错: {ex.Message}");
                throw;
            }
        }
    }
}
