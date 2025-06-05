namespace Linger.Results;

/// <summary>
/// 为 Result 类型提供异步操作扩展方法
/// </summary>
public static class ResultAsyncExtensions
{
    /// <summary>
    /// 异步映射成功结果的值
    /// </summary>
    /// <typeparam name="TValue">原始值类型</typeparam>
    /// <typeparam name="TResult">映射后的值类型</typeparam>
    /// <param name="result">要映射的结果</param>
    /// <param name="mapFunc">异步映射函数</param>
    /// <returns>包含映射值的新结果</returns>
    public static async Task<Result<TResult>> MapAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Task<TResult>> mapFunc)
    {
        if (result.IsSuccess)
        {
            TResult mappedValue = await mapFunc(result.Value).ConfigureAwait(false);
            return Result.Success(mappedValue);
        }

        return Result<TResult>.Failure(result.Errors);
    }

    /// <summary>
    /// 异步绑定成功结果的值到另一个结果
    /// </summary>
    /// <typeparam name="TValue">原始值类型</typeparam>
    /// <typeparam name="TResult">绑定后的值类型</typeparam>
    /// <param name="result">要绑定的结果</param>
    /// <param name="bindFunc">异步绑定函数，返回新的结果</param>
    /// <returns>绑定操作后的结果</returns>
    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Task<Result<TResult>>> bindFunc)
    {
        if (result.IsSuccess)
        {
            return await bindFunc(result.Value).ConfigureAwait(false);
        }

        return Result<TResult>.Failure(result.Errors);
    }

    /// <summary>
    /// 异步处理结果，根据结果状态执行不同的操作
    /// </summary>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="result">要处理的结果</param>
    /// <param name="onSuccess">成功时执行的异步操作</param>
    /// <param name="onFailure">失败时执行的异步操作</param>
    /// <returns>完成操作的任务</returns>
    public static async Task MatchAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, Task> onSuccess,
        Func<IEnumerable<Error>, Task> onFailure)
    {
        if (result.IsSuccess)
        {
            await onSuccess(result.Value).ConfigureAwait(false);
        }
        else
        {
            await onFailure(result.Errors).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 异步验证结果的值
    /// </summary>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="result">要验证的结果</param>
    /// <param name="predicate">验证谓词函数</param>
    /// <param name="error">验证失败时的错误</param>
    /// <returns>验证后的结果</returns>
    public static async Task<Result<TValue>> EnsureAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, Task<bool>> predicate,
        Error error)
    {
        if (!result.IsSuccess)
        {
            return result;
        }

        if (await predicate(result.Value).ConfigureAwait(false))
        {
            return result;
        }

        return Result<TValue>.Failure(error);
    }
}
