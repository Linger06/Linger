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
        catch (Exception ex)
        {
            throw new IOException($"Failed to get version for file: {fileInfo.FullName}", ex);
        }
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
