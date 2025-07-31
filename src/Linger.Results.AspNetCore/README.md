# Linger.Results.AspNetCore

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

Linger.Results.AspNetCore provides extension methods that seamlessly integrate the Linger.Results library with ASP.NET Core framework, allowing API controllers to easily return results in a unified format.

## Features

- Elegantly convert `Result` and `Result<T>` objects to ASP.NET Core `ActionResult`
- Support for **Minimal API** - convert `Result` and `Result<T>` to `IResult` using modern `Results` static class
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

### Using ToResult Extension Method in Minimal API

```csharp
var app = WebApplication.Create();

// Basic usage - automatic status code mapping
app.MapGet("/api/users/{id}", async (int id, IUserService userService) =>
{
    var result = await userService.GetUserByIdAsync(id);
    return result.ToResult(); // Automatically returns Ok(value) or NotFound/BadRequest
});

// Custom status codes
app.MapPost("/api/users", async (CreateUserRequest request, IUserService userService) =>
{
    var result = await userService.CreateUserAsync(request);
    return result.ToResult(StatusCodes.Status201Created);
});

// Created result with location
app.MapPost("/api/products", async (CreateProductRequest request, IProductService productService) =>
{
    var result = await productService.CreateProductAsync(request);
    return result.ToCreatedResult($"/api/products/{result.Value?.Id}");
});

// NoContent result for delete operations
app.MapDelete("/api/users/{id}", async (int id, IUserService userService) =>
{
    var result = await userService.DeleteUserAsync(id);
    return result.ToNoContentResult();
});

app.Run();
```

## API Method Comparison

| Scenario | Controller API | Minimal API |
|----------|----------------|-------------|
| Basic conversion | `result.ToActionResult()` | `result.ToResult()` |
| Custom status codes | `result.ToActionResult(201, 400)` | `result.ToResult(201, 400)` |
| Created response | `result.ToActionResult(201)` | `result.ToCreatedResult("/api/users/123")` |
| No content response | `result.ToActionResult(204)` | `result.ToNoContentResult()` |
| Problem details | `result.ToProblemDetails()` | `result.ToResult()` (uses ProblemDetails automatically) |

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

#### Controllers
```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteUser(int id)
{
    var result = await _userService.DeleteUserAsync(id);
    
    // On success returns 204 No Content, on failure automatically selects status code based on Result status
    return result.ToActionResult(successStatusCode: StatusCodes.Status204NoContent);
}
```

#### Minimal API
```csharp
app.MapPut("/api/users/{id}", async (int id, UpdateUserRequest request, IUserService userService) =>
{
    var result = await userService.UpdateUserAsync(id, request);
    return result.ToResult(StatusCodes.Status200OK, StatusCodes.Status404NotFound);
});
```

### Advanced Minimal API Examples

```csharp
// Multiple operations with error handling
app.MapPost("/api/transfer", async (TransferRequest request, IAccountService accountService) =>
{
    // Validate source account
    var sourceResult = await accountService.ValidateAccountAsync(request.SourceAccountId);
    if (sourceResult.IsFailure)
        return sourceResult.ToResult();
        
    // Validate target account  
    var targetResult = await accountService.ValidateAccountAsync(request.TargetAccountId);
    if (targetResult.IsFailure)
        return targetResult.ToResult();
    
    // Execute transfer
    var transferResult = await accountService.TransferAsync(
        request.SourceAccountId, 
        request.TargetAccountId, 
        request.Amount);
        
    return transferResult.ToResult();
});

// Using different response types
app.MapGet("/api/users", async (IUserService userService) =>
{
    var result = await userService.GetAllUsersAsync();
    return result.ToResult(); // Returns Ok(List<UserDto>) or BadRequest(errors)
});

app.MapPost("/api/users/{id}/activate", async (int id, IUserService userService) =>
{
    var result = await userService.ActivateUserAsync(id);
    return result.ToResult(StatusCodes.Status202Accepted); // Returns 202 Accepted or error
});
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

1. **Clear Separation of Concerns**: Service layer returns business results (`Result`), controller/endpoint converts them to HTTP responses
2. **Consistent API Responses**: Maintain consistency in API response formats for easier client-side handling
3. **Direct Value Return**: On success, directly return `result.Value` for more concise API responses
4. **Leverage Status Mapping**: Extend the `ResultStatus` enum to accommodate more business scenarios
5. **Prefer Using `ToProblemDetails`**: For client-facing APIs, use RFC 7807 compliant error formats whenever possible
6. **Choose the Right API Style**: Use Controllers for complex scenarios requiring features like model binding, filters, etc. Use Minimal API for simple, lightweight endpoints
7. **Consistent Error Handling**: Whether using Controllers or Minimal API, maintain consistent error response formats across your application

## Using with Linger.Results

This library is an extension of [Linger.Results](../Linger.Results/README.md). Please also refer to the Linger.Results documentation to learn more about result object functionality.