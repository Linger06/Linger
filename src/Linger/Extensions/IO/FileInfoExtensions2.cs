using System.Diagnostics;
using Linger.Extensions.Core;

namespace Linger.Extensions.IO;

/// <summary>
/// Extensions for <see cref="FileInfo"/>.
/// </summary>
public static partial class FileInfoExtensions
{
    /// <summary>
    /// 将当前 <see cref="FileInfo"/> 对象转换成 <see cref="MemoryStream"/> 对象
    /// </summary>
    /// <param name="fileInfo">要转换的 <see cref="FileInfo"/> 对象</param>
    /// <param name="deleteFile">是否删除文件</param>
    /// <returns>转换后的 <see cref="MemoryStream"/> 对象</returns>
    public static MemoryStream ToMemoryStream(this FileInfo fileInfo, bool deleteFile = false)
    {
        var memoryStream = new MemoryStream();
        FileStream fileStream = fileInfo.OpenRead();
        var bytes = new byte[fileStream.Length];
        _ = fileStream.Read(bytes, 0, (int)fileStream.Length);
        memoryStream.Write(bytes, 0, (int)fileStream.Length);
        fileStream.Close();
        if (deleteFile)
        {
            fileInfo.Delete();
        }
        _ = memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    /// <summary>
    /// 此方法已设定Position=0，与ToMemoryStream，ToMemoryStream3功能相同
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    public static MemoryStream ToMemoryStream2(this FileInfo fileInfo)
    {
        // 打开文件
        FileStream fileStream = fileInfo.OpenRead();
        // 读取文件的 byte[]
        var bytes = new byte[fileStream.Length];
        _ = fileStream.Read(bytes, 0, bytes.Length);
        fileStream.Close();
        // 把 byte[] 转换成 Stream
        var stream = new MemoryStream(bytes);
        return stream;
    }

    /// <summary>
    /// 与ToMemoryStream，ToMemoryStream2功能相同
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    public static MemoryStream ToMemoryStream3(this FileInfo fileInfo)
    {
        var memoryStream = new MemoryStream();
        using FileStream fileStream = fileInfo.OpenRead();
        fileStream.CopyTo(memoryStream);
        _ = memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    /// <summary>
    /// Sets file attributes for several files at once
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="attributes">The attributes to be set.</param>
    /// <returns>The changed files</returns>
    /// <example>
    /// <code>
    ///var files = directory.GetFiles("*.txt", "*.xml");
    ///files.SetAttributes(FileAttributes.Archive);
    /// </code>
    /// </example>
    public static FileInfo[] SetAttributes(this FileInfo[] files, FileAttributes attributes)
    {
        foreach (FileInfo file in files)
        {
            file.Attributes = attributes;
        }

        return files;
    }

    /// <summary>
    /// Appends file attributes for several files at once (additive to any existing attributes)
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="attributes">The attributes to be set.</param>
    /// <returns>The changed files</returns>
    /// <example>
    /// <code>
    ///var files = directory.GetFiles("*.txt", "*.xml");
    ///files.SetAttributesAdditive(FileAttributes.Archive);
    /// </code>
    /// </example>
    public static FileInfo[] SetAttributesAdditive(this FileInfo[] files, FileAttributes attributes)
    {
        foreach (FileInfo file in files)
        {
            file.Attributes |= attributes;
        }

        return files;
    }

    /// <summary>
    /// An IEnumerable&lt;FileInfo&gt; extension method that deletes the given @this.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    public static void Delete(this IEnumerable<FileInfo> @this)
    {
        foreach (FileInfo t in @this)
        {
            t.Delete();
        }
    }

    /// <summary>
    /// Enumerates for each in this collection.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="action">The action.</param>
    /// <returns>An enumerator that allows foreach to be used to process for each in this collection.</returns>
    public static IEnumerable<FileInfo> ForEach(this IEnumerable<FileInfo> @this, Action<FileInfo> action)
    {
        IEnumerable<FileInfo> iEnumerable = @this.ToList();
        foreach (FileInfo t in iEnumerable)
        {
            action(t);
        }

        return iEnumerable;
    }

    public static FileVersionInfo GetVersionInfo(this FileInfo fileInfo)
    {
        var path = fileInfo.FullName;
        return path.GetVersionInfo();
    }

    /// <summary>
    /// Retrieve the version information
    /// </summary>
    /// <param name="fileFullPath">
    /// The fully qualified path and name of the file to retrieve the version information for.
    /// </param>
    /// <returns></returns>
    public static FileVersionInfo GetVersionInfo(this string fileFullPath)
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(fileFullPath);
        return versionInfo;
    }


    /// <summary>
    /// 获取文件版本
    /// </summary>
    /// <param name="fileFullPath">完整路径 D://A/b.txt</param>
    /// <returns></returns>
    /// <example>
    /// <code>
    ///string fileFullPath = @"D://A/b.txt";
    ///string version = fileFullPath.GetFileVersion();
    /// </code>
    /// </example>
    public static string? GetFileVersion(this string fileFullPath)
    {
        return fileFullPath.GetVersionInfo().FileVersion;
    }

    /// <summary>
    /// Get the file Absolute Path
    /// </summary>
    /// <param name="filePath">D://A/b.txt Or A/b.txt</param>
    /// <returns>D://A Or A</returns>
    /// <example>
    /// <code>
    ///string filePath = "D://A/b.txt";
    ///string version = filePath.GetFilePath();
    /// </code>
    /// </example>
    public static string GetFilePath(this string filePath)
    {
        var fi = new FileInfo(filePath);
        return fi.FullName.Replace(fi.Name, string.Empty);
    }

    /// <summary>
    /// Get the Size of the file
    /// </summary>
    /// <param name="filePath">D://A/b.txt Or A/b.txt</param>
    /// <returns></returns>
    public static string FileSize(this string filePath)
    {
        var fi = new FileInfo(filePath);
        return fi.Length.FormatFileSize();
    }

    public static string FileSize(this FileInfo fileInfo)
    {
        return fileInfo.Length.FormatFileSize();
    }

    /// <summary>
    /// 从文件的绝对路径中获取文件名( 不包含扩展名 )
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    public static string GetFileNameNoExtension(this string filePath)
    {
        var fi = new FileInfo(filePath);
        return fi.GetFileNameNoExtension();
    }

    /// <summary>
    /// 从文件的绝对路径中获取文件名( 不包含扩展名 )
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    public static string GetFileNameNoExtensionString(this string filePath)
    {
        var fileNameNoExtension = filePath.Substring(filePath.LastIndexOf('\\') + 1,
            filePath.LastIndexOf('.') - filePath.LastIndexOf('\\') -
            1);

        return fileNameNoExtension;
    }

    /// <summary>
    /// 获取文件名( 不包含扩展名 )
    /// </summary>
    /// <param name="fileInfo">文件的绝对路径</param>
    public static string GetFileNameNoExtension(this FileInfo fileInfo)
    {
        return fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
    }

    /// <summary>
    /// 获取文件名(包含扩展名)
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetFileNameString(this string filePath)
    {
        var fileName = filePath.Substring(filePath.LastIndexOf('\\') + 1, filePath.Length - 1 - filePath.LastIndexOf('\\'));
        return fileName;
    }

    /// <summary>
    /// 从文件的绝对路径中获取文件路径(不包含文件名)
    /// </summary>
    /// <param name="fileFullPath"></param>
    /// <returns></returns>
    public static string GetFilePathString(this string fileFullPath)
    {
        var filePath = fileFullPath.Substring(0, fileFullPath.LastIndexOf('\\'));
        return filePath;
    }

    /// <summary>
    /// 返回当前 FileInfo 对象的MD5值
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    public static string ToMd5Hash(this FileInfo fileInfo)
    {
        return fileInfo.ToMemoryStream().ToMd5Hash();
    }

    public static byte[] ToMd5HashByte(this FileInfo fileInfo)
    {
        return fileInfo.ToMemoryStream().ToMd5HashByte();
    }

    /// <summary>
    /// 从文件的绝对路径中获取扩展名(包含.)
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    public static string GetExtension(this string filePath)
    {
        //获取文件的名称
        var fi = new FileInfo(filePath);
        return fi.Extension;
    }

    /// <summary>
    /// 从文件的绝对路径中获取扩展名(包含.)
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    public static string GetExtensionString(this string filePath)
    {
        var strExtensionName =
            filePath.Substring(filePath.LastIndexOf(".", StringComparison.Ordinal),
                filePath.Length - filePath.LastIndexOf(".", StringComparison.Ordinal));
        return strExtensionName;
    }

    /// <summary>
    /// 从文件的绝对路径中获取扩展名(不包含.)
    /// </summary>
    /// <param name="filePath">文件的绝对路径</param>
    public static string GetExtensionNotDotString(this string filePath)
    {
        var strExtensionName =
            filePath.Substring(filePath.LastIndexOf(".", StringComparison.Ordinal) + 1,
                filePath.Length - filePath.LastIndexOf(".", StringComparison.Ordinal) - 1);
        return strExtensionName;
    }

    /// <summary>
    /// 获取扩展名(包含.)
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    public static string GetExtension(this FileInfo fileInfo)
    {
        return fileInfo.Extension;
    }
}
