namespace Linger.Results.UnitTests;

public class ResultCombineTests
{
    [Fact]
    public void Combine_WithAllSuccessResults_ShouldReturnSuccess()
    {
        // Arrange
        var results = new[] { Result.Success(), Result.Success(), Result.Success() };

        // Act
        var combined = results.Combine();

        // Assert
        Assert.True(combined.IsSuccess);
        Assert.Equal(ResultStatus.Ok, combined.Status);
        Assert.Empty(combined.Errors);
    }

    [Fact]
    public void Combine_WithOneFailureResult_ShouldReturnFailureWithAllErrors()
    {
        // Arrange
        var results = new[] 
        { 
            Result.Success(), 
            Result.Failure("Second operation failed"), 
            Result.Success() 
        };

        // Act
        var combined = results.Combine();

        // Assert
        Assert.False(combined.IsSuccess);
        Assert.Equal(ResultStatus.Error, combined.Status);
        Assert.Single(combined.Errors);
        Assert.Equal("Second operation failed", combined.Errors.First().Message);
    }

    [Fact]
    public void Combine_WithMultipleFailureResults_ShouldCombineAllErrors()
    {
        // Arrange
        var results = new[] 
        { 
            Result.Failure("First error"), 
            Result.Success(), 
            Result.Failure("Third error") 
        };

        // Act
        var combined = results.Combine();

        // Assert
        Assert.False(combined.IsSuccess);
        Assert.Equal(ResultStatus.Error, combined.Status);
        Assert.Equal(2, combined.Errors.Count());
        Assert.Contains(combined.Errors, e => e.Message == "First error");
        Assert.Contains(combined.Errors, e => e.Message == "Third error");
    }

    [Fact]
    public void Combine_WithEmptyCollection_ShouldReturnSuccess()
    {
        // Arrange
        var results = Array.Empty<Result>();

        // Act
        var combined = results.Combine();

        // Assert
        Assert.True(combined.IsSuccess);
        Assert.Equal(ResultStatus.Ok, combined.Status);
        Assert.Empty(combined.Errors);
    }

    [Fact]
    public void Combine_WithGenericResults_ShouldReturnNonGenericResult()
    {
        // Arrange
        var results = new[] 
        { 
            Result<string>.Success("test1"),
            Result<string>.Success("test2"),
            Result<string>.Failure("Error in third")
        };

        // Act
        var combined = results.Combine();

        // Assert
        Assert.False(combined.IsSuccess);
        Assert.Equal(ResultStatus.Error, combined.Status);
        Assert.Single(combined.Errors);
        Assert.Equal("Error in third", combined.Errors.First().Message);
    }

    [Fact]
    public void Combine_WithMixedStatusResults_ShouldCombineAllFailures()
    {
        // Arrange
        var results = new[] 
        { 
            Result.Success(),
            Result.NotFound("Resource not found"),
            Result.Failure("Processing error")
        };

        // Act
        var combined = results.Combine();

        // Assert
        Assert.False(combined.IsSuccess);
        Assert.Equal(ResultStatus.Error, combined.Status); // Combine returns Error status
        Assert.Equal(2, combined.Errors.Count());
        Assert.Contains(combined.Errors, e => e.Message == "Resource not found");
        Assert.Contains(combined.Errors, e => e.Message == "Processing error");
    }
}