using Linger.Extensions.Core;

namespace Linger.Helper;

/// <summary>
/// Provides methods for normalizing and manipulating file paths.
/// </summary>
public static class PathNormalization
{
    /// <summary>
    /// Gets the root of the specified path.
    /// </summary>
    /// <param name="path">The path to get the root from.</param>
    /// <returns>The root of the path, or an empty string if the path is null or empty.</returns>
    /// <example>
    /// <code>
    /// string root = PathNormalization.GetPathRoot("C:\\Users\\");
    /// // root = "C:\\"
    /// </code>
    /// </example>
    public static string GetPathRoot(string? path)
    {
        if (path.IsNullOrEmpty())
            return string.Empty;

        var rootPath = string.Empty;
        try
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            var pathAsUri = new Uri(path.Replace("\\", "/"));
#else
            var pathAsUri = new Uri(path.Replace("\\", "/", StringComparison.Ordinal));
#endif
            rootPath = pathAsUri.GetLeftPart(UriPartial.Authority);
            if (pathAsUri.IsFile && !string.IsNullOrEmpty(rootPath))
                rootPath = new Uri(rootPath).LocalPath;
        }
        catch (UriFormatException)
        {
        }
        if (string.IsNullOrEmpty(rootPath))
            rootPath = Path.GetPathRoot(path) ?? string.Empty;

        return rootPath;
    }

    /// <summary>
    /// Trims the trailing directory separators from the specified path.
    /// </summary>
    /// <param name="path">The path to trim.</param>
    /// <returns>The trimmed path, or null if the input path is null.</returns>
    /// <example>
    /// <code>
    /// string trimmedPath = "C:\\Users\\".TrimPath();
    /// // trimmedPath = "C:\\Users"
    /// </code>
    /// </example>
    public static string? TrimPath(this string? path)
    {
        return path?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// Gets the parent directory of the specified path.
    /// </summary>
    /// <param name="path">The path to get the parent directory from.</param>
    /// <returns>The parent directory, or an empty string if the path is null or empty.</returns>
    /// <example>
    /// <code>
    /// string parentDir = PathNormalization.GetParentDir("C:\\Users\\Documents");
    /// // parentDir = "C:\\Users"
    /// </code>
    /// </example>
    public static string GetParentDir(string? path)
    {
        if (path.IsNullOrEmpty())
            return string.Empty;

#if NETFRAMEWORK || NETSTANDARD2_0
        var index = path.Contains('/') ? path.LastIndexOf("/", StringComparison.Ordinal) : path.LastIndexOf("\\", StringComparison.Ordinal);
#else
        var index = path.Contains('/', StringComparison.Ordinal) ? path.LastIndexOf("/", StringComparison.Ordinal) : path.LastIndexOf("\\", StringComparison.Ordinal);
#endif
        return path.Substring(0, index != -1 ? index : path.Length);
    }

    /// <summary>
    /// Combines the specified folder and name into a single path.
    /// </summary>
    /// <param name="folder">The folder path.</param>
    /// <param name="name">The name to combine with the folder.</param>
    /// <returns>The combined path.</returns>
    /// <example>
    /// <code>
    /// string combinedPath = PathNormalization.Combine("C:\\Users", "file.txt");
    /// // combinedPath = "C:\\Users\\file.txt"
    /// </code>
    /// </example>
    public static string Combine(string? folder, string name)
    {
        if (folder.IsNullOrEmpty())
            return name;

#if NETFRAMEWORK || NETSTANDARD2_0
        return folder.Contains('/') ? Path.Combine(folder, name).Replace("\\", "/") : Path.Combine(folder, name);
#else
        return folder.Contains('/', StringComparison.Ordinal) ? Path.Combine(folder, name).Replace("\\", "/", StringComparison.Ordinal) : Path.Combine(folder, name);
#endif
    }
}
