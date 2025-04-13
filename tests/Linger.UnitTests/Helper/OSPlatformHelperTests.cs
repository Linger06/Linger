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
}