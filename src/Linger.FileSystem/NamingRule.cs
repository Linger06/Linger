namespace Linger.FileSystem;

/// <summary>
/// 文件命名规则
/// </summary>
public enum NamingRule
{
    /// <summary>
    /// 保持原始文件名，可能会基于重复规则添加序号
    /// </summary>
    Normal,
    
    /// <summary>
    /// 使用文件内容的MD5哈希值作为文件名的一部分
    /// </summary>
    Md5,
    
    /// <summary>
    /// 使用随机生成的UUID作为文件名
    /// </summary>
    Uuid
}
