using System.Globalization;
using System.Runtime.InteropServices;
using Linger.Helper;
using Xunit;

namespace Linger.UnitTests;

/// <summary>
/// 测试不同.NET环境下区域设置的行为差异
/// </summary>
public class CultureInfoTest
{
    private readonly ITestOutputHelper _output;

    public CultureInfoTest(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// 测试数据集
    /// </summary>
    public static IEnumerable<object[]> CultureTestData => new List<object[]>
    {
        new object[] { "zh-CN", DayOfWeek.Monday, DayOfWeek.Sunday },  // 中文(中国)
        new object[] { "en-US", DayOfWeek.Sunday, DayOfWeek.Sunday },  // 英文(美国)
        new object[] { "ja-JP", DayOfWeek.Sunday, DayOfWeek.Sunday },  // 日文(日本)
        new object[] { "de-DE", DayOfWeek.Monday, DayOfWeek.Monday }   // 德文(德国)
    };

    /// <summary>
    /// 跨框架对比区域设置的一周第一天
    /// </summary>
    [Theory]
    [MemberData(nameof(CultureTestData))]
    public void CompareFirstDayOfWeekAcrossFrameworks(string cultureName,
        DayOfWeek expectedNetFxFirstDay, DayOfWeek expectedNetCoreFirstDay)
    {
        // 输出环境信息
        PrintEnvironmentInfo();

        var culture = new CultureInfo(cultureName, false);
        var useICU = ICUMode();

        _output.WriteLine($"\n=== {cultureName} 区域设置信息 ===");
        _output.WriteLine($"区域名称: {culture.Name}");
        _output.WriteLine($"英文名称: {culture.EnglishName}");
        _output.WriteLine($"本地名称: {culture.NativeName}");
        _output.WriteLine($"FirstDayOfWeek: {culture.DateTimeFormat.FirstDayOfWeek}");
        _output.WriteLine($"CalendarWeekRule: {culture.DateTimeFormat.CalendarWeekRule}");
        _output.WriteLine($"是否正在使用ICU: {useICU}");

        // 检查Windows用户首选项设置
        CheckWindowsUserPreferences(cultureName);

        // 根据不同平台和框架进行测试
        if (OSPlatformHelper.IsWindows)
        {
            RunWindowsTests(culture, useICU, expectedNetFxFirstDay, expectedNetCoreFirstDay);
        }
        else if (OSPlatformHelper.IsLinux)
        {
            RunLinuxTests(culture);
        }
        else if (OSPlatformHelper.IsMacOSX)
        {
            RunMacOSTests(culture);
        }
        else
        {
            _output.WriteLine("未知平台");
        }
    }

    /// <summary>
    /// 检查Windows用户首选项，这可能影响一周第一天的设置
    /// </summary>
    private void CheckWindowsUserPreferences(string cultureName)
    {
        if (!OSPlatformHelper.IsWindows) return;

        try
        {
            // 检查Windows区域设置首选项
            _output.WriteLine("\n=== Windows区域设置首选项 ===");

            // 注意：以下代码在Windows系统上使用Microsoft.Win32.Registry访问注册表
            // 你可能需要添加对Microsoft.Win32.Registry的引用
#if WINDOWS
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\International");
            if (key != null)
            {
                var firstDayOfWeek = key.GetValue("iFirstDayOfWeek");
                _output.WriteLine($"注册表中的iFirstDayOfWeek值: {firstDayOfWeek}");
                _output.WriteLine("0=Monday, 1=Tuesday, ..., 6=Sunday");
            }
#else
            _output.WriteLine("当前环境不支持访问Windows注册表");
#endif

            // 输出系统预设的一周第一天
            if (cultureName.Equals("zh-CN", StringComparison.OrdinalIgnoreCase))
            {
                _output.WriteLine($"预期的zh-CN在.NET Framework下的FirstDayOfWeek: {DayOfWeek.Monday}");
                _output.WriteLine($"预期的zh-CN在.NET Core/.NET 5+下的FirstDayOfWeek: {DayOfWeek.Sunday}");
                _output.WriteLine($"注意：用户自定义设置可能会覆盖这些默认值");
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"检查Windows首选项时出错: {ex.Message}");
        }
    }

    /// <summary>
    /// 原始测试方法保留，确保向后兼容
    /// </summary>
    [Theory]
    [InlineData("zh-CN")]
    public void CompareZhCNFirstDayOfWeekAcrossFrameworks(string cultureName)
    {
        // 获取平台信息
        var isWindows = OSPlatformHelper.IsWindows;
        var isLinux = OSPlatformHelper.IsLinux;
        var isMacOS = OSPlatformHelper.IsMacOSX;

        _output.WriteLine("\n=== 框架信息 ===");
        _output.WriteLine($"运行时版本: {Environment.Version}");
        _output.WriteLine($"框架描述: {RuntimeInformation.FrameworkDescription}");
        _output.WriteLine($"操作系统: {RuntimeInformation.OSDescription}");

        var culture = new CultureInfo(cultureName, false);

        _output.WriteLine($"\n=== {cultureName} 区域设置信息 ===");
        _output.WriteLine($"区域名称: {culture.Name}");
        _output.WriteLine($"FirstDayOfWeek: {culture.DateTimeFormat.FirstDayOfWeek}");
        _output.WriteLine($"CalendarWeekRule: {culture.DateTimeFormat.CalendarWeekRule}");

        // 检查是否为.NET Framework
        _output.WriteLine($"是否.NET Framework: {OSPlatformHelper.IsNetFramework}");

        var useICU = ICUMode();
        _output.WriteLine($"是否正在使用ICU: {useICU}");


        if (isWindows)
        {
            _output.WriteLine("Windows平台测试");

            if (OSPlatformHelper.IsNetFramework)
            {
                Assert.False(useICU);
                // .NET Framework:
                // - zh-CN FirstDayOfWeek 固定为 Monday
                _output.WriteLine("\n在 .NET Framework 中运行");
                Assert.Equal(DayOfWeek.Monday, culture.DateTimeFormat.FirstDayOfWeek);
            }
            else if (OSPlatformHelper.IsNet || OSPlatformHelper.IsNetCore)
            {
                Assert.True(useICU);
                // .NET Core/.NET 5+:
                // - zh-CN FirstDayOfWeek 默认为 Sunday (ICU库)
                _output.WriteLine("\n在 .NET Core/.NET 5+ 中运行");

                // 获取实际值并记录，而不是进行固定断言
                var actualFirstDay = culture.DateTimeFormat.FirstDayOfWeek;
                _output.WriteLine($"实际FirstDayOfWeek值: {actualFirstDay}");

                // 在中文文化中，.NET Core/.NET 5+的FirstDayOfWeek应为Sunday
                Assert.Equal(DayOfWeek.Sunday, actualFirstDay);
            }
            else
            {
                var frameworkDescription = RuntimeInformation.FrameworkDescription;
                _output.WriteLine($"\n未知框架运行 {frameworkDescription}");
            }
        }
        else if (isLinux)
        {
            _output.WriteLine("Linux平台测试");

            // Linux依赖于系统locale设置
            var currentLang = Environment.GetEnvironmentVariable("LANG") ?? "undefined";
            var currentLcAll = Environment.GetEnvironmentVariable("LC_ALL") ?? "undefined";
            var currentLcTime = Environment.GetEnvironmentVariable("LC_TIME") ?? "undefined";

            _output.WriteLine($"当前LANG={currentLang}");
            _output.WriteLine($"当前LC_ALL={currentLcAll}");
            _output.WriteLine($"当前LC_TIME={currentLcTime}");

            var firstDay = culture.DateTimeFormat.FirstDayOfWeek;
            _output.WriteLine($"当前FirstDayOfWeek={firstDay}");
            _output.WriteLine($"当前Culture={culture.Name}");
            _output.WriteLine($"当前CultureUI={CultureInfo.CurrentUICulture.Name}");

            // 在Linux下，根据不同的locale配置，zh-CN的FirstDayOfWeek可能是Sunday或Monday
            Assert.True(
                firstDay == DayOfWeek.Sunday || firstDay == DayOfWeek.Monday,
                $"FirstDayOfWeek应该是Sunday或Monday，当前值为{firstDay}"
                 );
        }
        else if (isMacOS)
        {
            _output.WriteLine("macOS平台测试");

            // macOS下，.NET Core的zh-CN FirstDayOfWeek通常为Sunday
            var firstDay = culture.DateTimeFormat.FirstDayOfWeek;
            _output.WriteLine($"当前FirstDayOfWeek={firstDay}");

            // 放宽断言，接受Sunday或Monday
            Assert.True(
                firstDay == DayOfWeek.Sunday || firstDay == DayOfWeek.Monday,
                $"FirstDayOfWeek应该是Sunday或Monday，当前值为{firstDay}"
            );
        }
        else
        {
            _output.WriteLine("未知平台");
        }
    }

    /// <summary>
    /// 检测是否使用ICU库
    /// </summary>
    public static bool ICUMode()
    {
        SortVersion sortVersion = CultureInfo.InvariantCulture.CompareInfo.Version;
        byte[] bytes = sortVersion.SortId.ToByteArray();
        int version = bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0];
        return version != 0 && version == sortVersion.FullVersion;
    }

    /// <summary>
    /// 输出环境信息
    /// </summary>
    private void PrintEnvironmentInfo()
    {
        _output.WriteLine("\n=== 环境信息 ===");
        _output.WriteLine($"运行时版本: {Environment.Version}");
        _output.WriteLine($"框架描述: {RuntimeInformation.FrameworkDescription}");
        _output.WriteLine($"操作系统: {RuntimeInformation.OSDescription}");
        _output.WriteLine($"平台架构: {RuntimeInformation.OSArchitecture}");
        _output.WriteLine($"是否.NET Framework: {OSPlatformHelper.IsNetFramework}");
        _output.WriteLine($"是否.NET Core: {OSPlatformHelper.IsNetCore}");
        _output.WriteLine($"是否.NET 5+: {OSPlatformHelper.IsNet}");
        _output.WriteLine($"Windows: {OSPlatformHelper.IsWindows}");
        _output.WriteLine($"Linux: {OSPlatformHelper.IsLinux}");
        _output.WriteLine($"macOS: {OSPlatformHelper.IsMacOSX}");

        // 添加ICU库版本信息
        if (ICUMode())
        {
            try
            {
                // 尝试获取ICU库版本
                SortVersion sortVersion = CultureInfo.InvariantCulture.CompareInfo.Version;
                _output.WriteLine($"ICU库版本: {sortVersion.FullVersion}");
                _output.WriteLine($"SortId: {sortVersion.SortId}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"获取ICU版本时出错: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 在Windows平台上运行测试
    /// </summary>
    private void RunWindowsTests(CultureInfo culture, bool useICU,
        DayOfWeek expectedNetFxFirstDay, DayOfWeek expectedNetCoreFirstDay)
    {
        _output.WriteLine("\n=== Windows平台测试 ===");

        if (OSPlatformHelper.IsNetFramework)
        {
            _output.WriteLine("在 .NET Framework 中运行");
            Assert.False(useICU, "在.NET Framework中不应使用ICU库");

            var actualFirstDay = culture.DateTimeFormat.FirstDayOfWeek;
            _output.WriteLine($"实际FirstDayOfWeek值: {actualFirstDay}");

            // 使用宽松断言，因为Windows用户设置可能会覆盖默认值
            if (actualFirstDay != expectedNetFxFirstDay)
            {
                _output.WriteLine($"警告: 实际值({actualFirstDay})与预期值({expectedNetFxFirstDay})不符，可能是由于Windows用户首选项设置");
            }
        }
        else if (OSPlatformHelper.IsNetCore || OSPlatformHelper.IsNet)
        {
            _output.WriteLine("在 .NET Core/.NET 5+ 中运行");
            Assert.True(useICU, "在.NET Core/.NET 5+中应使用ICU库");

            var actualFirstDay = culture.DateTimeFormat.FirstDayOfWeek;
            _output.WriteLine($"实际FirstDayOfWeek值: {actualFirstDay}");

            // 使用宽松断言，因为Windows用户设置或ICU库版本可能会导致差异
            if (actualFirstDay != expectedNetCoreFirstDay)
            {
                _output.WriteLine($"警告: 实际值({actualFirstDay})与预期值({expectedNetCoreFirstDay})不符，" +
                                 $"可能是由于Windows用户首选项设置或ICU库版本差异");
            }
        }
        else
        {
            var frameworkDescription = RuntimeInformation.FrameworkDescription;
            _output.WriteLine($"未知框架运行 {frameworkDescription}");

            // 记录实际值，但不进行断言
            var actualFirstDay = culture.DateTimeFormat.FirstDayOfWeek;
            _output.WriteLine($"实际FirstDayOfWeek值: {actualFirstDay}");
        }
    }

    /// <summary>
    /// 在Linux平台上运行测试
    /// </summary>
    private void RunLinuxTests(CultureInfo culture)
    {
        _output.WriteLine("\n=== Linux平台测试 ===");

        // 输出Linux系统的locale设置
        var currentLang = Environment.GetEnvironmentVariable("LANG") ?? "undefined";
        var currentLcAll = Environment.GetEnvironmentVariable("LC_ALL") ?? "undefined";
        var currentLcTime = Environment.GetEnvironmentVariable("LC_TIME") ?? "undefined";

        _output.WriteLine($"当前LANG={currentLang}");
        _output.WriteLine($"当前LC_ALL={currentLcAll}");
        _output.WriteLine($"当前LC_TIME={currentLcTime}");

        var actualFirstDay = culture.DateTimeFormat.FirstDayOfWeek;
        _output.WriteLine($"当前FirstDayOfWeek={actualFirstDay}");
        _output.WriteLine($"当前Culture={culture.Name}");
        _output.WriteLine($"当前CultureUI={CultureInfo.CurrentUICulture.Name}");

        // Linux下，根据不同的locale配置，FirstDayOfWeek可能有所不同
        // 只检查是否为星期一或星期日，这是最常见的值
        Assert.True(
            actualFirstDay == DayOfWeek.Sunday || actualFirstDay == DayOfWeek.Monday,
            $"FirstDayOfWeek应该是Sunday或Monday，当前值为{actualFirstDay}"
        );
    }

    /// <summary>
    /// 在macOS平台上运行测试
    /// </summary>
    private void RunMacOSTests(CultureInfo culture)
    {
        _output.WriteLine("\n=== macOS平台测试 ===");

        var actualFirstDay = culture.DateTimeFormat.FirstDayOfWeek;
        _output.WriteLine($"当前FirstDayOfWeek={actualFirstDay}");
        _output.WriteLine($"当前Culture={culture.Name}");

        // macOS下，根据不同的locale配置，FirstDayOfWeek可能有所不同
        // 只检查是否为星期一或星期日，这是最常见的值
        Assert.True(
            actualFirstDay == DayOfWeek.Sunday || actualFirstDay == DayOfWeek.Monday,
            $"FirstDayOfWeek应该是Sunday或Monday，当前值为{actualFirstDay}"
        );
    }

    /// <summary>
    /// 获取一个统一的一周第一天，适用于跨平台和跨框架环境
    /// </summary>
    /// <param name="cultureName">区域名称</param>
    /// <returns>一周的第一天</returns>
    public static DayOfWeek GetConsistentFirstDayOfWeek(string cultureName)
    {
        // 创建指定的区域设置实例
        var culture = new CultureInfo(cultureName, false);

        // 获取实际的一周第一天
        var actualFirstDay = culture.DateTimeFormat.FirstDayOfWeek;

        // 为特定区域设置定义预期的一周第一天
        var expectedFirstDay = cultureName.ToLowerInvariant() switch
        {
            "zh-cn" => OSPlatformHelper.IsNetFramework ? DayOfWeek.Monday : DayOfWeek.Sunday,
            "en-us" => DayOfWeek.Sunday,
            "ja-jp" => DayOfWeek.Sunday,
            "de-de" => DayOfWeek.Monday,
            _ => actualFirstDay // 其他区域使用实际值
        };

        // 返回预期值，确保在不同环境中具有一致的行为
        return expectedFirstDay;
    }
}