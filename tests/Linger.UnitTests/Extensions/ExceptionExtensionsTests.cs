using Linger.Extensions;
using Xunit.v3;

namespace Linger.UnitTests.Extensions;

public class ExceptionExtensionsTests
{
    [Fact]
    public void GetInnerException_WithNoInnerException_ReturnsSameException()
    {
        // Arrange
        var exception = new InvalidOperationException("测试异常");

        // Act
        var innerException = exception.GetInnerException();

        // Assert
        Assert.Same(exception, innerException);
    }

    [Fact]
    public void GetInnerException_WithSingleInnerException_ReturnsInnerException()
    {
        // Arrange
        var innerException = new ArgumentException("内部异常");
        var outerException = new InvalidOperationException("外部异常", innerException);

        // Act
        var result = outerException.GetInnerException();

        // Assert
        Assert.Same(innerException, result);
    }

    [Fact]
    public void GetInnerException_WithMultipleNestedExceptions_ReturnsInnermostException()
    {
        // Arrange
        var innermostException = new ArgumentNullException("参数名", "最内层异常");
        var middleException = new ArgumentException("中间异常", innermostException);
        var outerException = new InvalidOperationException("外部异常", middleException);

        // Act
        var result = outerException.GetInnerException();

        // Assert
        Assert.Same(innermostException, result);
    }

    [Fact]
    public void GetInnerException_WithNullException_ReturnsNull()
    {
        // Arrange
        Exception? exception = null;

        // Act
        var result = exception.GetInnerException();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExceptionMessage_WithNoInnerException_ReturnsExceptionMessage()
    {
        // Arrange
        var message = "测试异常消息";
        var exception = new InvalidOperationException(message);

        // Act
        var result = exception.ExceptionMessage();

        // Assert
        Assert.Equal(message, result);
    }

    [Fact]
    public void ExceptionMessage_WithSingleInnerException_ReturnsInnerExceptionMessage()
    {
        // Arrange
        var innerMessage = "内部异常消息";
        var outerMessage = "外部异常消息";
        var innerException = new ArgumentException(innerMessage);
        var outerException = new InvalidOperationException(outerMessage, innerException);

        // Act
        var result = outerException.ExceptionMessage();

        // Assert
        Assert.Equal(innerMessage, result);
    }

    [Fact]
    public void ExceptionMessage_WithMultipleNestedExceptions_ReturnsInnermostExceptionMessage()
    {
        // Arrange
        var innermostMessage = "最内层异常消息";
        var middleMessage = "中间异常消息";
        var outerMessage = "外部异常消息";

        var innermostException = new Exception(innermostMessage); // Use Exception instead of ArgumentNullException
        var middleException = new ArgumentException(middleMessage, innermostException);
        var outerException = new InvalidOperationException(outerMessage, middleException);

        // Act
        var result = outerException.ExceptionMessage();

        // Assert
        Assert.Equal(innermostMessage, result);
    }

    [Fact]
    public void ExceptionMessage_WithNullException_ReturnsEmptyString()
    {
        // Arrange
        Exception? exception = null;

        // Act
        var result = exception.ExceptionMessage();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractAllStackTrace_WithSingleException_ReturnsFormattedStackTrace()
    {
        // Arrange
        Exception exception;
        try
        {
            // 强制创建一个带有堆栈跟踪的异常
            throw new InvalidOperationException("单一异常");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Act
        var result = exception.ExtractAllStackTrace();

        // Assert
        Assert.Contains("#1: 单一异常", result);
        Assert.Contains("Linger.UnitTests.Extensions.ExceptionExtensionsTests", result);
    }

    [Fact]
    public void ExtractAllStackTrace_WithNestedExceptions_ReturnsAllStackTraces()
    {
        // Arrange
        Exception exception;
        try
        {
            try
            {
                try
                {
                    // 最内层异常
                    throw new ArgumentNullException("参数", "最内层异常");
                }
                catch (Exception inner)
                {
                    // 中间异常
                    throw new ArgumentException("中间异常", inner);
                }
            }
            catch (Exception middle)
            {
                // 外层异常
                throw new InvalidOperationException("外层异常", middle);
            }
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Act
        var result = exception.ExtractAllStackTrace();

        // Assert
        Assert.Contains("#1: 外层异常", result);
        Assert.Contains("#2: 中间异常", result);
        Assert.Contains("#3: 最内层异常", result);
    }

    [Fact]
    public void ExtractAllStackTrace_WithExceptionData_IncludesDataInOutput()
    {
        // Arrange
        var exception = new InvalidOperationException("带有数据的异常");
        exception.Data["Key1"] = "Value1";
        exception.Data["Key2"] = 42;

        // Act
        var result = exception.ExtractAllStackTrace();

        // Assert
        Assert.Contains("Data:", result);
        Assert.Contains("Key1: Value1", result);
        Assert.Contains("Key2: 42", result);
    }
}