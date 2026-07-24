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
        Array.ForEach(files, f => f.ChangeExtension(newExtension));
        return files;
    }

    /// <summary>
    /// Deletes several files at once and optionally consolidates any exceptions.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="consolidateExceptions">
    /// If set to <c>true</c>, exceptions are consolidated and the processing is not interrupted.
    /// </param>
    public static void Delete(this IEnumerable<FileInfo> files, bool consolidateExceptions)
    {
        _ = ExecuteFileBatch(
            files,
            static file =>
            {
                file.Delete();
                return file;
            },
            consolidateExceptions,
            "Error while deleting one or several files, see InnerExceptions array for details.");
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
        return ExecuteFileBatch(
                files,
                file => file.CopyTo(Path.Combine(targetPath, file.Name)),
                consolidateExceptions,
                "Error while copying one or several files, see InnerExceptions array for details.")
            .ToArray();
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
        return ExecuteFileBatch(
                files,
                file =>
                {
                    file.MoveTo(Path.Combine(targetPath, file.Name));
                    return file;
                },
                consolidateExceptions,
                "Error while moving one or several files, see InnerExceptions array for details.")
            .ToArray();
    }

    private static List<TResult> ExecuteFileBatch<TResult>(
        IEnumerable<FileInfo> files,
        Func<FileInfo, TResult> operation,
        bool consolidateExceptions,
        string aggregateMessage)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(operation);

        var results = new List<TResult>();
        List<Exception>? exceptions = null;

        foreach (FileInfo file in files)
        {
            try
            {
                results.Add(operation(file));
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || PathHelper.IsPathException(ex))
            {
                if (!consolidateExceptions)
                    throw;

                exceptions ??= [];
                exceptions.Add(ex);
            }
        }

        if (exceptions is { Count: > 0 })
            throw new AggregateException(aggregateMessage, exceptions);

        return results;
    }

    /// <summary>
    /// Gets the file size in bytes from a file path
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    public static long GetFileSize(this string filePath)
    {
        // EnsureFileExists performs the null/empty and existence checks.
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
        ArgumentNullException.ThrowIfNull(fileInfo);
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
    public static Task<byte[]> GetFileDataAsync(this string filePath)
    {
        return filePath.GetFileDataAsync(CancellationToken.None);
    }

    /// <summary>
    /// Asynchronously retrieves the file data as a byte array.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task whose result contains the file data.</returns>
    public static async Task<byte[]> GetFileDataAsync(this string filePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        cancellationToken.ThrowIfCancellationRequested();

        using var fs = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);
        if (fs.Length > int.MaxValue)
        {
            throw new IOException("The file is too large to fit in a byte array.");
        }

        var result = new byte[(int)fs.Length];
        var offset = 0;
        while (offset < result.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
#if NET5_0_OR_GREATER
            int bytesRead = await fs.ReadAsync(
                result.AsMemory(offset, result.Length - offset),
                cancellationToken).ConfigureAwait(false);
#else
            int bytesRead = await fs.ReadAsync(
                result,
                offset,
                result.Length - offset,
                cancellationToken).ConfigureAwait(false);
#endif
            if (bytesRead == 0)
            {
                throw new EndOfStreamException("The file ended before the expected number of bytes was read.");
            }

            offset += bytesRead;
        }

        return result;
    }
#endif
}
