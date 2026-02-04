namespace Linger.FileSystem.Local;

/// <summary>
/// 本地文件系统接口，扩展了 <see cref="IFileSystemOperations"/> 和 <see cref="IBatchFileSystemOperations"/> 以提供本地文件系统特有的功能。
/// </summary>
public interface ILocalFileSystem : IFileSystemOperations, IBatchFileSystemOperations
{
    /// <summary>
    /// 获取根目录路径。
    /// </summary>
    string RootDirectoryPath { get; }

    /// <summary>
    /// 检查根目录是否存在。
    /// </summary>
    /// <returns>如果根目录存在，则为 <c>true</c>；否则为 <c>false</c>。</returns>
    bool Exists();

    /// <summary>
    /// 异步检查根目录是否存在。
    /// </summary>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    /// <returns>表示异步操作的任务，任务结果为 <c>true</c> 表示根目录存在；否则为 <c>false</c>。</returns>
    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 如果根目录不存在则创建它。
    /// </summary>
    void CreateIfNotExists();

    /// <summary>
    /// 异步创建根目录（如果不存在）。
    /// </summary>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    /// <returns>表示异步操作的任务。</returns>
    Task CreateIfNotExistsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 上传流到本地文件系统，支持多种命名规则和冲突处理策略。
    /// </summary>
    /// <param name="inputStream">输入流。</param>
    /// <param name="sourceFileName">源文件名。</param>
    /// <param name="containerName">容器名称（子目录）。</param>
    /// <param name="destPath">目标路径。</param>
    /// <param name="namingRule">命名规则。</param>
    /// <param name="overwrite">是否覆盖已存在的文件。</param>
    /// <param name="useSequencedName">文件冲突时是否使用序号命名。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>上传结果信息。</returns>
    Task<UploadedInfo> UploadAsync(
        Stream inputStream,
        string sourceFileName,
        string containerName = "",
        string destPath = "",
        NamingRule? namingRule = null,
        bool? overwrite = null,
        bool? useSequencedName = null,
        CancellationToken cancellationToken = default);
}