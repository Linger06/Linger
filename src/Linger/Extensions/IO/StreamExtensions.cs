using System.Security.Cryptography;
using Linger.Extensions.Core;

namespace Linger.Extensions.IO;

/// <summary>
/// <see cref="Stream"/> extensions.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// 计算流的 MD5 哈希并返回小写十六进制字符串。
    /// 修复了网络流、请求流下的 Seek 崩溃 Bug（高工程价值，继续保留）。
    /// </summary>
    public static string ComputeHashMd5(this Stream inputStream)
    {
        ArgumentNullException.ThrowIfNull(inputStream);

        if (inputStream.CanSeek)
        {
            inputStream.Position = 0;
        }

        var arrayHashValue = inputStream.ToMd5HashByte();
        return arrayHashValue.ToMd5HashCode();
    }

    public static byte[] ToMd5HashByte(this Stream inputStream)
    {
        ArgumentNullException.ThrowIfNull(inputStream);
#if NET6_0_OR_GREATER
        return MD5.HashData(inputStream);
#else
        using var md5 = MD5.Create();
        return md5.ComputeHash(inputStream);
#endif
    }

#if NET6_0_OR_GREATER
    public static async Task<byte[]> ToMd5HashByteAsync(this Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return await MD5.HashDataAsync(stream).ConfigureAwait(false);
    }
#endif

    [Obsolete("此方法属于过度封装。建议直接在业务层使用原生的 'stream.CopyTo' 写入文件流，以便更加灵活地控制 FileShare 隔离级别和缓冲区大小。")]
    public static void ToFile(this Stream stream, string filePath)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

        if (stream.CanSeek) stream.Position = 0;
        stream.CopyTo(fs);
    }

    [Obsolete("此方法属于过度封装。建议直接在业务层使用原生的 'await stream.CopyToAsync' 写入文件流，避免锁死底层文件共享状态。")]
    public static async Task ToFileAsync(this Stream stream, string filePath)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

        if (stream.CanSeek) stream.Position = 0;
        await stream.CopyToAsync(fs).ConfigureAwait(false);
    }
}

/// <summary>
/// Represents extended metadata about an existing file.
/// </summary>
public class ExtendedFileInfo : BaseFileInfo
{
    /// <summary>
    /// Gets or sets the relative file path.
    /// </summary>
    public string RelativeFilePath { get; set; } = null!;
}

/// <summary>
/// Represents base information about a file.
/// </summary>
public class BaseFileInfo
{
    /// <summary>
    /// Gets or sets the hash data of the file.
    /// </summary>
    public string HashData { get; set; } = null!;

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the fully qualified path and name of the file.
    /// </summary>
    public string FullFilePath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the file size.
    /// </summary>
    public string FileSize { get; set; } = null!;

    /// <summary>
    /// Gets or sets the length of the file.
    /// </summary>
    public long Length { get; set; }
}
