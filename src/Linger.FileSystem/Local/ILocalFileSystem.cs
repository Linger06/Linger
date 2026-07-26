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
