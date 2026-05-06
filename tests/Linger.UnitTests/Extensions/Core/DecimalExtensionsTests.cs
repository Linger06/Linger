namespace Linger.UnitTests.Extensions.Core;

using Xunit;

public class DecimalExtensionsTests
{
    

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

        public static TheoryData<decimal, int> ToIntData()
    {
        return new TheoryData<decimal, int>
        {
            { 123m, 123 },
            { 1.0000m, 1 }
        };
    }

    [Theory]
    [MemberData(nameof(ToIntData))]
    public void ToInt_ConvertsIntegerValue(decimal value, int expected)
    {
        var result = value.ToInt();
        Assert.Equal(expected, result);
    }

    public static TheoryData<decimal> ToIntThrowsData()
    {
        return new TheoryData<decimal>
        {
            { 123.45m },
            { (decimal)int.MaxValue + 1m }
        };
    }

    [Theory]
    [MemberData(nameof(ToIntThrowsData))]
    public void ToInt_ThrowsExceptionForInvalidValue(decimal value)
    {
        Assert.Throws<InvalidCastException>(() => value.ToInt());
    }

    public static TheoryData<decimal, int> ToIntBoundaryData()
    {
        return new TheoryData<decimal, int>
        {
            { int.MinValue, int.MinValue },
            { int.MaxValue, int.MaxValue }
        };
    }

    [Theory]
    [MemberData(nameof(ToIntBoundaryData))]
    public void ToInt_ConvertsBoundaryValues(decimal value, int expected)
    {
        var result = value.ToInt();
        Assert.Equal(expected, result);
    }

    public static TheoryData<decimal> ToIntOutOfRangeBoundaryData()
    {
        return new TheoryData<decimal>
        {
            { int.MinValue - 1m },
            { int.MaxValue + 1m }
        };
    }

    [Theory]
    [MemberData(nameof(ToIntOutOfRangeBoundaryData))]
    public void ToInt_ThrowsForOutOfRangeBoundaryValues(decimal value)
    {
        Assert.Throws<InvalidCastException>(() => value.ToInt());
    }

    [Fact]
    public void ToInt_ThrowsExceptionWithInputValueInMessage()
    {
        const decimal value = 123.45m;

        var exception = Assert.Throws<InvalidCastException>(() => value.ToInt());

        Assert.Contains("value=123.45", exception.Message);
    }

    public static TheoryData<decimal?, int?> ToIntOrNullData()
    {
        return new TheoryData<decimal?, int?>
        {
            { 123m, 123 },
            { 123.45m, null },
            { 1.0000m, 1 },
            { (decimal)int.MaxValue + 1m, null },
            { null, null }
        };
    }

    [Theory]
    [MemberData(nameof(ToIntOrNullData))]
    public void ToIntOrNull_ReturnsExpectedResult(decimal? value, int? expected)
    {
        var result = value.ToIntOrNull();
        Assert.Equal(expected, result);
    }

    public static TheoryData<decimal, int?> ToIntOrNullBoundaryData()
    {
        return new TheoryData<decimal, int?>
        {
            { int.MinValue, int.MinValue },
            { int.MaxValue, int.MaxValue },
            { int.MinValue - 1m, null },
            { int.MaxValue + 1m, null }
        };
    }

    [Theory]
    [MemberData(nameof(ToIntOrNullBoundaryData))]
    public void ToIntOrNull_ReturnsExpectedForBoundaryValues(decimal value, int? expected)
    {
        var result = value.ToIntOrNull();
        Assert.Equal(expected, result);
    }

    public static TheoryData<decimal?, int, int> ToIntOrDefaultData()
    {
        return new TheoryData<decimal?, int, int>
        {
            { 123.45m, 42, 42 },
            { 1.0000m, 0, 1 },
            { null, 42, 42 }
        };
    }

    [Theory]
    [MemberData(nameof(ToIntOrDefaultData))]
    public void ToIntOrDefault_ReturnsExpectedResult(decimal? value, int defaultValue, int expected)
    {
        var result = value.ToIntOrDefault(defaultValue);
        Assert.Equal(expected, result);
    }

    public static TheoryData<decimal, int, int> ToIntOrDefaultBoundaryData()
    {
        return new TheoryData<decimal, int, int>
        {
            { int.MinValue, 42, int.MinValue },
            { int.MaxValue, 42, int.MaxValue },
            { int.MinValue - 1m, 42, 42 },
            { int.MaxValue + 1m, 42, 42 }
        };
    }

    [Theory]
    [MemberData(nameof(ToIntOrDefaultBoundaryData))]
    public void ToIntOrDefault_ReturnsExpectedForBoundaryValues(decimal value, int defaultValue, int expected)
    {
        var result = value.ToIntOrDefault(defaultValue);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(42, 42)]
    [InlineData(-1, -1)]
    public void ToIntOrDefault_NullableBoundaryValue_ReturnsDefaultWhenNull(int defaultValue, int expected)
    {
        decimal? value = null;

        var result = value.ToIntOrDefault(defaultValue);

        Assert.Equal(expected, result);
    }

    public static TheoryData<decimal?, bool, int> TryToIntData()
    {
        return new TheoryData<decimal?, bool, int>
        {
            { 123m, true, 123 },
            { 123.45m, false, 0 },
            { 1.0000m, true, 1 },
            { null, false, 0 }
        };
    }

    [Theory]
    [MemberData(nameof(TryToIntData))]
    public void TryToInt_ReturnsExpectedResult(decimal? value, bool expectedSuccess, int expected)
    {
        var success = value.TryToInt(out var result);
        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expected, result);
    }

    public static TheoryData<decimal, bool, int> TryToIntBoundaryData()
    {
        return new TheoryData<decimal, bool, int>
        {
            { int.MinValue, true, int.MinValue },
            { int.MaxValue, true, int.MaxValue },
            { int.MinValue - 1m, false, 0 },
            { int.MaxValue + 1m, false, 0 }
        };
    }

    [Theory]
    [MemberData(nameof(TryToIntBoundaryData))]
    public void TryToInt_ReturnsExpectedForBoundaryValues(decimal value, bool expectedSuccess, int expected)
    {
        var success = value.TryToInt(out var result);

        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryToInt_NullableBoundaryValue_ReturnsFalseWithZeroResultWhenNull()
    {
        decimal? value = null;

        var success = value.TryToInt(out var result);

        Assert.False(success);
        Assert.Equal(0, result);
    }

    public static TheoryData<decimal?, int, int?, int, bool, int> ConversionMatrixData()
    {
        return new TheoryData<decimal?, int, int?, int, bool, int>
        {
            { 123m, 42, 123, 123, true, 123 },
            { 1.0000m, 42, 1, 1, true, 1 },
            { 123.45m, 42, null, 42, false, 0 },
            { int.MinValue, 42, int.MinValue, int.MinValue, true, int.MinValue },
            { int.MaxValue, 42, int.MaxValue, int.MaxValue, true, int.MaxValue },
            { int.MinValue - 1m, 42, null, 42, false, 0 },
            { int.MaxValue + 1m, 42, null, 42, false, 0 },
            { null, 42, null, 42, false, 0 }
        };
    }

    [Theory]
    [MemberData(nameof(ConversionMatrixData))]
    public void ConversionApis_ShouldBeConsistentOnSameInput(decimal? value, int defaultValue, int? expectedNullable, int expectedDefault, bool expectedTrySuccess, int expectedTryResult)
    {
        var nullableResult = value.ToIntOrNull();
        var defaultResult = value.ToIntOrDefault(defaultValue);
        var trySuccess = value.TryToInt(out var tryResult);

        Assert.Equal(expectedNullable, nullableResult);
        Assert.Equal(expectedDefault, defaultResult);
        Assert.Equal(expectedTrySuccess, trySuccess);
        Assert.Equal(expectedTryResult, tryResult);
    }

    [Theory]
    [MemberData(nameof(ConversionMatrixData))]
    public void ConversionApis_Invariant_WhenTryToIntSucceeds_ToIntOrNullMustMatch(decimal? value, int defaultValue, int? expectedNullable, int expectedDefault, bool expectedTrySuccess, int expectedTryResult)
    {
        _ = defaultValue;
        _ = expectedNullable;
        _ = expectedDefault;
        _ = expectedTrySuccess;
        _ = expectedTryResult;

        var trySuccess = value.TryToInt(out var tryResult);
        var nullableResult = value.ToIntOrNull();

        if (trySuccess)
        {
            Assert.True(nullableResult.HasValue);
            Assert.Equal(tryResult, nullableResult.Value);
        }
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