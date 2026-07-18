using Linger.Helper;

#if NET
using System.Buffers;
#endif

namespace Linger.Extensions.IO;

/// <summary>
/// Public path helpers built on top of the local file-system rules.
/// </summary>
public static partial class PathExtensions
{
#if NET8_0_OR_GREATER
    private static readonly SearchValues<char> s_windowsInvalidChars = SearchValues.Create("*?\"<>|");
#else
    private static readonly char[] s_windowsInvalidChars = new[] { '*', '?', '"', '<', '>', '|' };
#endif

    private static readonly HashSet<string> s_windowsReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    /// <summary>
    /// Checks whether the path contains invalid characters or reserved Windows device names.
    /// </summary>
    public static bool ContainsInvalidPathChars(this string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        string nonNullPath = path!;
        string pathToValidate = nonNullPath;

        if (Path.DirectorySeparatorChar == '\\' &&
            (nonNullPath.StartsWith(@"\\?\", StringComparison.Ordinal) ||
             nonNullPath.StartsWith(@"\\.\", StringComparison.Ordinal)))
        {
            // The question mark or dot is part of the Windows device-path prefix.
            pathToValidate = nonNullPath.Substring(4);
        }

        if (pathToValidate.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            return true;

        if (Path.DirectorySeparatorChar == '\\')
        {
#if NET8_0_OR_GREATER
            if (pathToValidate.AsSpan().ContainsAny(s_windowsInvalidChars))
                return true;

            ReadOnlySpan<char> pathSpan = pathToValidate.AsSpan();
            while (!pathSpan.IsEmpty)
            {
                int separatorIndex = pathSpan.IndexOfAny('/', '\\');
                ReadOnlySpan<char> segment = separatorIndex == -1 ? pathSpan : pathSpan.Slice(0, separatorIndex);

                if (!segment.IsEmpty)
                {
                    int firstDot = segment.IndexOf('.');
                    ReadOnlySpan<char> nameToCheck = firstDot == -1 ? segment : segment.Slice(0, firstDot);
                    nameToCheck = nameToCheck.TrimEnd(' ');

                    if (nameToCheck.Length >= 3 && nameToCheck.Length <= 4)
                    {
                        if (s_windowsReservedNames.Contains(nameToCheck.ToString()))
                            return true;
                    }
                }

                if (separatorIndex == -1) break;
                pathSpan = pathSpan.Slice(separatorIndex + 1);
            }
#else
            if (pathToValidate.IndexOfAny(s_windowsInvalidChars) != -1)
                return true;

            int lastIdx = 0;
            while (lastIdx < pathToValidate.Length)
            {
                int nextSep = pathToValidate.IndexOfAny(PathHelper.PathSeparators, lastIdx);
                int len = (nextSep == -1 ? pathToValidate.Length : nextSep) - lastIdx;

                if (len > 0)
                {
                    string segment = pathToValidate.Substring(lastIdx, len);
                    int firstDot = segment.IndexOf('.');
                    string nameToCheck = firstDot != -1 ? segment.Substring(0, firstDot) : segment;
                    nameToCheck = nameToCheck.TrimEnd(' ');

                    if (s_windowsReservedNames.Contains(nameToCheck))
                        return true;
                }

                if (nextSep == -1) break;
                lastIdx = nextSep + 1;
            }
#endif
        }
        else
        {
            if (nonNullPath.Contains('\0'))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves a path to an absolute local path and normalizes trailing separators.
    /// </summary>
    /// <param name="relativePath">The relative or absolute local path to resolve.</param>
    /// <param name="basePath">
    /// The base path used to resolve a relative path. A relative base path is resolved against
    /// <see cref="Environment.CurrentDirectory"/>. When null, <see cref="Environment.CurrentDirectory"/> is used directly.
    /// </param>
    /// <param name="includeTrailingSeparator">
    /// <see langword="true"/> to include a trailing directory separator in the result;
    /// otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The resolved absolute local path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="relativePath"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="relativePath"/> or <paramref name="basePath"/> is empty, whitespace, or otherwise invalid.
    /// </exception>
    /// <example>
    /// <code>
    /// string fullPath = "logs/app.log".ToFullPath("data", includeTrailingSeparator: false);
    /// </code>
    /// </example>
    public static string ToFullPath(this string relativePath, string? basePath = null, bool includeTrailingSeparator = false)
    {
        ArgumentNullException.ThrowIfNull(relativePath);

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Path cannot be empty or whitespace.", nameof(relativePath));
        }

        if (basePath is not null && string.IsNullOrWhiteSpace(basePath))
        {
            throw new ArgumentException("Base path cannot be empty or whitespace.", nameof(basePath));
        }

        string resolvedBasePath;
        try
        {
            if (basePath is not null && basePath.ContainsInvalidPathChars())
            {
                throw new IOException($"Invalid local base path characters found: {basePath}");
            }

            resolvedBasePath = basePath is null
                ? Environment.CurrentDirectory
                : Path.GetFullPath(basePath);
        }
        catch (Exception ex) when (PathHelper.IsPathException(ex))
        {
            throw new ArgumentException(
                $"Failed to resolve the base path: {basePath}",
                nameof(basePath),
                ex);
        }

        string pathToResolve = relativePath;

        try
        {
            if (pathToResolve.ContainsInvalidPathChars())
            {
                throw new IOException($"Invalid local path characters found: {pathToResolve}");
            }

            string resolvedPath;

#if NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET
            resolvedPath = Path.GetFullPath(pathToResolve, resolvedBasePath);
#else
            // Older targets need a little extra logic to avoid odd rooted-path behavior.
            if (PathHelper.IsPathFullyQualified(pathToResolve))
            {
                resolvedPath = Path.GetFullPath(pathToResolve);
            }
            else if (Path.IsPathRooted(pathToResolve))
            {
                // Preserve the drive root when the relative path starts with a single slash.
                if (pathToResolve.Length > 0 && (pathToResolve[0] == '\\' || pathToResolve[0] == '/'))
                {
                    var baseRoot = Path.GetPathRoot(resolvedBasePath) ?? string.Empty;
                    var trimmedPath = pathToResolve.Substring(1);
                    resolvedPath = Path.GetFullPath(Path.Combine(baseRoot, trimmedPath));
                }
                else
                {
                    resolvedPath = Path.GetFullPath(pathToResolve);
                }
            }
            else
            {
                resolvedPath = Path.GetFullPath(Path.Combine(resolvedBasePath, pathToResolve));
            }
#endif

            return PathHelper.HandleEndingSeparator(resolvedPath, includeTrailingSeparator);
        }
        catch (Exception ex) when (PathHelper.IsPathException(ex))
        {
            throw new ArgumentException(
                $"Failed to resolve local absolute path. Base: {resolvedBasePath}, Relative: {relativePath}",
                nameof(relativePath),
                ex);
        }
    }

    /// <summary>
    /// Resolves the path to an absolute path and walks up the directory tree without file-system discovery.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="levels"/> is negative.</exception>
    public static string GetParentDirectory(this string path, int levels)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        if (levels < 0)
            throw new ArgumentOutOfRangeException(nameof(levels), levels, "Levels cannot be negative.");

        string currentPath = path.ToFullPath();
        if (levels == 0) return currentPath;

        string rootPath = Path.GetPathRoot(currentPath) ?? string.Empty;

        for (var i = 0; i < levels; i++)
        {
            if (string.Equals(currentPath, rootPath, PathHelper.PathComparison))
                break;

            var parent = Path.GetDirectoryName(currentPath);
            if (parent == null)
                break;

            currentPath = parent;
        }

        return currentPath;
    }

    /// <summary>
    /// Computes a relative path from one local path to another.
    /// </summary>
    /// <param name="relativeTo">Base path used as the origin.</param>
    /// <param name="path">Target path.</param>
    /// <returns>The relative path, or <c>.</c> when both paths resolve to the same location.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="relativeTo"/> or <paramref name="path"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="relativeTo"/> or <paramref name="path"/> is empty, whitespace, or otherwise invalid.
    /// </exception>
    /// <example>
    /// <code>
    /// string relativePath = "/var/app".GetRelativePath("/var/app/logs/app.log");
    /// // relativePath: "logs/app.log"
    /// </code>
    /// </example>
    public static string GetRelativePath(this string relativeTo, string path)
    {
        ArgumentNullException.ThrowIfNull(relativeTo);
        ArgumentNullException.ThrowIfNull(path);

        if (string.IsNullOrWhiteSpace(relativeTo))
        {
            throw new ArgumentException("Local base path cannot be empty or whitespace.", nameof(relativeTo));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Target path cannot be empty or whitespace.", nameof(path));
        }

        try
        {
            relativeTo = relativeTo.ToFullPath();
        }
        catch (Exception ex) when (PathHelper.IsPathException(ex))
        {
            throw new ArgumentException($"Invalid local path for relative calculation. Base: {relativeTo}, Target: {path}", nameof(relativeTo), ex);
        }

        try
        {
            path = path.ToFullPath();
        }
        catch (Exception ex) when (PathHelper.IsPathException(ex))
        {
            throw new ArgumentException($"Invalid local path for relative calculation. Base: {relativeTo}, Target: {path}", nameof(path), ex);
        }

        try
        {
            if (string.Equals(path, relativeTo, PathHelper.PathComparison))
                return ".";

#if NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET
            return Path.GetRelativePath(relativeTo, path);
#else
            return PathHelper.GetRelativePathFallback(relativeTo, path);
#endif
        }
        catch (Exception ex) when (PathHelper.IsPathException(ex))
        {
            throw new ArgumentException(
                $"Invalid local path for relative calculation. Base: {relativeTo}, Target: {path}",
                nameof(path),
                ex);
        }
    }

    /// <summary>
    /// Returns true when the requested local file-system entry exists and the path is valid.
    /// </summary>
    /// <param name="path">The local path to check.</param>
    /// <param name="checkAsFile">True to check for a file; false to check for a directory.</param>
    public static bool Exists(string path, bool checkAsFile = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (path.Contains('\0'))
                return false;

            var fullPath = Path.GetFullPath(path);
            if (fullPath.ContainsInvalidPathChars())
                return false;

            return checkAsFile ? File.Exists(fullPath) : Directory.Exists(fullPath);
        }
        catch (Exception ex) when (PathHelper.IsPathException(ex))
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether the path is a strict local absolute path, excluding classic UNC shares.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> when the path is fully qualified and is not a classic UNC share;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// bool isAbsolute = @"C:\data\file.txt".IsStrictAbsolutePath();
    /// </code>
    /// </example>
    [Obsolete("Use Path.IsPathFullyQualified and apply an explicit UNC policy instead. This compatibility helper will be removed in the next major version.")]
    public static bool IsStrictAbsolutePath(this string path)
    {
        if (!PathHelper.IsPathFullyQualified(path))
        {
            return false;
        }

        if (Path.DirectorySeparatorChar != '\\')
        {
            return true;
        }

        if (path.StartsWith(@"\\?\", StringComparison.Ordinal) ||
            path.StartsWith(@"\\.\", StringComparison.Ordinal))
        {
            return true;
        }

        return !path.StartsWith(@"\\", StringComparison.Ordinal);
    }
}
