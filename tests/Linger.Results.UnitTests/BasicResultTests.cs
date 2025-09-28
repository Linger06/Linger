namespace Linger.Results.UnitTests;

public class BasicResultTests
{
    [Fact]
    public void Result_Success_ShouldWork()
    {
        var result = Result.Success();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Result_Failure_ShouldWork()
    {
        var result = Result.Failure("test error");
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void ResultT_Success_ShouldWork()
    {
        var result = Result<string>.Success("test");
        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value);
    }

    [Fact]
    public void ResultT_Failure_ShouldWork()
    {
        var result = Result<string>.Failure("test error");
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromResult_ToResultT_ShouldWork()
    {
        // 这是我们最重要的功能！
        Result<string> GetUser() => Result.Failure("User not found"); // 隐式转换！

        var result = GetUser();
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("User not found", result.Errors.First().Message);
    }

    [Fact]
    public void ImplicitConversion_ElegantSyntax_ShouldWork()
    {
        // 验证优雅的语法
        static Result<User> ValidateAndCreateUser(string email, string name)
        {
            if (string.IsNullOrEmpty(email))
                return Result.Failure(new Error("ValidationError", "邮箱不能为空")); // 隐式转换！

            if (string.IsNullOrEmpty(name))
                return Result.Failure("姓名不能为空"); // 隐式转换！

            return Result<User>.Success(new User { Email = email, Name = name });
        }

        var invalidResult = ValidateAndCreateUser("", "test");
        Assert.False(invalidResult.IsSuccess);
        Assert.Equal("ValidationError", invalidResult.Errors.First().Code);
        Assert.Equal("邮箱不能为空", invalidResult.Errors.First().Message);

        var validResult = ValidateAndCreateUser("test@test.com", "test");
        Assert.True(validResult.IsSuccess);
        Assert.Equal("test@test.com", validResult.Value.Email);
    }

    private class User
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
