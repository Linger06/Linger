using System.Runtime.InteropServices;

namespace Linger.Helper;
public static class OSPlatformHelper
{
    public static bool IsLinux { get; }

    public static bool IsWindows { get; }

    public static bool IsMacOSX { get; }

    public static bool IsNetCore { get; }

    public static bool IsNetFramework { get; }

    /// <summary>
    /// >=.Net 5
    /// </summary>
    public static bool IsNet { get; }

    static OSPlatformHelper()
    {
        IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        IsMacOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        // 先检测特定框架，避免顺序依赖问题
        IsNetCore = RuntimeInformation.FrameworkDescription.StartsWith(".NET Core", StringComparison.OrdinalIgnoreCase);
        IsNetFramework = RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

        // 然后再判断是否为.NET 5+（新版.NET）
        // .NET 5+的框架描述通常以".NET"开头，但不是".NET Core"或".NET Framework"
        IsNet = RuntimeInformation.FrameworkDescription.StartsWith(".NET", StringComparison.OrdinalIgnoreCase);
    }
}
