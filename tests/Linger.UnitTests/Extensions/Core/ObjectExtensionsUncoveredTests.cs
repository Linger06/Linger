using System.Globalization;

namespace Linger.UnitTests.Extensions.Core;

/// <summary>
/// Unit tests for uncovered ObjectExtensions methods to improve test coverage.
/// </summary>
public class ObjectExtensionsUncoveredTests
{
    #region ToTrimmedString Tests

    [Fact]
    public void ToTrimmedString_ShouldReturnTrimmedString_WhenInputHasWhitespace()
    {
        object input = "  Hello World  ";
        var result = input.ToTrimmedString();
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void ToTrimmedString_ShouldReturnEmptyString_WhenInputIsNull()
    {
        object? input = null;
        var result = input.ToTrimmedString();
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ToTrimmedString_ShouldReturnTrimmedString_WhenInputIsNumber()
    {
        object input = 123;
        var result = input.ToTrimmedString();
        Assert.Equal("123", result);
    }

    [Fact]
    public void ToTrimmedString_ShouldReturnEmptyString_WhenInputIsWhitespaceOnly()
    {
        object input = "   ";
        var result = input.ToTrimmedString();
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region ToNormalizedString Tests

    [Fact]
    public void ToNormalizedString_ShouldReturnString_WhenTrimAndTreatEmptyAsNullAreFalse()
    {
        object input = "  Hello  ";
        var result = input.ToNormalizedString(trim: false, treatEmptyAsNull: false);
        Assert.Equal("  Hello  ", result);
    }

    [Fact]
    public void ToNormalizedString_ShouldReturnTrimmedString_WhenTrimIsTrue()
    {
        object input = "  Hello  ";
        var result = input.ToNormalizedString(trim: true, treatEmptyAsNull: false);
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void ToNormalizedString_ShouldReturnNull_WhenTreatEmptyAsNullIsTrueAndInputIsEmpty()
    {
        object input = "";
        var result = input.ToNormalizedString(trim: false, treatEmptyAsNull: true);
        Assert.Null(result);
    }

    [Fact]
    public void ToNormalizedString_ShouldReturnNull_WhenTrimAndTreatEmptyAsNullAreTrueAndInputIsWhitespace()
    {
        object input = "   ";
        var result = input.ToNormalizedString(trim: true, treatEmptyAsNull: true);
        Assert.Null(result);
    }

    [Fact]
    public void ToNormalizedString_ShouldReturnNull_WhenInputIsNull()
    {
        object? input = null;
        var result = input.ToNormalizedString(trim: true, treatEmptyAsNull: true);
        Assert.Null(result);
    }

    [Fact]
    public void ToNormalizedString_ShouldReturnTrimmedString_WhenBothOptionsAreTrue()
    {
        object input = "  Hello World  ";
        var result = input.ToNormalizedString(trim: true, treatEmptyAsNull: true);
        Assert.Equal("Hello World", result);
    }

    #endregion

    #region TryToShort Tests

    [Theory]
    [InlineData("123", true, (short)123)]
    [InlineData("-123", true, (short)-123)]
    [InlineData("32767", true, short.MaxValue)]
    [InlineData("-32768", true, short.MinValue)]
    [InlineData("abc", false, (short)0)]
    [InlineData("32768", false, (short)0)] // Overflow
    [InlineData(null, false, (short)0)]
    public void TryToShort_ShouldReturnExpectedResult(object? input, bool expectedSuccess, short expectedValue)
    {
        var success = input.TryToShort(out var value);
        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expectedValue, value);
    }

    [Fact]
    public void TryToShort_ShouldReturnTrue_WhenInputIsShort()
    {
        object input = (short)456;
        var success = input.TryToShort(out var value);
        Assert.True(success);
        Assert.Equal(456, value);
    }

    #endregion

    #region TryToDateTime Tests

    [Theory]
    [InlineData("2023-12-25", true)]
    [InlineData("2023/12/25", true)]
    [InlineData("12/25/2023", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void TryToDateTime_ShouldReturnExpectedResult(object? input, bool expectedSuccess)
    {
        var success = input.TryToDateTime(out var value);
        Assert.Equal(expectedSuccess, success);

        if (expectedSuccess)
        {
            Assert.NotEqual(DateTime.MinValue, value);
        }
        else
        {
            Assert.Equal(DateTime.MinValue, value);
        }
    }

    [Fact]
    public void TryToDateTime_ShouldReturnTrue_WhenInputIsDateTime()
    {
        var inputDate = new DateTime(2023, 12, 25);
        object input = inputDate;
        var success = input.TryToDateTime(out var value);
        Assert.True(success);
        Assert.Equal(inputDate, value);
    }

    #endregion

    #region TryToBool Tests

    [Theory]
    [InlineData("true", true, true)]
    [InlineData("false", true, false)]
    [InlineData("True", true, true)]
    [InlineData("False", true, false)]
    [InlineData("1", true, true)]
    [InlineData("0", true, false)]
    [InlineData("abc", false, false)]
    [InlineData("", false, false)]
    [InlineData(null, false, false)]
    public void TryToBool_ShouldReturnExpectedResult(object? input, bool expectedSuccess, bool expectedValue)
    {
        var success = input.TryToBool(out var value);
        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expectedValue, value);
    }

    [Fact]
    public void TryToBool_ShouldReturnTrue_WhenInputIsBoolean()
    {
        object input = true;
        var success = input.TryToBool(out var value);
        Assert.True(success);
        Assert.True(value);

        input = false;
        success = input.TryToBool(out value);
        Assert.True(success);
        Assert.False(value);
    }

    #endregion

    #region TryToGuid Tests

    [Theory]
    [InlineData("d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a", true)]
    [InlineData("{d3b07384-d9a0-4f1b-8b0d-1d2b3e0b0a0a}", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void TryToGuid_ShouldReturnExpectedResult(object? input, bool expectedSuccess)
    {
        var success = input.TryToGuid(out var value);
        Assert.Equal(expectedSuccess, success);

        if (expectedSuccess)
        {
            Assert.NotEqual(Guid.Empty, value);
        }
        else
        {
            Assert.Equal(Guid.Empty, value);
        }
    }

    [Fact]
    public void TryToGuid_ShouldReturnTrue_WhenInputIsGuid()
    {
        var inputGuid = Guid.NewGuid();
        object input = inputGuid;
        var success = input.TryToGuid(out var value);
        Assert.True(success);
        Assert.Equal(inputGuid, value);
    }

    #endregion

    #region TryToDouble Tests

    [Theory]
    [InlineData("123.45", true, 123.45)]
    [InlineData("-123.45", true, -123.45)]
    [InlineData("0", true, 0.0)]
    [InlineData("abc", false, 0.0)]
    [InlineData("", false, 0.0)]
    [InlineData(null, false, 0.0)]
    public void TryToDouble_ShouldReturnExpectedResult(object? input, bool expectedSuccess, double expectedValue)
    {
        var success = input.TryToTarget<double>(out var value);
        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expectedValue, value, 5); // 5 decimal places precision
    }

    [Fact]
    public void TryToDouble_ShouldReturnTrue_WhenInputIsDouble()
    {
        object input = 456.789;
        var success = input.TryToTarget<double>(out var value);
        Assert.True(success);
        Assert.Equal(456.789, value, 5);
    }

    [Fact]
    public void TryToDouble_ShouldHandleExtremeValues()
    {
        // Test with large but parseable values instead of extreme values
        object input = "1.7976931348623157E+308"; // Close to double.MaxValue but parseable
        var success = input.TryToTarget<double>(out var value);
        Assert.True(success);
        Assert.True(value > 1E+300);

        input = "-1.7976931348623157E+308"; // Close to double.MinValue but parseable
        success = input.TryToTarget<double>(out value);
        Assert.True(success);
        Assert.True(value < -1E+300);
    }

    #endregion

    #region TryToFloat Tests

    [Theory]
    [InlineData("123.45", true, 123.45f)]
    [InlineData("-123.45", true, -123.45f)]
    [InlineData("0", true, 0.0f)]
    [InlineData("abc", false, 0.0f)]
    [InlineData("", false, 0.0f)]
    [InlineData(null, false, 0.0f)]
    public void TryToFloat_ShouldReturnExpectedResult(object? input, bool expectedSuccess, float expectedValue)
    {
        var success = input.TryToTarget<float>(out var value);
        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expectedValue, value, 5); // 5 decimal places precision
    }

    [Fact]
    public void TryToFloat_ShouldReturnTrue_WhenInputIsFloat()
    {
        object input = 456.789f;
        var success = input.TryToTarget<float>(out var value);
        Assert.True(success);
        Assert.Equal(456.789f, value, 5);
    }

    [Fact]
    public void TryToFloat_ShouldHandleExtremeValues()
    {
        // Test with large but parseable values instead of extreme values
        object input = "3.40282E+38"; // Close to float.MaxValue but parseable
        var success = input.TryToTarget<float>(out var value);
        Assert.True(success);
        Assert.True(value > 1E+30f);

        input = "-3.40282E+38"; // Close to float.MinValue but parseable  
        success = input.TryToTarget<float>(out value);
        Assert.True(success);
        Assert.True(value < -1E+30f);
    }

    #endregion

    #region Generic Long Tail Tests

    [Fact]
    public void TryToTarget_GuidFromString_ShouldReturnTrueAndParsedValue()
    {
        object input = "12345678-1234-1234-1234-123456789abc";
        var success = input.TryToTarget<Guid>(out var value);

        Assert.True(success);
        Assert.Equal(Guid.Parse("12345678-1234-1234-1234-123456789abc"), value);
    }

    [Fact]
    public void TryToTarget_TimeSpanFromString_ShouldReturnTrueAndParsedValue()
    {
        object input = "01:02:03";
        var success = input.TryToTarget<TimeSpan>(out var value);

        Assert.True(success);
        Assert.Equal(new TimeSpan(1, 2, 3), value);
    }

    [Fact]
    public void TryToTarget_EnumFromName_ShouldReturnTrueAndParsedValue()
    {
        object input = "Friday";
        var success = input.TryToTarget<DayOfWeek>(out var value);

        Assert.True(success);
        Assert.Equal(DayOfWeek.Friday, value);
    }

    [Fact]
    public void TryToTarget_EnumFromUndefinedNumericString_ShouldReturnFalse()
    {
        object input = "999";
        var success = input.TryToTarget<DayOfWeek>(out var value);

        Assert.False(success);
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryToTarget_NullableEnumFromName_ShouldReturnTrueAndParsedValue()
    {
        object input = "Friday";
        var success = input.TryToTarget<DayOfWeek?>(out var value);

        Assert.True(success);
        Assert.Equal(DayOfWeek.Friday, value);
    }

    [Fact]
    public void TryToTarget_BoolFromOnOffText_ShouldReuseSharedBoolRules()
    {
        object onInput = "on";
        var onSuccess = onInput.TryToTarget<bool>(out var onValue);
        Assert.True(onSuccess);
        Assert.True(onValue);

        object offInput = "off";
        var offSuccess = offInput.TryToTarget<bool>(out var offValue);
        Assert.True(offSuccess);
        Assert.False(offValue);
    }

    [Fact]
    public void ToTarget_ShouldThrowArgumentNullException_WhenInputIsNull()
    {
        object? input = null;

        Assert.Throws<ArgumentNullException>(() => input.ToTarget<string>());
    }

    [Fact]
    public void ToTarget_ShouldThrowInvalidCastException_WhenConversionFails()
    {
        object input = "not-a-number";

        Assert.Throws<InvalidCastException>(() => input.ToTarget<int>());
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public void UncoveredMethods_ShouldHandleNullInputGracefully()
    {
        object? nullInput = null;

        // All Try methods should return false for null input
        Assert.False(nullInput.TryToShort(out _));
        Assert.False(nullInput.TryToDateTime(out _));
        Assert.False(nullInput.TryToBool(out _));
        Assert.False(nullInput.TryToGuid(out _));
        Assert.False(nullInput.TryToTarget<double>(out _));
        Assert.False(nullInput.TryToTarget<float>(out _));

        // String conversion methods should handle null gracefully
        Assert.Equal(string.Empty, nullInput.ToTrimmedString());
        Assert.Null(nullInput.ToNormalizedString());
    }

    [Fact]
    public void UncoveredMethods_ShouldWorkWithComplexObjects()
    {
        var complexObject = new { Name = "Test", Value = 123 };

        // Should convert to string representation
        var normalizedString = complexObject.ToNormalizedString(trim: true);
        Assert.NotNull(normalizedString);
        Assert.Contains("Test", normalizedString);
        Assert.Contains("123", normalizedString);

        var trimmedString = complexObject.ToTrimmedString();
        Assert.NotNull(trimmedString);
        Assert.Contains("Test", trimmedString);
    }

    [Theory]
    [InlineData("  123  ", "123")]
    [InlineData("\t456\n", "456")]
    [InlineData("  ", "")]
    [InlineData("", "")]
    public void ToTrimmedString_ShouldHandleVariousWhitespaceTypes(string input, string expected)
    {
        object obj = input;
        var result = obj.ToTrimmedString();
        Assert.Equal(expected, result);
    }

    #endregion
}
