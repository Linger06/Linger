// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Linger.Extensions.Core;

public static class DateTimeExtensions
{
    public static string ToFormatDate(this DateTime dateTime, string format = "yyyy-MM-dd") => dateTime.ToString(format, ExtensionMethodSetting.DefaultCulture);

    public static string? ToFormatDate(this DateTime? dateTime, string format = "yyyy-MM-dd") => dateTime?.ToString(format, ExtensionMethodSetting.DefaultCulture);

    public static string ToFormatDateTime(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss") => dateTime.ToString(format, ExtensionMethodSetting.DefaultCulture);

    public static string? ToFormatDateTime(this DateTime? dateTime, string format = "yyyy-MM-dd HH:mm:ss") => dateTime?.ToString(format, ExtensionMethodSetting.DefaultCulture);

    public static TimeSpan GetDateDifference(this DateTime dateTime1, DateTime dateTime2) => dateTime1 - dateTime2;

    public static double GetDateDifference(this DateTime dateTime1, DateTime? dateTime2, TimeUnit unit, bool abs = false)
    {
        var date2 = dateTime2 ?? DateTime.Now;
        var dateDiff = unit switch
        {
            TimeUnit.Days => (dateTime1 - date2).TotalDays,
            TimeUnit.Hours => (dateTime1 - date2).TotalHours,
            TimeUnit.Minutes => (dateTime1 - date2).TotalMinutes,
            TimeUnit.Seconds => (dateTime1 - date2).TotalSeconds,
            TimeUnit.Milliseconds => (dateTime1 - date2).TotalMilliseconds,
            TimeUnit.Months => (dateTime1.Year - date2.Year) * 12 + dateTime1.Month - date2.Month +
                             (dateTime1.Day >= date2.Day ? 0 : -1),
            TimeUnit.Years => dateTime1.Year - date2.Year +
                            ((dateTime1.Month > date2.Month ||
                             (dateTime1.Month == date2.Month && dateTime1.Day >= date2.Day)) ? 0 : -1),
            _ => 0
        };
        return abs ? Math.Abs(dateDiff) : dateDiff;
    }

    [Obsolete("Use GetDateDifference with TimeUnit instead.")]
    public static double GetDateDifference(this DateTime dateTime1, DateTime? dateTime2, string unit, bool abs = false)
    {
        var timeUnit = unit?.ToUpperInvariant() switch
        {
            "D" or "DAYS" => TimeUnit.Days,
            "H" or "HOURS" => TimeUnit.Hours,
            "M" or "MINUTES" => TimeUnit.Minutes,
            "MO" or "MONTHS" => TimeUnit.Months,
            "S" or "SECONDS" => TimeUnit.Seconds,
            "MS" or "MILLISECONDS" => TimeUnit.Milliseconds,
            "Y" or "YEARS" => TimeUnit.Years,
            _ => TimeUnit.Days
        };
        return GetDateDifference(dateTime1, dateTime2, timeUnit, abs);
    }

    /// <summary>
    /// Converts the DateTime to a string using the specified date mode.
    /// </summary>
    /// <param name="dateTime">The DateTime instance to format.</param>
    /// <param name="dateMode">The date mode to use.</param>
    /// <returns>A formatted string representation of the DateTime.</returns>
    /// <remarks>
    /// This method is obsolete. Use ToFormatDate() or ToFormatDateTime() methods instead, or use DateTime.ToString(format) directly.
    /// </remarks>
    [Obsolete("Use ToFormatDate() or ToFormatDateTime() methods instead, or use DateTime.ToString(format) directly. This method will be removed in a future version.")]
    public static string ToStringOfMode(this DateTime dateTime, int dateMode)
    {
        return dateMode switch
        {
            0 => dateTime.ToString("yyyy-MM-dd", ExtensionMethodSetting.DefaultCulture),
            1 => dateTime.ToString("yyyy-MM-dd HH:mm:ss", ExtensionMethodSetting.DefaultCulture),
            2 => dateTime.ToString("yyyy/MM/dd", ExtensionMethodSetting.DefaultCulture),
            4 => dateTime.ToString("MM-dd", ExtensionMethodSetting.DefaultCulture),
            5 => dateTime.ToString("MM/dd", ExtensionMethodSetting.DefaultCulture),
            7 => dateTime.ToString("yyyy-MM", ExtensionMethodSetting.DefaultCulture),
            8 => dateTime.ToString("yyyy/MM", ExtensionMethodSetting.DefaultCulture),
            10 => dateTime.ToString("yyyy-MM-dd", ExtensionMethodSetting.DefaultCulture) + " 00:00:00",
            11 => dateTime.ToString("yyyyMMdd", ExtensionMethodSetting.DefaultCulture),
            12 => dateTime.ToString("yyyyMMddHHmmss", ExtensionMethodSetting.DefaultCulture),
            13 => dateTime.ToString("MM/dd/yyyy", ExtensionMethodSetting.DefaultCulture),
            _ => dateTime.ToString(ExtensionMethodSetting.DefaultCulture)
        };
    }

    public static int CalculateAge(this DateTime dateOfBirth)
    {
        return dateOfBirth.CalculateAge(DateTime.Now);
    }

    public static int CalculateAge(this DateTime dateOfBirth, DateTime referenceDate)
    {
        var years = referenceDate.Year - dateOfBirth.Year;
        if (referenceDate.Month < dateOfBirth.Month ||
            (referenceDate.Month == dateOfBirth.Month && referenceDate.Day < dateOfBirth.Day))
        {
            --years;
        }

        return years;
    }

    public static int GetCountDaysOfMonth(this DateTime date)
    {
        DateTime nextMonth = date.AddMonths(1);
        return new DateTime(nextMonth.Year, nextMonth.Month, 1).AddDays(-1).Day;
    }

    public static bool IsToday(this DateTime dt)
    {
        return dt.Date == DateTime.Today;
    }

    public static bool InRange(this DateTime @this, DateTime minValue, DateTime maxValue)
    {
        return @this >= minValue && @this <= maxValue;
    }

    public static int DayInYear(this DateTime dateSrc)
    {
        return dateSrc.DayOfYear;
    }

    public static int DayInWeek(this DateTime dateSrc)
    {
        return dateSrc.DayOfWeek switch
        {
            DayOfWeek.Sunday => 7,
            _ => (int)dateSrc.DayOfWeek
        };
    }

    public static bool IsDateEqual(this DateTime date, DateTime dateToCompare)
    {
        return date.Date == dateToCompare.Date;
    }

    public static bool IsTimeEqual(this DateTime time, DateTime timeToCompare)
    {
        return time.TimeOfDay == timeToCompare.TimeOfDay;
    }

    public static DateTime AddWeeks(this DateTime date, int value)
    {
        return date.AddDays(value * 7);
    }

    public static int GetDays(int year)
    {
        return GetDays(year, ExtensionMethodSetting.DefaultCulture);
    }

    public static int GetDays(int year, CultureInfo culture)
    {
        var first = new DateTime(year, 1, 1, culture.Calendar);
        var last = new DateTime(year + 1, 1, 1, culture.Calendar);
        return first.GetDays(last);
    }

    public static int GetDays(this DateTime date)
    {
        return GetDays(date.Year, ExtensionMethodSetting.DefaultCulture);
    }

    public static int GetDays(this DateTime date, CultureInfo culture)
    {
        return GetDays(date.Year, culture);
    }

    public static int GetDays(this DateTime fromDate, DateTime toDate)
    {
        return Convert.ToInt32(toDate.Subtract(fromDate).TotalDays);
    }

    public static bool IsEaster(this DateTime date)
    {
        var y = date.Year;
        var a = y % 19;
        var b = y / 100;
        var c = y % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = ((19 * a) + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + (2 * e) + (2 * i) - h - k) % 7;
        var m = (a + (11 * h) + (22 * l)) / 451;
        var month = (h + l - (7 * m) + 114) / 31;
        var day = ((h + l - (7 * m) + 114) % 31) + 1;

        var dtEasterSunday = new DateTime(y, month, day);

        return date == dtEasterSunday;
    }

    public static bool IsBefore(this DateTime source, DateTime other)
    {
        return source.CompareTo(other) < 0;
    }

    public static bool IsAfter(this DateTime source, DateTime other)
    {
        return source.CompareTo(other) > 0;
    }

    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
    }

    public static DateTime EndOfDay(this DateTime date)
    {
        return date.SetDateTime(hour: 23, minute: 59, second: 59, millisecond: 999);
    }

    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1).AddMonths(1).AddMilliseconds(-1);
    }

    public static DateTime FirstDayOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    public static DateTime LastDayOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.GetCountDaysOfMonth());
    }

    public static DateTime EndOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1).AddYears(1).AddMilliseconds(-1);
    }

    public static DateTime ToDateTimeOfMode(this DateTime dateTime, TimeMode timeMode = TimeMode.Now)
    {
        return timeMode switch
        {
            TimeMode.Zero => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0),
            TimeMode.Full => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999),
            _ => dateTime
        };
    }

    public static bool IsLeapYear(this DateTime date)
    {
        if (date.Year % 4 != 0)
        {
            return false;
        }

        if (date.Year % 100 == 0)
        {
            return date.Year % 400 == 0;
        }

        return true;
    }

    public static DateTime SetDateTime(this DateTime current,
        int? year = null,
        int? month = null,
        int? day = null,
        int? hour = null,
        int? minute = null,
        int? second = null,
        int? millisecond = null)
    {
        return new DateTime(
            year ?? current.Year,
            month ?? current.Month,
            day ?? current.Day,
            hour ?? current.Hour,
            minute ?? current.Minute,
            second ?? current.Second,
            millisecond ?? current.Millisecond);
    }

#if NET6_0_OR_GREATER

    /// <summary>
    /// Converts the DateTime instance to a DateOnly instance.
    /// </summary>
    /// <param name="datetime">The DateTime instance.</param>
    /// <returns>A DateOnly instance.</returns>
    public static DateOnly ToDateOnly(this DateTime datetime)
    {
        return DateOnly.FromDateTime(datetime);
    }

    /// <summary>
    /// Converts the nullable DateTime instance to a nullable DateOnly instance.
    /// </summary>
    /// <param name="datetime">The nullable DateTime instance.</param>
    /// <returns>A nullable DateOnly instance.</returns>
    public static DateOnly? ToDateOnly(this DateTime? datetime)
    {
        return datetime.HasValue ? DateOnly.FromDateTime(datetime.Value) : null;
    }

    /// <summary>
    /// Converts the nullable DateOnly instance and TimeOnly instance to a nullable DateTime instance.
    /// </summary>
    /// <param name="dateOnly">The nullable DateOnly instance.</param>
    /// <param name="timeOnly">The TimeOnly instance.</param>
    /// <returns>A nullable DateTime instance.</returns>
    /// <example>
    /// <code>
    /// DateOnly? dateOnly = new DateOnly(2023, 10, 5);
    /// TimeOnly timeOnly = new TimeOnly(14, 30);
    /// DateTime? dateTime = dateOnly.ToDateTime(timeOnly);
    /// // dateTime is 2023-10-05 14:30:00
    /// </code>
    /// </example>
    public static DateTime? ToDateTime(this DateOnly? dateOnly, TimeOnly timeOnly)
    {
        return dateOnly?.ToDateTime(timeOnly);
    }

    /// <summary>
    /// Converts the nullable DateOnly instance to a nullable DateTime instance with the time set to midnight.
    /// </summary>
    /// <param name="dateOnly">The nullable DateOnly instance.</param>
    /// <returns>A nullable DateTime instance.</returns>
    public static DateTime? ToDateTime(this DateOnly? dateOnly)
    {
        return dateOnly?.ToDateTime(TimeOnly.MinValue);
    }

    /// <summary>
    /// Converts the DateOnly instance to a DateTime instance with the time set to midnight.
    /// </summary>
    /// <param name="dateOnly">The DateOnly instance.</param>
    /// <returns>A DateTime instance.</returns>
    public static DateTime ToDateTime(this DateOnly dateOnly)
    {
        return dateOnly.ToDateTime(TimeOnly.MinValue);
    }

    /// <summary>
    /// Converts the DateTime instance to a TimeOnly instance.
    /// </summary>
    /// <param name="datetime">The DateTime instance.</param>
    /// <returns>A TimeOnly instance.</returns>
    public static TimeOnly ToTimeOnly(this DateTime datetime)
    {
        return TimeOnly.FromDateTime(datetime);
    }

    /// <summary>
    /// Converts the nullable DateTime instance to a nullable TimeOnly instance.
    /// </summary>
    /// <param name="datetime">The nullable DateTime instance.</param>
    /// <returns>A nullable TimeOnly instance.</returns>
    public static TimeOnly? ToTimeOnly(this DateTime? datetime)
    {
        return datetime.HasValue ? TimeOnly.FromDateTime(datetime.Value) : null;
    }

#endif

    public static DateTimeOffset ToDateTimeOffset(this DateTime value)
    {
        if (value.Kind == DateTimeKind.Unspecified)
        {
            var localDateTime = DateTime.SpecifyKind(value, DateTimeKind.Local);
            return new DateTimeOffset(localDateTime);
        }
        return new DateTimeOffset(value);
    }

    [return: NotNullIfNotNull(nameof(input))]
    public static DateTimeOffset? ToDateTimeOffset(this DateTime? input)
    {
        if (input == null)
        {
            return null;
        }

        var value = input.Value;
        if (value.Kind == DateTimeKind.Unspecified)
        {
            var localDateTime = DateTime.SpecifyKind(value, DateTimeKind.Local);
            return new DateTimeOffset(localDateTime);
        }
        return new DateTimeOffset(value);
    }

    /// <summary>
    /// Gets the number of weeks in the specified year according to the specified culture's calendar.
    /// </summary>
    /// <param name="year">The year to get week count for (1-9999)</param>
    /// <param name="culture">The culture info to use for calculation. If null, uses default culture</param>
    /// <returns>The total number of weeks in the specified year</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when year is not between 1 and 9999</exception>
    public static int GetWeekCountOfYear(int year, CultureInfo? culture = null)
    {
        if (year < 1 || year > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1 and 9999.");
        }

        culture ??= ExtensionMethodSetting.DefaultCulture;

        var calendar = culture.Calendar;
        var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
        var calendarWeekRule = culture.DateTimeFormat.CalendarWeekRule;

        // Get the last day of the year
        var lastDayOfYear = new DateTime(year, 12, 31);
        var weekOfLastDay = calendar.GetWeekOfYear(lastDayOfYear, calendarWeekRule, firstDayOfWeek);

        return weekOfLastDay;
    }

    /// <summary>
    /// Gets the week number of the year for the specified date.
    /// </summary>
    /// <param name="dateSrc">The date to get week number for</param>
    /// <param name="culture">The culture info to use for calculation. If null, uses default culture</param>
    /// <returns>Week number of the year for the specified date</returns>

    public static int WeekNumberOfYear(this DateTime dateSrc, CultureInfo? culture = null)
    {
        culture ??= ExtensionMethodSetting.DefaultCulture;
        Calendar calendar = culture.Calendar;
        var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
        var calendarWeekRule = culture.DateTimeFormat.CalendarWeekRule;
        return calendar.GetWeekOfYear(dateSrc, calendarWeekRule, firstDayOfWeek);
    }

    /// <summary>
    /// Gets the start and end dates of a specific week in the specified year.
    /// </summary>
    /// <param name="year">The year (1-9999)</param>
    /// <param name="weekNumber">The week number within the year</param>
    /// <param name="culture">The culture info to use for calculation. If null, uses default culture</param>
    /// <returns>A tuple containing the start and end dates of the specified week</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when year is not between 1 and 9999 or weekNumber is invalid</exception>
    public static
#if NET40
    Tuple<DateTime, DateTime>
#else
    (DateTime Start, DateTime End)
#endif
    GetStartEndDayOfWeek(int year, int weekNumber, CultureInfo? culture = null)
    {
        // Parameter validation
        if (year < 1 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1 and 9999.");

        culture ??= ExtensionMethodSetting.DefaultCulture;

        // Get the number of weeks in the specified year
        var totalWeeks = GetWeekCountOfYear(year, culture);
        if (weekNumber < 1 || weekNumber > totalWeeks)
            throw new ArgumentOutOfRangeException(nameof(weekNumber), $"Week number must be between 1 and {totalWeeks} for year {year}.");

        // Determine the start date of the first week
        var firstWeekStartEndDay = GetStartEndDayOfFirstWeek(year, culture);
        if (weekNumber == 1)
        {
            return firstWeekStartEndDay;
        }
#if NET40
            var targetWeekStart = firstWeekStartEndDay.Item1.AddDays(7 * (weekNumber - 1));
            var targetWeekEnd = targetWeekStart.AddDays(7).AddMilliseconds(-1);
            return Tuple.Create(targetWeekStart, targetWeekEnd);
#else
        var targetWeekStart = firstWeekStartEndDay.Start.AddDays(7 * (weekNumber - 1));
        var targetWeekEnd = targetWeekStart.AddDays(7).AddMilliseconds(-1);
        return (targetWeekStart, targetWeekEnd);
#endif
    }

    /// <summary>
    /// Gets the start and end dates of the first week of the specified year based on culture settings.
    /// </summary>
    /// <param name="year">The year (1-9999)</param>
    /// <param name="culture">The culture info to use for calculation. If null, uses default culture</param>
    /// <returns>A tuple containing the start and end dates of the first week</returns>
    public static
#if NET40
    Tuple<DateTime, DateTime>
#else
    (DateTime Start, DateTime End)
#endif 
        GetStartEndDayOfFirstWeek(int year, CultureInfo? culture = null)
    {
        culture ??= ExtensionMethodSetting.DefaultCulture;
        var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
        var calendarWeekRule = culture.DateTimeFormat.CalendarWeekRule;

        return GetStartEndDayOfFirstWeek(year, firstDayOfWeek, calendarWeekRule);
    }

    /// <summary>
    /// Gets the start and end dates of the first week of the specified year based on explicit week rules.
    /// </summary>
    /// <param name="year">The year (1-9999)</param>
    /// <param name="firstDayOfWeek">The first day of the week</param>
    /// <param name="calendarWeekRule">The rule to determine the first week of the year</param>
    /// <returns>A tuple containing the start and end dates of the first week</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when year is not between 1 and 9999</exception>
    public static
#if NET40
    Tuple<DateTime, DateTime>
#else
    (DateTime Start, DateTime End)
#endif
        GetStartEndDayOfFirstWeek(int year, DayOfWeek firstDayOfWeek, CalendarWeekRule calendarWeekRule)
    {
        if (year is < 1 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1 and 9999.");
        }

        var jan1 = new DateTime(year, 1, 1);
        var jan1DayOfWeek = jan1.DayOfWeek;

        int dayOffset = ((7 + (int)firstDayOfWeek - (int)jan1DayOfWeek) % 7);

        DateTime firstWeekStart = jan1;
        DateTime firstWeekEnd = firstWeekStart.AddDays(7);

        switch (calendarWeekRule)
        {
            case CalendarWeekRule.FirstDay:
                firstWeekStart = jan1;
                if (firstDayOfWeek == jan1DayOfWeek)
                {
                    firstWeekEnd = jan1.AddDays(7);
                }
                else
                {
                    firstWeekEnd = jan1.AddDays(dayOffset);
                }
                break;

            case CalendarWeekRule.FirstFullWeek:
                if (firstDayOfWeek == jan1DayOfWeek)
                {
                    firstWeekStart = jan1;
                }
                else
                {
                    firstWeekStart = jan1.AddDays(dayOffset);
                }
                firstWeekEnd = firstWeekStart.AddDays(7);
                break;

            case CalendarWeekRule.FirstFourDayWeek:
                if (dayOffset > 4)
                {
                    firstWeekStart = jan1;
                    firstWeekEnd = jan1.AddDays(dayOffset);
                }
                else
                {
                    firstWeekStart = jan1.AddDays(dayOffset);
                    firstWeekEnd = firstWeekStart.AddDays(7);
                }
                break;
        }
        firstWeekEnd = firstWeekEnd.AddMilliseconds(-1);
#if NET40
        return Tuple.Create(firstWeekStart, firstWeekEnd);
#else
        return (firstWeekStart, firstWeekEnd);
#endif
    }
}

public enum TimeMode
{
    Now,
    Zero,
    Full
}

public enum TimeUnit
{
    Days,
    Hours,
    Minutes,
    Seconds,
    Milliseconds,
    Months,
    Years
}
