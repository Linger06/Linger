using System.Runtime.ExceptionServices;
using Linger.Exceptions;

namespace Linger.Helper;

/// <summary>
/// 重试操作的工具类
/// </summary>
public sealed class RetryHelper(RetryOptions? options = null)
{
    private readonly RetryOptions _options = options ?? new RetryOptions();

    /// <summary>
    /// Executes an asynchronous operation with retry and forwards cancellation to each attempt.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">The operation name used in errors.</param>
    /// <param name="shouldRetry">Determines whether an exception is retryable. When omitted, exceptions are not retried.</param>
    /// <param name="shouldRetryResult">Determines whether a returned result should trigger another attempt.</param>
    /// <param name="operationExpr">The caller expression for <paramref name="operation"/>.</param>
    /// <param name="cancellationToken">The cancellation token forwarded to the operation.</param>
    /// <returns>The first accepted result, or the last result when all result-based attempts are exhausted.</returns>
    /// <exception cref="OutOfRetryCountException">Thrown after all attempts fail.</exception>
    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string? operationName = null,
        Func<Exception, bool>? shouldRetry = null,
        Func<T, bool>? shouldRetryResult = null,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(operation))] string? operationExpr = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        operationName ??= operationExpr ?? nameof(operation);
        cancellationToken.ThrowIfCancellationRequested();

        return await ExecuteWithRetryAsync(
            () => operation(cancellationToken),
            operationName,
            shouldRetry,
            shouldRetryResult,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an asynchronous operation with retry and forwards cancellation to each attempt.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">The operation name used in errors.</param>
    /// <param name="shouldRetry">Determines whether an exception is retryable. When omitted, exceptions are not retried.</param>
    /// <param name="operationExpr">The caller expression for <paramref name="operation"/>.</param>
    /// <param name="cancellationToken">The cancellation token forwarded to the operation.</param>
    /// <returns>A task representing the operation.</returns>
    /// <exception cref="OutOfRetryCountException">Thrown after all attempts fail.</exception>
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        string? operationName = null,
        Func<Exception, bool>? shouldRetry = null,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(operation))] string? operationExpr = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        operationName ??= operationExpr ?? nameof(operation);
        cancellationToken.ThrowIfCancellationRequested();

        await ExecuteWithRetryAsync(
            async () =>
            {
                await operation(cancellationToken).ConfigureAwait(false);
                return true;
            },
            operationName,
            shouldRetry,
            shouldRetryResult: null,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 执行可重试的同步操作（无返回值版本）
    /// </summary>
    /// <param name="operation">要执行的操作</param>
    /// <param name="operationName">操作名称（用于异常信息）</param>
    /// <param name="shouldRetry">判断是否应该重试的函数；未提供时不重试。</param>
    /// <param name="operationExpr"></param>
    /// <exception cref="OutOfRetryCountException">超过重试次数时抛出</exception>
    public void Execute(
        Action operation,
        string? operationName = null,
        Func<Exception, bool>? shouldRetry = null,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(operation))] string? operationExpr = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        operationName ??= operationExpr ?? nameof(operation);

        // 这里不接受 CancellationToken，保持 API 简洁；如需取消请使用 ExecuteAsync。
        // 没有调用方令牌，因此 OperationCanceledException 与其他异常一样交给 shouldRetry 判断。
        _options.Validate();
        Exception? lastException = null;

        var start = DateTime.UtcNow;
        for (var retry = 0; retry < _options.MaxRetryAttempts; retry++)
        {
            try
            {
                operation();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (shouldRetry?.Invoke(ex) != true)
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                }

                if (retry == _options.MaxRetryAttempts - 1)
                {
                    continue;
                }

                var delayMs = CalculateDelayWithJitter(retry);
#if NET6_0_OR_GREATER
                System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(delayMs));
#else
                System.Threading.Thread.Sleep(delayMs);
#endif
            }
        }

        var elapsed = DateTime.UtcNow - start;
        throw new OutOfRetryCountException($"{operationName} 操作失败，已达到最大重试次数: {_options.MaxRetryAttempts}, 耗时: {elapsed.TotalMilliseconds:N0} ms", lastException);
    }

    /// <summary>
    /// 内部通用重试逻辑实现
    /// </summary>
    private async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        Func<Exception, bool>? shouldRetry,
        Func<T, bool>? shouldRetryResult,
        CancellationToken cancellationToken)
    {
        // 提前验证配置，确保无效参数尽早抛出（含 MaxRetryAttempts 等）
        _options.Validate();
        // 添加检查，如果令牌已取消，立即抛出异常
        cancellationToken.ThrowIfCancellationRequested();

        Exception? lastException = null;

        var start = DateTime.UtcNow;
        for (var retry = 0; retry < _options.MaxRetryAttempts; retry++)
        {
            // Per-attempt early cancellation (in addition to delay points) for responsiveness.
            cancellationToken.ThrowIfCancellationRequested();
            T result;
            try
            {
                result = await operation().ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // 只有调用方主动取消才直接传播；HttpClient 超时等内部取消交给 shouldRetry 判断
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;

                // 如果异常类型不需要重试，直接将原始异常重新抛出
                if (shouldRetry?.Invoke(ex) != true)
                {
                    // 保留原始异常的堆栈信息
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    throw; // 这行代码不会执行，但是需要它来满足编译器
                }

                // 如果已经到达最后一次重试，不再等待，直接进入下一次循环
                if (retry == _options.MaxRetryAttempts - 1)
                {
                    continue;
                }

                await DelayBeforeRetryAsync(retry, cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (shouldRetryResult?.Invoke(result) != true)
            {
                return result;
            }

            if (retry == _options.MaxRetryAttempts - 1)
            {
                return result;
            }

            await DelayBeforeRetryAsync(retry, cancellationToken).ConfigureAwait(false);
        }

        var elapsed = DateTime.UtcNow - start;
        // 如果所有重试都失败，抛出统一的异常
        throw new OutOfRetryCountException($"{operationName} 操作失败，已达到最大重试次数: {_options.MaxRetryAttempts}, 耗时: {elapsed.TotalMilliseconds:N0} ms", lastException);
    }

    private Task DelayBeforeRetryAsync(int retryAttempt, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.Delay(CalculateDelayWithJitter(retryAttempt), cancellationToken);
    }

    private int CalculateDelayWithJitter(int retryAttempt)
    {
        return CalculateDelay(_options, retryAttempt, ThreadSafeRandom.NextDouble());
    }

    internal static int CalculateDelay(RetryOptions options, int retryAttempt, double sample)
    {
        // 计算基础延迟（指数退避 2^n，使用Math.Min防止溢出）
        long baseDelay = options.DelayMilliseconds;
        long delay = options.UseExponentialBackoff
            ? Math.Min(baseDelay * (1L << Math.Min(retryAttempt, 30)), options.MaxDelayMilliseconds)
            : baseDelay;

        if (delay > options.MaxDelayMilliseconds)
        {
            delay = options.MaxDelayMilliseconds;
        }

        if (options.Jitter <= 0)
        {
            // 无抖动，返回确定性延迟，便于测试与预测
            return (int)delay;
        }

        // 在 [delay×(1−Jitter), delay×(1+Jitter)] 内均匀取值后再截断到上限，
        // 避免出现 0 延迟的紧凑重试，也保证不突破 MaxDelayMilliseconds
        var jittered = delay * (1 - options.Jitter + 2 * options.Jitter * sample);

        return (int)Math.Min(jittered, options.MaxDelayMilliseconds);
    }
}

public class RetryOptions
{
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// 基础延迟时间(毫秒)
    /// </summary>
    public int DelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// 是否使用指数退避
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// 最大延迟时间(毫秒)
    /// </summary>
    public int MaxDelayMilliseconds { get; set; } = 30000;

    /// <summary>
    /// 抖动因子(0-1之间)。实际延迟在 [基础延迟×(1−Jitter), 基础延迟×(1+Jitter)] 内随机取值，且不超过 <see cref="MaxDelayMilliseconds"/>。
    /// </summary>
    public double Jitter { get; set; } = 0.2;

    internal void Validate()
    {
        if (MaxRetryAttempts <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaxRetryAttempts));
        if (DelayMilliseconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(DelayMilliseconds));
        if (MaxDelayMilliseconds < DelayMilliseconds)
            throw new ArgumentOutOfRangeException(nameof(MaxDelayMilliseconds), "MaxDelayMilliseconds must be >= DelayMilliseconds");
        if (Jitter < 0 || Jitter > 1)
            throw new ArgumentOutOfRangeException(nameof(Jitter));
    }
}

internal static class ThreadSafeRandom
{
    /// <summary>
    /// 返回 [0, 1) 区间内的随机数。
    /// </summary>
    public static double NextDouble()
    {
#if NET6_0_OR_GREATER
        return Random.Shared.NextDouble();
#else
        return LocalRandom.Instance.NextDouble();
#endif
    }

#if !NET6_0_OR_GREATER
    private static class LocalRandom
    {
        [ThreadStatic]
        private static Random? _local;

        public static Random Instance => _local ??= new Random(unchecked(Environment.TickCount * 31 + Environment.CurrentManagedThreadId));
    }
#endif
}
