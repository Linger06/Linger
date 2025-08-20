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
        string? operationName = null,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default,
    [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(operation))] string? operationExpr = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        operationName ??= operationExpr ?? nameof(operation);

        // Fast-path: if already cancelled, avoid further validation/allocations.
        if (cancellationToken.IsCancellationRequested)
        {
            return await Task.FromCanceled<T>(cancellationToken).ConfigureAwait(false);
        }

        return await ExecuteWithRetryAsync(
            async () => await operation().ConfigureAwait(false),
            operationName,
            shouldRetry,
            cancellationToken).ConfigureAwait(false);
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
        string? operationName = null,
        Func<Exception, bool>? shouldRetry = null,
        CancellationToken cancellationToken = default,
    [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(operation))] string? operationExpr = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        operationName ??= operationExpr ?? nameof(operation);

        if (cancellationToken.IsCancellationRequested)
        {
            await Task.FromCanceled(cancellationToken).ConfigureAwait(false);
            return;
        }

        await ExecuteWithRetryAsync(
            async () =>
            {
                await operation().ConfigureAwait(false);
                return true; // 返回值不重要，仅作为泛型方法的结果
            },
            operationName,
            shouldRetry,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 执行可重试的同步操作（无返回值版本）
    /// </summary>
    /// <param name="operation">要执行的操作</param>
    /// <param name="operationName">操作名称（用于异常信息）</param>
    /// <param name="shouldRetry">判断是否应该重试的函数</param>
    /// <exception cref="OutOfReTryCountException">超过重试次数时抛出</exception>
    public void Execute(
        Action operation,
        string? operationName = null,
        Func<Exception, bool>? shouldRetry = null,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(operation))] string? operationExpr = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        operationName ??= operationExpr ?? nameof(operation);

        // 使用同步包装调用通用重试逻辑，避免阻塞异步上下文。
        // 这里不接受 CancellationToken，保持 API 简洁；如需取消请使用 ExecuteAsync。
        _options.Validate();
        Exception? lastException = null;
        shouldRetry ??= _ => true;

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
                if (ex is OperationCanceledException)
                {
                    throw;
                }

                lastException = ex;
                if (!shouldRetry(ex))
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
        CancellationToken cancellationToken)
    {
        // 提前验证配置，确保无效参数尽早抛出（含 MaxRetryAttempts 等）
        _options.Validate();
        // 添加检查，如果令牌已取消，立即抛出异常
        cancellationToken.ThrowIfCancellationRequested();

        Exception? lastException = null;
        shouldRetry ??= _ => true;

        var start = DateTime.UtcNow;
        for (var retry = 0; retry < _options.MaxRetryAttempts; retry++)
        {
            // Per-attempt early cancellation (in addition to delay points) for responsiveness.
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                return await operation().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // 如果是取消异常，直接重新抛出
                if (ex is OperationCanceledException)
                {
                    throw;
                }

                lastException = ex;

                // 如果异常类型不需要重试，直接将原始异常重新抛出
                if (!shouldRetry(ex))
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

                // 每次重试前再次检查取消状态
                cancellationToken.ThrowIfCancellationRequested();

                // 计算延迟时间
                var delayMs = CalculateDelayWithJitter(retry);
                await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
            }
        }

        var elapsed = DateTime.UtcNow - start;
        // 如果所有重试都失败，抛出统一的异常
        throw new OutOfRetryCountException($"{operationName} 操作失败，已达到最大重试次数: {_options.MaxRetryAttempts}, 耗时: {elapsed.TotalMilliseconds:N0} ms", lastException);
    }

    private int CalculateDelayWithJitter(int retryAttempt)
    {
        // 计算基础延迟（指数退避 2^n，使用Math.Min防止溢出）
        long baseDelay = _options.DelayMilliseconds;
        long delay = _options.UseExponentialBackoff
            ? Math.Min(baseDelay * (1L << Math.Min(retryAttempt, 30)), _options.MaxDelayMilliseconds)
            : baseDelay;

        if (delay > _options.MaxDelayMilliseconds)
        {
            delay = _options.MaxDelayMilliseconds;
        }

        // Full jitter: random between 0 and delay * (1 + Jitter)
        if (_options.Jitter <= 0)
        {
            // 无抖动，返回确定性延迟，便于测试与预测
            return (int)Math.Min(int.MaxValue, delay);
        }

        var maxWithJitter = delay + (long)(delay * _options.Jitter);
        var rnd = ThreadSafeRandom.NextLong(0, Math.Max(1, maxWithJitter));
        return (int)Math.Min(int.MaxValue, rnd);
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
    /// 抖动因子(0-1之间)
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
    public static long NextLong(long minInclusive, long maxExclusive)
    {
        if (maxExclusive <= minInclusive) return minInclusive;
#if NET6_0_OR_GREATER
        return Random.Shared.NextInt64(minInclusive, maxExclusive);
#else
        // Thread-local fallback instance
        return LocalRandom.NextLong(minInclusive, maxExclusive);
#endif
    }

#if !NET6_0_OR_GREATER
    private static class LocalRandom
    {
        [ThreadStatic]
        private static Random? _local;

        public static Random Instance => _local ??= new Random(unchecked(Environment.TickCount * 31 + Environment.CurrentManagedThreadId));

        public static long NextLong(long minInclusive, long maxExclusive)
        {
            var range = (double)maxExclusive - minInclusive;
            var sample = Instance.NextDouble();
            return (long)(minInclusive + sample * range);
        }
    }
#endif
}
