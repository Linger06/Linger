namespace Linger.UnitTests.Extensions.Core;

public class StringExtensionsTests
{

    [Fact]
    public void IsNull_NullString_ReturnsTrue()
    {
        // Arrange
        string? value = null;

        // Act
        var result = value.IsNull();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNull_NonNullString_ReturnsFalse()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsNull();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEmpty_EmptyString_ReturnsTrue()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var result = value.IsEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEmpty_NonEmptyString_ReturnsFalse()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsEmpty();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNullOrEmpty_NullString_ReturnsTrue()
    {
        // Arrange
        string? value = null;

        // Act
        var result = value.IsNullOrEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrEmpty_EmptyString_ReturnsTrue()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var result = value.IsNullOrEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrEmpty_NonEmptyString_ReturnsFalse()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsNullOrEmpty();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNullOrWhiteSpace_NullString_ReturnsTrue()
    {
        // Arrange
        string? value = null;

        // Act
        var result = value.IsNullOrWhiteSpace();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrWhiteSpace_WhiteSpaceString_ReturnsTrue()
    {
        // Arrange
        var value = "   ";

        // Act
        var result = value.IsNullOrWhiteSpace();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullOrWhiteSpace_NonWhiteSpaceString_ReturnsFalse()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsNullOrWhiteSpace();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNotNull_NonNullString_ReturnsTrue()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsNotNull();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNotNull_NullString_ReturnsFalse()
    {
        // Arrange
        string? value = null;

        // Act
        var result = value.IsNotNull();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNotEmpty_NonEmptyString_ReturnsTrue()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsNotEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNotEmpty_EmptyString_ReturnsFalse()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var result = value.IsNotEmpty();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNotNullAndEmpty_NonNullNonEmptyString_ReturnsTrue()
    {
        // Arrange
        var value = "test";

        // Act
        var result = value.IsNotNullAndEmpty();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNotNullAndEmpty_NullString_ReturnsFalse()
    {
        // Arrange
        string? value = null;

        // Act
        var result = value.IsNotNullAndEmpty();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsNotNullAndEmpty_EmptyString_ReturnsFalse()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var result = value.IsNotNullAndEmpty();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInt16_ValidInt16String_ReturnsTrue()
    {
        // Arrange
        var value = "12345";

        // Act
        var result = value.IsInt16();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInt16_InvalidInt16String_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsInt16();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInt_ValidIntString_ReturnsTrue()
    {
        // Arrange
        var value = "1234567890";

        // Act
        var result = value.IsInt();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInt_InvalidIntString_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsInt();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInt64_ValidInt64String_ReturnsTrue()
    {
        // Arrange
        var value = "1234567890123456789";

        // Act
        var result = value.IsInt64();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsInt64_InvalidInt64String_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsInt64();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDecimal_ValidDecimalString_ReturnsTrue()
    {
        // Arrange
        var value = "12345.6789";

        // Act
        var result = value.IsDecimal();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDecimal_InvalidDecimalString_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsDecimal();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSingle_ValidSingleString_ReturnsTrue()
    {
        // Arrange
        var value = "12345.6789";

        // Act
        var result = value.IsSingle();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSingle_InvalidSingleString_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsSingle();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDouble_ValidDoubleString_ReturnsTrue()
    {
        // Arrange
        var value = "12345.6789";

        // Act
        var result = value.IsDouble();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDouble_InvalidDoubleString_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsDouble();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsDateTime_ValidDateTimeString_ReturnsTrue()
    {
        // Arrange
        var value = "2021-10-01";

        // Act
        var result = value.IsDateTime();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDateTime_InvalidDateTimeString_ReturnsFalse()
    {
        // Arrange
        var value = "invalid";

        // Act
        var result = value.IsDateTime();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", true)]
    [InlineData("abc", false)]
    public void IsBoolean_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsBoolean();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("d3c4a2b1-4f5e-6d7c-8b9a-0f1e2d3c4b5a", true)]
    [InlineData("abc", false)]
    public void IsGuid_ShouldReturnExpectedResult(string value, bool expected)
    {
        var result = value.IsGuid();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("d3c4a2b14f5e6d7c8b9a0f1e2d3c4b5a", "N", true)]
    [InlineData("abc", "N", false)]
    public void IsGuid_WithFormat_ShouldReturnExpectedResult(string value, string format, bool expected)
    {
        var result = value.IsGuid(format);
        Assert.Equal(expected, result);
    }
}