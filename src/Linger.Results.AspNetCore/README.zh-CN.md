# Linger.Results.AspNetCore

> 📝 *查看此文档: [English](./README.md) | [中文](./README.zh-CN.md)*

Linger.Results.AspNetCore 提供了将 Linger.Results 库与 ASP.NET Core 框架无缝集成的扩展方法，使API控制器可以轻松返回统一格式的结果。

## 功能特点

- 将 `Result` 和 `Result<T>` 对象优雅地转换为 ASP.NET Core 的 `ActionResult`
- 支持 **Minimal API** - 使用现代 `Results` 静态类将 `Result` 和 `Result<T>` 转换为 `IResult`
- 基于结果状态自动选择适当的HTTP状态码
- 支持自定义成功和失败状态码
- 提供符合RFC 7807标准的ProblemDetails格式输出

## 支持的框架版本
- .NET 8.0+

## 安装

```shell
dotnet add package Linger.Results.AspNetCore
```

## 基本用法

### 控制器中使用

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return result.ToActionResult(); // 成功返回UserDto，失败返回错误数组
    }
    
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        return result.ToActionResult(successStatusCode: StatusCodes.Status201Created);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return result.ToProblemDetails(); // 使用RFC 7807格式
    }
}
```

### Minimal API中使用

```csharp
var app = WebApplication.Create();

app.MapGet("/api/users/{id}", async (int id, IUserService userService) =>
{
    var result = await userService.GetUserByIdAsync(id);
    return result.ToResult(); // 自动状态码映射
});

app.MapPost("/api/users", async (CreateUserRequest request, IUserService userService) =>
{
    var result = await userService.CreateUserAsync(request);
    return result.ToResult(StatusCodes.Status201Created);
});

app.MapDelete("/api/users/{id}", async (int id, IUserService userService) =>
{
    var result = await userService.DeleteUserAsync(id);
    return result.ToNoContentResult(); // 成功返回204 No Content
});
```

## API方法对比

| 场景 | 控制器API | Minimal API |
|------|-----------|-------------|
| 基本转换 | `result.ToActionResult()` | `result.ToResult()` |
| 自定义状态码 | `result.ToActionResult(201, 400)` | `result.ToResult(201, 400)` |
| Created响应 | `result.ToActionResult(201)` | `result.ToCreatedResult("/api/users/123")` |
| NoContent响应 | `result.ToActionResult(204)` | `result.ToNoContentResult()` |
| 问题详情 | `result.ToProblemDetails()` | `result.ToResult()` (自动使用ProblemDetails) |

## 返回格式示例

### 成功响应
```json
// Result<UserDto> 成功时
{
  "id": 123,
  "name": "张三",
  "email": "zhangsan@example.com"
}

// Result 成功时
{
  "status": "Ok",
  "isSuccess": true,
  "isFailure": false,
  "errors": []
}
```

### 失败响应
```json
// 标准错误格式
[
  {
    "code": "User.NotFound",
    "message": "ID为123的用户不存在"
  }
]

// ProblemDetails格式 (ToProblemDetails())
{
  "type": null,
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "邮箱格式不正确; 密码强度不足",
  "extensions": {
    "errors": {
      "User.InvalidEmail": "邮箱格式不正确",
      "User.WeakPassword": "密码强度不足"
    }
  }
}
```

## 状态码映射

- `Result.Success()` → 200 OK
- `Result.NotFound()` → 404 Not Found  
- `Result.Failure()` → 400 Bad Request

## 最佳实践

1. **关注点分离**：服务层返回 `Result`，控制器负责转换为HTTP响应
2. **一致的API响应**：在整个应用中保持统一的错误格式
3. **使用ProblemDetails**：面向客户端的API优先使用RFC 7807格式
4. **选择合适的API风格**：复杂场景用控制器，简单场景用Minimal API

## 与Linger.Results配合使用

本库是[Linger.Results](../Linger.Results/README.md)的扩展，请同时参考Linger.Results文档以了解更多关于结果对象的功能。
