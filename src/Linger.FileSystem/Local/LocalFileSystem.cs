using System.Text;

namespace Linger.FileSystem.Local;

/// <summary>
/// 本地文件系统实现，提供对本地磁盘的文件操作支持。
/// </summary>
public partial class LocalFileSystem : FileSystemBase, ILocalFileSystem
{
    private readonly LocalFileSystemOptions _options;
    private readonly Encoding _defaultEncoding;
    private readonly string _rootDirectoryFullPath;
    private readonly string _rootDirectoryPrefix;
    private readonly StringComparison _pathComparison = Path.DirectorySeparatorChar == '\\'
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    /// <summary>
    /// 获取根目录路径。
    /// </summary>
    public string RootDirectoryPath { get; }

    /// <summary>
    /// 使用指定的选项初始化 <see cref="LocalFileSystem"/> 的新实例。
    /// </summary>
    /// <param name="options">本地文件系统选项。</param>
    /// <param name="logger">日志记录器（可选）。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="options"/> 为 <c>null</c> 时抛出。</exception>
    public LocalFileSystem(LocalFileSystemOptions options, ILogger<LocalFileSystem>? logger = null)
        : base(options?.RetryOptions, logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        RootDirectoryPath = options.RootDirectoryPath;
        _rootDirectoryFullPath = Path.GetFullPath(RootDirectoryPath);
        _rootDirectoryPrefix = _rootDirectoryFullPath.EndsWith(Path.DirectorySeparatorChar)
            ? _rootDirectoryFullPath
            : _rootDirectoryFullPath + Path.DirectorySeparatorChar;
        _defaultEncoding = _options.TextEncoding;

        // 确保根目录存在
        Directory.CreateDirectory(_rootDirectoryFullPath);
        Logger.LogDebug("LocalFileSystem initialized with root path: {RootPath}", _rootDirectoryFullPath);
    }

    /// <summary>
    /// 使用指定的根目录路径初始化 <see cref="LocalFileSystem"/> 的新实例。
    /// </summary>
    /// <param name="rootDirectoryPath">根目录路径。</param>
    /// <param name="retryOptions">重试选项（可选）。</param>
    /// <param name="logger">日志记录器（可选）。</param>
    public LocalFileSystem(string rootDirectoryPath, RetryOptions? retryOptions = null, ILogger<LocalFileSystem>? logger = null)
        : this(new LocalFileSystemOptions
        {
            RootDirectoryPath = rootDirectoryPath,
            RetryOptions = retryOptions
        }, logger)
    {
    }

    // 这是本地文件系统，所以IsRemoteFileSystem保持为false (默认)

    public bool FileExists(string filePath)
    {
        var realPath = GetRealPath(filePath);
        return PathExtensions.Exists(realPath, true);
    }

    public bool DirectoryExists(string directoryPath)
    {
        var realPath = GetRealPath(directoryPath);
        return PathExtensions.Exists(realPath, false);
    }

    public void CreateDirectoryIfNotExists(string directoryPath)
    {
        var realPath = GetRealPath(directoryPath);
        Directory.CreateDirectory(realPath);
    }

    public void DeleteFileIfExists(string filePath)
    {
        var realPath = GetRealPath(filePath);
        FileHelper.DeleteFileIfExists(realPath);
    }

    private void TryDeleteFile(string filePath)
    {
        try
        {
            FileHelper.DeleteFileIfExists(filePath);
        }
        catch (IOException ex)
        {
            Logger.LogWarning(ex, "Failed to clean up file: {FilePath}", filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning(ex, "Failed to clean up file: {FilePath}", filePath);
        }
    }

    private async Task CopyFileAtomicallyAsync(
        string sourcePath,
        string destinationPath,
        bool overwrite,
        int bufferSize,
        CancellationToken cancellationToken)
    {
        if (!overwrite && File.Exists(destinationPath))
        {
            throw new DuplicateFileException(destinationPath);
        }

        CreateParentDirectory(destinationPath);
        var directory = Path.GetDirectoryName(destinationPath)!;
        var tempPath = Path.Combine(directory, $".download-{Guid.NewGuid():N}.tmp");

        try
        {
            using (var sourceStream = new FileStream(
                       sourcePath,
                       FileMode.Open,
                       FileAccess.Read,
                       FileShare.Read,
                       bufferSize,
                       FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var destinationStream = new FileStream(
                       tempPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       bufferSize,
                       FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken).ConfigureAwait(false);
                await destinationStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            MoveUploadedFile(tempPath, destinationPath, overwrite);
        }
        finally
        {
            TryDeleteFile(tempPath);
        }
    }

    private static void MoveUploadedFile(string sourcePath, string destinationPath, bool overwrite)
    {
        CreateParentDirectory(destinationPath);
        CommitTemporaryFile(sourcePath, destinationPath, overwrite);
    }

    private static void CreateParentDirectory(string filePath)
    {
        string? directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// 获取文件路径（支持序号命名避免文件名冲突）
    /// </summary>
    /// <param name="destPath">目标目录路径</param>
    /// <param name="destFileName">目标文件名（可以包含相对路径）</param>
    /// <param name="overwrite">是否允许覆盖已存在的文件</param>
    /// <param name="useSequencedName">当文件已存在且不允许覆盖时，是否使用序号命名（如 file[1].txt）</param>
    /// <param name="destRootPath">目标根路径，用于检查文件是否存在。默认为空字符串</param>
    /// <returns>返回最终的文件相对路径</returns>
    /// <exception cref="DuplicateFileException">当文件已存在且不允许覆盖和序号命名时抛出</exception>
    /// <remarks>
    /// <para>此方法处理文件名冲突的策略：</para>
    /// <list type="number">
    /// <item><description>如果 <paramref name="overwrite"/> 为 true，直接返回原始路径，允许覆盖</description></item>
    /// <item><description>如果文件不存在，直接返回原始路径</description></item>
    /// <item><description>如果文件存在且 <paramref name="useSequencedName"/> 为 false，抛出 <see cref="DuplicateFileException"/></description></item>
    /// <item><description>如果文件存在且 <paramref name="useSequencedName"/> 为 true，生成序号文件名（document[1].pdf, document[2].pdf...）</description></item>
    /// </list>
    /// <para>序号命名格式：文件名[序号].扩展名，序号从1开始递增直到找到不存在的文件名</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 允许覆盖的情况
    /// var path1 = GetDestFilePath("uploads", "document.pdf", overwrite: true, useSequencedName: false);
    /// // 返回: "uploads/document.pdf"（无论是否存在都直接覆盖）
    ///
    /// // 使用序号命名的情况
    /// var path2 = GetDestFilePath("uploads", "document.pdf", overwrite: false, useSequencedName: true);
    /// // 如果document.pdf存在，返回: "uploads/document[1].pdf"
    /// // 如果document[1].pdf也存在，返回: "uploads/document[2].pdf"，以此类推
    ///
    /// // 严格模式（不允许重复）
    /// var path3 = GetDestFilePath("uploads", "document.pdf", overwrite: false, useSequencedName: false);
    /// // 如果document.pdf存在，抛出 DuplicateFileException
    /// </code>
    /// </example>
    private static string GetDestFilePath(string destPath, string destFileName, bool overwrite, bool useSequencedName, string destRootPath = "")
    {
        // 如果允许覆盖，直接返回目标路径
        if (overwrite)
        {
            return Path.Combine(destPath, destFileName);
        }

        // 分析文件名组成部分，用于后续的序号命名
        var fileInfo = new FileInfo(destFileName);
        var baseFileName = Path.GetFileNameWithoutExtension(destFileName);
        var extension = fileInfo.Extension;

        // 从序号0开始尝试（0表示原始文件名，无序号）
        uint sequence = 0;
        while (true)
        {
            // 根据序号生成当前尝试的文件名
            var currentFileName = sequence == 0
                ? destFileName  // 序号为0时使用原始文件名
                : $"{baseFileName}[{sequence}]{extension}";  // 其他情况使用序号格式

            // 构建完整路径以检查文件是否存在
            var fullPath = Path.Combine(destRootPath, destPath, currentFileName);

            // 如果文件不存在，找到了可用的文件名
            if (!File.Exists(fullPath))
            {
                return Path.Combine(destPath, currentFileName);
            }

            // 文件存在，检查是否允许使用序号命名
            if (!useSequencedName)
            {
                throw new DuplicateFileException();
            }

            // 递增序号，继续尝试下一个序号
            sequence++;
        }
    }

    /// <summary>
    /// 得到 带有 RootDirectoryPath 的路径
    /// </summary>
    /// <param name="filePath">相对或绝对文件路径</param>
    /// <returns>带有根目录的完整路径</returns>
    /// <exception cref="ArgumentException">当路径试图走出根目录时抛出</exception>
    public string GetRealPath(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var fullPath = Path.GetFullPath(Path.Combine(_rootDirectoryFullPath, filePath));
        var isRoot = string.Equals(fullPath, _rootDirectoryFullPath, _pathComparison);

        // 防止路径穿越攻击（如 "../../etc/passwd"）
        if (!isRoot && !fullPath.StartsWith(_rootDirectoryPrefix, _pathComparison))
        {
            throw new ArgumentException($"Path traversal detected. The path '{filePath}' is outside the root directory.", nameof(filePath));
        }

        ThrowIfPathContainsReparsePoint(fullPath);

        return fullPath;
    }

    private void ThrowIfPathContainsReparsePoint(string fullPath)
    {
        if (string.Equals(fullPath, _rootDirectoryFullPath, _pathComparison))
        {
            return;
        }

        // GetRealPath 已验证 fullPath 位于根目录之下，因此可以直接截取相对部分。
        var relativePath = fullPath.Substring(_rootDirectoryPrefix.Length);
        var currentPath = _rootDirectoryFullPath;
        foreach (var segment in relativePath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries))
        {
            currentPath = Path.Combine(currentPath, segment);
            if (!File.Exists(currentPath) && !Directory.Exists(currentPath))
            {
                break;
            }

            if ((File.GetAttributes(currentPath) & FileAttributes.ReparsePoint) != 0)
            {
                throw new IOException($"Access through reparse points is not supported: {currentPath}");
            }
        }
    }

    public Stream OpenRead(string filePath)
    {
        var realPath = GetRealPath(filePath);

        if (!File.Exists(realPath))
        {
            throw new FileNotFoundException("Source file not found", realPath);
        }

#if NET6_0_OR_GREATER
        var stream = new FileStream(realPath, new FileStreamOptions
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            BufferSize = _options.DownloadBufferSize
        });
#else
        var stream = new FileStream(realPath, FileMode.Open, FileAccess.Read, FileShare.Read, _options.DownloadBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
#endif

        return stream;
    }

    public Stream OpenWrite(string filePath, bool overwrite = false)
    {
        var realPath = GetRealPath(filePath);
        CreateParentDirectory(realPath);

        if (!overwrite && File.Exists(realPath))
        {
            throw new DuplicateFileException();
        }

#if NET6_0_OR_GREATER
        var stream = new FileStream(realPath, new FileStreamOptions
        {
            Access = FileAccess.Write,
            Mode = overwrite ? FileMode.Create : FileMode.CreateNew,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            BufferSize = _options.UploadBufferSize
        });
#else
        var stream = new FileStream(realPath, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, _options.UploadBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
#endif

        return stream;
    }

    public StreamReader GetReader(string filePath, Encoding? encoding = null)
    {
        var stream = OpenRead(filePath);
        return new StreamReader(stream, encoding ?? _defaultEncoding, true, _options.DownloadBufferSize, false);
    }

    public StreamWriter GetWriter(string filePath, bool overwrite = false, Encoding? encoding = null)
    {
        var stream = OpenWrite(filePath, overwrite);
        return new StreamWriter(stream, encoding ?? _defaultEncoding, _options.UploadBufferSize, false);
    }

    public long? GetFileSize(string filePath)
    {
        var realPath = GetRealPath(filePath);

        if (!File.Exists(realPath))
        {
            return null;
        }

        var fileInfo = new FileInfo(realPath);
        return fileInfo.Length;
    }

    public FileOperationResult Delete(string filePath)
    {
        var realPath = GetRealPath(filePath);
        if (File.Exists(realPath))
        {
            File.Delete(realPath);
        }

        return FileOperationResult.CreateSuccess(filePath);
    }

    private static bool IsRetryableLocalFileOperationException(Exception exception)
    {
        return exception is not (
            ArgumentException or
            UnauthorizedAccessException or
            FileNotFoundException or
            DirectoryNotFoundException or
            DuplicateFileException or
            NotSupportedException or
            ObjectDisposedException);
    }

    /// <summary>
    /// 列出目录中的文件名（相对提供的目录路径）
    /// </summary>
    public IReadOnlyList<string> ListFiles(
        string directoryPath)
    {
        var realDir = GetRealPath(directoryPath);
        if (!Directory.Exists(realDir))
        {
            return Array.Empty<string>();
        }

        var list = Directory.EnumerateFiles(realDir)
            .Select(f => Path.GetFileName(f)!)
            .ToList();
        return list;
    }

    /// <summary>
    /// 列出目录中的子目录名（相对提供的目录路径）
    /// </summary>
    public IReadOnlyList<string> ListDirectories(string directoryPath)
    {
        var realDir = GetRealPath(directoryPath);
        if (!Directory.Exists(realDir))
        {
            return Array.Empty<string>();
        }

        var list = Directory.EnumerateDirectories(realDir)
            .Select(d => Path.GetFileName(d)!)
            .ToList();
        return list;
    }
}
