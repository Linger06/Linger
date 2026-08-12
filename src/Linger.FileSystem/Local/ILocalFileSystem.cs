using System.Text;

namespace Linger.FileSystem.Local;

/// <summary>
/// 本地文件系统接口。元数据、流创建和删除使用同步 API；文件内容传输使用真实异步 I/O。
/// </summary>
public interface ILocalFileSystem : IFileTransfer, ILocalBatchFileSystemOperations
{
    /// <summary>检查本地文件是否存在。</summary>
    /// <param name="filePath">相对于根目录或位于根目录内的绝对路径。</param>
    /// <returns>文件存在时返回 <see langword="true"/>。</returns>
    bool FileExists(string filePath);

    /// <summary>检查本地目录是否存在。</summary>
    /// <param name="directoryPath">相对于根目录或位于根目录内的绝对路径。</param>
    /// <returns>目录存在时返回 <see langword="true"/>。</returns>
    bool DirectoryExists(string directoryPath);

    /// <summary>确保本地目录存在。</summary>
    /// <param name="directoryPath">相对于根目录或位于根目录内的绝对路径。</param>
    void CreateDirectoryIfNotExists(string directoryPath);

    /// <summary>在本地文件存在时将其删除。</summary>
    /// <param name="filePath">相对于根目录或位于根目录内的绝对路径。</param>
    void DeleteFileIfExists(string filePath);

    /// <summary>同步打开本地文件并返回支持异步读取的流。</summary>
    /// <param name="filePath">相对于根目录或位于根目录内的绝对路径。</param>
    /// <returns>可读文件流。</returns>
    Stream OpenRead(string filePath);

    /// <summary>同步打开或创建本地文件并返回支持异步写入的流。</summary>
    /// <param name="filePath">相对于根目录或位于根目录内的绝对路径。</param>
    /// <param name="overwrite">是否覆盖已有文件。</param>
    /// <returns>可写文件流。</returns>
    Stream OpenWrite(string filePath, bool overwrite = false);

    /// <summary>同步创建本地文本读取器。</summary>
    /// <param name="filePath">相对于根目录或位于根目录内的绝对路径。</param>
    /// <param name="encoding">文本编码；为 <see langword="null"/> 时使用配置的默认编码。</param>
    /// <returns>文本读取器。</returns>
    StreamReader GetReader(string filePath, Encoding? encoding = null);

    /// <summary>同步创建本地文本写入器。</summary>
    /// <param name="filePath">相对于根目录或位于根目录内的绝对路径。</param>
    /// <param name="overwrite">是否覆盖已有文件。</param>
    /// <param name="encoding">文本编码；为 <see langword="null"/> 时使用配置的默认编码。</param>
    /// <returns>文本写入器。</returns>
    StreamWriter GetWriter(string filePath, bool overwrite = false, Encoding? encoding = null);

    /// <summary>获取本地文件大小。</summary>
    /// <param name="filePath">相对于根目录或位于根目录内的绝对路径。</param>
    /// <returns>文件字节数；文件不存在时返回 <see langword="null"/>。</returns>
    long? GetFileSize(string filePath);

    /// <summary>删除本地文件并返回操作结果。</summary>
    /// <param name="filePath">相对于根目录或位于根目录内的绝对路径。</param>
    /// <returns>文件操作结果。</returns>
    FileOperationResult Delete(string filePath);

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
    /// <example>
    /// <code>
    /// using var stream = File.OpenRead("avatar.png");
    /// var result = await fileSystem.UploadWithNamingAsync(
    ///     stream,
    ///     "avatar.png",
    ///     "users",
    ///     namingRule: NamingRule.Uuid);
    /// </code>
    /// </example>
    Task<UploadedInfo> UploadWithNamingAsync(
        Stream inputStream,
        string sourceFileName,
        string containerName = "",
        string destPath = "",
        NamingRule? namingRule = null,
        bool? overwrite = null,
        bool? useSequencedName = null,
        CancellationToken cancellationToken = default);

}
