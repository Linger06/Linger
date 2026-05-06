# Linger.Results.AspNetCore

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
        return result.ToActionResult(HttpStatusCode.Created);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return result.ToActionResult(HttpStatusCode.NoContent); // 成功返回204，失败返回错误
    }
    
    // 兼容返回类型为 IActionResult 的方法签名
    // IActionResult 接口已由 ActionResult 实现，无需专门转换
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
        return result.ToActionResult(); // 直接返回 ActionResult 给 IActionResult 类型
    }
}
```

### Minimal API中使用

```csharp
var app = WebApplication.Create();

app.MapGet("/api/users/{id}", async (int id, IUserService userService) =>
{
    var result = await userService.GetUserByIdAsync(id);
    return result.ToHttpResult(); // 自动状态码映射
});

app.MapPost("/api/users", async (CreateUserRequest request, IUserService userService) =>
{
    var result = await userService.CreateUserAsync(request);
    // 需要检查 IsSuccess，因为访问 result.Value 需要 result 成功
    return result.IsSuccess
      ? result.ToCreatedResult($"/api/users/{result.Value.Id}")
      : result.ToHttpResult(); // 失败时自动返回 ProblemDetails
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
| 自动状态码 | `result.ToActionResult()` | `result.ToHttpResult()` |
| 自定义成功状态码 | `result.ToActionResult(successStatusCode: HttpStatusCode.Created)` | `result.ToHttpResult(successStatusCode: HttpStatusCode.Created)` |
| 自定义两个状态码 | `result.ToActionResult(HttpStatusCode.Created, HttpStatusCode.Conflict)` | `result.ToHttpResult(HttpStatusCode.Created, HttpStatusCode.Conflict)` |
| Created响应 | `result.ToActionResult(HttpStatusCode.Created)` | `result.ToCreatedResult("/api/users/123")` |
| NoContent响应 | `result.ToActionResult(HttpStatusCode.NoContent)` | `result.ToNoContentResult()` |
| 问题详情 | `result.ToProblemDetails()` | `result.ToHttpResult()` (自动使用ProblemDetails) |

## 何时使用

- 大多数标准 CRUD 操作使用 `ToActionResult()` / `ToHttpResult()`（自动映射能正确处理状态码）
- POST 端点使用 `ToCreatedResult()` 返回 201 Created 和 location header
- DELETE/PUT 端点使用 `ToNoContentResult()` 成功时返回 204 No Content
- 需要明确 RFC 7807 问题详情格式时使用 `ToProblemDetails()`

> **状态码参数说明：**
> - `ToActionResult()` / `ToActionResult<T>()` / `ToHttpResult()` / `ToHttpResult<T>()`：`successStatusCode` 和 `failureStatusCode` 都是可选的。未指定时，`successStatusCode` 默认为 200 OK；`failureStatusCode` 根据 `Result.Status` 自动确定（如 `NotFound` → 404，其他 → 400）
> - `ToProblemDetails()` / `ToProblemDetails<T>()`：仅 `failureStatusCode` 是可选的，自动确定规则同上
> - `ToCreatedResult()` / `ToCreatedResult<T>()` / `ToNoContentResult()`：无状态码参数
> 所有方法使用 `System.Net.HttpStatusCode` 枚举提供类型安全和 IDE 智能提示。

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
  "errors": {
    "User.InvalidEmail": "邮箱格式不正确",
    "User.WeakPassword": "密码强度不足"
  }
}
```

说明：`errors` 为 RFC 7807 的扩展成员，服务端通过在 `ProblemDetails.Extensions["errors"]` 中添加键值对实现。ASP.NET Core 在序列化时会将 `Extensions` 中的条目作为顶层属性输出，因此你会看到顶层的 `errors` 字段。这种做法符合 RFC 7807 对扩展成员的规范。

## 状态码映射

- `Result.Success()` → 200 OK
- `Result.NotFound()` → 404 Not Found  
- `Result.Failure()` → 400 Bad Request

## 最佳实践

1. **关注点分离**：服务层返回 `Result`，控制器负责转换为HTTP响应
2. **一致的API响应**：在整个应用中保持统一的错误格式
3. **使用ProblemDetails**：面向客户端的API优先使用RFC 7807格式
4. **选择合适的API风格**：复杂场景用控制器，简单场景用Minimal API


## 设计决策：为什么没有 `IActionResult` 转换方法？

`ActionResult` 和 `ActionResult<T>` 是推荐的现代方式，而我们特意不提供 `IActionResult` 转换方法，原因如下：

- **类型安全性**：`ActionResult<T>` 是泛型的，能完整保留返回值的类型信息，便于框架进行OpenAPI/Swagger生成、类型检查等。而 `IActionResult` 是非泛型接口，会丢失泛型类型，不利于工具链支持。

- **无缝兼容**：`ActionResult` 和 `ActionResult<T>` 都已实现 `IActionResult` 接口。当你的方法签名需要返回 `IActionResult` 时，可以直接返回 `ActionResult` 或 `ActionResult<T>` 的实例，无需额外转换方法。

- **一致性**：我们遵循 ASP.NET Core 官方的最佳实践推荐，优先使用泛型的 `ActionResult<T>` 而非非泛型的 `IActionResult`。

**总结**：如果你的方法返回类型是 `IActionResult`，可以直接使用 `ToActionResult()` 赋给该变量——不需要专门的转换方法。

## 与Linger.Results配合使用

本库是[Linger.Results](../Linger.Results/README.md)的扩展，请同时参考Linger.Results文档以了解更多关于结果对象的功能。
