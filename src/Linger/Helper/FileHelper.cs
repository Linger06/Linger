using System.Security.Cryptography;
using System.Text;
using Linger.Extensions.Core;
using Linger.Extensions.IO;

namespace Linger.Helper;

public static partial class FileHelper
{
    #region File Read Operations

    /// <summary>
    /// Reads all text content from the specified file.
    /// </summary>
    /// <param name="filename">The path to the file to read.</param>
    /// <param name="encoding">The character encoding to use. Defaults to UTF-8 if not specified.</param>
    /// <returns>A string containing all text from the file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    /// <example>
    /// <code>
    /// string content = FileHelper.ReadText("C:\\data\\config.txt");
    /// </code>
    /// </example>
    public static string ReadText(string filename, Encoding? encoding = null)
    {
        filename.EnsureFileExists();

        encoding ??= Encoding.UTF8;

        using var sr = new StreamReader(filename, encoding);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// Attempts to read all text content from the specified file without throwing exceptions.
    /// </summary>
    /// <param name="filename">The path to the file to read.</param>
    /// <param name="content">When this method returns, contains the file content if successful; otherwise, an empty string.</param>
    /// <param name="encoding">The character encoding to use. Defaults to UTF-8 if not specified.</param>
    /// <returns><c>true</c> if the file was read successfully; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// if (FileHelper.TryReadText("config.txt", out string content))
    /// {
    ///     Console.WriteLine(content);
    /// }
    /// </code>
    /// </example>
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

    /// <summary>
    /// Writes the specified text content to a file. Creates the directory structure if it doesn't exist.
    /// </summary>
    /// <param name="filePath">The path to the file to write.</param>
    /// <param name="text">The text content to write.</param>
    /// <param name="encoding">The character encoding to use. Defaults to UTF-8 if not specified.</param>
    /// <exception cref="ArgumentException">Thrown when filePath is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// FileHelper.WriteText("C:\\data\\output.txt", "Hello, World!");
    /// </code>
    /// </example>
    public static void WriteText(string filePath, string text, Encoding? encoding = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        encoding ??= Encoding.UTF8;

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(filePath, text, encoding);
    }

    /// <summary>
    /// Appends the specified text content to a file. Creates the directory structure if it doesn't exist.
    /// </summary>
    /// <param name="filePath">The path to the file to append to.</param>
    /// <param name="content">The text content to append.</param>
    /// <exception cref="ArgumentException">Thrown when filePath is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// FileHelper.AppendText("C:\\logs\\app.log", "New log entry\n");
    /// </code>
    /// </example>
    public static void AppendText(string filePath, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.AppendAllText(filePath, content);
    }

    /// <summary>
    /// Attempts to write the specified text content to a file without throwing exceptions.
    /// </summary>
    /// <param name="filePath">The path to the file to write.</param>
    /// <param name="text">The text content to write.</param>
    /// <param name="encoding">The character encoding to use. Defaults to UTF-8 if not specified.</param>
    /// <returns><c>true</c> if the file was written successfully; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// if (FileHelper.TryWriteText("output.txt", "Hello"))
    /// {
    ///     Console.WriteLine("File saved successfully.");
    /// }
    /// </code>
    /// </example>
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
    /// Attempts to append the specified text content to a file without throwing exceptions.
    /// </summary>
    /// <param name="filePath">The path to the file to append to.</param>
    /// <param name="content">The text content to append.</param>
    /// <returns><c>true</c> if the content was appended successfully; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// if (FileHelper.TryAppendText("log.txt", "New entry"))
    /// {
    ///     Console.WriteLine("Log entry added.");
    /// }
    /// </code>
    /// </example>
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

    /// <summary>
    /// Moves a file to the specified destination directory.
    /// </summary>
    /// <param name="sourceFilePath">The path of the file to move.</param>
    /// <param name="destDirectoryPath">The destination directory path.</param>
    /// <exception cref="FileNotFoundException">Thrown when the source file does not exist.</exception>
    /// <example>
    /// <code>
    /// FileHelper.MoveFile("C:\\temp\\file.txt", "C:\\archive\\");
    /// </code>
    /// </example>
    public static void MoveFile(string sourceFilePath, string destDirectoryPath)
    {
        sourceFilePath.EnsureFileExists();

        var sourceFileName = Path.GetFileName(sourceFilePath);
        Directory.CreateDirectory(destDirectoryPath);
        var destFileName = Path.Combine(destDirectoryPath, sourceFileName);
        File.Move(sourceFilePath, destFileName);
    }

    /// <summary>
    /// Copies a file to the specified destination. Creates the directory structure if it doesn't exist.
    /// </summary>
    /// <param name="sourceFile">The path of the source file to copy.</param>
    /// <param name="destFile">The destination file path.</param>
    /// <exception cref="FileNotFoundException">Thrown when the source file does not exist.</exception>
    /// <example>
    /// <code>
    /// FileHelper.CopyFile("C:\\source\\file.txt", "C:\\backup\\file.txt");
    /// </code>
    /// </example>
    public static void CopyFile(string sourceFile, string destFile)
    {
        sourceFile.EnsureFileExists();

        var normalizedDest = PathHelper.CleanAndNormalizePureString(destFile, false);
        var directory = Path.GetDirectoryName(normalizedDest);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(sourceFile, normalizedDest, true);
    }

    /// <summary>
    /// Deletes the specified file if it exists. Does nothing if the file doesn't exist.
    /// </summary>
    /// <param name="file">The path of the file to delete.</param>
    /// <example>
    /// <code>
    /// FileHelper.DeleteFileIfExists("C:\\temp\\obsolete.txt");
    /// </code>
    /// </example>
    public static void DeleteFileIfExists(string file)
    {
        if (string.IsNullOrEmpty(file))
            return;

        if (File.Exists(file))
        {
            File.Delete(file);
        }
    }

    /// <summary>
    /// Clears all content from the specified file, leaving an empty file.
    /// </summary>
    /// <param name="filePath">The path of the file to clear.</param>
    /// <exception cref="ArgumentException">Thrown when filePath is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// FileHelper.ClearFile("C:\\logs\\app.log");
    /// </code>
    /// </example>
    public static void ClearFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        File.WriteAllBytes(filePath, []);
    }

    /// <summary>
    /// Creates a new file with optional content. Creates the directory structure if it doesn't exist.
    /// </summary>
    /// <param name="filePath">The path of the file to create.</param>
    /// <param name="content">The text content to write to the file. If null, buffer or empty file is created.</param>
    /// <param name="buffer">The byte array to write to the file. Used if content is null.</param>
    /// <param name="encoding">The character encoding to use for text content. Defaults to UTF-8.</param>
    /// <exception cref="ArgumentException">Thrown when filePath is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// // Create with text content
    /// FileHelper.CreateFile("output.txt", content: "Hello, World!");
    ///
    /// // Create with binary content
    /// FileHelper.CreateFile("data.bin", buffer: new byte[] { 0x01, 0x02, 0x03 });
    ///
    /// // Create empty file
    /// FileHelper.CreateFile("empty.txt");
    /// </code>
    /// </example>
    public static void CreateFile(string filePath, string? content = null, byte[]? buffer = null, Encoding? encoding = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (content != null)
        {
            encoding ??= Encoding.UTF8;
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

    /// <summary>
    /// Determines whether the specified directory contains files matching the search pattern.
    /// </summary>
    /// <param name="directoryPath">The absolute path of the directory to search.</param>
    /// <param name="searchPattern">The search pattern. Use "*" for zero or more characters, "?" for a single character. Example: "Log*.xml" matches all XML files starting with "Log".</param>
    /// <param name="isSearchChild">If <c>true</c>, searches subdirectories; otherwise, only searches the top directory.</param>
    /// <returns><c>true</c> if matching files are found; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// bool hasLogs = FileHelper.Contains("C:\\logs", "*.log", isSearchChild: true);
    /// </code>
    /// </example>
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

    /// <summary>
    /// 获取指定文件的扩展信息，包括哈希、路径和文件大小元数据（已完美修复内存爆作与路径颠倒隐患）。
    /// </summary>
    /// <param name="fullFileName">目标文件的全路径或相对路径。</param>
    /// <param name="relativeTo">用于计算相对路径的基准目录。若为 null 则缺省取当前工作目录。</param>
    /// <returns>一个包含文件元数据的 <see cref="ExtendedFileInfo"/> 实例；若文件不存在或路径非法则返回 <see langword="null"/>。</returns>
    public static ExtendedFileInfo? GetExistingFileInfo(string fullFileName, string? relativeTo = null)
    {
        // 1. 快速防御性拦截
        if (string.IsNullOrEmpty(fullFileName))
            return null;

        // 2. 规范化基准起点，统一使用 StandardPathHelper 推荐的异常防御机制
        var basePath = string.IsNullOrEmpty(relativeTo)
            ? Environment.CurrentDirectory
            : relativeTo;

        if (basePath is null)
            return null;

        try
        {
            // 3. 全面拥抱全新编写的链式扩展方法，获取绝对路径并进行非法字符/保留字强物理校验
            string absolutePath = fullFileName.ToFullPath();

            // 4. 利用已经封装好的强原子性 Exists 校验（内部包含 \0 及非法字拦截）
            if (!PathExtensions.Exists(absolutePath, checkAsFile: true))
                return null;

            var file = new FileInfo(absolutePath);
            string strHashData;

            // 5. 【流式哈希终极性能优化】：彻底弃用高危的 MemoryStream，采用 0 堆内存积压的 FileStream
            using (var fileStream = file.OpenRead())
            {
              strHashData =  fileStream.ComputeHashMd5();
// #if NET
//                 // 现代 .NET 提供的高性能、0分配流式哈希 API
//                 strHashData = Convert.ToHexString(MD5.HashData(fileStream));
// #else
//                 // .NET Framework 4.7 回退流式哈希计算，同样完美锁死内存开销
//                 using (var md5 = MD5.Create())
//                 {
//                     var hashBytes = md5.ComputeHash(fileStream);

//                     // 兼容旧版的高性能字节转十六进制字符串（避免产生海量临时 string）
//                     var sb = new System.Text.StringBuilder(hashBytes.Length * 2);
//                     foreach (var b in hashBytes)
//                     {
//                         sb.Append(b.ToString("X2"));
//                     }
//                     strHashData = sb.ToString();
//                 }
// #endif
            }

            // 6. 返回组装结果
            return new ExtendedFileInfo
            {
                HashData = strHashData,
                FileName = file.Name,
                RelativeFilePath = basePath.GetRelativePath(absolutePath),
                FullFilePath = file.FullName,
                FileSize = file.Length.FormatFileSize(), // 保持你人性化的格式化
                Length = file.Length
            };
        }
        catch (Exception ex) when (PathHelper.IsPathException(ex))
        {
            // 拦截由于非法字符或超长路径引发的系统文件系统异常
            return null;
        }
    }

    #endregion

    /// <summary>
    /// Gets all subdirectories in the specified directory.
    /// </summary>
    /// <param name="directoryPath">The directory path to search.</param>
    /// <param name="searchPattern">The search pattern. Defaults to "*" (all directories).</param>
    /// <param name="searchOption">Specifies whether to search subdirectories.</param>
    /// <param name="filter">An optional filter function to apply to the results.</param>
    /// <returns>An array of directory paths.</returns>
    /// <exception cref="ArgumentException">Thrown when directoryPath is null or empty.</exception>
    /// <example>
    /// <code>
    /// string[] dirs = FileHelper.GetDirectories("C:\\Projects", "*.Net*");
    /// </code>
    /// </example>
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
    /// Gets all file names in the specified directory.
    /// </summary>
    /// <param name="directoryPath">The directory path to search.</param>
    /// <param name="searchPattern">The search pattern. Defaults to "*.*" (all files).</param>
    /// <param name="containPath">If <c>true</c>, returns full file paths; otherwise, returns only file names.</param>
    /// <param name="containExtension">If <c>true</c>, includes file extensions; otherwise, excludes them.</param>
    /// <param name="searchOption">Specifies whether to search subdirectories.</param>
    /// <returns>A list of file names or paths.</returns>
    /// <exception cref="ArgumentException">Thrown when directoryPath is null or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
    /// <example>
    /// <code>
    /// List&lt;string&gt; files = FileHelper.GetFileNames("C:\\docs", "*.pdf", containPath: false);
    /// </code>
    /// </example>
    public static List<string> GetFileNames(
        string directoryPath,
        string searchPattern = "*.*",
        bool containPath = true,
        bool containExtension = true,
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

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

    /// <summary>
    /// Determines whether the specified directory is empty (contains no files or subdirectories).
    /// </summary>
    /// <param name="directory">The directory path to check.</param>
    /// <returns><c>true</c> if the directory is empty; otherwise, <c>false</c>.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
    /// <example>
    /// <code>
    /// if (FileHelper.IsEmptyDirectory("C:\\temp"))
    /// {
    ///     Console.WriteLine("Directory is empty.");
    /// }
    /// </code>
    /// </example>
    public static bool IsEmptyDirectory(string directory)
    {
        directory.EnsureDirectoryExists();
        return !Directory.EnumerateFileSystemEntries(directory).Any();
    }

    #region Directory Copy Operations

    /// <summary>
    /// Recursively copies a directory and all its contents to the destination.
    /// </summary>
    /// <param name="srcDirectory">The source directory path.</param>
    /// <param name="destDirectory">The destination directory path.</param>
    /// <exception cref="ArgumentException">Thrown when srcDirectory or destDirectory is null or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the source directory does not exist.</exception>
    /// <example>
    /// <code>
    /// FileHelper.CopyDir("C:\\source", "C:\\backup");
    /// </code>
    /// </example>
    public static void CopyDir(string srcDirectory, string destDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(srcDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(destDirectory);

        srcDirectory.EnsureDirectoryExists();

        // Normalize source and destination paths with a trailing separator.
        srcDirectory = Path.GetFullPath(srcDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar);
        destDirectory = Path.GetFullPath(destDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar);

        // Prevent recursive self-copy (destination equals source or is under source).
        var pathComparison = Path.DirectorySeparatorChar == '\\'
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (destDirectory.StartsWith(srcDirectory, pathComparison))
        {
            throw new ArgumentException("Destination directory cannot be the same as or a subdirectory of source directory.", nameof(destDirectory));
        }

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
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

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
