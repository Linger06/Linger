using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;
using Linger.Extensions.Core;

namespace Linger.Helper;

public static class PathHelper
{
    [return: NotNullIfNotNull(nameof(path))]
    public static string? NormalizePath(string? path)
    {
        if (path.IsNullOrWhiteSpace())
            return path;

        // 处理网络路径和特殊前缀
        if (path.StartsWith("""\\""", StringComparison.Ordinal)
            || path.StartsWith("//", StringComparison.Ordinal)
            || path.StartsWith("/", StringComparison.Ordinal)
            /*|| FtpHelpers.IsFtpPath(path)*/
            )
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // 确保路径以目录分隔符结尾
        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            path += Path.DirectorySeparatorChar;
        }

        try
        {
            // 对于标准路径，直接使用 Path.GetFullPath
            if (Path.IsPathRooted(path))
            {
                return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            // 对于特殊格式路径，使用 Uri
            if (path.Contains("file://"))
            {
                var pathUri = new Uri(path).LocalPath;
                if (string.IsNullOrEmpty(pathUri))
                    return path;

                return Path.GetFullPath(pathUri).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            // 其他情况尝试直接规范化
            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch (Exception ex)
        {
            if (ex is UriFormatException || ex is ArgumentException ||
             ex is SecurityException || ex is NotSupportedException)
            {
                // 如果转换失败，保持原始路径
                return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            throw;
        }
    }

    public static string NormalizePathEndingDirectorySeparator(string directory)
    {
        if (directory.IsNullOrWhiteSpace())
            return directory;

        var normalizedPath = NormalizePath(directory);
        return normalizedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
    }

    public static bool IsFile([NotNull] string path)
    {
        if (path == null)
            throw new ArgumentNullException(nameof(path));

        if (path.IsNullOrWhiteSpace())
            return false;

        try
        {
            // 获取完整路径并规范化
            string fullPath = Path.GetFullPath(path);

            // 检查特殊字符（包括Windows保留名称）
            if (ContainsInvalidPathChars(fullPath))
                return false;

            // 使用 FileInfo 可以一次性获取所有需要的信息
            var fileInfo = new FileInfo(fullPath);
            return fileInfo.Exists && !fileInfo.Attributes.HasFlag(FileAttributes.Directory);
        }
        catch (Exception ex) when (
            ex is SecurityException ||
            ex is IOException ||
            ex is NotSupportedException ||
            ex is PathTooLongException ||
            ex is ArgumentException)
        {
            return false;
        }
    }

    public static bool IsDirectory([NotNull] string path)
    {
        if (path == null)
            throw new ArgumentNullException(nameof(path));

        if (path.IsWhiteSpace())
            return false;

        try
        {
            // 获取完整路径并规范化
            string fullPath = Path.GetFullPath(path);

            // 检查特殊字符（包括Windows保留名称）
            if (ContainsInvalidPathChars(fullPath))
                return false;

            // 使用 DirectoryInfo 可以一次性获取所有需要的信息
            var dirInfo = new DirectoryInfo(fullPath);
            if (dirInfo.Exists)
                return true;

            // 如果不存在，进行额外的启发式检查
            bool hasDirectorySeparatorAtEnd = fullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
                                            fullPath.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal);

            // 如果存在同名文件，则不是目录
            if (!hasDirectorySeparatorAtEnd && File.Exists(fullPath))
                return false;

            // 目录通常没有扩展名
            return hasDirectorySeparatorAtEnd || string.IsNullOrEmpty(Path.GetExtension(fullPath));
        }
        catch (Exception ex) when (
            ex is SecurityException ||
            ex is NotSupportedException ||
            ex is PathTooLongException ||
            ex is ArgumentException)
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

        bool isWindows = IsWindowsOperatingSystem();
        if (isWindows)
        {
            // Windows 特定的检查
            // 检查Windows保留字符
            if (path.IndexOfAny(new[] { '*', '?', '"', '<', '>', '|' }) != -1)
                return true;

            // 检查Windows保留名称
            string[] windowsReservedNames = {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

            // 检查每个路径段
            var segments = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var segment in segments)
            {
                string upperSegment = segment.ToUpperInvariant();
                if (windowsReservedNames.Any(name =>
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

    /// <summary>
    /// Determines whether two paths are equivalent.
    /// </summary>
    public static bool PathEquals(string path1, string path2)
    {
        if (path1 == null || path2 == null)
            return path1 == path2;

        try
        {
            // 标准化两个路径
            var normalizedPath1 = NormalizePath(path1);
            var normalizedPath2 = NormalizePath(path2);

            // 获取完整路径
            string fullPath1 = Path.GetFullPath(normalizedPath1);
            string fullPath2 = Path.GetFullPath(normalizedPath2);

            // 检查操作系统类型
            bool isWindows = IsWindowsOperatingSystem();

            // 在 Windows 上忽略大小写，在其他平台上区分大小写
            return isWindows
                ? string.Equals(fullPath1, fullPath2, StringComparison.OrdinalIgnoreCase)
                : string.Equals(fullPath1, fullPath2, StringComparison.Ordinal);
        }
        catch (Exception ex)
        {
            if (ex is ArgumentException || ex is SecurityException ||
                ex is NotSupportedException || ex is PathTooLongException)
            {
                // 如果无法解析路径，则进行简单的字符串比较
                bool isWindows = IsWindowsOperatingSystem();
                return isWindows
                    ? string.Equals(path1, path2, StringComparison.OrdinalIgnoreCase)
                    : string.Equals(path1, path2, StringComparison.Ordinal);
            }
            throw;
        }
    }

    /// <summary>
    /// Determines whether the current operating system is Windows.
    /// </summary>
    private static bool IsWindowsOperatingSystem()
    {
#if NETFRAMEWORK
        // .NET Framework 4.0 方式
        OperatingSystem os = Environment.OSVersion;
        return os.Platform == PlatformID.Win32NT;
#else
        // 更新的 .NET 版本使用 RuntimeInformation
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
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