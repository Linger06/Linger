namespace Linger.UnitTests.Extensions.Core;

using Xunit;

public class DoubleExtensionsTests
{
    [Fact]
    public void FileSize_ReturnsCorrectSizeForBytes()
    {
        double length = 512;
        var result = length.FormatFileSize();
        Assert.Equal("512Bytes", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForKilobytes()
    {
        double length = 2048;
        var result = length.FormatFileSize();
        Assert.Equal("2KB", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForMegabytes()
    {
        double length = 1048576;
        var result = length.FormatFileSize();
        Assert.Equal("1MB", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForGigabytes()
    {
        double length = 1073741824;
        var result = length.FormatFileSize();
        Assert.Equal("1GB", result);
    }

    // 先取整再判断进位：1023.6KB 取整后已是 1024，应显示 1MB 而不是 "1024KB"
    [Theory]
    [InlineData(1023.6d, "1KB")]
    [InlineData(1048166d, "1MB")]
    [InlineData(1073741823d, "1GB")]
    [InlineData(1023d, "1023Bytes")]
    [InlineData(2305843009213693952d, "2048PB")]
    public void FileSize_PromotesUnitWhenRoundedValueReachesNextUnit(double length, string expected)
    {
        var result = length.FormatFileSize();
        Assert.Equal(expected, result);
    }

    // 默认的银行家舍入会把 2.5KB 显示成 "2KB"，这里与 DecimalExtensions.Round 保持一致，.5 一律进位
    [Theory]
    [InlineData(1536d, "2KB")]
    [InlineData(2560d, "3KB")]
    [InlineData(3584d, "4KB")]
    [InlineData(1535d, "1KB")]
    public void FileSize_RoundsHalfAwayFromZero(double length, string expected)
    {
        var result = length.FormatFileSize();
        Assert.Equal(expected, result);
    }
}