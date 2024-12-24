namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="DateTime"/> extensions
/// </summary>
public static partial class DateTimeExtensions
{
    /// <summary>
    /// Sql Server database <see cref="DateTime"/> initial value: January 1, 1900, 00:00:00.000
    /// </summary>
    public static DateTime MsSqlDateTimeInitial => new(1900, 1, 1, 0, 0, 0, 0);

    /// <summary>
    /// Sql Server database <see cref="DateTime"/> minimum value: January 1, 1900, 00:00:00.000
    /// </summary>
    public static DateTime MsSqlDateTimeMinValue => MsSqlDateTimeInitial;

    /// <summary>
    /// Sql Server database <see cref="DateTime"/> maximum value: December 31, 9999, 23:59:59.999
    /// </summary>
    public static DateTime MsSqlDateTimeMaxValue => DateTime.MaxValue;

    /// <summary>
    /// MySql database <see cref="DateTime"/> initial value: January 1, 1753, 00:00:00.000
    /// </summary>
    public static DateTime MySqlDateTimeInitial => new(1753, 1, 1, 0, 0, 0, 0);

    /// <summary>
    /// MySql database <see cref="DateTime"/> minimum value: January 1, 1753, 00:00:00.000
    /// </summary>
    public static DateTime MySqlDateTimeMinValue => MySqlDateTimeInitial;

    /// <summary>
    /// MySql database <see cref="DateTime"/> maximum value: December 31, 9999, 23:59:59.999
    /// </summary>
    public static DateTime MySqlDateTimeMaxValue => DateTime.MaxValue;

    /// <summary>
    /// Returns the system UTC offset.
    /// </summary>
    public static double UtcOffset => DateTime.Now.Subtract(DateTime.UtcNow).TotalHours;

    /// <summary>
    /// Converts the current <see cref="DateTime"/> instance to a formatted date string representation.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> instance to convert.</param>
    /// <param name="format">The string format for the date, default is "yyyy-MM-dd".</param>
    /// <returns>A string representation of the current <see cref="DateTime"/> object, formatted as specified.</returns>
    public static string ToFormatDate(this DateTime dateTime, string format = "yyyy-MM-dd") => dateTime.ToString(format);

    /// <summary>
    /// Converts the current nullable <see cref="DateTime"/> instance to a formatted date string representation.
    /// </summary>
    /// <param name="dateTime">The nullable <see cref="DateTime"/> instance to convert.</param>
    /// <param name="format">The string format for the date, default is "yyyy-MM-dd".</param>
    /// <returns>A string representation of the current nullable <see cref="DateTime"/> object, formatted as specified, or null if the instance is null.</returns>
    public static string? ToFormatDate(this DateTime? dateTime, string format = "yyyy-MM-dd") => dateTime?.ToString(format);

    /// <summary>
    /// Converts the current <see cref="DateTime"/> instance to a formatted date-time string representation.
    /// </summary>
    /// <param name="dateTime">The <see cref="DateTime"/> instance to convert.</param>
    /// <param name="format">The string format for the date-time, default is "yyyy-MM-dd HH:mm:ss".</param>
    /// <returns>A string representation of the current <see cref="DateTime"/> object, formatted as specified.</returns>
    public static string ToFormatDateTime(this DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss") => dateTime.ToString(format);

    /// <summary>
    /// Converts the current nullable <see cref="DateTime"/> instance to a formatted date-time string representation.
    /// </summary>
    /// <param name="dateTime">The nullable <see cref="DateTime"/> instance to convert.</param>
    /// <param name="format">The string format for the date-time, default is "yyyy-MM-dd HH:mm:ss".</param>
    /// <returns>A string representation of the current nullable <see cref="DateTime"/> object, formatted as specified, or null if the instance is null.</returns>
    public static string? ToFormatDateTime(this DateTime? dateTime, string format = "yyyy-MM-dd HH:mm:ss") => dateTime?.ToString(format);

    /// <summary>
    /// Gets the difference between two dates.
    /// </summary>
    /// <param name="dateTime1">The first date.</param>
    /// <param name="dateTime2">The second date.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the difference between the two dates (may be negative if dateTime2 is greater than dateTime1).</returns>
    public static TimeSpan DateDiff2(this DateTime dateTime1, DateTime dateTime2) => dateTime1 - dateTime2;

    /// <summary>
    /// Gets the difference between two dates in the specified unit.
    /// </summary>
    /// <param name="dateTime1">The earlier date.</param>
    /// <param name="dateTime2">The later date.</param>
    /// <param name="type">The unit of time to measure the difference in ("D" for days, "H" for hours, "M" for minutes, "S" for seconds, "MS" for milliseconds).</param>
    /// <param name="abs">Whether to return the absolute value of the difference.</param>
    /// <returns>The difference between the two dates in the specified unit.</returns>
    public static double DateDiff3(this DateTime dateTime1, DateTime? dateTime2, string type, bool abs = false)
    {
        TimeSpan ts = (dateTime2 ?? DateTime.Now) - dateTime1;
        var dateDiff = type switch
        {
            "D" => ts.TotalDays,
            "H" => ts.TotalHours,
            "M" => ts.TotalMinutes,
            "S" => ts.TotalSeconds,
            "MS" => ts.TotalMilliseconds,
            _ => 0
        };

        return abs ? Math.Abs(dateDiff) : dateDiff;
    }

    /// <summary>
    /// Formats the date-time according to the specified mode.
    /// </summary>
    /// <param name="dateTime">The date-time to format.</param>
    /// <param name="dateMode">The display mode (0-13) for formatting the date-time.</param>
    /// <returns>A string representation of the date-time formatted according to the specified mode.</returns>
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
}

/// <summary>
/// Time mode
/// </summary>
public enum TimeMode
{
    /// <summary>
    /// Returns the current hour, minute, and second.
    /// </summary>
    Now,

    /// <summary>
    /// Returns 00:00:00.000
    /// </summary>
    Zero,

    /// <summary>
    /// Returns 23:59:59.999
    /// </summary>
    Full
}
