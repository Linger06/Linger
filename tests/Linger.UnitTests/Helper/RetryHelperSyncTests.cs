using Linger.Exceptions;
using Linger.Helper;

namespace Linger.UnitTests.Helper;

public class RetryHelperSyncTests
{
    [Fact]
    public void Execute_Succeeds_On_First_Attempt()
    {
        var options = new RetryOptions
        {
            MaxRetryAttempts = 3,
            DelayMilliseconds = 1,
            MaxDelayMilliseconds = 1,
            UseExponentialBackoff = false,
            Jitter = 0
        };
        var retry = new RetryHelper(options);

        var counter = 0;
        retry.Execute(() => { counter++; });

        Assert.Equal(1, counter);
    }

    [Fact]
    public void Execute_Retries_And_Then_Succeeds()
    {
        var options = new RetryOptions
        {
            MaxRetryAttempts = 3,
            DelayMilliseconds = 1,
            MaxDelayMilliseconds = 1,
            UseExponentialBackoff = false,
            Jitter = 0
        };
        var retry = new RetryHelper(options);

        var attempts = 0;
        retry.Execute(() =>
        {
            attempts++;
            if (attempts < 2)
            {
                throw new InvalidOperationException("fail once");
            }
        });

        Assert.Equal(2, attempts);
    }

    [Fact]
    public void Execute_Throws_OutOfRetryCount_When_Always_Fails()
    {
        var options = new RetryOptions
        {
            MaxRetryAttempts = 2,
            DelayMilliseconds = 1,
            MaxDelayMilliseconds = 1,
            UseExponentialBackoff = false,
            Jitter = 0
        };
        var retry = new RetryHelper(options);

        var attempts = 0;
        Assert.Throws<OutOfRetryCountException>(() =>
        {
            retry.Execute(() =>
            {
                attempts++;
                throw new InvalidOperationException("always fail");
            });
        });

        // 应执行到最大次数
        Assert.Equal(2, attempts);
    }

    [Fact]
    public void Execute_Respects_ShouldRetry_Filter()
    {
        var options = new RetryOptions
        {
            MaxRetryAttempts = 5,
            DelayMilliseconds = 1,
            MaxDelayMilliseconds = 1,
            UseExponentialBackoff = false,
            Jitter = 0
        };
        var retry = new RetryHelper(options);

        var attempts = 0;
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            retry.Execute(() =>
            {
                attempts++;
                throw new InvalidOperationException("do-not-retry");
            }, shouldRetry: e => e is not InvalidOperationException);
        });

        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal(1, attempts);
    }
}
