using System.Globalization;
using Linger.Extensions.Core;

namespace Linger.UnitTests.Extensions.Core;

public partial class DateTimeExtensionsTests
{
    [Fact]
    public void ToFormatDate_WithDefaultFormat_ReturnsFormattedDate()
    {
        // Arrange
        var date = new DateTime(2023, 4, 15);

        // Act
        var result = date.ToFormatDate();

        // Assert
        Assert.Equal("2023-04-15", result);
    }

    [Fact]
    public void ToFormatDate_WithCustomFormat_ReturnsFormattedDate()
    {
        // Arrange
        var date = new DateTime(2023, 4, 15);

        // Act
        var result = date.ToFormatDate("yyyy/MM/dd");

        // Assert
        Assert.Equal("2023/04/15", result);
    }

    [Fact]
    public void ToFormatDate_WithNullableDateTime_ReturnsFormattedDate()
    {
        // Arrange
        DateTime? date = new DateTime(2023, 4, 15);

        // Act
        var result = date.ToFormatDate();

        // Assert
        Assert.Equal("2023-04-15", result);
    }

    [Fact]
    public void ToFormatDate_WithNullValue_ReturnsNull()
    {
        // Arrange
        DateTime? date = null;

        // Act
        var result = date.ToFormatDate();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ToFormatDateTime_WithDefaultFormat_ReturnsFormattedDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25);

        // Act
        var result = dateTime.ToFormatDateTime();

        // Assert
        Assert.Equal("2023-04-15 14:30:25", result);
    }

    [Fact]
    public void ToFormatDateTime_WithCustomFormat_ReturnsFormattedDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25);

        // Act
        var result = dateTime.ToFormatDateTime("yyyy/MM/dd HH:mm");

        // Assert
        Assert.Equal("2023/04/15 14:30", result);
    }

    [Fact]
    public void ToFormatDateTime_WithNullableDateTime_ReturnsFormattedDateTime()
    {
        // Arrange
        DateTime? dateTime = new DateTime(2023, 4, 15, 14, 30, 25);

        // Act
        var result = dateTime.ToFormatDateTime();

        // Assert
        Assert.Equal("2023-04-15 14:30:25", result);
    }

    [Fact]
    public void ToFormatDateTime_WithNullValue_ReturnsNull()
    {
        // Arrange
        DateTime? dateTime = null;

        // Act
        var result = dateTime.ToFormatDateTime();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetDateDifference_ReturnsDifferenceInTimeSpan()
    {
        // Arrange
        var date1 = new DateTime(2023, 4, 15);
        var date2 = new DateTime(2023, 4, 10);

        // Act
        var result = date1.GetDateDifference(date2);

        // Assert
        Assert.Equal(TimeSpan.FromDays(5), result);
    }

    [Theory]
    [InlineData(TimeUnit.Days, 5)]
    [InlineData(TimeUnit.Hours, 120)]
    [InlineData(TimeUnit.Minutes, 7200)]
    [InlineData(TimeUnit.Seconds, 432000)]
    [InlineData(TimeUnit.Milliseconds, 432000000)]
    [InlineData(TimeUnit.Months, 0)]
    [InlineData(TimeUnit.Years, 0)]
    public void GetDateDifference_WithTimeUnit_ReturnsDifferenceInSpecifiedUnit(TimeUnit unit, double expected)
    {
        // Arrange
        var date1 = new DateTime(2023, 4, 15);
        var date2 = new DateTime(2023, 4, 10);

        // Act
        var result = date1.GetDateDifference(date2, unit);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("D", 5)]
    [InlineData("DAYS", 5)]
    [InlineData("H", 120)]
    [InlineData("HOURS", 120)]
    [InlineData("M", 7200)]
    [InlineData("MINUTES", 7200)]
    [InlineData("S", 432000)]
    [InlineData("SECONDS", 432000)]
    [InlineData("MS", 432000000)]
    [InlineData("MILLISECONDS", 432000000)]
    [InlineData("MO", 0)]
    [InlineData("MONTHS", 0)]
    [InlineData("Y", 0)]
    [InlineData("YEARS", 0)]
    [InlineData("INVALID", 5)] // 默认应该是Days
    public void GetDateDifference_WithStringUnit_ReturnsDifferenceInSpecifiedUnit(string unit, double expected)
    {
        // Arrange
        var date1 = new DateTime(2023, 4, 15);
        var date2 = new DateTime(2023, 4, 10);

        // Act
        var result = date1.GetDateDifference(date2, unit);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, "2023-04-15")]
    [InlineData(1, "2023-04-15 14:30:25")]
    [InlineData(2, "2023/04/15")]
    [InlineData(4, "04-15")]
    [InlineData(5, "04/15")]
    [InlineData(7, "2023-04")]
    [InlineData(8, "2023/04")]
    [InlineData(10, "2023-04-15 00:00:00")]
    [InlineData(11, "20230415")]
    [InlineData(12, "20230415143025")]
    [InlineData(13, "04/15/2023")]
    public void ToStringOfMode_ReturnsCorrectFormat(int mode, string expected)
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25);

        // Act
        var result = dateTime.ToStringOfMode(mode);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateAge_ReturnsCorrectAge()
    {
        // Arrange
        var birthDate = new DateTime(1990, 6, 15);
        var referenceDate = new DateTime(2023, 6, 15);

        // Act
        var result = birthDate.CalculateAge(referenceDate);

        // Assert
        Assert.Equal(33, result);
    }

    [Fact]
    public void CalculateAge_BeforeBirthdayInSameYear_ReturnsCorrectAge()
    {
        // Arrange
        var birthDate = new DateTime(1990, 6, 15);
        var referenceDate = new DateTime(2023, 6, 14);

        // Act
        var result = birthDate.CalculateAge(referenceDate);

        // Assert
        Assert.Equal(32, result);
    }

    [Fact]
    public void CalculateAge_WithNoReferenceDate_UsesCurrentDate()
    {
        // Arrange
        var now = DateTime.Now;
        var birthDate = new DateTime(now.Year - 30, now.Month, 15);
        var expectedAge = 30;

        // 如果今天是生日之前，预期年龄应该是29
        if (now.Month == birthDate.Month && now.Day < birthDate.Day)
            expectedAge = 29;
        else if (now.Month < birthDate.Month)
            expectedAge = 29;

        // Act
        var result = birthDate.CalculateAge();

        // Assert
        Assert.Equal(expectedAge, result);
    }

    [Fact]
    public void GetCountDaysOfMonth_ReturnsCorrectDayCount()
    {
        // Arrange
        var date1 = new DateTime(2023, 2, 15); // February 2023 (28 days)
        var date2 = new DateTime(2023, 4, 15); // April 2023 (30 days)
        var date3 = new DateTime(2023, 1, 15); // January 2023 (31 days)
        var date4 = new DateTime(2024, 2, 15); // February 2024 (29 days - leap year)

        // Act
        var result1 = date1.GetCountDaysOfMonth();
        var result2 = date2.GetCountDaysOfMonth();
        var result3 = date3.GetCountDaysOfMonth();
        var result4 = date4.GetCountDaysOfMonth();

        // Assert
        Assert.Equal(28, result1);
        Assert.Equal(30, result2);
        Assert.Equal(31, result3);
        Assert.Equal(29, result4);
    }

    [Fact]
    public void IsToday_ReturnsCorrectResult()
    {
        // Arrange
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        var tomorrow = today.AddDays(1);

        // Act
        var resultToday = today.IsToday();
        var resultYesterday = yesterday.IsToday();
        var resultTomorrow = tomorrow.IsToday();

        // Assert
        Assert.True(resultToday);
        Assert.False(resultYesterday);
        Assert.False(resultTomorrow);
    }

    [Fact]
    public void InRange_ReturnsCorrectResult()
    {
        // Arrange
        var date = new DateTime(2023, 4, 15);
        var minDate = new DateTime(2023, 4, 10);
        var maxDate = new DateTime(2023, 4, 20);

        // Act
        var inRange = date.InRange(minDate, maxDate);
        var belowRange = minDate.AddDays(-1).InRange(minDate, maxDate);
        var aboveRange = maxDate.AddDays(1).InRange(minDate, maxDate);

        // Assert
        Assert.True(inRange);
        Assert.False(belowRange);
        Assert.False(aboveRange);
    }

    [Fact]
    public void DayInYear_ReturnsCorrectDayOfYear()
    {
        // Arrange
        var date1 = new DateTime(2023, 1, 1);
        var date2 = new DateTime(2023, 4, 15);
        var date3 = new DateTime(2023, 12, 31);

        // Act
        var result1 = date1.DayInYear();
        var result2 = date2.DayInYear();
        var result3 = date3.DayInYear();

        // Assert
        Assert.Equal(1, result1);
        Assert.Equal(105, result2);
        Assert.Equal(365, result3);
    }

    [Theory]
    [InlineData(2023, 4, 10, 1)] // Monday
    [InlineData(2023, 4, 11, 2)] // Tuesday
    [InlineData(2023, 4, 12, 3)] // Wednesday
    [InlineData(2023, 4, 13, 4)] // Thursday
    [InlineData(2023, 4, 14, 5)] // Friday
    [InlineData(2023, 4, 15, 6)] // Saturday
    [InlineData(2023, 4, 16, 7)] // Sunday
    public void DayInWeek_ReturnsCorrectDayOfWeek(int year, int month, int day, int expected)
    {
        // Arrange
        var date = new DateTime(year, month, day);

        // Act
        var result = date.DayInWeek();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsDateEqual_ReturnsCorrectResult()
    {
        // Arrange
        var date1 = new DateTime(2023, 4, 15, 14, 30, 0);
        var date2 = new DateTime(2023, 4, 15, 9, 45, 0);
        var date3 = new DateTime(2023, 4, 16, 14, 30, 0);

        // Act
        var result1 = date1.IsDateEqual(date2);
        var result2 = date1.IsDateEqual(date3);

        // Assert
        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    public void IsTimeEqual_ReturnsCorrectResult()
    {
        // Arrange
        var date1 = new DateTime(2023, 4, 15, 14, 30, 0);
        var date2 = new DateTime(2023, 4, 16, 14, 30, 0);
        var date3 = new DateTime(2023, 4, 15, 9, 45, 0);

        // Act
        var result1 = date1.IsTimeEqual(date2);
        var result2 = date1.IsTimeEqual(date3);

        // Assert
        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    public void AddWeeks_AddsCorrectNumberOfWeeks()
    {
        // Arrange
        var date = new DateTime(2023, 4, 15);

        // Act
        var result1 = date.AddWeeks(1);
        var result2 = date.AddWeeks(2);
        var result3 = date.AddWeeks(-1);

        // Assert
        Assert.Equal(new DateTime(2023, 4, 22), result1);
        Assert.Equal(new DateTime(2023, 4, 29), result2);
        Assert.Equal(new DateTime(2023, 4, 8), result3);
    }

    [Fact]
    public void GetDays_ReturnsCorrectNumberOfDaysInYear()
    {
        // Act
        var result1 = DateTimeExtensions.GetDays(2023); // Regular year
        var result2 = DateTimeExtensions.GetDays(2024); // Leap year

        // Assert
        Assert.Equal(365, result1);
        Assert.Equal(366, result2);
    }

    [Fact]
    public void GetDays_WithSpecificCulture_ReturnsCorrectDaysInYear()
    {
        // Arrange
        var japaneseCulture = new CultureInfo("ja-JP");
        japaneseCulture.DateTimeFormat.Calendar = new JapaneseCalendar();

        // Act
        var result = DateTimeExtensions.GetDays(2023, japaneseCulture);

        // Assert
        Assert.Equal(365, result); // 2023年在任何日历中都有365天
    }

    [Fact]
    public void GetDays_WithDateInstance_ReturnsCorrectNumberOfDaysInYear()
    {
        // Arrange
        var date1 = new DateTime(2023, 4, 15);
        var date2 = new DateTime(2024, 4, 15);

        // Act
        var result1 = date1.GetDays();
        var result2 = date2.GetDays();

        // Assert
        Assert.Equal(365, result1);
        Assert.Equal(366, result2);
    }

    [Fact]
    public void GetDays_WithDateRange_ReturnsCorrectNumberOfDays()
    {
        // Arrange
        var startDate = new DateTime(2023, 4, 10);
        var endDate = new DateTime(2023, 4, 20);

        // Act
        var result = startDate.GetDays(endDate);

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void IsEaster_ReturnsCorrectResult()
    {
        // Arrange
        var easter2023 = new DateTime(2023, 4, 9); // Easter 2023
        var notEaster = new DateTime(2023, 4, 15);

        // Act
        var result1 = easter2023.IsEaster();
        var result2 = notEaster.IsEaster();

        // Assert
        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    public void IsBefore_ReturnsCorrectResult()
    {
        // Arrange
        var earlier = new DateTime(2023, 4, 10);
        var later = new DateTime(2023, 4, 15);

        // Act
        var result1 = earlier.IsBefore(later);
        var result2 = later.IsBefore(earlier);
        var result3 = earlier.IsBefore(earlier);

        // Assert
        Assert.True(result1);
        Assert.False(result2);
        Assert.False(result3);
    }

    [Fact]
    public void IsAfter_ReturnsCorrectResult()
    {
        // Arrange
        var earlier = new DateTime(2023, 4, 10);
        var later = new DateTime(2023, 4, 15);

        // Act
        var result1 = later.IsAfter(earlier);
        var result2 = earlier.IsAfter(later);
        var result3 = earlier.IsAfter(earlier);

        // Assert
        Assert.True(result1);
        Assert.False(result2);
        Assert.False(result3);
    }

    [Fact]
    public void StartOfDay_ReturnsCorrectDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25);

        // Act
        var result = dateTime.StartOfDay();

        // Assert
        Assert.Equal(new DateTime(2023, 4, 15, 0, 0, 0), result);
    }

    [Fact]
    public void EndOfDay_ReturnsCorrectDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25);

        // Act
        var result = dateTime.EndOfDay();

        // Assert
        Assert.Equal(new DateTime(2023, 4, 15, 23, 59, 59, 999), result);
    }

    [Fact]
    public void StartOfMonth_ReturnsCorrectDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15);

        // Act
        var result = dateTime.StartOfMonth();

        // Assert
        Assert.Equal(new DateTime(2023, 4, 1), result);
    }

    [Fact]
    public void EndOfMonth_ReturnsCorrectDateTime()
    {
        // Arrange
        var dateTime1 = new DateTime(2023, 4, 15); // 30-day month
        var dateTime2 = new DateTime(2023, 2, 15); // 28-day month
        var dateTime3 = new DateTime(2024, 2, 15); // 29-day month (leap year)

        // Act
        var result1 = dateTime1.EndOfMonth();
        var result2 = dateTime2.EndOfMonth();
        var result3 = dateTime3.EndOfMonth();

        // Assert
        Assert.Equal(new DateTime(2023, 4, 30, 23, 59, 59, 999), result1);
        Assert.Equal(new DateTime(2023, 2, 28, 23, 59, 59, 999), result2);
        Assert.Equal(new DateTime(2024, 2, 29, 23, 59, 59, 999), result3);
    }

    [Fact]
    public void FirstDayOfMonth_WithNoDayOfWeek_ReturnsFirstDay()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15);

        // Act
        var result = dateTime.FirstDayOfMonth();

        // Assert
        Assert.Equal(new DateTime(2023, 4, 1), result);
    }

    [Fact]
    public void LastDayOfMonth_WithNoDayOfWeek_ReturnsLastDay()
    {
        // Arrange
        var dateTime1 = new DateTime(2023, 4, 15); // 30-day month
        var dateTime2 = new DateTime(2023, 2, 15); // 28-day month

        // Act
        var result1 = dateTime1.LastDayOfMonth();
        var result2 = dateTime2.LastDayOfMonth();

        // Assert
        Assert.Equal(new DateTime(2023, 4, 30), result1);
        Assert.Equal(new DateTime(2023, 2, 28), result2);
    }

    [Fact]
    public void EndOfYear_ReturnsCorrectDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15);

        // Act
        var result = dateTime.EndOfYear();

        // Assert
        Assert.Equal(new DateTime(2023, 12, 31, 23, 59, 59, 999), result);
    }

    [Fact]
    public void ToDateTimeOfMode_WithDefaultMode_ReturnsSameDateTime()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25);

        // Act
        var result = dateTime.ToDateTimeOfMode();

        // Assert
        Assert.Equal(dateTime, result);
    }

    [Fact]
    public void ToDateTimeOfMode_WithZeroMode_ReturnsStartOfDay()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25);

        // Act
        var result = dateTime.ToDateTimeOfMode(TimeMode.Zero);

        // Assert
        Assert.Equal(new DateTime(2023, 4, 15, 0, 0, 0), result);
    }

    [Fact]
    public void ToDateTimeOfMode_WithFullMode_ReturnsEndOfDay()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25);

        // Act
        var result = dateTime.ToDateTimeOfMode(TimeMode.Full);

        // Assert
        Assert.Equal(new DateTime(2023, 4, 15, 23, 59, 59, 999), result);
    }

    [Theory]
    [InlineData(2023, false)] // Regular year
    [InlineData(2024, true)]  // Leap year
    [InlineData(2000, true)]  // Leap year (divisible by 400)
    [InlineData(1900, false)] // Not a leap year (divisible by 100 but not by 400)
    public void IsLeapYear_ReturnsCorrectResult(int year, bool expected)
    {
        // Arrange
        var date = new DateTime(year, 1, 1);

        // Act
        var result = date.IsLeapYear();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SetDateTime_SetsAllComponentsCorrectly()
    {
        // Arrange
        var original = new DateTime(2023, 4, 15, 14, 30, 25, 500);

        // Act
        var result1 = original.SetDateTime(year: 2024);
        var result2 = original.SetDateTime(month: 6);
        var result3 = original.SetDateTime(day: 20);
        var result4 = original.SetDateTime(hour: 9);
        var result5 = original.SetDateTime(minute: 45);
        var result6 = original.SetDateTime(second: 30);
        var result7 = original.SetDateTime(millisecond: 750);
        var result8 = original.SetDateTime(2024, 6, 20, 9, 45, 30, 750);

        // Assert
        Assert.Equal(new DateTime(2024, 4, 15, 14, 30, 25, 500), result1);
        Assert.Equal(new DateTime(2023, 6, 15, 14, 30, 25, 500), result2);
        Assert.Equal(new DateTime(2023, 4, 20, 14, 30, 25, 500), result3);
        Assert.Equal(new DateTime(2023, 4, 15, 9, 30, 25, 500), result4);
        Assert.Equal(new DateTime(2023, 4, 15, 14, 45, 25, 500), result5);
        Assert.Equal(new DateTime(2023, 4, 15, 14, 30, 30, 500), result6);
        Assert.Equal(new DateTime(2023, 4, 15, 14, 30, 25, 750), result7);
        Assert.Equal(new DateTime(2024, 6, 20, 9, 45, 30, 750), result8);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void ToDateOnly_ConvertsDateTimeToDateOnly()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25);

        // Act
        var result = dateTime.ToDateOnly();

        // Assert
        Assert.Equal(new DateOnly(2023, 4, 15), result);
    }

    [Fact]
    public void ToDateOnly_WithNullableDateTime_ConvertsToNullableDateOnly()
    {
        // Arrange
        DateTime? dateTime1 = new DateTime(2023, 4, 15, 14, 30, 25);
        DateTime? dateTime2 = null;

        // Act
        var result1 = dateTime1.ToDateOnly();
        var result2 = dateTime2.ToDateOnly();

        // Assert
        Assert.Equal(new DateOnly(2023, 4, 15), result1);
        Assert.Null(result2);
    }

    [Fact]
    public void ToDateTime_FromDateOnly_ConvertsToDateTime()
    {
        // Arrange
        var dateOnly = new DateOnly(2023, 4, 15);

        // Act
        var result = dateOnly.ToDateTime();

        // Assert
        Assert.Equal(new DateTime(2023, 4, 15, 0, 0, 0), result);
    }

    [Fact]
    public void ToDateTime_FromNullableDateOnly_ConvertsToNullableDateTime()
    {
        // Arrange
        DateOnly? dateOnly1 = new DateOnly(2023, 4, 15);
        DateOnly? dateOnly2 = null;

        // Act
        var result1 = dateOnly1.ToDateTime();
        var result2 = dateOnly2.ToDateTime();

        // Assert
        Assert.Equal(new DateTime(2023, 4, 15, 0, 0, 0), result1);
        Assert.Null(result2);
    }

    [Fact]
    public void ToDateTime_FromDateOnlyWithTimeOnly_ConvertsToDateTime()
    {
        // Arrange
        var dateOnly = new DateOnly(2023, 4, 15);
        var timeOnly = new TimeOnly(14, 30, 25);

        // Act
        var result = dateOnly.ToDateTime(timeOnly);

        // Assert
        Assert.Equal(new DateTime(2023, 4, 15, 14, 30, 25), result);
    }

    [Fact]
    public void ToDateTime_FromNullableDateOnlyWithTimeOnly_ConvertsToNullableDateTime()
    {
        // Arrange
        DateOnly? dateOnly1 = new DateOnly(2023, 4, 15);
        DateOnly? dateOnly2 = null;
        var timeOnly = new TimeOnly(14, 30, 25);

        // Act
        var result1 = dateOnly1.ToDateTime(timeOnly);
        var result2 = dateOnly2.ToDateTime(timeOnly);

        // Assert
        Assert.Equal(new DateTime(2023, 4, 15, 14, 30, 25), result1);
        Assert.Null(result2);
    }

    [Fact]
    public void ToTimeOnly_ConvertsDateTimeToTimeOnly()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25);

        // Act
        var result = dateTime.ToTimeOnly();

        // Assert
        Assert.Equal(new TimeOnly(14, 30, 25), result);
    }

    [Fact]
    public void ToTimeOnly_WithNullableDateTime_ConvertsToNullableTimeOnly()
    {
        // Arrange
        DateTime? dateTime1 = new DateTime(2023, 4, 15, 14, 30, 25);
        DateTime? dateTime2 = null;

        // Act
        var result1 = dateTime1.ToTimeOnly();
        var result2 = dateTime2.ToTimeOnly();

        // Assert
        Assert.Equal(new TimeOnly(14, 30, 25), result1);
        Assert.Null(result2);
    }
#endif

    [Fact]
    public void ToDateTimeOffset_ConvertsDateTimeToDateTimeOffset()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25, DateTimeKind.Utc);

        // Act
        var result = dateTime.ToDateTimeOffset();

        // Assert
        Assert.Equal(new DateTimeOffset(2023, 4, 15, 14, 30, 25, TimeSpan.Zero), result);
    }

    [Fact]
    public void ToDateTimeOffset_UnspecifiedKind_ConvertsToLocalDateTimeOffset()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25, DateTimeKind.Unspecified);

        // Act
        var result = dateTime.ToDateTimeOffset();

        // Assert
        // DateTimeOffset does not preserve DateTime.Kind information
        // Instead, we check that the offset matches the local timezone offset
        Assert.Equal(TimeZoneInfo.Local.GetUtcOffset(dateTime), result.Offset);
    }

    [Fact]
    public void ToDateTimeOffset_WithNullableDateTime_ConvertsToNullableDateTimeOffset()
    {
        // Arrange
        DateTime? dateTime1 = new DateTime(2023, 4, 15, 14, 30, 25, DateTimeKind.Utc);
        DateTime? dateTime2 = null;

        // Act
        var result1 = dateTime1.ToDateTimeOffset();
        var result2 = dateTime2.ToDateTimeOffset();

        // Assert
        Assert.Equal(new DateTimeOffset(2023, 4, 15, 14, 30, 25, TimeSpan.Zero), result1);
        Assert.Null(result2);
    }

    [Fact]
    public void GetWeekCountOfYear_ReturnsCorrectWeekCount()
    {
        // Act
        var result2023 = DateTimeExtensions.GetWeekCountOfYear(2023);  // Regular year

        // Assert
        // The number of weeks can vary based on the culture's calendar, but typically:
        Assert.InRange(result2023, 52, 53);
    }

    [Fact]
    public void GetWeekCountOfYear_InvalidYear_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.GetWeekCountOfYear(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.GetWeekCountOfYear(10000));
    }

    [Fact]
    public void WeekNumberOfYear_ReturnsCorrectWeekNumber()
    {
        // Arrange
        var date = new DateTime(2023, 1, 1);  // This is typically week 52 or 1 depending on culture

        // Act
        var result = date.WeekNumberOfYear();

        // Assert
        // The result depends on the culture's calendar rules
        Assert.InRange(result, 1, 53);
    }

    [Fact]
    public void WeekNumberOfYear_WithSpecificCulture_ReturnsCorrectWeekNumber()
    {
        // Arrange
        var dateTime = new DateTime(2023, 12, 31); // 年末
        var usCulture = new CultureInfo("en-US");

        // Act
        var result = dateTime.WeekNumberOfYear(usCulture);

        // Assert
        // 美国文化中，12月31日通常是第53周或第52周
        Assert.InRange(result, 52, 53);
    }

    [Fact]
    public void GetStartEndDayOfWeek_ReturnsCorrectDateRange()
    {
        // Act
        var result = DateTimeExtensions.GetStartEndDayOfWeek(2023, 1); // First week of 2023

        // Assert
#if NET40
        Assert.NotNull(result.Item1);
        Assert.NotNull(result.Item2);
        Assert.True(result.Item1 <= result.Item2);
        Assert.Equal(7, Math.Ceiling((result.Item2 - result.Item1).TotalDays));
#else
        Assert.NotNull(result.Start);
        Assert.NotNull(result.End);
        Assert.True(result.Start <= result.End);
        Assert.Equal(7, Math.Ceiling((result.End - result.Start).TotalDays));
#endif
    }

    [Fact]
    public void GetStartEndDayOfWeek_InvalidYear_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.GetStartEndDayOfWeek(0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.GetStartEndDayOfWeek(10000, 1));
    }

    [Fact]
    public void GetStartEndDayOfWeek_InvalidWeekNumber_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.GetStartEndDayOfWeek(2023, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => DateTimeExtensions.GetStartEndDayOfWeek(2023, 54));
    }

    [Fact]
    public void GetStartEndDayOfFirstWeek_ReturnsCorrectDateRange()
    {
        // Arrange & Act
        var result = DateTimeExtensions.GetStartEndDayOfFirstWeek(2023);

        // Assert
#if NET40
        Assert.NotNull(result.Item1);
        Assert.NotNull(result.Item2);
        Assert.True(result.Item1.Year == 2023);
        Assert.True(result.Item2 > result.Item1);
#else
        Assert.NotNull(result.Start);
        Assert.NotNull(result.End);
        Assert.True(result.Start.Year == 2023);
        Assert.True(result.End > result.Start);
#endif
    }

    [Fact]
    public void GetStartEndDayOfFirstWeek_WithExplicitParameters_ReturnsCorrectDateRange()
    {
        // Arrange & Act
        // 使用 CalendarWeekRule.FirstFullWeek 规则，这样可以确保返回的周一定是从指定的星期几开始
        var result = DateTimeExtensions.GetStartEndDayOfFirstWeek(2023, DayOfWeek.Monday, CalendarWeekRule.FirstFullWeek);

        // Assert
#if NET40
        Assert.NotNull(result.Item1);
        Assert.NotNull(result.Item2);
        Assert.True(result.Item1.Year == 2023);
        Assert.True(result.Item2 > result.Item1);
        // 确保第一周开始日期是周一
        Assert.Equal(DayOfWeek.Monday, result.Item1.DayOfWeek);
#else
        Assert.NotNull(result.Start);
        Assert.NotNull(result.End);
        Assert.True(result.Start.Year == 2023);
        Assert.True(result.End > result.Start);
        // 确保第一周开始日期是周一
        Assert.Equal(DayOfWeek.Monday, result.Start.DayOfWeek);
#endif
    }

    [Fact]
    public void GetStartEndDayOfFirstWeek_WithFirstDayRuleShouldNotEnforceStartDayOfWeek()
    {
        // Arrange & Act
        // 使用 CalendarWeekRule.FirstDay 规则时，返回的开始日期将是年份的第一天
        // 2023年1月1日是周日，不是周一
        var result = DateTimeExtensions.GetStartEndDayOfFirstWeek(2023, DayOfWeek.Monday, CalendarWeekRule.FirstDay);

        // Assert
#if NET40
        Assert.NotNull(result.Item1);
        Assert.NotNull(result.Item2);
        Assert.True(result.Item1.Year == 2023);
        Assert.True(result.Item2 > result.Item1);
        // 2023年1月1日是周日，而不是周一
        Assert.Equal(new DateTime(2023, 1, 1), result.Item1);
        Assert.Equal(DayOfWeek.Sunday, result.Item1.DayOfWeek);
#else
        Assert.NotNull(result.Start);
        Assert.NotNull(result.End);
        Assert.True(result.Start.Year == 2023);
        Assert.True(result.End > result.Start);
        // 2023年1月1日是周日，而不是周一
        Assert.Equal(new DateTime(2023, 1, 1), result.Start);
        Assert.Equal(DayOfWeek.Sunday, result.Start.DayOfWeek);
#endif
    }

    [Fact]
    public void ToStringOfMode_WithInvalidMode_ReturnsDefaultFormat()
    {
        // Arrange
        var dateTime = new DateTime(2023, 4, 15, 14, 30, 25);
        var invalidMode = 999;

        // Act
        var result = dateTime.ToStringOfMode(invalidMode);

        // Assert
        // 使用默认文化格式化
        Assert.Equal(dateTime.ToString(ExtensionMethodSetting.DefaultCulture), result);
    }

    [Fact]
    public void GetDateDifference_WithNullSecondDate_UsesCurrentDate()
    {
        // Arrange
        var now = DateTime.Now;
        var date1 = now.AddDays(-5);
        var expectedDiff = -5.0; // 约-5天，因为date1早于now
        var tolerance = 0.01; // 允许0.01天的误差（约15分钟）

        // Act
        var result = date1.GetDateDifference(null, TimeUnit.Days);

        // Assert
        Assert.InRange(result, expectedDiff - tolerance, expectedDiff + tolerance);
    }

    [Fact]
    public void GetDateDifference_WithNegativeDifference_ReturnsNegativeValue()
    {
        // Arrange
        var date1 = new DateTime(2023, 4, 10);
        var date2 = new DateTime(2023, 4, 15);

        // Act
        var result = date1.GetDateDifference(date2, TimeUnit.Days);

        // Assert
        Assert.Equal(-5, result); // date1早于date2，应返回负值
    }

    [Fact]
    public void GetDateDifference_WithAbsTrue_ReturnsAbsoluteValue()
    {
        // Arrange
        var date1 = new DateTime(2023, 4, 10);
        var date2 = new DateTime(2023, 4, 15);

        // Act
        // date1早于date2，正常应返回负值
        var result = date1.GetDateDifference(date2, TimeUnit.Days, true);

        // Assert
        Assert.Equal(5, result); // 使用abs=true后应返回正值
    }
}