namespace Linger.UnitTests.Extensions.Core;

using Xunit;

public class SingleExtensionsTests
{
    [Fact]
    public void ToThousand_FormatsFloatWithThousandSeparators()
    {
        var value = 12345.678f;
        var result = value.ToThousand();
        Assert.Equal("12,345.68", result);
    }

    [Fact]
    public void ToThousand_FormatsNegativeFloatWithThousandSeparators()
    {
        var value = -12345.678f;
        var result = value.ToThousand();
        Assert.Equal("-12,345.68", result);
    }

    [Fact]
    public void ToThousand_FormatsZeroWithThousandSeparators()
    {
        var value = 0f;
        var result = value.ToThousand();
        Assert.Equal("0.00", result);
    }
}