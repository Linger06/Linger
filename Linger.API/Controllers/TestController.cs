using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Linger.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

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
        _logger.LogInformation($"用户 {username} 访问了受保护的API端点");
        return Ok(new { message = $"你好, {username}! 这是一个受保护的API端点，需要认证" });
    }
}
