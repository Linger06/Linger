namespace Linger.FileSystem;

/// <summary>
/// 批量操作进度信息
/// </summary>
/// <param name="Completed">已完成的文件数量</param>
/// <param name="Total">总文件数量</param>
/// <param name="CurrentFile">当前刚处理完成的文件路径</param>
/// <param name="Succeeded">成功的文件数量</param>
/// <param name="Failed">失败的文件数量</param>
/// <remarks>
/// <para>进度报告在每个文件处理完成后触发，<see cref="Completed"/> 值准确反映已完成的任务数。</para>
/// <para>在并发模式下（<c>MaxDegreeOfParallelism &gt; 1</c>），报告顺序可能与输入顺序不同，
/// 取决于各任务的完成时间。</para>
/// </remarks>
public readonly record struct BatchProgress(
    int Completed,
    int Total,
    string CurrentFile,
    int Succeeded,
    int Failed)
{
    /// <summary>
    /// 完成百分比 (0-100)
    /// </summary>
    public double PercentComplete => Total > 0 ? (double)Completed / Total * 100 : 0;
}

/// <summary>
/// 批量文件操作结果
/// </summary>
public class BatchOperationResult
{
    /// <summary>
    /// 成功的文件路径列表
    /// </summary>
    public IReadOnlyList<string> SucceededFiles { get; set; } = [];

    /// <summary>
    /// 失败的文件及其错误信息
    /// </summary>
    public IReadOnlyList<BatchOperationFailure> FailedFiles { get; set; } = [];

    /// <summary>
    /// 成功的文件数量
    /// </summary>
    public int SuccessCount => SucceededFiles.Count;

    /// <summary>
    /// 失败的文件数量
    /// </summary>
    public int FailureCount => FailedFiles.Count;

    /// <summary>
    /// 总文件数量
    /// </summary>
    public int TotalCount => SuccessCount + FailureCount;

    /// <summary>
    /// 是否全部成功
    /// </summary>
    public bool AllSucceeded => FailureCount == 0;

    /// <summary>
    /// 是否有任何成功
    /// </summary>
    public bool AnySucceeded => SuccessCount > 0;

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static BatchOperationResult Success(IEnumerable<string> files)
    {
        return new BatchOperationResult
        {
            SucceededFiles = files.ToList()
        };
    }

    /// <summary>
    /// 创建空结果
    /// </summary>
    public static BatchOperationResult Empty => new();
}

/// <summary>
/// 批量操作失败项
/// </summary>
public class BatchOperationFailure
{
    /// <summary>
    /// 初始化 <see cref="BatchOperationFailure"/> 类的新实例
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="errorMessage">错误信息</param>
    /// <param name="exception">异常信息（可选）</param>
    public BatchOperationFailure(string filePath, string errorMessage, Exception? exception = null)
    {
        FilePath = filePath;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// 异常信息（可选）
    /// </summary>
    public Exception? Exception { get; }
}

/// <summary>
/// 定义批量文件系统操作接口
/// </summary>
/// <remarks>
/// <para>此接口提供文件系统的批量操作功能，适用于需要处理多个文件的场景。</para>
/// <para>所有方法均为异步方法，支持取消操作和进度报告。</para>
/// </remarks>
public interface IBatchFileSystemOperations
{
    /// <summary>
    /// 批量上传本地文件到远程目录
    /// </summary>
    /// <param name="localFilePaths">本地文件路径列表</param>
    /// <param name="remoteDirectory">远程目标目录</param>
    /// <param name="overwrite">是否覆盖已存在的文件</param>
    /// <param name="progress">进度报告回调（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>批量操作结果</returns>
    /// <example>
    /// <code>
    /// // 不带进度报告
    /// var result = await fileSystem.UploadFilesAsync(files, "/remote/uploads", overwrite: true);
    /// 
    /// // 带进度报告
    /// var progress = new Progress&lt;BatchProgress&gt;(p =>
    ///     Console.WriteLine($"进度: {p.Completed}/{p.Total} ({p.PercentComplete:F1}%)"));
    /// var result = await fileSystem.UploadFilesAsync(files, "/remote/uploads", true, progress);
    /// </code>
    /// </example>
    Task<BatchOperationResult> UploadFilesAsync(
        IEnumerable<string> localFilePaths,
        string remoteDirectory,
        bool overwrite = false,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量下载远程文件到本地目录
    /// </summary>
    /// <param name="remoteFilePaths">远程文件路径列表</param>
    /// <param name="localDirectory">本地目标目录</param>
    /// <param name="overwrite">是否覆盖已存在的文件</param>
    /// <param name="progress">进度报告回调（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>批量操作结果</returns>
    /// <example>
    /// <code>
    /// var remoteFiles = new[] { "/remote/file1.txt", "/remote/file2.txt" };
    /// var result = await fileSystem.DownloadFilesAsync(remoteFiles, "C:/Downloads", overwrite: true);
    /// foreach (var failure in result.FailedFiles)
    /// {
    ///     Console.WriteLine($"下载失败: {failure.FilePath} - {failure.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    Task<BatchOperationResult> DownloadFilesAsync(
        IEnumerable<string> remoteFilePaths,
        string localDirectory,
        bool overwrite = false,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量删除文件
    /// </summary>
    /// <param name="filePaths">要删除的文件路径列表</param>
    /// <param name="progress">进度报告回调（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>批量操作结果</returns>
    /// <example>
    /// <code>
    /// var filesToDelete = new[] { "/remote/old1.txt", "/remote/old2.txt" };
    /// var result = await fileSystem.DeleteFilesAsync(filesToDelete);
    /// Console.WriteLine($"已删除 {result.SuccessCount} 个文件");
    /// </code>
    /// </example>
    Task<BatchOperationResult> DeleteFilesAsync(
        IEnumerable<string> filePaths,
        IProgress<BatchProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 列出目录中的文件
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件名列表</returns>
    /// <example>
    /// <code>
    /// var files = await fileSystem.ListFilesAsync("/remote/documents");
    /// foreach (var file in files)
    /// {
    ///     Console.WriteLine(file);
    /// }
    /// </code>
    /// </example>
    Task<IReadOnlyList<string>> ListFilesAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 列出目录中的子目录
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>子目录名列表</returns>
    /// <example>
    /// <code>
    /// var directories = await fileSystem.ListDirectoriesAsync("/remote");
    /// foreach (var dir in directories)
    /// {
    ///     Console.WriteLine(dir);
    /// }
    /// </code>
    /// </example>
    Task<IReadOnlyList<string>> ListDirectoriesAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);
}
