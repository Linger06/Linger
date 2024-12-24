namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="DateTime"/> extensions
/// </summary>
public static partial class DateTimeExtensions
{
    /// <summary>
    /// A DateTime extension method that returns a DateTime of the last day of the month with the
    /// time set to "23:59:59:999". The last moment of the last day of the month. Use "DateTime2"
    /// column type in SQL to keep the precision.
    /// </summary>
    /// <param name="this">The DateTime instance to act on.</param>
    /// <returns>A DateTime of the last day of the month with the time set to "23:59:59:999".</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime endOfMonth = date.EndOfMonth();
    /// // endOfMonth is 2023-10-31 23:59:59.999
    /// </code>
    /// </example>
    public static DateTime EndOfMonth(this DateTime @this)
    {
        return new DateTime(@this.Year, @this.Month, 1).AddMonths(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Returns the last day of the month of the provided date.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The last day of the month.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime lastDayOfMonth = date.LastDayOfMonth();
    /// // lastDayOfMonth is 2023-10-31
    /// </code>
    /// </example>
    public static DateTime LastDayOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.GetCountDaysOfMonth());
    }

    /// <summary>
    /// Returns the last day of the month of the provided date.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="dayOfWeek">The desired day of the week.</param>
    /// <returns>The DateTime instance representing the last occurrence of the specified day of the week in the month.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime lastFriday = date.LastDayOfMonth(DayOfWeek.Friday);
    /// // lastFriday is the last Friday of the month
    /// </code>
    /// </example>
    public static DateTime LastDayOfMonth(this DateTime date, DayOfWeek dayOfWeek)
    {
        DateTime dt = date.LastDayOfMonth();
        while (dt.DayOfWeek != dayOfWeek)
        {
            dt = dt.AddDays(-1);
        }

        return dt;
    }

    /// <summary>
    /// Gets the first day of the current month.
    /// </summary>
    /// <param name="dateTime">The DateTime instance.</param>
    /// <param name="mode">The time mode, default is the current time's hour, minute, and second.</param>
    /// <returns>A DateTime instance representing the first day of the current month.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime firstDayOfMonth = date.FirstDayOfMonth2();
    /// // firstDayOfMonth is 2023-10-01
    /// </code>
    /// </example>
    public static DateTime FirstDayOfMonth2(this DateTime dateTime, TimeMode mode = TimeMode.Now)
    {
        return dateTime.AddDays(1 - dateTime.Day).ToDateTimeOfMode(mode);
    }

    /// <summary>
    /// Returns the first day of the month of the provided date.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The first day of the month.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime firstDayOfMonth = date.FirstDayOfMonth();
    /// // firstDayOfMonth is 2023-10-01
    /// </code>
    /// </example>
    public static DateTime FirstDayOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    /// <summary>
    /// Returns the first day of the month of the provided date.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="dayOfWeek">The desired day of the week.</param>
    /// <returns>The DateTime instance representing the first occurrence of the specified day of the week in the month.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime firstMonday = date.FirstDayOfMonth(DayOfWeek.Monday);
    /// // firstMonday is the first Monday of the month
    /// </code>
    /// </example>
    public static DateTime FirstDayOfMonth(this DateTime date, DayOfWeek dayOfWeek)
    {
        DateTime dt = date.FirstDayOfMonth();
        while (dt.DayOfWeek != dayOfWeek)
        {
            dt = dt.AddDays(1);
        }

        return dt;
    }

    /// <summary>
    /// Converts the current <see cref="DateTime"/> instance to the specified time mode.
    /// </summary>
    /// <param name="dateTime">The DateTime instance.</param>
    /// <param name="timeMode">The time mode, default is the current time's hour, minute, and second.</param>
    /// <returns>A DateTime instance representing the date and time in the specified time mode.</returns>
    public static DateTime ToDateTimeOfMode(this DateTime dateTime, TimeMode timeMode = TimeMode.Now)
    {
        return timeMode switch
        {
            TimeMode.Zero => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0),
            TimeMode.Full => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999),
            _ => dateTime
        };
    }

    /// <summary>
    /// Indicates whether the specified date is a leap year.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns><c>true</c> if the specified date is a leap year; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2024, 1, 1);
    /// bool isLeapYear = date.IsLeapYear();
    /// // isLeapYear is true
    /// </code>
    /// </example>
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
}