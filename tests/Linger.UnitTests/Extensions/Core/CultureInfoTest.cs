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
        _output.WriteLine($"Framework Description: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
        _output.WriteLine($"OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
        _output.WriteLine($"Culture: {zhCN.Name}");
        _output.WriteLine($"FirstDayOfWeek: {zhCN.DateTimeFormat.FirstDayOfWeek}");
        _output.WriteLine($"CalendarWeekRule: {zhCN.DateTimeFormat.CalendarWeekRule}");
        _output.WriteLine($"Is ReadOnly: {zhCN.DateTimeFormat.IsReadOnly}");
        
        // 验证是否受系统区域设置影响
        var currentCulture = CultureInfo.CurrentCulture;
        _output.WriteLine($"Current Culture: {currentCulture.Name}");
        _output.WriteLine($"Current FirstDayOfWeek: {currentCulture.DateTimeFormat.FirstDayOfWeek}");
        _output.WriteLine($"Current CalendarWeekRule: {currentCulture.DateTimeFormat.CalendarWeekRule}");
        
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