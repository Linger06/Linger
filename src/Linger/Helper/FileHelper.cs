using System.Text;
using Linger.Extensions.Core;
using Linger.Extensions.IO;

namespace Linger.Helper;

public static class FileHelper
{
    #region File Existence & Information

    public static bool IsExistFile(string filePath)
    {
        return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
    }

    public static long GetFileSize(string filePath)
    {
        filePath.EnsureFileExist();
        return new FileInfo(filePath).Length;
    }

    public static int GetLineCount(string filePath)
    {
        filePath.EnsureFileExist();

        var count = 0;
        using (var reader = new StreamReader(filePath))
        {
            while (reader.ReadLine() != null)
            {
                count++;
            }
        }
        return count;
    }

    #endregion

    #region File Read Operations

    public static string ReadText(string filename, Encoding? encoding = null)
    {
        filename.EnsureFileExist();

        encoding ??= ExtensionMethodSetting.DefaultEncoding;

        using (var sr = new StreamReader(filename, encoding))
        {
            return sr.ReadToEnd();
        }
    }

    public static bool TryReadText(string filename, out string content, Encoding? encoding = null)
    {
        content = string.Empty;
        if (!IsExistFile(filename))
        {
            return false;
        }

        try
        {
            content = ReadText(filename, encoding);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region File Write Operations

    public static void WriteText(string filePath, string text, Encoding encoding)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));
        if (encoding == null)
            throw new ArgumentNullException(nameof(encoding));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            CreateDirectoryIfNotExists(directory);
        }
        File.WriteAllText(filePath, text, encoding);
    }

    public static void AppendText(string filePath, string content)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            CreateDirectoryIfNotExists(directory);
        }
        File.AppendAllText(filePath, content);
    }

    #endregion

    #region File Operations

    public static void MoveFile(string sourceFilePath, string destDirectoryPath)
    {
        sourceFilePath.EnsureFileExist();

        var sourceFileName = Path.GetFileName(sourceFilePath);
        CreateDirectoryIfNotExists(destDirectoryPath);
        var destFileName = Path.Combine(destDirectoryPath, sourceFileName);
        File.Move(sourceFilePath, destFileName);
    }

    public static void CopyFile(string originFile, string newFile)
    {
        originFile.EnsureFileExist();

        var directory = Path.GetDirectoryName(newFile);
        if (!string.IsNullOrEmpty(directory))
        {
            CreateDirectoryIfNotExists(directory);
        }
        File.Copy(originFile, newFile, true);
    }

    public static void DeleteFileIfExists(string file)
    {
        if (string.IsNullOrEmpty(file))
            return;

        if (File.Exists(file))
        {
            File.Delete(file);
        }
    }

    public static string GetFileName(string filePath)
    {
        return string.IsNullOrEmpty(filePath) ? string.Empty : Path.GetFileName(filePath);
    }

    public static void ClearFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

        File.WriteAllBytes(filePath, []);
    }

    public static void CreateFile(string filePath, string content, Encoding? encoding = null)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            CreateDirectoryIfNotExists(directory);
        }

        encoding ??= ExtensionMethodSetting.DefaultEncoding;
        File.WriteAllText(filePath, content, encoding);
    }

    public static void CreateFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            CreateDirectoryIfNotExists(directory);
        }

        if (!IsExistFile(filePath))
        {
            using (File.Create(filePath))
            {
                // File is automatically closed by using statement
            }
        }
    }

    public static void CreateFile(string filePath, byte[] buffer)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            CreateDirectoryIfNotExists(directory);
        }

        if (!IsExistFile(filePath))
        {
            using (var fs = File.Create(filePath))
            {
                fs.Write(buffer, 0, buffer.Length);
            }
        }
    }

    #endregion

    #region Path & Extension Operations

    public static string GetExtension(string filename)
    {
        return string.IsNullOrEmpty(filename) ? string.Empty : Path.GetExtension(filename);
    }

    public static string GetPostfixStr(string filename)
    {
        return GetExtension(filename);
    }

    #endregion

    #region Search Operations

    public static bool Contains(string directoryPath, string searchPattern)
    {
        return Contains(directoryPath, searchPattern, false);
    }

    public static bool Contains(string directoryPath, string searchPattern, bool isSearchChild)
    {
        if (string.IsNullOrEmpty(directoryPath) || string.IsNullOrEmpty(searchPattern))
            return false;

        var fileNames = GetFileNames(directoryPath, searchPattern, isSearchChild);
        return fileNames.Length > 0;
    }

    #endregion

    #region File Information

    public static CustomExistFileInfo? GetCustomFileInfo(string fullFileName)
    {
        if (string.IsNullOrEmpty(fullFileName))
            return null;

        var absolutePath = fullFileName.GetAbsolutePath();
        var file = new FileInfo(absolutePath);
        if (file.Exists)
        {
            using (var memoryStream = file.ToMemoryStream2())
            {
                var strHashData = memoryStream.ComputeHashMd5();
                return new CustomExistFileInfo
                {
                    HashData = strHashData,
                    FileName = file.Name,
                    RelativeFilePath = absolutePath.GetRelativePath(),
                    FullFilePath = file.FullName,
                    FileSize = file.Length.FileSize(),
                    Length = file.Length
                };
            }
        }
        return null;
    }

    #endregion

    #region Directory Existence & Creation

    public static bool IsExistDirectory(string directoryPath)
    {
        return !string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath);
    }

    public static DirectoryInfo CreateDirectoryIfNotExists(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
            throw new ArgumentNullException(nameof(directoryPath));

        return Directory.CreateDirectory(directoryPath);
    }

    #endregion

    #region Directory Content Operations

    public static List<string> GetFileNames(string directoryPath, bool containPath = true, bool containExtension = true)
    {
        directoryPath.EnsureDirectoryExist();
        var fileArray = Directory.GetFiles(directoryPath);
        var fileList = new List<string>(fileArray);

        if (containPath && containExtension)
        {
            return fileList;
        }

        return fileList.Select(file =>
        {
            if (!containPath && containExtension)
                return file.GetFileNameString();
            if (!containPath && !containExtension)
                return file.GetFileNameWithoutExtensionString();
            return file;
        }).ToList();
    }

    public static string[] GetDirectories(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
            throw new ArgumentNullException(nameof(directoryPath));

        return Directory.GetDirectories(directoryPath);
    }

    public static string[] GetDirectories(string directoryPath, string searchPattern, bool isSearchChild)
    {
        if (string.IsNullOrEmpty(directoryPath))
            throw new ArgumentNullException(nameof(directoryPath));
        if (string.IsNullOrEmpty(searchPattern))
            throw new ArgumentNullException(nameof(searchPattern));

        return Directory.GetDirectories(
            directoryPath,
            searchPattern,
            isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
        );
    }

    public static string[] GetFileNames(string directoryPath, string searchPattern, bool isSearchChild)
    {
        if (string.IsNullOrEmpty(directoryPath))
            throw new ArgumentNullException(nameof(directoryPath));
        if (string.IsNullOrEmpty(searchPattern))
            throw new ArgumentNullException(nameof(searchPattern));

        if (!IsExistDirectory(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        return Directory.GetFiles(
            directoryPath,
            searchPattern,
            isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
        );
    }

    public static bool IsEmptyDirectory(string directory)
    {
        directory.EnsureDirectoryExist();
        return !Directory.EnumerateFileSystemEntries(directory).Any();
    }

    #endregion

    #region Directory Copy Operations

    public static void CopyDir(string srcDirectory, string destDirectory)
    {
        if (string.IsNullOrEmpty(srcDirectory))
            throw new ArgumentNullException(nameof(srcDirectory));
        if (string.IsNullOrEmpty(destDirectory))
            throw new ArgumentNullException(nameof(destDirectory));

        srcDirectory.EnsureDirectoryExist();

        // Ensure the destination path ends with a directory separator
        destDirectory = Path.GetFullPath(destDirectory.TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar);

        // Create destination directory if it doesn't exist
        CreateDirectoryIfNotExists(destDirectory);

        // Get all entries (files and directories)
        var fileList = Directory.GetFileSystemEntries(srcDirectory);

        foreach (var file in fileList)
        {
            var destFile = Path.Combine(destDirectory, Path.GetFileName(file));

            if (Directory.Exists(file))
            {
                CopyDir(file, destFile);
            }
            else
            {
                File.Copy(file, destFile, true);
            }
        }
    }

    #endregion

    #region Directory Operations

 

    public static void ClearDirectory(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
            throw new ArgumentNullException(nameof(directoryPath));

        if (!IsExistDirectory(directoryPath))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directoryPath))
        {
            DeleteFileIfExists(file);
        }

        foreach (var dir in Directory.EnumerateDirectories(directoryPath))
        {
            DeleteDirectory(dir);
        }
    }

    public static void DeleteDirectory(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
            return;

        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
    }

    #endregion
}
