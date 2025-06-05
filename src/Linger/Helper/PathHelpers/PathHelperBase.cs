using System.Text;

namespace Linger.Helper.PathHelpers;

/// <summary>
/// 路径处理基础类，提供共享功能
/// </summary>
public abstract class PathHelperBase
{
    // 缓存常用值，减少重复计算
    protected static readonly char PlatformSeparator = Path.DirectorySeparatorChar;
    protected static readonly char[] PathSeparators = ['/', '\\'];
    protected const StringComparison PathComparison = StringComparison.OrdinalIgnoreCase;
    protected static readonly string SingleSeparator = PlatformSeparator.ToString();

    /// <summary>
    /// 检查是否为路径相关异常
    /// </summary>
    protected static bool IsPathException(Exception ex) =>
        ex is UriFormatException ||
        ex is ArgumentException ||
        ex is System.Security.SecurityException ||
        ex is NotSupportedException ||
        ex is PathTooLongException ||
        ex is IOException;

    /// <summary>
    /// 删除路径中多余的连续分隔符
    /// </summary>
    protected static string RemoveConsecutiveSeparators(string path)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
            return string.Empty;

        // 优化：检查是否需要处理连续分隔符
        bool hasConsecutiveSeparators = false;
        for (int i = 0; i < path.Length - 1; i++)
        {
            if (Array.IndexOf(PathSeparators, path[i]) >= 0 && Array.IndexOf(PathSeparators, path[i + 1]) >= 0)
            {
                hasConsecutiveSeparators = true;
                break;
            }
        }

        if (!hasConsecutiveSeparators)
            return path;

        StringBuilder result = new(path.Length);
        bool lastWasSeparator = false;

        foreach (char c in path)
        {
            bool isSeparator = Array.IndexOf(PathSeparators, c) >= 0;
            if (!(isSeparator && lastWasSeparator))
                result.Append(c);

            lastWasSeparator = isSeparator;
        }

        return result.ToString();
    }

    /// <summary>
    /// 处理路径末尾分隔符
    /// </summary>
    protected static string HandleEndingSeparator(string path, bool preserveEndingSeparator, int minLength = 0)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        var trimmedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (preserveEndingSeparator)
            return trimmedPath + SingleSeparator;

        return trimmedPath.Length > minLength ? trimmedPath : path;
    }

    /// <summary>
    /// 将路径拆分为段
    /// </summary>
    protected static string[] SplitPath(string path)
    {
        return path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// 统一路径分隔符为当前系统标准
    /// </summary>
    protected static string StandardizePathSeparators(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        return OSPlatformHelper.IsWindows ?
            path.Replace('/', '\\') :
            path.Replace('\\', '/');
    }

    /// <summary>
    /// 检查路径中是否包含非法字符
    /// </summary>
    protected static bool ContainsInvalidChars(string path, char[] invalidChars)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        foreach (var c in invalidChars)
        {
            if (path.Contains(c))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 基本路径标准化处理
    /// </summary>
    /// <param name="path">要标准化的路径</param>
    /// <param name="preserveEndingSeparator">是否保留末尾分隔符</param>
    /// <returns>标准化后的路径</returns>
    protected static string NormalizeBasicPath(string path, bool preserveEndingSeparator)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        try
        {
            // 标准化普通路径
            var standardPath = StandardizePathSeparators(path);
            standardPath = RemoveConsecutiveSeparators(standardPath);

            return HandleEndingSeparator(standardPath, preserveEndingSeparator);
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            // 错误情况下简化处理
            return HandleEndingSeparator(path, preserveEndingSeparator);
        }
    }

    /// <summary>
    /// 检查是否包含无效的路径字符（基础实现）
    /// </summary>
    protected static bool ContainsInvalidPathChars(string path)
    {
        // 基本实现：检查系统定义的非法字符
        return !string.IsNullOrEmpty(path) && path.IndexOfAny(Path.GetInvalidPathChars()) != -1;
    }
}
