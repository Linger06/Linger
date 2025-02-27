using Linger.FileSystem.Helpers;
using Linger.Helper;
using System.IO;

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
    /// 默认命名规则
    /// </summary>
    public NamingRule DefaultNamingRule { get; set; } = NamingRule.Md5;
    
    /// <summary>
    /// 默认是否覆盖文件
    /// </summary>
    public bool DefaultOverwrite { get; set; } = false;
    
    /// <summary>
    /// 默认是否使用序号命名（文件名冲突时）
    /// </summary>
    public bool DefaultUseSequencedName { get; set; } = true;
    
    /// <summary>
    /// 上传缓冲区大小（字节）
    /// </summary>
    public int UploadBufferSize { get; set; } = 81920; // 默认 80KB
    
    /// <summary>
    /// 是否验证文件完整性
    /// </summary>
    public bool ValidateFileIntegrity { get; set; } = true;
    
    /// <summary>
    /// 是否验证文件元数据
    /// </summary>
    public bool ValidateFileMetadata { get; set; } = true;
    
    /// <summary>
    /// 验证失败时是否自动清理文件
    /// </summary>
    public bool CleanupOnValidationFailure { get; set; } = true;
}