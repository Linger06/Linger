using Linger.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Linger.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataController : ControllerBase
{
    private static readonly List<DataItem> s_data = new()
    {
        new DataItem { Id = 1, Name = "项目一", Description = "示例项目描述1" },
        new DataItem { Id = 2, Name = "项目二", Description = "示例项目描述2" },
        new DataItem { Id = 3, Name = "项目三", Description = "示例项目描述3" },
        new DataItem { Id = 4, Name = "项目四", Description = "示例项目描述4" },
        new DataItem { Id = 5, Name = "项目五", Description = "示例项目描述5" }
    };

    private readonly ILogger<DataController> _logger;

    public DataController(ILogger<DataController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var username = User.Identity?.Name;
        _logger.LogInformation($"用户 {username} 请求获取所有数据");
        return Ok(s_data);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var item = s_data.FirstOrDefault(d => d.Id == id);
        if (item == null)
            return NotFound();

        return Ok(item);
    }

    [HttpGet("count")]
    public IActionResult GetCount()
    {
        // 确保返回一个原始的整数值，而不是包装对象
        _logger.LogInformation($"请求数据项数量: {s_data.Count}");
        return Ok(s_data.Count);
    }
}
