using System.Globalization;

namespace Linger.Tests.Extensions.Core;

public partial class DateTimeExtensionsTests
{
    [Fact]
    public void IsDateEqual_ReturnsTrueForSameDate()
    {
        var date1 = new DateTime(2023, 10, 1);
        var date2 = new DateTime(2023, 10, 1);
        Assert.True(date1.IsDateEqual(date2));
    }

    [Fact]
    public void IsTimeEqual_ReturnsTrueForSameTime()
    {
        var time1 = new DateTime(2023, 10, 1, 12, 0, 0);
        var time2 = new DateTime(2023, 10, 1, 12, 0, 0);
        Assert.True(time1.IsTimeEqual(time2));
    }

    [Fact]
    public void AddWeeks_AddsCorrectNumberOfDays()
    {
        var date = new DateTime(2023, 10, 1);
        DateTime newDate = date.AddWeeks(1);
        Assert.Equal(new DateTime(2023, 10, 8), newDate);
    }

    [Fact]
    public void GetDays_ReturnsCorrectNumberOfDaysInYear()
    {
        var days = DateTimeExtensions.GetDays(2023);
        Assert.Equal(365, days);
    }

    [Fact]
    public void GetDays_WithCulture_ReturnsCorrectNumberOfDaysInYear()
    {
        var days = DateTimeExtensions.GetDays(2023, CultureInfo.InvariantCulture);
        Assert.Equal(365, days);
    }

    [Fact]
    public void GetDays_FromDate_ReturnsCorrectNumberOfDaysInYear()
    {
        var date = new DateTime(2023, 10, 1);
        var days = date.GetDays();
        Assert.Equal(365, days);
    }

    [Fact]
    public void GetDays_FromDateWithCulture_ReturnsCorrectNumberOfDaysInYear()
    {
        var date = new DateTime(2023, 10, 1);
        var days = date.GetDays(CultureInfo.InvariantCulture);
        Assert.Equal(365, days);
    }

    [Fact]
    public void GetDays_BetweenTwoDates_ReturnsCorrectNumberOfDays()
    {
        var date1 = new DateTime(2023, 10, 1);
        var date2 = new DateTime(2023, 10, 10);
        var days = date1.GetDays(date2);
        Assert.Equal(9, days);
    }

    [Fact]
    public void IsEaster_ReturnsTrueForEasterSunday()
    {
        var easterSunday = new DateTime(2023, 4, 9);
        Assert.True(easterSunday.IsEaster());
    }

    [Fact]
    public void IsBefore_ReturnsTrueForEarlierDate()
    {
        var date1 = new DateTime(2023, 10, 1);
        var date2 = new DateTime(2023, 10, 2);
        Assert.True(date1.IsBefore(date2));
    }

    [Fact]
    public void IsAfter_ReturnsTrueForLaterDate()
    {
        var date1 = new DateTime(2023, 10, 2);
        var date2 = new DateTime(2023, 10, 1);
        Assert.True(date1.IsAfter(date2));
    }

    [Fact]
    public void EndOfDay_ReturnsCorrectEndOfDay()
    {
        var date = new DateTime(2023, 10, 1);
        DateTime endOfDay = date.EndOfDay();
        Assert.Equal(new DateTime(2023, 10, 1, 23, 59, 59, 999), endOfDay);
    }

    [Fact]
    public void IsDateEqual_ReturnsTrue_WhenDatesAreEqual()
    {
        var date1 = new DateTime(2023, 10, 5);
        var date2 = new DateTime(2023, 10, 5);
        var result = date1.IsDateEqual(date2);
        Assert.True(result);
    }

    [Fact]
    public void IsDateEqual_ReturnsFalse_WhenDatesAreNotEqual()
    {
        var date1 = new DateTime(2023, 10, 5);
        var date2 = new DateTime(2023, 10, 6);
        var result = date1.IsDateEqual(date2);
        Assert.False(result);
    }

    [Fact]
    public void IsTimeEqual_ReturnsTrue_WhenTimesAreEqual()
    {
        var time1 = new DateTime(2023, 10, 5, 14, 30, 0);
        var time2 = new DateTime(2023, 10, 6, 14, 30, 0);
        var result = time1.IsTimeEqual(time2);
        Assert.True(result);
    }

    [Fact]
    public void IsTimeEqual_ReturnsFalse_WhenTimesAreNotEqual()
    {
        var time1 = new DateTime(2023, 10, 5, 14, 30, 0);
        var time2 = new DateTime(2023, 10, 5, 15, 30, 0);
        var result = time1.IsTimeEqual(time2);
        Assert.False(result);
    }

    [Fact]
    public void AddWeeks_ReturnsCorrectDate()
    {
        var date = new DateTime(2023, 10, 5);
        DateTime result = date.AddWeeks(2);
        Assert.Equal(new DateTime(2023, 10, 19), result);
    }

    [Fact]
    public void GetDaysInYear_Returns365_ForNonLeapYear()
    {
        var year = 2023;
        var result = DateTimeExtensions.GetDays(year);
        Assert.Equal(365, result);
    }

    [Fact]
    public void GetDaysInYear_Returns366_ForLeapYear()
    {
        var year = 2024;
        var result = DateTimeExtensions.GetDays(year);
        Assert.Equal(366, result);
    }

    [Fact]
    public void GetDaysInYearWithCulture_Returns365_ForNonLeapYear()
    {
        var year = 2023;
        var culture = new CultureInfo("en-US");
        var result = DateTimeExtensions.GetDays(year, culture);
        Assert.Equal(365, result);
    }

    [Fact]
    public void GetDaysInYearWithCulture_Returns366_ForLeapYear()
    {
        var year = 2024;
        var culture = new CultureInfo("en-US");
        var result = DateTimeExtensions.GetDays(year, culture);
        Assert.Equal(366, result);
    }

    [Fact]
    public void GetDaysBetweenDates_ReturnsCorrectNumberOfDays()
    {
        var fromDate = new DateTime(2023, 10, 1);
        var toDate = new DateTime(2023, 10, 5);
        var result = fromDate.GetDays(toDate);
        Assert.Equal(4, result);
    }

    [Fact]
    public void IsEaster_ReturnsTrue_ForEasterDate()
    {
        var date = new DateTime(2023, 4, 9);
        var result = date.IsEaster();
        Assert.True(result);
    }

    [Fact]
    public void IsEaster_ReturnsFalse_ForNonEasterDate()
    {
        var date = new DateTime(2023, 4, 10);
        var result = date.IsEaster();
        Assert.False(result);
    }

    [Fact]
    public void IsBefore_ReturnsTrue_WhenSourceIsBeforeOther()
    {
        var source = new DateTime(2023, 10, 5);
        var other = new DateTime(2023, 10, 6);
        var result = source.IsBefore(other);
        Assert.True(result);
    }

    [Fact]
    public void IsBefore_ReturnsFalse_WhenSourceIsNotBeforeOther()
    {
        var source = new DateTime(2023, 10, 6);
        var other = new DateTime(2023, 10, 5);
        var result = source.IsBefore(other);
        Assert.False(result);
    }

    [Fact]
    public void IsAfter_ReturnsTrue_WhenSourceIsAfterOther()
    {
        var source = new DateTime(2023, 10, 6);
        var other = new DateTime(2023, 10, 5);
        var result = source.IsAfter(other);
        Assert.True(result);
    }

    [Fact]
    public void IsAfter_ReturnsFalse_WhenSourceIsNotAfterOther()
    {
        var source = new DateTime(2023, 10, 5);
        var other = new DateTime(2023, 10, 6);
        var result = source.IsAfter(other);
        Assert.False(result);
    }

    [Fact]
    public void EndOfDay_ReturnsDateAtEndOfDay()
    {
        var date = new DateTime(2023, 10, 5);
        DateTime result = date.EndOfDay();
        Assert.Equal(new DateTime(2023, 10, 5, 23, 59, 59, 999), result);
    }
}