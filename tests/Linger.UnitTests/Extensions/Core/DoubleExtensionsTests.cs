namespace Linger.UnitTests.Extensions.Core;

using Xunit;

public class DoubleExtensionsTests
{
    [Fact]
    public void FileSize_ReturnsCorrectSizeForBytes()
    {
        double length = 512;
        var result = length.FileSize();
        Assert.Equal("512Bytes", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForKilobytes()
    {
        double length = 2048;
        var result = length.FileSize();
        Assert.Equal("2KB", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForMegabytes()
    {
        double length = 1048576;
        var result = length.FileSize();
        Assert.Equal("1MB", result);
    }

    [Fact]
    public void FileSize_ReturnsCorrectSizeForGigabytes()
    {
        double length = 1073741824;
        var result = length.FileSize();
        Assert.Equal("1GB", result);
    }

    [Fact]
    public void Days_ReturnsCorrectTimeSpan()
    {
        var days = 2.5;
        TimeSpan result = days.Days();
        Assert.Equal(TimeSpan.FromDays(2.5), result);
    }

    [Fact]
    public void Hours_ReturnsCorrectTimeSpan()
    {
        var hours = 5.5;
        TimeSpan result = hours.Hours();
        Assert.Equal(TimeSpan.FromHours(5.5), result);
    }

    [Fact]
    public void Milliseconds_ReturnsCorrectTimeSpan()
    {
        double milliseconds = 1500;
        TimeSpan result = milliseconds.Milliseconds();
        Assert.Equal(TimeSpan.FromMilliseconds(1500), result);
    }

    [Fact]
    public void Minutes_ReturnsCorrectTimeSpan()
    {
        var minutes = 45.5;
        TimeSpan result = minutes.Minutes();
        Assert.Equal(TimeSpan.FromMinutes(45.5), result);
    }

    [Fact]
    public void Seconds_ReturnsCorrectTimeSpan()
    {
        var seconds = 90.5;
        TimeSpan result = seconds.Seconds();
        Assert.Equal(TimeSpan.FromSeconds(90.5), result);
    }
}