namespace Linger.Extensions.Tests;

public partial class DateTimeExtensionsTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void MsSqlDateTimeInitial_ShouldReturnCorrectValue()
    {
        var expected = new DateTime(1900, 1, 1, 0, 0, 0, 0);
        Assert.Equal(expected, DateTimeExtensions.MsSqlDateTimeInitial);
    }

    [Fact]
    public void MsSqlDateTimeMinValue_ShouldReturnCorrectValue()
    {
        var expected = new DateTime(1900, 1, 1, 0, 0, 0, 0);
        Assert.Equal(expected, DateTimeExtensions.MsSqlDateTimeMinValue);
    }

    [Fact]
    public void MsSqlDateTimeMaxValue_ShouldReturnCorrectValue()
    {
        Assert.Equal(DateTime.MaxValue, DateTimeExtensions.MsSqlDateTimeMaxValue);
    }

    [Fact]
    public void MySqlDateTimeInitial_ShouldReturnCorrectValue()
    {
        var expected = new DateTime(1753, 1, 1, 0, 0, 0, 0);
        Assert.Equal(expected, DateTimeExtensions.MySqlDateTimeInitial);
    }

    [Fact]
    public void MySqlDateTimeMinValue_ShouldReturnCorrectValue()
    {
        var expected = new DateTime(1753, 1, 1, 0, 0, 0, 0);
        Assert.Equal(expected, DateTimeExtensions.MySqlDateTimeMinValue);
    }

    [Fact]
    public void MySqlDateTimeMaxValue_ShouldReturnCorrectValue()
    {
        Assert.Equal(DateTime.MaxValue, DateTimeExtensions.MySqlDateTimeMaxValue);
    }

    [Fact]
    public void UtcOffset_ShouldReturnCorrectValue()
    {
        var expected = DateTime.Now.Subtract(DateTime.UtcNow).TotalHours;
        Assert.Equal(expected, DateTimeExtensions.UtcOffset, 1);
    }

    [Fact]
    public void ToFormatDate_ShouldReturnFormattedDate()
    {
        var date = new DateTime(2023, 10, 5);
        var formattedDate = date.ToFormatDate();
        Assert.Equal("2023-10-05", formattedDate);
    }

    [Fact]
    public void ToFormatDate_Nullable_ShouldReturnFormattedDate()
    {
        DateTime? date = null;
        var formattedDate = date.ToFormatDate();
        Assert.Null(formattedDate);
    }

    [Fact]
    public void ToFormatDateTime_ShouldReturnFormattedDateTime()
    {
        var date = new DateTime(2023, 10, 5, 14, 30, 0);
        var formattedDateTime = date.ToFormatDateTime();
        Assert.Equal("2023-10-05 14:30:00", formattedDateTime);
    }

    [Fact]
    public void ToFormatDateTime_Nullable_ShouldReturnFormattedDateTime()
    {
        DateTime? date = null;
        var formattedDateTime = date.ToFormatDateTime();
        Assert.Null(formattedDateTime);
    }

    [Fact]
    public void DateDiff2_ShouldReturnCorrectTimeSpan()
    {
        var date1 = new DateTime(2023, 1, 1);
        var date2 = new DateTime(2023, 1, 10);
        TimeSpan diff = date1.DateDiff2(date2);
        Assert.Equal(TimeSpan.FromDays(-9), diff);
    }

    [Fact]
    public void DateDiff3_ShouldReturnCorrectDifferenceInDays()
    {
        DateTime date1 = DateTime.Now.AddDays(3);
        DateTime? date2 = null;
        var diff = date1.DateDiff3(date2, "D", true);
        testOutputHelper.WriteLine(diff.ToString());
        Assert.True(diff <= 3 && diff > 2.9);
    }

    [Fact]
    public void ToStringOfMode_InvalidMode_ShouldReturnDefaultFormattedString()
    {
        var date = new DateTime(2023, 10, 5, 14, 30, 0);
        var formattedDate = date.ToStringOfMode(99);
        Assert.Equal(date.ToString(ExtensionMethodSetting.DefaultCulture), formattedDate);
    }

    [Theory]
    [InlineData("D", 3)]
    [InlineData("H", 72)]
    [InlineData("M", 4320)]
    [InlineData("S", 259200)]
    [InlineData("MS", 259200000)]
    public void DateDiff3_ShouldReturnCorrectDifference(string type, double expected)
    {
        var date1 = new DateTime(2023, 10, 1);
        var date2 = new DateTime(2023, 10, 4);
        Assert.Equal(expected, date1.DateDiff3(date2, type));
    }

    [Theory]
    [InlineData(0, "2023-10-01")]
    [InlineData(1, "2023-10-01 00:00:00")]
    [InlineData(2, "2023/10/01")]
    [InlineData(4, "10-01")]
    [InlineData(5, "10/01")]
    [InlineData(7, "2023-10")]
    [InlineData(8, "2023/10")]
    [InlineData(10, "2023-10-01 00:00:00")]
    [InlineData(11, "20231001")]
    [InlineData(12, "20231001000000")]
    [InlineData(13, "10/01/2023")]
    public void ToStringOfMode_ShouldReturnFormattedString(int mode, string expected)
    {
        var date = new DateTime(2023, 10, 1);
        Assert.Equal(expected, date.ToStringOfMode(mode));
    }
}