using Linger.Helper;

namespace Linger.Extensions.IO;

/// <summary>
/// Provides Extension methods for path.
/// </summary>
public static class PathExtensions
{
#if NETCOREAPP

    /// <summary>
    /// Gets the relative path from a specified base path.
    /// </summary>
    /// <param name="path">The target path.</param>
    /// <param name="relativeTo">The base path. If null, the current directory is used.</param>
    /// <returns>The relative path.</returns>
    /// <example>
    /// <code>
    /// string path = @"C:\Projects\MyProject\file.txt";
    /// string basePath = @"C:\Projects";
    /// string relativePath = path.GetRelativePath(basePath);
    /// // relativePath is "MyProject\file.txt"
    /// </code>
    /// </example>
    public static string GetRelativePath(this string path, string? relativeTo = null)
    {
        relativeTo ??= Environment.CurrentDirectory;
        return Path.GetRelativePath(relativeTo, path);
    }

#elif NETFRAMEWORK || NETSTANDARD
    /// <summary>
    /// Gets the relative path from a specified base path.
    /// </summary>
    /// <param name="path">The target path.</param>
    /// <param name="relativeTo">The base path. If null, the current directory is used.</param>
    /// <returns>The relative path.</returns>
    /// <example>
    /// <code>
    /// string path = @"C:\Projects\MyProject\file.txt";
    /// string basePath = @"C:\Projects";
    /// string relativePath = path.GetRelativePath(basePath);
    /// // relativePath is "MyProject\file.txt"
    /// </code>
    /// </example>
    public static string GetRelativePath(this string path, string? relativeTo = null)
    {
        relativeTo ??= Environment.CurrentDirectory;
        var absolutePath = path.GetAbsolutePath(relativeTo);
        return absolutePath.RelativeTo(relativeTo);
    }
#endif

    /// <summary>
    /// Gets the absolute path from a specified base path.
    /// </summary>
    /// <param name="path">The target path, which can be relative or absolute.</param>
    /// <param name="basePath">The base path. If null, the current directory is used.</param>
    /// <returns>The absolute path.</returns>
    /// <example>
    /// <code>
    /// string relativePath = @"..\MyProject\file.txt";
    /// string basePath = @"C:\Projects";
    /// string absolutePath = relativePath.GetAbsolutePath(basePath);
    /// // absolutePath is "C:\Projects\MyProject\file.txt"
    /// </code>
    /// </example>
    public static string GetAbsolutePath(this string path, string? basePath = null)
    {
        path.EnsureIsNotNull();

        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        basePath ??= Environment.CurrentDirectory;

        if (!Path.IsPathRooted(basePath))
        {
            throw new ArgumentException("Base path must be an absolute path.", nameof(basePath));
        }

        var combined = Path.Combine(basePath, path);
        combined = Path.GetFullPath(combined);
        return combined;
    }

    /// <summary>
    /// Determines whether the specified path is an absolute path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if the path is absolute; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the path is null or empty.</exception>
    /// <example>
    /// <code>
    /// string path = @"C:\Projects\MyProject\file.txt";
    /// bool isAbsolute = path.IsAbsolutePath();
    /// // isAbsolute is true
    /// </code>
    /// </example>
    public static bool IsAbsolutePath(this string path)
    {
        return Path.IsPathRooted(path);
    }

    /// <summary>
    /// Gets the relative path from the specified source path to the specified folder.
    /// </summary>
    /// <param name="sourcePath">The source path.</param>
    /// <param name="folder">The folder to which the relative path is calculated.</param>
    /// <returns>The relative path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the sourcePath or folder is null or empty.</exception>
    /// <example>
    /// <code>
    /// string sourcePath = @"C:\Projects\MyProject\file.txt";
    /// string folder = @"C:\Projects";
    /// string relativePath = sourcePath.RelativeTo(folder);
    /// // relativePath is "MyProject\file.txt"
    /// </code>
    /// </example>
    public static string RelativeTo(this string sourcePath, string folder)
    {
        sourcePath = PathHelper.NormalizePath(sourcePath);
        folder = PathHelper.NormalizePath(folder);

        var pathUri = new Uri(sourcePath);

        if (!Path.IsPathRooted(folder))
        {
            folder = folder.GetAbsolutePath();
        }

        // Folders must end in a slash
        if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            folder += Path.DirectorySeparatorChar;
        }

        var folderUri = new Uri(folder);
        Uri relativeUri = folderUri.MakeRelativeUri(pathUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());
        return relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }
}
