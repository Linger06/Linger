namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="DateTime"/> extensions
/// </summary>
public static partial class DateTimeExtensions
{
    /// <summary>
    /// Calculates the age based on today.
    /// </summary>
    /// <param name="dateOfBirth">The date of birth.</param>
    /// <returns>The calculated age.</returns>
    /// <example>
    /// <code>
    /// DateTime birthDate = new DateTime(1990, 1, 1);
    /// int age = birthDate.CalculateAge();
    /// // age is the number of years from 1990 to the current year
    /// </code>
    /// </example>
    public static int CalculateAge(this DateTime dateOfBirth)
    {
        return dateOfBirth.CalculateAge(DateTime.Now);
    }

    /// <summary>
    /// Calculates the age based on a passed reference date.
    /// </summary>
    /// <param name="dateOfBirth">The date of birth.</param>
    /// <param name="referenceDate">The reference date to calculate on.</param>
    /// <returns>The calculated age.</returns>
    /// <example>
    /// <code>
    /// DateTime birthDate = new DateTime(1990, 1, 1);
    /// DateTime referenceDate = new DateTime(2023, 1, 1);
    /// int age = birthDate.CalculateAge(referenceDate);
    /// // age is 33
    /// </code>
    /// </example>
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

    /// <summary>
    /// Returns the number of days in the month of the provided date.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The number of days.</returns>
    /// <example>
    /// <code>
    /// DateTime date = new DateTime(2023, 2, 1);
    /// int daysInMonth = date.GetCountDaysOfMonth();
    /// // daysInMonth is 28
    /// </code>
    /// </example>
    public static int GetCountDaysOfMonth(this DateTime date)
    {
        DateTime nextMonth = date.AddMonths(1);
        return new DateTime(nextMonth.Year, nextMonth.Month, 1).AddDays(-1).Day;
    }

    /// <summary>
    /// Indicates whether the date is today.
    /// </summary>
    /// <param name="dt">The date.</param>
    /// <returns><c>true</c> if the specified date is today; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// DateTime date = DateTime.Now;
    /// bool isToday = date.IsToday();
    /// // isToday is true
    /// </code>
    /// </example>
    public static bool IsToday(this DateTime dt)
    {
        return dt.Date == DateTime.Today;
    }

    /// <summary>
    /// Sets the time of the current date with minute precision.
    /// </summary>
    /// <param name="current">The current date.</param>
    /// <param name="hour">The hour.</param>
    /// <returns>A DateTime.</returns>
    /// <example>
    /// <code>
    /// DateTime date = DateTime.Now;
    /// DateTime newDate = date.SetTime(14);
    /// // newDate is the current date with the time set to 14:00
    /// </code>
    /// </example>
    public static DateTime SetTime(this DateTime current, int hour)
    {
        return current.SetTime(hour, 0);
    }

    /// <summary>
    /// Sets the time of the current date with millisecond precision.
    /// </summary>
    /// <param name="current">The current date.</param>
    /// <param name="hour">The hour.</param>
    /// <param name="minute">The minute.</param>
    /// <param name="second">The second.</param>
    /// <param name="millisecond">The millisecond.</param>
    /// <returns>A DateTime.</returns>
    /// <example>
    /// <code>
    /// DateTime date = DateTime.Now;
    /// DateTime newDate = date.SetTime(14, 30, 15, 500);
    /// // newDate is the current date with the time set to 14:30:15.500
    /// </code>
    /// </example>
    public static DateTime SetTime(this DateTime current, int hour, int minute, int second = 0, int millisecond = 0)
    {
        return new DateTime(current.Year, current.Month, current.Day, hour, minute, second, millisecond);
    }
}
