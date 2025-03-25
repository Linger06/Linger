using Microsoft.AspNetCore.Mvc;
using Linger.API.Models;
using Linger.API.Services;

namespace Linger.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(JwtService jwtService, ILogger<AuthController> logger)
        {
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation($"尝试登录用户: {request.Username}");

            // 简单的示例验证，实际应用中应该查询数据库验证用户
            if (request.Username == "admin" && request.Password == "password")
            {
                try
                {
                    var token = _jwtService.GenerateToken(request.Username);
                    _logger.LogInformation($"用户 {request.Username} 登录成功");
                    
                    return Ok(new LoginResponse 
                    { 
                        Success = true, 
                        Token = token,
                        Message = "登录成功"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"生成令牌失败: {ex.Message}");
                    return StatusCode(500, new LoginResponse 
                    { 
                        Success = false, 
                        Message = "服务器内部错误：令牌生成失败"
                    });
                }
            }

            _logger.LogWarning($"用户 {request.Username} 登录失败：凭据无效");
            return Unauthorized(new LoginResponse 
            { 
                Success = false, 
                Message = "用户名或密码不正确"
            });
        }
    }
}
