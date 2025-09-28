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
    public static string FormatFileSize(this int contentLength)
    {
        var length = ((double)contentLength);
        return length.FormatFileSize();
    }
}