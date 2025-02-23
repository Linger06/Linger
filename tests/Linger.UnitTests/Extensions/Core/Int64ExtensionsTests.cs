namespace Linger.UnitTests.Extensions.Core;

public class Int64ExtensionsTests
{
    [Fact]
    public void FileSize_ReturnsCorrectSizeForBytes()
    {
        long length = 512;
        var result = length.FormatFileSize();
        Assert.Equal("512Bytes", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForKilobytes()
    {
        long length = 2048;
        var result = length.FormatFileSize();
        Assert.Equal("2KB", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForMegabytes()
    {
        long length = 1048576;
        var result = length.FormatFileSize();
        Assert.Equal("1MB", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForGigabytes()
    {
        long length = 1073741824;
        var result = length.FormatFileSize();
        Assert.Equal("1GB", result);
    }
}