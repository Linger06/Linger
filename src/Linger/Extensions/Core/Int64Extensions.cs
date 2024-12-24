namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for the <see cref="long"/> (Int64) data type.
/// </summary>
public static class Int64Extensions
{
    /// <summary>
    /// Converts the Unix timestamp to a <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="timeStamp">The Unix timestamp.</param>
    /// <returns>A <see cref="DateTime"/> object representing the specified Unix timestamp.</returns>
    public static DateTime ToDateTime(this long timeStamp)
    {
        var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return start.AddMilliseconds(timeStamp).AddHours(8);
    }

    /// <summary>
    /// Converts the file size in bytes to a human-readable string representation.
    /// </summary>
    /// <param name="length">The file size in bytes.</param>
    /// <returns>A string representing the file size in a human-readable format (e.g., Bytes, KB, MB, GB, etc.).</returns>
    public static string FileSize(this long length)
    {
        var size = Convert.ToDouble(length);
        string[] units = ["Bytes", "KB", "MB", "GB", "TB", "PB"];
        const double Mod = 1024.0;
        var i = 0;
        while (size >= Mod)
        {
            size /= Mod;
            i++;
        }

        return $"{Math.Round(size)}{units[i]}";
    }

    /// <summary>
    /// Converts the value to a string representation with thousand separators.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A string representation of the value with thousand separators.</returns>
    public static string ToThousand(this long value)
    {
        return $"{value:N}";
    }
}