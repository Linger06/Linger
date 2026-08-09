namespace Linger.Results.UnitTests;

public class BasicResultTests
{
    [Fact]
    public void Map_WhenResultIsNotFound_ShouldPreserveStatus()
    {
        var result = Result<string>.NotFound("Value not found");

        var mapped = result.Map(value => value.Length);

        Assert.Equal(ResultStatus.NotFound, mapped.Status);
        Assert.Equal("Value not found", mapped.FirstError.Message);
    }

    [Fact]
    public void Bind_WhenResultIsNotFound_ShouldPreserveStatus()
    {
        var result = Result<string>.NotFound("Value not found");

        var bound = result.Bind(value => Result<int>.Success(value.Length));

        Assert.Equal(ResultStatus.NotFound, bound.Status);
        Assert.Equal("Value not found", bound.FirstError.Message);
    }

    [Fact]
    public async Task MapAsync_WhenResultIsNotFound_ShouldPreserveStatus()
    {
        var result = Result<string>.NotFound("Value not found");

        var mapped = await result.MapAsync((value, _) => Task.FromResult(value.Length));

        Assert.Equal(ResultStatus.NotFound, mapped.Status);
        Assert.Equal("Value not found", mapped.FirstError.Message);
    }

    [Fact]
    public async Task BindAsync_WhenResultIsNotFound_ShouldPreserveStatus()
    {
        var result = Result<string>.NotFound("Value not found");

        var bound = await result.BindAsync((value, _) => Task.FromResult(Result<int>.Success(value.Length)));

        Assert.Equal(ResultStatus.NotFound, bound.Status);
        Assert.Equal("Value not found", bound.FirstError.Message);
    }

    [Fact]
    public async Task TaskResult_MapAsyncAndBindAsync_ShouldCompose()
    {
        Task<Result<string>> resultTask = Task.FromResult(Result.Success("test"));

        var result = await resultTask
            .MapAsync((value, _) => Task.FromResult(value.Length))
            .BindAsync((value, _) => Task.FromResult(Result.Success(value.ToString())));

        Assert.True(result.IsSuccess);
        Assert.Equal("4", result.Value);
    }

    [Fact]
    public async Task TaskResult_MapAsync_WhenResultIsNotFound_ShouldSkipMapping()
    {
        var mapCalled = false;
        Task<Result<string>> resultTask = Task.FromResult(Result<string>.NotFound("Value not found"));

        var result = await resultTask.MapAsync((value, _) =>
        {
            mapCalled = true;

            return Task.FromResult(value.Length);
        });

        Assert.False(mapCalled);
        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal("Value not found", result.FirstError.Message);
    }

    [Fact]
    public async Task MatchAsync_WhenResultIsSuccessful_ShouldInvokeSuccessBranch()
    {
        var successValue = string.Empty;
        var failureCalled = false;
        var result = Result.Success("test");

        await result.MatchAsync(
            (value, _) =>
            {
                successValue = value;

                return Task.CompletedTask;
            },
            (_, _) =>
            {
                failureCalled = true;

                return Task.CompletedTask;
            });

        Assert.Equal("test", successValue);
        Assert.False(failureCalled);
    }

    [Fact]
    public async Task MatchAsync_WhenResultFails_ShouldInvokeFailureBranch()
    {
        var successCalled = false;
        var capturedError = Error.None;
        var result = Result<string>.Failure(new Error("Failure", "Operation failed"));

        await result.MatchAsync(
            (_, _) =>
            {
                successCalled = true;

                return Task.CompletedTask;
            },
            (errors, _) =>
            {
                capturedError = errors.Single();

                return Task.CompletedTask;
            });

        Assert.False(successCalled);
        Assert.Equal("Failure", capturedError.Code);
    }

    [Fact]
    public async Task EnsureAsync_WhenPredicateFails_ShouldReturnFailure()
    {
        var result = Result.Success("test");
        var error = new Error("Length", "Value is too short");

        var ensured = await result.EnsureAsync(
            (value, _) => Task.FromResult(value.Length > 10),
            error);

        Assert.False(ensured.IsSuccess);
        Assert.Equal(error, ensured.FirstError);
    }

    [Fact]
    public void Failure_WhenSourceCollectionChanges_ShouldKeepErrorSnapshot()
    {
        var errors = new List<Error> { new("Initial", "Initial error") };
        var result = Result.Failure(errors);

        errors.Add(new Error("Later", "Later error"));

        Assert.Single(result.Errors);
        Assert.Equal("Initial", result.FirstError.Code);
    }

    [Fact]
    public void GenericFailure_WhenSourceCollectionChanges_ShouldKeepErrorSnapshot()
    {
        var errors = new List<Error> { new("Initial", "Initial error") };
        var result = Result<string>.Failure(errors);

        errors.Clear();

        Assert.Single(result.Errors);
        Assert.Equal("Initial", result.FirstError.Code);
    }
}
