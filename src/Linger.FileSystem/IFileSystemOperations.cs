using System.Text;

namespace Linger.FileSystem;

/// <summary>
/// 定义统一的文件系统操作接口，适用于本地和远程文件系统
/// </summary>
/// <remarks>
/// <para>此接口提供了一套完整的文件系统操作 API，包括：</para>
/// <list type="bullet">
///   <item><description>流式读写：<see cref="OpenReadAsync"/>、<see cref="OpenWriteAsync"/></description></item>
///   <item><description>文本读写：<see cref="GetReaderAsync"/>、<see cref="GetWriterAsync"/></description></item>
///   <item><description>元数据查询：<see cref="IsDirectoryAsync"/>、<see cref="GetFileSizeAsync"/></description></item>
///   <item><description>文件传输：<see cref="UploadAsync"/>、<see cref="DownloadToStreamAsync"/> 等</description></item>
/// </list>
/// <para>所有方法均支持 <see cref="CancellationToken"/>，可在长时间操作中安全取消。</para>
/// </remarks>
public interface IFileSystemOperations : IFileSystem
{
    /// <summary>
    /// 异步打开文件并返回可读流。
    /// </summary>
    /// <param name="filePath">要读取的文件路径（相对于根目录或绝对路径）。</param>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    /// <returns>表示异步操作的任务，任务结果为可读取的 <see cref="Stream"/>。</returns>
    /// <exception cref="FileNotFoundException">当指定的文件不存在时抛出。</exception>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出。</exception>
    /// <remarks>
    /// <para>此方法返回的流可能在具体实现中包裹压缩解码或格式转换逻辑。</para>
    /// <para>调用方负责在使用完毕后释放返回的流。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// await using var stream = await fileSystem.OpenReadAsync("data/config.json", cancellationToken);
    /// // 读取流内容...
    /// </code>
    /// </example>
    Task<Stream> OpenReadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步打开或创建文件并返回可写流。
    /// </summary>
    /// <param name="filePath">要写入的文件路径（相对于根目录或绝对路径）。</param>
    /// <param name="overwrite">如果为 <c>true</c>，则覆盖已存在的文件；否则在文件存在时抛出异常。默认为 <c>false</c>。</param>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    /// <returns>表示异步操作的任务，任务结果为可写入的 <see cref="Stream"/>。</returns>
    /// <exception cref="DuplicateFileException">当 <paramref name="overwrite"/> 为 <c>false</c> 且文件已存在时抛出。</exception>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出。</exception>
    /// <remarks>
    /// <para>此方法会自动创建目标文件所在的目录（如果不存在）。</para>
    /// <para>返回的流可能在具体实现中包裹压缩编码或格式转换逻辑。</para>
    /// <para>调用方负责在使用完毕后释放返回的流，以确保数据正确写入。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// await using var stream = await fileSystem.OpenWriteAsync("output/result.txt", overwrite: true, cancellationToken);
    /// await stream.WriteAsync(data, cancellationToken);
    /// </code>
    /// </example>
    Task<Stream> OpenWriteAsync(string filePath, bool overwrite = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步打开文件并返回文本读取器。
    /// </summary>
    /// <param name="filePath">要读取的文件路径（相对于根目录或绝对路径）。</param>
    /// <param name="encoding">文本编码。如果为 <c>null</c>，则使用配置的默认编码（通常为 UTF-8）。</param>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    /// <returns>表示异步操作的任务，任务结果为 <see cref="StreamReader"/> 实例。</returns>
    /// <exception cref="FileNotFoundException">当指定的文件不存在时抛出。</exception>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出。</exception>
    /// <remarks>
    /// <para>此方法是 <see cref="OpenReadAsync"/> 的便捷封装，适用于文本文件读取场景。</para>
    /// <para>调用方负责在使用完毕后释放返回的读取器。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using var reader = await fileSystem.GetReaderAsync("data/log.txt", Encoding.UTF8, cancellationToken);
    /// var content = await reader.ReadToEndAsync();
    /// </code>
    /// </example>
    Task<StreamReader> GetReaderAsync(string filePath, Encoding? encoding = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步打开或创建文件并返回文本写入器。
    /// </summary>
    /// <param name="filePath">要写入的文件路径（相对于根目录或绝对路径）。</param>
    /// <param name="overwrite">如果为 <c>true</c>，则覆盖已存在的文件；否则在文件存在时抛出异常。默认为 <c>false</c>。</param>
    /// <param name="encoding">文本编码。如果为 <c>null</c>，则使用配置的默认编码（通常为 UTF-8）。</param>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    /// <returns>表示异步操作的任务，任务结果为 <see cref="StreamWriter"/> 实例。</returns>
    /// <exception cref="DuplicateFileException">当 <paramref name="overwrite"/> 为 <c>false</c> 且文件已存在时抛出。</exception>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出。</exception>
    /// <remarks>
    /// <para>此方法是 <see cref="OpenWriteAsync"/> 的便捷封装，适用于文本文件写入场景。</para>
    /// <para>调用方负责在使用完毕后释放返回的写入器，以确保数据正确刷新到文件。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// await using var writer = await fileSystem.GetWriterAsync("output/report.csv", overwrite: true, Encoding.UTF8, cancellationToken);
    /// await writer.WriteLineAsync("Name,Value");
    /// </code>
    /// </example>
    Task<StreamWriter> GetWriterAsync(string filePath, bool overwrite = false, Encoding? encoding = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步判断指定路径是否为目录。
    /// </summary>
    /// <param name="directoryPath">要检查的路径（相对于根目录或绝对路径）。</param>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    /// <returns>
    /// 表示异步操作的任务，任务结果为 <c>true</c> 表示路径是一个已存在的目录；
    /// <c>false</c> 表示路径不存在或不是目录。
    /// </returns>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出。</exception>
    /// <remarks>
    /// <para>此方法可用于在执行文件操作前判断路径类型，避免对目录执行文件操作。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// if (await fileSystem.IsDirectoryAsync("data", cancellationToken))
    /// {
    ///     // 处理目录...
    /// }
    /// </code>
    /// </example>
    Task<bool> IsDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步获取文件大小（以字节为单位）。
    /// </summary>
    /// <param name="filePath">要查询的文件路径（相对于根目录或绝对路径）。</param>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    /// <returns>
    /// 表示异步操作的任务，任务结果为文件大小（字节）；
    /// 如果文件不存在，则返回 <c>null</c>。
    /// </returns>
    /// <exception cref="OperationCanceledException">当操作被取消时抛出。</exception>
    /// <remarks>
    /// <para>此方法可用于在下载或处理文件前预估进度、分片大小或限流策略。</para>
    /// <para>对于远程文件系统，此操作可能涉及网络请求。</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var size = await fileSystem.GetFileSizeAsync("data/large-file.zip", cancellationToken);
    /// if (size.HasValue)
    /// {
    ///     Console.WriteLine($"文件大小: {size.Value} 字节");
    /// }
    /// </code>
    /// </example>
    Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// 上传流到文件系统
    /// </summary>
    /// <param name="inputStream">输入流</param>
    /// <param name="destinationFilePath">目标文件路径,包含文件名</param>
    /// <param name="overwrite">是否覆盖</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传结果</returns>
    Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 上传本地文件到文件系统
    /// </summary>
    /// <param name="localFilePath">本地文件路径</param>
    /// <param name="destinationFilePath">目标文件路径,包含文件名</param>
    /// <param name="overwrite">是否覆盖</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传结果</returns>
    Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationFilePath, bool overwrite = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载文件到流
    /// </summary>
    /// <param name="remoteFilePath">文件路径</param>
    /// <param name="outputStream">输出流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>下载结果</returns>
    Task<FileOperationResult> DownloadToStreamAsync(string remoteFilePath, Stream outputStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载文件到本地路径
    /// </summary>
    /// <param name="remoteFilePath">文件路径</param>
    /// <param name="localDestinationPath">本地目标路径</param>
    /// <param name="overwrite">是否覆盖</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>下载结果</returns>
    Task<FileOperationResult> DownloadFileAsync(string remoteFilePath, string localDestinationPath, bool overwrite = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>删除结果</returns>
    Task<FileOperationResult> DeleteAsync(string filePath, CancellationToken cancellationToken = default);
}
