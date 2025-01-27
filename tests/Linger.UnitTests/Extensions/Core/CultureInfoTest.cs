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

        /// <summary>
        /// 验证是否受系统区域设置影响
        /// </summary>
        [Fact]
        public void TestSystemLocaleImpact()
        {
                // 记录初始系统设置
                _output.WriteLine("=== Initial System Settings ===");
                _output.WriteLine($"System Default Culture: {CultureInfo.CurrentCulture.Name}");
                _output.WriteLine($"System UI Culture: {CultureInfo.CurrentUICulture.Name}");

                // 测试zh-CN文化信息
                var zhCN = new CultureInfo("zh-CN", false);
                _output.WriteLine("\n=== zh-CN Culture Settings ===");
                _output.WriteLine($"FirstDayOfWeek: {zhCN.DateTimeFormat.FirstDayOfWeek}");
                _output.WriteLine($"CalendarWeekRule: {zhCN.DateTimeFormat.CalendarWeekRule}");

                // 临时更改当前线程的文化信息
                var originalCulture = CultureInfo.CurrentCulture;
                try
                {
                        // 设置为不同的系统区域
                        CultureInfo.CurrentCulture = new CultureInfo("en-US");
                        var zhCN2 = new CultureInfo("zh-CN", false);

                        _output.WriteLine("\n=== After System Culture Change ===");
                        _output.WriteLine($"Current System Culture: {CultureInfo.CurrentCulture.Name}");
                        _output.WriteLine($"zh-CN FirstDayOfWeek: {zhCN2.DateTimeFormat.FirstDayOfWeek}");
                        _output.WriteLine($"zh-CN CalendarWeekRule: {zhCN2.DateTimeFormat.CalendarWeekRule}");

                        // 比较改变前后的值
                        Assert.Equal(zhCN.DateTimeFormat.FirstDayOfWeek, zhCN2.DateTimeFormat.FirstDayOfWeek);
                        Assert.Equal(zhCN.DateTimeFormat.CalendarWeekRule, zhCN2.DateTimeFormat.CalendarWeekRule);
                }
                finally
                {
                        // 恢复原始设置
                        CultureInfo.CurrentCulture = originalCulture;
                }
        }
}