# Linger.Results.AspNetCore

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

Linger.Results.AspNetCore provides extension methods that seamlessly integrate the Linger.Results library with ASP.NET Core framework, allowing API controllers to easily return results in a unified format.

## Features

- Elegantly convert `Result` and `Result<T>` objects to ASP.NET Core `ActionResult`
- Automatically select appropriate HTTP status codes based on result status
- Support for custom success and failure status codes
- Provide RFC 7807 standard ProblemDetails format output
- Separate business logic from HTTP protocol details

## Important Note

**Special Note**: When calling the `ToActionResult()` method, in successful cases, the returned value is `result.Value` (for `Result<T>`), not the entire `result` object. This makes API return values more concise, allowing clients to get the data they need directly without having to handle an extra wrapper layer.

For example, when a service returns `Result<UserDto>` and converts it via `ToActionResult()`, the client will receive the `UserDto` JSON directly, not a wrapper object containing properties like `Value`, `IsSuccess`, etc.

## Supported Framework Versions

- .NET Core 3.1+ 
- .NET 5.0+
- .NET 6.0+
- .NET 7.0+
- .NET 8.0+

## Installation

```shell
dotnet add package Linger.Results.AspNetCore
```

## Basic Usage

### Using ToActionResult Extension Method in Controllers

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
        // Service layer returns Result<UserDto>
        var result = await _userService.GetUserByIdAsync(id);
        
        // Automatically converts to appropriate HTTP response
        // On success, returns UserDto object (result.Value), not the entire Result object
        return result.ToActionResult();
    }
    
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        
        // On success, returns 201 Created status code and UserDto object (result.Value)
        return result.ToActionResult(successStatusCode: StatusCodes.Status201Created);
    }
}
```

### Return Value Examples

Assume the service layer method returns the following result:

```csharp
// Service layer
public async Task<Result<UserDto>> GetUserByIdAsync(int id)
{
    var user = await _repository.GetByIdAsync(id);
    if (user == null)
        return Result<UserDto>.NotFound($"User with ID {id} not found");
        
    var userDto = new UserDto 
    { 
        Id = user.Id, 
        Name = user.Name, 
        Email = user.Email 
    };
    
    return Result.Success(userDto);
}
```

When the controller calls `result.ToActionResult()`:

**On success**, the client receives JSON:
```json
{
  "id": 123,
  "name": "John Doe",
  "email": "john.doe@example.com"
}
```

**Instead of**:
```json
{
  "value": {
    "id": 123,
    "name": "John Doe",
    "email": "john.doe@example.com"
  },
  "isSuccess": true,
  "status": "Ok",
  "errors": []
}
```

**On failure**, the JSON received by the client depends on the error type and status code:

1. **NotFound error** (HTTP 404):
```json
{
  "errors": [
    {
      "code": "Error.NotFound",
      "message": "User with ID 456 not found"
    }
  ]
}
```

2. **Validation errors** (HTTP 400):
```json
{
  "errors": [
    {
      "code": "User.InvalidEmail",
      "message": "Email format is invalid"
    },
    {
      "code": "User.WeakPassword",
      "message": "Password strength is insufficient"
    }
  ]
}
```

3. **When using ToProblemDetails() method** (RFC 7807 format):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more errors occurred",
  "status": 400,
  "detail": "Email format is invalid; Password strength is insufficient",  "errors": ["Email format is invalid", "Password strength is insufficient"]
}
```

4. **Business rule errors** (HTTP 400):
```json
{
  "errors": [
    {
      "code": "Order.InsufficientStock",
      "message": "Insufficient stock: current stock: 5, requested quantity: 10"
    }
  ]
}
```

For non-generic `Result` in controllers, on failure it now also only returns error information:

```csharp
// Controller
public ActionResult DeleteUser(int id)
{
    var result = _userService.DeleteUser(id);
    return result.ToActionResult();
}

// If deletion fails, the returned JSON:
{
  "errors": [
    {
      "code": "User.CannotDelete",
      "message": "Cannot delete, this user has associated orders"
    }
  ]
}
```

This way, whether it's `Result` or `Result<T>`, they behave consistently when converted to ActionResult:
- On success: `Result` returns an empty object ({}), `Result<T>` returns the value of T
- On failure: Both return only the error information part, not the entire result object

### Automatic Status Code Mapping

```csharp
// In the service layer, return different Result statuses based on different scenarios
public async Task<Result<UserDto>> GetUserByIdAsync(int id)
{
    var user = await _repository.GetByIdAsync(id);
    
    if (user == null)
        return Result<UserDto>.NotFound($"User with ID {id} not found"); // Maps to 404 Not Found
        
    return Result.Success(user.ToDto()); // Maps to 200 OK
}
```

### Using ProblemDetails Format

```csharp
[HttpGet("{id}")]
public async Task<ActionResult> GetUserWithProblemDetails(int id)
{
    var result = await _userService.GetUserByIdAsync(id);
    
    // Returns error information in RFC 7807 format
    return result.ToProblemDetails();
}
```

## Advanced Usage

### Custom Failure Status Codes

```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteUser(int id)
{
    var result = await _userService.DeleteUserAsync(id);
    
    // On success returns 204 No Content, on failure automatically selects status code based on Result status
    return result.ToActionResult(successStatusCode: StatusCodes.Status204NoContent);
}
```

### Using Multiple Results Together

```csharp
[HttpPost("transfer")]
public async Task<ActionResult> TransferMoney(TransferRequest request)
{
    // Validate source account
    var sourceResult = await _accountService.ValidateAccountAsync(request.SourceAccountId);
    if (sourceResult.IsFailure)
        return sourceResult.ToActionResult();
        
    // Validate target account
    var targetResult = await _accountService.ValidateAccountAsync(request.TargetAccountId);
    if (targetResult.IsFailure)
        return targetResult.ToActionResult();
    
    // Execute transfer
    var transferResult = await _accountService.TransferAsync(
        request.SourceAccountId, 
        request.TargetAccountId, 
        request.Amount);
        
    return transferResult.ToActionResult();
}
```

## Best Practices

1. **Clear Separation of Concerns**: Service layer returns business results (`Result`), controller converts them to HTTP responses
2. **Consistent API Responses**: Maintain consistency in API response formats for easier client-side handling
3. **Direct Value Return**: On success, directly return `result.Value` for more concise API responses
4. **Leverage Status Mapping**: Extend the `ResultStatus` enum to accommodate more business scenarios
5. **Prefer Using `ToProblemDetails`**: For client-facing APIs, use RFC 7807 compliant error formats whenever possible

## Using with Linger.Results

This library is an extension of [Linger.Results](../Linger.Results/README.md). Please also refer to the Linger.Results documentation to learn more about result object functionality.

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