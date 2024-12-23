﻿using Linger.Extensions.Core;

namespace Linger.Extensions.IO;

/// <summary>
/// Extensions for <see cref="FileInfo"/>.
/// </summary>
public static partial class FileInfoExtensions
{
    /// <summary>
    /// Converts the current <see cref="FileInfo"/> object to a <see cref="MemoryStream"/> object.
    /// </summary>
    /// <param name="fileInfo">The <see cref="FileInfo"/> object to convert.</param>
    /// <param name="deleteFile">Whether to delete the file after conversion.</param>
    /// <returns>A <see cref="MemoryStream"/> object.</returns>
    /// <example>
    /// <code>
    /// var file = new FileInfo(@"c:\test.txt");
    /// using var memoryStream = file.ToMemoryStream();
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// var file = new FileInfo(@"c:\test.txt");
    /// using var memoryStream = file.ToMemoryStream2();
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// var file = new FileInfo(@"c:\test.txt");
    /// using var memoryStream = file.ToMemoryStream3();
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// var file = new FileInfo(@"c:\test.txt");
    /// string hash = file.ComputeHashMd5();
    /// </code>
    /// </example>
    public static string ComputeHashMd5(this FileInfo fileInfo)
    {
        using var fileStream = fileInfo.OpenRead();
        var arrayHashValue = fileStream.ToMd5HashByte();
        return BitConverter.ToString(arrayHashValue).Replace("-", string.Empty);
    }

    /// <summary>
    /// Renames a file.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="newName">The new name.</param>
    /// <returns>The renamed file.</returns>
    /// <example>
    /// <code>
    /// var file = new FileInfo(@"c:\test.txt");
    /// file.Rename("test2.txt");
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// var file = new FileInfo(@"c:\test.txt");
    /// file.RenameFileWithoutExtension("test3");
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// var file = new FileInfo(@"c:\test.txt");
    /// file.ChangeExtension("xml");
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// files.ChangeExtensions("tmp");
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// files.Delete();
    /// </code>
    /// </example>
    public static void Delete(this FileInfo[] files, bool consolidateExceptions = true)
    {
        if (consolidateExceptions)
        {
            var exceptions = new List<System.Exception>();

            foreach (FileInfo file in files)
            {
                try
                {
                    file.Delete();
                }
                catch (System.Exception e)
                {
                    exceptions.Add(e);
                }
            }

            if (exceptions.Count != 0)
            {
                throw new AggregateException(
                    "Error while deleting one or several files, see InnerExceptions array for details.",
                    exceptions);
            }
        }
        else
        {
            foreach (FileInfo file in files)
            {
                file.Delete();
            }
        }
    }

    /// <summary>
    /// Copies several files to a new folder at once and consolidates any exceptions.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="targetPath">The target path.</param>
    /// <returns>The newly created file copies.</returns>
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// var copiedFiles = files.CopyTo(@"c:\temp\");
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// var copiedFiles = files.CopyTo(@"c:\temp\");
    /// </code>
    /// </example>
    public static FileInfo[] CopyTo(this FileInfo[] files, string targetPath, bool consolidateExceptions)
    {
        var copiedFiles = new List<FileInfo>();
        List<System.Exception>? exceptions = null;

        foreach (FileInfo file in files)
        {
            try
            {
                var fileName = Path.Combine(targetPath, file.Name);
                copiedFiles.Add(file.CopyTo(fileName));
            }
            catch (System.Exception e)
            {
                if (consolidateExceptions)
                {
                    exceptions ??= new List<System.Exception>();
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
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// files.MoveTo(@"c:\temp\");
    /// </code>
    /// </example>
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
    /// <example>
    /// <code>
    /// var files = directory.GetFiles("*.txt", "*.xml");
    /// files.MoveTo(@"c:\temp\");
    /// </code>
    /// </example>
    public static FileInfo[] MoveTo(this FileInfo[] files, string targetPath, bool consolidateExceptions)
    {
        List<System.Exception>? exceptions = null;

        foreach (FileInfo file in files)
        {
            try
            {
                var fileName = Path.Combine(targetPath, file.Name);
                file.MoveTo(fileName);
            }
            catch (System.Exception e)
            {
                if (consolidateExceptions)
                {
                    exceptions ??= new List<System.Exception>();
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
