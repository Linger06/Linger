using System.Diagnostics.CodeAnalysis;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Linger.FileSystem;

/// <summary>
/// 所有文件系统的抽象基类，统一实现公共功能
/// </summary>
/// <remarks>
/// <para>此基类提供了文件系统操作的通用实现，包括重试机制和日志记录。</para>
/// <para>派生类可通过构造函数传入 <see cref="ILogger"/> 以启用日志记录。</para>
/// </remarks>
public abstract class FileSystemBase : IFileSystemOperations
{
    /// <summary>
    /// 重试助手，用于在操作失败时自动重试
    /// </summary>
    protected readonly RetryHelper RetryHelper;

    /// <summary>
    /// 日志记录器（可选）
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// 初始化 <see cref="FileSystemBase"/> 的新实例。
    /// </summary>
    /// <param name="retryOptions">重试选项，为 <c>null</c> 时使用默认配置。</param>
    /// <param name="logger">日志记录器，为 <c>null</c> 时使用 <see cref="NullLogger"/>。</param>
    protected FileSystemBase(RetryOptions? retryOptions = null, ILogger? logger = null)
    {
        RetryHelper = new RetryHelper(retryOptions ?? new RetryOptions());
        Logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Executes a stream operation with retry when the stream can be restored safely.
    /// </summary>
    protected Task<T> ExecuteStreamOperationAsync<T>(
        Stream stream,
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        bool restoreLength,
        CancellationToken cancellationToken,
        Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(operation);

        var canRestore = stream.CanSeek && (!restoreLength || stream.CanWrite);
        var initialPosition = canRestore ? stream.Position : 0;
        var initialLength = canRestore && restoreLength ? stream.Length : 0;

        if (canRestore && restoreLength)
        {
            try
            {
                stream.SetLength(initialLength);
                stream.Position = initialPosition;
            }
            catch (NotSupportedException)
            {
                canRestore = false;
            }
        }

        return RetryHelper.ExecuteAsync(
            async operationCancellationToken =>
            {
                if (canRestore)
                {
                    stream.Position = initialPosition;
                    if (restoreLength)
                    {
                        stream.SetLength(initialLength);
                        stream.Position = initialPosition;
                    }
                }

                return await operation(operationCancellationToken).ConfigureAwait(false);
            },
            operationName,
            exception => canRestore && (shouldRetry?.Invoke(exception) ?? true),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Executes one batch item, records failures, and reports completion unless the operation is cancelled.
    /// </summary>
    protected static async Task ExecuteBatchItemAsync(
        string filePath,
        Func<Task> operation,
        BatchOperationTracker tracker)
    {
        var reportCompletion = true;
        try
        {
            await operation().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            reportCompletion = false;
            throw;
        }
        catch (Exception ex)
        {
            tracker.AddFailure(filePath, ex.Message, ex);
        }
        finally
        {
            if (reportCompletion)
            {
                tracker.ReportCompleted(filePath);
            }
        }
    }

    /// <summary>
    /// Tracks batch operation results and progress consistently across file-system implementations.
    /// </summary>
    protected sealed class BatchOperationTracker
    {
        private readonly ConcurrentBag<string> _succeeded = [];
        private readonly ConcurrentBag<BatchOperationFailure> _failed = [];
        private readonly IProgress<BatchProgress>? _progress;
        private readonly int _total;
        private int _completed;

        /// <summary>
        /// Initializes a tracker for a batch operation.
        /// </summary>
        public BatchOperationTracker(int total, IProgress<BatchProgress>? progress)
        {
            _total = total;
            _progress = progress;
        }

        /// <summary>
        /// Records a successful file operation.
        /// </summary>
        public void AddSuccess(string filePath)
        {
            _succeeded.Add(filePath);
        }

        /// <summary>
        /// Records a failed file operation.
        /// </summary>
        public void AddFailure(string filePath, string errorMessage, Exception? exception = null)
        {
            _failed.Add(new BatchOperationFailure(filePath, errorMessage, exception));
        }

        /// <summary>
        /// Records an existing failure result.
        /// </summary>
        public void AddFailure(BatchOperationFailure failure)
        {
            ArgumentNullException.ThrowIfNull(failure);
            _failed.Add(failure);
        }

        /// <summary>
        /// Reports completion of one file operation.
        /// </summary>
        public void ReportCompleted(string filePath)
        {
            var completed = Interlocked.Increment(ref _completed);
            _progress?.Report(new BatchProgress(completed, _total, filePath, _succeeded.Count, _failed.Count));
        }

        /// <summary>
        /// Completes progress reporting and creates the batch result.
        /// </summary>
        public BatchOperationResult Complete()
        {
            _progress?.Report(new BatchProgress(_total, _total, string.Empty, _succeeded.Count, _failed.Count));

            return new BatchOperationResult
            {
                SucceededFiles = _succeeded.ToList(),
                FailedFiles = _failed.ToList()
            };
        }
    }

    /// <summary>
    /// 是否为远程文件系统
    /// </summary>
    public virtual bool IsRemoteFileSystem => false;

    #region 基础路径操作

    /// <summary>
    /// 异常处理并记录日志
    /// </summary>
    [DoesNotReturn]
    protected virtual void HandleException(string operation, Exception ex, string? path = null, [CallerMemberName] string callerMethod = "")
    {
        if (ex is OperationCanceledException)
        {
            ExceptionDispatchInfo.Capture(ex).Throw();
        }

        var message = $"{operation} failed. {(path is not null ? $"Path: {path}" : string.Empty)}, Method: {callerMethod}";
        Logger.LogError(ex, "{Message}", message);
        throw new FileSystemException(operation, path, message, ex);
    }

    #endregion

    #region IFileSystem 实现

    public abstract Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

    public abstract Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    public abstract Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    public abstract Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default);

    #endregion

    #region IFileSystemOperations 实现

    public abstract Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default);

    public abstract Task<Stream> OpenWriteAsync(string filePath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<StreamReader> GetReaderAsync(string filePath, Encoding? encoding = null, CancellationToken cancellationToken = default);

    public abstract Task<StreamWriter> GetWriterAsync(string filePath, bool overwrite = false, Encoding? encoding = null, CancellationToken cancellationToken = default);

    public abstract Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DownloadFileAsync(string remoteFilePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default);

    public abstract Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    #endregion
}
