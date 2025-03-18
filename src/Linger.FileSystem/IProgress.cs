using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Linger.FileSystem;

/// <summary>
/// 文件操作进度信息
/// </summary>
public class FileTransferProgress
{
    /// <summary>
    /// 已传输的字节数
    /// </summary>
    public long BytesTransferred { get; set; }
    
    /// <summary>
    /// 总字节数
    /// </summary>
    public long TotalBytes { get; set; }
    
    /// <summary>
    /// 传输进度百分比 (0-100)
    /// </summary>
    public double ProgressPercentage => TotalBytes > 0 ? (double)BytesTransferred / TotalBytes * 100 : 0;
    
    /// <summary>
    /// 文件路径
    /// </summary>
    public string? FilePath { get; set; }
    
    /// <summary>
    /// 传输类型 (上传/下载)
    /// </summary>
    public string TransferType { get; set; } = string.Empty;
}

/// <summary>
/// 支持进度报告的文件系统操作扩展接口
/// </summary>
public interface IFileSystemOperationsWithProgress : IFileSystemOperations
{
    Task<FileOperationResult> UploadAsync(Stream inputStream, string destinationPath, string fileName, 
        System.IProgress<FileTransferProgress>? progress, bool overwrite = false, CancellationToken cancellationToken = default);
        
    Task<FileOperationResult> UploadFileAsync(string localFilePath, string destinationPath, 
        System.IProgress<FileTransferProgress>? progress, bool overwrite = false, CancellationToken cancellationToken = default);
        
    Task<FileOperationResult> DownloadToStreamAsync(string filePath, Stream outputStream, 
        System.IProgress<FileTransferProgress>? progress, CancellationToken cancellationToken = default);
        
    Task<FileOperationResult> DownloadFileAsync(string filePath, string localDestinationPath, 
        System.IProgress<FileTransferProgress>? progress, bool overwrite = false, CancellationToken cancellationToken = default);
}
