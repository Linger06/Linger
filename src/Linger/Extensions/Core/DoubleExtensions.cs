namespace Linger.Extensions.Core;

/// <summary>
/// <see cref="double"/> extensions
/// </summary>
public static class DoubleExtensions
{
    /// <summary>
    /// Returns the size corresponding to the ContentLength.
    /// </summary>
    /// <param name="length">The length of the ContentLength.</param>
    /// <returns>The size in Bytes/KB/MB/GB/TB/PB.</returns>
    public static string FileSize(this double length)
    {
        string[] units = ["Bytes", "KB", "MB", "GB", "TB", "PB"];
        const double Mod = 1024.0;
        var i = 0;
        while (length >= Mod)
        {
            length /= Mod;
            i++;
        }

        return Math.Round(length) + units[i];
    }

    /// <summary>
    /// Gets a TimeSpan from a double number of days.
    /// </summary>
    /// <param name="days">The number of days the TimeSpan will contain.</param>
    /// <returns>A TimeSpan containing the specified number of days.</returns>
    /// <remarks>Contributed by jceddy</remarks>
    public static TimeSpan Days(this double days)
    {
        return TimeSpan.FromDays(days);
    }

    /// <summary>
    /// Gets a TimeSpan from a double number of hours.
    /// </summary>
    /// <param name="hours">The number of hours the TimeSpan will contain.</param>
    /// <returns>A TimeSpan containing the specified number of hours.</returns>
    /// <remarks>Contributed by jceddy</remarks>
    public static TimeSpan Hours(this double hours)
    {
        return TimeSpan.FromHours(hours);
    }

    /// <summary>
    /// Gets a TimeSpan from a double number of milliseconds.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds the TimeSpan will contain.</param>
    /// <returns>A TimeSpan containing the specified number of milliseconds.</returns>
    /// <remarks>Contributed by jceddy</remarks>
    public static TimeSpan Milliseconds(this double milliseconds)
    {
        return TimeSpan.FromMilliseconds(milliseconds);
    }

    /// <summary>
    /// Gets a TimeSpan from a double number of minutes.
    /// </summary>
    /// <param name="minutes">The number of minutes the TimeSpan will contain.</param>
    /// <returns>A TimeSpan containing the specified number of minutes.</returns>
    /// <remarks>Contributed by jceddy</remarks>
    public static TimeSpan Minutes(this double minutes)
    {
        return TimeSpan.FromMinutes(minutes);
    }

    /// <summary>
    /// Gets a TimeSpan from a double number of seconds.
    /// </summary>
    /// <param name="seconds">The number of seconds the TimeSpan will contain.</param>
    /// <returns>A TimeSpan containing the specified number of seconds.</returns>
    /// <remarks>Contributed by jceddy</remarks>
    public static TimeSpan Seconds(this double seconds)
    {
        return TimeSpan.FromSeconds(seconds);
    }
}