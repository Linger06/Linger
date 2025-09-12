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
    /// Indicates whether the current OS platform is FreeBSD.
    /// </summary>
    public static bool IsFreeBSD { get; }

    /// <summary>
    /// Indicates whether the current OS platform is Android.
    /// </summary>
    public static bool IsAndroid { get; }

    /// <summary>
    /// Indicates whether the current OS platform is iOS.
    /// </summary>
    public static bool IsIOS { get; }

    /// <summary>
    /// Indicates whether the current OS platform is WebAssembly (WASM).
    /// Available in .NET 10+
    /// </summary>
    public static bool IsWasm { get; }

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

    /// <summary>
    /// Gets the current OS architecture (e.g., X64, X86, Arm64).
    /// </summary>
    public static Architecture OSArchitecture { get; }

    /// <summary>
    /// Gets the current process architecture.
    /// </summary>
    public static Architecture ProcessArchitecture { get; }

    /// <summary>
    /// Indicates whether the current process is running on a 64-bit architecture.
    /// </summary>
    public static bool Is64BitProcess { get; }

    /// <summary>
    /// Indicates whether the current OS is a Unix-like system (Linux, macOS, FreeBSD).
    /// </summary>
    public static bool IsUnix { get; }

    /// <summary>
    /// Gets the framework description string.
    /// </summary>
    public static string FrameworkDescription { get; }

    static OSPlatformHelper()
    {
        // Basic OS Platform detection
        IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        IsMacOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        // FreeBSD support (available in .NET Core 2.1+)
#if NET5_0_OR_GREATER
        IsFreeBSD = RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
#else
        IsFreeBSD = RuntimeInformation.IsOSPlatform(OSPlatform.Create("FREEBSD"));
#endif

        // Mobile platforms (.NET 6+)
#if NET6_0_OR_GREATER
        IsAndroid = OperatingSystem.IsAndroid();
        IsIOS = OperatingSystem.IsIOS();
#else
        IsAndroid = RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"));
        IsIOS = RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS"));
#endif

        // WASM support
#if NET10_0_OR_GREATER
        IsWasm = OperatingSystem.IsBrowser();
#elif NET5_0_OR_GREATER
        IsWasm = RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));
#else
        IsWasm = RuntimeInformation.IsOSPlatform(OSPlatform.Create("WEBASSEMBLY"));
#endif

        // Architecture information
        OSArchitecture = RuntimeInformation.OSArchitecture;
        ProcessArchitecture = RuntimeInformation.ProcessArchitecture;
        Is64BitProcess = Environment.Is64BitProcess;

        // Unix-like systems
        IsUnix = IsLinux || IsMacOSX || IsFreeBSD;

        // Framework description
        FrameworkDescription = RuntimeInformation.FrameworkDescription;

        // 先检测特定框架，避免顺序依赖问题
        IsNetCore = RuntimeInformation.FrameworkDescription.StartsWith(".NET Core", StringComparison.OrdinalIgnoreCase);
        IsNetFramework = RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework");

        // 然后再判断是否为 .NET 5+（新版 .NET）
        // .NET 5+ 的框架描述通常以 ".NET" 开头，但不包含 ".NET Core" 或 ".NET Framework" 前缀。
        // 例如：".NET 8.0.7"、".NET 6.0"、".NET 10.0" 等。
        var desc = RuntimeInformation.FrameworkDescription;
        IsNet = desc.StartsWith(".NET", StringComparison.OrdinalIgnoreCase)
            && !IsNetCore
            && !IsNetFramework;
    }

    /// <summary>
    /// Gets a human-readable description of the current platform and runtime.
    /// </summary>
    /// <returns>A string describing the current platform and runtime environment.</returns>
    public static string GetPlatformDescription()
    {
        var platform = GetCurrentPlatformName();
        var arch = ProcessArchitecture.ToString();
        var framework = RuntimeInformation.FrameworkDescription;
        
        return $"{platform} ({arch}) - {framework}";
    }

    /// <summary>
    /// Gets the name of the current platform.
    /// </summary>
    /// <returns>A string representing the current platform name.</returns>
    public static string GetCurrentPlatformName()
    {
        if (IsWindows) return "Windows";
        if (IsLinux) return "Linux";
        if (IsMacOSX) return "macOS";
        if (IsFreeBSD) return "FreeBSD";
        if (IsAndroid) return "Android";
        if (IsIOS) return "iOS";
        if (IsWasm) return "WebAssembly";
        
        return "Unknown";
    }

    /// <summary>
    /// 获取操作系统版本信息
    /// </summary>
    /// <returns>操作系统版本</returns>
    public static Version GetOSVersion()
    {
        return Environment.OSVersion.Version;
    }

    /// <summary>
    /// 检查当前运行时版本是否满足最低版本要求
    /// </summary>
    /// <param name="minimumVersion">最低版本要求</param>
    /// <returns>是否满足版本要求</returns>
    public static bool IsRuntimeVersionAtLeast(Version minimumVersion)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(minimumVersion);
#else
        if (minimumVersion is null)
            throw new System.ArgumentNullException(nameof(minimumVersion));
#endif
        
        return Environment.Version >= minimumVersion;
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Indicates whether the current OS is Windows 11 or later.
    /// Available in .NET 6+
    /// </summary>
    public static bool IsWindows11OrGreater => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);

    /// <summary>
    /// Indicates whether the current OS is macOS Monterey (12.0) or later.
    /// Available in .NET 6+
    /// </summary>
    public static bool IsMacOSMontereyOrGreater => OperatingSystem.IsMacOSVersionAtLeast(12, 0);

    /// <summary>
    /// Indicates whether the current OS is Android API level 31 or later.
    /// Available in .NET 6+
    /// </summary>
    public static bool IsAndroidApi31OrGreater => OperatingSystem.IsAndroidVersionAtLeast(31);
#endif
}
