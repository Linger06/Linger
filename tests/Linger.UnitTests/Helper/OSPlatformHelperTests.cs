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
}