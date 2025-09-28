namespace Linger.UnitTests.Extensions.Core;

using Xunit;

public class DecimalExtensionsTests
{
    public static IEnumerable<object[]> FileData = new List<object[]>
    {
        new object[] { 0.03, 2, 0.03 },
        new object[] { 1.23, 2, 1.23 },
        new object[] { 1.45, 0, 1 },
        new object[] { 1.45, 1, 1.5 },
        new object[] { 1.45, 2, 1.45 },
        new object[] { 1.45, 3, 1.450 },
        new object[] { 1.456, 3, 1.456 }
    };

    [Theory]
    [InlineData(1.1)]
    [InlineData(-1.1)]
    [InlineData(0.1)]
    [InlineData(-0.1)]
    [InlineData(0.123456)]
    [InlineData(-0.123456)]
    public void IsInteger(decimal value)
    {
        var result = value.IsInteger();
        Assert.False(result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    public void IsInteger2(decimal value)
    {
        var result = value.IsInteger();
        Assert.True(result);
    }

    [Theory]
    [MemberData(nameof(FileData))]
    public void ToRounding(decimal num1, int num2, decimal num3)
    {
        var result = num1.ToRounding(num2);
        Assert.Equal(result, num3);
    }

    [Theory]
    [InlineData(0.2000001, 0.2000001)]
    [InlineData(0.2000000, 0.2)]
    [InlineData(123.000, 123)]
    public void DeleteZeroTest(decimal value, decimal value2)
    {
        Assert.Equal(value.DeleteZero(), value2);
    }

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