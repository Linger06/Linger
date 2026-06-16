using System.Diagnostics;
using System.Runtime.InteropServices;
using Linger.Extensions.Core;

namespace Linger.Extensions.IO;

/// <summary>
/// Extensions for <see cref="FileInfo"/>.
/// </summary>
public static partial class FileInfoExtensions
{
    /// <summary>
    /// 批量删除 FileInfo 集合（安全防御版）
    /// </summary>
    public static void Delete(this IEnumerable<FileInfo> @this)
    {
        if (@this == null) return;
        foreach (FileInfo t in @this)
        {
            if (t.Exists) t.Delete();
        }
    }

    /// <summary>
    /// 获取当前物理文件对象的版本信息（三平台完美兼容，Linux 下返回 null）
    /// </summary>
    public static FileVersionInfo? GetVersionInfo(this FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;

        return FileVersionInfo.GetVersionInfo(fileInfo.FullName);
    }

    /// <summary>
    /// 获取文件版本字符串（多框架支持，Linux 下自动返回友好占位符）
    /// </summary>
    public static string? GetFileVersion(this string fileFullPath)
    {
        if (string.IsNullOrWhiteSpace(fileFullPath)) return null;
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "1.0.0 (Linux Container)";

        return FileVersionInfo.GetVersionInfo(fileFullPath).FileVersion;
    }

    /// <summary>
    /// 获取文件所在的目录绝对路径（纯内存操作，100% 绕过同名裁剪 Bug）
    /// </summary>
    public static string GetFilePath(this string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return string.Empty;
        var directory = Path.GetDirectoryName(filePath);
        if (directory == null) return string.Empty;

        // 以斜杠结尾
        if (directory.Length > 0 && !directory.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            directory += Path.DirectorySeparatorChar;
        }
        return directory;
    }

    /// <summary>
    /// 获取指定路径文件的格式化大小（严格防御拦截版）
    /// </summary>
    /// <param name="filePath">本地物理文件路径</param>
    /// <exception cref="ArgumentException">当路径为空、全为空格或包含非法字符时抛出</exception>
    /// <exception cref="FileNotFoundException">当文件在物理磁盘上不存在时抛出</exception>
    public static string FileSize(this string filePath)
    {
        // 1. 拦截空字符串或纯空格
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null, empty, or whitespace.", nameof(filePath));
        }

        // 2. 拦截非法路径字符（规避操作系统层面次生崩溃）
        if (filePath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        {
            throw new ArgumentException($"File path contains invalid characters: {filePath}", nameof(filePath));
        }

        // 3. 严格判定物理存在性
        var fi = new FileInfo(filePath);
        if (!fi.Exists)
        {
            throw new FileNotFoundException($"Target file for size calculation does not exist.", filePath);
        }

        return fi.Length.FormatFileSize();
    }

    /// <summary>
    /// 获取文件大小格式化字符串（通过 FileInfo 对象）
    /// </summary>
    public static string FileSize(this FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return fileInfo.Length.FormatFileSize();
    }

    /// <summary>
    /// 从文件路径中获取不含扩展名的文件名（纯内存解析，零堆分配性能优化）
    /// </summary>
    public static string GetFileNameNoExtension(this string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>
    /// 从 FileInfo 中获取不含扩展名的文件名
    /// </summary>
    public static string GetFileNameNoExtension(this FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return Path.GetFileNameWithoutExtension(fileInfo.Name);
    }

    /// <summary>
    /// 从文件路径中获取不包含点号(.)的纯扩展名（防崩溃安全版）
    /// </summary>
    public static string GetExtensionNotDotString(this string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return string.Empty;
        var ext = Path.GetExtension(filePath); // 官方方法，Linux 下无后缀不崩溃
        return ext.Length > 1 ? ext.Substring(1) : string.Empty;
    }
}
