using System.Text;

#if NET
using System.Buffers;
#endif

namespace Linger.Helper;

/// <summary>
/// 路径处理核心工具类，提供跨平台物理路径清洗、分隔符规范化等底层核心工具方法
/// </summary>
public static class PathHelper
{
    // 当前平台标准的路径分隔符（Windows 为 '\'，Unix 为 '/'）
    internal static readonly char PlatformSeparator = Path.DirectorySeparatorChar;

    // 当前平台的备用路径分隔符（Windows 为 '/'，Unix 为 '\')
    internal static readonly char AltPlatformSeparator = Path.AltDirectorySeparatorChar;

    // 兼容旧版本 .NET 的标准路径分隔符数组
    internal static readonly char[] PathSeparators = new[] { '/', '\\' };

    // 默认的路径字符串比较规则（忽略大小写）
    internal const StringComparison PathComparison = StringComparison.OrdinalIgnoreCase;

    // 当前平台标准路径分隔符的字符串形式
    internal static readonly string SingleSeparator = PlatformSeparator.ToString();

    /// <summary>
    /// 检查指定的异常是否属于文件或路径操作相关的异常
    /// </summary>
    internal static bool IsPathException(Exception ex) =>
        ex is ArgumentException ||
        ex is System.Security.SecurityException ||
        ex is NotSupportedException ||
        ex is PathTooLongException ||
        ex is IOException;

    /// <summary>
    /// 统一路径分隔符为当前操作系统的标准分隔符
    /// </summary>
    internal static string StandardizePathSeparators(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        // 性能优化：若路径中不包含备用分隔符，则直接返回原引用，避免触发内存分配
        if (!path.Contains(AltPlatformSeparator))
        {
            return path;
        }

        return path.Replace(AltPlatformSeparator, PlatformSeparator);
    }

    /// <summary>
    /// 去除路径中多余的连续分隔符（支持保留 Windows 本地长路径前缀及本地设备路径）
    /// </summary>
    internal static string RemoveConsecutiveSeparators(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        // 【安全防御】：如果路径中包含网络协议头，直接拦截并原样返回
#if NET
        if (path.Contains("://", StringComparison.Ordinal)) return path;
#else
        if (path.IndexOf("://", PathComparison) != -1) return path;
#endif

        // 性能优化：快速全字扫描，若无连续分隔符则直接返回原引用，避免后续堆分配
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

        // 判定是否以双斜杠开头（如 Windows UNC 路径 \\server\share 或长路径前缀 \\?\）
        var startIndex = 0;
        bool isDoubleStart = path.Length > 2 &&
                             (path[0] == '/' || path[0] == '\\') &&
                             (path[1] == '/' || path[1] == '\\');

#if NET
        // 【Modern .NET 极致优化】：利用 ArrayPool 租用内存，完全抹平高频调用时 StringBuilder 的堆分配开销
        int maxResultLength = path.Length;
        char[] rentedArray = ArrayPool<char>.Shared.Rent(maxResultLength);
        int pos = 0;

        try
        {
            if (isDoubleStart)
            {
                // 保持原本的双斜杠前缀结构不变（保护 UNC 头部）
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
        // 【.NET Framework 4.7 适配优化】：锁定 StringBuilder 的初始容量，防止高频追加时内部数组频繁重分配
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
    /// 处理路径末尾分隔符，防止因裁剪导致物理根目录退化
    /// </summary>
    internal static string HandleEndingSeparator(string path, bool preserveEndingSeparator, int minLength = 0)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        var trimmedPath = path.TrimEnd('/', '\\');

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
            // Linux 本地物理根目录被裁剪后恢复为 "/"
            return SingleSeparator;
        }

        if (PlatformSeparator == '\\' && trimmedPath.Length == 2 && trimmedPath[1] == ':')
        {
            // Windows 本地根盘符被裁剪后恢复为 "C:\"
            return trimmedPath + SingleSeparator;
        }

        return trimmedPath.Length > minLength ? trimmedPath : path;
    }

    /// <summary>
    /// 将路径按照分隔符拆分为段
    /// </summary>
    internal static string[] SplitPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return Array.Empty<string>();

#if NET
        // 在新框架下顺手利用系统级组合枚举裁剪多余段内空格
        return path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
#else
        return path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
#endif
    }

    /// <summary>
    /// 纯内存级别的路径标准化清洗基础管道（统一分隔符 -> 去除连续分隔符 -> 尾部斜杠控制）
    /// </summary>
    public static string CleanAndNormalizePureString(string path, bool preserveEndingSeparator)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var standardPath = StandardizePathSeparators(path);
        standardPath = RemoveConsecutiveSeparators(standardPath);

        return HandleEndingSeparator(standardPath, preserveEndingSeparator);
    }

    /// <summary>
    /// 检查是否包含当前操作系统定义的非法物理路径字符
    /// </summary>
    internal static bool ContainsInvalidPathChars(string path)
    {
        return !string.IsNullOrEmpty(path) && path.IndexOfAny(Path.GetInvalidPathChars()) != -1;
    }
}
