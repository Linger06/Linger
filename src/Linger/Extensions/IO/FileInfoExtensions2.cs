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
    /// Deletes each file using the legacy fail-fast behavior.
    /// </summary>
    [Obsolete("Use Delete(files, consolidateExceptions) and choose the exception behavior explicitly. This API will be removed in 2.0.")]
    public static void Delete(this IEnumerable<FileInfo> files)
    {
        if (files == null) return;

        foreach (FileInfo file in files)
        {
            if (file.Exists) file.Delete();
        }
    }

    /// <summary>
    /// Retrieves version metadata for a physical file.
    /// </summary>
    [Obsolete("Use FileVersionInfo.GetVersionInfo(fileInfo.FullName) and apply an explicit platform policy. This API will be removed in 2.0.")]
    public static FileVersionInfo? GetVersionInfo(this FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;

        return FileVersionInfo.GetVersionInfo(fileInfo.FullName);
    }

    /// <summary>
    /// Gets the file version string from a path.
    /// Returns <see langword="null"/> on non-Windows platforms.
    /// </summary>
    [Obsolete("Use new FileInfo(fileFullPath).GetFileVersion() instead. This API will be removed in 2.0.")]
    public static string? GetFileVersion(this string fileFullPath)
    {
        if (string.IsNullOrWhiteSpace(fileFullPath)) return null;
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;

        return FileVersionInfo.GetVersionInfo(fileFullPath).FileVersion;
    }

    /// <summary>
    /// Gets the directory portion of a file path and appends the native separator.
    /// </summary>
    [Obsolete("Prefer Path.GetDirectoryName(filePath) and append a separator explicitly when needed. This API will be removed in 2.0.")]
    public static string GetFilePath(this string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return string.Empty;
        var directory = Path.GetDirectoryName(filePath);
        if (directory == null) return string.Empty;

        // Normalize the return value to a directory path with a trailing separator.
        if (directory.Length > 0 && !directory.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            directory += Path.DirectorySeparatorChar;
        }

        return directory;
    }

    /// <summary>
    /// Gets the formatted size string for a file path.
    /// </summary>
    /// <param name="filePath">Physical file path.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the path is null, blank, or contains invalid characters.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the target file does not exist.
    /// </exception>
    [Obsolete("Use filePath.GetFileSizeFormatted() instead. This API will be removed in 2.0.")]
    public static string FileSize(this string filePath)
    {
        // Guard against blank inputs before touching the file system.
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null, empty, or whitespace.", nameof(filePath));
        }

        // Let path validation fail fast with a clear exception.
        if (filePath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        {
            throw new ArgumentException($"File path contains invalid characters: {filePath}", nameof(filePath));
        }

        // Match the strict contract: the file must exist.
        var fi = new FileInfo(filePath);
        if (!fi.Exists)
        {
            throw new FileNotFoundException("Target file for size calculation does not exist.", filePath);
        }

        return fi.Length.FormatFileSize();
    }

    /// <summary>
    /// Gets the formatted size string from a <see cref="FileInfo"/> instance.
    /// </summary>
    [Obsolete("Use fileInfo.GetFileSizeFormatted() instead. This API will be removed in 2.0.")]
    public static string FileSize(this FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return fileInfo.Length.FormatFileSize();
    }

    /// <summary>
    /// Gets the file name without its extension from a path.
    /// </summary>
    [Obsolete("Use Path.GetFileNameWithoutExtension(filePath) instead. This API will be removed in 2.0.")]
    public static string GetFileNameNoExtension(this string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>
    /// Gets the file name without its extension from a <see cref="FileInfo"/> instance.
    /// </summary>
    [Obsolete("Use Path.GetFileNameWithoutExtension(fileInfo.Name) instead. This API will be removed in 2.0.")]
    public static string GetFileNameNoExtension(this FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        return Path.GetFileNameWithoutExtension(fileInfo.Name);
    }

    /// <summary>
    /// Gets the extension text without the leading dot.
    /// </summary>
    [Obsolete("Use Path.GetExtension(filePath).TrimStart('.') instead. This API will be removed in 2.0.")]
    public static string GetExtensionNotDotString(this string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return string.Empty;

        var ext = Path.GetExtension(filePath);
        return ext.Length > 1 ? ext.Substring(1) : string.Empty;
    }
}
