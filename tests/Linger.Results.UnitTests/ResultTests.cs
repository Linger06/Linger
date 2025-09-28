namespace Linger.Results.UnitTests;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_WithMessage_ShouldCreateFailedResult()
    {
        // Arrange
        const string errorMessage = "Operation failed";

        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Single(result.Errors);
        Assert.Equal(errorMessage, result.Errors.First().Message);
    }

    [Fact]
    public void Failure_WithError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Single(result.Errors);
        Assert.Equal(error, result.Errors.First());
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldCreateFailedResult()
    {
        // Arrange
        var errors = new[]
        {
            new Error("ERROR1", "First error"),
            new Error("ERROR2", "Second error")
        };

        // Act
        var result = Result.Failure(errors);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Equal(2, result.Errors.Count());
        Assert.Contains(errors[0], result.Errors);
        Assert.Contains(errors[1], result.Errors);
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundResult()
    {
        // Act
        var result = Result.NotFound();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Single(result.Errors);
        Assert.Equal("Not Found.", result.Errors.First().Message);
    }

    [Fact]
    public void NotFound_WithMessage_ShouldCreateNotFoundResult()
    {
        // Arrange
        const string message = "User not found";

        // Act
        var result = Result.NotFound(message);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Single(result.Errors);
        Assert.Equal(message, result.Errors.First().Message);
    }

    [Fact]
    public void Create_WithTrueCondition_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Create(true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Ok, result.Status);
    }

    [Fact]
    public void Create_WithFalseCondition_ShouldCreateFailureResult()
    {
        // Act
        var result = Result.Create(false);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Single(result.Errors);
        Assert.Equal("The specified condition was not met.", result.Errors.First().Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_WithCondition_ShouldMatchExpectation(bool condition)
    {
        // Act
        var result = Result.Create(condition);

        // Assert
        Assert.Equal(condition, result.IsSuccess);
        Assert.Equal(!condition, result.IsFailure);
    }
}
