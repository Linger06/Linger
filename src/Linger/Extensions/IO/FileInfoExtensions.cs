using Linger.Extensions.Core;

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
}