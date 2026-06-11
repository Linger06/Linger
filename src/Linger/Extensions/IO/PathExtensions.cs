using Linger.Extensions.Core;

namespace Linger.Extensions.IO;

public static class PathExtensions
{
#if NETCOREAPP

    /// <summary>
    /// 获取相对路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="relativeTo"></param>
    /// <returns></returns>
    [Obsolete("Use StandardPathHelper.GetRelativePath instead of this method. Please note the order of those parameters is important!")]
    public static string GetRelativePath(this string path, string? relativeTo = null)
    {
        relativeTo ??= Environment.CurrentDirectory;
        return Path.GetRelativePath(relativeTo, path);
    }

#else
    /// <summary>
    /// 获取相对路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="relativeTo"></param>
    /// <returns></returns>
    [Obsolete("Use StandardPathHelper.GetRelativePath instead of this method. Please note the order of those parameters is important!")]
    public static string GetRelativePath(this string path, string? relativeTo = null)
    {
        relativeTo ??= Environment.CurrentDirectory;
        var absolutePath = path.GetAbsolutePath(relativeTo);
        return absolutePath.RelativeTo(relativeTo);
    }
#endif

    /// <summary>
    /// 获取绝对路径
    /// </summary>
    /// <param name="path">"..\Test" or "C:\Test"</param>
    /// <param name="basePath"></param>
    /// <returns>如果 path 为绝对路径，直接返回，若path为相对路径，就需要basePath</returns>
    [Obsolete("Use StandardPathHelper.ResolveToAbsolutePath instead of this method. Please note the order of those parameters is important!")]
    public static string GetAbsolutePath(this string path, string? basePath = null)
    {
        if (path.IsAbsolutePath())
        {
            return Path.GetFullPath(path);
        }

        basePath ??= Environment.CurrentDirectory;

        var combined = Path.Combine(basePath, path);
        combined = Path.GetFullPath(combined);
        return combined;
    }

    /// <summary>
    /// 判断此路径是否为绝对路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    [Obsolete("Use Path.IsPathRooted instead.")]
    public static bool IsAbsolutePath(this string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Path.IsPathRooted(path);
    }

    [Obsolete("Use StandardPathHelper.GetRelativePath instead.")]
    public static string RelativeTo(this string sourcePath, string folder)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourcePath);
        ArgumentException.ThrowIfNullOrEmpty(folder);

        if (!Path.IsPathRooted(sourcePath))
        {
            sourcePath = Path.GetFullPath(sourcePath);
        }

        var pathUri = new Uri(sourcePath);

        if (!folder.IsAbsolutePath())
        {
            folder = folder.GetAbsolutePath();
        }

        // Folders must end in a slash
        if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            folder += Path.DirectorySeparatorChar;
        }

        var folderUri = new Uri(folder);
        Uri relativeUri = folderUri.MakeRelativeUri(pathUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());
        return relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
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

    /// <summary>
    /// 安全地将相对路径或未规范化的路径转换为标准的绝对路径。
    /// 完美兼容 Windows、Linux、macOS 以及所有 .NET 版本。
    /// </summary>
    /// <param name="path">需要转换的路径。</param>
    /// <param name="basePath">基准路径（如果输入的 path 是相对路径）。默认为当前工作目录。</param>
    /// <param name="preserveEndingSeparator">是否强制在返回的路径末尾保留目录分隔符。</param>
    public static string GetAbsolutePath(
        string path,
        string? basePath = null,
        bool preserveEndingSeparator = false)
    {
        // 1. 快速防御性检查（合并自 Linger 的空值处理逻辑）
        if (string.IsNullOrWhiteSpace(path))
        {
            return path ?? string.Empty;
        }
        try
        {
            string resolvedPath;
            // 2. 利用条件编译进行高性能平台路由切换
#if NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER
            // 现代 .NET：原生运行时极其高效地处理了相对标记（..）、
            // 斜杠统一，并能完美融合相对路径和半绝对路径，无任何字符串分配开销。
            basePath ??= Environment.CurrentDirectory;
            resolvedPath = Path.GetFullPath(path, basePath);
#else
            // 旧版本 .NET 框架（.NET Framework 4.x / Standard 2.0）回退兼容逻辑
            if (path.IsStrictAbsolutePath())
            {
                resolvedPath = Path.GetFullPath(path);
            }
            else
            {
                basePath ??= Environment.CurrentDirectory;
                // 修复旧版本关键 Bug：如果相对路径以单斜杠开头（例如 "\foo"），
                // 原生的 Path.Combine 会直接丢弃整个 basePath。
                if (path.Length > 0 && (path[0] == '\\' || path[0] == '/'))
                {
                    var baseRoot = Path.GetPathRoot(basePath) ?? string.Empty;
                    var trimmedPath = path.Substring(1);
                    resolvedPath = Path.GetFullPath(Path.Combine(baseRoot, trimmedPath));
                }
                else
                {
                    resolvedPath = Path.GetFullPath(Path.Combine(basePath, path));
                }
            }
#endif
            // 3. 将斜杠统一为当前操作系统的标准（Windows 为 \，Unix 为 /）
            resolvedPath = resolvedPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            // 4. 安全地实现 Linger 的尾部分隔符定制化业务规则
            bool currentlyHasSeparator = resolvedPath.EndsWith(Path.DirectorySeparatorChar);
            if (preserveEndingSeparator && !currentlyHasSeparator)
            {
                return resolvedPath + Path.DirectorySeparatorChar;
            }

            if (!preserveEndingSeparator && currentlyHasSeparator)
            {
                // 防御性安全检查：防止过度裁剪导致 Linux 的根目录 "/" 变成空字符串 ""
                // 在 Windows 下同理防止 "C:\" 变成 "C:"
                if (resolvedPath.Length > 1 && (Path.DirectorySeparatorChar == '/' || resolvedPath.Length > 3))
                {
                    return resolvedPath.TrimEnd(Path.DirectorySeparatorChar);
                }
            }
            return resolvedPath;
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is IOException)
        {
            // 捕获所有路径异常并包装，提供清晰的调试上下文
            throw new ArgumentException(
                $"Failed to resolve absolute path. Input: '{path}', Base: '{basePath ?? "<null>"}'",
                nameof(path),
                ex);
        }
    }
}
