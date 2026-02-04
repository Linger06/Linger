using System.Text;

namespace Linger.FileSystem.Local;

/// <summary>
/// 本地文件系统配置选项
/// </summary>
public class LocalFileSystemOptions
{
    /// <summary>
    /// 根目录路径
    /// </summary>
    public string RootDirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// 重试选项
    /// </summary>
    public RetryOptions? RetryOptions { get; set; }

    /// <summary>
    /// 默认文本编码
    /// </summary>
    public Encoding TextEncoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// 默认命名规则
    /// </summary>
    public NamingRule DefaultNamingRule { get; set; } = NamingRule.Md5;

    /// <summary>
    /// 默认是否覆盖文件
    /// </summary>
    public bool DefaultOverwrite { get; set; }

    /// <summary>
    /// 默认是否使用序号命名（文件名冲突时）
    /// </summary>
    public bool DefaultUseSequencedName { get; set; } = true;

    private int _uploadBufferSize = 81920; // 默认 80KB
    private int _downloadBufferSize = 81920; // 默认 80KB

    /// <summary>
    /// 上传操作的缓冲区大小（字节）
    /// </summary>
    /// <remarks>
    /// 建议值在32KB到1MB之间，过小会影响上传效率，过大会占用过多内存
    /// </remarks>
    public int UploadBufferSize
    {
        get => _uploadBufferSize;
        set => _uploadBufferSize = value < 4096 ? 4096 : (value > 4194304 ? 4194304 : value);
    }

    /// <summary>
    /// 下载操作的缓冲区大小（字节）
    /// </summary>
    /// <remarks>
    /// 建议值在32KB到1MB之间，过小会影响下载效率，过大会占用过多内存
    /// </remarks>
    public int DownloadBufferSize
    {
        get => _downloadBufferSize;
        set => _downloadBufferSize = value < 4096 ? 4096 : (value > 4194304 ? 4194304 : value);
    }

    /// <summary>
    /// 文件验证级别
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description><see cref="FileValidationLevel.None"/>: 不验证（最快，适用于信任环境）</description></item>
    ///   <item><description><see cref="FileValidationLevel.SizeOnly"/>: 只验证文件大小（轻量级，开销极小）</description></item>
    ///   <item><description><see cref="FileValidationLevel.Full"/>: 完整验证：大小 + MD5 哈希（最安全，有额外 I/O 开销）</description></item>
    /// </list>
    /// </remarks>
    public FileValidationLevel ValidationLevel { get; set; } = FileValidationLevel.Full;

    /// <summary>
    /// 验证失败时是否自动清理文件
    /// </summary>
    public bool CleanupOnValidationFailure { get; set; } = true;

    /// <summary>
    /// 批量操作的最大并发度（默认 1 表示串行）
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 1;
}
