using System.Runtime.ExceptionServices;
using Linger.FileSystem.Exceptions;

namespace Linger.FileSystem.Helpers;

/// <summary>
/// 重试操作的工具类
/// </summary>
public sealed class RetryHelper
{
    private readonly RetryOptions _options;
    private readonly Random _random;

    public RetryHelper(RetryOptions? options = null)
    {
        _options = options ?? new RetryOptions();
        _random = new Random();
    }

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

        //Exception? lastException = null;
        //for (int retry = 0; retry < _options.MaxRetries; retry++)
        //{
        //    try
        //    {
        //        return await operation();
        //    }
        //    catch (DuplicateFileException)
        //    {
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        lastException = ex;
        //        if (retry == _options.MaxRetries - 1)
        //        {
        //            throw new OutOfReTryCountException("已达到最大重试次数", lastException);
        //        }
        //        // 计算延迟时间
        //        var delayMs = CalculateDelayWithJitter(retry);
        //        // 等待后继续重试
        //        await Task.Delay(delayMs);
        //    }
        //}
        //throw new OutOfReTryCountException("已达到最大重试次数", lastException);


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
