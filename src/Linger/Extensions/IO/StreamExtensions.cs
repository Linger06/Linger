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
}
