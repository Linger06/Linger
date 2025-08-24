using System.Security.Cryptography;
using System.Text;
using Linger.Extensions.Core;
using Linger.Helper;
#if NET6_0_OR_GREATER
#endif

namespace Linger.Extensions.IO;

/// <summary>
/// <see cref="Stream"/> extensions.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Returns the MD5 hash of the current Stream object as a string.
    /// </summary>
    /// <param name="inputStream">The input stream.</param>
    /// <returns>The MD5 hash as a string.</returns>
    /// <example>
    /// <code>
    /// using (var stream = File.OpenRead("example.txt"))
    /// {
    ///     string hash = stream.ToMd5Hash();
    ///     Console.WriteLine(hash);
    /// }
    /// </code>
    /// </example>
    [Obsolete("This method is obsolete, use ComputeHashMd5 instead. Will be removed in 1.0.0.")]
    public static string ToMd5Hash(this Stream inputStream)
    {
        _ = inputStream.Seek(0, SeekOrigin.Begin);
        var hashBytes = inputStream.ToMd5HashByte();
        var sb = new StringBuilder();
        foreach (var bytes in hashBytes)
        {
            _ = sb.Append(bytes.ToString("X2", ExtensionMethodSetting.DefaultCulture));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns the MD5 hash of the current Stream object as a byte array.
    /// </summary>
    /// <param name="inputStream">The input stream.</param>
    /// <returns>The MD5 hash as a byte array.</returns>
    /// <example>
    /// <code>
    /// using (var stream = File.OpenRead("example.txt"))
    /// {
    ///     byte[] hashBytes = stream.ToMd5HashByte();
    ///     Console.WriteLine(BitConverter.ToString(hashBytes));
    /// }
    /// </code>
    /// </example>
    public static byte[] ToMd5HashByte(this Stream inputStream)
    {
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(inputStream);
        return hashBytes;
    }

#if NET6_0_OR_GREATER

    /// <summary>
    /// Asynchronously returns the MD5 hash of the current Stream object as a byte array.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the MD5 hash as a byte array.</returns>
    /// <example>
    /// <code>
    /// using (var stream = File.OpenRead("example.txt"))
    /// {
    ///     byte[] hashBytes = await stream.ToMd5HashByteAsync();
    ///     Console.WriteLine(BitConverter.ToString(hashBytes));
    /// }
    /// </code>
    /// </example>
    public static async Task<byte[]> ToMd5HashByteAsync(this Stream stream)
    {
        using var md5 = MD5.Create();
        var hashBytes = await md5.ComputeHashAsync(stream).ConfigureAwait(false);
        return hashBytes;
    }

#endif

    /// <summary>
    /// Computes the MD5 hash of the current Stream object and returns it as a string.
    /// </summary>
    /// <param name="inputStream">The input stream.</param>
    /// <returns>The MD5 hash as a string.</returns>
    /// <example>
    /// <code>
    /// using (var stream = File.OpenRead("example.txt"))
    /// {
    ///     string hash = stream.ComputeHashMd5();
    ///     Console.WriteLine(hash);
    /// }
    /// </code>
    /// </example>
    public static string ComputeHashMd5(this Stream inputStream)
    {
        _ = inputStream.Seek(0, SeekOrigin.Begin);
        var arrayHashValue = inputStream.ToMd5HashByte();
        return arrayHashValue.ToMd5HashCode();
    }

    /// <summary>
    /// 将流写入到文件
    /// </summary>
    /// <param name="stream">要写入的流</param>
    /// <param name="filePath">文件路径</param>
    /// <example>
    /// <code>
    /// using (var stream = File.OpenRead("example.txt"))
    /// {
    ///     stream.ToFile("output.txt");
    /// }
    /// </code>
    /// </example>
    public static void ToFile(this Stream stream, string filePath)
    {
        FileHelper.EnsureDirectoryExists(filePath);

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);

        // 对于可查找的流，尝试将位置重置到开始
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        // 使用CopyTo方法进行流复制，适用于所有类型的流
        stream.CopyTo(fs);
        fs.Flush();
    }

    /// <summary>
    /// 异步将流写入到文件
    /// </summary>
    /// <param name="stream">要写入的流</param>
    /// <param name="filePath">文件路径</param>
    /// <returns>表示异步操作的任务</returns>
    /// <example>
    /// <code>
    /// using (var stream = File.OpenRead("example.txt"))
    /// {
    ///     await stream.ToFileAsync("output.txt");
    /// }
    /// </code>
    /// </example>
    public static async Task ToFileAsync(this Stream stream, string filePath)
    {
        FileHelper.EnsureDirectoryExists(filePath);

        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);

        // 对于可查找的流，尝试将位置重置到开始
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        // 使用CopyToAsync方法进行异步流复制
        await stream.CopyToAsync(fs).ConfigureAwait(false);
        await fs.FlushAsync().ConfigureAwait(false);
    }
}

/// <summary>
/// Represents information about an existing file.
/// </summary>
public class CustomExistFileInfo : BaseFileInfo
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
