using System.Text;

namespace Linger.Helper;

public static partial class FileHelper
{
    #region File Read Operations

    /// <summary>
    /// Tries to read all text from the specified file without throwing on missing files.
    /// </summary>
    public static bool TryReadText(string filename, [NotNullWhen(true)] out string? content, Encoding? encoding = null)
    {
        content = null;
        if (string.IsNullOrWhiteSpace(filename) || !File.Exists(filename))
        {
            return false;
        }

        try
        {
            content = File.ReadAllText(filename, encoding ?? Encoding.UTF8);
            return true;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException || PathHelper.IsPathException(ex))
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
                                 || PathHelper.IsPathException(ex))
        {
            return false;
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
    /// Determines whether the specified directory is empty.
    /// </summary>
    /// <param name="directory">The directory path to check.</param>
    /// <returns><c>true</c> if the directory contains no files or subdirectories; otherwise, <c>false</c>.</returns>
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
    /// <exception cref="IOException">Thrown when the source directory contains a reparse point.</exception>
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

        srcDirectory = Path.GetFullPath(srcDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar);
        destDirectory = Path.GetFullPath(destDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar);

        ThrowIfReparsePoint(srcDirectory);

        var pathComparison = Path.DirectorySeparatorChar == '\\'
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (destDirectory.StartsWith(srcDirectory, pathComparison))
        {
            throw new ArgumentException("Destination directory cannot be the same as or a subdirectory of source directory.", nameof(destDirectory));
        }

        Directory.CreateDirectory(destDirectory);

        var fileList = Directory.GetFileSystemEntries(srcDirectory);

        foreach (var file in fileList)
        {
            ThrowIfReparsePoint(file);

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

    private static void ThrowIfReparsePoint(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
        {
            throw new IOException($"Copying reparse points is not supported: {path}");
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

    #endregion
}
