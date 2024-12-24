namespace Linger.Extensions.Tests;

public partial class DateTimeExtensionsTests
{
    [Fact]
    public void CalculateAge_Today()
    {
        var dateOfBirth = new DateTime(2000, 1, 1);
        var age = dateOfBirth.CalculateAge();
        Assert.Equal(DateTime.Now.Year - 2000, age);
    }

    [Theory]
    [InlineData(1990, 1, 1, 2023, 1, 1, 33)]
    [InlineData(1990, 1, 1, 2023, 2, 1, 33)] // Different months
    [InlineData(1990, 1, 15, 2023, 1, 1, 32)] // Same month, different days
    public void CalculateAge_ShouldReturnCorrectAge(int birthYear, int birthMonth, int birthDay, int refYear, int refMonth, int refDay, int expectedAge)
    {
        // Arrange
        var birthDate = new DateTime(birthYear, birthMonth, birthDay);
        var referenceDate = new DateTime(refYear, refMonth, refDay);

        // Act
        var age = birthDate.CalculateAge(referenceDate);

        // Assert
        Assert.Equal(expectedAge, age);
    }

    [Fact]
    public void GetCountDaysOfMonth()
    {
        var date = new DateTime(2021, 2, 1);
        var days = date.GetCountDaysOfMonth();
        Assert.Equal(28, days);
    }

    [Fact]
    public void IsToday()
    {
        DateTime date = DateTime.Today;
        Assert.True(date.IsToday());
    }

    [Fact]
    public void SetTime_Hour()
    {
        var date = new DateTime(2021, 1, 1);
        DateTime newDate = date.SetTime(10);
        Assert.Equal(new DateTime(2021, 1, 1, 10, 0, 0), newDate);
    }

    [Fact]
    public void SetTime_HourMinuteSecondMillisecond()
    {
        var date = new DateTime(2021, 1, 1);
        DateTime newDate = date.SetTime(10, 30, 45, 500);
        Assert.Equal(new DateTime(2021, 1, 1, 10, 30, 45, 500), newDate);
    }
}