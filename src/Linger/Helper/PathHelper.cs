using System.Diagnostics.CodeAnalysis;
using System.Security;
using Linger.Extensions.Core;

namespace Linger.Helper;

public static class PathHelper
{
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
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
            if (path.Contains("file://") || path.Contains("://"))
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
        if (string.IsNullOrEmpty(directory))
            return directory;
        try
        {
            string normalizedPath = NormalizePath(directory);

            if (!normalizedPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                return normalizedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            }

            return normalizedPath;
        }
        catch (Exception ex) when (ex is ArgumentException || ex is SecurityException)
        {
            return directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }
    }

    public static bool IsFile([NotNull] string path)
    {
        if (path == null)
            throw new ArgumentNullException(nameof(path));

        if (path.IsNullOrWhiteSpace())
            return false;

        try
        {
            // 获取完整路径，处理相对路径情况
            string fullPath = Path.GetFullPath(path);

            // 优先检查文件是否存在
            if (!File.Exists(fullPath))
                return false;

            // 获取文件属性并检查是否为目录
            FileAttributes attrs = File.GetAttributes(fullPath);
            return !attrs.HasFlag(FileAttributes.Directory);
        }
        catch (Exception ex) when (
            ex is SecurityException ||     // 没有访问权限
            ex is IOException ||           // IO 错误
            ex is NotSupportedException || // 路径格式无效
            ex is PathTooLongException ||  // 路径太长
            ex is ArgumentException)       // 路径包含无效字符
        {
            return false;
        }
    }

    public static bool IsDirectory([NotNull] string path)
    {
        if (path == null)
            throw new ArgumentNullException(nameof(path));

        if (path.IsNullOrWhiteSpace())
            return false;

        try
        {
            // 获取完整路径，处理相对路径情况
            string fullPath = Path.GetFullPath(path);

            // 检查路径是否存在
            if (Directory.Exists(fullPath))
                return true;

            // 如果是文件则直接返回 false
            if (File.Exists(fullPath))
                return false;

            // 对于不存在的路径，通过以下方式判断：
            // 1. 检查是否有扩展名
            // 2. 检查路径末尾是否有目录分隔符
            return string.IsNullOrEmpty(Path.GetExtension(fullPath)) ||
                   fullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
                   fullPath.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal);
        }
        catch (Exception ex) when (
            ex is SecurityException ||    // 没有访问权限
            ex is NotSupportedException || // 路径格式无效
            ex is PathTooLongException || // 路径太长
            ex is ArgumentException)      // 路径包含无效字符
        {
            return false;
        }
    }
}