using Linger.API.Models;
using Linger.API.Services;
using Linger.Results;
using Linger.Results.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Linger.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    UserService userService,
    ILogger<UsersController> logger) : ControllerBase
{

    // GET: api/users/{id}
    [HttpGet("{id}")]
    public ActionResult<UserInfo> GetUser(string id)
    {
        var user = userService.GetUser(id);
        if (user == null)
            return Result.NotFound("用户不存在").ToActionResult();

        return Result.Success(user).ToActionResult();
    }

    // POST: api/users
    [HttpPost]
    public ActionResult<UserInfo> CreateUser(UserCreateRequest request)
    {
        if (!ModelState.IsValid)
            return Result.Failure("无效的用户数据").ToActionResult();

        try
        {
            var newUser = userService.CreateUser(request);
            return Result.Success(newUser).ToActionResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "创建用户时出错");
            return Result.Failure("创建用户时发生服务器错误").ToActionResult(failureStatusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // POST: api/users/avatar
    [HttpPost("avatar")]
    public async Task<ActionResult<string>> UploadAvatar()
    {
        try
        {
            // 从请求表单中获取userId
            if (!Request.Form.TryGetValue("userId", out var userIdValues))
                return Result.Failure("缺少用户ID").ToActionResult();

            var userId = userIdValues.ToString();

            // 检查用户是否存在
            var user = userService.GetUser(userId);
            if (user == null)
                return Result.NotFound("用户不存在").ToActionResult();

            // 获取上传的文件
            var file = Request.Form.Files[0];
            if (file == null)
                return Result.Failure("没有上传文件").ToActionResult();

            // 创建用户头像目录
            var avatarDirectory = Path.Combine("images", "avatars", userId);
            if (!Directory.Exists(avatarDirectory))
                Directory.CreateDirectory(avatarDirectory);

            // 保存文件
            var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(avatarDirectory, uniqueFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 更新用户头像URL
            var avatarUrl = userService.UpdateAvatar(userId, uniqueFileName);

            return Result.Success(avatarUrl).ToActionResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "上传头像时出错");
            return Result.Failure("上传头像时发生服务器错误").ToActionResult(failureStatusCode: StatusCodes.Status500InternalServerError);
        }
    }

    // PUT: api/users
    [HttpPut]
    public ActionResult<UserInfo> UpdateUser(UserUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return Result.Failure("无效的用户数据").ToActionResult();

        try
        {
            var updatedUser = userService.UpdateUserInfo(request);
            if (updatedUser == null)
                return Result.NotFound("用户不存在").ToActionResult();

            return Result.Success(updatedUser).ToActionResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新用户时出错");
            return Result.Failure("更新用户时发生服务器错误").ToActionResult(failureStatusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
