using Linger.Helper.PathHelpers;

namespace Linger.FileSystem.Local;

// 添加继承自FileSystemBase
public class LocalFileSystem : FileSystemBase, ILocalFileSystem
{
    private readonly LocalFileSystemOptions _options;

    public string RootDirectoryPath { get; }

    public LocalFileSystem(LocalFileSystemOptions options)
        : base(options.RetryOptions)
    {
        _options = options ?? throw new System.ArgumentNullException(nameof(options));
        RootDirectoryPath = options.RootDirectoryPath;

        // 确保根目录存在
        Directory.CreateDirectory(RootDirectoryPath);
    }

    public LocalFileSystem(string rootDirectoryPath, RetryOptions? retryOptions = null)
        : this(new LocalFileSystemOptions
        {
            RootDirectoryPath = rootDirectoryPath,
            RetryOptions = retryOptions
        })
    {
    }

    // 这是本地文件系统，所以IsRemoteFileSystem保持为false (默认)

    public bool Exists()
    {
        return DirectoryExists(RootDirectoryPath);
    }

    public void CreateIfNotExists()
    {
        CreateDirectoryIfNotExists(RootDirectoryPath);
    }

    public override void CreateDirectoryIfNotExists(string directoryPath)
    {
        var realPath = GetRealPath(directoryPath);
        Directory.CreateDirectory(realPath);
    }

    public override void DeleteFileIfExists(string filePath)
    {
        var realPath = GetRealPath(filePath);
        FileHelper.DeleteFileIfExists(realPath);
    }

    public override bool DirectoryExists(string directoryPath)
    {
        var realPath = GetRealPath(directoryPath);
        return StandardPathHelper.Exists(realPath, false);
    }

    public override bool FileExists(string filePath)
    {
        var realPath = GetRealPath(filePath);
        return StandardPathHelper.Exists(realPath, true);
    }

    public override Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(FileExists(filePath));
    }

    public override Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DirectoryExists(directoryPath));
    }

    public override Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        CreateDirectoryIfNotExists(directoryPath);
        return Task.CompletedTask;
    }

    public override Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        DeleteFileIfExists(filePath);
        return Task.CompletedTask;
    }

    public async Task<UploadedInfo> UploadAsync(
        Stream inputStream,
        string sourceFileName,
        string containerName = "",
        string destPath = "",
        NamingRule? namingRule = null,
        bool? overwrite = null,
        bool? useSequencedName = null)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        ArgumentException.ThrowIfNullOrEmpty(sourceFileName);

        // 使用传入的值或默认值
        var effectiveNamingRule = namingRule ?? _options.DefaultNamingRule;
        var effectiveOverwrite = overwrite ?? _options.DefaultOverwrite;
        var effectiveUseSequencedName = useSequencedName ?? _options.DefaultUseSequencedName;

        return await RetryHelper.ExecuteAsync(
            async () => await UploadInternalAsync(
                inputStream,
                sourceFileName,
                containerName,
                destPath,
                effectiveNamingRule,
                effectiveOverwrite,
                effectiveUseSequencedName).ConfigureAwait(false),
            "文件上传",
            ex => ex is not DuplicateFileException).ConfigureAwait(false); // 文件重复异常不重试

    }

    private async Task<UploadedInfo> UploadInternalAsync(
        Stream inputStream,
        string sourceFileName,
        string containerName,
        string destPath,
        NamingRule namingRule = NamingRule.Md5, bool overwrite = false, bool useSequencedName = true)
    {
        // 先将输入流的内容复制到内存流中，这样可以多次读取
        using var memoryStream = new MemoryStream();
        await inputStream.CopyToAsync(memoryStream, _options.UploadBufferSize).ConfigureAwait(false);
        memoryStream.Position = 0;

        // 计算源文件哈希值
        var sourceHashData = memoryStream.ComputeHashMd5();

        // 获取目标文件路径
        var filePath = GetDestFilePath(
            memoryStream,
            sourceFileName,
            containerName,
            destPath,
            namingRule,
                overwrite,
                useSequencedName);

        var relativeFilePath = Path.Combine(RootDirectoryPath, filePath);

        // 确保目录存在
        FileHelper.EnsureDirectoryExists(relativeFilePath);

        // 写入文件
        memoryStream.Position = 0;

        var fileStream = File.Create(relativeFilePath);

#if NET8_0_OR_GREATER
        await using (fileStream.ConfigureAwait(false))
#else
        using (fileStream)
#endif
        {
            await memoryStream.CopyToAsync(fileStream).ConfigureAwait(false);
        }

        // 验证文件完整性
        var fileInfo = new FileInfo(relativeFilePath);

#if NET8_0_OR_GREATER
        await ValidateFileIntegrityAsync(fileInfo, sourceHashData).ConfigureAwait(false);
#else
        ValidateFileIntegrity(fileInfo, sourceHashData);
#endif

        // 构建上传信息
        return new UploadedInfo
        {
            HashData = sourceHashData,
            FileName = sourceFileName,
            NewFileName = fileInfo.Name,
            FilePath = filePath,
            RelativeFilePath = relativeFilePath,
            FullFilePath = fileInfo.FullName,
            FileSize = memoryStream.Length.FormatFileSize(),
            Length = fileInfo.Length
        };
    }

    private
#if NET8_0_OR_GREATER
       async Task ValidateFileIntegrityAsync
#else
        void ValidateFileIntegrity
#endif
     // 验证文件完整性时使用配置
     (FileInfo fileInfo, string sourceHashData)
    {
        if (!_options.ValidateFileIntegrity)
            return;

        // 验证文件MD5

        var stream = fileInfo.OpenRead();

#if NET8_0_OR_GREATER
        await using (stream.ConfigureAwait(false))
#else
        using (stream)
#endif
        {
            var uploadedFileHash = stream.ComputeHashMd5();
            if (uploadedFileHash != sourceHashData)
            {
                var ex = new InvalidOperationException($"File integrity check failed: MD5 hash mismatch for {fileInfo.FullName}");

                // 根据配置决定是否清理
                if (_options.CleanupOnValidationFailure)
                {
                    try { File.Delete(fileInfo.FullName); } catch { /* 忽略清理失败 */ }
                }

                throw ex;
            }
        }

        // 验证文件元数据
        if (_options.ValidateFileMetadata)
        {
            var customFileInfo = FileHelper.GetCustomFileInfo(fileInfo.FullName);
            if (customFileInfo?.HashData != null && customFileInfo.HashData != sourceHashData)
            {
                var ex = new InvalidOperationException($"File integrity check failed: Metadata hash mismatch for {fileInfo.FullName}");

                if (_options.CleanupOnValidationFailure)
                {
                    try { File.Delete(fileInfo.FullName); } catch { /* 忽略清理失败 */ }
                }

                throw ex;
            }
        }
    }

    public Task<UploadedInfo> UploadAsync(string sourceFilePathName, string containerName, string destPath = "", NamingRule namingRule = NamingRule.Md5, bool overwrite = false, bool useSequencedName = true)
    {
        var fileInfo = new FileInfo(sourceFilePathName);
        // 先将文件内容读入内存流中
        using var memoryStream = new MemoryStream();
        using FileStream fileStream = fileInfo.OpenRead();
        fileStream.CopyTo(memoryStream);
        // 重置内存流位置
        memoryStream.Position = 0;
        // 使用内存流进行上传
        return UploadAsync(memoryStream, fileInfo.Name, containerName, destPath, namingRule, overwrite, useSequencedName);
    }

    /// <summary>
    /// 获取目标文件路径
    /// </summary>
    /// <param name="inputStream">输入流</param>
    /// <param name="sourceFileName">源文件名</param>
    /// <param name="containerName">容器名</param>
    /// <param name="destPath">目标路径</param>
    /// <param name="namingRule">文件命名规则（决定如何为上传的文件命名）</param>
    /// <param name="overwrite">是否覆盖已存在的文件</param>
    /// <param name="useSequencedName">是否使用序号命名（当文件名冲突时）</param>
    /// <returns>目标文件相对路径(只返回除destRootPath以外的存储位置)</returns>
    private string GetDestFilePath(
        Stream inputStream,
        string sourceFileName,
        string containerName,
        string destPath,
        NamingRule namingRule = NamingRule.Md5, bool overwrite = false, bool useSequencedName = true)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
        Linger.ArgumentException.ThrowIfNullOrEmpty(sourceFileName);

        var fileExtension = Path.GetExtension(sourceFileName);

        switch (namingRule)
        {
            case NamingRule.Uuid:
                return GenerateUuidBasedPath(containerName, destPath, fileExtension);
            case NamingRule.Md5:
                return GenerateHashBasedPath(inputStream, sourceFileName, containerName, destPath);
            case NamingRule.Normal:
            default:
                {
                    var basePath = Path.Combine(containerName, destPath);
                    return GetDestFilePath(basePath, sourceFileName, overwrite, useSequencedName, RootDirectoryPath);
                }
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
            var fullPath = Path.Combine(RootDirectoryPath, containerName, destPath, fileName);

            if (!File.Exists(fullPath))
            {
                return Path.Combine(containerName, destPath, fileName);
            }
        }
    }

    /// <summary>
    /// 生成基于文件哈希值的文件路径
    /// </summary>
    private static string GenerateHashBasedPath(Stream inputStream, string sourceFileName, string containerName, string destPath)
    {
        var hashData = inputStream.ComputeHashMd5();
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFileName).Replace(" ", string.Empty);
        var fileExtension = Path.GetExtension(sourceFileName);

        // 使用更标准的分隔符
        var fileName = $"{fileNameWithoutExtension}-{hashData}{fileExtension}";
        return Path.Combine(containerName, destPath, fileName);
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
    public async Task<string> DownloadAsync(string sourceFilePath, string localDestinationPath, bool overwrite = false, bool useSequencedName = true)
    {
        Linger.ArgumentException.ThrowIfNullOrEmpty(sourceFilePath);
        Linger.ArgumentException.ThrowIfNullOrEmpty(localDestinationPath);

        return await RetryHelper.ExecuteAsync(
            async () =>
            {
                var realSourcePath = GetRealPath(sourceFilePath);
                if (!File.Exists(realSourcePath))
                {
                    throw new FileNotFoundException("源文件不存在", realSourcePath);
                }

                var destFilePath = await GetUniqueDestFilePathAsync(
                    localDestinationPath,
                    overwrite,
                    useSequencedName).ConfigureAwait(false);

                var sourceStream = File.OpenRead(realSourcePath);
                var destStream = File.Create(destFilePath);

#if NET8_0_OR_GREATER
                await using (sourceStream.ConfigureAwait(false))
                await using (destStream.ConfigureAwait(false))
#else
                using (sourceStream)
                using (destStream)
#endif
                {
                    await sourceStream.CopyToAsync(destStream, _options.DownloadBufferSize).ConfigureAwait(false);
                }
                return destFilePath;
            },
            "文件下载",
            ex => !(ex is FileNotFoundException || ex is DuplicateFileException)).ConfigureAwait(false);

    }

    /// <summary>
    /// 下载文件到指定的流
    /// </summary>
    /// <param name="filePath">源文件路径</param>
    /// <param name="destStream">目标流</param>
    /// <returns>完成任务</returns>
    /// <exception cref="ArgumentNullException">文件路径为空时抛出</exception>
    /// <exception cref="ArgumentNullException">目标流为空时抛出</exception>
    /// <exception cref="FileNotFoundException">源文件不存在时抛出</exception>
    public async Task DownloadToStreamAsync(string filePath, Stream destStream)
    {
        Linger.ArgumentException.ThrowIfNullOrEmpty(filePath);
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
            await sourceStream.CopyToAsync(destStream, _options.DownloadBufferSize).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 异步获取唯一的目标文件路径（确保目录存在）
    /// </summary>
    /// <param name="destFileName">目标文件名（可以包含相对路径）</param>
    /// <param name="overwrite">是否允许覆盖已存在的文件</param>
    /// <param name="useSequencedName">当文件已存在且不允许覆盖时，是否使用序号命名（如 file[1].txt）</param>
    /// <returns>返回唯一的目标文件路径，如果需要会自动生成序号</returns>
    /// <exception cref="DuplicateFileException">当文件已存在且不允许覆盖和序号命名时抛出</exception>
    /// <remarks>
    /// <para>此方法是 <see cref="GetDestFilePath(string, string, bool, bool, string)"/> 的异步包装版本，提供以下额外功能：</para>
    /// <list type="bullet">
    /// <item><description>自动创建目标目录（如果不存在）</description></item>
    /// <item><description>支持相对路径的文件名参数</description></item>
    /// <item><description>异步操作，避免阻塞UI线程</description></item>
    /// </list>
    /// <para>处理逻辑：</para>
    /// <list type="number">
    /// <item><description>调用 <see cref="GetDestFilePath(string, string, bool, bool, string)"/> 获取唯一路径</description></item>
    /// <item><description>提取目标路径中的目录部分</description></item>
    /// <item><description>如果目录不存在，异步创建目录</description></item>
    /// <item><description>返回最终的文件路径</description></item>
    /// </list>
    /// <para>此方法主要用于下载操作，确保目标文件可以被成功创建。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // 基本使用 - 仅文件名
    /// var path1 = await GetUniqueDestFilePathAsync("document.pdf", false, true);
    /// // 如果document.pdf存在，可能返回: "document[1].pdf"
    /// 
    /// // 包含相对路径的文件名
    /// var path2 = await GetUniqueDestFilePathAsync("downloads\\document.pdf", false, true);
    /// // 会自动创建downloads目录，如果文件存在可能返回: "downloads\\document[1].pdf"
    /// 
    /// // 允许覆盖的情况
    /// var path3 = await GetUniqueDestFilePathAsync("temp\\file.txt", true, false);
    /// // 返回: "temp\\file.txt"，会创建temp目录并允许覆盖
    /// 
    /// // 严格模式
    /// try 
    /// {
    ///     var path4 = await GetUniqueDestFilePathAsync("existing.txt", false, false);
    /// }
    /// catch (DuplicateFileException)
    /// {
    ///     // 如果existing.txt已存在，会抛出此异常
    /// }
    /// </code>
    /// </example>
    private async Task<string> GetUniqueDestFilePathAsync(string destFileName, bool overwrite, bool useSequencedName)
    {
        // 调用静态方法获取唯一的文件路径
        // 使用空字符串作为destPath，因为destFileName可能已包含相对路径
        var destFilePath = GetDestFilePath(string.Empty, destFileName, overwrite, useSequencedName);

        // 提取路径中的目录部分，为后续的目录创建做准备
        var directory = Path.GetDirectoryName(destFilePath);

        // 如果存在目录路径且目录不存在，则异步创建目录
        if (!string.IsNullOrEmpty(directory))
        {
            await CreateDirectoryIfNotExistsAsync(directory).ConfigureAwait(false);
        }

        // 返回已确保目录存在的唯一文件路径
        return destFilePath;
    }

    /// <summary>
    /// 得到 带有 RootDirectoryPath 的路径
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public string GetRealPath(string filePath)
    {
        if (filePath.StartsWith(RootDirectoryPath))
        {
            return filePath;
        }
        filePath = Path.Combine(RootDirectoryPath, filePath);
        return filePath;
    }

    public async Task DeleteAsync(string filePath)
    {
        var realPath = GetRealPath(filePath);
        await DeleteFileIfExistsAsync(realPath).ConfigureAwait(false);
    }

    public override async Task<FileOperationResult> UploadAsync(Stream inputStream, string filePath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        try
        {
            // 分离路径和文件名
            var destinationPath = Path.GetDirectoryName(filePath) ?? string.Empty;
            var fileName = Path.GetFileName(filePath);

            var uploadedInfo = await UploadAsync(inputStream, fileName, string.Empty, destinationPath,
                _options.DefaultNamingRule, overwrite, !overwrite).ConfigureAwait(false);

            return FileOperationResult.CreateSuccess(
                uploadedInfo.FilePath,
                uploadedInfo.FullFilePath,
                uploadedInfo.Length,
                uploadedInfo.HashData);
        }
        catch (Exception ex)
        {
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationPath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(localFilePath))
            {
                return FileOperationResult.CreateFailure($"本地文件不存在: {localFilePath}");
            }

            using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
            var fileName = Path.GetFileName(localFilePath);
            // 构建完整的文件路径并调用更新后的UploadAsync
            var filePath = Path.Combine(destinationPath, fileName);
            return await UploadAsync(fileStream, filePath, overwrite, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FileOperationResult.CreateFailure($"上传文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default)
    {
        try
        {
            await DownloadToStreamAsync(remoteFilePath, outputStream).ConfigureAwait(false);
            return FileOperationResult.CreateSuccess();
        }
        catch (Exception ex)
        {
            return FileOperationResult.CreateFailure($"下载文件到流失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DownloadFileAsync(string remoteFilePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        try
        {
            localDestinationPath = await DownloadAsync(remoteFilePath, localDestinationPath, overwrite, false).ConfigureAwait(false);
            var fileInfo = new FileInfo(localDestinationPath);
            return FileOperationResult.CreateSuccess(remoteFilePath, localDestinationPath, fileInfo.Length);
        }
        catch (Exception ex)
        {
            return FileOperationResult.CreateFailure($"下载文件失败: {ex.Message}", ex);
        }
    }

    public override async Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var realPath = GetRealPath(filePath);
            if (!File.Exists(realPath))
            {
                return FileOperationResult.CreateSuccess(filePath); // 文件不存在也视为成功
            }

            await Task.Run(() => File.Delete(realPath), cancellationToken).ConfigureAwait(false);
            return FileOperationResult.CreateSuccess(filePath);
        }
        catch (Exception ex)
        {
            return FileOperationResult.CreateFailure($"删除文件失败: {ex.Message}", ex);
        }
    }
}

public class UploadedInfo : CustomExistFileInfo
{
    /// <summary>
    /// Gets or sets the new file name.
    /// </summary>
    public string? NewFileName { get; set; }

    /// <summary>
    /// 除 destRootPath 以外的存储位置
    /// </summary>
    public string FilePath { get; set; } = null!;
}
