namespace Linger.Results;

/// <summary>
/// 为 Result 类型提供扩展方法
/// </summary>
public static class ResultExtensions
{
    #region 同步方法

    /// <summary>
    /// 映射成功结果中的值到新的类型
    /// </summary>
    /// <typeparam name="TValue">原始值类型</typeparam>
    /// <typeparam name="TResult">映射后的值类型</typeparam>
    /// <param name="result">要映射的结果</param>
    /// <param name="mapFunc">映射函数</param>
    /// <returns>包含映射值的新结果</returns>
    public static Result<TResult> Map<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, TResult> mapFunc)
    {
        if (result.IsSuccess)
        {
            return Result.Success(mapFunc(result.Value));
        }

        return new Result<TResult>(default, result.State);
    }

    /// <summary>
    /// 将当前结果绑定到另一个结果上
    /// </summary>
    /// <typeparam name="TValue">原始值类型</typeparam>
    /// <typeparam name="TResult">绑定后的值类型</typeparam>
    /// <param name="result">要绑定的结果</param>
    /// <param name="bindFunc">绑定函数，返回新的结果</param>
    /// <returns>绑定后的结果</returns>
    public static Result<TResult> Bind<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, Result<TResult>> bindFunc)
    {
        if (result.IsSuccess)
        {
            return bindFunc(result.Value);
        }

        return new Result<TResult>(default, result.State);
    }

    /// <summary>
    /// 确保结果值满足特定条件
    /// </summary>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="result">要验证的结果</param>
    /// <param name="predicate">验证条件</param>
    /// <param name="error">验证失败时的错误</param>
    /// <returns>原始结果或失败结果</returns>
    public static Result<TValue> Ensure<TValue>(
        this Result<TValue> result,
        Func<TValue, bool> predicate,
        Error error)
    {
        if (!result.IsSuccess)
        {
            return result;
        }

        if (predicate(result.Value))
        {
            return result;
        }

        return Result<TValue>.Failure(error);
    }

    /// <summary>
    /// 多个结果合并为一个结果
    /// </summary>
    /// <param name="results">要合并的结果集合</param>
    /// <returns>合并后的单一结果</returns>
    public static Result Combine(this IEnumerable<Result> results)
    {
        return CombineStates(results.Select(result => result.State));
    }

    /// <summary>
    /// 多个泛型结果合并为一个结果
    /// </summary>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="results">要合并的结果集合</param>
    /// <returns>合并后的不带值的结果</returns>
    public static Result Combine<TValue>(this IEnumerable<Result<TValue>> results)
    {
        return CombineStates(results.Select(result => result.State));
    }

    private static Result CombineStates(IEnumerable<ResultState> states)
    {
        var hasFailure = false;
        var errors = new List<Error>();

        foreach (var state in states)
        {
            if (state.Status is ResultStatus.Ok)
            {
                continue;
            }

            hasFailure = true;
            errors.AddRange(state.Errors);
        }

        return hasFailure ? Result.Failure(errors) : Result.Success();
    }

    #endregion

    #region 异步方法

    /// <summary>
    /// 异步映射成功结果的值
    /// </summary>
    /// <typeparam name="TValue">原始值类型</typeparam>
    /// <typeparam name="TResult">映射后的值类型</typeparam>
    /// <param name="result">要映射的结果</param>
    /// <param name="mapFunc">异步映射函数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>包含映射值的新结果</returns>
    public static async Task<Result<TResult>> MapAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, CancellationToken, Task<TResult>> mapFunc,
        CancellationToken cancellationToken = default)
    {
        if (result.IsSuccess)
        {
            TResult mappedValue = await mapFunc(result.Value, cancellationToken).ConfigureAwait(false);

            return Result.Success(mappedValue);
        }

        return new Result<TResult>(default, result.State);
    }

    /// <summary>
    /// 等待结果任务，并异步映射成功结果中的值
    /// </summary>
    /// <typeparam name="TValue">原始值类型</typeparam>
    /// <typeparam name="TResult">映射后的值类型</typeparam>
    /// <param name="resultTask">要等待并映射的结果任务</param>
    /// <param name="mapFunc">异步映射函数</param>
    /// <param name="cancellationToken">传递给映射函数的取消令牌</param>
    /// <returns>包含映射值的新结果任务</returns>
    /// <example>
    /// <code>
    /// var result = await GetUserAsync(cancellationToken)
    ///     .MapAsync((user, token) => LoadProfileAsync(user, token), cancellationToken);
    /// </code>
    /// </example>
    public static async Task<Result<TResult>> MapAsync<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, CancellationToken, Task<TResult>> mapFunc,
        CancellationToken cancellationToken = default)
    {
        var result = await resultTask.ConfigureAwait(false);

        return await result.MapAsync(mapFunc, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步绑定成功结果的值到另一个结果
    /// </summary>
    /// <typeparam name="TValue">原始值类型</typeparam>
    /// <typeparam name="TResult">绑定后的值类型</typeparam>
    /// <param name="result">要绑定的结果</param>
    /// <param name="bindFunc">异步绑定函数，返回新的结果</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>绑定操作后的结果</returns>
    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(
        this Result<TValue> result,
        Func<TValue, CancellationToken, Task<Result<TResult>>> bindFunc,
        CancellationToken cancellationToken = default)
    {
        if (result.IsSuccess)
        {
            return await bindFunc(result.Value, cancellationToken).ConfigureAwait(false);
        }

        return new Result<TResult>(default, result.State);
    }

    /// <summary>
    /// 根据结果状态异步执行成功或失败分支。
    /// </summary>
    /// <typeparam name="TValue">结果值类型。</typeparam>
    /// <param name="result">要处理的结果。</param>
    /// <param name="onSuccess">成功时执行的异步操作。</param>
    /// <param name="onFailure">失败时执行的异步操作。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>表示分支操作完成的任务。</returns>
    public static async Task MatchAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, CancellationToken, Task> onSuccess,
        Func<IEnumerable<Error>, CancellationToken, Task> onFailure,
        CancellationToken cancellationToken = default)
    {
        if (result.IsSuccess)
        {
            await onSuccess(result.Value, cancellationToken).ConfigureAwait(false);

            return;
        }

        await onFailure(result.Errors, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 异步验证成功结果中的值。
    /// </summary>
    /// <typeparam name="TValue">结果值类型。</typeparam>
    /// <param name="result">要验证的结果。</param>
    /// <param name="predicate">异步验证条件。</param>
    /// <param name="error">验证失败时返回的错误。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>原始结果或验证失败结果。</returns>
    public static async Task<Result<TValue>> EnsureAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, CancellationToken, Task<bool>> predicate,
        Error error,
        CancellationToken cancellationToken = default)
    {
        if (!result.IsSuccess)
        {
            return result;
        }

        if (await predicate(result.Value, cancellationToken).ConfigureAwait(false))
        {
            return result;
        }

        return Result<TValue>.Failure(error);
    }

    /// <summary>
    /// 等待结果任务，并将成功结果异步绑定到另一个结果
    /// </summary>
    /// <typeparam name="TValue">原始值类型</typeparam>
    /// <typeparam name="TResult">绑定后的值类型</typeparam>
    /// <param name="resultTask">要等待并绑定的结果任务</param>
    /// <param name="bindFunc">异步绑定函数，返回新的结果</param>
    /// <param name="cancellationToken">传递给绑定函数的取消令牌</param>
    /// <returns>绑定后的结果任务</returns>
    /// <example>
    /// <code>
    /// var result = await GetUserAsync(cancellationToken)
    ///     .BindAsync((user, token) => SaveUserAsync(user, token), cancellationToken);
    /// </code>
    /// </example>
    public static async Task<Result<TResult>> BindAsync<TValue, TResult>(
        this Task<Result<TValue>> resultTask,
        Func<TValue, CancellationToken, Task<Result<TResult>>> bindFunc,
        CancellationToken cancellationToken = default)
    {
        var result = await resultTask.ConfigureAwait(false);

        return await result.BindAsync(bindFunc, cancellationToken).ConfigureAwait(false);
    }

    #endregion
}
