namespace Linger.UnitTests.Extensions.Core;

public class IntExtensionsTests
{
    [Fact]
    public void FileSize_ReturnsCorrectSizeForBytes()
    {
        var length = 512;
        var result = length.FormatFileSize();
        Assert.Equal("512Bytes", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForKilobytes()
    {
        var length = 2048;
        var result = length.FormatFileSize();
        Assert.Equal("2KB", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForMegabytes()
    {
        var length = 1048576;
        var result = length.FormatFileSize();
        Assert.Equal("1MB", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForGigabytes()
    {
        var length = 1073741824;
        var result = length.FormatFileSize();
        Assert.Equal("1GB", result);
    }
}