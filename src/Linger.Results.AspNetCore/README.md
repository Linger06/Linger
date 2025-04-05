# Linger.Results.AspNetCore

Linger.Results.AspNetCore 提供了将 Linger.Results 库与 ASP.NET Core 框架无缝集成的扩展方法，使API控制器可以轻松返回统一格式的结果。

## 功能特点

- 将 `Result` 和 `Result<T>` 对象优雅地转换为 ASP.NET Core 的 `ActionResult`
- 基于结果状态自动选择适当的HTTP状态码
- 支持自定义成功和失败状态码
- 提供符合RFC 7807标准的ProblemDetails格式输出
- 分离业务逻辑与HTTP协议细节

## 重要说明

**特别注意**：当调用 `ToActionResult()` 方法时，成功情况下返回的是 `result.Value`（对于 `Result<T>`），而不是整个 `result` 对象。这使得API返回值更加简洁，客户端可以直接获取所需的数据，而不需要处理额外的包装层。

例如，当一个服务返回 `Result<UserDto>` 并通过 `ToActionResult()` 转换时，客户端将直接收到 `UserDto` JSON，而不是包含 `Value`、`IsSuccess` 等属性的包装对象。

## 支持的框架版本

- .NET Core 3.1+ 
- .NET 5.0+
- .NET 6.0+
- .NET 7.0+
- .NET 8.0+

## 安装

```shell
dotnet add package Linger.Results.AspNetCore
```

## 基本用法

### 在控制器中使用ToActionResult扩展方法

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        // 服务层返回Result<UserDto>
        var result = await _userService.GetUserByIdAsync(id);
        
        // 自动转换为适当的HTTP响应
        // 成功时返回UserDto对象(result.Value)，而不是整个Result对象
        return result.ToActionResult();
    }
    
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        
        // 成功时返回201 Created状态码和UserDto对象(result.Value)
        return result.ToActionResult(successStatusCode: StatusCodes.Status201Created);
    }
}
```

### 返回值示意

假设服务层方法返回以下结果：

```csharp
// 服务层
public async Task<Result<UserDto>> GetUserByIdAsync(int id)
{
    var user = await _repository.GetByIdAsync(id);
    if (user == null)
        return Result<UserDto>.NotFound($"ID为{id}的用户不存在");
        
    var userDto = new UserDto 
    { 
        Id = user.Id, 
        Name = user.Name, 
        Email = user.Email 
    };
    
    return Result.Success(userDto);
}
```

当控制器调用 `result.ToActionResult()` 后：

**成功情况**下，客户端收到的JSON：
```json
{
  "id": 123,
  "name": "张三",
  "email": "zhangsan@example.com"
}
```

**而不是**：
```json
{
  "value": {
    "id": 123,
    "name": "张三",
    "email": "zhangsan@example.com"
  },
  "isSuccess": true,
  "status": "Ok",
  "errors": []
}
```

**失败情况**下，客户端收到的JSON取决于错误类型和状态码：

1. **NotFound错误** (HTTP 404)：
```json
{
  "errors": [
    {
      "code": "Error.NotFound",
      "message": "ID为456的用户不存在"
    }
  ]
}
```

2. **验证错误** (HTTP 400)：
```json
{
  "errors": [
    {
      "code": "User.InvalidEmail",
      "message": "邮箱格式不正确"
    },
    {
      "code": "User.WeakPassword",
      "message": "密码强度不足"
    }
  ]
}
```

3. **使用ToProblemDetails()方法时** (RFC 7807格式)：
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more errors occurred",
  "status": 400,
  "detail": "邮箱格式不正确; 密码强度不足",
  "errors": ["邮箱格式不正确", "密码强度不足"]
}
```

4. **业务规则错误** (HTTP 400)：
```json
{
  "errors": [
    {
      "code": "Order.InsufficientStock",
      "message": "库存不足，当前库存:5，请求数量:10"
    }
  ]
}
```

对于控制器中没有使用泛型的`Result`，失败时现在也只返回错误信息：

```csharp
// 控制器
public ActionResult DeleteUser(int id)
{
    var result = _userService.DeleteUser(id);
    return result.ToActionResult();
}

// 如果删除失败，返回的JSON：
{
  "errors": [
    {
      "code": "User.CannotDelete",
      "message": "无法删除，该用户有关联的订单"
    }
  ]
}
```

这样，无论是`Result`还是`Result<T>`，在转换为ActionResult时都会有一致的行为：
- 成功时：`Result`返回空对象({}), `Result<T>`返回T的值
- 失败时：两者都只返回错误信息部分，不包含整个result对象

### 自动状态码映射

```csharp
// 在服务层，根据不同情况返回不同状态的Result
public async Task<Result<UserDto>> GetUserByIdAsync(int id)
{
    var user = await _repository.GetByIdAsync(id);
    
    if (user == null)
        return Result<UserDto>.NotFound($"ID为{id}的用户不存在"); // 将映射为404 Not Found
        
    return Result.Success(user.ToDto()); // 将映射为200 OK
}
```

### 使用ProblemDetails格式

```csharp
[HttpGet("{id}")]
public async Task<ActionResult> GetUserWithProblemDetails(int id)
{
    var result = await _userService.GetUserByIdAsync(id);
    
    // 返回RFC 7807格式的错误信息
    return result.ToProblemDetails();
}
```

## 高级用法

### 自定义失败状态码

```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteUser(int id)
{
    var result = await _userService.DeleteUserAsync(id);
    
    // 成功返回204 No Content，失败根据Result状态自动选择状态码
    return result.ToActionResult(successStatusCode: StatusCodes.Status204NoContent);
}
```

### 组合使用多个Result

```csharp
[HttpPost("transfer")]
public async Task<ActionResult> TransferMoney(TransferRequest request)
{
    // 验证源账户
    var sourceResult = await _accountService.ValidateAccountAsync(request.SourceAccountId);
    if (sourceResult.IsFailure)
        return sourceResult.ToActionResult();
        
    // 验证目标账户
    var targetResult = await _accountService.ValidateAccountAsync(request.TargetAccountId);
    if (targetResult.IsFailure)
        return targetResult.ToActionResult();
    
    // 执行转账
    var transferResult = await _accountService.TransferAsync(
        request.SourceAccountId, 
        request.TargetAccountId, 
        request.Amount);
        
    return transferResult.ToActionResult();
}
```

## 最佳实践

1. **清晰的关注点分离**：服务层返回业务结果 (`Result`)，控制器负责将其转换为HTTP响应
2. **一致的API响应**：保持API返回格式的一致性，使客户端处理更简单
3. **直接返回值**：成功时直接返回 `result.Value`，让API响应更简洁
4. **利用状态映射**：扩展`ResultStatus`枚举以满足更多业务场景
5. **优先使用`ToProblemDetails`**：对于面向客户端的API，尽可能使用符合RFC 7807的错误格式

## 与Linger.Results配合使用

本库是[Linger.Results](../Linger.Results/README.md)的扩展，请同时参考Linger.Results文档以了解更多关于结果对象的功能。