using Linger.Exceptions;

namespace Linger.UnitTests.Exceptions;

public class OutOfRetryCountExceptionTests
{
    [Fact]
    public void Ctor_Parameterless_HasDefaultMessage()
    {
        var ex = new OutOfRetryCountException();
        Assert.Contains("Retry attempts exhausted", ex.Message);
    }

    [Fact]
    public void Ctor_MessageOnly_SetsMessage()
    {
        var ex = new OutOfRetryCountException("custom");
        Assert.Equal("custom", ex.Message);
    }

    [Fact]
    public void Ctor_MessageAndInner_SetsInner()
    {
        var inner = new InvalidOperationException();
        var ex = new OutOfRetryCountException("msg", inner);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void LegacyType_StillConstructible()
    {
        var ex = new OutOfReTryCountException("legacy");
        Assert.IsType<OutOfReTryCountException>(ex);
        Assert.Equal("legacy", ex.Message);
    }
}
