using System.Diagnostics;
using Linger.Extensions.Core;

namespace Linger.Extensions.IO;

/// <summary>
/// Extensions for <see cref="FileInfo"/>.
/// </summary>
public static partial class FileInfoExtensions
{
    /// <summary>
    /// Sets file attributes for several files at once.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="attributes">The attributes to be set.</param>
    /// <returns>The changed files.</returns>
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// files.SetAttributes(FileAttributes.Archive);
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
    /// Appends file attributes for several files at once (additive to any existing attributes).
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="attributes">The attributes to be set.</param>
    /// <returns>The changed files.</returns>
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// files.SetAttributesAdditive(FileAttributes.Archive);
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
    /// Deletes the given files.
    /// </summary>
    /// <param name="this">The files to delete.</param>
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt");
    /// files.Delete();
    /// </code>
    /// </example>
    public static void Delete(this IEnumerable<FileInfo> @this)
    {
        foreach (FileInfo t in @this)
        {
            t.Delete();
        }
    }

    /// <summary>
    /// Formats a byte count as a string for display (e.g., "1K", "1M", "1G").
    /// </summary>
    /// <param name="bytes">The byte count.</param>
    /// <returns>A formatted string representing the byte count.</returns>
    /// <example>
    /// <code>
    /// int size = 1024;
    /// string formattedSize = size.ToFileSizeBytesString();
    /// </code>
    /// </example>
    public static string ToFileSizeBytesString(this int bytes)
    {
        return bytes switch
        {
            >= 1073741824 => (bytes / (double)1073741824).ToString("0") + "G",
            >= 1048576 => (bytes / (double)1048576).ToString("0") + "M",
            >= 1024 => (bytes / (double)1024).ToString("0") + "K",
            _ => bytes + "Bytes"
        };
    }

    /// <summary>
    /// Retrieves the version information for the specified file.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The version information.</returns>
    /// <example>
    /// <code>
    /// var fileInfo = new FileInfo("path/to/file");
    /// var versionInfo = fileInfo.GetVersionInfo();
    /// </code>
    /// </example>
    public static FileVersionInfo GetVersionInfo(this FileInfo fileInfo)
    {
        var path = fileInfo.FullName;
        return path.GetVersionInfo();
    }

    /// <summary>
    /// Retrieves the version information for the specified file path.
    /// </summary>
    /// <param name="fileFullPath">The fully qualified path and name of the file.</param>
    /// <returns>The version information.</returns>
    /// <example>
    /// <code>
    /// string filePath = "path/to/file";
    /// var versionInfo = filePath.GetVersionInfo();
    /// </code>
    /// </example>
    public static FileVersionInfo GetVersionInfo(this string fileFullPath)
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(fileFullPath);
        return versionInfo;
    }

    /// <summary>
    /// Retrieves the file version for the specified file.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The file version.</returns>
    /// <example>
    /// <code>
    /// var fileInfo = new FileInfo("path/to/file");
    /// string version = fileInfo.GetFileVersion();
    /// </code>
    /// </example>
    public static string? GetFileVersion(this FileInfo fileInfo)
    {
        return fileInfo.GetVersionInfo().FileVersion;
    }

    /// <summary>
    /// Retrieves the file version for the specified file path.
    /// </summary>
    /// <param name="fileFullPath">The fully qualified path and name of the file.</param>
    /// <returns>The file version.</returns>
    /// <example>
    /// <code>
    /// string fileFullPath = "path/to/file";
    /// string version = fileFullPath.GetFileVersion();
    /// </code>
    /// </example>
    public static string? GetFileVersion(this string fileFullPath)
    {
        return fileFullPath.GetVersionInfo().FileVersion;
    }

    /// <summary>
    /// Retrieves the absolute path of the specified file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The absolute path.</returns>
    /// <example>
    /// <code>
    /// string filePath = "path/to/file";
    /// string absolutePath = filePath.GetFilePath();
    /// </code>
    /// </example>
    public static string? GetFilePath(this string filePath)
    {
        FileInfo fi = new FileInfo(filePath);
        return fi.DirectoryName;
    }

    /// <summary>
    /// Retrieves the absolute path of the specified file.
    /// </summary>
    /// <param name="fi">The file.</param>
    /// <returns>The absolute path.</returns>
    /// <example>
    /// <code>
    /// var fileInfo = new FileInfo("path/to/file");
    /// string absolutePath = fileInfo.GetFilePath();
    /// </code>
    /// </example>
    public static string? GetFilePath(this FileInfo fi)
    {
        return fi.DirectoryName;
    }

    /// <summary>
    /// Retrieves the size of the specified file.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file size.</returns>
    /// <example>
    /// <code>
    /// string filePath = "path/to/file";
    /// string size = filePath.FileSize();
    /// </code>
    /// </example>
    public static string FileSize(this string filePath)
    {
        var fi = new FileInfo(filePath);
        return fi.Length.FileSize();
    }

    /// <summary>
    /// Retrieves the size of the specified file.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The file size.</returns>
    /// <example>
    /// <code>
    /// var fileInfo = new FileInfo("path/to/file");
    /// string size = fileInfo.FileSize();
    /// </code>
    /// </example>
    public static string FileSize(this FileInfo fileInfo)
    {
        return fileInfo.Length.FileSize();
    }

    /// <summary>
    /// Retrieves the file name without extension from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file name without extension.</returns>
    /// <example>
    /// <code>
    /// string filePath = "path/to/file.txt";
    /// string fileName = filePath.GetFileNameNoExtension();
    /// </code>
    /// </example>
    public static string GetFileNameWithoutExtension(this string filePath)
    {
        var fi = new FileInfo(filePath);
        return fi.GetFileNameWithoutExtension();
    }

    /// <summary>
    /// Retrieves the file name without extension from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file name without extension.</returns>
    /// <example>
    /// <code>
    /// string filePath = "path/to/file.txt";
    /// string fileName = filePath.GetFileNameNoExtensionString();
    /// </code>
    /// </example>
    public static string GetFileNameWithoutExtensionString(this string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>
    /// Retrieves the file name without extension from the specified file.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The file name without extension.</returns>
    /// <example>
    /// <code>
    /// var fileInfo = new FileInfo("path/to/file.txt");
    /// string fileName = fileInfo.GetFileNameNoExtension();
    /// </code>
    /// </example>
    public static string GetFileNameWithoutExtension(this FileInfo fileInfo)
    {
        return fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
    }

    /// <summary>
    /// Retrieves the file name with extension from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file name with extension.</returns>
    /// <example>
    /// <code>
    /// string filePath = "path/to/file.txt";
    /// string fileName = filePath.GetFileNameString();
    /// </code>
    /// </example>
    public static string GetFileNameString(this string filePath)
    {
        return Path.GetFileName(filePath);
    }

    /// <summary>
    /// Retrieves the file path without the file name from the specified file path.
    /// </summary>
    /// <param name="fileFullPath">The file path.</param>
    /// <returns>The file path without the file name.</returns>
    /// <example>
    /// <code>
    /// string fileFullPath = "path/to/file.txt";
    /// string filePath = fileFullPath.GetFilePathString();
    /// </code>
    /// </example>
    public static string? GetFilePathString(this string fileFullPath)
    {
        return Path.GetDirectoryName(fileFullPath);
    }

    /// <summary>
    /// Returns the MD5 hash of the specified file.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The MD5 hash.</returns>
    /// <example>
    /// <code>
    /// var fileInfo = new FileInfo("path/to/file");
    /// string md5Hash = fileInfo.ToMd5Hash();
    /// </code>
    /// </example>
    public static string ToMd5Hash(this FileInfo fileInfo)
    {
        return fileInfo.ToMemoryStream().ToMd5Hash();
    }

    /// <summary>
    /// Returns the MD5 hash as a byte array of the specified file.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The MD5 hash as a byte array.</returns>
    /// <example>
    /// <code>
    /// var fileInfo = new FileInfo("path/to/file");
    /// byte[] md5Hash = fileInfo.ToMd5HashByte();
    /// </code>
    /// </example>
    public static byte[] ToMd5HashByte(this FileInfo fileInfo)
    {
        return fileInfo.ToMemoryStream().ToMd5HashByte();
    }

    /// <summary>
    /// Retrieves the file extension (including the dot) from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file extension.</returns>
    /// <example>
    /// <code>
    /// string filePath = "path/to/file.txt";
    /// string extension = filePath.GetExtension();
    /// </code>
    /// </example>
    public static string GetExtension(this string filePath)
    {
        var fi = new FileInfo(filePath);
        return fi.Extension;
    }

    /// <summary>
    /// Retrieves the file extension (including the dot) from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file extension.</returns>
    /// <example>
    /// <code>
    /// string filePath = "path/to/file.txt";
    /// string extension = filePath.GetExtensionString();
    /// </code>
    /// </example>
    public static string GetExtensionString(this string filePath)
    {
        return Path.GetExtension(filePath);
    }

    /// <summary>
    /// Retrieves the file extension (excluding the dot) from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file extension.</returns>
    /// <example>
    /// <code>
    /// string filePath = "path/to/file.txt";
    /// string extension = filePath.GetExtensionNotDotString();
    /// </code>
    /// </example>
    public static string GetExtensionWithoutDotString(this string filePath)
    {
        return Path.GetExtension(filePath).Replace(".", string.Empty);
    }

    /// <summary>
    /// Retrieves the file extension (including the dot) from the specified file.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The file extension.</returns>
    /// <example>
    /// <code>
    /// var fileInfo = new FileInfo("path/to/file.txt");
    /// string extension = fileInfo.GetExtension();
    /// </code>
    /// </example>
    public static string GetExtension(this FileInfo fileInfo)
    {
        return fileInfo.Extension;
    }

#if NET451_OR_GREATER || NETSTANDARD|| NET5_0_OR_GREATER
    /// <summary>
    /// Asynchronously retrieves the file data as a byte array.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the file data as a byte array.</returns>
    /// <example>
    /// <code>
    /// string filePath = "path/to/file";
    /// byte[] data = await filePath.GetFileDataAsync();
    /// </code>
    /// </example>
    public static async Task<byte[]> GetFileDataAsync(this string filePath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using var ms = new MemoryStream(ExtensionMethodSetting.DefaultBufferSize);
        await fs.CopyToAsync(ms);
        return ms.ToArray();
    }
#endif
}
