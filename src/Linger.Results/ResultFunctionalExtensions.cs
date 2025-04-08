namespace Linger.Results;

/// <summary>
/// 为 Result 类型提供函数式编程风格的扩展方法
/// </summary>
public static class ResultFunctionalExtensions
{
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
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="results">要合并的结果集合</param>
    /// <returns>合并后的单一结果</returns>
    public static Result Combine(this IEnumerable<Result> results)
    {
        var failedResults = results.Where(r => r.IsFailure).ToArray();

        if (failedResults.Length == 0)
            return Result.Success();

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
            return Result.Success();

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

    /// <summary>
    /// 尝试异步执行操作，并将异常转换为失败结果
    /// </summary>
    /// <typeparam name="TValue">返回值类型</typeparam>
    /// <param name="func">要尝试执行的异步函数</param>
    /// <param name="errorHandler">异常处理函数，用于将异常转换为错误对象</param>
    /// <returns>包含函数返回值的结果，或包含异常信息的失败结果</returns>
    public static async Task<Result<TValue>> TryAsync<TValue>(
        Func<Task<TValue>> func,
        Func<Exception, Error> errorHandler)
    {
        try
        {
            return Result.Success(await func());
        }
        catch (Exception ex)
        {
            return Result<TValue>.Failure(errorHandler(ex));
        }
    }
}