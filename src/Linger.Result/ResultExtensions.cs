namespace Linger.Result;

public static class ResultExtensions
{
    public static Result<TValue> ToTypedResult<TValue>(this Result result, TValue value)
    {
        return result.IsSuccess
            ? Result.Success(value)
            : Result<TValue>.Failure(result.Errors);
    }

    public static Result ToResult<TValue>(this Result<TValue> result)
    {
        return result.IsSuccess
            ? Result.Success()
            : Result.Failure(result.Errors);
    }
}