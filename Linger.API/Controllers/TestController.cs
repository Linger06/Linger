using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Linger.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController(ILogger<TestController> logger) : ControllerBase
{
    [HttpGet("public")]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { message = "这是一个公开的API端点，无需认证" });
    }

    [HttpGet("protected")]
    [Authorize]
    public IActionResult ProtectedEndpoint()
    {
        var username = User.Identity?.Name ?? "未知用户";
        logger.LogInformation("用户 {Username} 访问了受保护的API端点", username);
        return Ok(new { message = $"你好, {username}! 这是一个受保护的API端点，需要认证" });
    }
}
