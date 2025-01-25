using System.Diagnostics;
using Linger.Extensions.Core;
using Linger.Helper;

namespace Linger.Extensions.IO;

/// <summary>
/// Extensions for <see cref="FileInfo"/>.
/// </summary>
public static partial class FileInfoExtensions
{
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
    /// Gets the file size in bytes from a file path
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    public static long GetFileSize(this string filePath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(filePath));
        GuardExtensions.EnsureFileExist(filePath);
        try
        {
            return new FileInfo(filePath).Length;
        }
        catch (System.Exception ex)
        {
            throw new IOException($"Failed to get size for file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Gets formated file size string (e.g. "1.5 MB" from a file path)
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file size.</returns>
    public static string GetFileSizeFormatted(this string filePath)
    {
        var bytes = GetFileSize(filePath);
        return bytes.FormatFileSize();
    }

    /// <summary>
    /// Retrieves the size of the specified file.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The file size.</returns>
    public static string GetFileSizeFormatted(this FileInfo fileInfo)
    {
        return fileInfo.Length.FormatFileSize();
    }

    /// <summary>
    /// Gets the file version from a FileInfo object.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The file version.</returns>
    public static string? GetFileVersion(this FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(fileInfo));
        if (!fileInfo.Exists)
            throw new FileNotFoundException("File not found", fileInfo.FullName);

        try
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
            return versionInfo.FileVersion;
        }
        catch (System.Exception ex)
        {
            throw new IOException($"Failed to get version for file: {fileInfo.FullName}", ex);
        }
    }

    /// <summary>
    /// Gets the file version from a file path.
    /// </summary>
    /// <param name="fileFullPath">The fully qualified path and name of the file.</param>
    /// <returns>The file version.</returns>
    public static string? GetFileVersion(this string fileFullPath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(fileFullPath));
        GuardExtensions.EnsureFileExist(fileFullPath);
        try
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(fileFullPath);
            return versionInfo.FileVersion;
        }
        catch (System.Exception ex)
        {
            throw new IOException($"Failed to get version for file: {fileFullPath}", ex);
        }
    }

    /// <summary>
    /// Gets the directory path from a file path string.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The absolute path.</returns>
    public static string? GetDirectoryPath(this string filePath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(filePath));
        return Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? throw new DirectoryNotFoundException($"Directory not found for path: {filePath}");
    }

    /// <summary>
    /// Retrieves the full path of the specified file.
    /// </summary>
    /// <param name="fi">The file.</param>
    /// <returns>The absolute path.</returns>
    public static string? GetFilePath(this string filePath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(filePath));
        return Path.GetFullPath(filePath);
    }

    /// <summary>
    /// Retrieves the full path from a FileInfo object.
    /// </summary>
    /// <param name="fi">The file.</param>
    /// <returns>The absolute path.</returns>
    public static string? GetFilePath(this FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(fileInfo));
        return fileInfo.FullName;
    }

    /// <summary>
    /// Retrieves the file name without extension from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file name without extension.</returns>
    public static string GetFileNameWithoutExtension(this string filePath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(filePath));
        return Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>
    /// Retrieves the file name without extension from the specified file.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The file name without extension.</returns>
    /// </example>
    public static string GetFileNameWithoutExtension(this FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(fileInfo));
        return Path.GetFileNameWithoutExtension(fileInfo.Name);
    }

    /// <summary>
    /// Retrieves the file name with extension from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file name with extension.</returns>
    public static string GetFileName(this string filePath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(filePath));
        return Path.GetFileName(filePath);
    }

    /// <summary>
    /// Retrieves the file extension (including the dot) from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file extension.</returns>
    public static string GetExtension(this string filePath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(filePath));
        return Path.GetExtension(filePath);
    }

    /// <summary>
    /// Retrieves the file extension without the dot from the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The file extension.</returns>
    public static string GetExtensionWithoutDot(this string filePath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(filePath));
        return Path.GetExtension(filePath).TrimStart('.');
    }

    /// <summary>
    /// Retrieves the file extension (including the dot) from the specified file.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The file extension.</returns>
    public static string GetExtension(this FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(nameof(fileInfo));
        return fileInfo.Extension;
    }

    /// <summary>
    /// Returns the MD5 hash of the specified file.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The MD5 hash.</returns>
    public static string ToMd5Hash(this FileInfo fileInfo)
    {
        return fileInfo.ToMemoryStream().ToMd5Hash();
    }

    /// <summary>
    /// Returns the MD5 hash as a byte array of the specified file.
    /// </summary>
    /// <param name="fileInfo">The file.</param>
    /// <returns>The MD5 hash as a byte array.</returns>
    public static byte[] ToMd5HashByte(this FileInfo fileInfo)
    {
        return fileInfo.ToMemoryStream().ToMd5HashByte();
    }

    /// <summary>
    /// Converts the current <see cref="FileInfo"/> object to a <see cref="MemoryStream"/> object.
    /// </summary>
    /// <param name="fileInfo">The <see cref="FileInfo"/> object to convert.</param>
    /// <param name="deleteFile">Whether to delete the file after conversion.</param>
    /// <returns>A <see cref="MemoryStream"/> object.</returns>
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
    /// Converts the current <see cref="FileInfo"/> object to a <see cref="MemoryStream"/> object. This method sets the position to 0.
    /// </summary>
    /// <param name="fileInfo">The <see cref="FileInfo"/> object to convert.</param>
    /// <returns>A <see cref="MemoryStream"/> object.</returns>
    public static MemoryStream ToMemoryStream2(this FileInfo fileInfo)
    {
        FileStream fileStream = fileInfo.OpenRead();
        var bytes = new byte[fileStream.Length];
        _ = fileStream.Read(bytes, 0, bytes.Length);
        fileStream.Close();
        var stream = new MemoryStream(bytes);
        return stream;
    }

    /// <summary>
    /// Converts the current <see cref="FileInfo"/> object to a <see cref="MemoryStream"/> object.
    /// </summary>
    /// <param name="fileInfo">The <see cref="FileInfo"/> object to convert.</param>
    /// <returns>A <see cref="MemoryStream"/> object.</returns>
    public static MemoryStream ToMemoryStream3(this FileInfo fileInfo)
    {
        var memoryStream = new MemoryStream();
        using FileStream fileStream = fileInfo.OpenRead();
        fileStream.CopyTo(memoryStream);
        _ = memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    /// <summary>
    /// Computes the MD5 hash of the file.
    /// </summary>
    /// <param name="fileInfo">The <see cref="FileInfo"/> object.</param>
    /// <returns>The MD5 hash as a string.</returns>
    public static string ComputeHashMd5(this FileInfo fileInfo)
    {
        using var fileStream = fileInfo.OpenRead();
        var arrayHashValue = fileStream.ToMd5HashByte();
        return arrayHashValue.ToMd5HashCode();
    }


#if NET451_OR_GREATER || NETSTANDARD|| NET5_0_OR_GREATER
    /// <summary>
    /// Asynchronously retrieves the file data as a byte array.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the file data as a byte array.</returns>
    public static async Task<byte[]> GetFileDataAsync(this string filePath)
    {
        using FileStream fs = File.OpenRead(filePath);
        using var ms = new MemoryStream(ExtensionMethodSetting.DefaultBufferSize);
        await fs.CopyToAsync(ms);
        return ms.ToArray();
    }
#endif
}
