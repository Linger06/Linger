using System.Security.Cryptography;
using Linger.Extensions.Core;

namespace Linger.Extensions.IO;

/// <summary>
/// <see cref="Stream"/> extensions.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Computes the MD5 hash of the stream and returns a lowercase hex string.
    /// Resets the position for seekable streams before hashing.
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

    [Obsolete("This wrapper is obsolete. Prefer stream.CopyTo with an explicitly configured FileStream.")]
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

    [Obsolete("This wrapper is obsolete. Prefer stream.CopyToAsync with an explicitly configured FileStream.")]
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
