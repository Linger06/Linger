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

        if (nonNullPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            return true;

        if (Path.DirectorySeparatorChar == '\\')
        {
#if NET8_0_OR_GREATER
            if (nonNullPath.AsSpan().ContainsAny(s_windowsInvalidChars))
                return true;

            ReadOnlySpan<char> pathSpan = nonNullPath.AsSpan();
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
            if (nonNullPath.IndexOfAny(s_windowsInvalidChars) != -1)
                return true;

            int lastIdx = 0;
            while (lastIdx < nonNullPath.Length)
            {
                int nextSep = nonNullPath.IndexOfAny(PathHelper.PathSeparators, lastIdx);
                int len = (nextSep == -1 ? nonNullPath.Length : nextSep) - lastIdx;

                if (len > 0)
                {
                    string segment = nonNullPath.Substring(lastIdx, len);
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
    public static string ToFullPath(this string? relativePath, string? basePath = null, bool preserveEndingSeparator = false)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return relativePath ?? string.Empty;
        }

        string pathToResolve = relativePath!;

        try
        {
            if (PathHelper.ContainsInvalidPathChars(pathToResolve))
            {
                throw new IOException($"Invalid local path characters found: {pathToResolve}");
            }

            string resolvedPath;

#if NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET
            basePath ??= Environment.CurrentDirectory;
            resolvedPath = Path.GetFullPath(pathToResolve, basePath);
#else
            // Older targets need a little extra logic to avoid odd rooted-path behavior.
            if (pathToResolve.IsStrictAbsolutePath())
            {
                resolvedPath = Path.GetFullPath(pathToResolve);
            }
            else
            {
                basePath ??= Environment.CurrentDirectory;

                // Preserve the drive root when the relative path starts with a single slash.
                if (pathToResolve.Length > 0 && (pathToResolve[0] == '\\' || pathToResolve[0] == '/'))
                {
                    var baseRoot = Path.GetPathRoot(basePath) ?? string.Empty;
                    var trimmedPath = pathToResolve.Substring(1);
                    resolvedPath = Path.GetFullPath(Path.Combine(baseRoot, trimmedPath));
                }
                else
                {
                    resolvedPath = Path.GetFullPath(Path.Combine(basePath, pathToResolve));
                }
            }
#endif

            return PathHelper.HandleEndingSeparator(resolvedPath, preserveEndingSeparator);
        }
        catch (Exception ex) when (PathHelper.IsPathException(ex))
        {
            throw new ArgumentException(
                $"Failed to resolve local absolute path. Base: {basePath ?? "<null>"}, Relative: {relativePath ?? "<null>"}",
                nameof(relativePath),
                ex);
        }
    }

    /// <summary>
    /// Walks up the directory tree without performing file-system discovery.
    /// </summary>
    public static string GetParentDirectory(this string path, int levels)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        levels = Math.Abs(levels);
        if (levels == 0) return path;

        try
        {
            string currentPath = path.ToFullPath();
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
        catch (Exception ex) when (PathHelper.IsPathException(ex))
        {
            return path;
        }
    }

    /// <summary>
    /// Computes a relative path from one local path to another.
    /// </summary>
    /// <param name="relativeTo">Base path used as the origin.</param>
    /// <param name="path">Target path.</param>
    /// <returns>The relative path, "." for identical paths, or an empty string when the target input is blank.</returns>
    /// <exception cref="ArgumentException">Thrown when the base path is blank or path normalization fails.</exception>
    public static string GetRelativePath(this string relativeTo, string path)
    {
        if (string.IsNullOrWhiteSpace(relativeTo))
            throw new ArgumentException("Local base path cannot be null or empty", nameof(relativeTo));

        if (string.IsNullOrWhiteSpace(path))
            return path ?? string.Empty;

        relativeTo = relativeTo.ToFullPath();
        path = path.ToFullPath();

        if (string.Equals(path, relativeTo, PathHelper.PathComparison))
            return ".";

        try
        {
#if NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET
            return Path.GetRelativePath(relativeTo, path);
#else
            const string sep = "\\";

            // Normalize both paths before using Uri-based relative path calculation.
            string cleanFrom = relativeTo.Replace('/', '\\');
            string cleanTo = path.Replace('/', '\\');

            if (!cleanFrom.EndsWith(sep)) cleanFrom += sep;

            bool isToDirectory = cleanTo.EndsWith(sep) || !Path.HasExtension(cleanTo);
            if (isToDirectory && !cleanTo.EndsWith(sep))
            {
                cleanTo += sep;
            }

            var fromUri = new Uri(cleanFrom);
            var toUri = new Uri(cleanTo);
            if (fromUri.Scheme != toUri.Scheme) return path;

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            string result = relativePath.Replace('/', '\\');
            if (result.Length > 1 && result.EndsWith(sep))
            {
                result = result.TrimEnd('\\');
            }

            return string.IsNullOrEmpty(result) ? "." : result;
#endif
        }
        catch (Exception ex) when (PathHelper.IsPathException(ex))
        {
            throw new ArgumentException($"Invalid local path for relative calculation. Base: {relativeTo}, Target: {path}", ex);
        }
    }

    /// <summary>
    /// Returns true when the local file or directory exists and the path is valid.
    /// </summary>
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
    public static bool IsStrictAbsolutePath(this string path)
    {
        if (!Path.IsPathRooted(path)) return false;

        var root = Path.GetPathRoot(path);
        if (string.IsNullOrEmpty(root)) return false;

        // On Unix-like systems, a single leading slash is a local absolute path.
        if (Path.DirectorySeparatorChar == '/')
        {
            return path.Length > 0 && path[0] == '/' && (path.Length == 1 || path[1] != '/');
        }

        // On Windows, accept drive-rooted and device-prefixed local paths only.
        if (root.Length >= 2 && char.IsLetter(root[0]) && root[1] == ':')
        {
            return root.Length == 2 || root[2] == '\\' || root[2] == '/';
        }

        if (root.StartsWith(@"\\?\", StringComparison.Ordinal) || root.StartsWith(@"\\.\", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
