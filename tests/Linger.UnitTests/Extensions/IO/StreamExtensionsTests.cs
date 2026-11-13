using System.Security.Cryptography;
using System.Text;
using Linger.Extensions.IO;
using Linger.Helper;

namespace Linger.UnitTests.Extensions.IO;

public class StreamExtensionsTests
{
    [Fact]
    public void ToMd5HashByte_ShouldCalculateCorrectHash()
    {
        string testData = "Test data for MD5 calculation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        using var md5 = MD5.Create();
        byte[] expectedHash = md5.ComputeHash(testBytes);

        memoryStream.Position = 0;
        byte[] actualHash = memoryStream.ToMd5HashByte();

        Assert.Equal(expectedHash, actualHash);
    }

    [Fact]
    public void ComputeHashMd5_ShouldCalculateCorrectHash()
    {
        string testData = "Test data for ComputeHashMd5";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        using var md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(testBytes);
        string expectedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        memoryStream.Position = 0;
        string actualHash = memoryStream.ComputeHashMd5();

        Assert.Equal(expectedHash, actualHash);
    }

#if NET6_0_OR_GREATER
    [Fact]
    public async Task ToMd5HashByteAsync_ShouldCalculateCorrectHash()
    {
        string testData = "Test data for MD5 async calculation";
        byte[] testBytes = Encoding.UTF8.GetBytes(testData);
        using var memoryStream = new MemoryStream(testBytes);

        using var md5 = MD5.Create();
        byte[] expectedHash = md5.ComputeHash(testBytes);

        memoryStream.Position = 0;
        byte[] actualHash = await memoryStream.ToMd5HashByteAsync();

        Assert.Equal(expectedHash, actualHash);
    }

    [Fact]
    public async Task ToMd5HashByteAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("cancel"));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => memoryStream.ToMd5HashByteAsync(cts.Token));
    }
#endif
}
