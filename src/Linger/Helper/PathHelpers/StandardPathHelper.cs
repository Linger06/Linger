using Linger.Extensions.Core;

namespace Linger.Helper.PathHelpers;

/// <summary>
/// 标准路径处理类，用于处理普通文件系统路径
/// </summary>
public class StandardPathHelper : PathHelperBase
{
#if NET8_0_OR_GREATER
    private static readonly System.Buffers.SearchValues<char> s_windowsInvalidChars = System.Buffers.SearchValues.Create("*?\"<>|");
#else
    private static readonly char[] s_windowsInvalidChars = ['*', '?', '"', '<', '>', '|'];
#endif

    private static readonly HashSet<string> s_windowsReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    /// <summary>
    /// 解析并生成绝对路径
    /// </summary>
    /// <param name="basePath">基础路径。如果未提供，则使用当前目录</param>
    /// <param name="relativePath">要处理的相对路径或绝对路径</param>
    /// <param name="preserveEndingSeparator">是否保留路径末尾的分隔符</param>
    /// <returns>标准化后的路径</returns>
    public static string ResolveToAbsolutePath(string? basePath, string? relativePath, bool preserveEndingSeparator = false)
    {
        // 检查路径是否为空
        if (relativePath.IsNullOrWhiteSpace())
            return relativePath ?? string.Empty;

        try
        {
            // 处理绝对路径和无效路径情况
            if (ContainsInvalidPathChars(relativePath))
                throw new IOException($"Invalid path: {relativePath}");

            if (Path.IsPathRooted(relativePath))
                return NormalizePath(relativePath, preserveEndingSeparator);

            // 处理基础路径和相对路径组合的情况
            basePath ??= Environment.CurrentDirectory;

            if (!Path.IsPathRooted(basePath))
                throw new ArgumentException("Base path must be absolute", nameof(basePath));

            return NormalizePath(Path.Combine(basePath, relativePath), preserveEndingSeparator);
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            throw new ArgumentException(
                $"Invalid path. Base: {basePath ?? "<null>"}, Relative: {relativePath ?? "<null>"}",
                nameof(relativePath),
                ex);
        }
    }

    /// <summary>
    /// 标准化路径
    /// </summary>
    [return: NotNullIfNotNull(nameof(path))]
    public static string? NormalizePath(string? path, bool preserveEndingSeparator = false)
    {
        if (path == null)
            return null;

        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        return NormalizeBasicPath(path, preserveEndingSeparator);
    }

    /// <summary>
    /// 判断两个路径是否相等
    /// </summary>
    public static bool PathEquals(string? path1, string? path2, bool ignoreCase = true)
    {
        if (path1 == null || path2 == null)
            return path1 == path2;

        try
        {
            var comparison = ignoreCase ? PathComparison : StringComparison.Ordinal;

            // 尝试标准化路径进行比较
            var fullPath1 = NormalizePath(path1);
            var fullPath2 = NormalizePath(path2);

            if (string.Equals(fullPath1, fullPath2, comparison))
                return true;

            // 转为绝对路径再比较
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
            // 异常时简单字符串比较
            return string.Equals(path1, path2,
                ignoreCase ? PathComparison : StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// 验证是否为Windows盘符
    /// </summary>
    public static bool IsWindowsDriveLetter(string? input)
    {
        if (input.IsNullOrEmpty() || input.Length < 2)
            return false;

        if (!char.IsLetter(input[0]) || input[1] != ':')
            return false;

        if (input.Length == 2)
            return true;

        if (input.Length > 2)
        {
            if (input[2] != '/' && input[2] != '\\')
                return false;

            if (input.Length > 3 && ContainsInvalidPathChars(input.Substring(3)))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 获取相对路径
    /// </summary>
    /// <param name="relativeTo">源路径，始终被视为目录</param>
    /// <param name="path">目标路径</param>
    /// <returns>从relativeTo到path的相对路径，如果无法计算则返回原路径</returns>
    public static string GetRelativePath(string relativeTo, string path)
    {
        if (relativeTo.IsNullOrWhiteSpace())
            throw new ArgumentException("Base path cannot be null or empty", nameof(relativeTo));

        if (path.IsNullOrWhiteSpace())
            return path ?? string.Empty;

        // 标准化路径
        path = NormalizePath(path);
        relativeTo = NormalizePath(relativeTo, true);

        if (PathEquals(path, relativeTo))
            return ".";

        try
        {
#if NETCOREAPP
            return Path.GetRelativePath(relativeTo, path);
#else
            return GetRelativePathCore(relativeTo, path);
#endif
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            throw new ArgumentException($"Invalid path. Path: {path}, Base: {relativeTo}", ex);
        }
    }

#if !NETCOREAPP
    // 在非.NET Core环境下的实现
    private static string GetRelativePathCore(string relativeTo, string path)
    {
        if (!Path.IsPathRooted(path))
            path = Path.GetFullPath(path);

        if (!Path.IsPathRooted(relativeTo))
            relativeTo = Path.GetFullPath(relativeTo);

        if (PathEquals(path, relativeTo))
            return ".";

        var pathRoot = Path.GetPathRoot(path) ?? string.Empty;
        var relativeToRoot = Path.GetPathRoot(relativeTo) ?? string.Empty;

        if (!string.Equals(pathRoot, relativeToRoot, PathComparison))
            return path;

        // 确保基路径以分隔符结尾
        if (!relativeTo.EndsWith(PlatformSeparator) &&
            !relativeTo.EndsWith(Path.AltDirectorySeparatorChar))
        {
            relativeTo += PlatformSeparator;
        }

        // 处理路径部分
        var relativeToWithoutRoot = relativeTo.Substring(relativeToRoot.Length);
        var pathWithoutRoot = path.Substring(pathRoot.Length);

        var fromParts = SplitPath(relativeToWithoutRoot);
        var toParts = SplitPath(pathWithoutRoot);

        // 找到共同前缀
        var commonLength = 0;
        var minLength = Math.Min(fromParts.Length, toParts.Length);

        for (var i = 0; i < minLength; i++)
        {
            if (string.Equals(fromParts[i], toParts[i], PathComparison))
                commonLength++;
            else
                break;
        }

        // 构建相对路径
        var result = new List<string>();

        // 上级路径部分
        for (var i = commonLength; i < fromParts.Length; i++)
        {
            result.Add("..");
        }

        // 目标路径部分
        for (var i = commonLength; i < toParts.Length; i++)
        {
            result.Add(toParts[i]);
        }

        return result.Count > 0 ? string.Join(SingleSeparator, result) : ".";
    }
#endif

    /// <summary>
    /// 获取父目录
    /// </summary>
    public static string GetParentDirectory(string? path, int levels)
    {
        path.EnsureIsNotNullAndWhiteSpace();

        levels = Math.Abs(levels);
        if (levels == 0) return path;

        // 使用循环代替递归
        for (var i = 0; i < levels; i++)
        {
            var info = Directory.GetParent(path);
            if (info == null) return path;
            path = info.FullName;
        }

        return path;
    }

    /// <summary>
    /// 检查路径中是否包含非法字符
    /// </summary>
    public static new bool ContainsInvalidPathChars(string? path)
    {
        if (path.IsNullOrEmpty())
            return false;

        // 系统定义的非法字符
        if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            return true;

        if (OSPlatformHelper.IsWindows)
        {
            // Windows特殊字符检查
#if NET8_0_OR_GREATER
            if (path.AsSpan().ContainsAny(s_windowsInvalidChars))
                return true;
#else
            if (ContainsInvalidChars(path, s_windowsInvalidChars))
                return true;
#endif

            // Windows保留名
            var segments = path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                var upperSegment = segment.ToUpperInvariant();
                if (s_windowsReservedNames.Contains(upperSegment) ||
                    (upperSegment.Contains('.') &&
                     s_windowsReservedNames.Contains(upperSegment.Take(upperSegment.IndexOf('.')))))
                {
                    return true;
                }
            }
        }
        else
        {
            // Unix系统检查
            if (path.Contains('\0'))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 检查文件或目录是否存在
    /// </summary>
    public static bool Exists(string path, bool checkAsFile = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var fullPath = Path.GetFullPath(path);

            // 检查是否包含无效路径字符
            if (ContainsInvalidPathChars(fullPath))
                return false;

            return checkAsFile ? File.Exists(path) : Directory.Exists(path);
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            return false;
        }
    }
}
