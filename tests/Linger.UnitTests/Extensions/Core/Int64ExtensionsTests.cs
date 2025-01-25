namespace Linger.UnitTests.Extensions.Core;

public class Int64ExtensionsTests
{
    [Fact]
    public void ToDateTime_ReturnsCorrectDateTimeForUnixTimestamp()
    {
        var timeStamp = 1625097600000; // Unix timestamp for 2021-07-01 00:00:00 UTC
        var result = timeStamp.ToDateTime();
        Assert.Equal(new DateTime(2021, 7, 1, 8, 0, 0, DateTimeKind.Utc), result);
    }

    [Fact]
    public void ToDateTime_ReturnsEpochForZeroTimestamp()
    {
        long timeStamp = 0;
        var result = timeStamp.ToDateTime();
        Assert.Equal(new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Utc), result);
    }

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