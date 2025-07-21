namespace Linger.Results.UnitTests;

public class ResultTTests
{
    private readonly TestUser _testUser = new("John", "john@example.com");

    [Fact]
    public void Success_WithValue_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result<TestUser>.Success(_testUser);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.Equal(_testUser, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Success_UsingResultStaticMethod_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success(_testUser);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.Equal(_testUser, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_WithMessage_ShouldCreateFailedResult()
    {
        // Arrange
        const string errorMessage = "User creation failed";

        // Act
        var result = Result<TestUser>.Failure(errorMessage);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Single(result.Errors);
        Assert.Equal(errorMessage, result.Errors.First().Message);
    }

    [Fact]
    public void Failure_WithError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("USER_ERROR", "User validation failed");

        // Act
        var result = Result<TestUser>.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Single(result.Errors);
        Assert.Equal(error, result.Errors.First());
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundResult()
    {
        // Act
        var result = Result<TestUser>.NotFound();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Single(result.Errors);
        Assert.Equal("Not Found.", result.Errors.First().Message);
    }

    [Fact]
    public void NotFound_WithMessage_ShouldCreateNotFoundResult()
    {
        // Arrange
        const string message = "User with ID 123 not found";

        // Act
        var result = Result<TestUser>.NotFound(message);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Single(result.Errors);
        Assert.Equal(message, result.Errors.First().Message);
    }

    [Fact]
    public void Create_WithNonNullValue_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result<TestUser>.Create(_testUser);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.Equal(_testUser, result.Value);
    }

    [Fact]
    public void Create_WithNullValue_ShouldCreateFailureResult()
    {
        // Act
        var result = Result<TestUser>.Create(null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Single(result.Errors);
        Assert.Equal("Value cannot be null.", result.Errors.First().Message);
    }

    [Fact]
    public void TryGetValue_OnSuccess_ShouldReturnTrueAndValue()
    {
        // Arrange
        var result = Result<TestUser>.Success(_testUser);

        // Act
        bool success = result.TryGetValue(out var value);

        // Assert
        Assert.True(success);
        Assert.Equal(_testUser, value);
    }

    [Fact]
    public void TryGetValue_OnFailure_ShouldReturnFalseAndDefault()
    {
        // Arrange
        var result = Result<TestUser>.Failure("Test error");

        // Act
        bool success = result.TryGetValue(out var value);

        // Assert
        Assert.False(success);
        Assert.Equal(default(TestUser), value);
    }

    [Fact]
    public void ValueOrDefault_OnSuccess_ShouldReturnValue()
    {
        // Arrange
        var result = Result<TestUser>.Success(_testUser);

        // Act
        var value = result.ValueOrDefault;

        // Assert
        Assert.Equal(_testUser, value);
    }

    [Fact]
    public void ValueOrDefault_OnFailure_ShouldReturnDefault()
    {
        // Arrange
        var result = Result<TestUser>.Failure("Test error");

        // Act
        var value = result.ValueOrDefault;

        // Assert
        Assert.Equal(default(TestUser), value);
    }

    [Fact]
    public void GetValueOrDefault_WithDefaultValue_OnSuccess_ShouldReturnValue()
    {
        // Arrange
        var result = Result<TestUser>.Success(_testUser);
        var defaultUser = new TestUser("Default", "default@example.com");

        // Act
        var value = result.GetValueOrDefault(defaultUser);

        // Assert
        Assert.Equal(_testUser, value);
    }

    [Fact]
    public void GetValueOrDefault_WithDefaultValue_OnFailure_ShouldReturnDefault()
    {
        // Arrange
        var result = Result<TestUser>.Failure("Test error");
        var defaultUser = new TestUser("Default", "default@example.com");

        // Act
        var value = result.GetValueOrDefault(defaultUser);

        // Assert
        Assert.Equal(defaultUser, value);
    }

    [Fact]
    public void Match_OnSuccess_ShouldCallSuccessFunction()
    {
        // Arrange
        var result = Result<TestUser>.Success(_testUser);
        const string expectedOutput = "Success: John";

        // Act
        var output = result.Match(
            user => $"Success: {user.Name}",
            errors => $"Error: {string.Join(", ", errors.Select(e => e.Message))}"
        );

        // Assert
        Assert.Equal(expectedOutput, output);
    }

    [Fact]
    public void Match_OnFailure_ShouldCallFailureFunction()
    {
        // Arrange
        var result = Result<TestUser>.Failure("Test error");
        const string expectedOutput = "Error: Test error";

        // Act
        var output = result.Match(
            user => $"Success: {user.Name}",
            errors => $"Error: {string.Join(", ", errors.Select(e => e.Message))}"
        );

        // Assert
        Assert.Equal(expectedOutput, output);
    }

    [Fact]
    public void Match_OnSuccess_ShouldCallSuccessAction()
    {
        // Arrange
        var result = Result<TestUser>.Success(_testUser);
        var called = false;
        string? calledWithName = null;

        // Act
        result.Match(
            user =>
            {
                called = true;
                calledWithName = user.Name;
            },
            errors => { }
        );

        // Assert
        Assert.True(called);
        Assert.Equal(_testUser.Name, calledWithName);
    }

    [Fact]
    public void Match_OnFailure_ShouldCallFailureAction()
    {
        // Arrange
        var result = Result<TestUser>.Failure("Test error");
        var called = false;
        string? errorMessage = null;

        // Act
        result.Match(
            user => { },
            errors =>
            {
                called = true;
                errorMessage = errors.First().Message;
            }
        );

        // Assert
        Assert.True(called);
        Assert.Equal("Test error", errorMessage);
    }

    private record TestUser(string Name, string Email);
}
