namespace Linger.Results.UnitTests;

public class IntegrationTests
{
    [Fact]
    public void RealWorldScenario_UserValidationAndCreation_ShouldWorkCorrectly()
    {
        // Arrange
        var userService = new UserService();
        var validRequest = new CreateUserRequest("john.doe", "john@example.com", "Password123!");
        var invalidRequest = new CreateUserRequest("", "invalid-email", "weak");

        // Act - Valid request
        var validResult = userService.CreateUser(validRequest);

        // Act - Invalid request
        var invalidResult = userService.CreateUser(invalidRequest);

        // Assert - Valid result
        Assert.True(validResult.IsSuccess);
        Assert.Equal("john.doe", validResult.Value.Username);
        Assert.Equal("john@example.com", validResult.Value.Email);

        // Assert - Invalid result
        Assert.False(invalidResult.IsSuccess);
        Assert.Equal(ResultStatus.Error, invalidResult.Status);
        Assert.True(invalidResult.Errors.Count() >= 2); // Username and email validation errors
    }

    [Fact]
    public void ChainedOperations_ShouldWorkWithImplicitConversions()
    {
        // Arrange
        var userService = new UserService();

        // Act - Create user and then authenticate
        var createResult = userService.CreateUser(new CreateUserRequest("testuser", "test@example.com", "Password123!"));
        
        Result<string> authResult = createResult.IsSuccess 
            ? userService.AuthenticateUser("testuser", "Password123!")
            : Result.Failure("User creation failed");

        // Assert
        Assert.True(createResult.IsSuccess);
        Assert.True(authResult.IsSuccess);
        Assert.Equal("Authentication successful for testuser", authResult.Value);
    }

    [Fact]
    public void ErrorHandling_WithCustomErrorTypes_ShouldWorkCorrectly()
    {
        // Arrange
        var userService = new UserService();

        // Act - Try to authenticate non-existent user
        var result = userService.AuthenticateUser("nonexistent", "password");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal("USER_NOT_FOUND", result.Errors.First().Code);
    }

    // Test helper classes
    private class UserService
    {
        private readonly List<User> _users = new();

        public Result<User> CreateUser(CreateUserRequest request)
        {
            var errors = new List<Error>();

            // Validate username
            if (string.IsNullOrEmpty(request.Username))
            {
                errors.Add(UserErrors.InvalidUsername);
            }

            // Validate email
            if (!IsValidEmail(request.Email))
            {
                errors.Add(UserErrors.InvalidEmail);
            }

            // Validate password
            if (!IsValidPassword(request.Password))
            {
                errors.Add(UserErrors.WeakPassword);
            }

            // If there are validation errors, return them
            if (errors.Count > 0)
            {
                return Result.Failure(errors.ToArray());
            }

            // Check if user already exists
            if (_users.Any(u => u.Username == request.Username))
            {
                return Result.Failure(new Error("USERNAME_TAKEN", "Username already taken"));
            }

            // Create user
            var user = new User(request.Username, request.Email);
            _users.Add(user);

            return Result<User>.Success(user);
        }

        public Result<string> AuthenticateUser(string username, string password)
        {
            var user = _users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return Result.NotFound(UserErrors.UserNotFound);
            }

            // Simulate password validation
            if (password == "Password123!")
            {
                return Result.Success($"Authentication successful for {username}");
            }

            return Result.Failure(UserErrors.InvalidCredentials);
        }

        private static bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains('@') && email.Contains('.');
        }

        private static bool IsValidPassword(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Length >= 8;
        }
    }

    private static class UserErrors
    {
        public static readonly Error InvalidUsername = new("INVALID_USERNAME", "Username cannot be empty");
        public static readonly Error InvalidEmail = new("INVALID_EMAIL", "Email format is invalid");
        public static readonly Error WeakPassword = new("WEAK_PASSWORD", "Password must be at least 8 characters");
        public static readonly Error UserNotFound = new("USER_NOT_FOUND", "User not found");
        public static readonly Error InvalidCredentials = new("INVALID_CREDENTIALS", "Invalid username or password");
    }

    private record CreateUserRequest(string Username, string Email, string Password);
    private record User(string Username, string Email);
}
