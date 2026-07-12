using System.Text;

#if NET
using System.Buffers;
#endif

namespace Linger.Helper;

/// <summary>
/// Low-level helpers for path normalization and validation across platforms.
/// </summary>
public static class PathHelper
{
    // Native separator for the current platform.
    internal static readonly char PlatformSeparator = Path.DirectorySeparatorChar;

    // Alternate separator accepted by the current platform.
    internal static readonly char AltPlatformSeparator = Path.AltDirectorySeparatorChar;

    // Shared separator set used by framework-specific fallback code.
    internal static readonly char[] PathSeparators = new[] { '/', '\\' };

    // Match the host file system's case-sensitivity rules.
    internal static readonly StringComparison PathComparison =
        PlatformSeparator == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    // Cached string form of the native separator.
    internal static readonly string SingleSeparator = PlatformSeparator.ToString();

    /// <summary>
    /// Returns true when the exception belongs to path or file-system handling.
    /// </summary>
    internal static bool IsPathException(Exception ex) =>
        ex is ArgumentException ||
        ex is System.Security.SecurityException ||
        ex is NotSupportedException ||
        ex is PathTooLongException ||
        ex is IOException;

    /// <summary>
    /// Rewrites alternate separators to the native separator.
    /// </summary>
    internal static string StandardizePathSeparators(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        // Fast path: return the original string when no alternate separator exists.
        if (!path.Contains(AltPlatformSeparator))
        {
            return path;
        }

        return path.Replace(AltPlatformSeparator, PlatformSeparator);
    }

    /// <summary>
    /// Collapses repeated separators while preserving UNC and device prefixes.
    /// </summary>
    internal static string RemoveConsecutiveSeparators(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        // Leave URI-like strings alone; this helper is only for local paths.
#if NET
        if (path.Contains("://", StringComparison.Ordinal)) return path;
#else
        if (path.IndexOf("://", PathComparison) != -1) return path;
#endif

        // Fast path before allocating buffers.
        var hasConsecutiveSeparators = false;
        for (var i = 0; i < path.Length - 1; i++)
        {
            char current = path[i];
            char next = path[i + 1];
            if ((current == '/' || current == '\\') && (next == '/' || next == '\\'))
            {
                hasConsecutiveSeparators = true;
                break;
            }
        }

        if (!hasConsecutiveSeparators)
            return path;

        // Preserve a double-separator prefix for UNC and device paths.
        var startIndex = 0;
        bool isDoubleStart = path.Length > 2 &&
                             (path[0] == '/' || path[0] == '\\') &&
                             (path[1] == '/' || path[1] == '\\');

#if NET
        // Use pooled buffers on modern targets to avoid repeated allocations.
        int maxResultLength = path.Length;
        char[] rentedArray = ArrayPool<char>.Shared.Rent(maxResultLength);
        int pos = 0;

        try
        {
            if (isDoubleStart)
            {
                // Keep the leading UNC/device prefix intact.
                rentedArray[pos++] = PlatformSeparator;
                rentedArray[pos++] = PlatformSeparator;
                startIndex = 2;
            }

            var lastWasSeparator = false;
            for (var i = startIndex; i < path.Length; i++)
            {
                char c = path[i];
                var isSeparator = c == '/' || c == '\\';

                if (isSeparator)
                {
                    if (!lastWasSeparator)
                    {
                        rentedArray[pos++] = PlatformSeparator;
                    }
                    lastWasSeparator = true;
                }
                else
                {
                    rentedArray[pos++] = c;
                    lastWasSeparator = false;
                }
            }

            return new string(rentedArray, 0, pos);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedArray);
        }
#else
        // Framework fallback keeps behavior identical without ArrayPool.
        StringBuilder result = new StringBuilder(path.Length);
        if (isDoubleStart)
        {
            result.Append(PlatformSeparator);
            result.Append(PlatformSeparator);
            startIndex = 2;
        }

        var lastWasSeparator = false;
        for (var i = startIndex; i < path.Length; i++)
        {
            char c = path[i];
            var isSeparator = c == '/' || c == '\\';

            if (isSeparator)
            {
                if (!lastWasSeparator)
                {
                    result.Append(PlatformSeparator);
                }
                lastWasSeparator = true;
            }
            else
            {
                result.Append(c);
                lastWasSeparator = false;
            }
        }

        return result.ToString();
#endif
    }

    /// <summary>
    /// Applies trailing-separator policy without breaking root paths.
    /// </summary>
    internal static string HandleEndingSeparator(string path, bool preserveEndingSeparator, int minLength = 0)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        var trimmedPath = path.TrimEnd('/', '\\');
        var pathRoot = Path.GetPathRoot(path);

        if (!string.IsNullOrEmpty(pathRoot))
        {
            var trimmedRoot = pathRoot.TrimEnd('/', '\\');
            if (string.Equals(trimmedPath, trimmedRoot, PathComparison))
            {
                return pathRoot;
            }
        }

        if (preserveEndingSeparator)
        {
            if (trimmedPath.Length == 0)
            {
                return SingleSeparator;
            }
            return trimmedPath + SingleSeparator;
        }

        if (trimmedPath.Length == 0)
        {
            // Keep the Unix root path as "/".
            return SingleSeparator;
        }

        if (PlatformSeparator == '\\' && trimmedPath.Length == 2 && trimmedPath[1] == ':')
        {
            // Keep Windows drive roots in the canonical "C:\" shape.
            return trimmedPath + SingleSeparator;
        }

        return trimmedPath.Length > minLength ? trimmedPath : path;
    }

    /// <summary>
    /// Splits a path into non-empty segments.
    /// </summary>
    internal static string[] SplitPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return Array.Empty<string>();

#if NET
        // Newer targets can also trim segment whitespace during split.
        return path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
#else
        return path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
#endif
    }

    /// <summary>
    /// Normalizes separators and trailing slash policy without touching the file system.
    /// </summary>
    public static string CleanAndNormalizePureString(string path, bool preserveEndingSeparator)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var standardPath = StandardizePathSeparators(path);
        standardPath = RemoveConsecutiveSeparators(standardPath);

        return HandleEndingSeparator(standardPath, preserveEndingSeparator);
    }

}
