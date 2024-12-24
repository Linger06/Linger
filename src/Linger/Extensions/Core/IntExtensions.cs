namespace Linger.Extensions.Core;

/// <summary>
/// Provides extension methods for the <see cref="int"/> data type.
/// </summary>
public static class IntExtensions
{
    /// <summary>
    /// Converts the file size in bytes to a human-readable string representation.
    /// </summary>
    /// <param name="contentLength">The file size in bytes.</param>
    /// <returns>A string representing the file size in a human-readable format (e.g., Bytes, KB, MB, GB, etc.).</returns>
    public static string FileSize(this int contentLength)
    {
        var size = Convert.ToDouble(contentLength);
        string[] units = ["Bytes", "KB", "MB", "GB", "TB", "PB"];
        const double Mod = 1024.0;
        var i = 0;
        while (size >= Mod)
        {
            size /= Mod;
            i++;
        }

        return Math.Round(size) + units[i];
    }

    /// <summary>
    /// Converts the value to a string representation with thousand separators.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A string representation of the value with thousand separators.</returns>
    public static string ToThousand(this int value)
    {
        return $"{value:N}";
    }
}