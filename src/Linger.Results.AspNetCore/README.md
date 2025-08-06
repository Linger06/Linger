# Linger.Results.AspNetCore

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

Linger.Results.AspNetCore provides extension methods that seamlessly integrate the Linger.Results library with ASP.NET Core framework, allowing API controllers to easily return results in a unified format.

## Features

- Elegantly convert `Result` and `Result<T>` objects to ASP.NET Core `ActionResult`
- Support for **Minimal API** - convert `Result` and `Result<T>` to `IResult` using modern `Results` static class
- Automatically select appropriate HTTP status codes based on result status
- Support for custom success and failure status codes
- Provide RFC 7807 standard ProblemDetails format output

## Supported Framework Versions
- .NET 8.0+

## Installation

```shell
dotnet add package Linger.Results.AspNetCore
```

## Basic Usage

### Using in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return result.ToActionResult(); // Success returns UserDto, failure returns error array
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
        return result.ToProblemDetails(); // Use RFC 7807 format
    }
}
```

### Using in Minimal API

```csharp
var app = WebApplication.Create();

app.MapGet("/api/users/{id}", async (int id, IUserService userService) =>
{
    var result = await userService.GetUserByIdAsync(id);
    return result.ToResult(); // Automatic status code mapping
});

app.MapPost("/api/users", async (CreateUserRequest request, IUserService userService) =>
{
    var result = await userService.CreateUserAsync(request);
    return result.ToResult(StatusCodes.Status201Created);
});

app.MapDelete("/api/users/{id}", async (int id, IUserService userService) =>
{
    var result = await userService.DeleteUserAsync(id);
    return result.ToNoContentResult(); // Success returns 204 No Content
});
```

## API Method Comparison

| Scenario | Controller API | Minimal API |
|----------|----------------|-------------|
| Basic conversion | `result.ToActionResult()` | `result.ToResult()` |
| Custom status codes | `result.ToActionResult(201, 400)` | `result.ToResult(201, 400)` |
| Created response | `result.ToActionResult(201)` | `result.ToCreatedResult("/api/users/123")` |
| No content response | `result.ToActionResult(204)` | `result.ToNoContentResult()` |
| Problem details | `result.ToProblemDetails()` | `result.ToResult()` (uses ProblemDetails automatically) |

## Response Format Examples

### Success Responses
```json
// Result<UserDto> success
{
  "id": 123,
  "name": "John Doe",
  "email": "john.doe@example.com"
}

// Result success
{
  "status": "Ok",
  "isSuccess": true,
  "isFailure": false,
  "errors": []
}
```

### Failure Responses
```json
// Standard error format
[
  {
    "code": "User.NotFound",
    "message": "User with ID 123 not found"
  }
]

// ProblemDetails format (ToProblemDetails())
{
  "type": null,
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "Email format is invalid; Password strength is insufficient",
  "extensions": {
    "errors": {
      "User.InvalidEmail": "Email format is invalid",
      "User.WeakPassword": "Password strength is insufficient"
    }
  }
}
```

## Status Code Mapping

- `Result.Success()` → 200 OK
- `Result.NotFound()` → 404 Not Found  
- `Result.Failure()` → 400 Bad Request

## Best Practices

1. **Separation of Concerns**: Service layer returns `Result`, controller handles HTTP response conversion
2. **Consistent API Responses**: Maintain unified error formats across the application
3. **Use ProblemDetails**: Prefer RFC 7807 format for client-facing APIs
4. **Choose Right API Style**: Use Controllers for complex scenarios, Minimal API for simple cases

## Using with Linger.Results

This library is an extension of [Linger.Results](../Linger.Results/README.md). Please also refer to the Linger.Results documentation to learn more about result object functionality.
