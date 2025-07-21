namespace Linger.Results.UnitTests;

public class ErrorTests
{
    [Fact]
    public void Constructor_WithCodeAndMessage_ShouldSetProperties()
    {
        // Arrange
        const string code = "TEST_ERROR";
        const string message = "This is a test error";

        // Act
        var error = new Error(code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessageAndEmptyCode()
    {
        // Arrange
        const string message = "This is a test error";

        // Act
        var error = new Error(string.Empty, message);

        // Assert
        Assert.Equal(string.Empty, error.Code);
        Assert.Equal(message, error.Message);
    }

    [Fact]
    public void Equality_WithSameCodeAndMessage_ShouldBeEqual()
    {
        // Arrange
        var error1 = new Error("TEST", "Message");
        var error2 = new Error("TEST", "Message");

        // Assert
        Assert.Equal(error1, error2);
        Assert.True(error1 == error2);
        Assert.False(error1 != error2);
        Assert.Equal(error1.GetHashCode(), error2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentCode_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = new Error("TEST1", "Message");
        var error2 = new Error("TEST2", "Message");

        // Assert
        Assert.NotEqual(error1, error2);
        Assert.False(error1 == error2);
        Assert.True(error1 != error2);
    }

    [Fact]
    public void Equality_WithDifferentMessage_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = new Error("TEST", "Message1");
        var error2 = new Error("TEST", "Message2");

        // Assert
        Assert.NotEqual(error1, error2);
        Assert.False(error1 == error2);
        Assert.True(error1 != error2);
    }

    [Fact]
    public void Equality_WithNull_ShouldNotBeEqual()
    {
        // Arrange
        var error = new Error("TEST", "Message");

        // Assert
        Assert.False(error.Equals(null));
        Assert.False(error == null);
        Assert.True(error != null);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "This is a test error");

        // Act
        var result = error.ToString();

        // Assert
        Assert.Equal("TEST_ERROR: This is a test error", result);
    }

    [Fact]
    public void ToString_WithEmptyCode_ShouldReturnMessageOnly()
    {
        // Arrange
        var error = new Error(string.Empty, "This is a test error");

        // Act
        var result = error.ToString();

        // Assert
        Assert.Equal("This is a test error", result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("CODE", "")]
    [InlineData("", "Message")]
    [InlineData("CODE", "Message")]
    public void Constructor_WithVariousInputs_ShouldHandleCorrectly(string code, string message)
    {
        // Act
        var error = new Error(code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
    }

    [Fact]
    public void PredefinedErrors_ShouldHaveCorrectValues()
    {
        // Arrange & Act & Assert
        Assert.Equal("Error.ConditionNotMet", Error.ConditionNotMet.Code);
        Assert.Equal("The specified condition was not met.", Error.ConditionNotMet.Message);

        Assert.Equal("Error.NullValue", Error.NullValue.Code);
        Assert.Equal("Value cannot be null.", Error.NullValue.Message);
    }
}
