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
    /// <summary>
    /// Computes the MD5 hash of a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to hash.</param>
    /// <returns>The computed hash.</returns>
    public static Task<byte[]> ToMd5HashByteAsync(this Stream stream)
    {
        return stream.ToMd5HashByteAsync(CancellationToken.None);
    }

    /// <summary>
    /// Computes the MD5 hash of a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to hash.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The computed hash.</returns>
    public static async Task<byte[]> ToMd5HashByteAsync(this Stream stream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return await MD5.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
    }
#endif

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

    /// <summary>
    /// Writes the stream to a file asynchronously.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <param name="filePath">The destination file path.</param>
    /// <returns>A task representing the write operation.</returns>
    public static Task ToFileAsync(this Stream stream, string filePath)
    {
        return stream.ToFileAsync(filePath, CancellationToken.None);
    }

    /// <summary>
    /// Writes the stream to a file asynchronously.
    /// </summary>
    /// <param name="stream">The source stream.</param>
    /// <param name="filePath">The destination file path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the write operation.</returns>
    public static async Task ToFileAsync(this Stream stream, string filePath, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        cancellationToken.ThrowIfCancellationRequested();

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        using var fs = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            4096,
            useAsync: true);

        if (stream.CanSeek) stream.Position = 0;
        await stream.CopyToAsync(fs, 81920, cancellationToken).ConfigureAwait(false);
    }
}
