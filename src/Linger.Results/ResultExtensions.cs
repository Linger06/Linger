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

        return Result<TResult>.Failure(result.Errors);
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

        return Result<TResult>.Failure(result.Errors);
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
        var failedResults = results.Where(r => r.IsFailure).ToArray();

        if (failedResults.Length == 0)
        {
            return Result.Success();
        }

        var errors = failedResults.SelectMany(r => r.Errors).ToArray();

        return Result.Failure(errors);
    }

    /// <summary>
    /// 多个泛型结果合并为一个结果
    /// </summary>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="results">要合并的结果集合</param>
    /// <returns>合并后的不带值的结果</returns>
    public static Result Combine<TValue>(this IEnumerable<Result<TValue>> results)
    {
        var failedResults = results.Where(r => r.IsFailure).ToArray();

        if (failedResults.Length == 0)
        {
            return Result.Success();
        }

        var errors = failedResults.SelectMany(r => r.Errors).ToArray();

        return Result.Failure(errors);
    }

    /// <summary>
    /// 尝试执行操作，并将异常转换为失败结果
    /// </summary>
    /// <typeparam name="TValue">返回值类型</typeparam>
    /// <param name="func">要尝试执行的函数</param>
    /// <param name="errorHandler">异常处理函数，用于将异常转换为错误对象</param>
    /// <returns>包含函数返回值的结果，或包含异常信息的失败结果</returns>
    public static Result<TValue> Try<TValue>(
        Func<TValue> func,
        Func<Exception, Error> errorHandler)
    {
        try
        {
            return Result.Success(func());
        }
        catch (Exception ex)
        {
            return Result<TValue>.Failure(errorHandler(ex));
        }
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

        return Result<TResult>.Failure(result.Errors);
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

        return Result<TResult>.Failure(result.Errors);
    }

    /// <summary>
    /// 异步处理结果，根据结果状态执行不同的操作
    /// </summary>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="result">要处理的结果</param>
    /// <param name="onSuccess">成功时执行的异步操作</param>
    /// <param name="onFailure">失败时执行的异步操作</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>完成操作的任务</returns>
    public static async Task MatchAsync<TValue>(
        this Result<TValue> result,
        Func<TValue, CancellationToken, Task> onSuccess,
        Func<IEnumerable<Error>, CancellationToken, Task> onFailure,
        CancellationToken cancellationToken = default)
    {
        if (result.IsSuccess)
        {
            await onSuccess(result.Value, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await onFailure(result.Errors, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 异步验证结果的值
    /// </summary>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="result">要验证的结果</param>
    /// <param name="predicate">验证谓词函数</param>
    /// <param name="error">验证失败时的错误</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证后的结果</returns>
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
    /// 尝试异步执行操作，并将异常转换为失败结果
    /// </summary>
    /// <typeparam name="TValue">返回值类型</typeparam>
    /// <param name="func">要尝试执行的异步函数</param>
    /// <param name="errorHandler">异常处理函数，用于将异常转换为错误对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>包含函数返回值的结果，或包含异常信息的失败结果</returns>
    public static async Task<Result<TValue>> TryAsync<TValue>(
        Func<CancellationToken, Task<TValue>> func,
        Func<Exception, Error> errorHandler,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Result.Success(await func(cancellationToken).ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            return Result<TValue>.Failure(errorHandler(ex));
        }
    }

    #endregion
}
