namespace Linger.FileSystem.Local;

/// <summary>
/// 文件验证级别
/// </summary>
public enum FileValidationLevel
{
    /// <summary>
    /// 不验证（最快，适用于信任环境）
    /// </summary>
    None,

    /// <summary>
    /// 只验证文件大小（轻量级，开销极小）
    /// </summary>
    SizeOnly,

    /// <summary>
    /// 完整验证：大小 + MD5 哈希（最安全，有额外 I/O 开销）
    /// </summary>
    Full
}
