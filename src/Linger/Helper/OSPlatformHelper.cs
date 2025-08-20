using System.Runtime.InteropServices;

namespace Linger.Helper;
public static class OSPlatformHelper
{
    /// <summary>
    /// Indicates whether the current OS platform is Linux.
    /// </summary>
    public static bool IsLinux { get; }

    /// <summary>
    /// Indicates whether the current OS platform is Windows.
    /// </summary>
    public static bool IsWindows { get; }

    /// <summary>
    /// Indicates whether the current OS platform is macOS (OSX).
    /// </summary>
    public static bool IsMacOSX { get; }

    /// <summary>
    /// Indicates whether the current runtime is .NET Core (3.x and below).
    /// </summary>
    public static bool IsNetCore { get; }

    /// <summary>
    /// Indicates whether the current runtime is .NET Framework (e.g., 4.x).
    /// </summary>
    public static bool IsNetFramework { get; }

    /// <summary>
    /// Indicates whether the current runtime is .NET 5 or greater (a.k.a. modern .NET).
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

        // 然后再判断是否为 .NET 5+（新版 .NET）
        // .NET 5+ 的框架描述通常以 ".NET" 开头，但不包含 ".NET Core" 或 ".NET Framework" 前缀。
        // 例如：".NET 8.0.7"、".NET 6.0" 等。
        var desc = RuntimeInformation.FrameworkDescription;
        IsNet = desc.StartsWith(".NET", StringComparison.OrdinalIgnoreCase)
            && !IsNetCore
            && !IsNetFramework;
    }
}
