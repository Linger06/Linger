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
        if (path is null or "")
        {
            return false;
        }

        string pathToValidate = path;

        if (Path.DirectorySeparatorChar == '\\' &&
            (path.StartsWith(@"\\?\", StringComparison.Ordinal) ||
             path.StartsWith(@"\\.\", StringComparison.Ordinal)))
        {
            // The question mark or dot is part of the Windows device-path prefix.
            pathToValidate = path.Substring(4);
        }

        if (pathToValidate.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        {
            return true;
        }

        if (Path.DirectorySeparatorChar == '\\')
        {
            return ContainsWindowsInvalidChars(pathToValidate)
                || ContainsWindowsReservedName(pathToValidate);
        }

        return path.Contains('\0');
    }

    private static bool ContainsWindowsInvalidChars(string path)
    {
#if NET8_0_OR_GREATER
        return path.AsSpan().ContainsAny(s_windowsInvalidChars);
#else
        return path.IndexOfAny(s_windowsInvalidChars) != -1;
#endif
    }

    private static bool ContainsWindowsReservedName(string path)
    {
        var segmentStart = 0;
        while (segmentStart < path.Length)
        {
            var separatorIndex = path.IndexOfAny(PathHelper.PathSeparators, segmentStart);
            var segmentEnd = separatorIndex == -1 ? path.Length : separatorIndex;
            var nameEnd = path.IndexOf('.', segmentStart, segmentEnd - segmentStart);
            if (nameEnd == -1)
            {
                nameEnd = segmentEnd;
            }

            while (nameEnd > segmentStart && path[nameEnd - 1] == ' ')
            {
                nameEnd--;
            }

            var nameLength = nameEnd - segmentStart;
            if (nameLength is >= 3 and <= 4
                && s_windowsReservedNames.Contains(path.Substring(segmentStart, nameLength)))
            {
                return true;
            }

            if (separatorIndex == -1)
            {
                break;
            }

            segmentStart = separatorIndex + 1;
        }

        return false;
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

}
