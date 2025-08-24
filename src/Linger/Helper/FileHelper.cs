using System.Text;
using Linger.Extensions.Core;
using Linger.Extensions.IO;
using Linger.Helper.PathHelpers;

namespace Linger.Helper;

public static class FileHelper
{
    #region File Read Operations

    public static string ReadText(string filename, Encoding? encoding = null)
    {
        filename.EnsureFileExists();

        encoding ??= ExtensionMethodSetting.DefaultEncoding;

        using var sr = new StreamReader(filename, encoding);
        return sr.ReadToEnd();
    }

    public static bool TryReadText(string filename, out string content, Encoding? encoding = null)
    {
        content = string.Empty;
        if (!File.Exists(filename))
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

    public static void WriteText(string filePath, string text, Encoding? encoding = null)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new System.ArgumentNullException(nameof(filePath));
        encoding ??= ExtensionMethodSetting.DefaultEncoding;

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(filePath, text, encoding);
    }

    public static void AppendText(string filePath, string content)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new System.ArgumentNullException(nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.AppendAllText(filePath, content);
    }

    /// <summary>
    /// 尝试写入文本，失败返�?false 不抛异常�?
    /// </summary>
    public static bool TryWriteText(string filePath, string text, Encoding? encoding = null)
    {
        try
        {
            WriteText(filePath, text, encoding);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 尝试追加文本，失败返�?false 不抛异常�?
    /// </summary>
    public static bool TryAppendText(string filePath, string content)
    {
        try
        {
            AppendText(filePath, content);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region File Operations

    public static void MoveFile(string sourceFilePath, string destDirectoryPath)
    {
        sourceFilePath.EnsureFileExists();

        var sourceFileName = Path.GetFileName(sourceFilePath);
        Directory.CreateDirectory(destDirectoryPath);
        var destFileName = Path.Combine(destDirectoryPath, sourceFileName);
        File.Move(sourceFilePath, destFileName);
    }

    public static void CopyFile(string sourceFile, string destFile)
    {
        sourceFile.EnsureFileExists();

        var normalizedDest = StandardPathHelper.NormalizePath(destFile);
        var directory = Path.GetDirectoryName(normalizedDest);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(sourceFile, normalizedDest, true);
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

    public static void ClearFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new System.ArgumentNullException(nameof(filePath));

        File.WriteAllBytes(filePath, []);
    }

    public static void CreateFile(string filePath, string? content = null, byte[]? buffer = null, Encoding? encoding = null)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new System.ArgumentNullException(nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (content != null)
        {
            encoding ??= ExtensionMethodSetting.DefaultEncoding;
            File.WriteAllText(filePath, content, encoding);
        }
        else if (buffer != null)
        {
            using var fs = File.Create(filePath);
            fs.Write(buffer, 0, buffer.Length);
        }
        else
        {
            using (File.Create(filePath)) { }
        }
    }

    #endregion

    #region Search Operations

    public static bool Contains(string directoryPath, string searchPattern, bool isSearchChild = false)
    {
        if (string.IsNullOrEmpty(directoryPath) || string.IsNullOrEmpty(searchPattern))
            return false;

        try
        {
            var searchOption = isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.EnumerateFiles(directoryPath, searchPattern, searchOption).Any();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException
                                 || ex is DirectoryNotFoundException
                                 || ex is PathTooLongException)
        {
            return false;
        }
    }

    #endregion

    #region File Information

    public static CustomExistFileInfo? GetCustomFileInfo(string fullFileName)
    {
        if (string.IsNullOrEmpty(fullFileName))
            return null;

        var absolutePath = StandardPathHelper.ResolveToAbsolutePath(null, fullFileName);
        var file = new FileInfo(absolutePath);
        if (file.Exists)
        {
            using var memoryStream = file.ToMemoryStream();
            var strHashData = memoryStream.ComputeHashMd5();
            return new CustomExistFileInfo
            {
                HashData = strHashData,
                FileName = file.Name,
                RelativeFilePath = StandardPathHelper.GetRelativePath(absolutePath, Environment.CurrentDirectory),
                FullFilePath = file.FullName,
                FileSize = file.Length.FormatFileSize(),
                Length = file.Length
            };
        }
        return null;
    }

    #endregion

    /// <summary>
    /// 获取指定目录下的所有子目录
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="searchPattern">搜索模式，默认为"*"</param>
    /// <param name="searchOption">搜索选项，是否包含子目录</param>
    /// <param name="filter">自定义过滤器</param>
    /// <returns>目录路径数组</returns>
    public static string[] GetDirectories(
        string directoryPath,
        string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly,
        Func<DirectoryInfo, bool>? filter = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(directoryPath);

        var directories = Directory.GetDirectories(directoryPath, searchPattern, searchOption);

        if (filter != null)
        {
            return directories
                .Select(dir => new DirectoryInfo(dir))
                .Where(filter)
                .Select(dir => dir.FullName)
                .ToArray();
        }

        return directories;
    }

    /// <summary>
    /// 获取指定目录下的所有文件名
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="searchPattern">搜索模式 (默认�?"*.*")</param>
    /// <param name="containPath">是否包含完整路径</param>
    /// <param name="containExtension">是否包含扩展�?/param>
    /// <param name="searchOption">搜索选项，是否包含子目录</param>
    /// <returns>文件名列�?/returns>
    public static List<string> GetFileNames(
        string directoryPath,
        string searchPattern = "*.*",
        bool containPath = true,
        bool containExtension = true,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (string.IsNullOrEmpty(directoryPath))
            throw new System.ArgumentNullException(nameof(directoryPath));

        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var files = Directory.GetFiles(directoryPath, searchPattern, searchOption);
        var result = new List<string>();

        foreach (var file in files)
        {
            var fileName = file;
            if (!containPath)
                fileName = Path.GetFileName(file);
            if (!containExtension)
                fileName = Path.GetFileNameWithoutExtension(fileName);
            result.Add(fileName);
        }

        return result;
    }

    public static bool IsEmptyDirectory(string directory)
    {
        directory.EnsureDirectoryExists();
        return !Directory.EnumerateFileSystemEntries(directory).Any();
    }

    #region Directory Copy Operations

    public static void CopyDir(string srcDirectory, string destDirectory)
    {
        if (string.IsNullOrEmpty(srcDirectory))
            throw new System.ArgumentNullException(nameof(srcDirectory));
        if (string.IsNullOrEmpty(destDirectory))
            throw new System.ArgumentNullException(nameof(destDirectory));

        srcDirectory.EnsureDirectoryExists();

        // Ensure the destination path ends with a directory separator
        destDirectory = Path.GetFullPath(destDirectory.TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar);

        // Create destination directory if it doesn't exist
        Directory.CreateDirectory(destDirectory);

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
            throw new System.ArgumentNullException(nameof(directoryPath));

        if (!Directory.Exists(directoryPath))
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

    /// <summary>
    /// 确保目录存在
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    #endregion
}
