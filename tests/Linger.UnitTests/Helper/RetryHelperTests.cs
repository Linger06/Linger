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
        var result = await retryHelper.ExecuteAsync(() => Task.FromResult(42), "TestOperation");

        // Assert
        Assert.Equal(42, result);
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
                () => throw exception,
                "TestOperation",
                ex => false);
        });

        Assert.Same(exception, actualException);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingOperationThatExceedsRetries_ShouldThrowOutOfRetryCountException()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 3, DelayMilliseconds = 10 }; // 使用较短的延迟以加快测试
        var retryHelper = new RetryHelper(options);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OutOfReTryCountException>(async () =>
        {
            await retryHelper.ExecuteAsync<int>(
                () => throw new InvalidOperationException("Test exception"),
                "TestOperation");
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
        await retryHelper.ExecuteAsync(async () =>
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
        var exception = await Assert.ThrowsAsync<OutOfReTryCountException>(async () =>
        {
            await retryHelper.ExecuteAsync(
                () => throw new InvalidOperationException("Test exception"),
                "TestOperation");
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
        var result = await retryHelper.ExecuteAsync(async () =>
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw new InvalidOperationException("Test exception");
            }
            await Task.Delay(1);
            return 42;
        }, "TestOperation");

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
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await retryHelper.ExecuteAsync<int>(
                async () =>
                {
                    await Task.Delay(1000, cts.Token);
                    return 42;
                },
                "TestOperation",
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
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await retryHelper.ExecuteAsync(
                async () => { await Task.Delay(1000, cts.Token); },
                "TestOperation",
                null,
                cts.Token);
        });
    }

    [Fact]
    public async Task ExecuteAsync_WithLastAttemptSucceeding_ShouldReturnResult()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 3, DelayMilliseconds = 10 };
        var retryHelper = new RetryHelper(options);
        var attemptCount = 0;

        // Act
        var result = await retryHelper.ExecuteAsync(async () =>
        {
            attemptCount++;
            if (attemptCount < 3) // 前两次失败
            {
                throw new InvalidOperationException($"Test exception, attempt {attemptCount}");
            }
            await Task.Delay(1);
            return 42;
        }, "TestOperation");

        // Assert
        Assert.Equal(42, result);
        Assert.Equal(3, attemptCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithFixedDelay_ShouldRetryWithConsistentDelay()
    {
        // Arrange
        var options = new RetryOptions
        {
            MaxRetryAttempts = 3,
            DelayMilliseconds = 50,
            UseExponentialBackoff = false,
            Jitter = 0.0 // 移除抖动以便于测试
        };
        var retryHelper = new RetryHelper(options);
        var attemptCount = 0;
        var sw = new Stopwatch();

        // Act
        sw.Start();
        var result = await retryHelper.ExecuteAsync(async () =>
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw new InvalidOperationException("Test exception");
            }
            await Task.Delay(1);
            return 42;
        }, "TestOperation");
        sw.Stop();

        // Assert
        Assert.Equal(42, result);
        Assert.Equal(3, attemptCount);

        // 验证延迟时间在预期范围内
        // 两次重试，每次50ms，加上一些执行时间，应该在100-200ms范围内
        Assert.True(sw.ElapsedMilliseconds >= 100 && sw.ElapsedMilliseconds <= 300);
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroRetries_ShouldNotRetry()
    {
        // Arrange
        var options = new RetryOptions { MaxRetryAttempts = 1, DelayMilliseconds = 10 }; // 设置为1表示只尝试一次，没有重试
        var retryHelper = new RetryHelper(options);
        var attemptCount = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OutOfReTryCountException>(async () =>
        {
            await retryHelper.ExecuteAsync<int>(
                () =>
                {
                    attemptCount++;
                    throw new InvalidOperationException("Test exception");
                },
                "TestOperation");
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
            await retryHelper.ExecuteAsync<int>(null!, "TestOperation");
        });
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyOperationName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var retryHelper = new RetryHelper();

        // Act & Assert
        await Assert.ThrowsAsync<System.ArgumentException>(async () =>
        {
            await retryHelper.ExecuteAsync<int>(() => Task.FromResult(42), "");
        });
    }

    [Fact]
    public async Task ExecuteAsync_Void_WithNullOperation_ShouldThrowArgumentNullException()
    {
        // Arrange
        var retryHelper = new RetryHelper();

        // Act & Assert
        await Assert.ThrowsAsync<System.ArgumentNullException>(async () =>
        {
            await retryHelper.ExecuteAsync(null!, "TestOperation");
        });
    }

    [Fact]
    public async Task ExecuteAsync_WithNullOptions_ShouldUseDefaultOptions()
    {
        // Arrange
        var retryHelper = new RetryHelper(null);
        var attemptCount = 0;

        // Act
        var result = await retryHelper.ExecuteAsync(async () =>
        {
            attemptCount++;
            if (attemptCount < 2)
            {
                throw new InvalidOperationException("Test exception");
            }
            await Task.Delay(1);
            return 42;
        }, "TestOperation");

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
        await retryHelper.ExecuteAsync(async () =>
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
        }, "TestOperation");

        // Assert
        Assert.Equal(5, attemptCount);

        // 验证延迟时间有差异（表明抖动生效）
        // 至少有两个不同的延迟时间
        Assert.True(delayTimes.Distinct().Count() > 1);
    }
}