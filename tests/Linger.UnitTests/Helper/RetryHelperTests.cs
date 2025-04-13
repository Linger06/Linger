using Linger.Exceptions;
using Linger.Helper;
using Xunit.v3;
using Xunit.Sdk;

namespace Linger.UnitTests.Helper;

public class RetryHelperTests
{
    [Fact]
    public async Task ExecuteAsync_WithSuccessfulOperation_ShouldReturnResult()
    {
        // Arrange
        var options = new RetryOptions { MaxRetries = 3 };
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
        var options = new RetryOptions { MaxRetries = 3 };
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
        var options = new RetryOptions { MaxRetries = 3, BaseDelayMs = 10 }; // 使用较短的延迟以加快测试
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
        var options = new RetryOptions { MaxRetries = 3 };
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
        var options = new RetryOptions { MaxRetries = 3, BaseDelayMs = 10 }; // 使用较短的延迟以加快测试
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
            MaxRetries = 3, 
            BaseDelayMs = 10,
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
        Assert.Equal(3, options.MaxRetries);
        Assert.Equal(1000, options.BaseDelayMs);
        Assert.True(options.UseExponentialBackoff);
        Assert.Equal(30000, options.MaxDelayMs);
        Assert.Equal(0.2, options.JitterFactor);
    }
}