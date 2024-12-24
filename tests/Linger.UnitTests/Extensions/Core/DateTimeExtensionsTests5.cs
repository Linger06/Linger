namespace Linger.Extensions.Core.Tests;

public partial class DateTimeExtensionsTests
{
    [Fact]
    public void SetDay_ShouldReturnCorrectDate()
    {
        var date = new DateTime(2023, 10, 10);
        DateTime result = date.SetDay(5);
        Assert.Equal(new DateTime(2023, 10, 5), result);
    }

    [Fact]
    public void SetMonth_ShouldReturnCorrectDate()
    {
        var date = new DateTime(2023, 10, 10);
        DateTime result = date.SetMonth(5);
        Assert.Equal(new DateTime(2023, 5, 10), result);
    }

    [Fact]
    public void SetYear_ShouldReturnCorrectDate()
    {
        var date = new DateTime(2023, 10, 10);
        DateTime result = date.SetYear(2025);
        Assert.Equal(new DateTime(2025, 10, 10), result);
    }

    [Fact]
    public void StartOfMonth_ShouldReturnCorrectDate()
    {
        var date = new DateTime(2023, 10, 10);
        DateTime result = date.StartOfMonth();
        Assert.Equal(new DateTime(2023, 10, 1), result);
    }

    [Fact]
    public void StartOfDay_ShouldReturnCorrectDate()
    {
        var date = new DateTime(2023, 10, 10, 15, 30, 45);
        DateTime result = date.StartOfDay();
        Assert.Equal(new DateTime(2023, 10, 10), result);
    }

    [Fact]
    public void EndOfYear_ShouldReturnCorrectDate()
    {
        var date = new DateTime(2023, 10, 10);
        DateTime result = date.EndOfYear();
        Assert.Equal(new DateTime(2023, 12, 31, 23, 59, 59, 999), result);
    }
}