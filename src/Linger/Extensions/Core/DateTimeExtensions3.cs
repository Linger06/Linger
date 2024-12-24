using System.Globalization;

namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="DateTime"/> extensions
/// </summary>
public static partial class DateTimeExtensions
{
    /// <summary>
    /// Determines whether the date only part of two DateTime values are equal.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="dateToCompare">The date to compare with.</param>
    /// <returns><c>true</c> if both date values are equal; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// DateTime date1 = new DateTime(2023, 10, 5);
    /// DateTime date2 = new DateTime(2023, 10, 5, 12, 0, 0);
    /// bool isEqual = date1.IsDateEqual(date2);
    /// // isEqual is true
    /// </code>
    /// </example>
    public static bool IsDateEqual(this DateTime date, DateTime dateToCompare)
    {
        return date.Date == dateToCompare.Date;
    }

    /// <summary>
    /// Determines whether the time only part of two DateTime values are equal.
    /// </summary>
    /// <param name="time">The time.</param>
    /// <param name="timeToCompare">The time to compare.</param>
    /// <returns><c>true</c> if both time values are equal; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// DateTime time1 = new DateTime(2023, 10, 5, 14, 30, 0);
    /// DateTime time2 = new DateTime(2023, 10, 6, 14, 30, 0);
    /// bool isEqual = time1.IsTimeEqual(time2);
    /// // isEqual is true
    /// </code>
    /// </example>
    public static bool IsTimeEqual(this DateTime time, DateTime timeToCompare)
    {
        return time.TimeOfDay == timeToCompare.TimeOfDay;
    }

    /// <summary>
    /// Adds the specified amount of weeks (=7 days in the Gregorian calendar) to the passed date value.
    /// </summary>
    /// <param name="date">The origin date.</param>
    /// <param name="value">The amount of weeks to be added.</param>
    /// <returns>The new date value.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime newDate = date.AddWeeks(2);
    /// // newDate is 2023-10-19
    /// </code>
    /// </example>
    public static DateTime AddWeeks(this DateTime date, int value)
    {
        return date.AddDays(value * 7);
    }

    /// <summary>
    /// Get the number of days within that year.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <returns>The number of days within that year.</returns>
    /// <example>
    /// <code>
    /// int days = DateTimeExtensions.GetDays(2023);
    /// // days is 365
    /// </code>
    /// </example>
    public static int GetDays(int year)
    {
        return GetDays(year, ExtensionMethodSetting.DefaultCulture);
    }

    /// <summary>
    /// Get the number of days within that year. Uses the specified culture.
    /// </summary>
    /// <param name="year">The year.</param>
    /// <param name="culture">Specific culture.</param>
    /// <returns>The number of days within that year.</returns>
    /// <example>
    /// <code>
    /// CultureInfo culture = new CultureInfo("en-US");
    /// int days = DateTimeExtensions.GetDays(2023, culture);
    /// // days is 365
    /// </code>
    /// </example>
    public static int GetDays(int year, CultureInfo culture)
    {
        var first = new DateTime(year, 1, 1, culture.Calendar);
        var last = new DateTime(year + 1, 1, 1, culture.Calendar);
        return first.GetDays(last);
    }

    /// <summary>
    /// Get the number of days within that date year.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The number of days within that year.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// int days = date.GetDays();
    /// // days is 365
    /// </code>
    /// </example>
    public static int GetDays(this DateTime date)
    {
        return GetDays(date.Year, ExtensionMethodSetting.DefaultCulture);
    }

    /// <summary>
    /// Get the number of days within that date year. Allows user to specify culture.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="culture">Specific culture.</param>
    /// <returns>The number of days within that year.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// CultureInfo culture = new CultureInfo("en-US");
    /// int days = date.GetDays(culture);
    /// // days is 365
    /// </code>
    /// </example>
    public static int GetDays(this DateTime date, CultureInfo culture)
    {
        return GetDays(date.Year, culture);
    }

    /// <summary>
    /// Get the number of days between two dates.
    /// </summary>
    /// <param name="fromDate">The origin date.</param>
    /// <param name="toDate">The end date.</param>
    /// <returns>The number of days between the two dates.</returns>
    /// <example>
    /// <code>
    /// DateTime fromDate = new DateTime(2023, 1, 1);
    /// DateTime toDate = new DateTime(2023, 12, 31);
    /// int days = fromDate.GetDays(toDate);
    /// // days is 364
    /// </code>
    /// </example>
    public static int GetDays(this DateTime fromDate, DateTime toDate)
    {
        return Convert.ToInt32(toDate.Subtract(fromDate).TotalDays);
    }

    /// <summary>
    /// Indicates whether the specified date is Easter in the Christian calendar.
    /// </summary>
    /// <param name="date">Instance value.</param>
    /// <returns>True if the instance value is a valid Easter Date.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 4, 9);
    /// bool isEaster = date.IsEaster();
    /// // isEaster is true
    /// </code>
    /// </example>
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

    /// <summary>
    /// Indicates whether the source DateTime is before the supplied DateTime.
    /// </summary>
    /// <param name="source">The source DateTime.</param>
    /// <param name="other">The compared DateTime.</param>
    /// <returns>True if the source is before the other DateTime, False otherwise.</returns>
    /// <example>
    /// <code>
    /// DateTime date1 = new DateTime(2023, 10, 5);
    /// DateTime date2 = new DateTime(2023, 10, 6);
    /// bool isBefore = date1.IsBefore(date2);
    /// // isBefore is true
    /// </code>
    /// </example>
    public static bool IsBefore(this DateTime source, DateTime other)
    {
        return source.CompareTo(other) < 0;
    }

    /// <summary>
    /// Indicates whether the source DateTime is after the supplied DateTime.
    /// </summary>
    /// <param name="source">The source DateTime.</param>
    /// <param name="other">The compared DateTime.</param>
    /// <returns>True if the source is after the other DateTime, False otherwise.</returns>
    /// <example>
    /// <code>
    /// DateTime date1 = new DateTime(2023, 10, 6);
    /// DateTime date2 = new DateTime(2023, 10, 5);
    /// bool isAfter = date1.IsAfter(date2);
    /// // isAfter is true
    /// </code>
    /// </example>
    public static bool IsAfter(this DateTime source, DateTime other)
    {
        return source.CompareTo(other) > 0;
    }
}
