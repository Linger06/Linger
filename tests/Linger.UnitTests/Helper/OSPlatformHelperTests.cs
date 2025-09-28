using System;
using System.Runtime.InteropServices;
using Linger.Helper;
using Xunit.v3;

namespace Linger.UnitTests.Helper;

public class OSPlatformHelperTests
{    
    [Fact]
    public void IsWindows_ShouldReturnExpectedValue()
    {
        // Act
        var result = OSPlatformHelper.IsWindows;
        
        // Assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Assert.True(result);
        }
        else
        {
            Assert.False(result);
        }
    }
    
    [Fact]
    public void IsLinux_ShouldReturnExpectedValue()
    {
        // Act
        var result = OSPlatformHelper.IsLinux;
        
        // Assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.True(result);
        }
        else
        {
            Assert.False(result);
        }
    }
    
    [Fact]
    public void IsOSX_ShouldReturnExpectedValue()
    {
        // Act
        var result = OSPlatformHelper.IsMacOSX;
        
        // Assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Assert.True(result);
        }
        else
        {
            Assert.False(result);
        }
    }

    [Fact]
    public void RuntimeFlags_ShouldBeMutuallyExclusive_AndMatchFrameworkDescription()
    {
        // Arrange
        var desc = RuntimeInformation.FrameworkDescription;

        // Act
        int trueFlags = (OSPlatformHelper.IsNet ? 1 : 0)
                    + (OSPlatformHelper.IsNetCore ? 1 : 0)
                    + (OSPlatformHelper.IsNetFramework ? 1 : 0);

        // Assert: only one runtime family flag can be true
        Assert.Equal(1, trueFlags);

        if (desc.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase))
        {
            Assert.True(OSPlatformHelper.IsNetFramework);
            Assert.False(OSPlatformHelper.IsNetCore);
            Assert.False(OSPlatformHelper.IsNet);
        }
        else if (desc.StartsWith(".NET Core", StringComparison.OrdinalIgnoreCase))
        {
            Assert.True(OSPlatformHelper.IsNetCore);
            Assert.False(OSPlatformHelper.IsNetFramework);
            // Some older runtimes may not start with plain ".NET"; IsNet should be false for .NET Core
            Assert.False(OSPlatformHelper.IsNet);
        }
        else if (desc.StartsWith(".NET", StringComparison.OrdinalIgnoreCase))
        {
            // Modern .NET (5+)
            Assert.True(OSPlatformHelper.IsNet);
            Assert.False(OSPlatformHelper.IsNetCore);
            Assert.False(OSPlatformHelper.IsNetFramework);
        }
        else
        {
            // Fallback: ensure at least one flag is true
            Assert.True(OSPlatformHelper.IsNet || OSPlatformHelper.IsNetCore || OSPlatformHelper.IsNetFramework);
        }
    }

    [Fact]
    public void IsFreeBSD_ShouldHaveValidValue()
    {
        // Act
        var result = OSPlatformHelper.IsFreeBSD;
        
        // Assert - should be bool value (not throw exception)
        Assert.IsType<bool>(result);
    }

    [Fact]
    public void IsAndroid_ShouldHaveValidValue()
    {
        // Act
        var result = OSPlatformHelper.IsAndroid;
        
        // Assert - should be bool value (not throw exception)
        Assert.IsType<bool>(result);
    }

    [Fact]
    public void IsIOS_ShouldHaveValidValue()
    {
        // Act
        var result = OSPlatformHelper.IsIOS;
        
        // Assert - should be bool value (not throw exception)
        Assert.IsType<bool>(result);
    }

    [Fact]
    public void IsWasm_ShouldHaveValidValue()
    {
        // Act
        var result = OSPlatformHelper.IsWasm;
        
        // Assert - should be bool value (not throw exception)
        Assert.IsType<bool>(result);
    }

    [Fact]
    public void ArchitectureProperties_ShouldReturnValidValues()
    {
        // Act
        var osArch = OSPlatformHelper.OSArchitecture;
        var processArch = OSPlatformHelper.ProcessArchitecture;
        var is64Bit = OSPlatformHelper.Is64BitProcess;

        // Assert
        Assert.True(Enum.IsDefined(typeof(Architecture), osArch));
        Assert.True(Enum.IsDefined(typeof(Architecture), processArch));
        Assert.IsType<bool>(is64Bit);
        
        // 64-bit process should match expected architectures
        if (is64Bit)
        {
#if NET5_0_OR_GREATER
            Assert.True(processArch == Architecture.X64 || 
                       processArch == Architecture.Arm64 || 
                       processArch == Architecture.S390x ||
                       processArch == Architecture.LoongArch64 ||
                       processArch == Architecture.Ppc64le);
#else
            Assert.True(processArch == Architecture.X64 || 
                       processArch == Architecture.Arm64);
#endif
        }
        else
        {
#if NET6_0_OR_GREATER
            Assert.True(processArch == Architecture.X86 || 
                       processArch == Architecture.Arm ||
                       processArch == Architecture.Armv6 ||
                       processArch == Architecture.Wasm);
#else
            Assert.True(processArch == Architecture.X86 || 
                       processArch == Architecture.Arm);
#endif
        }
    }

    [Fact]
    public void IsUnix_ShouldBeConsistentWithUnixPlatforms()
    {
        // Act
        var isUnix = OSPlatformHelper.IsUnix;
        var expectedUnix = OSPlatformHelper.IsLinux || OSPlatformHelper.IsMacOSX || OSPlatformHelper.IsFreeBSD;
        
        // Assert
        Assert.Equal(expectedUnix, isUnix);
    }

    [Fact]
    public void OSPlatforms_ShouldBeMutuallyExclusiveForDesktop()
    {
        // Desktop platforms should be mutually exclusive
        var desktopPlatforms = new[] { OSPlatformHelper.IsWindows, OSPlatformHelper.IsLinux, OSPlatformHelper.IsMacOSX, OSPlatformHelper.IsFreeBSD };
        var truePlatforms = desktopPlatforms.Count(x => x);
        
        // At most one desktop platform should be true
        Assert.True(truePlatforms <= 1, "Multiple desktop platforms reported as true");
        
        // If no desktop platform is true, we might be on mobile or WASM
        if (truePlatforms == 0)
        {
            Assert.True(OSPlatformHelper.IsAndroid || OSPlatformHelper.IsIOS || OSPlatformHelper.IsWasm, 
                       "No platform detected as true");
        }
    }

    [Fact]
    public void GetCurrentPlatformName_ShouldReturnValidString()
    {
        // Act
        var platformName = OSPlatformHelper.GetCurrentPlatformName();
        
        // Assert
        Assert.NotNull(platformName);
        Assert.NotEmpty(platformName);
        
        var validPlatforms = new[] { "Windows", "Linux", "macOS", "FreeBSD", "Android", "iOS", "WebAssembly", "Unknown" };
        Assert.Contains(platformName, validPlatforms);
    }

    [Fact]
    public void GetPlatformDescription_ShouldReturnValidString()
    {
        // Act
        var description = OSPlatformHelper.GetPlatformDescription();
        
        // Assert
        Assert.NotNull(description);
        Assert.NotEmpty(description);
        
        // Should contain framework description and platform information
        Assert.Contains(RuntimeInformation.FrameworkDescription, description);
        
        // Should contain platform name and architecture information
        var platformName = OSPlatformHelper.GetCurrentPlatformName();
        var architecture = OSPlatformHelper.ProcessArchitecture.ToString();
        
        Assert.Contains(platformName, description);
        Assert.Contains(architecture, description);
    }

    [Fact]
    public void FrameworkDescription_ShouldMatchRuntimeInformation()
    {
        // Act
        var frameworkDesc = OSPlatformHelper.FrameworkDescription;
        var expected = RuntimeInformation.FrameworkDescription;
        
        // Assert
        Assert.Equal(expected, frameworkDesc);
    }

    [Fact]
    public void GetOSVersion_ShouldReturnValidVersion()
    {
        // Act
        var version = OSPlatformHelper.GetOSVersion();
        
        // Assert
        Assert.NotNull(version);
        Assert.True(version.Major >= 0);
        Assert.Equal(Environment.OSVersion.Version, version);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(5, 0)]
    [InlineData(6, 0)]
    [InlineData(8, 0)]
    [InlineData(9, 0)]
    [InlineData(10, 0)]
    public void IsRuntimeVersionAtLeast_ShouldWorkCorrectly(int major, int minor)
    {
        // Arrange
        var testVersion = new Version(major, minor);
        var currentVersion = Environment.Version;
        
        // Act
        var result = OSPlatformHelper.IsRuntimeVersionAtLeast(testVersion);
        var expected = currentVersion >= testVersion;
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsRuntimeVersionAtLeast_WithNullVersion_ShouldThrow()
    {
        // Act & Assert
        try
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            OSPlatformHelper.IsRuntimeVersionAtLeast(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.True(false, "Expected ArgumentNullException was not thrown");
        }
        catch (System.ArgumentNullException)
        {
            // Expected exception
        }
    }

#if NET6_0_OR_GREATER
    [Fact]
    public void Net6PlusVersionChecks_ShouldHaveValidValues()
    {
        // Act & Assert - These should not throw exceptions
        var isWindows11 = OSPlatformHelper.IsWindows11OrGreater;
        var isMacOSMonterey = OSPlatformHelper.IsMacOSMontereyOrGreater;
        var isAndroidApi31 = OSPlatformHelper.IsAndroidApi31OrGreater;
        
        Assert.IsType<bool>(isWindows11);
        Assert.IsType<bool>(isMacOSMonterey);
        Assert.IsType<bool>(isAndroidApi31);
        
        // If we're on Windows, the Windows 11 check should be meaningful
        if (OSPlatformHelper.IsWindows)
        {
            // This is just to ensure the property can be accessed without error
            _ = isWindows11;
        }
        
        // If we're on macOS, the macOS version check should be meaningful
        if (OSPlatformHelper.IsMacOSX)
        {
            _ = isMacOSMonterey;
        }
        
        // If we're on Android, the API level check should be meaningful
        if (OSPlatformHelper.IsAndroid)
        {
            _ = isAndroidApi31;
        }
    }
#endif

    [Fact]
    public void StaticProperties_ShouldBeInitializedConsistently()
    {
        // This test ensures all static properties are properly initialized
        // and don't throw exceptions when accessed multiple times
        
        // Access all properties twice to ensure consistency
        for (int i = 0; i < 2; i++)
        {
            _ = OSPlatformHelper.IsWindows;
            _ = OSPlatformHelper.IsLinux;
            _ = OSPlatformHelper.IsMacOSX;
            _ = OSPlatformHelper.IsFreeBSD;
            _ = OSPlatformHelper.IsAndroid;
            _ = OSPlatformHelper.IsIOS;
            _ = OSPlatformHelper.IsWasm;
            _ = OSPlatformHelper.IsUnix;
            _ = OSPlatformHelper.IsNet;
            _ = OSPlatformHelper.IsNetCore;
            _ = OSPlatformHelper.IsNetFramework;
            _ = OSPlatformHelper.OSArchitecture;
            _ = OSPlatformHelper.ProcessArchitecture;
            _ = OSPlatformHelper.Is64BitProcess;
            _ = OSPlatformHelper.FrameworkDescription;
        }
        
        // If we get here without exceptions, the test passes
        Assert.True(true);
    }

    [Fact]
    public void PlatformDetection_ShouldBeLogicallyConsistent()
    {
        // Verify logical consistency of platform detection
        
        // Unix platforms should contribute to IsUnix
        if (OSPlatformHelper.IsLinux || OSPlatformHelper.IsMacOSX || OSPlatformHelper.IsFreeBSD)
        {
            Assert.True(OSPlatformHelper.IsUnix);
        }
        
        // Windows should not be Unix
        if (OSPlatformHelper.IsWindows)
        {
            Assert.False(OSPlatformHelper.IsUnix);
            Assert.False(OSPlatformHelper.IsLinux);
            Assert.False(OSPlatformHelper.IsMacOSX);
            Assert.False(OSPlatformHelper.IsFreeBSD);
        }
        
        // Mobile platforms and desktop platforms logic
        var isMobile = OSPlatformHelper.IsAndroid || OSPlatformHelper.IsIOS;
        var isDesktop = OSPlatformHelper.IsWindows || OSPlatformHelper.IsLinux || OSPlatformHelper.IsMacOSX || OSPlatformHelper.IsFreeBSD;
        
        // We should detect at least one platform (or WASM)
        Assert.True(isMobile || isDesktop || OSPlatformHelper.IsWasm);
    }
}