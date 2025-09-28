namespace Linger.Results.UnitTests;

public class ImplicitConversionTests
{
    private readonly TestUser _testUser = new("John", "john@example.com");

    [Fact]
    public void ImplicitConversion_FromResultFailure_ToResultT_ShouldPreserveStatus()
    {
        // Arrange
        Result nonGenericResult = Result.Failure("Test error");

        // Act
        Result<TestUser> genericResult = nonGenericResult;

        // Assert
        Assert.False(genericResult.IsSuccess);
        Assert.True(genericResult.IsFailure);
        Assert.Equal(ResultStatus.Error, genericResult.Status);
        Assert.Throws<InvalidOperationException>(() => genericResult.Value);
        Assert.Single(genericResult.Errors);
        Assert.Equal("Test error", genericResult.Errors.First().Message);
    }

    [Fact]
    public void ImplicitConversion_FromResultNotFound_ToResultT_ShouldPreserveStatus()
    {
        // Arrange
        Result nonGenericResult = Result.NotFound("User not found");

        // Act
        Result<TestUser> genericResult = nonGenericResult;

        // Assert
        Assert.False(genericResult.IsSuccess);
        Assert.True(genericResult.IsFailure);
        Assert.Equal(ResultStatus.NotFound, genericResult.Status);
        Assert.Throws<InvalidOperationException>(() => genericResult.Value);
        Assert.Single(genericResult.Errors);
        Assert.Equal("User not found", genericResult.Errors.First().Message);
    }

    [Fact]
    public void ImplicitConversion_FromResultSuccess_ToResultT_ShouldHaveDefaultValue()
    {
        // Arrange
        Result nonGenericResult = Result.Success();

        // Act
        Result<TestUser> genericResult = nonGenericResult;

        // Assert
        Assert.True(genericResult.IsSuccess);
        Assert.False(genericResult.IsFailure);
        Assert.Equal(ResultStatus.Ok, genericResult.Status);
        Assert.Equal(default(TestUser), genericResult.Value);
        Assert.Empty(genericResult.Errors);
    }

    [Fact]
    public void ImplicitConversion_InMethodReturn_ShouldWorkWithFailure()
    {
        // Act
        var result = GetUserWithValidation("");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Equal("Email cannot be empty", result.Errors.First().Message);
    }

    [Fact]
    public void ImplicitConversion_InMethodReturn_ShouldWorkWithNotFound()
    {
        // Act
        var result = GetUserWithValidation("nonexistent@example.com");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal("User not found", result.Errors.First().Message);
    }

    [Fact]
    public void ImplicitConversion_InMethodReturn_ShouldWorkWithSuccess()
    {
        // Act
        var result = GetUserWithValidation("john@example.com");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.Equal("john@example.com", result.Value.Email);
    }

    [Fact]
    public void DirectGenericSuccess_ShouldWorkCorrectly()
    {
        // Act - This uses Result.Success<T>(value), not conversion
        Result<TestUser> result = Result.Success(_testUser);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_testUser, result.Value);
        Assert.Equal("John", result.Value.Name);
    }

    [Fact]
    public void MultipleErrorsConversion_ShouldPreserveAllErrors()
    {
        // Arrange
        var errors = new[]
        {
            new Error("ERROR1", "First error"),
            new Error("ERROR2", "Second error")
        };
        Result nonGenericResult = Result.Failure(errors);

        // Act
        Result<TestUser> genericResult = nonGenericResult;

        // Assert
        Assert.False(genericResult.IsSuccess);
        Assert.Equal(2, genericResult.Errors.Count());
        Assert.Contains(errors[0], genericResult.Errors);
        Assert.Contains(errors[1], genericResult.Errors);
    }

    // 模拟实际使用场景的方法
    private Result<TestUser> GetUserWithValidation(string email)
    {
        // 验证邮箱
        if (string.IsNullOrEmpty(email))
        {
            // 直接返回 Result.Failure，会自动转换为 Result<TestUser>
            return Result.Failure(new Error("ValidationError", "Email cannot be empty"));
        }

        // 模拟查找用户
        if (email == "john@example.com")
        {
            return Result.Success(new TestUser("John", email));
        }

        // 用户不存在 - 使用隐式转换
        return Result.NotFound("User not found");
    }

    private record TestUser(string Name, string Email);
}
