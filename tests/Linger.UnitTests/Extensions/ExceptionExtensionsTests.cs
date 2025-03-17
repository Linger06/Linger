using Linger.Extensions;

namespace Linger.UnitTests.Extensions;

public class ExceptionExtensionsTests
{
    [Fact]
    public void GetInnerException_ReturnsNull_WhenExceptionIsNull()
    {
        Exception? exception = null;
        Exception? result = exception.GetInnerException();
        Assert.Null(result);
    }

    [Fact]
    public void GetInnerException_ReturnsInnermostException()
    {
        var innerMostException = new Exception("Innermost");
        var middleException = new Exception("Middle", innerMostException);
        var outerException = new Exception("Outer", middleException);

        Exception? result = outerException.GetInnerException();
        Assert.Equal(innerMostException, result);
    }

    [Fact]
    public void ExceptionMessage_ReturnsEmptyString_WhenExceptionIsNull()
    {
        Exception? exception = null;
        var result = exception.ExceptionMessage();
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExceptionMessage_ReturnsInnermostExceptionMessage()
    {
        var innerMostException = new Exception("Innermost");
        var middleException = new Exception("Middle", innerMostException);
        var outerException = new Exception("Outer", middleException);

        var result = outerException.ExceptionMessage();
        Assert.Equal("Innermost", result);
    }

    [Fact]
    public void ExtractAllStackTrace_ReturnsStackTraceWithInnerExceptions()
    {
        var innerMostException = new Exception("Innermost");
        var middleException = new Exception("Middle", innerMostException);
        var outerException = new Exception("Outer", middleException);

        var result = outerException.ExtractAllStackTrace();
        Assert.Contains("Innermost", result);
        Assert.Contains("Middle", result);
        Assert.Contains("Outer", result);
    }

    [Fact]
    public void ExtractAllStackTrace_IncludesExceptionData()
    {
        var exception = new Exception("Exception with data");
        exception.Data["Key1"] = "Value1";
        exception.Data["Key2"] = "Value2";

        var result = exception.ExtractAllStackTrace();
        Assert.Contains("Key1: Value1", result);
        Assert.Contains("Key2: Value2", result);
    }
}