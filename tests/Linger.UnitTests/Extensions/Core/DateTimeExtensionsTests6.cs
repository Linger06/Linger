namespace Linger.Tests.Extensions.Core;

public partial class DateTimeExtensionsTests
{
    [Fact]
    public void InRange_ReturnsTrue_WhenDateIsInRange()
    {
        var date = new DateTime(2023, 5, 15);
        var minValue = new DateTime(2023, 1, 1);
        var maxValue = new DateTime(2023, 12, 31);

        var result = date.InRange(minValue, maxValue);

        Assert.True(result);
    }

    [Fact]
    public void InRange_ReturnsFalse_WhenDateIsOutOfRange()
    {
        var date = new DateTime(2022, 12, 31);
        var minValue = new DateTime(2023, 1, 1);
        var maxValue = new DateTime(2023, 12, 31);

        var result = date.InRange(minValue, maxValue);

        Assert.False(result);
    }

    [Fact]
    public void DayInYear_ReturnsCorrectDayOfYear()
    {
        var date = new DateTime(2023, 5, 15);

        var result = date.DayInYear();

        Assert.Equal(135, result);
    }

    [Theory]
    [InlineData("2023-10-02", 1)] // Monday
    [InlineData("2023-10-03", 2)] // Tuesday
    [InlineData("2023-10-04", 3)] // Wednesday
    [InlineData("2023-10-05", 4)] // Thursday
    [InlineData("2023-10-06", 5)] // Friday
    [InlineData("2023-10-07", 6)] // Saturday
    [InlineData("2023-10-08", 7)] // Sunday
    public void DayInWeek_ShouldReturnCorrectResult(string dateStr, int expected)
    {
        // Arrange
        var date = DateTime.Parse(dateStr);

        // Act
        var result = date.DayInWeek();

        // Assert
        Assert.Equal(expected, result);
    }

#if NET6_0_OR_GREATER

    [Fact]
    public void ToDateOnly_ReturnsCorrectDateOnly()
    {
        var date = new DateTime(2023, 5, 15);

        var result = date.ToDateOnly();

        Assert.Equal(new DateOnly(2023, 5, 15), result);
    }

    [Fact]
    public void ToDateOnly_Nullable_ReturnsCorrectDateOnly()
    {
        DateTime? date = new DateTime(2023, 5, 15);

        var result = date.ToDateOnly();

        Assert.Equal(new DateOnly(2023, 5, 15), result);
    }

    [Fact]
    public void ToDateOnly_Nullable_ReturnsNull_WhenDateIsNull()
    {
        DateTime? date = null;

        var result = date.ToDateOnly();

        Assert.Null(result);
    }

    [Theory]
    [InlineData("2023-10-05", "00:00:00", "2023-10-05 00:00:00")] // ToDateTime with midnight
    [InlineData("2023-10-05", "14:30:00", "2023-10-05 14:30:00")] // ToDateTime with specific time
    public void ToDateTime_ShouldReturnCorrectResult(string dateStr, string timeStr, string expectedStr)
    {
        // Arrange
        var date = DateOnly.Parse(dateStr);
        var time = TimeOnly.Parse(timeStr);
        var expected = DateTime.Parse(expectedStr);

        // Act
        var result = date.ToDateTime(time);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToDateTime_NullableDateOnly_ShouldReturnCorrectResult()
    {
        // Arrange
        DateOnly? date = new DateOnly(2023, 10, 5);
        var time = new TimeOnly(14, 30);
        DateTime? expected = new DateTime(2023, 10, 5, 14, 30, 0);

        // Act
        var result = date.ToDateTime(time);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToDateTime_NullableDateOnly_ShouldReturnNull_WhenDateOnlyIsNull()
    {
        // Arrange
        DateOnly? date = null;
        var time = new TimeOnly(14, 30);

        // Act
        var result = date.ToDateTime(time);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToDateTime_WithMidnight_ShouldReturnCorrectResult()
    {
        // Arrange
        var date = new DateOnly(2023, 10, 5);
        var expected = new DateTime(2023, 10, 5, 0, 0, 0);

        // Act
        var result = date.ToDateTime();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToDateTime_NullableDateOnly_WithMidnight_ShouldReturnCorrectResult()
    {
        // Arrange
        DateOnly? date = new DateOnly(2023, 10, 5);
        DateTime? expected = new DateTime(2023, 10, 5, 0, 0, 0);

        // Act
        var result = date.ToDateTime();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToDateTime_NullableDateOnly_WithMidnight_ShouldReturnNull_WhenDateOnlyIsNull()
    {
        // Arrange
        DateOnly? date = null;

        // Act
        var result = date.ToDateTime();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToTimeOnly_ReturnsCorrectTimeOnly()
    {
        var date = new DateTime(2023, 5, 15, 10, 30, 0);

        var result = date.ToTimeOnly();

        Assert.Equal(new TimeOnly(10, 30), result);
    }

    [Fact]
    public void ToTimeOnly_Nullable_ReturnsCorrectTimeOnly()
    {
        DateTime? date = new DateTime(2023, 5, 15, 10, 30, 0);

        var result = date.ToTimeOnly();

        Assert.Equal(new TimeOnly(10, 30), result);
    }

    [Fact]
    public void ToTimeOnly_Nullable_ReturnsNull_WhenDateIsNull()
    {
        DateTime? date = null;

        var result = date.ToTimeOnly();

        Assert.Null(result);
    }

#endif
}