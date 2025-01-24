using System.Diagnostics.CodeAnalysis;
using System.Security;
using Linger.Extensions.Core;

namespace Linger.Helper;

public static class PathHelper
{
    private static readonly HashSet<string> WindowsReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    private static readonly char[] WindowsInvalidChars = ['*', '?', '"', '<', '>', '|'];

    public static string ProcessPath(string? basePath, string? relativePath, bool preserveEndingSeparator = false)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return relativePath ?? string.Empty;

        try
        {
            // 如果是特殊路径，直接标准化处理
            if (IsSpecialPath(relativePath))
                return NormalizePath(relativePath, preserveEndingSeparator);

            // 如果是绝对路径，直接标准化
            if (Path.IsPathRooted(relativePath))
                return NormalizePath(relativePath, preserveEndingSeparator);

            // 处理基础路径
            basePath ??= Environment.CurrentDirectory;
            if (!Path.IsPathRooted(basePath))
                throw new ArgumentException("Base path must be absolute", nameof(basePath));

            // 组合并标准化路径
            var combinedPath = Path.Combine(basePath, relativePath);
            return NormalizePath(combinedPath, preserveEndingSeparator);
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            throw new ArgumentException(
                $"Invalid path. Base: {basePath}, Relative: {relativePath}", ex);
        }
    }

    /// <summary>
    /// 获取相对路径
    /// </summary>
    public static string GetRelativePath(string path, string basePath)
    {
        // 标准化两个路径
        path = NormalizePath(path);
        basePath = NormalizePath(basePath, true);

        try
        {
#if NETCOREAPP
            return Path.GetRelativePath(basePath, path);
#else
            var pathUri = new Uri(path);
            var baseUri = new Uri(basePath);
            var relativeUri = baseUri.MakeRelativeUri(pathUri);
            return Uri.UnescapeDataString(relativeUri.ToString())
                     .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
#endif
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            throw new ArgumentException($"Invalid path. Path: {path}, Base: {basePath}", ex);
        }
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static string? NormalizePath(string? path, bool preserveEndingSeparator = false)
    {
        if (path.IsNullOrWhiteSpace())
            return path ?? string.Empty;

        // 处理网络路径和特殊前缀
        if (IsSpecialPath(path))
        {
            return preserveEndingSeparator
                ? path
                : path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        try
        {
            var normalizedPath = StandardizePath(path);
            return preserveEndingSeparator
                ? normalizedPath + Path.DirectorySeparatorChar
                : normalizedPath;
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            return preserveEndingSeparator
                ? path + Path.DirectorySeparatorChar
                : path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }

    /// <summary>
    /// 检查是否为特殊路径格式
    /// </summary>
    private static bool IsSpecialPath(string path) =>
        path.StartsWith("""\\""", StringComparison.Ordinal) ||
        path.StartsWith("//", StringComparison.Ordinal) ||
        path.StartsWith("/", StringComparison.Ordinal) ||
        path.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("ftps://", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("sftp://", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 标准化路径格式
    /// </summary>
    private static string StandardizePath(string path)
    {
        // 对于标准路径,直接使用 GetFullPath
        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        // 处理 file:// 格式
        if (path.Contains("file://"))
        {
            var localPath = new Uri(path).LocalPath;
            if (!string.IsNullOrEmpty(localPath))
            {
                return Path.GetFullPath(localPath)
                          .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }

        // 其他情况尝试直接规范化
        return Path.GetFullPath(path)
                  .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public static bool IsPathException(Exception ex) =>
        ex is UriFormatException ||
        ex is ArgumentException ||
        ex is SecurityException ||
        ex is NotSupportedException ||
        ex is PathTooLongException ||
        ex is IOException;

    public static string NormalizePathEndingDirectorySeparator(string directory)
    {
        if (directory.IsNullOrWhiteSpace())
            return directory;

        var normalizedPath = NormalizePath(directory);
        return normalizedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
    }

    /// <summary>
    /// 统一的文件/目录存在性检查
    /// </summary>
    public static bool Exists(string path, bool checkAsFile = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var fullPath = Path.GetFullPath(path);

            if (ContainsInvalidPathChars(fullPath))
                return false;

            return checkAsFile ?
                   File.Exists(path) :
                   Directory.Exists(path);
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            return false;
        }
    }

    /// <summary>
    /// 检查路径中是否包含非法字符
    /// </summary>
    private static bool ContainsInvalidPathChars(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        // 系统定义的非法字符
        if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            return true;

        bool isWindows = OSPlatformHelper.IsWindows;
        if (isWindows)
        {
            // Windows 特定的检查
            // 检查Windows保留字符
            if (path.IndexOfAny(WindowsInvalidChars) != -1)
                return true;

            // 检查每个路径段
            var segments = path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var segment in segments)
            {
                string upperSegment = segment.ToUpperInvariant();
                if (WindowsReservedNames.Any(name =>
                    upperSegment.Equals(name, StringComparison.Ordinal) ||
                    upperSegment.StartsWith(name + ".", StringComparison.Ordinal)))
                {
                    return true;
                }
            }
        }
        else
        {
            // Unix/Linux 特定的检查
            if (path.Contains('\0')) // 空字符在Unix系统中不允许
                return true;

            // Unix系统中文件名不能为 "." 或 ".."
            var fileName = Path.GetFileName(path);
            if (fileName == "." || fileName == "..")
                return true;
        }

        return false;
    }

    public static bool PathEquals(string? path1, string? path2, bool ignoreCase = true)
    {
        if (path1 == null || path2 == null)
            return path1 == path2;

        try
        {
            // 尝试获取完整路径
            var fullPath1 = NormalizePath(path1);
            var fullPath2 = NormalizePath(path2);

            var comparison = ignoreCase ?
                StringComparison.OrdinalIgnoreCase :
                StringComparison.Ordinal;

            // 首先比较标准化路径
            if (string.Equals(fullPath1, fullPath2, comparison))
                return true;

            // 如果标准化路径不相等，尝试解析为绝对路径再比较
            if (Path.IsPathRooted(fullPath1) && Path.IsPathRooted(fullPath2))
            {
                var absolutePath1 = Path.GetFullPath(fullPath1);
                var absolutePath2 = Path.GetFullPath(fullPath2);
                return string.Equals(absolutePath1, absolutePath2, comparison);
            }

            return false;
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            // 如果路径解析失败，回退到简单字符串比较
            return string.Equals(path1, path2,
                ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
    }

    public static string? GetParentDirectory(string? path, int levels)
    {
        path.EnsureIsNotNullAndWhiteSpace();

        levels = Math.Abs(levels);
        if (levels == 0) return path;

        var info = Directory.GetParent(path);
        if (info == null) return path;
        path = info.FullName;

        --levels;

        return levels >= 1 ? GetParentDirectory(path, levels) : path;
    }
}