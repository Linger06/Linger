# Linger.Results

> 📝 *View this document in: [English](./README.md) | [中文](./README.zh-CN.md)*

A modern operation result handling library designed with a functional style approach, helping developers handle various operation results more elegantly. By using the Result pattern instead of exceptions, it enables more controllable and predictable error handling processes.

## Features

- Provides clear success/failure result representation
- Supports generic results that can carry return values of any type
- Offers rich functional operations (mapping, binding, combining, etc.)
- Strong typing for error handling, more controllable than exceptions
- Supports multiple frameworks including .NET 9.0, .NET 8.0, .NET Standard 2.0, and .NET Framework 4.7.2

## Installation

```shell
dotnet add package Linger.Results
```

## Basic Usage

### Creating Results

```csharp
// Create success results
var success = Result.Success();
var successWithValue = Result.Success(42);

// Create failure results
var failure = Result.Failure("Operation failed");
var failureWithError = Result.Failure(new Error("ErrorCode", "Detailed error message"));

// Create not found results
var notFound = Result.NotFound();
```

### Using Generic Results

```csharp
// Define a method that returns user information
public Result<User> GetUser(int id)
{
    var user = _repository.FindById(id);
    if (user == null)
        return Result<User>.NotFound($"User with ID {id} not found");
    
    return Result<User>.Success(user);
}

// Elegant syntax with implicit conversion
public Result<User> GetUserWithValidation(string email)
{
    // Validate email
    if (string.IsNullOrEmpty(email))
    {
        // Directly return Result.Failure, automatically converts to Result<User>
        return Result.Failure(new Error("ValidationError", "Email cannot be empty"));
    }

    var user = _repository.FindByEmail(email);
    if (user == null)
    {
        // This also converts automatically
        return Result.NotFound("User not found");
    }

    return Result<User>.Success(user);
}

// Usage example
var result = GetUser(123);
if (result.IsSuccess)
{
    var user = result.Value; // Value is only accessible when result is successful
    Console.WriteLine($"Found user: {user.Name}");
}
else
{
    Console.WriteLine($"Error: {string.Join(", ", result.Errors.Select(e => e.Message))}");
}
```

### Using Match Method

```csharp
// Use Match method to handle different result states
string displayName = result.Match(
    user => $"User: {user.Name}",
    errors => $"Error: {string.Join(", ", errors.Select(e => e.Message))}"
);
```

### Safe Value Access

```csharp
// Use TryGetValue to safely access result value
if (result.TryGetValue(out var user))
{
    // Successfully obtained user
    Console.WriteLine($"User: {user.Name}");
}

// Use ValueOrDefault to get value or default value
var safeUser = result.ValueOrDefault; // null when failed

// Specify default value
var userOrGuest = result.GetValueOrDefault(new User { Name = "Guest" });
```

### Method Chaining

```csharp
// Use extension methods for method chaining
var finalResult = GetUser(123)
    .Map(user => user.Email)
    .Bind(email => SendEmail(email))
    .Ensure(success => success, new Error("EmailError", "Email sending failed"));
```

### Async Support

```csharp
// Async operations
var result = await GetUserAsync(123)
    .MapAsync(async user => await GetUserPreferencesAsync(user))
    .BindAsync(async prefs => await UpdatePreferencesAsync(prefs));
```

### Using Result.Create for Condition Checking

```csharp
// Create results based on boolean conditions
public Result ValidatePassword(string password)
{
    return Result.Create(password.Length >= 8)
        .Ensure(() => password.Any(char.IsUpper), new Error("Password", "Password must contain uppercase letters"))
        .Ensure(() => password.Any(char.IsDigit), new Error("Password", "Password must contain digits"));
}
```

## Implicit Conversion and Elegant Syntax

Linger.Results provides powerful implicit conversion features to make your code more concise and elegant:

### Implicit Conversion from Result to Result&lt;T&gt;

```csharp
public Result<User> CreateUser(CreateUserRequest request)
{
    // Validate username
    if (string.IsNullOrEmpty(request.Username))
    {
        // Directly return Result.Failure, automatically converts to Result<User>
        return Result.Failure("Username cannot be empty");
    }
    
    // Check email format
    if (!IsValidEmail(request.Email))
    {
        // Using custom error object, also converts automatically
        return Result.Failure(new Error("Email.Invalid", "Invalid email format"));
    }
    
    // Check if user already exists
    if (UserExists(request.Username))
    {
        // NotFound also supports implicit conversion
        return Result.NotFound("Username is already taken");
    }
    
    // Success case
    var user = new User { Username = request.Username, Email = request.Email };
    return Result<User>.Success(user);
}
```

### API Design Principles

After optimization, Linger.Results follows these design principles:

1. **Clear API Boundaries**: `Result` class focuses on non-generic operations, `Result<T>` class handles operations with return values
2. **Implicit Conversion Support**: Supports natural conversion from `Result` to `Result<T>`, but avoids unexpected value conversions
3. **Type Safety**: Ensures type correctness at compile time, avoiding runtime errors
4. **Concise Syntax**: Reduces boilerplate code and improves development efficiency

### Error Handling

```csharp
// Use Try method to catch exceptions and convert to results
var result = ResultFunctionalExtensions.Try(
    () => SomeOperationThatMightThrow(),
    ex => ex.ToError()
);
```

## Advanced Usage

### Combining Multiple Results

```csharp
// Combine multiple results, succeeds only when all results succeed
var combinedResult = Result.Combine(
    ValidateUsername(request.Username),
    ValidateEmail(request.Email),
    ValidatePassword(request.Password)
);

if (combinedResult.IsSuccess)
{
    // All validations passed
    return Result.Success(new User { /* ... */ });
}
```

### Custom Error Types

```csharp
// Define domain-specific error types
public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "User not found");
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Invalid username or password");
    public static readonly Error DuplicateEmail = new("User.DuplicateEmail", "Email is already in use");
}

// Use custom errors
public Result<User> Authenticate(string username, string password)
{
    var user = _repository.FindByUsername(username);
    if (user == null)
        return Result.Failure(UserErrors.NotFound); // Implicit conversion to Result<User>
        
    if (!ValidatePassword(password, user.PasswordHash))
        return Result.Failure(UserErrors.InvalidCredentials); // Implicit conversion to Result<User>
        
    return Result<User>.Success(user);
}
```

### Conditional Branch Processing

```csharp
public async Task<Result<OrderConfirmation>> ProcessOrder(Order order)
{
    // Chain process order workflow
    return await ValidateOrder(order)
        .BindAsync(async validOrder => 
        {
            // Choose different processing paths based on payment method
            if (validOrder.PaymentMethod == PaymentMethod.CreditCard)
                return await ProcessCreditCardPayment(validOrder);
            else if (validOrder.PaymentMethod == PaymentMethod.BankTransfer)
                return await ProcessBankTransfer(validOrder);
            else
                return Result<OrderConfirmation>.Failure("Unsupported payment method");
        })
        .TapAsync(async confirmation => 
        {
            // Execute side effect operations on success, but don't change the result
            await SendConfirmationEmail(confirmation);
            await UpdateInventory(order);
        });
}
```

## ResultStatus Enum Extension

The default `ResultStatus` enum includes `Ok`, `NotFound`, and `Error`. You can extend this enum to include more statuses as needed:

```csharp
// Create a partial class extension in your project
namespace Linger.Results
{
    public enum ResultStatus
    {
        // Existing statuses
        Ok,
        NotFound,
        Error,
        
        // New custom statuses
        Unauthorized,
        Forbidden,
        Conflict,
        ValidationError
    }
}
```

## Best Practices

1. **Prefer Result and Result<T> over exceptions**:
   - Return Result for expected errors (validation errors, resource not found, etc.)
   - Use exceptions only for truly exceptional situations (program errors, unexpected system failures)

2. **Maintain consistency in return values**:
   - Service methods should consistently return Result or Result<T>, not mix results and exceptions
   - Maintain unified error handling patterns for easier handling by callers

3. **Use meaningful error codes**:
   - Define domain-specific error constants
   - Use structured error codes (like "Category.SubCategory.Error")

4. **Leverage implicit conversion to simplify code**:
   - In methods returning `Result<T>`, you can directly return `Result.Failure()` or `Result.NotFound()`
   - This makes code more concise and elegant while maintaining type safety

5. **Use API correctly**:
   - For generic results, use `Result<T>.Success()`, `Result<T>.Failure()` methods directly
   - Leverage implicit conversion from `Result` to `Result<T>` instead of relying on removed forwarding methods

6. **Leverage method chaining**:
   - Use functional composition instead of traditional conditional statements
   - Map, Bind, Tap methods can greatly improve code readability

7. **For Web APIs**:
   - Combine with [Linger.Results.AspNetCore](../Linger.Results.AspNetCore/README.md) package to convert to HTTP responses
   - Use ProblemDetails format to return standardized error responses

## Comparison with Exception Handling

| Aspect | Result Pattern | Exception Mechanism |
|--------|---------------|-------------------|
| Visibility | Explicit return type, visible at compile time | Implicit throwing, known only at runtime |
| Performance | Better, no stack capture overhead | Worse, especially in high-frequency call scenarios |
| Composability | Excellent, supports chaining and composition operations | Weak, requires multiple try-catch layers |
| Type Safety | Strong typing, compiler assistance | Weak typing, based on string matching |
| Use Cases | Business logic, expected errors | Program errors, unexpected exceptions |

## License

MIT