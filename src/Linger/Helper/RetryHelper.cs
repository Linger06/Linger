using System.Runtime.ExceptionServices;
using Linger.Exceptions;

namespace Linger.Helper;

/// <summary>
/// 重试操作的工具类
/// </summary>
public sealed class RetryHelper(RetryOptions? options = null)
{
    private readonly RetryOptions _options = options ?? new RetryOptions();
    private readonly Random _random = new();

    /// <summary>
    /// 执行可重试的异步操作
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="operation">要执行的操作</param>
    /// <param name="operationName">操作名称（用于异常信息）</param>
    /// <param name="shouldRetry">判断是否应该重试的函数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作结果</returns>
    /// <exception cref="OutOfReTryCountException">超过重试次数时抛出</exception>
    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNullOrEmpty(operationName);

        // 添加检查，如果令牌已取消，立即抛出异常
        cancellationToken.ThrowIfCancellationRequested();

        Exception? lastException = null;
        shouldRetry ??= _ => true;

        for (var retry = 0; retry < _options.MaxRetries; retry++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                // 如果是取消异常，直接重新抛出
                if (ex is OperationCanceledException)
                {
                    throw;
                }
                
                lastException = ex;
                if (shouldRetry(ex) && retry == _options.MaxRetries - 1)
                {
                    throw new OutOfReTryCountException("已达到最大重试次数", lastException);
                }
                else if (!shouldRetry(ex))
                {
                    // 保留原始异常的堆栈信息
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    throw; // 这行代码不会执行，但是需要它来满足编译器
                }

                // 每次重试前再次检查取消状态
                cancellationToken.ThrowIfCancellationRequested();

                // 计算延迟时间
                var delayMs = CalculateDelayWithJitter(retry);
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        throw new OutOfReTryCountException($"{operationName} 操作失败，已达到最大重试次数: {_options.MaxRetries}", lastException);
    }

    /// <summary>
    /// 执行可重试的异步操作（无返回值版本）
    /// </summary>
    /// <param name="operation">要执行的操作</param>
    /// <param name="operationName">操作名称（用于异常信息）</param>
    /// <param name="shouldRetry">判断是否应该重试的函数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示操作完成的任务</returns>
    /// <exception cref="OutOfReTryCountException">超过重试次数时抛出</exception>
    public async Task ExecuteAsync(
        Func<Task> operation,
        string operationName,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNullOrEmpty(operationName);

        // 添加检查，如果令牌已取消，立即抛出异常
        cancellationToken.ThrowIfCancellationRequested();

        Exception? lastException = null;
        shouldRetry ??= _ => true;

        for (var retry = 0; retry < _options.MaxRetries; retry++)
        {
            try
            {
                await operation();
                return; // 操作成功完成，直接返回
            }
            catch (Exception ex)
            {
                // 如果是取消异常，直接重新抛出
                if (ex is OperationCanceledException)
                {
                    throw;
                }
                
                lastException = ex;
                if (shouldRetry(ex) && retry == _options.MaxRetries - 1)
                {
                    throw new OutOfReTryCountException("已达到最大重试次数", lastException);
                }
                else if (!shouldRetry(ex))
                {
                    // 保留原始异常的堆栈信息
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    throw; // 这行代码不会执行，但是需要它来满足编译器
                }

                // 每次重试前再次检查取消状态
                cancellationToken.ThrowIfCancellationRequested();

                // 计算延迟时间
                var delayMs = CalculateDelayWithJitter(retry);
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        throw new OutOfReTryCountException($"{operationName} 操作失败，已达到最大重试次数: {_options.MaxRetries}", lastException);
    }

    private int CalculateDelayWithJitter(int retryAttempt)
    {
        // 计算基础延迟
        var delay = _options.UseExponentialBackoff
            ? _options.BaseDelayMs * Math.Pow(2, retryAttempt)
            : _options.BaseDelayMs;

        // 限制最大延迟时间
        delay = Math.Min(delay, _options.MaxDelayMs);

        // 添加随机抖动
        var jitterRange = delay * _options.JitterFactor;
        var jitter = _random.NextDouble() * jitterRange;

        return (int)(delay + jitter);
    }
}

public class RetryOptions
{
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// 基础延迟时间(毫秒)
    /// </summary>
    public int BaseDelayMs { get; set; } = 1000;

    /// <summary>
    /// 是否使用指数退避
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// 最大延迟时间(毫秒)
    /// </summary>
    public int MaxDelayMs { get; set; } = 30000;

    /// <summary>
    /// 抖动因子(0-1之间)
    /// </summary>
    public double JitterFactor { get; set; } = 0.2;
}
