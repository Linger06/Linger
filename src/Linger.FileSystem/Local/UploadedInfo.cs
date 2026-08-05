namespace Linger.FileSystem.Local;

/// <summary>
/// 上传文件的结果信息
/// </summary>
public class UploadedInfo
{
    /// <summary>
    /// Gets or sets the hash data of the file.
    /// </summary>
    public string HashData { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fully qualified path and name of the file.
    /// </summary>
    public string FullFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size.
    /// </summary>
    public string FileSize { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the length of the file.
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// Gets or sets the new file name.
    /// </summary>
    public string? NewFileName { get; set; }

    /// <summary>
    /// 除 destRootPath 以外的存储位置
    /// </summary>
    public string FilePath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the file path relative to the configured storage root.
    /// </summary>
    public string RelativeFilePath { get; set; } = string.Empty;
}
