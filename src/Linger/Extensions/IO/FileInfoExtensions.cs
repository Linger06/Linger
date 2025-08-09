using System.Diagnostics;
using Linger.Extensions.Core;
using Linger.Helper;

namespace Linger.Extensions.IO;

/// <summary>
/// Extensions for <see cref="FileInfo"/>.
/// </summary>
public static class FileInfoExtensions
{
    /// <summary>
    /// Renames a file.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="newName">The new name.</param>
    /// <returns>The renamed file.</returns>
    public static FileInfo Rename(this FileInfo file, string newName)
    {
        if (!file.Exists)
        {
            throw new FileNotFoundException("File not found.");
        }

        var rootPath = Path.GetDirectoryName(file.FullName);
        var newPath = Path.Combine(rootPath!, newName);
        file.MoveTo(newPath);
        return file;
    }

    /// <summary>
    /// Renames a file without changing its extension.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="newName">The new name.</param>
    /// <returns>The renamed file.</returns>
    public static FileInfo RenameFileWithoutExtension(this FileInfo file, string newName)
    {
        var fileName = string.Concat(newName, file.Extension);
        return file.Rename(fileName);
    }

    /// <summary>
    /// Changes the file's extension.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="newExtension">The new extension.</param>
    /// <returns>The renamed file.</returns>
    public static FileInfo ChangeExtension(this FileInfo file, string newExtension)
    {
        newExtension = newExtension.EnsureStartsWith(".");
        var fileName = string.Concat(Path.GetFileNameWithoutExtension(file.FullName), newExtension);
        return file.Rename(fileName);
    }

    /// <summary>
    /// Changes the extensions of several files at once.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="newExtension">The new extension.</param>
    /// <returns>The renamed files.</returns>
    public static FileInfo[] ChangeExtensions(this FileInfo[] files, string newExtension)
    {
        ArrayExtensions.ForEach(files, f => f.ChangeExtension(newExtension));
        return files;
    }

    /// <summary>
    /// Deletes several files at once and optionally consolidates any exceptions.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="consolidateExceptions">
    /// If set to <c>true</c>, exceptions are consolidated and the processing is not interrupted.
    /// </param>
    public static void Delete(this IEnumerable<FileInfo> files, bool consolidateExceptions = true)
    {
        var exceptions = new List<Exception>();
        foreach (FileInfo file in files)
        {
            try
            {
                file.Delete();
            }
            catch (Exception e)
            {
                if (consolidateExceptions)
                {
                    exceptions.Add(e);
                }
                else
                {
                    throw;
                }
            }
        }

        if (exceptions is { Count: > 0 })
        {
            throw new AggregateException(
                "Error while deleting one or several files, see InnerExceptions array for details.",
                exceptions);
        }
    }

    /// <summary>
    /// Copies several files to a new folder at once and consolidates any exceptions.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The newly created file copies.</returns>
    public static FileInfo[] CopyTo(this FileInfo[] files, string targetPath)
    {
        return files.CopyTo(targetPath, true);
    }

    /// <summary>
    /// Copies several files to a new folder at once and optionally consolidates any exceptions.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="targetPath">The target path.</param>
    /// <param name="consolidateExceptions">
    /// If set to <c>true</c>, exceptions are consolidated and the processing is not interrupted.
    /// </param>
    /// <returns>The newly created file copies.</returns>
    public static FileInfo[] CopyTo(this FileInfo[] files, string targetPath, bool consolidateExceptions)
    {
        var copiedFiles = new List<FileInfo>();
        List<Exception>? exceptions = null;

        foreach (FileInfo file in files)
        {
            try
            {
                var fileName = Path.Combine(targetPath, file.Name);
                copiedFiles.Add(file.CopyTo(fileName));
            }
            catch (Exception e)
            {
                if (consolidateExceptions)
                {
                    exceptions ??= [];
                    exceptions.Add(e);
                }
                else
                {
                    throw;
                }
            }
        }

        if (exceptions is { Count: > 0 })
        {
            throw new AggregateException(
                "Error while copying one or several files, see InnerExceptions array for details.",
                exceptions);
        }

        return copiedFiles.ToArray();
    }

    /// <summary>
    /// Moves several files to a new folder at once and consolidates any exceptions.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The moved files.</returns>
    public static FileInfo[] MoveTo(this FileInfo[] files, string targetPath)
    {
        return files.MoveTo(targetPath, true);
    }

    /// <summary>
    /// Moves several files to a new folder at once and optionally consolidates any exceptions.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="targetPath">The target path.</param>
    /// <param name="consolidateExceptions">
    /// If set to <c>true</c>, exceptions are consolidated and the processing is not interrupted.
    /// </param>
    /// <returns>The moved files.</returns>
    public static FileInfo[] MoveTo(this FileInfo[] files, string targetPath, bool consolidateExceptions)
    {
        List<Exception>? exceptions = null;

        foreach (FileInfo file in files)
        {
            try
            {
                var fileName = Path.Combine(targetPath, file.Name);
                file.MoveTo(fileName);
            }
            catch (Exception e)
            {
                if (consolidateExceptions)
                {
                    exceptions ??= [];
                    exceptions.Add(e);
                }
                else
                {
                    throw;
                }
            }
        }

        if (exceptions is { Count: > 0 })
        {
            throw new AggregateException(
                "Error while moving one or several files, see InnerExceptions array for details.",
                exceptions);
        }

        return files;
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
            >= 1073741824 => (bytes / (double)1073741824).ToString("0", ExtensionMethodSetting.DefaultCulture) + "G",
            >= 1048576 => (bytes / (double)1048576).ToString("0", ExtensionMethodSetting.DefaultCulture) + "M",
            >= 1024 => (bytes / (double)1024).ToString("0", ExtensionMethodSetting.DefaultCulture) + "K",
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
    GuardExtensions.EnsureFileExists(filePath);

        return new FileInfo(filePath).Length;
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

        var versionInfo = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
        return versionInfo.FileVersion;
    }

    /// <summary>
    /// Converts the current <see cref="FileInfo"/> object to a <see cref="MemoryStream"/> object.
    /// </summary>
    /// <param name="fileInfo">The <see cref="FileInfo"/> object to convert.</param>
    /// <returns>A <see cref="MemoryStream"/> object.</returns>
    /// <example>
    /// <code>
    /// FileInfo file = new FileInfo("path/to/file.txt");
    /// using MemoryStream stream = file.ToMemoryStream();
    /// </code>
    /// </example>
    public static MemoryStream ToMemoryStream(this FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

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
        await fs.CopyToAsync(ms).ConfigureAwait(false);
        return ms.ToArray();
    }
#endif
}
