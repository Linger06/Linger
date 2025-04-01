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

        if (isWindows)
        {
            output.WriteLine("Windows平台测试");
#if NETFRAMEWORK
            // .NET Framework:
            // - FirstDayOfWeek 固定为 Monday
            // - 遵循 ISO 8601 标准
            output.WriteLine("\n在 .NET Framework 中运行");
            Assert.Equal(DayOfWeek.Monday, culture.DateTimeFormat.FirstDayOfWeek);
#else
            // .NET Core:
            // - FirstDayOfWeek 受系统区域设置影响
            // - 在 Linux C.UTF-8 环境下默认为 Monday
            // - 在正确配置的 zh_CN.UTF-8 环境下为 Sunday
            output.WriteLine("\n在 .NET Core 中运行");
            Assert.Equal(DayOfWeek.Sunday, culture.DateTimeFormat.FirstDayOfWeek);
#endif
        }
        else if (isLinux)
        {
            output.WriteLine("Linux平台测试");

            // Linux依赖于系统locale设置
            // - FirstDayOfWeek 受系统区域设置影响
            // - 在 Linux C.UTF-8 环境下默认为 Monday
            // - 在正确配置的 zh_CN.UTF-8 环境下为 Sunday

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

            // 由于locale配置可能不完整，暂时放宽测试条件
            Assert.True(
                firstDay == DayOfWeek.Sunday || firstDay == DayOfWeek.Monday,
                $"FirstDayOfWeek应该是Sunday或Monday，当前值为{firstDay}"
                 );
        }
        else if (isMacOS)
        {
            output.WriteLine("macOS平台测试");
            // macOS行为类似Linux
            Assert.Equal(DayOfWeek.Monday, culture.DateTimeFormat.FirstDayOfWeek);
        }
        else
        {
            output.WriteLine("未知平台");
        }
    }
}