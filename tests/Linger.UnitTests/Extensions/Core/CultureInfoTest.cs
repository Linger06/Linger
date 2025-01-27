using System.Globalization;
public class CultureInfoTest
{

   private readonly ITestOutputHelper _output;

    public CultureInfoTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestZhCNCulture()
    {
        var zhCN = new CultureInfo("zh-CN", false);

        _output.WriteLine($"Framework: {Environment.Version}");
        _output.WriteLine($"Is .NET Core: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.Contains(".NET Core")}");
        _output.WriteLine($"FirstDayOfWeek: {zhCN.DateTimeFormat.FirstDayOfWeek}");
        _output.WriteLine($"CalendarWeekRule: {zhCN.DateTimeFormat.CalendarWeekRule}");
    
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