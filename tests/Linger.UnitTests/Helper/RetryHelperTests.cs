using System.Diagnostics;
using Linger.Exceptions;
using Linger.Helper;
using Xunit.Sdk;
using Xunit.v3;

namespace Linger.UnitTests.Helper;

public class RetryHelperTests
{
    [Fact]
    public async Task ExecuteAsync_WithSuccessfulOperation_ShouldReturnResult()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 3 };
        var retryHelper = new RetryHelper(options);

        // Act
        var result = await retryHelper.ExecuteAsync(_ => Task.FromResult(42), "TestOperation");

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ExecuteAsync_WithRetryableResult_RetriesUntilResultSucceeds()
    {
        var options = new RetryOptions
        {
            MaxRetryAttempts = 3,
            DelayMilliseconds = 1,
            MaxDelayMilliseconds = 1,
            UseExponentialBackoff = false,
            Jitter = 0
        };
        var retryHelper = new RetryHelper(options);
        var attempts = 0;

        var result = await retryHelper.ExecuteAsync(
            _ => Task.FromResult(++attempts == 3),
            shouldRetryResult: success => !success,
            operationName: "ResultOperation");

        Assert.True(result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAllResultsAreRetryable_ReturnsLastResult()
    {
        var options = new RetryOptions
        {
            MaxRetryAttempts = 3,
            DelayMilliseconds = 1,
            MaxDelayMilliseconds = 1,
            UseExponentialBackoff = false,
            Jitter = 0
        };
        var retryHelper = new RetryHelper(options);
        var attempts = 0;

        var result = await retryHelper.ExecuteAsync(
            _ =>
            {
                attempts++;
                return Task.FromResult(false);
            },
            shouldRetryResult: success => !success,
            operationName: "ResultOperation");

        Assert.False(result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_WithResultPolicyAndPermanentException_DoesNotRetry()
    {
        var options = new RetryOptions
        {
            MaxRetryAttempts = 3,
            DelayMilliseconds = 1,
            MaxDelayMilliseconds = 1
        };
        var retryHelper = new RetryHelper(options);
        var attempts = 0;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            retryHelper.ExecuteAsync<bool>(
                _ =>
                {
                    attempts++;
                    throw new UnauthorizedAccessException("Access denied.");
                },
                shouldRetryResult: success => !success,
                operationName: "ResultOperation",
                shouldRetry: _ => false));

        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingOperationThatShouldNotRetry_ShouldThrowOriginalException()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 3 };
        var retryHelper = new RetryHelper(options);
        var exception = new InvalidOperationException("Test exception");

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await retryHelper.ExecuteAsync<int>(
                _ => throw exception,
                "TestOperation",
                _ => false);
        });

        Assert.Same(exception, actualException);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutShouldRetry_ShouldNotRetry()
    {
        var options = new RetryOptions
        {
            MaxRetryAttempts = 3,
            DelayMilliseconds = 1,
            MaxDelayMilliseconds = 1
        };
        var retryHelper = new RetryHelper(options);
        var exception = new InvalidOperationException("Test exception");
        var attemptCount = 0;

        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            retryHelper.ExecuteAsync<int>(_ =>
            {
                attemptCount++;
                throw exception;
            }));

        Assert.Same(exception, actualException);
        Assert.Equal(1, attemptCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingOperationThatExceedsRetries_ShouldThrowOutOfRetryCountException()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 3, DelayMilliseconds = 10 }; // 使用较短的延迟以加快测试
        var retryHelper = new RetryHelper(options);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OutOfRetryCountException>(async () =>
            {
                await retryHelper.ExecuteAsync<int>(
                    _ => throw new InvalidOperationException("Test exception"),
                    "TestOperation",
                    _ => true);
            });

        Assert.Contains("已达到最大重试次数", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Void_WithSuccessfulOperation_ShouldComplete()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 3 };
        var retryHelper = new RetryHelper(options);
        var operationExecuted = false;

        // Act
        await retryHelper.ExecuteAsync(async _ =>
        {
            await Task.Delay(1);
            operationExecuted = true;
        }, "TestOperation");

        // Assert
        Assert.True(operationExecuted);
    }

    [Fact]
    public async Task ExecuteAsync_Void_WithFailingOperationThatExceedsRetries_ShouldThrowOutOfRetryCountException()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 3, DelayMilliseconds = 10 }; // 使用较短的延迟以加快测试
        var retryHelper = new RetryHelper(options);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OutOfRetryCountException>(async () =>
            {
                await retryHelper.ExecuteAsync(
                    _ => throw new InvalidOperationException("Test exception"),
                    "TestOperation",
                    _ => true);
            });

        Assert.Contains("已达到最大重试次数", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithExponentialBackoff_ShouldRetryAndSucceed()
    {
        // Arrange
        var options = new RetryOptions
        {
            MaxRetryAttempts = 3,
            DelayMilliseconds = 10,
            UseExponentialBackoff = true
        };
        var retryHelper = new RetryHelper(options);
        var attemptCount = 0;

        // Act
        var result = await retryHelper.ExecuteAsync(async _ =>
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw new InvalidOperationException("Test exception");
            }
            await Task.Delay(1);
            return 42;
        }, "TestOperation", _ => true);

        // Assert
        Assert.Equal(42, result);
        Assert.Equal(3, attemptCount);
    }

    [Fact]
    public void RetryOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new RetryOptions();

        // Assert
        Assert.Equal(3, options.MaxRetryAttempts);
        Assert.Equal(1000, options.DelayMilliseconds);
        Assert.True(options.UseExponentialBackoff);
        Assert.Equal(30000, options.MaxDelayMilliseconds);
        Assert.Equal(0.2, options.Jitter);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 3, DelayMilliseconds = 10 };
        var retryHelper = new RetryHelper(options);
        var cts = new CancellationTokenSource();
        cts.Cancel(); // 立即取消

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await retryHelper.ExecuteAsync<int>(
                async _ =>
                {
                            await Task.Delay(1000, cts.Token);
                            return 42;
                        },
                "TestOperation",
                null,
                null,
                null,
                cts.Token);
            });
    }

    [Fact]
    public async Task ExecuteAsync_Void_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 3, DelayMilliseconds = 10 };
        var retryHelper = new RetryHelper(options);
        var cts = new CancellationTokenSource();
        cts.Cancel(); // 立即取消

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await retryHelper.ExecuteAsync(
                    async _ => { await Task.Delay(1000, cts.Token); },
                    "TestOperation",
                    null,
                    null,
                    cts.Token);
            });
    }

    [Fact]
    public async Task ExecuteAsync_WithTokenAwareOperation_CancelsRunningAttempt()
    {
        var options = new RetryOptions { MaxRetryAttempts = 3, DelayMilliseconds = 10 };
        var retryHelper = new RetryHelper(options);
        using var cts = new CancellationTokenSource();

        Task<int> operation = retryHelper.ExecuteAsync(
            async cancellationToken =>
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
                return 42;
            },
            cancellationToken: cts.Token);
        cts.CancelAfter(50);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
    }

    [Fact]
    public async Task ExecuteAsync_WithLastAttemptSucceeding_ShouldReturnResult()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 3, DelayMilliseconds = 10 };
        var retryHelper = new RetryHelper(options);
        var attemptCount = 0;

        // Act
        var result = await retryHelper.ExecuteAsync(async _ =>
        {
            attemptCount++;
            if (attemptCount < 3) // 前两次失败
            {
                throw new InvalidOperationException($"Test exception, attempt {attemptCount}");
            }
            await Task.Delay(1);
            return 42;
        }, "TestOperation", _ => true);

        // Assert
        Assert.Equal(42, result);
        Assert.Equal(3, attemptCount);
    }


    [Fact]
    public async Task ExecuteAsync_WithZeroRetries_ShouldNotRetry()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 1, DelayMilliseconds = 10 }; // 设置为1表示只尝试一次，没有重试
        var retryHelper = new RetryHelper(options);
        var attemptCount = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OutOfRetryCountException>(async () =>
            {
                await retryHelper.ExecuteAsync<int>(
                    _ =>
                    {
                        attemptCount++;
                        throw new InvalidOperationException("Test exception");
                    },
                    "TestOperation",
                    _ => true);
            });

        Assert.Equal(1, attemptCount);
        Assert.Contains("已达到最大重试次数", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullOperation_ShouldThrowArgumentNullException()
    {
        // Arrange
        var retryHelper = new RetryHelper();

        // Act & Assert
        await Assert.ThrowsAsync<System.ArgumentNullException>(async () =>
        {
            await retryHelper.ExecuteAsync<int>((Func<CancellationToken, Task<int>>)null!, "TestOperation");
        });
    }

    // Removed empty operation name test because operationName is now optional and auto-captured.
    public async Task ExecuteAsync_Void_WithNullOperation_ShouldThrowArgumentNullException()
    {
        // Arrange
        var retryHelper = new RetryHelper();

        // Act & Assert
        await Assert.ThrowsAsync<System.ArgumentNullException>(async () =>
        {
            await retryHelper.ExecuteAsync((Func<CancellationToken, Task>)null!, "TestOperation");
        });
    }

    [Fact]
    public async Task ExecuteAsync_WithNullOptions_ShouldUseDefaultOptions()
    {
        // Arrange
        var retryHelper = new RetryHelper(null);
        var attemptCount = 0;

        // Act
        var result = await retryHelper.ExecuteAsync(async _ =>
        {
            attemptCount++;
            if (attemptCount < 2)
            {
                throw new InvalidOperationException("Test exception");
            }
            await Task.Delay(1);
            return 42;
        }, "TestOperation", _ => true);

        // Assert
        Assert.Equal(42, result);
        Assert.Equal(2, attemptCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithJitter_ShouldApplyRandomDelay()
    {
        // Arrange
        var options = new RetryOptions
        {
            MaxRetryAttempts = 10,
            DelayMilliseconds = 10,
            UseExponentialBackoff = false,
            Jitter = 1.0 // 最大抖动
        };
        var retryHelper = new RetryHelper(options);
        var delayTimes = new List<long>();
        var attemptCount = 0;
        var sw = new Stopwatch();

        // Act
        await retryHelper.ExecuteAsync(async _ =>
            {
                if (attemptCount > 0)
                {
                    delayTimes.Add(sw.ElapsedMilliseconds);
                }

                attemptCount++;
                sw.Restart();

                if (attemptCount < 5) // 需要多次重试才能观察到随机性
                {
                    throw new InvalidOperationException("Test exception");
                }

                await Task.Delay(1);
            }, "TestOperation", _ => true);

        // Assert
        Assert.Equal(5, attemptCount);

        // 验证延迟时间有差异（表明抖动生效）
        // 至少有两个不同的延迟时间
        Assert.True(delayTimes.Distinct().Count() > 1);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutOperationName_CapturesCallerArgumentExpression()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 1, DelayMilliseconds = 1 };
        var retryHelper = new RetryHelper(options);

        // Act
        var ex = await Assert.ThrowsAsync<OutOfRetryCountException>(async () =>
            await retryHelper.ExecuteAsync<int>(
                _ => throw new InvalidOperationException("fail"),
                shouldRetry: _ => true)
        );

        // Assert: message should contain part of the lambda expression text
        Assert.Contains("InvalidOperationException", ex.Message);
    }

    [Fact]
    public void RetryOptions_Validate_ShouldThrow_WhenMaxRetryAttempts_NonPositive()
    {
        var options = new RetryOptions { MaxRetryAttempts = 0 };
        var helper = new RetryHelper(options);
        var ex = Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await helper.ExecuteAsync(_ => Task.FromResult(1))).Result;
        Assert.Equal("MaxRetryAttempts", ex.ParamName);
    }

    [Fact]
    public void RetryOptions_Validate_ShouldThrow_WhenDelayMilliseconds_NonPositive()
    {
        var options = new RetryOptions { DelayMilliseconds = 0 };
        var helper = new RetryHelper(options);
        var ex = Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await helper.ExecuteAsync(_ => Task.FromResult(1))).Result;
        Assert.Equal("DelayMilliseconds", ex.ParamName);
    }

    [Fact]
    public void RetryOptions_Validate_ShouldThrow_WhenMaxDelay_LessThan_Delay()
    {
        var options = new RetryOptions { DelayMilliseconds = 100, MaxDelayMilliseconds = 50 };
        var helper = new RetryHelper(options);
        var ex = Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await helper.ExecuteAsync(_ => Task.FromResult(1))).Result;
        Assert.Equal("MaxDelayMilliseconds", ex.ParamName);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void RetryOptions_Validate_ShouldThrow_WhenJitter_OutOfRange(double jitter)
    {
        var options = new RetryOptions { Jitter = jitter };
        var helper = new RetryHelper(options);
        var ex = Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await helper.ExecuteAsync(_ => Task.FromResult(1))).Result;
        Assert.Equal("Jitter", ex.ParamName);
    }

    [Fact]
    public void RetryOptions_Validate_ShouldPass_ForBoundaryValues()
    {
        var options = new RetryOptions
        {
            MaxRetryAttempts = 1,
            DelayMilliseconds = 1,
            MaxDelayMilliseconds = 1,
            Jitter = 0
        };
        var helper = new RetryHelper(options);
        helper.ExecuteAsync(_ => Task.FromResult(0)).GetAwaiter().GetResult();
        options.Jitter = 1;
        options.MaxDelayMilliseconds = 2; // > DelayMilliseconds
        helper = new RetryHelper(options);
        helper.ExecuteAsync(_ => Task.FromResult(0)).GetAwaiter().GetResult();
    }
}
