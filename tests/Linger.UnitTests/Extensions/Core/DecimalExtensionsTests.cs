namespace Linger.UnitTests.Extensions.Core;

using Xunit;

public class DecimalExtensionsTests
{

    [Fact]
    public void IsInteger_ReturnsTrueForIntegerValue()
    {
        var value = 123m;
        var result = value.IsInteger();
        Assert.True(result);
    }

    [Fact]
    public void IsInteger_ReturnsFalseForNonIntegerValue()
    {
        var value = 123.45m;
        var result = value.IsInteger();
        Assert.False(result);
    }

    [Fact]
    public void ToInt_ConvertsIntegerValue()
    {
        var value = 123m;
        var result = value.ToInt();
        Assert.Equal(123, result);
    }

    [Fact]
    public void ToInt_ThrowsExceptionForNonIntegerValue()
    {
        var value = 123.45m;
        Assert.Throws<InvalidCastException>(() => value.ToInt());
    }

    [Fact]
    public void ToRounding_RoundsValueToSpecifiedDecimalPlaces()
    {
        var value = 123.4567m;
        var result = value.ToRounding(2);
        Assert.Equal(123.46m, result);
    }

    [Fact]
    public void DeleteZero_RemovesTrailingZeros()
    {
        var value = 123.4500m;
        var result = value.DeleteZero();
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void ToStringDeleteZero_FormatsDecimalWithoutTrailingZeros()
    {
        var value = 123.4500m;
        var result = value.ToStringDeleteZero();
        Assert.Equal("123.45", result);
    }

    [Fact]
    public void ToStringDeleteZero_NullableDecimalFormatsWithoutTrailingZeros()
    {
        decimal? value = 123.4500m;
        var result = value.ToStringDeleteZero();
        Assert.Equal("123.45", result);
    }

    [Fact]
    public void ToStringDeleteZero_NullableDecimalReturnsNull()
    {
        decimal? value = null;
        var result = value.ToStringDeleteZero();
        Assert.Null(result);
    }
}