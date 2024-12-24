namespace Linger.Helper;

/// <summary>
/// Provides helper methods for path normalization.
/// </summary>
/// <value>Static class containing path normalization methods.</value>
public static class PathHelper
{
    /// <summary>
    /// Normalizes the specified path by ensuring it ends with a directory separator character and converting it to a full path.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized path.</returns>
    /// <example>
    /// <code>
    /// string path = "C:\\example\\path";
    /// string normalizedPath = PathHelper.NormalizePath(path);
    /// // normalizedPath is "C:\\example\\path\\"
    /// </code>
    /// </example>
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        if (path.StartsWith("""\\""", StringComparison.Ordinal) || path.StartsWith("//", StringComparison.Ordinal) /*|| FtpHelpers.IsFtpPath(path)*/)
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            path += Path.DirectorySeparatorChar;

        try
        {
            var pathUri = new Uri(path).LocalPath;
            if (string.IsNullOrEmpty(pathUri))
                return path;

            return Path.GetFullPath(pathUri)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch (Exception ex) when (ex is UriFormatException or ArgumentException)
        {
            return path;
        }
    }
}
