using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using Xunit.v3;

namespace Linger.UnitTests;

public class ExtensionMethodSettingTests
{
    [Fact]
    public void DefaultEncoding_ShouldBeUTF8()
    {
        // Act & Assert
        Assert.Equal(Encoding.UTF8, ExtensionMethodSetting.DefaultEncoding);
    }
    
    [Fact]
    public void DefaultCulture_ShouldBeInvariantCulture()
    {
        // Act & Assert
        Assert.Equal(CultureInfo.InvariantCulture, ExtensionMethodSetting.DefaultCulture);
    }
    
    [Fact]
    public void DefaultBufferSize_ShouldBe4096()
    {
        // Act & Assert
        Assert.Equal(4096, ExtensionMethodSetting.DefaultBufferSize);
    }
    
#if !NETFRAMEWORK || NET462_OR_GREATER
    [Fact]
    public void DefaultJsonSerializerOptions_ShouldHaveExpectedSettings()
    {
        // Act
        var options = ExtensionMethodSetting.DefaultJsonSerializerOptions;
        
        // Assert
        Assert.True(options.WriteIndented);
        Assert.True(options.PropertyNameCaseInsensitive);
        Assert.Equal(JsonIgnoreCondition.WhenWritingNull, options.DefaultIgnoreCondition);
        Assert.True(options.IgnoreReadOnlyProperties);
        Assert.True(options.IgnoreReadOnlyFields);
        Assert.False(options.AllowTrailingCommas);
        Assert.Equal(JsonCommentHandling.Disallow, options.ReadCommentHandling);
    }
    
    [Fact]
    public void DefaultPostJsonOption_ShouldHaveExpectedSettings()
    {
        // Act
        var options = ExtensionMethodSetting.DefaultPostJsonOption;
        
        // Assert
        Assert.Null(options.PropertyNamingPolicy);
        Assert.True(options.AllowTrailingCommas);
        Assert.Equal(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString, options.NumberHandling);
    }
#endif
}