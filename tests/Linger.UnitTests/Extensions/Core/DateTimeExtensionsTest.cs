namespace Linger.UnitTests.Extensions.Core;

public class DateTimeExtensionsTest
{
    [Fact]
    public void EndOfDay()
    {
        // Type
        DateTime @this = DateTime.Now;

        // Examples
        DateTime value = @this.EndOfDay(); // value = "yyyy/MM/dd 23:59:59:999";

        // Unit Test
        Assert.Equal(new DateTime(value.Year, value.Month, value.Day, 23, 59, 59, 999), value);
    }

    [Fact]
    public void EndOfMonth()
    {
        // Type
        var @this = new DateTime(2013, 12, 22);

        // Examples
        DateTime value = @this.EndOfMonth();

        // Unit Test
        Assert.Equal(new DateTime(2013, 12, 31, 23, 59, 59, 999), value);
    }

    [Fact]
    public void EndOfYear()
    {
        // Type
        var @this = new DateTime(2013, 04, 13);

        // Examples
        DateTime value = @this.EndOfYear(); // value = "2013/12/31 23:59:59:999";

        // Unit Test
        Assert.Equal(new DateTime(2013, 12, 31, 23, 59, 59, 999), value);
    }

    [Fact]
    public void IsDateEqual()
    {
        // Type
        var thisMorning = new DateTime(2014, 04, 12, 8, 0, 0);
        var thisAfternoon = new DateTime(2014, 04, 12, 17, 0, 0);

        // Exemples
        var result = thisMorning.IsDateEqual(thisAfternoon); // return true;

        // Unit Test
        Assert.True(result);
    }

    [Fact]
    public void IsTimeEqual()
    {
        // Type
        DateTime thisToday = DateTime.Today;
        DateTime thisYesterday = thisToday.AddDays(-1);

        // Exemples
        var result = thisYesterday.IsTimeEqual(thisToday); // return true;

        // Unit Test
        Assert.True(result);
    }

    [Fact]
    public void IsToday()
    {
        // Type
        DateTime thisToday = DateTime.Today;
        DateTime thisYesterday = thisToday.AddDays(-1);

        // Exemples
        var result1 = thisToday.IsToday(); // return true;
        var result2 = thisYesterday.IsToday(); // return false;

        // Unit Test
        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    public void SetTime()
    {
        // Type
        DateTime thisToday = DateTime.Today;

        // Exemples
        DateTime result = thisToday.SetTime(15); // Set hours to 15

        // Unit Test
        Assert.Equal(15, result.Hour);
    }

    [Fact]
    public void StartOfDay()
    {
        // Type
        DateTime @this = DateTime.Now;

        // Examples
        DateTime value = @this.StartOfDay(); // value = "yyyy/MM/dd 00:00:00:000";

        // Unit Test
        Assert.Equal(new DateTime(value.Year, value.Month, value.Day), value);
    }

    [Fact]
    public void StartOfMonth()
    {
        // Type
        DateTime @this = DateTime.Now;

        // Examples
        DateTime value = @this.StartOfMonth(); // value = "yyyy/MM/01 00:00:00:000";

        // Unit Test
        Assert.Equal(new DateTime(value.Year, value.Month, 1), value);
    }

    [Fact]
    public void DateDiff2Test()
    {
        DateTime value1 = DateTime.Now;
        DateTime value2 = value1.AddDays(2);

        TimeSpan diff = value1.GetDateDifference(value2);
        Assert.Equal(diff.TotalDays, -2.ToDouble());

        TimeSpan diff2 = value2.GetDateDifference(value1);
        Assert.Equal(diff2.TotalDays, 2.ToDouble());
    }
}