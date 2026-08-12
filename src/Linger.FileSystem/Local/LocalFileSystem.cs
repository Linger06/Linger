using System.Text;

namespace Linger.FileSystem.Local;

/// <summary>
/// 本地文件系统实现，提供对本地磁盘的文件操作支持。
/// </summary>
public class LocalFileSystem : FileSystemBase, ILocalFileSystem
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

    public async Task<UploadedInfo> UploadWithNamingAsync(
        Stream inputStream,
        string sourceFileName,
        string containerName = "",
        string destPath = "",
        NamingRule? namingRule = null,
        bool? overwrite = null,
        bool? useSequencedName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        ArgumentException.ThrowIfNullOrEmpty(sourceFileName);

        Logger.LogDebug("Starting upload: {FileName} to container: {Container}, path: {Path}", sourceFileName, containerName, destPath);

        // 使用传入的值或默认值
        var effectiveNamingRule = namingRule ?? _options.DefaultNamingRule;
        var effectiveOverwrite = overwrite ?? _options.DefaultOverwrite;
        var effectiveUseSequencedName = useSequencedName ?? _options.DefaultUseSequencedName;

        var result = await ExecuteStreamOperationAsync(
            inputStream,
            async operationCancellationToken => await UploadInternalAsync(
                inputStream,
                sourceFileName,
                containerName,
                destPath,
                effectiveNamingRule,
                effectiveOverwrite,
                effectiveUseSequencedName,
                operationCancellationToken).ConfigureAwait(false),
            "文件上传",
            restoreLength: false,
            shouldRetry: IsRetryableLocalFileOperationException,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        Logger.LogInformation("Upload completed: {FileName} -> {NewFileName}, Size: {Size}", sourceFileName, result.NewFileName ?? string.Empty, result.FileSize);
        return result;
    }

    private async Task<UploadedInfo> UploadInternalAsync(
        Stream inputStream,
        string sourceFileName,
        string containerName,
        string destPath,
        NamingRule namingRule = NamingRule.Md5,
        bool overwrite = false,
        bool useSequencedName = true,
        CancellationToken cancellationToken = default)
    {
        var writeResult = namingRule switch
        {
            NamingRule.Md5 => await UploadWithMd5NameAsync(
                inputStream,
                sourceFileName,
                containerName,
                destPath,
                overwrite,
                cancellationToken).ConfigureAwait(false),
            NamingRule.Uuid or NamingRule.Normal => await UploadWithSelectedNameAsync(
                inputStream,
                sourceFileName,
                containerName,
                destPath,
                namingRule,
                overwrite,
                useSequencedName,
                cancellationToken).ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(namingRule), namingRule, "Unsupported naming rule.")
        };

        var fileInfo = new FileInfo(writeResult.FullFilePath);
        await ValidateFileAsync(fileInfo, writeResult.HashData, writeResult.Length, cancellationToken).ConfigureAwait(false);

        return new UploadedInfo
        {
            HashData = writeResult.HashData,
            FileName = sourceFileName,
            NewFileName = fileInfo.Name,
            FilePath = writeResult.FilePath,
            RelativeFilePath = Path.Combine(RootDirectoryPath, writeResult.FilePath),
            FullFilePath = fileInfo.FullName,
            FileSize = writeResult.Length.FormatFileSize(),
            Length = fileInfo.Length
        };
    }

    private async Task<UploadWriteResult> UploadWithMd5NameAsync(
        Stream inputStream,
        string sourceFileName,
        string containerName,
        string destPath,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        var requestedDirectory = Path.Combine(containerName, destPath);
        var destinationDirectory = string.IsNullOrEmpty(requestedDirectory)
            ? _rootDirectoryFullPath
            : GetRealPath(requestedDirectory);
        Directory.CreateDirectory(destinationDirectory);
        var tempPath = Path.Combine(destinationDirectory, $".upload-{Guid.NewGuid():N}.tmp");

        try
        {
            var copyResult = await CopyAndHashToFileAsync(
                inputStream,
                tempPath,
                FileMode.Create,
                cancellationToken).ConfigureAwait(false);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFileName).Replace(" ", string.Empty);
            var fileName = $"{fileNameWithoutExtension}-{copyResult.HashData}{Path.GetExtension(sourceFileName)}";
            var filePath = Path.Combine(containerName, destPath, fileName);
            var fullFilePath = GetRealPath(filePath);

            MoveUploadedFile(tempPath, fullFilePath, overwrite);

            return new UploadWriteResult(filePath, fullFilePath, copyResult.HashData, copyResult.Length);
        }
        finally
        {
            TryDeleteFile(tempPath);
        }
    }

    private async Task<UploadWriteResult> UploadWithSelectedNameAsync(
        Stream inputStream,
        string sourceFileName,
        string containerName,
        string destPath,
        NamingRule namingRule,
        bool overwrite,
        bool useSequencedName,
        CancellationToken cancellationToken)
    {
        var basePath = Path.Combine(containerName, destPath);
        var filePath = namingRule == NamingRule.Uuid
            ? GenerateUuidBasedPath(containerName, destPath, Path.GetExtension(sourceFileName))
            : GetDestFilePath(basePath, sourceFileName, overwrite, useSequencedName, _rootDirectoryFullPath);
        var fullFilePath = GetRealPath(filePath);
        CreateParentDirectory(fullFilePath);
        if (!overwrite && File.Exists(fullFilePath))
        {
            throw new DuplicateFileException();
        }

        var destinationDirectory = Path.GetDirectoryName(fullFilePath)!;
        var tempPath = Path.Combine(destinationDirectory, $".upload-{Guid.NewGuid():N}.tmp");

        try
        {
            var copyResult = await CopyAndHashToFileAsync(
                inputStream,
                tempPath,
                FileMode.CreateNew,
                cancellationToken).ConfigureAwait(false);
            MoveUploadedFile(tempPath, fullFilePath, overwrite);

            return new UploadWriteResult(filePath, fullFilePath, copyResult.HashData, copyResult.Length);
        }
        finally
        {
            TryDeleteFile(tempPath);
        }
    }

    private async Task<(string HashData, long Length)> CopyAndHashToFileAsync(
        Stream inputStream,
        string destinationFilePath,
        FileMode fileMode,
        CancellationToken cancellationToken)
    {
        using var hash = System.Security.Cryptography.IncrementalHash.CreateHash(System.Security.Cryptography.HashAlgorithmName.MD5);
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(_options.UploadBufferSize);
        long totalBytes = 0;
        FileStream? destinationStream = null;
        var completed = false;

        try
        {
            destinationStream = new FileStream(
                destinationFilePath,
                fileMode,
                FileAccess.Write,
                FileShare.None,
                _options.UploadBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

#if NET8_0_OR_GREATER
            await using (destinationStream.ConfigureAwait(false))
#else
            using (destinationStream)
#endif
            {
#if NET8_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                int bytesRead;
                while ((bytesRead = await inputStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await destinationStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
#else
                int bytesRead;
                while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
#endif
                    hash.AppendData(buffer, 0, bytesRead);
                    totalBytes += bytesRead;
                }
            }

            completed = true;

            return (hash.GetHashAndReset().ToMd5HashCode(), totalBytes);
        }
        finally
        {
            if (!completed && destinationStream is not null)
            {
                TryDeleteFile(destinationFilePath);
            }

            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async Task ValidateFileAsync(
        FileInfo fileInfo,
        string sourceHashData,
        long expectedSize,
        CancellationToken cancellationToken)
    {
        if (_options.ValidationLevel == FileValidationLevel.None)
        {
            return;
        }

        fileInfo.Refresh();
        if (fileInfo.Length != expectedSize)
        {
            CleanupInvalidFile(fileInfo.FullName);
            throw new InvalidOperationException($"File validation failed: Size mismatch for {fileInfo.FullName}. Expected: {expectedSize}, Actual: {fileInfo.Length}");
        }

        if (_options.ValidationLevel == FileValidationLevel.SizeOnly)
        {
            return;
        }

        var uploadedFileHash = await ComputeFileHashAsync(fileInfo.FullName, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(uploadedFileHash, sourceHashData, StringComparison.Ordinal))
        {
            CleanupInvalidFile(fileInfo.FullName);
            throw new InvalidOperationException($"File validation failed: MD5 hash mismatch for {fileInfo.FullName}");
        }
    }

    private async Task<string> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        using var hash = System.Security.Cryptography.IncrementalHash.CreateHash(System.Security.Cryptography.HashAlgorithmName.MD5);
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(_options.DownloadBufferSize);

        try
        {
            var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                _options.DownloadBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

#if NET8_0_OR_GREATER
            await using (stream.ConfigureAwait(false))
#else
            using (stream)
#endif
            {
                int bytesRead;
#if NET8_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
#else
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
#endif
                {
                    hash.AppendData(buffer, 0, bytesRead);
                }
            }

            return hash.GetHashAndReset().ToMd5HashCode();
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private void CleanupInvalidFile(string filePath)
    {
        if (_options.CleanupOnValidationFailure)
        {
            TryDeleteFile(filePath);
        }
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

    private static void MoveUploadedFile(string sourcePath, string destinationPath, bool overwrite)
    {
        CreateParentDirectory(destinationPath);
        if (!overwrite && File.Exists(destinationPath))
        {
            throw new DuplicateFileException();
        }

#if NET5_0_OR_GREATER
        try
        {
            File.Move(sourcePath, destinationPath, overwrite);
        }
        catch (IOException) when (!overwrite && File.Exists(destinationPath))
        {
            throw new DuplicateFileException();
        }
#else
        if (!overwrite)
        {
            try
            {
                File.Move(sourcePath, destinationPath);
            }
            catch (IOException) when (File.Exists(destinationPath))
            {
                throw new DuplicateFileException();
            }

            return;
        }

        if (File.Exists(destinationPath))
        {
            File.Replace(sourcePath, destinationPath, null);
            return;
        }

        try
        {
            File.Move(sourcePath, destinationPath);
        }
        catch (IOException) when (File.Exists(destinationPath))
        {
            File.Replace(sourcePath, destinationPath, null);
        }
#endif
    }

    private readonly struct UploadWriteResult
    {
        public UploadWriteResult(string filePath, string fullFilePath, string hashData, long length)
        {
            FilePath = filePath;
            FullFilePath = fullFilePath;
            HashData = hashData;
            Length = length;
        }

        public string FilePath { get; }
        public string FullFilePath { get; }
        public string HashData { get; }
        public long Length { get; }
    }

    /// <summary>
    /// 上传本地文件并应用指定的命名规则。
    /// </summary>
    /// <param name="sourceFilePathName">源文件路径。</param>
    /// <param name="containerName">容器名称（子目录）。</param>
    /// <param name="destPath">目标路径。</param>
    /// <param name="namingRule">命名规则。</param>
    /// <param name="overwrite">是否覆盖已存在的文件。</param>
    /// <param name="useSequencedName">文件冲突时是否使用序号命名。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>上传结果信息。</returns>
    /// <example>
    /// <code>
    /// var result = await fileSystem.UploadFileWithNamingAsync(
    ///     "avatar.png",
    ///     "users",
    ///     namingRule: NamingRule.Uuid);
    /// </code>
    /// </example>
    public async Task<UploadedInfo> UploadFileWithNamingAsync(
        string sourceFilePathName,
        string containerName,
        string destPath = "",
        NamingRule namingRule = NamingRule.Md5,
        bool overwrite = false,
        bool useSequencedName = true,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fileInfo = new FileInfo(sourceFilePathName);
        var fileStream = fileInfo.OpenRead();

#if NET8_0_OR_GREATER
        await using (fileStream.ConfigureAwait(false))
#else
        using (fileStream)
#endif
        {
            return await UploadWithNamingAsync(
                fileStream,
                fileInfo.Name,
                containerName,
                destPath,
                namingRule,
                overwrite,
                useSequencedName,
                cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 生成基于UUID的文件路径
    /// </summary>
    private string GenerateUuidBasedPath(string containerName, string destPath, string fileExtension)
    {
        while (true)
        {
            var fileName = $"{Guid.NewGuid():N}{fileExtension}";
            var relativePath = Path.Combine(containerName, destPath, fileName);
            var fullPath = GetRealPath(relativePath);

            if (!File.Exists(fullPath))
            {
                return relativePath;
            }
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
    private static void CreateParentDirectory(string filePath)
    {
        string? directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

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
    /// 异步下载文件到当前工作目录下的指定文件名
    /// </summary>
    /// <param name="sourceFilePath">源文件的相对路径（相对于 RootDirectoryPath）或绝对路径</param>
    /// <param name="localDestinationPath">目标文件名（仅文件名，不包含路径）。文件将保存到当前工作目录</param>
    /// <param name="overwrite">是否覆盖已存在的同名文件。默认为 false</param>
    /// <param name="useSequencedName">当目标文件已存在且不允许覆盖时，是否使用序号命名（如 file[1].txt）。默认为 true</param>
    /// <returns>返回实际下载后的文件完整路径</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="sourceFilePath"/> 或 <paramref name="localDestinationPath"/> 为 null 或空字符串时抛出</exception>
    /// <exception cref="FileNotFoundException">当源文件不存在时抛出</exception>
    /// <exception cref="DuplicateFileException">当目标文件已存在且 <paramref name="overwrite"/> 为 false 且 <paramref name="useSequencedName"/> 为 false 时抛出</exception>
    /// <remarks>
    /// <para>此方法会自动处理文件名冲突：</para>
    /// <list type="bullet">
    /// <item><description>如果 <paramref name="overwrite"/> 为 true，直接覆盖现有文件</description></item>
    /// <item><description>如果 <paramref name="overwrite"/> 为 false 且 <paramref name="useSequencedName"/> 为 true，会生成序号文件名（如 file[1].txt, file[2].txt）</description></item>
    /// <item><description>如果 <paramref name="overwrite"/> 为 false 且 <paramref name="useSequencedName"/> 为 false，会抛出 <see cref="DuplicateFileException"/> 异常</description></item>
    /// </list>
    /// <para>方法内部使用 <see cref="RetryHelper"/> 进行重试，确保操作的可靠性。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 下载文件到当前目录，如果存在则使用序号命名
    /// var actualPath = await fileSystem.DownloadAsync("uploads/document.pdf", "document.pdf");
    ///
    /// // 下载文件并直接覆盖同名文件
    /// var actualPath = await fileSystem.DownloadAsync("uploads/document.pdf", "document.pdf", overwrite: true);
    ///
    /// // 下载文件，如果存在同名文件则抛出异常
    /// var actualPath = await fileSystem.DownloadAsync("uploads/document.pdf", "document.pdf",
    ///     overwrite: false, useSequencedName: false);
    /// </code>
    /// </example>
    public async Task<string> DownloadAsync(string sourceFilePath, string localDestinationPath, bool overwrite = false, bool useSequencedName = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFilePath);
        ArgumentException.ThrowIfNullOrEmpty(localDestinationPath);

        Logger.LogDebug("Starting download: {Source} -> {Destination}", sourceFilePath, localDestinationPath);

        var realSourcePath = GetRealPath(sourceFilePath);
        if (!File.Exists(realSourcePath))
        {
            throw new FileNotFoundException("源文件不存在", realSourcePath);
        }

        var destFilePath = GetUniqueDestFilePath(
            localDestinationPath,
            overwrite,
            useSequencedName,
            cancellationToken);

        return await RetryHelper.ExecuteAsync(
            async operationCancellationToken =>
            {
                await CopyFileAtomicallyAsync(
                    realSourcePath,
                    destFilePath,
                    overwrite,
                    _options.DownloadBufferSize,
                    operationCancellationToken).ConfigureAwait(false);

                return destFilePath;
            },
            "文件下载",
            IsRetryableLocalFileOperationException,
            cancellationToken: cancellationToken).ConfigureAwait(false);

    }

    /// <summary>
    /// 下载文件到指定的流（内部实现）
    /// </summary>
    /// <param name="filePath">源文件路径</param>
    /// <param name="destStream">目标流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>完成任务</returns>
    /// <exception cref="ArgumentNullException">文件路径为空时抛出</exception>
    /// <exception cref="ArgumentNullException">目标流为空时抛出</exception>
    /// <exception cref="FileNotFoundException">源文件不存在时抛出</exception>
    private async Task DownloadToStreamInternalAsync(string filePath, Stream destStream, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(destStream);

        var sourceFilePath = GetRealPath(filePath);
        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("Source file not found", sourceFilePath);
        }

        var sourceStream = File.OpenRead(sourceFilePath);

#if NET8_0_OR_GREATER
        await using (sourceStream.ConfigureAwait(false))
#else
        using (sourceStream)
#endif
        {
            sourceStream.Position = 0;
            await sourceStream.CopyToAsync(destStream, _options.DownloadBufferSize, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 获取唯一的目标文件路径并确保其父目录存在。
    /// </summary>
    /// <param name="destFileName">目标文件名（可以包含相对路径）</param>
    /// <param name="overwrite">是否允许覆盖已存在的文件</param>
    /// <param name="useSequencedName">当文件已存在且不允许覆盖时，是否使用序号命名（如 file[1].txt）</param>
    /// <returns>返回唯一的目标文件路径，如果需要会自动生成序号</returns>
    /// <exception cref="DuplicateFileException">当文件已存在且不允许覆盖和序号命名时抛出</exception>
    /// <remarks>
    /// <para>此方法基于 <see cref="GetDestFilePath(string, string, bool, bool, string)"/> 处理命名冲突，并使用调用方提供的本地目标路径。</para>
    /// <list type="bullet">
    /// <item><description>自动创建目标目录（如果不存在）</description></item>
    /// <item><description>支持相对路径的文件名参数</description></item>
    /// </list>
    /// <para>处理逻辑：</para>
    /// <list type="number">
    /// <item><description>调用 <see cref="GetDestFilePath(string, string, bool, bool, string)"/> 获取唯一路径</description></item>
    /// <item><description>提取目标路径中的目录部分</description></item>
    /// <item><description>返回最终的文件路径</description></item>
    /// </list>
    /// <para>此方法主要用于下载操作，确保目标文件可以被成功创建。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 基本使用 - 仅文件名
    /// var path1 = GetUniqueDestFilePath("document.pdf", false, true);
    /// // 如果document.pdf存在，可能返回: "document[1].pdf"
    ///
    /// // 包含相对路径的文件名
    /// var path2 = GetUniqueDestFilePath("downloads\\document.pdf", false, true);
    /// // 会自动创建downloads目录，如果文件存在可能返回: "downloads\\document[1].pdf"
    ///
    /// // 允许覆盖的情况
    /// var path3 = GetUniqueDestFilePath("temp\\file.txt", true, false);
    /// // 返回: "temp\\file.txt"，会创建temp目录并允许覆盖
    ///
    /// // 严格模式
    /// try
    /// {
    ///     var path4 = GetUniqueDestFilePath("existing.txt", false, false);
    /// }
    /// catch (DuplicateFileException)
    /// {
    ///     // 如果existing.txt已存在，会抛出此异常
    /// }
    /// </code>
    /// </example>
    private static string GetUniqueDestFilePath(string destFileName, bool overwrite, bool useSequencedName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fullDestinationPath = Path.GetFullPath(destFileName);
        var directory = Path.GetDirectoryName(fullDestinationPath);
        if (string.IsNullOrEmpty(directory))
        {
            throw new ArgumentException("Destination path must include a directory.", nameof(destFileName));
        }

        Directory.CreateDirectory(directory);

        var destinationFilePath = GetDestFilePath(
            directory,
            Path.GetFileName(fullDestinationPath),
            overwrite,
            useSequencedName);

        return destinationFilePath;
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

    public override async Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        try
        {
            // 分离路径和文件名
            var destinationPath = Path.GetDirectoryName(destinationFilePath) ?? string.Empty;
            var fileName = Path.GetFileName(destinationFilePath);

            var uploadedInfo = await UploadWithNamingAsync(
                inputStream,
                fileName,
                string.Empty,
                destinationPath,
                NamingRule.Normal,
                overwrite,
                useSequencedName: false,
                cancellationToken).ConfigureAwait(false);

            return FileOperationResult.CreateSuccess(uploadedInfo.FilePath);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (DuplicateFileException ex)
        {
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DownloadToStreamAsync(string sourceFilePath, Stream outputStream, CancellationToken cancellationToken = default)
    {
        try
        {
            await DownloadToStreamInternalAsync(sourceFilePath, outputStream, cancellationToken).ConfigureAwait(false);
            return FileOperationResult.CreateSuccess();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (FileNotFoundException ex)
        {
            return FileOperationResult.CreateFailure($"下载文件到流失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DownloadFileAsync(string sourceFilePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        try
        {
            localDestinationPath = await DownloadAsync(sourceFilePath, localDestinationPath, overwrite, false, cancellationToken).ConfigureAwait(false);
            return FileOperationResult.CreateSuccess(sourceFilePath);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (FileNotFoundException ex)
        {
            return FileOperationResult.CreateFailure($"下载文件失败: {ex.Message}", ex);
        }
        catch (DuplicateFileException ex)
        {
            return FileOperationResult.CreateFailure($"下载文件失败: {ex.Message}", ex);
        }
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

    #region 批量操作

    /// <summary>
    /// 批量“上传”本地文件到目标目录（本地实现为拷贝到目标目录）
    /// </summary>
    /// <example>
    /// <code>
    /// var fs = new LocalFileSystem(new LocalFileSystemOptions { RootDirectoryPath = "/data" });
    /// var files = new[] { "C:/input/a.txt", "C:/input/b.txt" };
    /// var result = await fs.UploadFilesAsync(files, "uploads", overwrite: true);
    /// Console.WriteLine($"成功 {result.SuccessCount}, 失败 {result.FailureCount}");
    /// </code>
    /// </example>
    public async Task<BatchOperationResult> UploadFilesAsync(
        IEnumerable<string> localFilePaths,
        string destinationDirectory,
        bool overwrite = false,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var paths = localFilePaths?.ToList() ?? [];
        if (paths.Count == 0)
        {
            return BatchOperationResult.Empty;
        }

        var destDir = GetRealPath(destinationDirectory);
        Directory.CreateDirectory(destDir);
        var duplicateResult = CreateDuplicateTargetResult(
            paths,
            sourcePath => Path.Combine(destDir, Path.GetFileName(sourcePath)),
            Path.DirectorySeparatorChar == '\\' ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal,
            progress);
        if (duplicateResult is not null)
        {
            return duplicateResult;
        }

        return await ExecuteLocalBatchAsync(
            paths,
            async (sourcePath, operationCancellationToken) =>
            {
                if (!File.Exists(sourcePath))
                {
                    return new BatchOperationFailure(sourcePath, "本地文件不存在");
                }

                var fileName = Path.GetFileName(sourcePath);
                var destPath = Path.Combine(destDir, fileName);
                await CopyFileAtomicallyAsync(
                    sourcePath,
                    destPath,
                    overwrite,
                    _options.UploadBufferSize,
                    operationCancellationToken).ConfigureAwait(false);

                return null;
            },
            progress,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 批量“下载”文件到本地目录（本地实现为从根目录拷贝到指定目录）
    /// </summary>
    /// <example>
    /// <code>
    /// var fs = new LocalFileSystem(new LocalFileSystemOptions { RootDirectoryPath = "/data" });
    /// var result = await fs.DownloadFilesAsync(new[] { "docs/a.txt", "docs/b.txt" }, "C:/out", overwrite: true);
    /// foreach (var fail in result.FailedFiles) Console.WriteLine(fail.ErrorMessage);
    /// </code>
    /// </example>
    public async Task<BatchOperationResult> DownloadFilesAsync(
        IEnumerable<string> sourceFilePaths,
        string localDirectory,
        bool overwrite = false,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var paths = sourceFilePaths?.ToList() ?? [];
        if (paths.Count == 0)
        {
            return BatchOperationResult.Empty;
        }

        Directory.CreateDirectory(localDirectory);
        var duplicateResult = CreateDuplicateTargetResult(
            paths,
            remotePath => Path.Combine(localDirectory, Path.GetFileName(remotePath)),
            Path.DirectorySeparatorChar == '\\' ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal,
            progress);
        if (duplicateResult is not null)
        {
            return duplicateResult;
        }

        return await ExecuteLocalBatchAsync(
            paths,
            async (remotePath, operationCancellationToken) =>
            {
                var sourcePath = GetRealPath(remotePath);
                if (!File.Exists(sourcePath))
                {
                    return new BatchOperationFailure(remotePath, "源文件不存在");
                }

                var fileName = Path.GetFileName(sourcePath);
                var destPath = Path.Combine(localDirectory, fileName);
                await CopyFileAtomicallyAsync(
                    sourcePath,
                    destPath,
                    overwrite,
                    _options.DownloadBufferSize,
                    operationCancellationToken).ConfigureAwait(false);

                return null;
            },
            progress,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 批量删除文件（相对根目录路径）
    /// </summary>
    public BatchOperationResult DeleteFiles(
        IEnumerable<string> filePaths,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var paths = filePaths?.ToList() ?? [];
        if (paths.Count == 0)
        {
            return BatchOperationResult.Empty;
        }

        var tracker = new BatchOperationTracker(paths.Count, progress);
        foreach (var filePath in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var realPath = GetRealPath(filePath);
                if (File.Exists(realPath))
                {
                    File.Delete(realPath);
                }

                tracker.AddSuccess(filePath);
            }
            catch (IOException ex)
            {
                tracker.AddFailure(filePath, ex.Message, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                tracker.AddFailure(filePath, ex.Message, ex);
            }
            catch (ArgumentException ex)
            {
                tracker.AddFailure(filePath, ex.Message, ex);
            }
            catch (NotSupportedException ex)
            {
                tracker.AddFailure(filePath, ex.Message, ex);
            }
        }

        return tracker.Complete();
    }

    private static async Task<BatchOperationResult> ExecuteLocalBatchAsync(
        IReadOnlyList<string> filePaths,
        Func<string, CancellationToken, Task<BatchOperationFailure?>> operation,
        IProgress<BatchProgress>? progress,
        CancellationToken cancellationToken)
    {
        var tracker = new BatchOperationTracker(filePaths.Count, progress);

        async Task ExecuteAsync(string filePath)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var failure = await operation(filePath, cancellationToken).ConfigureAwait(false);
                if (failure is null)
                {
                    tracker.AddSuccess(filePath);
                }
                else
                {
                    tracker.AddFailure(failure);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracker.AddFailure(filePath, ex.Message, ex);
            }
        }

        foreach (var filePath in filePaths)
        {
            await ExecuteAsync(filePath).ConfigureAwait(false);
        }

        return tracker.Complete();
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

    #endregion
}
