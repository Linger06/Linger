using Microsoft.Extensions.Configuration;

namespace Linger.Configuration;

/// <summary>
/// Provides a singleton instance for accessing application configuration
/// </summary>
public class AppConfig
{
    private static readonly Lazy<AppConfig> s_instance = new(() => new AppConfig());

    private AppConfig()
    {
        Config = Build();
    }

    /// <summary>
    /// Gets the configuration root
    /// </summary>
    public IConfigurationRoot Config { get; }

    /// <summary>
    /// Gets the singleton instance of AppConfig
    /// </summary>
    public static AppConfig Instance => s_instance.Value;

    private static IConfigurationRoot Build()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), true, true);
        IConfigurationRoot configuration = builder.Build();
        return configuration;
    }
}