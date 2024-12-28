namespace Linger.UnitTests.Extensions;

public class GuardExtensionsTests
{
    [Fact]
    public void EnsureIsNotNull_WithNonNullValue_DoesNotThrow()
    {
        // Arrange
        var obj = new object();

        // Act & Assert
        obj.EnsureIsNotNull();
    }

    [Fact]
    public void EnsureIsNotNull_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        object? obj = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => obj.EnsureIsNotNull());
    }

    [Fact]
    public void EnsureStringIsNotNullOrEmpty_WithNonEmptyString_DoesNotThrow()
    {
        // Arrange
        var str = "test";

        // Act & Assert
        str.EnsureIsNotNullAndEmpty();
    }

    [Fact]
    public void EnsureStringIsNotNullOrEmpty_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        var str = "";

        // Act & Assert
        ArgumentException? exception = Assert.Throws<ArgumentException>(() => str.EnsureIsNotNullAndEmpty());
        Assert.Equal("Value cannot be an empty string", exception.Message);
    }

    [Fact]
    public void EnsureStringIsNotNullOrEmpty_WithNullString_ThrowsArgumentNullException()
    {
        // Arrange
        string? str = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => str.EnsureIsNotNullAndEmpty());
    }

    [Fact]
    public void EnsureIsNull_WithNullValue_DoesNotThrow()
    {
        // Arrange
        object? obj = null;

        // Act & Assert
        obj.EnsureIsNull();
    }

    [Fact]
    public void EnsureIsNull_WithNonNullValue_ThrowsArgumentException()
    {
        // Arrange
        var obj = new object();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => obj.EnsureIsNull());
    }
}