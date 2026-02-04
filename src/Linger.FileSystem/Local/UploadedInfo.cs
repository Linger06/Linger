namespace Linger.FileSystem.Local;

/// <summary>
/// 上传文件的结果信息
/// </summary>
public class UploadedInfo : ExtendedFileInfo
{
    /// <summary>
    /// Gets or sets the new file name.
    /// </summary>
    public string? NewFileName { get; set; }

    /// <summary>
    /// 除 destRootPath 以外的存储位置
    /// </summary>
    public string FilePath { get; set; } = null!;
}
