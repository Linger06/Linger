namespace Linger.FileSystem.Local;

public partial class LocalFileSystem
{
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
                    return new BatchOperationFailure(sourcePath, "The local file does not exist.");
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
                    return new BatchOperationFailure(remotePath, "The source file does not exist.");
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
}
