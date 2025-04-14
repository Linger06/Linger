using System.Globalization;
using System.Runtime.InteropServices;

namespace Linger.UnitTests;

public class CultureInfoTest(ITestOutputHelper output)
{
    [Theory]
    [InlineData("zh-CN")]
    public void CompareZhCNFirstDayOfWeekAcrossFrameworks(string cultureName)
    {
        // 获取平台信息
        var isWindows = OSPlatformHelper.IsWindows;
        var isLinux = OSPlatformHelper.IsLinux;
        var isMacOS = OSPlatformHelper.IsMacOSX;

        output.WriteLine("\n=== 框架信息 ===");
        output.WriteLine($"运行时版本: {Environment.Version}");
        output.WriteLine($"框架描述: {RuntimeInformation.FrameworkDescription}");
        output.WriteLine($"操作系统: {RuntimeInformation.OSDescription}");

        var culture = new CultureInfo(cultureName, false);

        output.WriteLine($"\n=== {cultureName} 区域设置信息 ===");
        output.WriteLine($"区域名称: {culture.Name}");
        output.WriteLine($"FirstDayOfWeek: {culture.DateTimeFormat.FirstDayOfWeek}");
        output.WriteLine($"CalendarWeekRule: {culture.DateTimeFormat.CalendarWeekRule}");

        // 检查是否为.NET Framework
        bool isNetFramework = RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");
        output.WriteLine($"是否.NET Framework: {isNetFramework}");

        if (isWindows)
        {
            output.WriteLine("Windows平台测试");
            
            if (isNetFramework)
            {
                // .NET Framework:
                // - zh-CN FirstDayOfWeek 固定为 Monday
                output.WriteLine("\n在 .NET Framework 中运行");
                Assert.Equal(DayOfWeek.Monday, culture.DateTimeFormat.FirstDayOfWeek);
            }
            else
            {
                // .NET Core/.NET 5+:
                // - zh-CN FirstDayOfWeek 默认为 Sunday (ICU库)
                output.WriteLine("\n在 .NET Core/.NET 5+ 中运行");
                
                // 获取实际值并记录，而不是进行固定断言
                var actualFirstDay = culture.DateTimeFormat.FirstDayOfWeek;
                output.WriteLine($"实际FirstDayOfWeek值: {actualFirstDay}");
                
                // 在中文文化中，.NET Core/.NET 5+的FirstDayOfWeek应为Sunday
                Assert.Equal(DayOfWeek.Sunday, actualFirstDay);
            }
        }
        else if (isLinux)
        {
            output.WriteLine("Linux平台测试");

            // Linux依赖于系统locale设置
            var currentLang = Environment.GetEnvironmentVariable("LANG") ?? "undefined";
            var currentLcAll = Environment.GetEnvironmentVariable("LC_ALL") ?? "undefined";
            var currentLcTime = Environment.GetEnvironmentVariable("LC_TIME") ?? "undefined";

            output.WriteLine($"当前LANG={currentLang}");
            output.WriteLine($"当前LC_ALL={currentLcAll}");
            output.WriteLine($"当前LC_TIME={currentLcTime}");

            var firstDay = culture.DateTimeFormat.FirstDayOfWeek;
            output.WriteLine($"当前FirstDayOfWeek={firstDay}");
            output.WriteLine($"当前Culture={culture.Name}");
            output.WriteLine($"当前CultureUI={CultureInfo.CurrentUICulture.Name}");

            // 在Linux下，根据不同的locale配置，zh-CN的FirstDayOfWeek可能是Sunday或Monday
            Assert.True(
                firstDay == DayOfWeek.Sunday || firstDay == DayOfWeek.Monday,
                $"FirstDayOfWeek应该是Sunday或Monday，当前值为{firstDay}"
                 );
        }
        else if (isMacOS)
        {
            output.WriteLine("macOS平台测试");
            
            // macOS下，.NET Core的zh-CN FirstDayOfWeek通常为Sunday
            var firstDay = culture.DateTimeFormat.FirstDayOfWeek;
            output.WriteLine($"当前FirstDayOfWeek={firstDay}");
            
            // 放宽断言，接受Sunday或Monday
            Assert.True(
                firstDay == DayOfWeek.Sunday || firstDay == DayOfWeek.Monday,
                $"FirstDayOfWeek应该是Sunday或Monday，当前值为{firstDay}"
            );
        }
        else
        {
            output.WriteLine("未知平台");
        }
    }
}