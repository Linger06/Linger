namespace Linger.Results.UnitTests;

/// <summary>
/// 验证Result模式的隐式转换功能
/// </summary>
public class ImplicitConversionValidationTests
{
    private readonly TestUser _testUser = new("John", "john@example.com");

    [Fact]
    public void ResultT_To_Result_ConversionShouldWork()
    {
        // Arrange
        Result<TestUser> genericResult = Result<TestUser>.Success(_testUser);

        // Act - 隐式转换 Result<T> -> Result
        Result nonGenericResult = genericResult;

        // Assert
        Assert.True(nonGenericResult.IsSuccess);
        Assert.Equal(ResultStatus.Ok, nonGenericResult.Status);
        Assert.Empty(nonGenericResult.Errors);
    }

    [Fact]
    public void ResultT_To_Result_FailureConversionShouldWork()
    {
        // Arrange
        Result<TestUser> genericResult = Result<TestUser>.Failure("Test error");

        // Act - 隐式转换 Result<T> -> Result
        Result nonGenericResult = genericResult;

        // Assert
        Assert.False(nonGenericResult.IsSuccess);
        Assert.Equal(ResultStatus.Error, nonGenericResult.Status);
        Assert.Single(nonGenericResult.Errors);
        Assert.Equal("Test error", nonGenericResult.Errors.First().Message);
    }

    [Fact]
    public void Result_To_ResultT_ConversionShouldWork()
    {
        // Arrange
        Result nonGenericResult = Result.Success();

        // Act - 隐式转换 Result -> Result<T>
        Result<TestUser> genericResult = nonGenericResult;

        // Assert
        Assert.True(genericResult.IsSuccess);
        Assert.Equal(ResultStatus.Ok, genericResult.Status);
        Assert.Equal(default(TestUser), genericResult.ValueOrDefault);
        Assert.Empty(genericResult.Errors);
    }

    [Fact]
    public void Result_To_ResultT_FailureConversionShouldWork()
    {
        // Arrange
        Result nonGenericResult = Result.Failure("Test error");

        // Act - 隐式转换 Result -> Result<T>
        Result<TestUser> genericResult = nonGenericResult;

        // Assert
        Assert.False(genericResult.IsSuccess);
        Assert.Equal(ResultStatus.Error, genericResult.Status);
        Assert.Throws<InvalidOperationException>(() => genericResult.Value);
        Assert.Single(genericResult.Errors);
        Assert.Equal("Test error", genericResult.Errors.First().Message);
    }

    [Fact]
    public void Object_To_ResultT_ConversionShouldWork()
    {
        // Act - 隐式转换 T -> Result<T>
        Result<TestUser> result = _testUser;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.Equal(_testUser, result.Value);
        Assert.Equal("John", result.Value.Name);
        Assert.Equal("john@example.com", result.Value.Email);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Null_To_ResultT_ConversionShouldCreateFailure()
    {
        // Act - 隐式转换 null -> Result<T>
        Result<TestUser> result = (TestUser?)null;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Single(result.Errors);
        Assert.Equal("Value cannot be null.", result.Errors.First().Message);
    }

    [Fact]
    public void ChainedConversions_ShouldWork()
    {
        // Arrange
        Result<TestUser> originalResult = _testUser;

        // Act - 链式转换 Result<T> -> Result -> Result<T>
        Result nonGeneric = originalResult;
        Result<TestUser> backToGeneric = nonGeneric;

        // Assert
        Assert.True(backToGeneric.IsSuccess);
        Assert.Equal(ResultStatus.Ok, backToGeneric.Status);
        // 注意：经过转换后，值会变为default，因为非泛型Result不保存值
        Assert.Equal(default(TestUser), backToGeneric.ValueOrDefault);
    }

    [Fact]
    public void MethodReturnType_ImplicitConversion_ShouldWork()
    {
        // Act
        var resultFromObject = GetUserById(1);
        var resultFromResult = GetUserById(0);
        var resultFromFailure = GetUserById(-1);

        // Assert
        Assert.True(resultFromObject.IsSuccess);
        Assert.Equal(_testUser, resultFromObject.Value);

        Assert.True(resultFromResult.IsSuccess);
        Assert.Equal(default(TestUser), resultFromResult.ValueOrDefault);

        Assert.False(resultFromFailure.IsSuccess);
        Assert.Equal("User not found", resultFromFailure.Errors.First().Message);
    }

    // 演示不同的返回方式都能隐式转换为 Result<TestUser>
    private Result<TestUser> GetUserById(int id)
    {
        return id switch
        {
            1 => _testUser,                                    // T -> Result<T>
            0 => Result.Success(),                             // Result -> Result<T>
            _ => Result.Failure("User not found")              // Result -> Result<T>
        };
    }

    private record TestUser(string Name, string Email);
}
