using Linger.Configuration;

namespace Linger.Configuration.UnitTests;

public class AppSettingsHelperTests
{
    [Fact]
    public void GetSection_WithoutConfigFile_ReturnsEmptySection()
    {
        var section = AppSettingsHelper.GetSection("MissingSection");

        Assert.Null(section.Value);
    }

    [Fact]
    public void GetSectionValue_WithoutConfigFile_ReturnsNull()
    {
        Assert.Null(AppSettingsHelper.GetSectionValue("MissingKey"));
    }

    [Fact]
    public void GetConnectionString_WithoutConfigFile_ReturnsNull()
    {
        Assert.Null(AppSettingsHelper.GetConnectionString("Missing"));
    }

    [Fact]
    public void ConvertToObject_WithoutConfigFile_ReturnsNull()
    {
        Assert.Null(AppSettingsHelper.ConvertToObject<TestSettings>());
    }

    private sealed class TestSettings
    {
        public string? Name { get; set; }

        public int Count { get; set; }
    }
}