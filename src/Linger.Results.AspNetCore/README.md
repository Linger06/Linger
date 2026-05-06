# Linger.Results.AspNetCore

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
        return result.ToActionResult(HttpStatusCode.Created);
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return result.ToActionResult(HttpStatusCode.NoContent); // Success returns 204, failure returns error
    }
    
    // Compatible with methods returning IActionResult type signature
    // IActionResult interface is already implemented by ActionResult, no specialized conversion needed
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
        return result.ToActionResult(); // Directly return ActionResult as IActionResult type
    }
}
```

### Using in Minimal API

```csharp
var app = WebApplication.Create();

app.MapGet("/api/users/{id}", async (int id, IUserService userService) =>
{
    var result = await userService.GetUserByIdAsync(id);
    return result.ToHttpResult(); // Automatic status code mapping
});

app.MapPost("/api/users", async (CreateUserRequest request, IUserService userService) =>
{
    var result = await userService.CreateUserAsync(request);
    // Check IsSuccess because accessing result.Value requires the result to be successful
    return result.IsSuccess
      ? result.ToCreatedResult($"/api/users/{result.Value.Id}")
      : result.ToHttpResult(); // Failure returns ProblemDetails automatically
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
| Auto status codes | `result.ToActionResult()` | `result.ToHttpResult()` |
| Custom success code | `result.ToActionResult(successStatusCode: HttpStatusCode.Created)` | `result.ToHttpResult(successStatusCode: HttpStatusCode.Created)` |
| Custom both codes | `result.ToActionResult(HttpStatusCode.Created, HttpStatusCode.Conflict)` | `result.ToHttpResult(HttpStatusCode.Created, HttpStatusCode.Conflict)` |
| Specific created response | `result.ToActionResult(HttpStatusCode.Created)` | `result.ToCreatedResult("/api/users/123")` |
| No content response | `result.ToActionResult(HttpStatusCode.NoContent)` | `result.ToNoContentResult()` |
| Problem details | `result.ToProblemDetails()` | `result.ToHttpResult()` (uses ProblemDetails automatically) |

## When to use

- Use `ToActionResult()` / `ToHttpResult()` for most standard CRUD operations (default auto-mapping handles status codes correctly)
- Use `ToCreatedResult()` for POST endpoints to return 201 Created with location header
- Use `ToNoContentResult()` for DELETE/PUT endpoints to return 204 No Content on success
- Use `ToProblemDetails()` when you explicitly need RFC 7807 Problem Details format response

> **Status Code Parameters:**
> - `ToActionResult()` / `ToActionResult<T>()` / `ToHttpResult()` / `ToHttpResult<T>()`: Both `successStatusCode` and `failureStatusCode` are optional. When not specified, `successStatusCode` defaults to 200 OK; `failureStatusCode` is auto-determined by `Result.Status` (e.g., `NotFound` → 404, others → 400)
> - `ToProblemDetails()` / `ToProblemDetails<T>()`: Only `failureStatusCode` is optional, same auto-determination applies
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
  "errors": {
    "User.InvalidEmail": "Email format is invalid",
    "User.WeakPassword": "Password strength is insufficient"
  }
}
```

Note: `errors` is an RFC 7807 extension member. The server adds it via `ProblemDetails.Extensions["errors"]`. ASP.NET Core serializes `Extensions` entries as top-level properties, so you will see a top-level `errors` field. This is compliant with RFC 7807's extension mechanism.

## Status Code Mapping

- `Result.Success()` → 200 OK
- `Result.NotFound()` → 404 Not Found  
- `Result.Failure()` → 400 Bad Request

## Best Practices

1. **Separation of Concerns**: Service layer returns `Result`, controller handles HTTP response conversion
2. **Consistent API Responses**: Maintain unified error formats across the application
3. **Use ProblemDetails**: Prefer RFC 7807 format for client-facing APIs
4. **Choose Right API Style**: Use Controllers for complex scenarios, Minimal API for simple cases


## Design Decision: Why No `IActionResult` Conversion Methods?

`ActionResult` and `ActionResult<T>` are the recommended modern approach, and we intentionally do not provide `IActionResult` conversion methods for the following reasons:

- **Type Safety**: `ActionResult<T>` is generic and preserves the complete type information of return values, facilitating framework features like OpenAPI/Swagger generation and type checking. `IActionResult` is a non-generic interface that loses generic type information, hindering tool chain support.

- **Seamless Compatibility**: Both `ActionResult` and `ActionResult<T>` already implement the `IActionResult` interface. When your method signature needs to return `IActionResult`, you can directly return an instance of `ActionResult` or `ActionResult<T>` without needing additional conversion methods.

- **Consistency**: We follow ASP.NET Core official best practices by prioritizing the use of generic `ActionResult<T>` over non-generic `IActionResult`.

**Summary**: If your method return type is `IActionResult`, you can directly use `ToActionResult()` and assign it to that variable—no specialized conversion method is needed.

## Using with Linger.Results

This library is an extension of [Linger.Results](../Linger.Results/README.md). Please also refer to the Linger.Results documentation to learn more about result object functionality.
