namespace Linger.Extensions.Core.Tests;

public partial class DateTimeExtensionsTests
{
    [Fact]
    public void EndOfMonth_ShouldReturnLastMomentOfMonth()
    {
        var date = new DateTime(2023, 10, 1);
        var expected = new DateTime(2023, 10, 31, 23, 59, 59, 999);
        Assert.Equal(expected, date.EndOfMonth());
    }

    [Fact]
    public void LastDayOfMonth_ShouldReturnLastDay()
    {
        var date = new DateTime(2023, 10, 1);
        var expected = new DateTime(2023, 10, 31);
        Assert.Equal(expected, date.LastDayOfMonth());
    }

    [Fact]
    public void LastDayOfMonth_WithDayOfWeek_ShouldReturnLastSpecifiedDayOfWeek()
    {
        var date = new DateTime(2023, 10, 1);
        var expected = new DateTime(2023, 10, 25); // Last Wednesday of October 2023
        Assert.Equal(expected, date.LastDayOfMonth(DayOfWeek.Wednesday));
    }


    [Fact]
    public void FirstDayOfMonth2_ShouldReturnFirstDay()
    {
        var date = new DateTime(2023, 10, 4);
        var expected = new DateTime(2023, 10, 1);
        Assert.Equal(expected, date.FirstDayOfMonth2(TimeMode.Zero));
    }

    [Fact]
    public void FirstDayOfMonth_ShouldReturnFirstDay()
    {
        var date = new DateTime(2023, 10, 4);
        var expected = new DateTime(2023, 10, 1);
        Assert.Equal(expected, date.FirstDayOfMonth());
    }

    [Fact]
    public void FirstDayOfMonth_WithDayOfWeek_ShouldReturnFirstSpecifiedDayOfWeek()
    {
        var date = new DateTime(2023, 10, 4);
        var expected = new DateTime(2023, 10, 2); // First Monday of October 2023
        Assert.Equal(expected, date.FirstDayOfMonth(DayOfWeek.Monday));
    }

    [Theory]
    [InlineData(2023, 10, 5, TimeMode.Now, 2023, 10, 5, 0, 0, 0, 0)] // TimeMode.Now
    [InlineData(2023, 10, 5, TimeMode.Zero, 2023, 10, 5, 0, 0, 0, 0)] // TimeMode.Zero
    [InlineData(2023, 10, 5, TimeMode.Full, 2023, 10, 5, 23, 59, 59, 999)] // TimeMode.Full
    public void ToDateTimeOfMode_ShouldReturnCorrectDateTime(int year, int month, int day, TimeMode mode, int expectedYear, int expectedMonth, int expectedDay, int expectedHour, int expectedMinute, int expectedSecond, int expectedMillisecond)
    {
        // Arrange
        var dateTime = new DateTime(year, month, day);

        // Act
        DateTime result = dateTime.ToDateTimeOfMode(mode);

        // Assert
        Assert.Equal(expectedYear, result.Year);
        Assert.Equal(expectedMonth, result.Month);
        Assert.Equal(expectedDay, result.Day);
        Assert.Equal(expectedHour, result.Hour);
        Assert.Equal(expectedMinute, result.Minute);
        Assert.Equal(expectedSecond, result.Second);
        Assert.Equal(expectedMillisecond, result.Millisecond);
    }

    [Theory]
    [InlineData(2020, true)]  // Leap year
    [InlineData(2021, false)] // Not a leap year
    [InlineData(1900, false)] // Not a leap year (divisible by 100 but not by 400)
    [InlineData(2000, true)]  // Leap year (divisible by 400)
    public void IsLeapYear_ShouldReturnCorrectResult(int year, bool expected)
    {
        // Arrange
        var date = new DateTime(year, 1, 1);

        // Act
        var result = date.IsLeapYear();

        // Assert
        Assert.Equal(expected, result);
    }
}