using Linger.Extensions.Core;
using Linger.Extensions.IO;
#if NET8_0_OR_GREATER
using System.Buffers;
#endif

namespace Linger.Helper.PathHelpers;

/// <summary>
/// 标准路径处理类，专门用于处理操作系统的本地物理文件系统路径（Local Paths）
/// </summary>
public class StandardPathHelper : PathHelperBase
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
    /// 解析并生成本地绝对路径（自动处理本地磁盘、上级目录跳转及长路径）
    /// </summary>
    public static string ResolveToAbsolutePath(string? basePath, string? relativePath, bool preserveEndingSeparator = false)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return relativePath ?? string.Empty;
        }

        try
        {
            // 本地物理路径的安全性拦截
            if (ContainsInvalidPathChars(relativePath))
            {
                throw new IOException($"Invalid local path characters found: {relativePath}");
            }

            string resolvedPath;

#if NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER
            // 现代 .NET：原生驱动，直接调用本地文件系统的核心解析引擎
            basePath ??= Environment.CurrentDirectory;
            resolvedPath = Path.GetFullPath(relativePath, basePath);
#else
            // 旧版本 .NET 框架回退兼容逻辑
            if (relativePath.IsStrictAbsolutePath())
            {
                resolvedPath = Path.GetFullPath(relativePath);
            }
            else
            {
                basePath ??= Environment.CurrentDirectory;

                // 修复本地相对路径以单斜杠开头时（如 \Windows），Path.Combine 丢失本地盘符的 Bug
                if (relativePath.Length > 0 && (relativePath == "\\" || relativePath == "/"))
                {
                    var baseRoot = Path.GetPathRoot(basePath) ?? string.Empty;
                    var trimmedPath = relativePath.Substring(1);
                    resolvedPath = Path.GetFullPath(Path.Combine(baseRoot, trimmedPath));
                }
                else
                {
                    resolvedPath = Path.GetFullPath(Path.Combine(basePath, relativePath));
                }
            }
#endif

            // 将斜杠统一为当前操作系统的本地标准（Windows 转换为 \，Linux/macOS 转换为 /）
            resolvedPath = resolvedPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            // 本地文件系统目录的尾部斜杠控制
            bool currentlyHasSeparator = resolvedPath.EndsWith(Path.DirectorySeparatorChar);

            if (preserveEndingSeparator && !currentlyHasSeparator)
            {
                return resolvedPath + Path.DirectorySeparatorChar;
            }

            if (!preserveEndingSeparator && currentlyHasSeparator)
            {
                // 本地路径边界安全锁：防止 Linux 本地根目录 "/" 被裁剪，或 Windows 本地根盘符 "C:\" 缩水成相对路径 "C:"
                if (resolvedPath.Length > 1 && (Path.DirectorySeparatorChar == '/' || resolvedPath.Length > 3))
                {
                    return resolvedPath.TrimEnd(Path.DirectorySeparatorChar);
                }
            }

            return resolvedPath;
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            throw new ArgumentException(
                $"Failed to resolve local absolute path. Base: {basePath ?? "<null>"}, Relative: {relativePath ?? "<null>"}",
                nameof(relativePath),
                ex);
        }
    }

    /// <summary>
    /// 标准化本地路径
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
    /// 判断两个本地路径在当前操作系统下是否指向同一物理位置（大小写自适应）
    /// </summary>
    public static bool PathEquals(string? path1, string? path2, bool ignoreCase = true)
    {
        if (path1 == null || path2 == null)
            return path1 == path2;

        var comparison = ignoreCase ? PathComparison : StringComparison.Ordinal;
        if (string.Equals(path1, path2, comparison))
            return true;

        try
        {
            // 通过本地绝对路径引擎彻底拉平所有相对标记
            var absPath1 = ResolveToAbsolutePath(null, path1, false);
            var absPath2 = ResolveToAbsolutePath(null, path2, false);

            return string.Equals(absPath1, absPath2, comparison);
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            return string.Equals(path1, path2, comparison);
        }
    }

    /// <summary>
    /// 验证是否为合法的 Windows 本地物理盘符（如 C: 或 D:\）
    /// </summary>
    public static bool IsWindowsDriveLetter(string? input)
    {
        if (input.IsNullOrEmpty() || input.Length < 2)
            return false;

        char drive = input[0];
        if (!((drive >= 'A' && drive <= 'Z') || (drive >= 'a' && drive <= 'z')) || input[1] != ':')
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
    /// 获取两个本地物理路径之间的相对路径
    /// </summary>
    public static string GetRelativePath(string relativeTo, string path)
    {
        if (relativeTo.IsNullOrWhiteSpace())
            throw new ArgumentException("Local base path cannot be null or empty", nameof(relativeTo));

        if (path.IsNullOrWhiteSpace())
            return path ?? string.Empty;

        // 统一转换为本地标准绝对路径后再计算
        relativeTo = ResolveToAbsolutePath(null, relativeTo, false);
        path = ResolveToAbsolutePath(null, path, false);

        if (PathEquals(path, relativeTo))
            return ".";

        try
        {
#if NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP
            return Path.GetRelativePath(relativeTo, path);
#else
            return GetRelativePathCoreOptimized(relativeTo, path);
#endif
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            throw new ArgumentException($"Invalid local path for relative calculation. Path: {path}, Base: {relativeTo}", ex);
        }
    }

#if !NETCOREAPP2_1_OR_GREATER && !NET5_0_OR_GREATER && !NETCOREAPP
    private static string GetRelativePathCoreOptimized(string relativeTo, string path)
    {
        var relativeToRoot = Path.GetPathRoot(relativeTo) ?? string.Empty;
        var pathRoot = Path.GetPathRoot(path) ?? string.Empty;

        // Windows 本地路径：如果不在同一个物理分区（如 C 盘和 D 盘），无法建立相对路径，直接返回原绝对路径
        if (!string.Equals(relativeToRoot, pathRoot, PathComparison))
            return path;

        var fromParts = SplitPath(relativeTo);
        var toParts = SplitPath(path);

        int commonLength = 0;
        int minLength = Math.Min(fromParts.Length, toParts.Length);

        for (int i = 0; i < minLength; i++)
        {
            if (string.Equals(fromParts[i], toParts[i], PathComparison))
                commonLength++;
            else
                break;
        }

        int gapDirectories = fromParts.Length - commonLength;
        int remainingDirectories = toParts.Length - commonLength;

        int totalParts = gapDirectories + remainingDirectories;
        if (totalParts == 0) return ".";

        var resultParts = new string[totalParts];
        int index = 0;

        for (int i = 0; i < gapDirectories; i++)
        {
            resultParts[index++] = "..";
        }

        for (int i = commonLength; i < toParts.Length; i++)
        {
            resultParts[index++] = toParts[i];
        }

        return string.Join(SingleSeparator, resultParts);
    }
#endif

    /// <summary>
    /// 基于本地文件系统逻辑结构获取父级目录（100% 内存计算，不触发磁盘 I/O）
    /// </summary>
    public static string GetParentDirectory(string? path, int levels)
    {
        path.EnsureIsNotNullOrWhiteSpace();

        levels = Math.Abs(levels);
        if (levels == 0 || string.IsNullOrEmpty(path)) return path;

        try
        {
            string currentPath = ResolveToAbsolutePath(null, path, false);

            for (var i = 0; i < levels; i++)
            {
                var parent = Path.GetDirectoryName(currentPath);

                // 到达本地文件系统的根节点（如 Linux 的 "/" 或 Windows 的 "C:\"）时中止
                if (parent == null)
                    return currentPath;

                currentPath = parent;
            }

            return currentPath;
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            return path;
        }
    }

    /// <summary>
    /// 全面验证路径中是否包含当前本地物理文件系统不支持的非法字符或保留字
    /// </summary>
    public static new bool ContainsInvalidPathChars(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        // 1. 系统底层定义的非法字符拦截（跨平台自适应）
        if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            return true;

        // 2. 识别是否处于 Windows 本地物理环境
        if (Path.DirectorySeparatorChar == '\\')
        {
#if NET8_0_OR_GREATER
            if (path.AsSpan().ContainsAny(s_windowsInvalidChars))
                return true;
#else
            if (ContainsInvalidChars(path, s_windowsInvalidChars))
                return true;
#endif

            // Windows 文件系统独有的保留名称拦截（杜绝创建 CON, PRN 等引发底层崩溃）
            // 优化：Windows 的机制是只要文件名部分去掉所有扩展名后等于保留字，即为非法
            var segments = path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var segment in segments)
            {
                var upperSegment = segment.ToUpperInvariant();

                // 修复：Windows 允许文件名包含多个点，如 "CON.tar.gz"。
                // 真正的非法判定应该提取第一个点（.`）之前的所有内容；如果没点，则看全称。
                int firstDotIndex = upperSegment.IndexOf('.');
                string nameToCheck = firstDotIndex != -1 ? upperSegment.Substring(0, firstDotIndex) : upperSegment;

                // 移除尾部的空格（因为 Windows 下 "CON " 也是非法的）
                nameToCheck = nameToCheck.TrimEnd(' ');

                if (s_windowsReservedNames.Contains(nameToCheck))
                {
                    return true;
                }
            }
        }
        else
        {
            // Linux/macOS 本地物理环境拦截：只禁止空字符 \0
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

            // 检查是否包含空字符，如果有则直接返回 false
            if (path.Contains('\0'))
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
