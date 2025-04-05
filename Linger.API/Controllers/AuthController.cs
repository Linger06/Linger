using Linger.API.Models;
using Linger.AspNetCore.Jwt.Contracts;
using Linger.Results;
using Linger.Results.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Linger.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJwtService jwtService, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation($"尝试登录用户: {request.Username}");

        // 简单的示例验证，实际应用中应该查询数据库验证用户
        if (request.Username == "admin" && request.Password == "password")
        {
            try
            {
                var token = await _jwtService.CreateTokenAsync(request.Username);
                _logger.LogInformation($"用户 {request.Username} 登录成功");

                var response = new LoginResponse
                {
                    Success = true,
                    Token = token.AccessToken,
                    Message = "登录成功"
                };

                return Result.Success(response).ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"生成令牌失败: {ex.Message}");

                var errorResponse = new LoginResponse
                {
                    Success = false,
                    Message = "服务器内部错误：令牌生成失败"
                };

                return Result.Failure(errorResponse.Message)
                    .ToActionResult(failureStatusCode: StatusCodes.Status500InternalServerError);
            }
        }

        _logger.LogWarning($"用户 {request.Username} 登录失败：凭据无效");

        var unauthorizedResponse = new LoginResponse
        {
            Success = false,
            Message = "用户名或密码不正确"
        };

        return Result.Failure(unauthorizedResponse.Message)
            .ToActionResult(failureStatusCode: StatusCodes.Status401Unauthorized);
    }
}
