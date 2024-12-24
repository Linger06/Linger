namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="DateTime"/> extensions
/// </summary>
public static partial class DateTimeExtensions
{
    /// <summary>
    /// Checks if the DateTime value is between the specified minimum and maximum values inclusively.
    /// </summary>
    /// <param name="this">The DateTime instance to act on.</param>
    /// <param name="minValue">The minimum value.</param>
    /// <param name="maxValue">The maximum value.</param>
    /// <returns>true if the value is between the minimum and maximum values inclusively; otherwise, false.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateTime minValue = new DateTime(2023, 1, 1);
    /// DateTime maxValue = new DateTime(2023, 12, 31);
    /// bool inRange = date.InRange(minValue, maxValue);
    /// // inRange is true
    /// </code>
    /// </example>
    public static bool InRange(this DateTime @this, DateTime minValue, DateTime maxValue)
    {
        return @this >= minValue && @this <= maxValue;
    }

    /// <summary>
    /// Gets the day of the year for the specified DateTime.
    /// </summary>
    /// <param name="dateSrc">The DateTime instance.</param>
    /// <returns>The day of the year.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// int dayOfYear = date.DayInYear();
    /// // dayOfYear is 278
    /// </code>
    /// </example>
    public static int DayInYear(this DateTime dateSrc)
    {
        return dateSrc.DayOfYear;
    }

    /// <summary>
    /// Gets the day of the week for the specified DateTime.
    /// </summary>
    /// <param name="dateSrc">The DateTime instance.</param>
    /// <returns>The day of the week as an integer.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// int dayOfWeek = date.DayInWeek();
    /// // dayOfWeek is 4 (Thursday)
    /// </code>
    /// </example>
    public static int DayInWeek(this DateTime dateSrc)
    {
        return dateSrc.DayOfWeek switch
        {
            DayOfWeek.Sunday => 7,
            _ => (int)dateSrc.DayOfWeek
        };
    }

#if NET6_0_OR_GREATER

    /// <summary>
    /// Converts the DateTime instance to a DateOnly instance.
    /// </summary>
    /// <param name="datetime">The DateTime instance.</param>
    /// <returns>A DateOnly instance.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5);
    /// DateOnly dateOnly = date.ToDateOnly();
    /// // dateOnly is 2023-10-05
    /// </code>
    /// </example>
    public static DateOnly ToDateOnly(this DateTime datetime)
    {
        return DateOnly.FromDateTime(datetime);
    }

    /// <summary>
    /// Converts the nullable DateTime instance to a nullable DateOnly instance.
    /// </summary>
    /// <param name="datetime">The nullable DateTime instance.</param>
    /// <returns>A nullable DateOnly instance.</returns>
    /// <example>
    /// <code>
    /// DateTime? date = new DateTime(2023, 10, 5);
    /// DateOnly? dateOnly = date.ToDateOnly();
    /// // dateOnly is 2023-10-05
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// DateOnly? dateOnly = new DateOnly(2023, 10, 5);
    /// DateTime? dateTime = dateOnly.ToDateTime();
    /// // dateTime is 2023-10-05 00:00:00
    /// </code>
    /// </example>
    public static DateTime? ToDateTime(this DateOnly? dateOnly)
    {
        return dateOnly?.ToDateTime(TimeOnly.MinValue);
    }

    /// <summary>
    /// Converts the DateOnly instance to a DateTime instance with the time set to midnight.
    /// </summary>
    /// <param name="dateOnly">The DateOnly instance.</param>
    /// <returns>A DateTime instance.</returns>
    /// <example>
    /// <code>
    /// DateOnly dateOnly = new DateOnly(2023, 10, 5);
    /// DateTime dateTime = dateOnly.ToDateTime();
    /// // dateTime is 2023-10-05 00:00:00
    /// </code>
    /// </example>
    public static DateTime ToDateTime(this DateOnly dateOnly)
    {
        return dateOnly.ToDateTime(TimeOnly.MinValue);
    }

    /// <summary>
    /// Converts the DateTime instance to a TimeOnly instance.
    /// </summary>
    /// <param name="datetime">The DateTime instance.</param>
    /// <returns>A TimeOnly instance.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 10, 5, 14, 30, 0);
    /// TimeOnly timeOnly = date.ToTimeOnly();
    /// // timeOnly is 14:30:00
    /// </code>
    /// </example>
    public static TimeOnly ToTimeOnly(this DateTime datetime)
    {
        return TimeOnly.FromDateTime(datetime);
    }

    /// <summary>
    /// Converts the nullable DateTime instance to a nullable TimeOnly instance.
    /// </summary>
    /// <param name="datetime">The nullable DateTime instance.</param>
    /// <returns>A nullable TimeOnly instance.</returns>
    /// <example>
    /// <code>
    /// DateTime? date = new DateTime(2023, 10, 5, 14, 30, 0);
    /// TimeOnly? timeOnly = date.ToTimeOnly();
    /// // timeOnly is 14:30:00
    /// </code>
    /// </example>
    public static TimeOnly? ToTimeOnly(this DateTime? datetime)
    {
        return datetime.HasValue ? TimeOnly.FromDateTime(datetime.Value) : null;
    }

#endif
}