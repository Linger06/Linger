using Linger.Configuration;
using Microsoft.Extensions.Configuration;

namespace Linger.Configuration.UnitTests;

public class AppConfigTests
{
    [Fact]
    public void Instance_ReturnsSameSingletonInstance()
    {
        var first = AppConfig.Instance;
        var second = AppConfig.Instance;

        Assert.Same(first, second);
    }

    [Fact]
    public void Config_WithoutConfigFile_ReturnsEmptyConfiguration()
    {
        var config = AppConfig.Instance.Config;

        Assert.NotNull(config);
        Assert.Null(config.GetSection("MissingSection").Value);
    }

    [Fact]
    public void AppSettingsHelper_DelegatesToAppConfigInstance()
    {
        var config = AppConfig.Instance.Config;

        Assert.Equal(config.GetSection("SharedSection").Value, AppSettingsHelper.GetSection("SharedSection").Value);
        Assert.Equal(config.GetConnectionString("Shared"), AppSettingsHelper.GetConnectionString("Shared"));
    }
}
