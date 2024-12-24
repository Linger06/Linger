namespace Linger.UnitTests.Extensions.Core;

public class IntExtensionsTests
{
    [Fact]
    public void FileSize_ReturnsCorrectSizeForBytes()
    {
        var length = 512;
        var result = length.FileSize();
        Assert.Equal("512Bytes", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForKilobytes()
    {
        var length = 2048;
        var result = length.FileSize();
        Assert.Equal("2KB", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForMegabytes()
    {
        var length = 1048576;
        var result = length.FileSize();
        Assert.Equal("1MB", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForGigabytes()
    {
        var length = 1073741824;
        var result = length.FileSize();
        Assert.Equal("1GB", result);
    }

    [Fact]
    public void ToThousand_FormatsIntWithThousandSeparators()
    {
        var value = 1234567;
        var result = value.ToThousand();
        Assert.Equal("1,234,567.00", result);
    }

    [Fact]
    public void ToThousand_FormatsNegativeIntWithThousandSeparators()
    {
        var value = -1234567;
        var result = value.ToThousand();
        Assert.Equal("-1,234,567.00", result);
    }
}