using System;

namespace Linger.FileSystem;

/// <summary>
/// 文件操作结果
/// </summary>
public class FileOperationResult
{
    /// <summary>
    /// 操作是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 错误信息（如果操作失败）
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 异常信息（如果操作失败）
    /// </summary>
    public Exception? Exception { get; set; }
    
    /// <summary>
    /// 文件路径
    /// </summary>
    public string? FilePath { get; set; }
    
    /// <summary>
    /// 完整文件路径（本地文件系统适用）
    /// </summary>
    public string? FullFilePath { get; set; }
    
    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// 文件哈希值（如MD5）
    /// </summary>
    public string? FileHash { get; set; }
    
    /// <summary>
    /// 创建成功的操作结果
    /// </summary>
    public static FileOperationResult CreateSuccess(string filePath, string? fullFilePath = null, long fileSize = 0, string? fileHash = null)
    {
        return new FileOperationResult
        {
            Success = true,
            FilePath = filePath,
            FullFilePath = fullFilePath,
            FileSize = fileSize,
            FileHash = fileHash
        };
    }
    
    /// <summary>
    /// 创建失败的操作结果
    /// </summary>
    public static FileOperationResult CreateFailure(string errorMessage, Exception? exception = null)
    {
        return new FileOperationResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}
