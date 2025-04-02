using Linger.API.Models;
using Linger.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Linger.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        UserService userService,
        IWebHostEnvironment environment,
        ILogger<UsersController> logger)
    {
        _userService = userService;
        _environment = environment;
        _logger = logger;
    }

    // GET: api/users/{id}
    [HttpGet("{id}")]
    public IActionResult GetUser(string id)
    {
        var user = _userService.GetUser(id);
        if (user == null)
            return NotFound("用户不存在");

        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public IActionResult CreateUser(UserCreateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("无效的用户数据");

        try
        {
            var newUser = _userService.CreateUser(request);
            return Ok(newUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建用户时出错");
            return StatusCode(500, "创建用户时发生服务器错误");
        }
    }

    // POST: api/users/avatar
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar()
    {
        try
        {
            // 从请求表单中获取userId
            if (!Request.Form.TryGetValue("userId", out var userIdValues))
                return BadRequest("缺少用户ID");

            string userId = userIdValues.ToString();

            // 检查用户是否存在
            var user = _userService.GetUser(userId);
            if (user == null)
                return NotFound("用户不存在");

            // 获取上传的文件
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null)
                return BadRequest("没有上传文件");

            // 创建用户头像目录
            string avatarDirectory = Path.Combine(_environment.WebRootPath, "images", "avatars", userId);
            if (!Directory.Exists(avatarDirectory))
                Directory.CreateDirectory(avatarDirectory);

            // 保存文件
            string uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(avatarDirectory, uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 更新用户头像URL
            string avatarUrl = _userService.UpdateAvatar(userId, uniqueFileName);

            return Ok(avatarUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传头像时出错");
            return StatusCode(500, "上传头像时发生服务器错误");
        }
    }
}
