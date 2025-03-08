using System.Globalization;

namespace Linger.Extensions.Core;

public static partial class DateTimeExtensions
{
    public static string ToFormatDate(this DateTime dateTime, string format = "yyyy-MM-dd") => dateTime.ToString(format);

    public static string? ToFormatDate(this DateTime? dateTime, string format = "yyyy-MM-dd") => dateTime?.ToString(format);

    public static string ToFormatDateTime(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss") => dateTime.ToString(format);

    public static string? ToFormatDateTime(this DateTime? dateTime, string format = "yyyy-MM-dd HH:mm:ss") => dateTime?.ToString(format);

    public static TimeSpan GetDateDifference(this DateTime dateTime1, DateTime dateTime2) => dateTime1 - dateTime2;

    public static double GetDateDifference(this DateTime dateTime1, DateTime? dateTime2, TimeUnit unit, bool abs = false)
    {
        var date2 = dateTime2 ?? DateTime.Now;
        var dateDiff = unit switch
        {
            TimeUnit.Days => (date2 - dateTime1).TotalDays,
            TimeUnit.Hours => (date2 - dateTime1).TotalHours,
            TimeUnit.Minutes => (date2 - dateTime1).TotalMinutes,
            TimeUnit.Seconds => (date2 - dateTime1).TotalSeconds,
            TimeUnit.Milliseconds => (date2 - dateTime1).TotalMilliseconds,
            TimeUnit.Months => (date2.Year - dateTime1.Year) * 12 + date2.Month - dateTime1.Month +
                             (date2.Day >= dateTime1.Day ? 0 : -1),
            TimeUnit.Years => date2.Year - dateTime1.Year +
                            ((date2.Month > dateTime1.Month ||
                             (date2.Month == dateTime1.Month && date2.Day >= dateTime1.Day)) ? 0 : -1),
            _ => 0
        };
        return abs ? Math.Abs(dateDiff) : dateDiff;
    }

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

    public static string ToStringOfMode(this DateTime dateTime, int dateMode)
    {
        return dateMode switch
        {
            0 => dateTime.ToString("yyyy-MM-dd"),
            1 => dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
            2 => dateTime.ToString("yyyy/MM/dd"),
            4 => dateTime.ToString("MM-dd"),
            5 => dateTime.ToString("MM/dd"),
            7 => dateTime.ToString("yyyy-MM"),
            8 => dateTime.ToString("yyyy/MM"),
            10 => dateTime.ToString("yyyy-MM-dd") + " 00:00:00",
            11 => dateTime.ToString("yyyyMMdd"),
            12 => dateTime.ToString("yyyyMMddHHmmss"),
            13 => dateTime.ToString("MM/dd/yyyy"),
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

    public static DateTime SetTime(this DateTime current, int hour)
    {
        return current.SetTime(hour, 0);
    }

    public static DateTime SetTime(this DateTime current, int hour, int minute, int second = 0, int millisecond = 0)
    {
        return new DateTime(current.Year, current.Month, current.Day, hour, minute, second, millisecond);
    }

    public static DateTime SetDay(this DateTime time, int day)
    {
        return new DateTime(time.Year, time.Month, day);
    }

    public static DateTime SetMonth(this DateTime time, int month)
    {
        return new DateTime(time.Year, month, time.Day);
    }

    public static DateTime SetYear(this DateTime time, int year)
    {
        return new DateTime(year, time.Month, time.Day);
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

    public static DateTime StartOfDay(this DateTime @this)
    {
        return new DateTime(@this.Year, @this.Month, @this.Day);
    }

    public static DateTime EndOfDay(this DateTime date)
    {
        return date.SetTime(23, 59, 59, 999);
    }

    public static DateTime StartOfMonth(this DateTime @this)
    {
        return new DateTime(@this.Year, @this.Month, 1);
    }

    public static DateTime EndOfMonth(this DateTime @this)
    {
        return new DateTime(@this.Year, @this.Month, 1).AddMonths(1).AddMilliseconds(-1);
    }

    public static DateTime FirstDayOfMonth(this DateTime date, DayOfWeek? dayOfWeek = null)
    {
        var firstDay = new DateTime(date.Year, date.Month, 1);
        if (!dayOfWeek.HasValue)
            return firstDay;

        while (firstDay.DayOfWeek != dayOfWeek.Value)
        {
            firstDay = firstDay.AddDays(1);
        }
        return firstDay;
    }

    public static DateTime FirstDayOfMonth2(this DateTime dateTime, TimeMode mode = TimeMode.Now)
    {
        return dateTime.AddDays(1 - dateTime.Day).ToDateTimeOfMode(mode);
    }

    public static DateTime LastDayOfMonth(this DateTime date, DayOfWeek? dayOfWeek = null)
    {
        var lastDay = new DateTime(date.Year, date.Month, date.GetCountDaysOfMonth());
        if (!dayOfWeek.HasValue)
            return lastDay;

        while (lastDay.DayOfWeek != dayOfWeek.Value)
        {
            lastDay = lastDay.AddDays(-1);
        }
        return lastDay;
    }

    public static DateTime EndOfYear(this DateTime @this)
    {
        return new DateTime(@this.Year, 1, 1).AddYears(1).Subtract(new TimeSpan(0, 0, 0, 0, 1));
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
