namespace Linger.UnitTests.Extensions.Core;

using Xunit;

public class ShortExtensionsTests
{
    [Fact]
    public void ToThousand_FormatsShortWithThousandSeparators()
    {
        short value = 12345;
        var result = value.ToThousand();
        Assert.Equal("12,345.00", result);
    }

    [Fact]
    public void ToThousand_FormatsNegativeShortWithThousandSeparators()
    {
        short value = -12345;
        var result = value.ToThousand();
        Assert.Equal("-12,345.00", result);
    }

    [Fact]
    public void ToThousand_FormatsZeroWithThousandSeparators()
    {
        short value = 0;
        var result = value.ToThousand();
        Assert.Equal("0.00", result);
    }

    [Fact]
    public void ToThousand_FormatsMaxShortWithThousandSeparators()
    {
        var value = short.MaxValue;
        var result = value.ToThousand();
        Assert.Equal("32,767.00", result);
    }

    [Fact]
    public void ToThousand_FormatsMinShortWithThousandSeparators()
    {
        var value = short.MinValue;
        var result = value.ToThousand();
        Assert.Equal("-32,768.00", result);
    }
}