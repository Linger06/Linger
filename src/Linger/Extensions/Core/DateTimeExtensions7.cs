using System.Globalization;

namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="DateTime"/> extensions
/// </summary>
public static partial class DateTimeExtensions
{
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
            throw new ArgumentOutOfRangeException(nameof(weekNumber),
                $"Week number must be between 1 and {totalWeeks} for year {year}.");

        // Determine the start date of the first week
        var firstWeekStartEndDay = GetStartEndDayOfFirstWeek(year, culture);
        if (weekNumber == 1)
        {
            return firstWeekStartEndDay;
        }
        else
        {

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