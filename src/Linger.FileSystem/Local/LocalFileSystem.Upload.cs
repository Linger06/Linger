namespace Linger.FileSystem.Local;

public partial class LocalFileSystem
{
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
}
