using System;

namespace Linger.FileSystem.Exceptions;

/// <summary>
/// 文件系统操作的基础异常类型
/// </summary>
public class FileSystemException : Exception
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public string Operation { get; }
    
    /// <summary>
    /// 相关文件路径
    /// </summary>
    public string? FilePath { get; }
    
    /// <summary>
    /// 服务器信息（如适用）
    /// </summary>
    public string? ServerInfo { get; }

    public FileSystemException(string message) : base(message)
    {
        Operation = "Unknown";
    }
    
    public FileSystemException(string operation, string message) : base(message)
    {
        Operation = operation;
    }
    
    public FileSystemException(string operation, string message, Exception innerException) : base(message, innerException)
    {
        Operation = operation;
    }
    
    public FileSystemException(string operation, string? filePath, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Operation = operation;
        FilePath = filePath;
    }
    
    public FileSystemException(string operation, string? filePath, string? serverInfo, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Operation = operation;
        FilePath = filePath;
        ServerInfo = serverInfo;
    }
}
