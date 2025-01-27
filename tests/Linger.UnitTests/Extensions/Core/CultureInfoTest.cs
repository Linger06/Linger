using System.Globalization;
public class CultureInfoTest
{
    [Fact]
    public void TestZhCNCulture()
    {
        var zhCN = new CultureInfo("zh-CN", false);
#if NETFRAMEWORK
        // Framework使用ISO 8601标准
        Assert.Equal(DayOfWeek.Monday, zhCN.DateTimeFormat.FirstDayOfWeek);
        Assert.Equal(CalendarWeekRule.FirstFourDayWeek, zhCN.DateTimeFormat.CalendarWeekRule);
#else
        // Core使用简化的日历周计算
        Assert.Equal(DayOfWeek.Sunday, zhCN.DateTimeFormat.FirstDayOfWeek);
        Assert.Equal(CalendarWeekRule.FirstDay, zhCN.DateTimeFormat.CalendarWeekRule);
#endif
        
    }
}