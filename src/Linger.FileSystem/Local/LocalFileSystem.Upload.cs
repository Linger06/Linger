namespace Linger.FileSystem.Local;

public partial class LocalFileSystem
{
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
            return FileOperationResult.CreateFailure($"Failed to upload the file: {ex.Message}", ex);
        }
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
            "File upload",
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
        var basePath = Path.Combine(containerName, destPath);
        string? filePath = namingRule switch
        {
            NamingRule.Md5 => null,
            NamingRule.Uuid => GenerateUuidBasedPath(containerName, destPath, Path.GetExtension(sourceFileName)),
            NamingRule.Normal => GetDestFilePath(basePath, sourceFileName, overwrite, useSequencedName, _rootDirectoryFullPath),
            _ => throw new ArgumentOutOfRangeException(nameof(namingRule), namingRule, "Unsupported naming rule.")
        };
        var fullFilePath = filePath is null ? null : GetRealPath(filePath);
        if (!overwrite && fullFilePath is not null && File.Exists(fullFilePath))
        {
            throw new DuplicateFileException();
        }

        var directory = fullFilePath is not null
            ? Path.GetDirectoryName(fullFilePath)!
            : string.IsNullOrEmpty(basePath) ? _rootDirectoryFullPath : GetRealPath(basePath);
        Directory.CreateDirectory(directory);
        var tempPath = Path.Combine(directory, $".upload-{Guid.NewGuid():N}.tmp");
        var retainInvalidFile = false;
        try
        {
            var (hash, length) = await CopyAndHashToFileAsync(inputStream, tempPath, cancellationToken).ConfigureAwait(false);
            if (filePath is null)
            {
                var stem = Path.GetFileNameWithoutExtension(sourceFileName).Replace(" ", string.Empty);
                filePath = Path.Combine(basePath, $"{stem}-{hash}{Path.GetExtension(sourceFileName)}");
            }

            fullFilePath ??= GetRealPath(filePath);

            // 校验本次写入的临时文件，不能校验或清理其他上传可能已替换的目标。
            try
            {
                await ValidateFileAsync(new FileInfo(tempPath), hash, length, cancellationToken).ConfigureAwait(false);
            }
            catch (InvalidOperationException) when (!_options.CleanupOnValidationFailure)
            {
                retainInvalidFile = true;
                throw;
            }

            cancellationToken.ThrowIfCancellationRequested();
            MoveUploadedFile(tempPath, fullFilePath, overwrite);

            return new UploadedInfo
            {
                HashData = hash,
                FileName = sourceFileName,
                NewFileName = Path.GetFileName(fullFilePath),
                FilePath = filePath,
                RelativeFilePath = Path.Combine(RootDirectoryPath, filePath),
                FullFilePath = fullFilePath,
                FileSize = length.FormatFileSize(),
                Length = length
            };
        }
        finally
        {
            if (!retainInvalidFile)
            {
                TryDeleteFile(tempPath);
            }
        }
    }

    private async Task<(string HashData, long Length)> CopyAndHashToFileAsync(
        Stream inputStream,
        string destinationFilePath,
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
                FileMode.CreateNew,
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
            throw new InvalidOperationException($"File validation failed: Size mismatch for {fileInfo.FullName}. Expected: {expectedSize}, Actual: {fileInfo.Length}");
        }

        if (_options.ValidationLevel == FileValidationLevel.SizeOnly)
        {
            return;
        }

        var uploadedFileHash = await ComputeFileHashAsync(fileInfo.FullName, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(uploadedFileHash, sourceHashData, StringComparison.Ordinal))
        {
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
}
