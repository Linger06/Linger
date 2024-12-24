namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="DateTime"/> extensions
/// </summary>
public static partial class DateTimeExtensions
{
    /// <summary>
    /// Sets the day of the current DateTime instance.
    /// </summary>
    /// <param name="time">The DateTime instance.</param>
    /// <param name="day">The day to set.</param>
    /// <returns>A new DateTime instance with the specified day.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime newDate = date.SetDay(15);
    /// // newDate is 2023-10-15
    /// </code>
    /// </example>
    public static DateTime SetDay(this DateTime time, int day)
    {
        return new DateTime(time.Year, time.Month, day);
    }

    /// <summary>
    /// Sets the month of the current DateTime instance.
    /// </summary>
    /// <param name="time">The DateTime instance.</param>
    /// <param name="month">The month to set.</param>
    /// <returns>A new DateTime instance with the specified month.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime newDate = date.SetMonth(12);
    /// // newDate is 2023-12-05
    /// </code>
    /// </example>
    public static DateTime SetMonth(this DateTime time, int month)
    {
        return new DateTime(time.Year, month, time.Day);
    }

    /// <summary>
    /// Sets the year of the current DateTime instance.
    /// </summary>
    /// <param name="time">The DateTime instance.</param>
    /// <param name="year">The year to set.</param>
    /// <returns>A new DateTime instance with the specified year.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime newDate = date.SetYear(2025);
    /// // newDate is 2025-10-05
    /// </code>
    /// </example>
    public static DateTime SetYear(this DateTime time, int year)
    {
        return new DateTime(year, time.Month, time.Day);
    }

    /// <summary>
    /// Returns the first day of the month with the time set to "00:00:00:000".
    /// </summary>
    /// <param name="this">The DateTime instance.</param>
    /// <returns>A DateTime instance representing the first day of the month with the time set to "00:00:00:000".</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime startOfMonth = date.StartOfMonth();
    /// // startOfMonth is 2023-10-01 00:00:00.000
    /// </code>
    /// </example>
    public static DateTime StartOfMonth(this DateTime @this)
    {
        return new DateTime(@this.Year, @this.Month, 1);
    }

    /// <summary>
    /// Returns the first moment of the day with the time set to "00:00:00:000".
    /// </summary>
    /// <param name="this">The DateTime instance.</param>
    /// <returns>A DateTime instance representing the first moment of the day with the time set to "00:00:00:000".</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5, 14, 30, 0);
    /// DateTime startOfDay = date.StartOfDay();
    /// // startOfDay is 2023-10-05 00:00:00.000
    /// </code>
    /// </example>
    public static DateTime StartOfDay(this DateTime @this)
    {
        return new DateTime(@this.Year, @this.Month, @this.Day);
    }

    /// <summary>
    /// Returns the date at 23:59:59.999 for the specified DateTime.
    /// </summary>
    /// <param name="date">The DateTime to be processed.</param>
    /// <returns>The date at 23:59:59.999.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime endOfDay = date.EndOfDay();
    /// // endOfDay is 2023-10-05 23:59:59.999
    /// </code>
    /// </example>
    public static DateTime EndOfDay(this DateTime date)
    {
        return date.SetTime(23, 59, 59, 999);
    }

    /// <summary>
    /// Returns the last day of the year with the time set to "23:59:59:999".
    /// </summary>
    /// <param name="this">The DateTime instance.</param>
    /// <returns>A DateTime instance representing the last day of the year with the time set to "23:59:59:999".</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime endOfYear = date.EndOfYear();
    /// // endOfYear is 2023-12-31 23:59:59.999
    /// </code>
    /// </example>
    public static DateTime EndOfYear(this DateTime @this)
    {
        return new DateTime(@this.Year, 1, 1).AddYears(1).Subtract(new TimeSpan(0, 0, 0, 0, 1));
    }
}
