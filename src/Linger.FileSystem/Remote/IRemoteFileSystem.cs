using System.Text;

namespace Linger.FileSystem.Remote;

/// <summary>
/// 定义远程文件系统的异步协议操作和连接生命周期。
/// </summary>
/// <remarks>
/// <para>此接口扩展了 <see cref="IFileTransfer"/>，并直接声明远程元数据、流和目录操作。</para>
/// <para>实现类应支持 <see cref="IAsyncDisposable"/> 以便正确释放异步资源。</para>
/// <para>远程实现通常在实例内复用一个有状态客户端。不要在同一实例上并发执行操作，
/// 也不要在其他操作运行期间调用 <see cref="SetWorkingDirectoryAsync"/>。</para>
/// </remarks>
public interface IRemoteFileSystem : IFileTransfer, IDisposable, IAsyncDisposable
{
    /// <summary>异步检查远程文件是否存在。</summary>
    /// <param name="filePath">远程文件路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>文件存在时返回 <see langword="true"/>。</returns>
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>异步检查远程目录是否存在。</summary>
    /// <param name="directoryPath">远程目录路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>目录存在时返回 <see langword="true"/>。</returns>
    Task<bool> DirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>异步确保远程目录存在。</summary>
    /// <param name="directoryPath">远程目录路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    Task CreateDirectoryIfNotExistsAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>异步删除存在的远程文件。</summary>
    /// <param name="filePath">远程文件路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    Task DeleteFileIfExistsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>异步打开远程文件并返回可读流。</summary>
    /// <param name="filePath">远程文件路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>可读流。</returns>
    Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>异步打开或创建远程文件并返回可写流。</summary>
    /// <param name="filePath">远程文件路径。</param>
    /// <param name="overwrite">是否覆盖已有文件。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>可写流。</returns>
    Task<Stream> OpenWriteAsync(string filePath, bool overwrite = false, CancellationToken cancellationToken = default);

    /// <summary>异步创建远程文本读取器。</summary>
    /// <param name="filePath">远程文件路径。</param>
    /// <param name="encoding">文本编码；为 <see langword="null"/> 时使用默认编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>文本读取器。</returns>
    Task<StreamReader> GetReaderAsync(string filePath, Encoding? encoding = null, CancellationToken cancellationToken = default);

    /// <summary>异步创建远程文本写入器。</summary>
    /// <param name="filePath">远程文件路径。</param>
    /// <param name="overwrite">是否覆盖已有文件。</param>
    /// <param name="encoding">文本编码；为 <see langword="null"/> 时使用默认编码。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>文本写入器。</returns>
    Task<StreamWriter> GetWriterAsync(string filePath, bool overwrite = false, Encoding? encoding = null, CancellationToken cancellationToken = default);

    /// <summary>异步获取远程文件大小。</summary>
    /// <param name="filePath">远程文件路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>文件字节数；文件不存在时返回 <see langword="null"/>。</returns>
    Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>异步删除远程文件。</summary>
    /// <param name="filePath">远程文件路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>文件操作结果。</returns>
    Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously gets the last modified time of a remote file.
    /// </summary>
    /// <param name="filePath">The remote file path.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The remote file's last modified time.</returns>
    Task<DateTime> GetLastModifiedTimeAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously changes the current working directory on the remote server.
    /// </summary>
    /// <param name="directoryPath">The remote directory path.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetWorkingDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步列出远程目录中的文件名。
    /// </summary>
    /// <param name="directoryPath">远程目录路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>远程目录中的文件名列表。</returns>
    Task<IReadOnlyList<string>> ListFilesAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步列出远程目录中的子目录名。
    /// </summary>
    /// <param name="directoryPath">远程目录路径。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>远程目录中的子目录名列表。</returns>
    Task<IReadOnlyList<string>> ListDirectoriesAsync(
        string directoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取服务器详细信息的描述字符串。
    /// </summary>
    /// <value>包含服务器类型、主机、端口等信息的字符串。</value>
    string ServerDetails { get; }
}
