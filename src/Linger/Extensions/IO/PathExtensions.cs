using Linger.Helper;

#if NET
using System.Buffers;
#endif

namespace Linger.Extensions.IO;

/// <summary>
/// 供外部直接调用的本地（文件系统）路径公共服务扩展工具类
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
    /// 【扩展方法】全面验证路径中是否包含当前本地物理文件系统不支持的非法字符或内核级安全保留字（如 CON, PRN）
    /// </summary>
    public static bool ContainsInvalidPathChars(this string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            return true;

        if (Path.DirectorySeparatorChar == '\\')
        {
#if NET8_0_OR_GREATER
            if (path.AsSpan().ContainsAny(s_windowsInvalidChars))
                return true;

            ReadOnlySpan<char> pathSpan = path.AsSpan();
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
                        Span<char> upperName = stackalloc char[nameToCheck.Length];
                        nameToCheck.ToUpperInvariant(upperName);
                        if (s_windowsReservedNames.Contains(upperName.ToString()))
                            return true;
                    }
                }

                if (separatorIndex == -1) break;
                pathSpan = pathSpan.Slice(separatorIndex + 1);
            }
#else
            if (path.IndexOfAny(s_windowsInvalidChars) != -1)
                return true;

            int lastIdx = 0;
            while (lastIdx < path.Length)
            {
                int nextSep = path.IndexOfAny(PathHelper.PathSeparators, lastIdx);
                int len = (nextSep == -1 ? path.Length : nextSep) - lastIdx;

                if (len > 0)
                {
                    string segment = path.Substring(lastIdx, len);
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
            if (path.IndexOf('\0') != -1)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 解析并生成本地绝对路径（自动处理本地磁盘、上级目录跳转及长路径）
    /// </summary>
    public static string ResolveToAbsolutePath(this string? relativePath, string? basePath = null, bool preserveEndingSeparator = false)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return relativePath ?? string.Empty;
        }

        try
        {
            if (PathHelper.ContainsInvalidPathChars(relativePath))
            {
                throw new IOException($"Invalid local path characters found: {relativePath}");
            }

            string resolvedPath;

#if NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET
            // 现代 .NET：原生驱动极其强大，已经天然处理了跨平台与高性能路由
            basePath ??= Environment.CurrentDirectory;
            resolvedPath = Path.GetFullPath(relativePath, basePath);
#else
            // 【旧版本 .NET 4.7 完美修复路由】：
            // 重新启用你的 IsStrictAbsolutePath 扩展方法！
            // 只有真正的本地物理路径（如 C:\ 或 /）才直接获取全路径，100% 绕过了 Windows UNC 网络路径引发的 I/O 阻塞隐患
            if (relativePath.IsStrictAbsolutePath())
            {
                resolvedPath = Path.GetFullPath(relativePath);
            }
            else
            {
                basePath ??= Environment.CurrentDirectory;

                // 修复本地相对路径以单斜杠开头时（如 \Windows），Path.Combine 疏漏盘符退化的经典 Bug
                if (relativePath.Length > 0 && (relativePath[0] == '\\' || relativePath[0] == '/'))
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

            // 调用底层的统一管道控制尾部斜杠并转换格式，保证整体行为的一致性
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
    /// 【扩展方法】基于本地文件系统逻辑结构获取父级目录（100% 内存计算，不触发磁盘 I/O）
    /// </summary>
    public static string GetParentDirectory(this string path, int levels)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        levels = Math.Abs(levels);
        if (levels == 0) return path;

        try
        {
            // 利用上面刚改造好的扩展方法进行内部调用
            string currentPath = path.ResolveToAbsolutePath();
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
    /// 获取当前基准路径到指定目标路径的相对路径（支持跨平台与 .NET Framework 降级兼容）。
    /// </summary>
    /// <param name="relativeTo">当前的基准目录路径（作为计算的参照物起点）。</param>
    /// <param name="path">要到达的目标物理路径（终点）。</param>
    /// <returns>
    /// 返回计算后的相对路径（例如 "..\logs" 或 "sub/file.txt"）。<br/>
    /// 如果两个路径完全相同，则返回 "."；<br/>
    /// 如果目标路径为空，则原样返回目标路径或 <see cref="string.Empty"/>。
    /// </returns>
    /// <exception cref="ArgumentException">
    /// 当基准路径 <paramref name="relativeTo"/> 为空、全是空格，或者由于跨盘符/无效字符导致无法计算相对路径时抛出。
    /// </exception>
    /// <remarks>
    /// <para>【安全与漏洞修复】</para>
    /// 内部在计算前会强制将两个路径链式解析为绝对路径（<c>ResolveToAbsolutePath</c>），<br/>
    /// 能够有效阻断恶意利用相对路径符号（如 <c>../../</c>）越界爬行的路径穿越漏洞（Path Traversal）。
    /// <para>【跨平台与多框架适配】</para>
    /// - 在 Modern .NET 环境下，直接托管给系统级高性能官方 API，无内存分配负担。<br/>
    /// - 在 .NET Framework 环境下，降级采用基于 <see cref="Uri"/> 的解算方案，并统一进行路径分隔符的规范化。
    /// </remarks>
    public static string GetRelativePath(this string relativeTo, string path)
    {
        if (string.IsNullOrWhiteSpace(relativeTo))
            throw new ArgumentException("Local base path cannot be null or empty", nameof(relativeTo));

        if (string.IsNullOrWhiteSpace(path))
            return path ?? string.Empty;

        relativeTo = relativeTo.ResolveToAbsolutePath();
        path = path.ResolveToAbsolutePath();

        if (string.Equals(path, relativeTo, PathHelper.PathComparison))
            return ".";

        try
        {
#if NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET
            return Path.GetRelativePath(relativeTo, path);
#else
            // -----------------------------------------------------------------
            // 【Windows 专属修复分支】：.NET Framework 4.7.2
            // -----------------------------------------------------------------
            const string sep = "\\";
            // 【前置净化】：既然 Windows 允许正斜杠，在进入 Uri 复杂计算前，
            // 纯内存无 I/O 地将所有正斜杠统一转换为标准反斜杠，确保后续补齐与裁剪算法 100% 稳健。
            string cleanFrom = relativeTo.Replace('/', '\\');
            string cleanTo = path.Replace('/', '\\');
            // 1. 强制补齐基准路径的末尾反斜杠，明确其为目录上下文
            if (!cleanFrom.EndsWith(sep)) cleanFrom += sep;
            // 2. 核心修复：基于纯字面（零磁盘 I/O）判定目标路径是否为目录
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
            // 3. 将 Uri 吐出的标准正斜杠 '/' 统一转换回 Windows 的反斜杠 '\'
            string result = relativePath.Replace('/', '\\');
            // 4. 还原状态：把我们此前为了欺骗 Uri 引擎而强补的尾巴切掉
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
    /// 【坚守静态方法】检查文件或目录是否存在（具备强安全原子性保护，不改为扩展方法）
    /// </summary>
    public static bool Exists(string path, bool checkAsFile = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            if (path.IndexOf('\0') != -1)
                return false;

            var fullPath = Path.GetFullPath(path);

            // 内部调用刚改造好的扩展方法
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
    /// 辅助方法：用于旧版 .NET 判断【严格的本地物理绝对路径】（排除了网络 UNC 路径）
    /// </summary>
    public static bool IsStrictAbsolutePath(this string path)
    {
        if (!Path.IsPathRooted(path)) return false;

        var root = Path.GetPathRoot(path);
        if (string.IsNullOrEmpty(root)) return false;

        // 1. Linux/macOS 平台：只要以 '/' 开头且不是双斜杠（双斜杠是网络或特殊的网络根），就是严格本地路径
        if (Path.DirectorySeparatorChar == '/')
        {
            return path.Length > 0 && path[0] == '/' && (path.Length == 1 || path[1] != '/');
        }

        // 2. Windows 平台：必须是本地物理盘符或本地长路径
        // 排除传统的网络 UNC 路径（如 \\server\share），因为它们已经去到 NetworkPathHelper 了

        // 检查标准本地盘符 (如 "C:\")
        if (root.Length >= 2 && char.IsLetter(root[0]) && root[1] == ':')
        {
            return root.Length == 2 || root[2] == '\\' || root[2] == '/';
        }

        // 检查 Windows 本地高级路径前缀（如 "\\?\" 长路径 或 "\\.\" 本地设备路径）
        if (root.StartsWith(@"\\?\", StringComparison.Ordinal) || root.StartsWith(@"\\.\", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }
}
