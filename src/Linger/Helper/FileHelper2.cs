namespace Linger.Helper;

public static partial class FileHelper
{
    /// <summary>
    /// Gets the number of lines in a text file.
    /// </summary>
    /// <param name="filePath">Absolute file path.</param>
    [Obsolete("Use File.ReadLines(filePath).Count() or a dedicated streaming reader instead. This API will be removed in 2.0.0.")]
    public static int GetLineCount(string filePath)
    {
        var rows = File.ReadAllLines(filePath);
        return rows.Length;
    }

    /// <summary>
    /// Gets the size of a file in bytes.
    /// </summary>
    /// <param name="filePath">Absolute file path.</param>
    [Obsolete("Use filePath.GetFileSize() instead. This API will be removed in 2.0.0.")]
    public static long GetFileSize(string filePath)
    {
        var fi = new FileInfo(filePath);
        return fi.Length;
    }

    /// <summary>
    /// Gets all direct child directories under a directory.
    /// </summary>
    /// <param name="directoryPath">Absolute directory path.</param>
    [Obsolete("Use FileHelper.GetDirectories(directoryPath, searchOption: SearchOption.TopDirectoryOnly) instead. This API will be removed in 2.0.0.")]
    public static string[] GetDirectories(string directoryPath)
    {
        return Directory.GetDirectories(directoryPath);
    }

    /// <summary>
    /// Gets directories that match a search pattern.
    /// </summary>
    /// <param name="directoryPath">Absolute directory path.</param>
    /// <param name="searchPattern">Search pattern such as <c>*.xml</c>.</param>
    /// <param name="isSearchChild">Whether to include nested directories.</param>
    [Obsolete("Use FileHelper.GetDirectories(directoryPath, searchPattern, isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly) instead. This API will be removed in 2.0.0.")]
    public static string[] GetDirectories(string directoryPath, string searchPattern, bool isSearchChild)
    {
        return GetDirectories(
            directoryPath,
            searchPattern,
            searchOption: isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
    }

    /// <summary>
    /// Gets file names that match a search pattern.
    /// </summary>
    /// <param name="directoryPath">Absolute directory path.</param>
    /// <param name="searchPattern">Search pattern such as <c>*.xml</c>.</param>
    /// <param name="isSearchChild">Whether to include nested directories.</param>
    [Obsolete("Use FileHelper.GetFileNames(directoryPath, searchPattern, searchOption: isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly) instead. This API will be removed in 2.0.0.")]
    public static string[] GetFileNames(string directoryPath, string searchPattern, bool isSearchChild)
    {
        return GetFileNames(
            directoryPath,
            searchPattern,
            searchOption: isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToArray();
    }

    /// <summary>
    /// Creates the directory if it does not already exist.
    /// </summary>
    /// <param name="directoryPath">Directory path to create.</param>
    [Obsolete("Use Directory.CreateDirectory(directoryPath) instead. This API will be removed in 2.0.0.")]
    public static void CreateDirectoryIfNotExists(string directoryPath)
    {
        _ = Directory.CreateDirectory(directoryPath);
    }

    /// <summary>
    /// Deletes files from the target directory when the same file name exists in the source directory.
    /// </summary>
    /// <param name="varFromDirectory">Source directory used to enumerate file names.</param>
    /// <param name="varToDirectory">Target directory from which matching files are deleted.</param>
    [Obsolete("No direct replacement is planned. Prefer explicit Directory and File operations in the caller. This API will be removed in 2.0.0.")]
    public static void DeleteFolderFiles(string varFromDirectory, string varToDirectory)
    {
        _ = Directory.CreateDirectory(varToDirectory);

        if (!Directory.Exists(varFromDirectory))
        {
            return;
        }

        var directories = Directory.GetDirectories(varFromDirectory);

        if (directories.Length > 0)
        {
            foreach (var d in directories)
            {
                var destDir = Path.Combine(varToDirectory, Path.GetFileName(d));
                DeleteFolderFiles(d, destDir);
            }
        }

        var files = Directory.GetFiles(varFromDirectory);

        if (files.Length > 0)
        {
            foreach (var s in files)
            {
                var destFile = Path.Combine(varToDirectory, Path.GetFileName(s));
                if (File.Exists(destFile))
                {
                    File.Delete(destFile);
                }
            }
        }
    }
}
