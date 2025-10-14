namespace Linger.Results;

/// <summary>
/// 提供 Result 和 ExecuteResult 之间的兼容层，简化迁移过程
/// </summary>
public static class ResultCompat
{
    /// <summary>
    /// 将 Result 转换为 ExecuteResult
    /// </summary>
    /// <param name="result">要转换的 Result 对象</param>
    /// <returns>等效的 ExecuteResult 对象</returns>
    public static ExecuteResult ToExecuteResult(this Result result)
    {
        if (result.IsSuccess)
            return ExecuteResult.Success();

        var message = result.Errors.FirstOrDefault()?.Message ?? string.Empty;
        return ExecuteResult.Fail(message);
    }

    /// <summary>
    /// 将泛型 Result 转换为泛型 ExecuteResult
    /// </summary>
    /// <typeparam name="TValue">结果值的类型</typeparam>
    /// <param name="result">要转换的 Result 对象</param>
    /// <returns>等效的 ExecuteResult 对象</returns>
    public static ExecuteResult<TValue> ToExecuteResult<TValue>(this Result<TValue> result)
    {
        if (result.IsSuccess)
            return ExecuteResult<TValue>.Success(result.Value);

        var message = result.Errors.FirstOrDefault()?.Message ?? string.Empty;
        return ExecuteResult<TValue>.Fail(message);
    }

    /// <summary>
    /// 将 ExecuteResult 转换为 Result
    /// </summary>
    /// <param name="executeResult">要转换的 ExecuteResult 对象</param>
    /// <returns>等效的 Result 对象</returns>
    public static Result ToResult(this ExecuteResult executeResult)
    {
        if (executeResult.IsSucceed)
            return Result.Success();

        return Result.Failure(executeResult.Message);
    }

    /// <summary>
    /// 将泛型 ExecuteResult 转换为泛型 Result
    /// </summary>
    /// <typeparam name="TValue">结果值的类型</typeparam>
    /// <param name="executeResult">要转换的 ExecuteResult 对象</param>
    /// <returns>等效的 Result 对象</returns>
    public static Result<TValue> ToResult<TValue>(this ExecuteResult<TValue> executeResult)
    {
        if (executeResult.IsSucceed)
            return Result.Success(executeResult.Value);

        return Result<TValue>.Failure(executeResult.Message);
    }
}
