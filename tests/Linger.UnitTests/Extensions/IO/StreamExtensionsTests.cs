namespace Linger.UnitTests.Extensions.IO;

public class StreamExtensionsTests
{
    [Fact]
    public void ToMd5Hash_ReturnsCorrectHash()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!"));
        var result = stream.ToMd5Hash();
        Assert.Equal("65A8E27D8879283831B664BD8B7F0AD4", result);
    }

    [Fact]
    public void ToMd5HashByte_ReturnsCorrectHashBytes()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!"));
        var result = stream.ToMd5HashByte();
        Assert.Equal(new byte[] { 0x65, 0xA8, 0xE2, 0x7D, 0x88, 0x79, 0x28, 0x38, 0x31, 0xB6, 0x64, 0xBD, 0x8B, 0x7F, 0x0A, 0xD4 }, result);
    }

#if NET6_0_OR_GREATER

    [Fact]
    public async Task ToMd5HashByteAsync_ReturnsCorrectHashBytes()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!"));
        var result = await stream.ToMd5HashByteAsync();
        Assert.Equal(new byte[] { 0x65, 0xA8, 0xE2, 0x7D, 0x88, 0x79, 0x28, 0x38, 0x31, 0xB6, 0x64, 0xBD, 0x8B, 0x7F, 0x0A, 0xD4 }, result);
    }

#endif

    [Fact]
    public void ComputeHashMd5_ReturnsCorrectHash()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!"));
        var result = stream.ComputeHashMd5();
        Assert.Equal("65A8E27D8879283831B664BD8B7F0AD4", result);
    }

    [Fact]
    public void ToFile_WritesStreamToFile()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!"));
        var filePath = "test.txt";
        stream.ToFile(filePath);
        Assert.True(File.Exists(filePath));
        Assert.Equal("Hello, World!", File.ReadAllText(filePath));
        File.Delete(filePath);
    }
}