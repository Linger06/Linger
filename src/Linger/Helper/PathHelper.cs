using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Text;
using Linger.Extensions.Core;

namespace Linger.Helper;

public static class PathHelper
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
    /// <exception cref="ArgumentException">当路径无效或基础路径不是绝对路径时抛出</exception>
    public static string ResolveToAbsolutePath(string? basePath, string? relativePath, bool preserveEndingSeparator = false)
    {
        // 检查相对路径是否为空或仅包含空白字符
        if (relativePath.IsNullOrWhiteSpace())
            return relativePath ?? string.Empty;

        try
        {
            // 如果是特殊路径（如网络路径、FTP路径等），直接标准化处理
            if (IsSpecialPath(relativePath))
                return NormalizePath(relativePath, preserveEndingSeparator);

            if (ContainsInvalidPathChars(relativePath))
            {
                throw new IOException($"Invalid path: {relativePath}");
            }

            // 如果是绝对路径，直接标准化
            if (Path.IsPathRooted(relativePath))
                return NormalizePath(relativePath, preserveEndingSeparator);

            // 处理基础路径
            // 如果未提供基础路径，则使用当前目录
            basePath ??= Environment.CurrentDirectory;

            // 确保基础路径必须是绝对路径
            if (!Path.IsPathRooted(basePath))
                throw new ArgumentException("Base path must be absolute", nameof(basePath));

            // 组合基础路径和相对路径，并进行标准化处理
            var combinedPath = Path.Combine(basePath, relativePath);

            // 对组合后的路径进行标准化处理
            return NormalizePath(combinedPath, preserveEndingSeparator);
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            // 捕获所有路径相关的异常，并转换为ArgumentException
            throw new ArgumentException(
            $"Invalid path. Base: {basePath ?? "<null>"}, Relative: {relativePath ?? "<null>"}",
            nameof(relativePath),
            ex);
        }
    }

    // 辅助方法：检查是否为特殊路径格式
    private static bool IsSpecialPath(string path) =>
        path.StartsWith("""\\""", StringComparison.Ordinal) ||  // UNC路径
        path.StartsWith("//", StringComparison.Ordinal) ||      // 网络路径
        IsUrlWithProtocol(path);                                // URL类型路径

    // 辅助方法：检查是否为包含协议的URL
    private static bool IsUrlWithProtocol(string path) =>
        path.Contains("://") && 
        (path.StartsWith("ftp:", StringComparison.OrdinalIgnoreCase) ||
         path.StartsWith("ftps:", StringComparison.OrdinalIgnoreCase) ||
         path.StartsWith("sftp:", StringComparison.OrdinalIgnoreCase) ||
         path.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
         path.StartsWith("https:", StringComparison.OrdinalIgnoreCase) ||
         path.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
         path.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase));

    // 更新 NormalizePath 方法的调用
    [return: NotNullIfNotNull(nameof(path))]
    public static string? NormalizePath(string? path, bool preserveEndingSeparator = false)
    {
        if (path.IsNullOrWhiteSpace())
            return path ?? string.Empty;

        try
        {
            // 处理特殊路径
            if (IsSpecialPath(path))
            {
                // 处理 UNC 路径
                if (path.StartsWith("""\\""", StringComparison.Ordinal) || path.StartsWith("//", StringComparison.Ordinal))
                {
                    // 标准化 UNC 路径
                    // 步骤1: 统一使用反斜杠(Windows)或正斜杠(Unix)
                    var uncPathWithStandardSeparator = OSPlatformHelper.IsWindows 
                        ? path.Replace('/', '\\') 
                        : path.Replace('\\', '/');
                    
                    // 步骤2: 确保保留双斜杠前缀，然后处理后续部分（移除多余的连续分隔符）
                    char separator = OSPlatformHelper.IsWindows ? '\\' : '/';
                    string prefix = new string(separator, 2);
                    
                    var remainingPath = uncPathWithStandardSeparator.Substring(2);
                    // 将连续的分隔符替换为单个分隔符
                    while (remainingPath.Contains(new string(separator, 2)))
                    {
                        remainingPath = remainingPath.Replace(new string(separator, 2), separator.ToString());
                    }
                    
                    // 步骤3: 重新组合路径
                    var normalizedUncPath = prefix + remainingPath;
                    
                    // 处理末尾分隔符
                    normalizedUncPath = normalizedUncPath.TrimEnd(separator);
                    
                    return preserveEndingSeparator
                        ? normalizedUncPath + separator
                        : normalizedUncPath;
                }
                
                // 处理 URL 类型路径
                if (IsUrlWithProtocol(path))
                {
                    // 查找协议分隔符 "://"
                    int protocolIndex = path.IndexOf("://", StringComparison.Ordinal);
                    string protocol = path.Substring(0, protocolIndex + 3); // 包含 "://"
                    string remainder = path.Substring(protocolIndex + 3);
                    
                    // 对 URL 路径部分进行标准化，统一使用正斜杠
                    var segments = remainder.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    // 重新组合 URL
                    var normalizedUrl = protocol;
                    if (segments.Length > 0)
                    {
                        normalizedUrl += segments[0]; // 主机名
                        
                        if (segments.Length > 1)
                        {
                            normalizedUrl += "/" + string.Join("/", segments.Skip(1));
                        }
                    }
                    
                    // 处理末尾分隔符
                    if (preserveEndingSeparator && !normalizedUrl.EndsWith("/"))
                    {
                        normalizedUrl += "/";
                    }
                    else if (!preserveEndingSeparator && normalizedUrl.EndsWith("/") && normalizedUrl.Length > protocol.Length)
                    {
                        normalizedUrl = normalizedUrl.TrimEnd('/');
                    }
                    
                    return normalizedUrl;
                }
            }

            // 处理 file:// 格式
            if (path.Contains("file://"))
            {
                try 
                {
                    // 仅当路径严格符合URI格式时才转换为本地路径
                    if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsFile)
                    {
                        var localPath = uri.LocalPath;
                        if (!string.IsNullOrEmpty(localPath))
                        {
                            path = localPath;
                        }
                    }
                    // 如果无法解析为有效的文件URI，保留原始路径
                }
                catch (UriFormatException)
                {
                    // URI格式无效，保持原路径不变
                }
            }

            // 统一路径分隔符为当前系统的标准分隔符
            var standardizedPath = StandardizePathSeparators(path);

            // 删除多余的连续分隔符
            standardizedPath = RemoveConsecutiveSeparators(standardizedPath);

            // 处理末尾分隔符
            var normalizedPath = standardizedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return preserveEndingSeparator
                ? normalizedPath + Path.DirectorySeparatorChar
                : normalizedPath;
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            // 异常情况下的简单处理，仅处理末尾分隔符
            return preserveEndingSeparator
                ? path + Path.DirectorySeparatorChar
                : path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }

    /// <summary>
    /// 统一路径分隔符为当前系统标准
    /// </summary>
    private static string StandardizePathSeparators(string path)
    {
        // Windows系统使用反斜杠，Unix/Linux系统使用正斜杠
        char currentSeparator = Path.DirectorySeparatorChar;
        char altSeparator = Path.AltDirectorySeparatorChar;
        
        // 由于特殊路径已在NormalizePath中单独处理，这里只需处理常规路径
        return path.Replace(altSeparator, currentSeparator);
    }

    /// <summary>
    /// 删除路径中多余的连续分隔符
    /// </summary>
    private static string RemoveConsecutiveSeparators(string path)
    {
        // 处理常规路径
        char separator = Path.DirectorySeparatorChar;
        string doubleSeparator = new string(separator, 2);
        
        while (path.Contains(doubleSeparator))
        {
            path = path.Replace(doubleSeparator, separator.ToString());
        }
        
        return path;
    }

    // 辅助方法：检查是否为路径相关异常
    private static bool IsPathException(Exception ex) =>
        ex is UriFormatException ||
        ex is ArgumentException ||
        ex is SecurityException ||
        ex is NotSupportedException ||
        ex is PathTooLongException ||
        ex is IOException;

    /// <summary>
    /// 获取相对路径
    /// </summary>
    /// <param name="relativeTo">The source path the output should be relative to. This path is always considered to be a directory.</param>
    /// <param name="path">The destination path.</param>
    /// <returns>返回从relativeTo到path的相对路径，如果无法创建相对路径则返回原始path。当两个路径相同时返回"."</returns>
    /// <exception cref="ArgumentException">当路径无效时抛出</exception>
    public static string GetRelativePath(string relativeTo, string path)
    {
        // 参数验证
        if (relativeTo.IsNullOrWhiteSpace())
            throw new ArgumentException("Base path cannot be null or empty", nameof(relativeTo));
        
        if (path.IsNullOrWhiteSpace())
            return path ?? string.Empty;

        // 标准化两个路径
        path = NormalizePath(path);
        relativeTo = NormalizePath(relativeTo, true);

        // 检查两个路径是否相同（忽略大小写和末尾分隔符）
        if (PathEquals(path, relativeTo))
            return ".";

        try
        {
#if NETCOREAPP
            // .NET Core 已内置此功能
            return Path.GetRelativePath(relativeTo, path);
#else
            // 确保路径是绝对路径
            if (!Path.IsPathRooted(path))
                path = Path.GetFullPath(path);
            
            if (!Path.IsPathRooted(relativeTo))
                relativeTo = Path.GetFullPath(relativeTo);
            
            // 再次检查转换为绝对路径后是否相同
            if (PathEquals(path, relativeTo))
                return ".";
                
            // 检查是否在相同的驱动器或根路径上
            if (Path.GetPathRoot(path) != Path.GetPathRoot(relativeTo))
                return path; // 不同驱动器，无法创建相对路径
            
            try
            {
                var pathUri = new Uri(path);
                var baseUri = new Uri(relativeTo);
                var relativeUri = baseUri.MakeRelativeUri(pathUri);
                var result = Uri.UnescapeDataString(relativeUri.ToString())
                         .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                
                // 处理Uri方法返回空字符串的情况
                return string.IsNullOrEmpty(result) ? "." : result;
            }
            catch (UriFormatException)
            {
                // 备用方法：手动计算相对路径
                return ComputeRelativePath(relativeTo, path);
            }
#endif
        }
        catch (Exception ex) when (IsPathException(ex))
        {
            throw new ArgumentException($"Invalid path. Path: {path}, Base: {relativeTo}", ex);
        }
    }

#if !NETCOREAPP
    // 在非.NET Core环境下使用的备用相对路径计算方法
    private static string ComputeRelativePath(string relativeTo, string path)
    {
        var fromParts = relativeTo.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        var toParts = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

        // 找到共同的路径部分
        int commonLength = 0;
        int minLength = Math.Min(fromParts.Length, toParts.Length);
        
        for (int i = 0; i < minLength; i++)
        {
            if (string.Equals(fromParts[i], toParts[i], StringComparison.OrdinalIgnoreCase))
                commonLength++;
            else
                break;
        }

        // 构建相对路径
        var result = new StringBuilder();
        
        // 添加向上导航的部分 ".."
        for (int i = commonLength; i < fromParts.Length; i++)
            result.Append(".." + Path.DirectorySeparatorChar);
        
        // 添加目标路径的非共享部分
        for (int i = commonLength; i < toParts.Length; i++)
        {
            if (i > commonLength)
                result.Append(Path.DirectorySeparatorChar);
            result.Append(toParts[i]);
        }

        return result.Length > 0 ? result.ToString() : ".";
    }
#endif

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
            if (path.AsSpan().IndexOfAny(s_windowsInvalidChars) != -1)
                return true;

            // 检查每个路径段
            var segments = path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var segment in segments)
            {
                string upperSegment = segment.ToUpperInvariant();
                if (s_windowsReservedNames.Any(name =>
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

    public static string GetParentDirectory(string? path, int levels)
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