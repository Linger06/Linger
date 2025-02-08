namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="DateTime"/> extensions
/// </summary>
public static partial class DateTimeExtensions
{
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